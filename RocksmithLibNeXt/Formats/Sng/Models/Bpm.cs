using System.IO;

namespace RocksmithLibNeXt.Formats.Sng.Models
{
    public class Bpm
    {
        #region Properties

        public short Beat { get; set; }
        public int Mask { get; set; }
        public short Measure { get; set; }

        public int PhraseIteration { get; set; }
        public float Time { get; set; }

        #endregion

        #region Main functions

        public static Bpm Read(BinaryReader r)
        {
            return new() {
                Time = r.ReadSingle(),
                Measure = r.ReadInt16(),
                Beat = r.ReadInt16(),
                PhraseIteration = r.ReadInt32(),
                Mask = r.ReadInt32(),
            };
        }

        public void Write(BinaryWriter w)
        {
            w.Write(Time);
            w.Write(Measure);
            w.Write(Beat);
            w.Write(PhraseIteration);
            w.Write(Mask);
        }

        #endregion Main functions
    }
}