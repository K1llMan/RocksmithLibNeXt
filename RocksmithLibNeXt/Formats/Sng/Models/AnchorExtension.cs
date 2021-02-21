using System.IO;

namespace RocksmithLibNeXt.Formats.Sng.Models
{
    public class AnchorExtension
    {
        #region Properties

        public float BeatTime { get; set; }
        public byte FretId { get; set; }

        public int Unk2_0 { get; set; }
        public short Unk3_0 { get; set; }
        public byte Unk4_0 { get; set; }

        #endregion Properties

        #region Main functions

        public static AnchorExtension Read(BinaryReader r)
        {
            return new() {
                BeatTime = r.ReadSingle(),
                FretId = r.ReadByte(),
                Unk2_0 = r.ReadInt32(),
                Unk3_0 = r.ReadInt16(),
                Unk4_0 = r.ReadByte()
            };
        }

        #endregion Main functions
    }
}