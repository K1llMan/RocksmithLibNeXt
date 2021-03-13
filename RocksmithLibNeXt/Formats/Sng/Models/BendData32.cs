using System.IO;

namespace RocksmithLibNeXt.Formats.Sng.Models
{
    public class BendData32
    {
        #region Properties

        public float Step { get; set; }
        public float Time { get; set; }
        public short Unk3_0 { get; set; }
        public byte Unk4_0 { get; set; }
        public byte Unk5 { get; set; }

        #endregion Properties

        #region Main functions

        public static BendData32 Read(BinaryReader r)
        {
            return new() {
                Time = r.ReadSingle(),
                Step = r.ReadSingle(),
                Unk3_0 = r.ReadInt16(),
                Unk4_0 = r.ReadByte(),
                Unk5 = r.ReadByte()
            };
        }

        public void Write(BinaryWriter w)
        {
            w.Write(Time);
            w.Write(Step);
            w.Write(Unk3_0);
            w.Write(Unk4_0);
            w.Write(Unk5);
        }

        #endregion Main functions
    }
}