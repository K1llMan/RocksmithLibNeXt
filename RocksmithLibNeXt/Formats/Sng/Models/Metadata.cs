using System.IO;

namespace RocksmithLibNeXt.Formats.Sng.Models
{
    public class Metadata
    {
        #region Fields

        private byte[] lastConversionDateTime = new byte[32];

        #endregion Fields

        #region Properties

        public byte CapoFretId { get; set; }
        public float FirstBeatLength { get; set; }
        public byte[] LastConversionDateTime
        {
            get => lastConversionDateTime;
            set => lastConversionDateTime = value;
        }
        public int MaxDifficulty { get; set; }
        public double MaxNotesAndChords { get; set; }
        public double MaxNotesAndChordsReal { get; set; }
        public double MaxScore { get; set; }

        public short Part { get; set; }
        public double PointsPerNote { get; set; }
        public float SongLength { get; set; }
        public float StartTime { get; set; }
        public int StringCount { get; set; }
        public short[] Tuning { get; set; }
        public float Unk11_FirstNoteTime { get; set; }
        public float Unk12_FirstNoteTime { get; set; }

        #endregion Properties

        #region Main functions

        public static Metadata Read(BinaryReader r)
        {
            Metadata m = new() {
                MaxScore = r.ReadDouble(),
                MaxNotesAndChords = r.ReadDouble(),
                MaxNotesAndChordsReal = r.ReadDouble(),
                PointsPerNote = r.ReadDouble(),
                FirstBeatLength = r.ReadSingle(),
                StartTime = r.ReadSingle(),
                CapoFretId = r.ReadByte(),
                LastConversionDateTime = r.ReadBytes(32),
                Part = r.ReadInt16(),
                SongLength = r.ReadSingle(),
                StringCount = r.ReadInt32()
            };

            m.Tuning = new short[m.StringCount];
            for (int i = 0; i < m.StringCount; i++) 
                m.Tuning[i] = r.ReadInt16();
            m.Unk11_FirstNoteTime = r.ReadSingle();
            m.Unk12_FirstNoteTime = r.ReadSingle();
            m.MaxDifficulty = r.ReadInt32();

            return m;
        }

        public void Write(BinaryWriter w)
        {
            w.Write(MaxScore);
            w.Write(MaxNotesAndChords);
            w.Write(MaxNotesAndChordsReal);
            w.Write(PointsPerNote);
            w.Write(FirstBeatLength);
            w.Write(StartTime);
            w.Write(CapoFretId);
            w.Write(LastConversionDateTime);
            w.Write(Part);
            w.Write(SongLength);
            w.Write(StringCount);

            foreach (short s in Tuning)
                w.Write(s);

            w.Write(Unk11_FirstNoteTime);
            w.Write(Unk12_FirstNoteTime);
            w.Write(MaxDifficulty);
        }

        #endregion Main functions
    }
}