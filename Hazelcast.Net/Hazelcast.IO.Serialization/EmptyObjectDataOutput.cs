using System;

namespace Hazelcast.IO.Serialization
{
    internal sealed class EmptyObjectDataOutput : IObjectDataOutput
    {
        /// <exception cref="System.IO.IOException"></exception>
        public void WriteObject(object @object)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void Write(int b)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void Write(byte[] b)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void Write(byte[] b, int off, int len)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteBoolean(bool v)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteByte(byte v)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteShort(int v)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteChar(int v)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteInt(int v)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteLong(long v)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteFloat(float v)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteDouble(double v)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteBytes(string s)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteChars(string s)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteUTF(string s)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteCharArray(char[] chars)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteIntArray(int[] ints)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteLongArray(long[] longs)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteDoubleArray(double[] values)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteFloatArray(float[] values)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteShortArray(short[] values)
        {
        }

        public byte[] ToByteArray()
        {
            throw new NotSupportedException();
        }

        public bool IsBigEndian()
        {
            return true;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void Close()
        {
        }
    }
}