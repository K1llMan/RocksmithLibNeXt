using System.IO;

namespace RocksmithLibNeXt.Formats.Sng.Models
{
    public class ChordNotes
    {
        #region Fields

        private BendData[] bend = new BendData[6];
        private uint[] noteMask = new uint[6];
        private byte[] slideTo = new byte[6];
        private byte[] slideUnpitchTo = new byte[6];
        private short[] vibrato = new short[6];

        #endregion Fields

        #region Properties

        public BendData[] Bend
        {
            get => bend;
            set => bend = value;
        }

        public uint[] NoteMask
        {
            get => noteMask;
            set => noteMask = value;
        }

        public byte[] SlideTo
        {
            get => slideTo;
            set => slideTo = value;
        }

        public byte[] SlideUnpitchTo
        {
            get => slideUnpitchTo;
            set => slideUnpitchTo = value;
        }

        public short[] Vibrato
        {
            get => vibrato;
            set => vibrato = value;
        }

        #endregion Properties

        #region Main functions

        public static ChordNotes Read(BinaryReader r)
        {
            ChordNotes cn = new() {
                NoteMask = new uint[6],
                Bend = new BendData[6],
                Vibrato = new short[6]
            };

            for (int i = 0; i < 6; i++)
                cn.NoteMask[i] = r.ReadUInt32();

            for (int i = 0; i < 6; i++)
                cn.Bend[i] = BendData.Read(r);

            cn.SlideTo = r.ReadBytes(6);
            cn.SlideUnpitchTo = r.ReadBytes(6);

            for (int i = 0; i < 6; i++)
                cn.Vibrato[i] = r.ReadInt16();

            return cn;
        }

        #endregion Main functions
    }
}