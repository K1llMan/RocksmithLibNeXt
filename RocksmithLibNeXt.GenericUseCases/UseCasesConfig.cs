using System.IO;
using System.Reflection;
using System.Security.Cryptography;

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

        public string OutputSngStream { get; set; }

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
            OutputSngStream = Path.Combine(LocalDir, "sng_unpacked_output.stream");
            TempDir = Path.Combine(LocalDir, "temp");
        }

        #endregion Auxiliary functions

        #region Main functions

        public UseCasesConfig()
        {
            InitProperties();
        }

        public byte[] CalculateHash(Stream s)
        {
            return new MD5CryptoServiceProvider().ComputeHash(s);
        }

        public bool CompareStreams(Stream s1, Stream s2)
        {
            byte[] hashS1 = new MD5CryptoServiceProvider().ComputeHash(s1);
            byte[] hashS2 = new MD5CryptoServiceProvider().ComputeHash(s2);

            if (hashS1.Length != hashS2.Length)
                return false;

            for (int i = 0; i < hashS1.Length; i++)
                if (hashS1[i] != hashS2[i])
                    return false;

            return true;
        }

        #endregion Main functions
    }
}