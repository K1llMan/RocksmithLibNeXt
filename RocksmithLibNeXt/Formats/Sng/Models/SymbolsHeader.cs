using System.IO;

namespace RocksmithLibNeXt.Formats.Sng.Models
{
    public class SymbolsHeader
    {
        #region Properties

        public int Unk1 { get; set; }
        public int Unk2 { get; set; }
        public int Unk3 { get; set; }
        public int Unk4 { get; set; }
        public int Unk5 { get; set; }
        public int Unk6 { get; set; }
        public int Unk7 { get; set; }
        public int Unk8 { get; set; }

        #endregion Properties

        #region Main functions

        public static SymbolsHeader Read(BinaryReader r)
        {
            return new() {
                Unk1 = r.ReadInt32(),
                Unk2 = r.ReadInt32(),
                Unk3 = r.ReadInt32(),
                Unk4 = r.ReadInt32(),
                Unk5 = r.ReadInt32(),
                Unk6 = r.ReadInt32(),
                Unk7 = r.ReadInt32(),
                Unk8 = r.ReadInt32()
            };
        }

        #endregion Main functions
    }
}