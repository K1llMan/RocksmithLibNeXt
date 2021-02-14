using System;
using System.IO;

namespace RocksmithLibNeXt.Common.Streams
{
    public class BigEndianBinaryWriter : BinaryWriter
    {
        #region Main functions

        public override void Write(byte v)
        {
            //try
            //{
            BaseStream.WriteByte(v);
            //}
            //catch (Exception ex)
            //{
            //    // little fish
            //    Console.WriteLine("<ERROR> Little Fish: " + ex.Message);
            //    this.Flush();
            //    Thread.Sleep(200);
            //}
        }

        public override void Write(short v)
        {
            byte[] bytes = BitConverter.GetBytes(v);
            for (int i = 0; i < bytes.Length; i++) 
                Write(bytes[bytes.Length - i - 1]);
        }

        public override void Write(int v)
        {
            byte[] bytes = BitConverter.GetBytes(v);
            for (int i = 0; i < bytes.Length; i++) 
                Write(bytes[bytes.Length - i - 1]);
        }

        public override void Write(long v)
        {
            byte[] bytes = BitConverter.GetBytes(v);
            for (int i = 0; i < bytes.Length; i++) 
                Write(bytes[bytes.Length - i - 1]);
        }

        public override void Write(ushort v)
        {
            byte[] bytes = BitConverter.GetBytes(v);
            for (int i = 0; i < bytes.Length; i++) 
                Write(bytes[bytes.Length - i - 1]);
        }

        public override void Write(uint v)
        {
            byte[] bytes = BitConverter.GetBytes(v);
            for (int i = 0; i < bytes.Length; i++) 
                Write(bytes[bytes.Length - i - 1]);
        }

        public override void Write(ulong v)
        {
            byte[] bytes = BitConverter.GetBytes(v);
            for (int i = 0; i < bytes.Length; i++) 
                Write(bytes[bytes.Length - i - 1]);
        }

        public override void Write(float v)
        {
            byte[] bytes = BitConverter.GetBytes(v);
            for (int i = 0; i < bytes.Length; i++) 
                Write(bytes[bytes.Length - i - 1]);
        }

        public override void Write(double v)
        {
            byte[] bytes = BitConverter.GetBytes(v);
            for (int i = 0; i < bytes.Length; i++) 
                Write(bytes[bytes.Length - i - 1]);
        }

        public void WriteUInt24(uint v)
        {
            byte[] bytes = BitConverter.GetBytes(v);
            for (int i = 1; i < bytes.Length; i++) 
                Write(bytes[bytes.Length - i - 1]);
        }

        public void WriteUInt40(ulong v)
        {
            byte[] bytes = BitConverter.GetBytes(v);
            for (int i = 3; i < bytes.Length; i++) 
                Write(bytes[bytes.Length - i - 1]);
        }

        public override void Write(byte[] val)
        {
            //try
            //{
            foreach (byte v in val) {
                Write(v);
            }

            //}
            //catch (Exception ex)
            //{
            //    // big fish
            //    Console.WriteLine("<ERROR> Big Fish: " + ex.Message);
            //    this.Flush();
            //    Thread.Sleep(200);
            //}
        }

        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[32768];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0) output.Write(buffer, 0, read);
        }

        #endregion Main functions

        #region Constructors

        public BigEndianBinaryWriter(Stream input) : base(input)
        {
        }

        #endregion Constructors
    }
}