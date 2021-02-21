using System.IO;

namespace RocksmithLibNeXt.Formats.Sng.Models
{
    public class Rect
    {
        #region Properties

        public float XMax { get; set; }
        public float XMin { get; set; }
        public float YMax { get; set; }
        public float YMin { get; set; }

        #endregion Properties

        #region Main functions

        public static Rect Read(BinaryReader r)
        {
            return new() {
                YMin = r.ReadSingle(),
                XMin = r.ReadSingle(),
                YMax = r.ReadSingle(),
                XMax = r.ReadSingle()
            };
        }

        #endregion Main functions
    }
}