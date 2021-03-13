using System.IO;

namespace RocksmithLibNeXt.Formats.Sng.Models
{
    public class NLinkedDifficulty
    {
        #region Properties

        public int LevelBreak { get; set; }
        public int[] NLDPhrase { get; set; }

        public int PhraseCount { get; set; }

        #endregion Properties

        #region Main functions

        public static NLinkedDifficulty Read(BinaryReader r)
        {
            NLinkedDifficulty n = new() {
                LevelBreak = r.ReadInt32(),
                PhraseCount = r.ReadInt32()
            };

            n.NLDPhrase = new int[n.PhraseCount];
            for (int i = 0; i < n.PhraseCount; i++)
                n.NLDPhrase[i] = r.ReadInt32();
            
            return n;
        }

        public void Write(BinaryWriter w)
        {
            w.Write(LevelBreak);
            w.Write(PhraseCount);

            foreach (int phrase in NLDPhrase)
                w.Write(phrase);
        }

        #endregion Main functions
    }
}