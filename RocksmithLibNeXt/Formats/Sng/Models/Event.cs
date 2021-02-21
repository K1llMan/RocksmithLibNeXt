using System.IO;

namespace RocksmithLibNeXt.Formats.Sng.Models
{
    public class Event
    {
        #region Fields

        private byte[] eventName = new byte[256];

        #endregion Fields

        #region Properties

        public byte[] EventName
        {
            get => eventName;
            set => eventName = value;
        }
        public float Time { get; set; }

        #endregion Properties

        #region Main functions

        public static Event Read(BinaryReader r)
        {
            return new() {
                Time = r.ReadSingle(),
                EventName = r.ReadBytes(256)
            };
        }

        #endregion Main functions
    }
}