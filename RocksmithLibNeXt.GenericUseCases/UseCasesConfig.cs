using System.IO;
using System.Reflection;

using RocksmithLibNeXt.Formats.Psarc;

namespace RocksmithLibNeXt.GenericUseCases
{
    public class UseCasesConfig
    {
        #region Properties

        public string LocalDir { get; set; }

        public string InputPsarc { get; set; }

        public string OutputPsarc{ get; set; }

        public string InputSng { get; set; }

        public string InputSngStream { get; set; }

        public string TempDir { get; set; }

        public Psarc Psarc { get; set; }

        #endregion Properties

        #region Auxiliary functions

        private void InitProperties()
        {
            LocalDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestData");

            InputPsarc = Path.Combine(LocalDir, "test.psarc");
            OutputPsarc = Path.Combine(LocalDir, "testOut.psarc");
            InputSng = Path.Combine(LocalDir, "test.sng");
            InputSngStream = Path.Combine(LocalDir, "sng_unpacked.stream");
            TempDir = Path.Combine(LocalDir, "temp");
        }

        #endregion Auxiliary functions

        #region Main functions

        public UseCasesConfig()
        {
            InitProperties();
        }

        #endregion Main functions
    }
}