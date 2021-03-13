using System.IO;

namespace RocksmithLibNeXt.Formats.Sng.Models
{
    public class Phrase
    {
        #region Fields

        private byte[] name = new byte[32];

        #endregion Fields

        #region Properties

        public byte Disparity { get; set; }
        public byte Ignore { get; set; }
        public int MaxDifficulty { get; set; }
        public byte Padding { get; set; }
        public int PhraseIterationLinks { get; set; }
        public byte Solo { get; set; }
        public byte[] Name
        {
            get => name;
            set => name = value;
        }

        #endregion Properties

        #region Main functions

        public static Phrase Read(BinaryReader r)
        {
            return new() {
                Solo = r.ReadByte(),
                Disparity = r.ReadByte(),
                Ignore = r.ReadByte(),
                Padding = r.ReadByte(),
                MaxDifficulty = r.ReadInt32(),
                PhraseIterationLinks = r.ReadInt32(),
                Name = r.ReadBytes(32),
            };
        }

        public void Write(BinaryWriter w)
        {
            w.Write(Solo);
            w.Write(Disparity);
            w.Write(Ignore);
            w.Write(Padding);
            w.Write(MaxDifficulty);
            w.Write(PhraseIterationLinks);
            w.Write(Name);
        }

        #endregion Main functions
    }
}