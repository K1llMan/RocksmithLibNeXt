using System;
using System.IO;

using RocksmithLibNeXt.Formats.Psarc;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Psarc archive = new Psarc(false);
            archive.Open("C:\\testOut.psarc");
            archive.Extract("C:\\extractTest2");
            //archive.Save("C:\\testOut.psarc", true);

            //archive.Read(fileStream);

            Console.ReadLine();
        }
    }
}
