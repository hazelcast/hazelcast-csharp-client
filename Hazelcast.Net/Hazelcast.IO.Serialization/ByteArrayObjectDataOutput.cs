using System;
using System.IO;
using System.Text;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
    internal class ByteArrayObjectDataOutput : OutputStream, IBufferObjectDataOutput, ISerializationContextAware
    {
        internal readonly int initialSize;
        internal readonly ISerializationService service;

        internal byte[] buffer;

        internal int pos = 0;

        internal ByteArrayObjectDataOutput(int size, ISerializationService service)
        {
            initialSize = size;
            buffer = new byte[size];
            this.service = service;
        }

        void IDataOutput.Write(byte[] b)
        {
            Write(b, 0, b.Length);
        }

        public virtual void Write(int position, int b)
        {
            buffer[position] = unchecked((byte) b);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteBoolean(bool v)
        {
            Write(v ? 1 : 0);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteBoolean(int position, bool v)
        {
            Write(position, v ? 1 : 0);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteByte(byte v)
        {
            Write(v);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteByte(int position, int v)
        {
            Write(position, v);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteBytes(string s)
        {
            int len = s.Length;
            EnsureAvailable(len);
            for (int i = 0; i < len; i++)
            {
                buffer[pos++] = unchecked((byte) s[i]);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteChar(int v)
        {
            EnsureAvailable(2);
            buffer[pos++] = unchecked((byte) (((int) (((uint) v) >> 8)) & unchecked(0xFF)));
            buffer[pos++] = unchecked((byte) ((v) & unchecked(0xFF)));
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteChar(int position, int v)
        {
            Write(position, ((int) (((uint) v) >> 8)) & unchecked(0xFF));
            Write(position, (v) & unchecked(0xFF));
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteChars(string s)
        {
            int len = s.Length;
            EnsureAvailable(len*2);
            for (int i = 0; i < len; i++)
            {
                int v = s[i];
                WriteChar(pos, v);
                pos += 2;
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteDouble(double v)
        {
            WriteLong(BitConverter.DoubleToInt64Bits(v));
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteDouble(int position, double v)
        {
            WriteLong(position, BitConverter.DoubleToInt64Bits(v));
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteFloat(float v)
        {
            WriteInt(BitConverter.ToInt32(BitConverter.GetBytes(v), 0));
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteFloat(int position, float v)
        {
            WriteInt(position, BitConverter.ToInt32(BitConverter.GetBytes(v), 0));
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteInt(int v)
        {
            EnsureAvailable(4);
            buffer[pos++] = unchecked((byte) (((int) (((uint) v) >> 24)) & unchecked(0xFF)));
            buffer[pos++] = unchecked((byte) (((int) (((uint) v) >> 16)) & unchecked(0xFF)));
            buffer[pos++] = unchecked((byte) (((int) (((uint) v) >> 8)) & unchecked(0xFF)));
            buffer[pos++] = unchecked((byte) ((v) & unchecked(0xFF)));
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteInt(int position, int v)
        {
            Write(position, ((int) (((uint) v) >> 24)) & unchecked(0xFF));
            Write(position + 1, ((int) (((uint) v) >> 16)) & unchecked(0xFF));
            Write(position + 2, ((int) (((uint) v) >> 8)) & unchecked(0xFF));
            Write(position + 3, (v) & unchecked(0xFF));
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteLong(long v)
        {
            EnsureAvailable(8);
            buffer[pos++] = unchecked((byte) ((long) (((ulong) v) >> 56)));
            buffer[pos++] = unchecked((byte) ((long) (((ulong) v) >> 48)));
            buffer[pos++] = unchecked((byte) ((long) (((ulong) v) >> 40)));
            buffer[pos++] = unchecked((byte) ((long) (((ulong) v) >> 32)));
            buffer[pos++] = unchecked((byte) ((long) (((ulong) v) >> 24)));
            buffer[pos++] = unchecked((byte) ((long) (((ulong) v) >> 16)));
            buffer[pos++] = unchecked((byte) ((long) (((ulong) v) >> 8)));
            buffer[pos++] = unchecked((byte) (v));
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteLong(int position, long v)
        {
            Write(position, (int) ((long) (((ulong) v) >> 56)));
            Write(position + 1, (int) ((long) (((ulong) v) >> 48)));
            Write(position + 2, (int) ((long) (((ulong) v) >> 40)));
            Write(position + 3, (int) ((long) (((ulong) v) >> 32)));
            Write(position + 4, (int) ((long) (((ulong) v) >> 24)));
            Write(position + 5, (int) ((long) (((ulong) v) >> 16)));
            Write(position + 6, (int) ((long) (((ulong) v) >> 8)));
            Write(position + 7, (int) (v));
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteShort(int v)
        {
            EnsureAvailable(2);
            buffer[pos++] = unchecked((byte) (((int) (((uint) v) >> 8)) & unchecked(0xFF)));
            buffer[pos++] = unchecked((byte) ((v) & unchecked(0xFF)));
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteShort(int position, int v)
        {
            Write(position, ((int) (((uint) v) >> 8)) & unchecked(0xFF));
            Write(position, (v) & unchecked(0xFF));
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteUTF(string str)
        {
            UTFUtil.WriteUTF(this, str);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteCharArray(char[] chars)
        {
            int len = chars != null ? chars.Length : 0;
            WriteInt(len);
            if (len > 0)
            {
                foreach (char c in chars)
                {
                    WriteChar(c);
                }
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteIntArray(int[] ints)
        {
            int len = ints != null ? ints.Length : 0;
            WriteInt(len);
            if (len > 0)
            {
                foreach (int i in ints)
                {
                    WriteInt(i);
                }
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteLongArray(long[] longs)
        {
            int len = longs != null ? longs.Length : 0;
            WriteInt(len);
            if (len > 0)
            {
                foreach (long l in longs)
                {
                    WriteLong(l);
                }
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteDoubleArray(double[] doubles)
        {
            int len = doubles != null ? doubles.Length : 0;
            WriteInt(len);
            if (len > 0)
            {
                foreach (double d in doubles)
                {
                    WriteDouble(d);
                }
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteFloatArray(float[] floats)
        {
            int len = floats != null ? floats.Length : 0;
            WriteInt(len);
            if (len > 0)
            {
                foreach (float f in floats)
                {
                    WriteFloat(f);
                }
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteShortArray(short[] shorts)
        {
            int len = shorts != null ? shorts.Length : 0;
            WriteInt(len);
            if (len > 0)
            {
                foreach (short s in shorts)
                {
                    WriteShort(s);
                }
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteObject(object obj)
        {
            service.WriteObject(this, obj);
        }

        /// <summary>Returns this buffer's position.</summary>
        /// <remarks>Returns this buffer's position.</remarks>
        public int Position()
        {
            return pos;
        }

        public void WriteZeroBytes(int count)
        {
            for (int k = 0; k < count; k++)
            {
                Write(0);
            }
        }


        public virtual void Position(int newPos)
        {
            if ((newPos > buffer.Length) || (newPos < 0))
            {
                throw new InternalBufferOverflowException("Buffer overflow. Max size is "+buffer.Length +" but new position is "+ newPos);
            }
            pos = newPos;
        }

        public virtual byte[] GetBuffer()
        {
            return buffer;
        }

        public virtual byte[] ToByteArray()
        {
            if (buffer == null)
            {
                return new byte[0];
            }
            var newBuffer = new byte[pos];
            Array.Copy(buffer, 0, newBuffer, 0, pos);
            return newBuffer;
        }

        public virtual void Clear()
        {
            pos = 0;
            if (buffer != null && buffer.Length > initialSize*8)
            {
                buffer = new byte[initialSize*8];
            }
        }

        public virtual bool IsBigEndian()
        {
            //TODO BIG ENDIAN
            return true; //!BitConverter.IsLittleEndian;
        }

        public void Dispose()
        {
            Close();
        }

        public virtual ISerializationContext GetSerializationContext()
        {
            return service.GetSerializationContext();
        }

        public void Write(int b)
        {
            EnsureAvailable(1);
            buffer[pos++] = unchecked((byte) (b));
        }

        void OutputStream.Write(byte[] b)
        {
            Write(b, 0, b.Length);
        }

        public void Write(byte[] b, int off, int len)
        {
            if ((off < 0) || (off > b.Length) || (len < 0) || ((off + len) > b.Length) || ((off + len) < 0))
            {
                throw new IndexOutOfRangeException();
            }
            if (len == 0)
            {
                return;
            }
            EnsureAvailable(len);
            Array.Copy(b, off, buffer, pos, len);
            pos += len;
        }

        public void Flush()
        {
        }

        public void Close()
        {
            Clear();
            buffer = null;
        }

        internal void EnsureAvailable(int len)
        {
            if (Available() < len)
            {
                if (buffer != null)
                {
                    int newCap = Math.Max(buffer.Length << 1, buffer.Length + len);
                    var newBuffer = new byte[newCap];
                    Array.Copy(buffer, 0, newBuffer, 0, pos);
                    buffer = newBuffer;
                }
                else
                {
                    buffer = new byte[len > initialSize/2 ? len*2 : initialSize];
                }
            }
        }

        public virtual int Available()
        {
            return buffer != null ? buffer.Length - pos : 0;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("ByteArrayObjectDataOutput");
            sb.Append("{size=").Append(buffer != null ? buffer.Length : 0);
            sb.Append(", pos=").Append(pos);
            sb.Append('}');
            return sb.ToString();
        }
    }
}