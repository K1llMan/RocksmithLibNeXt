using System.IO;

namespace RocksmithLibNeXt.Formats.Sng.Models
{
    public class Action
    {
        #region Fields

        public byte[] actionName = new byte[256];

        #endregion Fields

        #region Properties

        public byte[] ActionName
        {
            get => actionName;
            set => actionName = value;
        }

        public float Time { get; set; }

        #endregion Properties

        #region Main functions

        public static Action Read(BinaryReader r)
        {
            return new() {
                Time = r.ReadSingle(),
                ActionName = r.ReadBytes(256)
            };
        }

        #endregion Main functions
    }
}