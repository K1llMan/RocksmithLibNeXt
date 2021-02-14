using System;
using System.IO;

using Elskom.Generic.Libs;

namespace RocksmithLibNeXt.Common.Archives
{
    public class Archives
    {
        #region Zip

        /// <summary>
        /// Packs data
        /// </summary>
        /// <param name="stream">Input stream</param>
        /// <param name="outStream">Out stream</param>
        /// <param name="plainLen">Plain data size</param>
        /// <param name ="rewind">Manual control for stream seek position</param>
        /// <returns></returns>
        public static long Zip(Stream stream, Stream outStream, long plainLen, bool rewind = true)
        {
            /*zlib works great, can't say that about SharpZipLib*/
            byte[] buffer = new byte[65536];
            ZOutputStream zOutputStream = new(outStream, 9);
            while (stream.Position < plainLen)
            {
                int size = (int)Math.Min(plainLen - stream.Position, buffer.Length);
                stream.Read(buffer, 0, size);
                zOutputStream.Write(buffer, 0, size);
            }
            zOutputStream.Finish();

            if (rewind)
            {
                outStream.Position = 0;
                outStream.Flush();
            }
            return zOutputStream.TotalOut;
        }

        /// <summary>
        /// Packs data
        /// </summary>
        /// <param name="data">Bytes array</param>
        /// <param name="outStream">Out stream</param>
        /// /// <param name="plainLen">Plain data size</param>
        /// <param name ="rewind">Manual control for stream seek position</param>
        /// <returns></returns>
        public static long Zip(byte[] data, Stream outStream, long plainLen, bool rewind = true)
        {
            return Zip(new MemoryStream(data), outStream, plainLen, rewind);
        }

        /// <summary>
        /// Unpacks zipped data
        /// </summary>
        /// <param name="stream">Input stream</param>
        /// <param name="outStream">Out stream</param>
        /// <param name="rewind">Manual control for stream seek position</param>
        public static void Unzip(Stream stream, Stream outStream, bool rewind = true)
        {
            int len;
            byte[] buffer = new byte[65536];
            ZInputStream zOutputStream = new(stream);

            while ((len = zOutputStream.Read(buffer, 0, buffer.Length)) > 0)
                outStream.Write(buffer, 0, len);

            zOutputStream.Close();

            if (rewind) {
                outStream.Position = 0;
                outStream.Flush();
            }
        }

        /// <summary>
        /// Unpacks zipped data
        /// </summary>
        /// <param name="data">Bytes array</param>
        /// <param name="outStream">Out stream</param>
        /// <param name="rewind">Manual control for stream seek position</param>
        public static void Unzip(byte[] data, Stream outStream, bool rewind = true)
        {
            Unzip(new MemoryStream(data), outStream, rewind);
        }

        #endregion Zip
    }
}