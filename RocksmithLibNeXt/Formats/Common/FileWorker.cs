using System;
using System.IO;

namespace RocksmithLibNeXt.Formats.Common
{
    /// <summary>
    /// Base file reader class
    /// </summary>
    public class FileWorker : Loggable
    {
        public void Open(string fileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException();

            FileStream fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            Open(fs);
        }

        public virtual void Open(Stream fileStream)
        {
            throw new NotImplementedException();
        }

        public void Save(string fileName, bool replace = false)
        {
            if (File.Exists(fileName) && !replace)
                throw new Exception($"File \"{fileName}\" already exists.");

            FileStream fs = File.Open(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            Save(fs);
        }

        public virtual void Save(Stream fileStream)
        {
            throw new NotImplementedException();
        }
    }
}