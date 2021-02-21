using System.IO;

namespace RocksmithLibNeXt.Formats.Sng.Models
{
    public class PhraseIteration
    {
        #region Fields

        private int[] difficulty = new int[3];

        #endregion Fields

        #region Properties

        public int[] Difficulty
        {
            get => difficulty;
            set => difficulty = value;
        }
        public float NextPhraseTime { get; set; }
        public int PhraseId { get; set; }
        public float StartTime { get; set; }

        #endregion Properties

        #region Main functions

        public static PhraseIteration Read(BinaryReader r)
        {
            PhraseIteration pi = new() {
                PhraseId = r.ReadInt32(),
                StartTime = r.ReadSingle(),
                NextPhraseTime = r.ReadSingle(),
                Difficulty = new int[3]
            };

            for (int i = 0; i < 3; i++)
                pi.Difficulty[i] = r.ReadInt32();

            return pi;
        }

        #endregion Main functions
    }
}