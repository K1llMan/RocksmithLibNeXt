using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using RocksmithLibNeXt.Common.Archives;
using RocksmithLibNeXt.Common.Encryption;
using RocksmithLibNeXt.Common.Streams;
using RocksmithLibNeXt.Formats.Common;
using RocksmithLibNeXt.Formats.Psarc.Models;

namespace RocksmithLibNeXt.Formats.Psarc
{
    public class Psarc : FileWorker, IDisposable
    {
        #region Fields

        private PsarcHeader header;
        private uint[] zBlocksSizeList;

        private BigEndianBinaryReader reader;

        #endregion Fields

        #region Properties

        private int bNum => (int) Math.Log(header.BlockSizeAlloc, byte.MaxValue + 1);

        public bool UseMemory { get; }

        public List<PsarcEntry> TableOfContent { get; }

        #endregion Properties

        #region Auxiliary functions

        private void ParseTableOfContent(BigEndianBinaryReader tocReader)
        {
            // Parse TOC Entries
            for (int i = 0, tocFiles = (int) header.NumFiles; i < tocFiles; i++)
                TableOfContent.Add(new PsarcEntry {
                    Id = i,
                    MD5 = tocReader.ReadBytes(16),
                    zIndexBegin = tocReader.ReadUInt32(),
                    Length = tocReader.ReadUInt40(),
                    Offset = tocReader.ReadUInt40()
                }); /* FIXME: general idea was to implement parallel inflate route, still need to re-think this.
                if (i == 0) continue;
                if (i == tocFiles - 1)
                    TableOfContent[i].zDatalen = (ulong)reader.BaseStream.Length - TableOfContent[i].Offset; //HACK: fails if psarc is truncated.
                TableOfContent[i-1].zDatalen = TableOfContent[i].Offset - TableOfContent[i-1].Offset; */
        }

        /// <summary>
        /// Checks if psarc is not truncated
        /// </summary>
        /// <returns>The psarc size</returns>
        private long RequiredPsarcSize()
        {
            // get lastEntry.offset and it's size
            if (TableOfContent.Count > 0) {
                PsarcEntry lastEntry = TableOfContent.Last();
                ulong totalLen = lastEntry.Offset;
                long zNum = zBlocksSizeList.Length - lastEntry.zIndexBegin;
                for (int z = 0; z < zNum; z++) {
                    uint num = zBlocksSizeList[lastEntry.zIndexBegin + z];
                    totalLen += num == 0 ? header.BlockSizeAlloc : num;
                }

                return (long) totalLen;
            }

            return header.TotalTOCSize; // already read
        }

        #region Manifest

        /// <summary>
        /// Reads file names from the manifest
        /// </summary>
        private void ReadManifest()
        {
            PsarcEntry toc = TableOfContent.First();

            toc.Name = "NamesBlock.bin";

            InflateEntry(toc);

            using StreamReader bReader = new(toc.Data, true);
            int count = TableOfContent.Count;
            string[] data = bReader.ReadToEnd().Split('\n'); //0x0A

            Parallel.For(0, data.Length, i =>
            {
                if (i + 1 != count)
                    TableOfContent[i + 1].Name = data[i];
            });

            // commented out to leave NamesXblock.bin for debugging
            // TableOfContent.RemoveAt(0);
        }

        private void WriteManifest()
        {
            if (TableOfContent.Count == 0)
                TableOfContent.Add(new PsarcEntry { Name = "NamesBlock.bin" });

            if (string.IsNullOrEmpty(TableOfContent[0].Name))
                TableOfContent[0].Name = "NamesBlock.bin";

            if (TableOfContent[0].Name != "NamesBlock.bin")
                TableOfContent.Insert(0, new PsarcEntry { Name = "NamesBlock.bin" });

            // generate NamesBlock.bin content
            BinaryWriter binaryWriter = new(new MemoryStream());
            for (int i = 1, len = TableOfContent.Count; i < len; i++) {
                // '/' - unix path separator
                byte[] bytes = Encoding.ASCII.GetBytes(TableOfContent[i].Name);

                // don't include toolkit.version in NamesBlock.bin
                //if (Encoding.ASCII.GetString(bytes) == "toolkit.version")
                //    continue;

                binaryWriter.Write(bytes);
                // '\n' - unix line separator
                if (i == len - 1) {
                    binaryWriter.BaseStream.Position = 0;
                    continue;
                }

                binaryWriter.Write('\n'); //data.WriteByte(0x0A);
            }

            TableOfContent[0].Data = binaryWriter.BaseStream;
            TableOfContent[0].Length = (ulong) binaryWriter.BaseStream.Length;
        }

        #endregion Manifest

        #endregion Auxiliary functions

        #region Main functions

        public Psarc()
        {
            header = new PsarcHeader();
            TableOfContent = new List<PsarcEntry> { new() };
        }

        public Psarc(bool useMemory) : this()
        {
            UseMemory = useMemory;
        }

        #region Reading

        /// <summary>
        /// Inflates selected entry
        /// </summary>
        /// <param name="entry">Entry to unpack</param>
        public void InflateEntry(PsarcEntry entry)
        {
            // Skip empty files
            if (entry.Length == 0)
                return;

            // Decompress Entry
            const int zHeader = 0x78DA;
            uint zChunkId = entry.zIndexBegin;
            int blockSize = (int) header.BlockSizeAlloc;
            //bool isZlib = header.CompressionMethod == 2053925218;

            entry.Data = UseMemory
                ? new MemoryStream(new byte[entry.Length]) //new MemoryStreamExtension()
                : new TempFileStream();

            reader.BaseStream.Position = (long) entry.Offset;

            do {
                // check for corrupt CDLC content and catch exceptions
                try {
                    if (zBlocksSizeList[zChunkId] == 0U) // raw. full cluster used.
                    {
                        entry.Data.Write(reader.ReadBytes(blockSize), 0, blockSize);
                    }
                    else {
                        ushort num = reader.ReadUInt16();
                        reader.BaseStream.Position -= 2L;

                        byte[] array = reader.ReadBytes((int) zBlocksSizeList[zChunkId]);

                        if (num == zHeader)
                            // compressed
                            try {
                                Archives.Unzip(array, entry.Data, false);
                            }
                            catch (Exception ex) //IOException
                            {
                                // corrupt CDLC zlib.net exception ... try to unpack
                                Logger.LogError(string.IsNullOrEmpty(entry.Name)
                                    ? @$"CDLC contains a zlib exception.{Environment.NewLine}Warning: {ex}"
                                    : @$"CDLC contains a broken datachunk in file '{entry.Name.Split('/').Last()}'.{Environment.NewLine}Warning Type 1: {ex}");
                            }

                        else // raw. used only for data(chunks) smaller than 64 kb
                            entry.Data.Write(array, 0, array.Length);
                    }

                    zChunkId += 1;
                }
                catch (Exception ex) // index is outside the bounds of the array 
                {
                    // corrupt CDLC data length ... try to unpack
                    Logger.LogError(@$"CDLC contains a broken datachunk in file '{entry.Name.Split('/').Last()}'.{Environment.NewLine}Warning Type 2: {ex}");
                    break;
                }
            } while (entry.Data.Length < (long) entry.Length);

            entry.Data.Seek(0, SeekOrigin.Begin);
            entry.Data.Flush();
        }

        /// <summary>
        /// Inflates the entry.
        /// </summary>
        /// <param name="name">Name with extension.</param>
        public void InflateEntry(string name)
        {
            InflateEntry(TableOfContent.First(t => t.Name.EndsWith(name, StringComparison.Ordinal)));
        }

        /// <summary>
        /// Inflates all entries in current psarc.
        /// </summary>
        public void InflateEntries()
        {
            foreach (PsarcEntry current in TableOfContent)
                // We really can use Parallel here.
                InflateEntry(current);
        }

        public override void Open(Stream fileStream)
        {
            TableOfContent.Clear();
            reader = new BigEndianBinaryReader(fileStream);
            header.MagicNumber = reader.ReadUInt32();

            // PSAR (BE)
            if (header.MagicNumber == 1347633490U) {
                //Parse Header
                header.VersionNumber = reader.ReadUInt32();
                header.CompressionMethod = reader.ReadUInt32();
                header.TotalTOCSize = reader.ReadUInt32();
                header.TOCEntrySize = reader.ReadUInt32();
                header.NumFiles = reader.ReadUInt32();
                header.BlockSizeAlloc = reader.ReadUInt32();
                header.ArchiveFlags = reader.ReadUInt32();

                //Read TOC
                int tocSize = (int) (header.TotalTOCSize - 32U);

                BigEndianBinaryReader tocReader = reader;

                // TOC_ENCRYPTED
                if (header.ArchiveFlags == 4) {
                    using MemoryStream decStream = new();

                    RijndaelEncryptor.DecryptPsarc(fileStream, decStream, header.TotalTOCSize);
                    byte[] buffer = new byte[tocSize];
                    decStream.Read(buffer);

                    // Decrypt TOC
                    MemoryStream tocStream = new();
                    tocStream.Write(buffer);
                    tocStream.Position = 0;

                    tocReader = new BigEndianBinaryReader(tocStream);
                    ParseTableOfContent(tocReader);
                }

                //Parse zBlocksSizeList
                int tocChunkSize = (int) (header.NumFiles * header.TOCEntrySize); //(int)reader.BaseStream.Position //don't alter this with. causes issues
                int zNum = (tocSize - tocChunkSize) / bNum;
                uint[] zLengths = new uint[zNum];

                for (int i = 0; i < zNum; i++)
                    switch (bNum) {
                        case 2: //64KB
                            zLengths[i] = tocReader.ReadUInt16();
                            break;
                        case 3: //16MB
                            zLengths[i] = tocReader.ReadUInt24();
                            break;
                        case 4: //4GB
                            zLengths[i] = tocReader.ReadUInt32();
                            break;
                    }

                zBlocksSizeList = zLengths; //TODO: validate
                tocReader.BaseStream.Flush(); //Free tocStream resources

                reader = new BigEndianBinaryReader(fileStream);

                // Validate psarc size
                // if (psarc.Length < RequiredPsarcSize())
                // throw new InvalidDataException("Truncated psarc.");
                // try to unpack corrupt CDLC for now

                switch (header.CompressionMethod) {
                    case 2053925218: //zlib (BE)
                        ReadManifest();
                        fileStream.Seek(header.TotalTOCSize, SeekOrigin.Begin);
                        // Decompress Data
                        InflateEntries();
                        break;
                    case 1819962721: //lzma (BE)
                        throw new NotImplementedException("LZMA compression not supported.");
                    default:
                        throw new InvalidDataException("Unknown compression.");
                }
            }

            fileStream.Flush();
        }

        #endregion Reading

        #region Writing

        /// <summary>
        /// Packs Entries to zStream
        /// </summary>
        /// <param name="entryDeflatedData">zStreams</param>
        /// <param name="zLengths">zBlocksSizeList</param>
        private void DeflateEntries(out Dictionary<PsarcEntry, byte[]> entryDeflatedData, out List<uint> zLengths)
        {
            // TODO: This produces perfect results for song archives (original vs repacked)
            // there are slight differences in the binary of large archives (original vs repacked).  WHY?
            //
            entryDeflatedData = new Dictionary<PsarcEntry, byte[]>();
            uint blockSize = header.BlockSizeAlloc;
            zLengths = new List<uint>();

            int ndx = 0; // for debugging

            foreach (PsarcEntry entry in TableOfContent) {
                List<Tuple<byte[], long>> zList = new();
                entry.zIndexBegin = (uint) zLengths.Count;
                entry.Data.Seek(0, SeekOrigin.Begin);

                while (entry.Data.Position < entry.Data.Length) {
                    byte[] arrayI = new byte[blockSize];
                    byte[] arrayO = new byte[blockSize * 2];

                    using MemoryStream memoryStream = new(arrayO);
                    int plainLen = entry.Data.Read(arrayI);//, 0/*, arrayI.Length*/);
                    long packedLen = Archives.Zip(arrayI, memoryStream, plainLen, false);

                    // If packed data "worse" than plain (i.e. already packed) z = 0
                    if (packedLen >= plainLen) {
                        zList.Add(new Tuple<byte[], long>(arrayI, plainLen));
                    }
                    else // If packed data is good
                    {
                        zList.Add(packedLen < blockSize - 1 
                            ? new Tuple<byte[], long>(arrayO, packedLen) 
                            : new Tuple<byte[], long>(arrayI, plainLen));
                    }
                }

                long zSisesSum = 0;
                foreach (Tuple<byte[], long> zSize in zList) {
                    zSisesSum += zSize.Item2;
                    zLengths.Add((uint) zSize.Item2);
                }

                byte[] array3 = new byte[zSisesSum];
                using (MemoryStream memoryStream2 = new(array3)) {
                    foreach (Tuple<byte[], long> entryBlock in zList)
                        memoryStream2.Write(entryBlock.Item1, 0, Convert.ToInt32(entryBlock.Item2));
                }

                entryDeflatedData.Add(entry, array3);

                #if DEBUG
                Logger.LogDebug($"Deflating \"{entry.Name}\" ({++ndx}/{TableOfContent.Count}): {entry.Data.Length} -> {zSisesSum}");
                #endif
            }
        }

        /// <summary>
        /// Writes to the inputStream
        /// <para>Default 'seek' is true, flushes and seeks to the end of stream after write is finished</para>
        /// <para>Eliminates the need for coding output.Flush() followed by output.Seek(0, SeekOrigin.Begin)</para>
        /// </summary>
        /// <param name="inputStream"></param>
        public override void Save(Stream inputStream)
        {
            //TODO: really needed?
            bool encrypt = true;
            bool seek = true;

            header.ArchiveFlags = encrypt ? 4U : 0U;
            header.TOCEntrySize = 30U;

            // track artifacts
            WriteManifest();

            // Pack entries
            List<uint> zLengths;
            Dictionary<PsarcEntry, byte[]> zStreams;
            DeflateEntries(out zStreams, out zLengths);

            //Build zLengths
            BigEndianBinaryWriter writer = new(inputStream);
            header.TotalTOCSize = (uint) (32 + TableOfContent.Count * header.TOCEntrySize + zLengths.Count * bNum);
            TableOfContent[0].Offset = header.TotalTOCSize;

            for (int i = 1; i < TableOfContent.Count; i++) 
                TableOfContent[i].Offset = TableOfContent[i - 1].Offset + (ulong) zStreams[TableOfContent[i - 1]].Length;

            //Write Header
            writer.Write(header.MagicNumber);
            writer.Write(header.VersionNumber);
            writer.Write(header.CompressionMethod);
            writer.Write(header.TotalTOCSize);
            writer.Write(header.TOCEntrySize);
            writer.Write(TableOfContent.Count);
            writer.Write(header.BlockSizeAlloc);
            writer.Write(header.ArchiveFlags);

            //Write Table of contents
            foreach (PsarcEntry entry in TableOfContent) {
                entry.UpdateNameMD5();
                writer.Write(entry.MD5);
                writer.Write(entry.zIndexBegin);
                writer.WriteUInt40((ulong) entry.Data.Length);
                writer.WriteUInt40(entry.Offset);

                #if DEBUG
                Logger.LogDebug($"Writing tocData: {entry.Id}");
                #endif
            }

            foreach (uint zLen in zLengths)
                switch (bNum) {
                    case 2: //16bit
                        writer.Write((ushort) zLen);
                        break;
                    case 3: //24bit
                        writer.WriteUInt24(zLen);
                        break;
                    case 4: //32bit
                        writer.Write(zLen);
                        break;
                }

            zLengths = null;

            // Write zData
            foreach (PsarcEntry entry in TableOfContent) {
                // skip NamesBlock.bin
                //if (current.Name == "NamesBlock.bin")
                //    continue;

                //try
                //{
                // use chunk write method to avoid OOM Exceptions
                byte[] z = zStreams[entry];
                int len = z.Length;
                if (len > header.BlockSizeAlloc) {
                    using MemoryStreamExtension msInput = new(z);
                    using MemoryStreamExtension msExt = new();
                    using BigEndianBinaryWriter writer2 = new(msExt);
                    int bytesRead;
                    int totalBytesRead = 0;
                    byte[] buffer = new byte[header.BlockSizeAlloc];
                    while ((bytesRead = msInput.Read(buffer, 0, buffer.Length)) > 0) {
                        totalBytesRead += bytesRead;
                        if (totalBytesRead > len)
                            bytesRead = len - (totalBytesRead - bytesRead);

                        using MemoryStreamExtension msOutput = new();
                        msOutput.Write(buffer, 0, bytesRead);
                        writer2.Write(msOutput.ToArray());
                    }

                    writer.Write(msExt.ToArray());
                }
                else {
                    writer.Write(zStreams[entry]);
                }

                entry.Data?.Close();
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine("<ERROR> writer.Write: " + ex.Message);
                //    writer.Flush();
                //    writer.Dispose();
                //    break;
                //}

                #if DEBUG
                Logger.LogDebug($"Writing zData: {entry.Id}");
                #endif

            }

            zStreams = null;

            // Encrypt TOC
            if (encrypt) {
                using MemoryStreamExtension outputStream = new();
                using MemoryStreamExtension encStream = new();
                inputStream.Position = 32L;
                RijndaelEncryptor.EncryptPsarc(inputStream, outputStream, header.TotalTOCSize);
                inputStream.Position = 0L;

                // quick copy header from input stream
                byte[] buffer = new byte[32];
                encStream.Write(buffer, 0, inputStream.Read(buffer, 0, buffer.Length));
                encStream.Position = 32; //sanity check ofc
                inputStream.Flush();

                int tocSize = (int) header.TotalTOCSize - 32;
                int decSize = 0;
                buffer = new byte[1024 * 16]; // more efficient use of memory

                int ndx = 0; // for debugging

                int bytesRead;
                while ((bytesRead = outputStream.Read(buffer, 0, buffer.Length)) > 0) {
                    decSize += bytesRead;
                    if (decSize > tocSize)
                        bytesRead = tocSize - (decSize - bytesRead);

                    encStream.Write(buffer, 0, bytesRead);

                    #if DEBUG
                    Logger.LogDebug($"Writing encryptedData: {ndx++}");
                    #endif
                }

                inputStream.Position = 0;
                encStream.Position = 0;
                encStream.CopyTo(inputStream, (int) header.BlockSizeAlloc);
            }

            if (seek) {
                inputStream.Flush();
                inputStream.Seek(0, SeekOrigin.Begin);
            }
        }

        #endregion Writing

        public void AddEntry(string name, Stream data)
        {
            if (name == "NamesBlock.bin")
                return;

            PsarcEntry entry = new() {
                Name = name,
                Data = data,
                Length = (ulong) data.Length
            };

            AddEntry(entry);
        }

        public void AddEntry(string name, string fileName)
        {
            if (name == "NamesBlock.bin")
                return;

            if (!File.Exists(fileName))
                return;

            Stream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);

            PsarcEntry entry = new()
            {
                Name = name,
                Data = fileStream,
                Length = (ulong)fileStream.Length
            };

            AddEntry(entry);
        }

        public void AddEntry(PsarcEntry psarcEntry)
        {
            // important hierarchy
            TableOfContent.Add(psarcEntry);
            psarcEntry.Id = TableOfContent.Count - 1;
        }

        public void Extract(string path)
        {
            Parallel.ForEach(TableOfContent, e => {
                if (string.IsNullOrEmpty(e.Name))
                    return;

                e.Extract(path);
            });
        }

        #endregion Main functions

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            header = null;

            TableOfContent.ForEach(e => e.Data?.Dispose());
            TableOfContent.Clear();
        }

        #endregion IDisposable
    }
}