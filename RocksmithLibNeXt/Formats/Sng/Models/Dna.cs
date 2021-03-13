using System.IO;

namespace RocksmithLibNeXt.Formats.Sng.Models
{
    public class Dna
    {
        #region Properties

        public int DnaId { get; set; }

        public float Time { get; set; }

        #endregion Properties

        #region Main functions

        public static Dna Read(BinaryReader r)
        {
            return new() {
                Time = r.ReadSingle(),
                DnaId = r.ReadInt32()
            };
        }

        public void Write(BinaryWriter w)
        {
            w.Write(Time);
            w.Write(DnaId);
        }

        #endregion Main functions
    }
}