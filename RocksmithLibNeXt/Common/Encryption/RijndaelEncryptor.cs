using System;
using System.IO;
using System.Security.Cryptography;

namespace RocksmithLibNeXt.Common.Encryption
{
    public static class RijndaelEncryptor
    {
        #region RS1

        public static byte[] DLCKey = new byte[32] {
            0xFA, 0x6F, 0x4F, 0x42, 0x3E, 0x66, 0x9F, 0x9E,
            0x6A, 0xD2, 0x3A, 0x2F, 0x8F, 0xE5, 0x81, 0x88,
            0x63, 0xD9, 0xB8, 0xFD, 0xED, 0xDF, 0xFE, 0xBD,
            0x12, 0xB2, 0x7F, 0x76, 0x80, 0xD1, 0x51, 0x41
        };

        public static byte[] PCFilesKey = new byte[32] {
            0xB8, 0x7A, 0x00, 0xBD, 0xB8, 0x9C, 0x21, 0x03,
            0xA3, 0x94, 0xC0, 0x44, 0x71, 0x51, 0xEE, 0xC4,
            0x3C, 0x3F, 0x72, 0x17, 0xCA, 0x7F, 0x44, 0xC1,
            0xE4, 0x36, 0xFC, 0xFC, 0x84, 0xE6, 0xE7, 0x15
        };

        #endregion

        #region RS2

        //metadata
        public static byte[] PCMetaDatKey = new byte[32] {
            0x5F, 0xB0, 0x23, 0xEF, 0x19, 0xD5, 0xDC, 0x37,
            0xAD, 0xDA, 0xC8, 0xF0, 0x17, 0xF8, 0x8F, 0x0E,
            0x98, 0x18, 0xA3, 0xAC, 0x2F, 0x72, 0x46, 0x96,
            0xA5, 0x9D, 0xE2, 0xBF, 0x05, 0x25, 0x12, 0xEB
        };

        //profile and other cdr profile.json stuff common for RS2\RS1
        public static byte[] PCSaveKey = new byte[32] {
            0x72, 0x8B, 0x36, 0x9E, 0x24, 0xED, 0x01, 0x34,
            0x76, 0x85, 0x11, 0x02, 0x18, 0x12, 0xAF, 0xC0,
            0xA3, 0xC2, 0x5D, 0x02, 0x06, 0x5F, 0x16, 0x6B,
            0x4B, 0xCC, 0x58, 0xCD, 0x26, 0x44, 0xF2, 0x9E
        };

        public static byte[] IniKey_Mac = new byte[32] {
            0x37, 0x8B, 0x90, 0x26, 0xEE, 0x7D, 0xE7, 0x0B,
            0x8A, 0xF1, 0x24, 0xC1, 0xE3, 0x09, 0x78, 0x67,
            0x0F, 0x9E, 0xC8, 0xFD, 0x5E, 0x72, 0x85, 0xA8,
            0x64, 0x42, 0xDD, 0x73, 0x06, 0x8C, 0x04, 0x73
        };

        #endregion

        #region Auxiliary function

        private static void Crypto(Stream input, Stream output, ICryptoTransform transform, long len)
        {
            byte[] buffer = new byte[512];
            int pad = buffer.Length - (int) (len % buffer.Length);
            CryptoStream coder = new(output, transform, CryptoStreamMode.Write);
            while (input.Position < len) {
                int size = (int) Math.Min(len - input.Position, buffer.Length);
                input.Read(buffer, 0, size);
                coder.Write(buffer, 0, size);
            }

            if (pad > 0)
                coder.Write(new byte[pad], 0, pad);

            coder.Flush();
            output.Seek(0, SeekOrigin.Begin);
            output.Flush();
        }

        #endregion Auxiliary function

        #region Main functions

        /*
        public static void EncryptFile(Stream input, Stream output, byte[] key, CipherMode mode, long len)
        {
            using RijndaelManaged rij = new();
            InitRijndael(rij, key, CipherMode.ECB);
            Crypto(input, output, rij.CreateEncryptor(), input.Length);
        }

        public static void DecryptFile(Stream input, Stream output, byte[] key, CipherMode mode, long len)
        {
            using RijndaelManaged rij = new();
            InitRijndael(rij, key, CipherMode.ECB);
            Crypto(input, output, rij.CreateDecryptor(), input.Length);
        }         
         */

        public static RijndaelManaged InitRijndael(byte[] key, CipherMode cipher)
        {
            return new() {
                Padding = PaddingMode.None,
                Mode = cipher,
                BlockSize = 128,
                IV = new byte[16],
                Key = key, // byte[32]
            };
        }

        public static void EncryptFile(Stream input, Stream output, byte[] key, CipherMode mode, long len)
        {
            using RijndaelManaged rij = InitRijndael(key, mode);
            Crypto(input, output, rij.CreateEncryptor(), len);
        }

        public static void DecryptFile(Stream input, Stream output, byte[] key, CipherMode mode, long len)
        {
            using RijndaelManaged rij = InitRijndael(key, mode);
            Crypto(input, output, rij.CreateDecryptor(), len);
        }

        #endregion Main functions

        #region ProfileEncryption

        /*
                 /// <summary>
        /// All profile stuff: crd (u play credentials), LocalProfiles.json and profiles themselves
        /// Good for RS2014 and RS1
        /// </summary>
        /// <param name="str"></param>
        /// <param name="outStream"></param>
        public static void DecryptProfile(Stream str, Stream outStream)
        {
            var source = EndianBitConverter.Little;
            var dec = EndianBitConverter.Big;

            str.Position = 0;
            using var decrypted = new MemoryStream();
            using (var br = new EndianBinaryReader(source, str))
                using (var brDec = new EndianBinaryReader(dec, decrypted))
                {
                    //EVAS + header
                    br.ReadBytes(16);
                    uint zLen = br.ReadUInt32();
                    DecryptFile(br.BaseStream, decrypted, PCSaveKey);

                    //unZip
                    ushort xU = brDec.ReadUInt16();
                    brDec.BaseStream.Position -= sizeof(ushort);
                    if (xU == 30938)//LE 55928 //BE 30938
                    {
                        Unzip(brDec.BaseStream, outStream);
                    }//endless loop if not
                }
        }
         */

        #endregion ProfileEncryption

        /*
        #region PS3 EDAT Encrypt/Decrypt
        private const string Flags = "0C",    //0x0c
                             Type = "00",
                             Version = "03";  //02 or 03
        private const string kLic = "CB4A06E85378CED307E63EFD1084C19D";
        private const string ContentID = "UP0001-BLUS30670_00-RS001PACK0000003";

        /// <summary>
        /// Ensure that we running JVM x86
        /// </summary>
        /// <returns></returns>
        public static bool IsJavaInstalled()
        {
            try {
                using var version = new Process();
                version.StartInfo.FileName = "java";
                version.StartInfo.Arguments = "-version";
                version.StartInfo.CreateNoWindow = true;
                version.StartInfo.UseShellExecute = false;
                // Java uses this output instead of stout.
                version.StartInfo.RedirectStandardError = true;
                version.Start();
                version.WaitForExit();

                // Get the output into a string
                var output = version.StandardError.ReadLine();
                if (!output.Contains("java version"))
                    return false;

                // Parse java version and detect if it's good.
                var javaVer = output.Split('\"')[1].Split('.');
                int maj = int.Parse(javaVer[0]);
                int min = int.Parse(javaVer[1]);

                if (maj > 0 && min >= 0)
                    return true;

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Encrypt using TrueAncestor Edat Rebuilder 
        /// NOTE: source files must be in "/edat" folder in application root directory
        /// </summary>
        /// <returns>Output message from execution</returns>
        public static string EncryptPS3Edat()
        {
            if (!IsJavaInstalled())
                return "No JDK or JRE is installed on your machine";

            var errors = String.Empty;
            var files = Directory.EnumerateFiles(Path.Combine(ExternalApps.TOOLKIT_ROOT, "edat"), "*.psarc");

            foreach (var InFile in files)
            {
                var OutFile = InFile + ".edat";
                var command = String.Format("EncryptEDAT \"{0}\" \"{1}\" {2} {3} {4} {5} {6}",
                    InFile, OutFile, kLic, ContentID, Flags, Type, Version);

                errors += EdatCrypto(command);
            }


            return String.IsNullOrEmpty(errors) ? Packer.EDAT_MSG : errors;
        }

        /// <summary>
        /// Decrypt using TrueAncestor Edat Rebuilder 
        /// NOTE: files must be in "/edat" folder in application root directory
        /// </summary>
        /// <returns>Output message from execution</returns>
        public static string DecryptPS3Edat()
        {
            if (!IsJavaInstalled())
                return "No JDK or JRE is installed on your machine";

            var errors = String.Empty;
            var files = Directory.EnumerateFiles(Path.Combine(ExternalApps.TOOLKIT_ROOT, "edat"), "*.edat").ToList();

            foreach (var InFile in files)
            {
                var OutFile = Path.ChangeExtension(InFile, ".dat");
                var command = String.Format("DecryptFree \"{0}\" \"{1}\" {2}", InFile, OutFile, kLic);
                errors += EdatCrypto(command);
            }

            return String.IsNullOrEmpty(errors) ? "Decrypt all EDAT files successfully" : errors;
        }

        internal static string EdatCrypto(string command)
        {
            // Encrypt/Decrypt using TrueAncestor Edat Rebuilder v1.4c
            using var PS3Process = new Process();
            PS3Process.StartInfo.FileName = "java";
            PS3Process.StartInfo.Arguments = String.Format("-cp \"{0}\" -Xms256m -Xmx1024m {1}", Path.Combine(ExternalApps.TOOLKIT_ROOT, ExternalApps.APP_COREJAR), command);
            PS3Process.StartInfo.WorkingDirectory = ExternalApps.TOOLKIT_ROOT;
            PS3Process.StartInfo.UseShellExecute = false;
            PS3Process.StartInfo.CreateNoWindow = true;
            PS3Process.StartInfo.RedirectStandardError = true;
            PS3Process.Start();
            PS3Process.WaitForExit();

            var stdout = PS3Process.StandardError.ReadToEnd();
            //Improve me please
            if (!String.IsNullOrEmpty(stdout))
                return String.Format("System error occurred {0}\n", stdout);

            return "";
        }

        #endregion
        */
    }
}