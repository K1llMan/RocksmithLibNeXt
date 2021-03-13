using System.IO;

namespace RocksmithLibNeXt.Formats.Sng.Models
{
    public class Anchor
    {
        #region Fields

        private byte[] padding = new byte[3];

        #endregion Fields

        #region Properties

        public float EndBeatTime { get; set; }
        public byte FretId { get; set; }
        public byte[] Padding
        {
            get => padding;
            set => padding = value;
        }
        public int PhraseIterationId { get; set; }
        public float StartBeatTime { get; set; }
        public float Unk3_FirstNoteTime { get; set; }
        public float Unk4_LastNoteTime { get; set; }
        public int Width { get; set; }

        #endregion Properties

        #region Main functions

        public static Anchor Read(BinaryReader r)
        {
            return new() {
                StartBeatTime = r.ReadSingle(),
                EndBeatTime = r.ReadSingle(),
                Unk3_FirstNoteTime = r.ReadSingle(),
                Unk4_LastNoteTime = r.ReadSingle(),
                FretId = r.ReadByte(),
                Padding = r.ReadBytes(3),
                Width = r.ReadInt32(),
                PhraseIterationId = r.ReadInt32(),
            };
        }

        public void Write(BinaryWriter w)
        {
            w.Write(StartBeatTime);
            w.Write(EndBeatTime);
            w.Write(Unk3_FirstNoteTime);
            w.Write(Unk4_LastNoteTime);
            w.Write(FretId);
            w.Write(Padding);
            w.Write(Width);
            w.Write(PhraseIterationId);
        }

        #endregion Main functions
    }
}