using System.IO;

using RocksmithLibNeXt.Formats.Sng.Common;

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
        public SngCollection<BendData> BendData { get; set; }
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
            n.BendData = SngCollection<BendData>.Read(r);

            return n;
        }

        public void Write(BinaryWriter w)
        {
            w.Write(NoteMask);
            w.Write(NoteFlags);
            w.Write(Hash);
            w.Write(Time);
            w.Write(StringIndex);
            w.Write(FretId);
            w.Write(AnchorFretId);
            w.Write(AnchorWidth);
            w.Write(ChordId);
            w.Write(ChordNotesId);
            w.Write(PhraseId);
            w.Write(PhraseIterationId);

            foreach (short id in FingerPrintId)
                w.Write(id);

            w.Write(NextIterNote);
            w.Write(PrevIterNote);
            w.Write(ParentPrevNote);
            w.Write(SlideTo);
            w.Write(SlideUnpitchTo);
            w.Write(LeftHand);
            w.Write(Tap);
            w.Write(PickDirection);
            w.Write(Slap);
            w.Write(Pluck);
            w.Write(Vibrato);
            w.Write(Sustain);
            w.Write(MaxBend);
            BendData.Write(w);
        }

        #endregion Main functions
    }
}