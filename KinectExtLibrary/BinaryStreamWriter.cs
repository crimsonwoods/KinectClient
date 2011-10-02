using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace crimsonwoods.windows.library.KinectExtLibrary
{
    public class BinaryStreamWriter : IDisposable
    {
        public enum ByteOrder
        {
            Little,
            Big,
        }

        private static ByteOrder defaultByteOrder = GetBitConverterEndian();

        public static ByteOrder DefaultByteOrder
        {
            get
            {
                return defaultByteOrder;
            }
            set
            {
                defaultByteOrder = value;
            }
        }

        public BinaryStreamWriter(Stream s)
            : this(s, DefaultByteOrder)
        {
        }

        public BinaryStreamWriter(Stream s, ByteOrder outputEndian)
        {
            if (null == s)
            {
                throw new ArgumentNullException();
            }

            OutputEndian = outputEndian;
            Stream = s;
        }

        public void Close()
        {
            Stream.Close();
        }

        public ByteOrder OutputEndian
        {
            get;
            private set;
        }

        public Stream Stream
        {
            get;
            private set;
        }

        public void Write(byte[] value, int offset, int count)
        {
            if (0 > count)
            {
                throw new ArgumentException();
            }

            if ((offset + count) > value.Length){
                throw new IndexOutOfRangeException();
            }

            Stream.Write(value, offset, count);
        }

        public void Write(byte[] value)
        {
            Write(value, 0, value.Length);
        }

        public void Write(int value)
        {
            Write(GetOrderedBytes(BitConverter.GetBytes(value)));
        }

        public void Write(uint value)
        {
            Write(GetOrderedBytes(BitConverter.GetBytes(value)));
        }

        public void Write(long value)
        {
            Write(GetOrderedBytes(BitConverter.GetBytes(value)));
        }

        public void Write(ulong value)
        {
            Write(GetOrderedBytes(BitConverter.GetBytes(value)));
        }

        public void Write(float value)
        {
            Write(GetOrderedBytes(BitConverter.GetBytes(value)));
        }

        public void Write(double value)
        {
            Write(GetOrderedBytes(BitConverter.GetBytes(value)));
        }

        public void Write(String value)
        {
            Write(Encoding.UTF8.GetBytes(value));
        }

        private byte[] GetOrderedBytes(byte[] value)
        {
            if (GetBitConverterEndian() != OutputEndian)
            {
                return value.Reverse().ToArray();
            }
            return value;
        }

        private static ByteOrder GetBitConverterEndian()
        {
            return BitConverter.IsLittleEndian ? ByteOrder.Little : ByteOrder.Big;
        }

        public void Dispose()
        {
            Close();
            Stream.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
