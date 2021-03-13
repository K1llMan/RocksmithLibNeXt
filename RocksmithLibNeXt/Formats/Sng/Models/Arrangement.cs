using System.IO;

using RocksmithLibNeXt.Formats.Sng.Common;

namespace RocksmithLibNeXt.Formats.Sng.Models
{
    public class Arrangement
    {
        #region Properties

        public SngCollection<AnchorExtension> AnchorExtensions { get; set; }
        public SngCollection<Anchor> Anchors { get; set; }
        public float[] AverageNotesPerIteration { get; set; }
        public int Difficulty { get; set; }
        public SngCollection<Fingerprint> Fingerprints1 { get; set; }
        public SngCollection<Fingerprint> Fingerprints2 { get; set; }
        public SngCollection<Notes> Notes { get; set; }
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
                Anchors = SngCollection<Anchor>.Read(r),
                AnchorExtensions = SngCollection<AnchorExtension>.Read(r),
                Fingerprints1 = SngCollection<Fingerprint>.Read(r),
                Fingerprints2 = SngCollection<Fingerprint>.Read(r),
                Notes = SngCollection<Notes>.Read(r),
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

        public void Write(BinaryWriter w)
        {
            w.Write(Difficulty);
            Anchors.Write(w);
            AnchorExtensions.Write(w);
            Fingerprints1.Write(w);
            Fingerprints2.Write(w);
            Notes.Write(w);
            
            w.Write(PhraseCount);
            foreach (float notes in AverageNotesPerIteration)
                w.Write(notes);
            
            w.Write(PhraseIterationCount1);
            foreach (int notes in NotesInIteration1)
                w.Write(notes);
            
            w.Write(PhraseIterationCount2);
            foreach (int notes in NotesInIteration2)
                w.Write(notes);
        }

        #endregion Main functions
    }
}