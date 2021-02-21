using System.IO;

namespace RocksmithLibNeXt.Formats.Sng.Models
{
    public class Notes
    {
        #region Fields

        private short[] fingerPrintId = new short[2];

        #endregion Fields

        #region Properties

        public byte AnchorFretId { get; set; }
        public byte AnchorWidth { get; set; }
        public BendDataCollection BendData { get; set; }
        public int ChordId { get; set; }
        public int ChordNotesId { get; set; }
        public byte FretId { get; set; }
        public uint Hash { get; set; }
        public byte LeftHand { get; set; }
        public float MaxBend { get; set; }
        public short NextIterNote { get; set; }
        public uint NoteFlags { get; set; }
        public uint NoteMask { get; set; }
        public short[] FingerPrintId
        {
            get => fingerPrintId;
            set => fingerPrintId = value;
        }
        public short ParentPrevNote { get; set; }
        public int PhraseId { get; set; }
        public int PhraseIterationId { get; set; }
        public byte PickDirection { get; set; }
        public byte Pluck { get; set; }
        public short PrevIterNote { get; set; }
        public byte Slap { get; set; }
        public byte SlideTo { get; set; }
        public byte SlideUnpitchTo { get; set; }
        public byte StringIndex { get; set; }
        public float Sustain { get; set; }
        public byte Tap { get; set; }
        public float Time { get; set; }
        public short Vibrato { get; set; }

        #endregion Properties

        #region Main functions

        public static Notes Read(BinaryReader r)
        {
            Notes n = new() {
                NoteMask = r.ReadUInt32(),
                NoteFlags = r.ReadUInt32(),
                Hash = r.ReadUInt32(),
                Time = r.ReadSingle(),
                StringIndex = r.ReadByte(),
                FretId = r.ReadByte(),
                AnchorFretId = r.ReadByte(),
                AnchorWidth = r.ReadByte(),
                ChordId = r.ReadInt32(),
                ChordNotesId = r.ReadInt32(),
                PhraseId = r.ReadInt32(),
                PhraseIterationId = r.ReadInt32(),
                FingerPrintId = new short[2]
            };

            for (int i = 0; i < 2; i++)
                n.FingerPrintId[i] = r.ReadInt16();

            n.NextIterNote = r.ReadInt16();
            n.PrevIterNote = r.ReadInt16();
            n.ParentPrevNote = r.ReadInt16();
            n.SlideTo = r.ReadByte();
            n.SlideUnpitchTo = r.ReadByte();
            n.LeftHand = r.ReadByte();
            n.Tap = r.ReadByte();
            n.PickDirection = r.ReadByte();
            n.Slap = r.ReadByte();
            n.Pluck = r.ReadByte();
            n.Vibrato = r.ReadInt16();
            n.Sustain = r.ReadSingle();
            n.MaxBend = r.ReadSingle();
            n.BendData = BendDataCollection.Read(r);

            return n;
        }

        #endregion Main functions
    }
}