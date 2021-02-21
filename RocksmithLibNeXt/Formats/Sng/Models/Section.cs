using System.IO;

namespace RocksmithLibNeXt.Formats.Sng.Models
{
    public class Section
    {
        #region Fields

        private byte[] name = new byte[32];
        private byte[] stringMask = new byte[36];

        #endregion Fields

        #region Properties

        public int EndPhraseIterationId { get; set; }
        public float EndTime { get; set; }
        public int Number { get; set; }
        public byte[] Name
        {
            get => name;
            set => name = value;
        }

        public byte[] StringMask
        {
            get => stringMask;
            set => stringMask = value;
        }
        public int StartPhraseIterationId { get; set; }
        public float StartTime { get; set; }

        #endregion Properties

        #region Main functions

        public static Section Read(BinaryReader r)
        {
            return new() {
                Name = r.ReadBytes(32),
                Number = r.ReadInt32(),
                StartTime = r.ReadSingle(),
                EndTime = r.ReadSingle(),
                StartPhraseIterationId = r.ReadInt32(),
                EndPhraseIterationId = r.ReadInt32(),
                StringMask = r.ReadBytes(36)
            };
        }

        #endregion Main functions
    }
}