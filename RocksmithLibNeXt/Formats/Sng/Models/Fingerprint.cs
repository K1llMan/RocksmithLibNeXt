using System.IO;

namespace RocksmithLibNeXt.Formats.Sng.Models
{
    public class Fingerprint
    {
        #region Properties

        public int ChordId { get; set; }
        public float EndTime { get; set; }
        public float StartTime { get; set; }
        public float Unk3_FirstNoteTime { get; set; }
        public float Unk4_LastNoteTime { get; set; }

        #endregion Properties

        #region Main functions

        public static Fingerprint Read(BinaryReader r)
        {
            return new() {
                ChordId = r.ReadInt32(),
                StartTime = r.ReadSingle(),
                EndTime = r.ReadSingle(),
                Unk3_FirstNoteTime = r.ReadSingle(),
                Unk4_LastNoteTime = r.ReadSingle()
            };
        }

        #endregion Main functions
    }
}