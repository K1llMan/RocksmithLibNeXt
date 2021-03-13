using System.IO;

namespace RocksmithLibNeXt.Formats.Sng.Models
{
    public class BendData
    {
        #region Fields

        private BendData32[] bend32 = new BendData32[32];

        #endregion Fields

        #region Properties

        public BendData32[] Bend32
        {
            get => bend32;
            set => bend32 = value;
        }

        public int UsedCount { get; set; }

        #endregion Properties

        #region Main functions

        public static BendData Read(BinaryReader r)
        {
            BendData32[] bends = new BendData32[32];
            for (int i = 0; i < 32; i++)
                bends[i] = BendData32.Read(r);

            return new() {
                Bend32 = bends,
                UsedCount = r.ReadInt32()
            };
        }

        public void Write(BinaryWriter w)
        {
            foreach (BendData32 bendData32 in Bend32) 
                bendData32.Write(w);

            w.Write(UsedCount);
        }

        #endregion Main functions
    }
}