using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using Microsoft.Extensions.Logging;

using RocksmithLibNeXt.Formats.Common;

namespace RocksmithLibNeXt.Formats.Psarc.Models
{
    public class PsarcEntry : Loggable, IDisposable
    {
        #region Properties

        public int Id { get; set; }

        public byte[] MD5 { get; set; }

        public uint zIndexBegin { get; set; }

        /// <summary>
        /// Original data length of this entry
        /// </summary>
        /// <value>The length</value>
        public ulong Length { get; set; }

        /// <summary>
        /// Starting offset from
        /// </summary>
        /// <value>The offset</value>
        public ulong Offset { get; set; }

        public Stream Data { get; set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="PsarcEntry"/> is compressed
        /// </summary>
        /// <value><c>true</c> if compressed; otherwise, <c>false</c></value>
        /// <remarks>Kinda rubbish but could be useful someday. Now inactive</remarks>
        public bool Compressed { get; set; }

        public string Name { get; set; }

        #endregion Properties

        #region Main functions

        public PsarcEntry()
        {
            Id = 0;
            Name = string.Empty;
        }

        //My best guess is: RS2014 uses method like PSARCBrowser, they pick file from Psarc using md5(faster to use first file from collection, since it's defined be format, haha) and meet 2 of them, so they fail to read filenames from Manifest, rest is impossible. Changed check to Name instead of Id.
        public void UpdateNameMD5()
        {
            MD5 = Name == string.Empty ? new byte[16] : new MD5CryptoServiceProvider().ComputeHash(Encoding.ASCII.GetBytes(Name));
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