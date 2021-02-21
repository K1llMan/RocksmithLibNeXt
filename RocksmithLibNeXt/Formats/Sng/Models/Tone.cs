using System.IO;

namespace RocksmithLibNeXt.Formats.Sng.Models
{
    public class Tone
    {
        #region Properties

        public float Time { get; set; }
        public int ToneId { get; set; }

        #endregion Properties

        #region Main functions

        public static Tone Read(BinaryReader r)
        {
            return new() {
                Time = r.ReadSingle(),
                ToneId = r.ReadInt32()
            };
        }

        #endregion Main functions
    }
}