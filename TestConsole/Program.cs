using System;
using System.IO;
using System.Linq;

using RocksmithLibNeXt.Formats.Psarc;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Psarc psarc = new(false);

            string outputDir = "C:\\extractTest3";
            if (true) {
                psarc.Open("C:\\testOut.psarc");
                psarc.Extract(outputDir);
            }
            else {
                Directory.GetFiles(outputDir, "*", SearchOption.AllDirectories).ToList().ForEach(f => {
                    string relPath = Path.GetRelativePath(outputDir, f);
                    psarc.AddEntry(relPath, f);
                });
                //archive.Open("C:\\test.psarc");
                psarc.Save("C:\\testOut.psarc", true);
                //psarc.SaveOld("C:\\testOut.psarc", true);
            }

            //archive.Read(fileStream);

            Console.ReadLine();
        }
    }
}
