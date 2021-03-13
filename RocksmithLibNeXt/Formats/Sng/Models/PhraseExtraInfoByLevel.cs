using System.IO;

namespace RocksmithLibNeXt.Formats.Sng.Models
{
    public class PhraseExtraInfoByLevel
    {
        #region Properties

        public int Difficulty { get; set; }
        public int Empty { get; set; }
        public byte LevelJump { get; set; }
        public byte Padding { get; set; }
        public int PhraseId { get; set; }
        public short Redundant { get; set; }

        #endregion Properties

        #region Main functions

        public static PhraseExtraInfoByLevel Read(BinaryReader r)
        {
            return new() {
                PhraseId = r.ReadInt32(),
                Difficulty = r.ReadInt32(),
                Empty = r.ReadInt32(),
                LevelJump = r.ReadByte(),
                Redundant = r.ReadInt16(),
                Padding = r.ReadByte()
            };
        }

        public void Write(BinaryWriter w)
        {
            w.Write(PhraseId);
            w.Write(Difficulty);
            w.Write(Empty);
            w.Write(LevelJump);
            w.Write(Redundant);
            w.Write(Padding);
        }

        #endregion Main functions
    }
}