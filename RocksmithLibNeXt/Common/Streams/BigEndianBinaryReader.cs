using System;
using System.IO;
using System.Text;

namespace RocksmithLibNeXt.Common.Streams
{
    public class BigEndianBinaryReader : BinaryReader
    {
        #region Поля

        public override int ReadInt32()
        {
            byte[] data = base.ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToInt32(data, 0);
        }

        public override short ReadInt16()
        {
            byte[] data = base.ReadBytes(2);
            Array.Reverse(data);
            return BitConverter.ToInt16(data, 0);
        }

        public override long ReadInt64()
        {
            byte[] data = base.ReadBytes(8);
            Array.Reverse(data);
            return BitConverter.ToInt64(data, 0);
        }

        public override uint ReadUInt32()
        {
            byte[] data = base.ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToUInt32(data, 0);
        }


        public override int PeekChar()
        {
            int result = BaseStream.ReadByte();
            BaseStream.Seek(-1L, SeekOrigin.Current);
            return result;
        }

        public override int Read()
        {
            return BaseStream.ReadByte();
        }

        public override int Read(byte[] buffer, int index, int count)
        {
            return BaseStream.Read(buffer, index, count);
        }

        public override bool ReadBoolean()
        {
            return BaseStream.ReadByte() != 0;
        }

        public override byte ReadByte()
        {
            return (byte) BaseStream.ReadByte();
        }

        public override byte[] ReadBytes(int count)
        {
            byte[] array = new byte[count];
            Read(array, 0, count);
            return array;
        }

        public override char ReadChar()
        {
            return (char) ReadByte();
        }

        public override double ReadDouble()
        {
            byte[] data = new byte[8];
            for (int i = 0; i < 8; i++)
                data[7 - i] = ReadByte();
            return BitConverter.ToDouble(data, 0);
        }

        public override sbyte ReadSByte()
        {
            return (sbyte) ReadByte();
        }

        public override float ReadSingle()
        {
            byte[] data = new byte[4];
            for (int i = 0; i < 4; i++)
                data[3 - i] = ReadByte();
            return BitConverter.ToSingle(data, 0);
        }

        public override ushort ReadUInt16()
        {
            byte[] data = base.ReadBytes(2);
            Array.Reverse(data);
            return BitConverter.ToUInt16(data, 0);
        }

        public uint ReadUInt24()
        {
            byte[] data = new byte[4];
            for (int i = 0; i < 3; i++)
                data[2 - i] = ReadByte();
            return BitConverter.ToUInt32(data, 0);
        }

        public ulong ReadUInt40()
        {
            byte[] data = new byte[8];
            for (int i = 0; i < 5; i++)
                data[4 - i] = ReadByte();
            return BitConverter.ToUInt64(data, 0);
        }

        public override ulong ReadUInt64()
        {
            byte[] data = base.ReadBytes(8);
            Array.Reverse(data);
            return BitConverter.ToUInt64(data, 0);
        }

        #endregion

        #region Constructors

        public BigEndianBinaryReader(Stream input) : base(input)
        {
        }

        public BigEndianBinaryReader(Stream input, Encoding encoding) : base(input, encoding)
        {
        }

        public BigEndianBinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
        {
        }

        #endregion Constructors
    }
}