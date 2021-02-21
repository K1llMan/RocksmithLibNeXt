using System.IO;

namespace RocksmithLibNeXt.Formats.Sng.Models
{
    public class Arrangement
    {
        #region Properties

        public AnchorExtensionCollection AnchorExtensions { get; set; }
        public AnchorCollection Anchors { get; set; }
        public float[] AverageNotesPerIteration { get; set; }
        public int Difficulty { get; set; }
        public FingerprintCollection Fingerprints1 { get; set; }
        public FingerprintCollection Fingerprints2 { get; set; }
        public NotesCollection Notes { get; set; }
        public int[] NotesInIteration1 { get; set; }
        public int[] NotesInIteration2 { get; set; }

        public int PhraseCount { get; set; }
        public int PhraseIterationCount1 { get; set; }
        public int PhraseIterationCount2 { get; set; }

        #endregion Properties

        #region Main functions

        public static Arrangement Read(BinaryReader r)
        {
            Arrangement a = new() {
                Difficulty = r.ReadInt32(),
                Anchors = AnchorCollection.Read(r),
                AnchorExtensions = AnchorExtensionCollection.Read(r),
                Fingerprints1 = FingerprintCollection.Read(r),
                Fingerprints2 = FingerprintCollection.Read(r),
                Notes = NotesCollection.Read(r),
                PhraseCount = r.ReadInt32()
            };

            a.AverageNotesPerIteration = new float[a.PhraseCount];
            for (int i = 0; i < a.PhraseCount; i++) 
                a.AverageNotesPerIteration[i] = r.ReadSingle();

            a.PhraseIterationCount1 = r.ReadInt32();
            a.NotesInIteration1 = new int[a.PhraseIterationCount1];
            for (int i = 0; i < a.PhraseIterationCount1; i++)
                a.NotesInIteration1[i] = r.ReadInt32();

            a.PhraseIterationCount2 = r.ReadInt32();
            a.NotesInIteration2 = new int[a.PhraseIterationCount2];
            for (int i = 0; i < a.PhraseIterationCount2; i++)
                a.NotesInIteration2[i] = r.ReadInt32();

            return a;
        }

        #endregion Main functions
    }
}