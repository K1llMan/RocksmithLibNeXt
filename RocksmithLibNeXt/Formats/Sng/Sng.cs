using System;
using System.IO;
using System.Security.Cryptography;

using RocksmithLibNeXt.Common.Archives;
using RocksmithLibNeXt.Common.Encryption;
using RocksmithLibNeXt.Common.Enums;
using RocksmithLibNeXt.Formats.Common;
using RocksmithLibNeXt.Formats.Sng.Models;

namespace RocksmithLibNeXt.Formats.Sng
{
    public class Sng: FileWorker
    {
        #region Constants

        private static byte[] SngKeyMac = {
            0x98, 0x21, 0x33, 0x0E, 0x34, 0xB9, 0x1F, 0x70,
            0xD0, 0xA4, 0x8C, 0xBD, 0x62, 0x59, 0x93, 0x12,
            0x69, 0x70, 0xCE, 0xA0, 0x91, 0x92, 0xC0, 0xE6,
            0xCD, 0xA6, 0x76, 0xCC, 0x98, 0x38, 0x28, 0x9D
        };

        private static byte[] SngKeyPC = {
            0xCB, 0x64, 0x8D, 0xF3, 0xD1, 0x2A, 0x16, 0xBF,
            0x71, 0x70, 0x14, 0x14, 0xE6, 0x96, 0x19, 0xEC,
            0x17, 0x1C, 0xCA, 0x5D, 0x2A, 0x14, 0x2E, 0x3E,
            0x59, 0xDE, 0x7A, 0xDD, 0xA1, 0x8A, 0x3A, 0x30
        };

        #endregion Constants

        #region Properties

        public SngData Data { get; set; }

        #endregion Properties

        #region Auxiliary functions

        private byte[] GetEncodingKey(GamePlatform platform)
        {
            switch (platform)
            {
                case GamePlatform.Pc:
                    return SngKeyPC;
                case GamePlatform.Mac:
                    return SngKeyMac;
                default:
                    return null;
            }
        }

        private void Decrypt(Stream input, Stream output, byte[] key)
        {
            BinaryReader reader = new(input);
            if (0x4A != reader.ReadUInt32())
                throw new InvalidDataException("This is not valid SNG file to decrypt.");
            reader.ReadBytes(4);    // platform header (bitfield? 001 - Compressed; 010 - Encrypted;)
            byte[] iv = reader.ReadBytes(16);

            using RijndaelManaged rij = RijndaelEncryptor.InitRijndael(key, CipherMode.CFB);
            rij.IV = iv;

            byte[] buffer = new byte[16];
            long len = input.Length - input.Position;
            for (long i = 0; i < len; i += buffer.Length)
            {
                using (ICryptoTransform transform = rij.CreateDecryptor())
                {
                    CryptoStream cs = new(output, transform, CryptoStreamMode.Write);
                    int bytesRead = input.Read(buffer, 0, buffer.Length);
                    cs.Write(buffer, 0, bytesRead);

                    int pad = buffer.Length - bytesRead;
                    if (pad > 0)
                        cs.Write(new byte[pad], 0, pad);

                    cs.Flush();
                }

                int j;
                bool carry;
                for (j = rij.IV.Length - 1, carry = true; j >= 0 && carry; j--)
                    carry = (iv[j] = (byte)(rij.IV[j] + 1)) == 0;
                rij.IV = iv;
            }

            output.SetLength(input.Length - (iv.Length + 8));

            output.Flush();
            output.Seek(0, SeekOrigin.Begin);
        }

        public void Encrypt(Stream input, Stream output, byte[] key)
        {
            BinaryWriter writer = new(output);

            writer.Write(0x4A);
            // For PC and Mac always 3
            // int platformHeader = conv == EndianBitConverter.Big ? 1 : 3;
            writer.Write(3);

            byte[] iv = new byte[16];
            using RijndaelManaged rij = RijndaelEncryptor.InitRijndael(key, CipherMode.CFB);
            output.Write(iv, 0, iv.Length);

            byte[] buffer = new byte[16];
            long len = input.Length - input.Position;
            for (long i = 0; i < len; i += buffer.Length)
            {
                using (ICryptoTransform transform = rij.CreateEncryptor())
                {
                    CryptoStream cs = new(output, transform, CryptoStreamMode.Write);
                    int bytesRead = input.Read(buffer, 0, buffer.Length);
                    cs.Write(buffer, 0, bytesRead);

                    int pad = buffer.Length - bytesRead;
                    if (pad > 0)
                        cs.Write(new byte[pad], 0, pad);

                    cs.FlushFinalBlock();
                }

                int j;
                bool carry;
                for (j = rij.IV.Length - 1, carry = true; j >= 0 && carry; j--)
                    carry = (iv[j] = (byte)(rij.IV[j] + 1)) == 0;
                rij.IV = iv;
            }

            output.Write(new byte[56]); // append zero signature
        }

        #endregion Auxiliary functions

        #region Main functions

        public void Open(string fileName, GamePlatform platform)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException();

            FileStream fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            Open(fs, platform);
        }

        public override void Open(Stream fileStream)
        {
            Open(fileStream, GamePlatform.Pc);
        }

        public void Open(Stream fileStream, GamePlatform platform)
        {
            using MemoryStream decrypted = new();
            using BinaryReader ebrDec = new(decrypted);

            Decrypt(fileStream, decrypted, GetEncodingKey(platform));

            using MemoryStream output = new();
            //unZip
            ushort xU = ebrDec.ReadUInt16();
            decrypted.Position -= 2;
            if (xU == 0x78DA || xU == 0xDA78) //LE 55928 //BE 30938
                Archives.Unzip(decrypted, output, false);

            output.Seek(0, SeekOrigin.Begin);

            Data = SngData.Read(new BinaryReader(output));
        }

        public void Save(string fileName, GamePlatform platform, bool replace = false)
        {
            if (File.Exists(fileName) && !replace)
                throw new Exception($"File \"{fileName}\" already exists.");

            FileStream fs = File.Open(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            Save(fs, platform);
        }

        public override void Save(Stream fileStream)
        {
            Save(fileStream, GamePlatform.Pc);
        }

        public void Save(Stream output, GamePlatform platform)
        {
            using MemoryStream decrypted = new();
            using BinaryWriter writer = new(decrypted);
            Data.Write(writer);

            using MemoryStream zipped = new();
            Archives.Zip(decrypted, zipped, decrypted.Length, false);

            using MemoryStream encrypted = new();
            Encrypt(zipped, encrypted, GetEncodingKey(platform));

            output.Write(encrypted.GetBuffer());
        }

        #endregion Main functions

        /*
        public static void PackSng(Stream input, Stream output, Platform platform)
        {
            EndianBitConverter conv = platform.GetBitConverter;
            int platformHeader = conv == EndianBitConverter.Big ? 1 : 3;

            using (var w = new EndianBinaryWriter(conv, output))
            {
                using (MemoryStream zData = new())
                {
                    using (MemoryStream plain = new())
                    {
                        using (MemoryStream encrypted = new())
                        {
                            using (var encw = new EndianBinaryWriter(conv, plain))
                            {
                                w.Write(0x4A);
                                w.Write(platformHeader);

                                // pack with zlib TODO: better packing required!
                                RijndaelEncryptor.Zip(input, zData, input.Length);

                                if (platformHeader == 3)
                                {
                                    // write size of uncompressed data and packed data itself | already there
                                    encw.Write((int)input.Length);
                                    encw.Write(zData.GetBuffer());
                                    encw.Flush();

                                    // choose key
                                    byte[] key;
                                    switch (platform.platform)
                                    {
                                        case GamePlatform.Mac:
                                            key = RijndaelEncryptor.SngKeyMac;
                                            break;
                                        default: //PC
                                            key = RijndaelEncryptor.SngKeyPC;
                                            break;
                                    }

                                    // encrypt (writes 16B IV and encrypted data)
                                    plain.Position = 0;
                                    RijndaelEncryptor.EncryptSngData(plain, encrypted, key);
                                    w.Write(encrypted.GetBuffer());
                                    w.Write(new byte[56]); // append zero signature
                                }
                                else
                                {
                                    // unencrypted and unsigned
                                    w.Write((int)input.Length);
                                    w.Write(zData.GetBuffer());
                                }

                                output.Flush();
                            }
                        }
                    }
                }
            }
        }
        */


        private byte[] chartBE;
        private byte[] chartLE;
        /*
        public static Sng ConvertXML(string xmlPath, ArrangementType type, string cdata = null)
        {
            return type == ArrangementType.Vocal 
                ? Sng2014FileWriter.ReadVocals(xmlPath, cdata) 
                : ConvertSong(xmlPath);
        }

        // this is platform independent SNG object
        public static Sng ConvertSong(string xmlFile)
        {
            var song = Song2014LoadFromFile(xmlFile);
            Sng2014FileWriter parser = new();
            Sng sng = new();
            parser.ReadSong(song, sng);
            sng.NoteCount = parser.NoteCount;
            sng.DNACount = parser.DNACount;
            return sng;
        }

        /// <summary>
        /// Raw SNG data reader.
        /// </summary>
        /// <param name="inputFile">Packed and encrypted SNG file</param>
        /// <returns><see cref="Sng" /> exemplar.</returns>
        /// <param name="platform"></param>
        public static Sng LoadFromFile(string inputFile, Platform platform)
        {
            using (FileStream fs = new(inputFile, FileMode.Open)) {
                return ReadSng(fs, platform);
            }
        }

        public static Sng ReadSng(Stream input, Platform platform)
        {
            Sng sng = new();

            using (MemoryStream ms = new()) {
                using (var r = new EndianBinaryReader(platform.GetBitConverter, ms)) {
                    UnpackSng(input, ms, platform);
                    ms.Flush();
                    ms.Seek(0, SeekOrigin.Begin);
                    sng.Read(r);
                }
            }

            return sng;
        }

        public static void UnpackSng(Stream input, Stream output, Platform platform)
        {
            EndianBitConverter conv = platform.GetBitConverter;

            using (MemoryStream decrypted = new()) {
                using (var ebrDec = new EndianBinaryReader(conv, decrypted)) {
                    byte[] key;
                    switch (platform.platform) {
                        case GamePlatform.Mac:
                            key = RijndaelEncryptor.SngKeyMac;
                            break;
                        case GamePlatform.Pc:
                            key = RijndaelEncryptor.SngKeyPC;
                            break;
                        default:
                            key = null;
                            break;
                    }

                    if (key != null) {
                        RijndaelEncryptor.DecryptSngData(input, decrypted, key, conv);
                    }
                    else {
                        input.CopyTo(decrypted);
                        decrypted.Seek(8, SeekOrigin.Begin);
                    }

                    //unZip
                    long plainLen = ebrDec.ReadUInt32();
                    ushort xU = ebrDec.ReadUInt16();
                    decrypted.Position -= 2;
                    if (xU == 0x78DA || xU == 0xDA78) //LE 55928 //BE 30938
                        RijndaelEncryptor.Unzip(decrypted, output, false);
                }
            }
        }

        public void WriteSng(Stream output, Platform platform)
        {
            byte[] chartData = getChartData(platform);
            using (Stream input = new MemoryStream(chartData)) {
                PackSng(input, output, platform);
            }
        }

        public static void PackSng(Stream input, Stream output, Platform platform)
        {
            EndianBitConverter conv = platform.GetBitConverter;
            int platformHeader = conv == EndianBitConverter.Big ? 1 : 3;

            using (var w = new EndianBinaryWriter(conv, output)) {
                using (MemoryStream zData = new()) {
                    using (MemoryStream plain = new()) {
                        using (MemoryStream encrypted = new()) {
                            using (var encw = new EndianBinaryWriter(conv, plain)) {
                                w.Write(0x4A);
                                w.Write(platformHeader);

                                // pack with zlib TODO: better packing required!
                                RijndaelEncryptor.Zip(input, zData, input.Length);

                                if (platformHeader == 3) {
                                    // write size of uncompressed data and packed data itself | already there
                                    encw.Write((int) input.Length);
                                    encw.Write(zData.GetBuffer());
                                    encw.Flush();

                                    // choose key
                                    byte[] key;
                                    switch (platform.platform) {
                                        case GamePlatform.Mac:
                                            key = RijndaelEncryptor.SngKeyMac;
                                            break;
                                        default: //PC
                                            key = RijndaelEncryptor.SngKeyPC;
                                            break;
                                    }

                                    // encrypt (writes 16B IV and encrypted data)
                                    plain.Position = 0;
                                    RijndaelEncryptor.EncryptSngData(plain, encrypted, key);
                                    w.Write(encrypted.GetBuffer());
                                    w.Write(new byte[56]); // append zero signature
                                }
                                else {
                                    // unencrypted and unsigned
                                    w.Write((int) input.Length);
                                    w.Write(zData.GetBuffer());
                                }

                                output.Flush();
                            }
                        }
                    }
                }
            }
        }

        private byte[] getChartData(Platform platform)
        {
            using (MemoryStream stream = new()) {
                var conv = platform.GetBitConverter;

                // cached result
                if (conv == EndianBitConverter.Little && chartLE != null)
                    return chartLE;
                if (conv == EndianBitConverter.Big && chartBE != null)
                    return chartBE;

                using (var w = new EndianBinaryWriter(conv, stream)) {
                    Write(w);
                }

                stream.Flush();

                byte[] data = stream.ToArray();
                if (conv == EndianBitConverter.Little)
                    chartLE = data;
                else
                    chartBE = data;

                return data;
            }
        }

        public void WriteChartData(string outfile, Platform platform)
        {
            byte[] data = getChartData(platform);
            using (FileStream fs = new(outfile, FileMode.Create)) {
                fs.Write(data, 0, data.Length);
            }
        }

        public bool IsCustomFont()
        {
            return !SymbolsTexture.SymbolsTextures[0].Font.ToNullTerminatedUTF8().Contains("lyrics.dds");
        }

        public void Read(EndianBinaryReader r)
        {
            BPMs = new BpmCollection();
            BPMs.Read(r);
            Phrases = new PhraseCollection();
            Phrases.Read(r);
            Chords = new ChordCollection();
            Chords.Read(r);
            ChordNotes = new ChordNotesCollection();
            ChordNotes.Read(r);
            Vocals = new VocalCollection();
            Vocals.Read(r);
            if (Vocals.Count > 0) {
                SymbolsHeader = new SymbolsHeaderCollection();
                SymbolsHeader.Read(r);
                SymbolsTexture = new SymbolsTextureCollection();
                SymbolsTexture.Read(r);
                SymbolsDefinition = new SymbolDefinitionCollection();
                SymbolsDefinition.Read(r);
            }

            PhraseIterations = new PhraseIterationCollection();
            PhraseIterations.read(r);
            PhraseExtraInfo = new PhraseExtraInfoByLevelCollection();
            PhraseExtraInfo.read(r);
            NLD = new NLinkedDifficultyCollection();
            NLD.read(r);
            Actions = new ActionCollection();
            Actions.Read(r);
            Events = new EventCollection();
            Events.read(r);
            Tones = new ToneCollection();
            Tones.read(r);
            DNAs = new DnaCollection();
            DNAs.read(r);
            Sections = new SectionCollection();
            Sections.Read(r);
            Arrangements = new ArrangementCollection();
            Arrangements.Read(r);
            Metadata = new Metadata();
            Metadata.Read(r);
        }

        public void Write(EndianBinaryWriter w)
        {
            writeStruct(w, BPMs);
            writeStruct(w, Phrases);
            writeStruct(w, Chords);
            writeStruct(w, ChordNotes);

            writeStruct(w, Vocals);
            if (Vocals.Count > 0) {
                writeStruct(w, SymbolsHeader);
                writeStruct(w, SymbolsTexture);
                writeStruct(w, SymbolsDefinition);
            }

            writeStruct(w, PhraseIterations);
            writeStruct(w, PhraseExtraInfo);
            writeStruct(w, NLD);
            writeStruct(w, Actions);
            writeStruct(w, Events);
            writeStruct(w, Tones); // monitor for multitone exceptions
            writeStruct(w, DNAs);
            writeStruct(w, Sections);
            writeStruct(w, Arrangements);
            writeStruct(w, Metadata);
        }

        public MemoryStream CopyStruct(object obj)
        {
            EndianBitConverter conv = EndianBitConverter.Little;
            MemoryStream data = new();
            var w = new EndianBinaryWriter(conv, data);
            writeStruct(w, obj);
            w.Flush();
            data.Position = 0;
            return data;
        }

        public uint HashStruct(object obj)
        {
            MemoryStream data = CopyStruct(obj);
            uint crc = Crc32.Compute(data.ToArray());
            return crc;
        }

        private void writeStruct(EndianBinaryWriter w, object obj)
        {
            string[] order = (string[]) getPropertyValue(obj, "order");
            foreach (string name in order) {
                object value = getPropertyValue(obj, name);
                if (value.GetType().IsArray || value.GetType().IsPrimitive)
                    writeField(w, value);
                else
                    writeStruct(w, value);
            }
        }

        private void writeField(EndianBinaryWriter w, object value)
        {
            Type type = value.GetType();
            string typeName = type.Name;

            if (type.IsArray) {
                if (type.GetElementType().IsPrimitive)
                    foreach (object v in (IEnumerable) value) {
                        writeField(w, v);
                    }
                else
                    foreach (object v in (IEnumerable) value)
                        writeStruct(w, v);
            }
            else {
                switch (typeName) {
                    case "UInt32":
                        w.Write((uint) value);
                        break;
                    case "Int32":
                        w.Write((int) value);
                        break;
                    case "Int16":
                        w.Write((short) value);
                        break;
                    case "Byte":
                        w.Write((byte) value);
                        break;
                    case "Single":
                        w.Write((float) value);
                        break;
                    case "Double":
                        w.Write((double) value);
                        break;
                    default:
                        throw new Exception("Unhandled type");
                }
            }
        }

        private object getPropertyValue(object obj, string propertyName)
        {
            Type t = obj.GetType();
            PropertyInfo prop = t.GetProperty(propertyName);
            if (prop != null)
                return prop.GetValue(obj, null);
            throw new Exception("Unknown or unaccessible property");
        }

        // none, solo, riff, chord
        public int[] DNACount { get; set; }

        // Easy, Medium, Hard = 0, 1, 2
        public int[] NoteCount { get; set; }

        public Sng()
        {
        }

        public Sng(Stream data)
        {
            var r = new EndianBinaryReader(EndianBitConverter.Little, data);
            Read(r);
        }
        */
    }
}