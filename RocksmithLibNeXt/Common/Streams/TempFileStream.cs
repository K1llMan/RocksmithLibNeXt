using System.IO;

namespace RocksmithLibNeXt.Common.Streams
{
    public class TempFileStream : FileStream
    {
        private const int _buffer_size = 65536;

        public TempFileStream()
            : base(Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite, FileShare.Read, _buffer_size, FileOptions.DeleteOnClose)
        {
        }

        public TempFileStream(FileMode mode) // for Appending can not use FileAccess.ReadWrite
            : base(Path.GetTempFileName(), mode, FileAccess.Write, FileShare.Read, _buffer_size, FileOptions.DeleteOnClose)
        {
        }

        public TempFileStream(FileAccess access)
            : base(Path.GetTempFileName(), FileMode.Create, access, FileShare.Read, _buffer_size, FileOptions.DeleteOnClose)
        {
        }

        public TempFileStream(FileAccess access, FileShare share)
            : base(Path.GetTempFileName(), FileMode.Create, access, share, _buffer_size, FileOptions.DeleteOnClose)
        {
        }

        public TempFileStream(FileAccess access, FileShare share, int bufferSize)
            : base(Path.GetTempFileName(), FileMode.Create, access, share, bufferSize, FileOptions.DeleteOnClose)
        {
        }

        public TempFileStream(string path, FileMode mode)
            : base(path, mode)
        {
        }
    }
}