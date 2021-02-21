using System.IO;

namespace RocksmithLibNeXt.Formats.Sng.Models
{
    public class SymbolsTexture
    {
        #region Fields

        private byte[] font = new byte[128];

        #endregion Fields

        #region Properties

        public byte[] Font
        {
            get => font;
            set => font = value;
        }

        public int FontpathLength { get; set; }
        public int Height { get; set; }

        public int Unk1_0 { get; set; }
        public int Width { get; set; }

        #endregion Properties

        #region Main functions

        public static SymbolsTexture Read(BinaryReader r)
        {
            return new() {
                Font = r.ReadBytes(128),
                FontpathLength = r.ReadInt32(),
                Unk1_0 = r.ReadInt32(),
                Width = r.ReadInt32(),
                Height = r.ReadInt32()
            };
        }

        #endregion Main functions
    }
}