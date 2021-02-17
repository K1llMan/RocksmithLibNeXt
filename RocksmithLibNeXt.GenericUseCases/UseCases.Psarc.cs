using System.IO;
using System.Linq;

using RocksmithLibNeXt.Formats.Psarc;

namespace RocksmithLibNeXt.GenericUseCases
{
    public static partial class UseCases
    {
        public static Psarc PsarcOpen(string fileName)
        {
            Psarc psarc = new();

            psarc.Open(fileName);

            return psarc;
        }

        public static void PsarcExtract(string fileName, string outputDir)
        {
            Psarc psarc = new();
            psarc.Open(fileName);

            psarc.Extract(outputDir);
        }

        public static void PsarcSave(string inputDir, string outputFileName)
        {
            Psarc psarc = new();

            Directory.GetFiles(inputDir, "*", SearchOption.AllDirectories).ToList().ForEach(f => {
                string relPath = Path.GetRelativePath(inputDir, f);
                psarc.AddEntry(relPath, f);
            });

            psarc.Save(outputFileName);
        }
    }
}
