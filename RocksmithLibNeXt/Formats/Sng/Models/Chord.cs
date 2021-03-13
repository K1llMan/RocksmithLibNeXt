using System.IO;

namespace RocksmithLibNeXt.Formats.Sng.Models
{
    public class Chord
    {
        #region Fields

        private byte[] fingers = new byte[6];
        private byte[] frets = new byte[6];
        private byte[] name = new byte[32];
        private int[] notes = new int[6];

        #endregion Fields

        #region Properties

        public uint Mask { get; set; }
        public byte[] Fingers
        {
            get => fingers;
            set => fingers = value;
        }

        public byte[] Frets
        {
            get => frets;
            set => frets = value;
        }

        public byte[] Name
        {
            get => name;
            set => name = value;
        }

        public int[] Notes
        {
            get => notes;
            set => notes = value;
        }

        #endregion

        #region Main functions

        public static Chord Read(BinaryReader r)
        {
            Chord c = new() {
                Mask = r.ReadUInt32(),
                Frets = r.ReadBytes(6),
                Fingers = r.ReadBytes(6)
            };

            c.Notes = new int[6];
            for (int i = 0; i < 6; i++)
                c.Notes[i] = r.ReadInt32();
            c.Name = r.ReadBytes(32);

            return c;
        }

        public void Write(BinaryWriter w)
        {
            w.Write(Mask);
            w.Write(Frets);
            w.Write(Fingers);

            foreach (int note in Notes)
                w.Write(note);

            w.Write(Name);
        }

        #endregion Main functions
    }
}