using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using Microsoft.Extensions.Logging;

using RocksmithLibNeXt.Common.Archives;
using RocksmithLibNeXt.Common.Streams;
using RocksmithLibNeXt.Formats.Common;

namespace RocksmithLibNeXt.Formats.Psarc.Models
{
    public class PsarcEntry : Loggable, IDisposable
    {
        #region Fieds

        private Stream dataStream;

        #endregion Fieds

        #region Properties

        public int Id { get; set; }

        public byte[] MD5 { get; set; }

        public int zIndexBegin { get; set; }

        /// <summary>
        /// Original data length of this entry
        /// </summary>
        /// <value>The length</value>
        public long Length { get; set; }

        /// <summary>
        /// Starting offset from
        /// </summary>
        /// <value>The offset</value>
        public long Offset { get; set; }

        /// <summary>
        /// Data stream
        /// </summary>
        public Stream Data
        {
            get { return GetStream(); }
            set { dataStream = value; }
        }

            /// <summary>
        /// Gets a value indicating whether this <see cref="PsarcEntry"/> is compressed
        /// </summary>
        /// <value><c>true</c> if compressed; otherwise, <c>false</c></value>
        /// <remarks>Kinda rubbish but could be useful someday. Now inactive</remarks>
        public bool Compressed { get; set; }

        /// <summary>
        /// Entry name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Sizes of compressed blocks
        /// </summary>
        public List<int> BlockSizes { get; set; }

        #endregion Properties

        #region Auxiliary functions

        private Stream InflateData(Stream stream)
        {
            byte[] deflatedDate = new byte[BlockSizes.Sum()];

            // Lock stream while reading deflated data
            lock (stream) {
                stream.Position = Offset;
                stream.Read(deflatedDate);
            }

            BigEndianBinaryReader reader = new(new MemoryStream(deflatedDate));

            const int zHeader = 0x78DA;

            MemoryStream outputStream = new();
            foreach (int size in BlockSizes)
            {
                ushort num = reader.ReadUInt16();
                reader.BaseStream.Position -= 2L;

                byte[] data = reader.ReadBytes(size);

                // Compressed
                if (num == zHeader)
                    try
                    {
                        Archives.Unzip(data, outputStream, false);
                    }
                    catch (Exception ex)
                    {
                        // corrupt CDLC zlib.net exception ... try to unpack
                        Logger.LogError(string.IsNullOrEmpty(Name)
                            ? @$"CDLC contains a zlib exception.{Environment.NewLine}Warning: {ex}"
                            : @$"CDLC contains a broken datachunk in file '{Name.Split('/').Last()}'.{Environment.NewLine}Warning Type 1: {ex}");
                    }
                // Raw
                else
                    outputStream.Write(data, 0, data.Length);
            }

            outputStream.Seek(0, SeekOrigin.Begin);
            outputStream.Flush();

            return outputStream; 
        }

        private Stream DeflateData(Stream stream)
        {
            // TODO: block size from header
            int blockSize = 65536;

            BlockSizes.Clear();
            Length = stream.Length;
            stream.Seek(0, SeekOrigin.Begin);

            MemoryStream outputStream = new();

            while (stream.Position < stream.Length)
            {
                byte[] arrayI = new byte[blockSize];
                byte[] arrayO = new byte[blockSize * 2];

                using MemoryStream memoryStream = new(arrayO);

                int plainLen = stream.Read(arrayI);
                long packedLen = Archives.Zip(arrayI, memoryStream, plainLen, false);

                // If packed data "worse" than plain (i.e. already packed) z = 0
                if (packedLen >= plainLen)
                {
                    BlockSizes.Add(plainLen);
                    outputStream.Write(arrayI, 0, plainLen);
                }
                // If packed data is good
                else
                {
                    int size = packedLen < blockSize - 1 ? (int)packedLen : plainLen;

                    BlockSizes.Add(size);
                    outputStream.Write(memoryStream.ToArray(), 0, size);
                }
            }

            #if DEBUG
            Logger.LogDebug($"Deflating \"{Name}\": {stream.Length} -> {outputStream.Length}");
            #endif

            return outputStream;
        }

        private Stream GetStream()
        {
            if (!Compressed)
                return dataStream;

            return InflateData(dataStream);
        }

        #endregion Auxiliary functions

        #region Main functions

        public PsarcEntry(Stream stream = null)
        {
            dataStream = stream;

            Id = 0;
            Name = string.Empty;
            BlockSizes = new List<int>();
        }

        //My best guess is: RS2014 uses method like PSARCBrowser, they pick file from Psarc using md5(faster to use first file from collection, since it's defined be format, haha) and meet 2 of them, so they fail to read filenames from Manifest, rest is impossible. Changed check to Name instead of Id.
        public void UpdateNameMD5()
        {
            MD5 = Name == string.Empty ? new byte[16] : new MD5CryptoServiceProvider().ComputeHash(Encoding.ASCII.GetBytes(Name));
        }

        public Stream GetDeflatedStream()
        {
            return DeflateData(Data);
        }

        public void Extract(string path)
        {
            string fullPath = Path.Combine(path, Name.Replace('/', Path.DirectorySeparatorChar));

            try {
                if (!Directory.Exists(Path.GetDirectoryName(fullPath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

                using FileStream fs = new(fullPath, FileMode.Create);
                Data.CopyTo(fs);

                #if DEBUG
                Logger.LogDebug($"\"{Name}\" extracted.");
                #endif

            }
            catch (Exception ex) {
                Logger.LogError($"Extraction error on {fullPath}: {ex}.");
            }
        }

        #endregion Main functions

        #region IDisposable implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            Data?.Dispose();
            MD5 = null;
        }

        #endregion

        public override string ToString()
        {
            return Name;
        }
    }
}