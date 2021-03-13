using System.IO;

namespace RocksmithLibNeXt.Formats.Sng.Models
{
    public class SymbolDefinition
    {
        #region Fields

        private byte[] text = new byte[12];

        #endregion Fields

        #region Properties

        public Rect RectInner { get; set; }
        public Rect RectOuter { get; set; }
        public byte[] Text
        {
            get => text;
            set => text = value;
        }

        #endregion Properties

        #region Main functions

        public static SymbolDefinition Read(BinaryReader r)
        {
            return new() {
                Text = r.ReadBytes(12),
                RectOuter = Rect.Read(r),
                RectInner = Rect.Read(r),
            };
        }

        public void Write(BinaryWriter w)
        {
            w.Write(Text);
            RectOuter.Write(w);
            RectInner.Write(w);
        }

        #endregion Main functions
    }
}