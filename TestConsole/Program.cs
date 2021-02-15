using System;

using RocksmithLibNeXt.Formats.Psarc;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Psarc archive = new(false);

            if (!true) {
                archive.Open("C:\\testOut.psarc");
                archive.Extract("C:\\extractTest2");
            }
            else {
                archive.Open("C:\\test.psarc");
                archive.Save("C:\\testOut.psarc", true);
            }

            //archive.Read(fileStream);

            Console.ReadLine();
        }
    }
}
