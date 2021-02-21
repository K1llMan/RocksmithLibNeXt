using System.IO;

namespace RocksmithLibNeXt.Formats.Sng.Models
{
    public class Vocal
    {
        #region Fields

        private byte[] lyric = new byte[48];

        #endregion Fields

        #region Properties

        public float Length { get; set; }

        public byte[] Lyric
        {
            get => lyric;
            set => lyric = value;
        }

        public int Note { get; set; }

        public float Time { get; set; }

        #endregion Properties

        #region Main functions

        public static Vocal Read(BinaryReader r)
        {
            return new() {
                Time = r.ReadSingle(),
                Note = r.ReadInt32(),
                Length = r.ReadSingle(),
                Lyric = r.ReadBytes(48)
            };
        }

        #endregion Main functions
    }
}