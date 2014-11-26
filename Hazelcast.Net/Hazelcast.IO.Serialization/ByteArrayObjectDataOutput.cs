using System;
using System.Text;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
    internal class ByteArrayObjectDataOutput : OutputStream, IPortableDataOutput
    {
        private readonly bool bigEndian;
        internal readonly int initialSize;
        internal readonly ISerializationService service;

        internal byte[] buffer;

        internal DynamicByteBuffer header;
        internal int pos;

        private byte[] utfBuffer;

        internal ByteArrayObjectDataOutput(int size, ISerializationService service, ByteOrder byteOrder)
        {
            initialSize = size;
            buffer = new byte[size];
            this.service = service;
            bigEndian = byteOrder == ByteOrder.BigEndian;
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
        public void WriteByte(int v)
        {
            Write(v);
        }

        public void WriteZeroBytes(int count)
        {
            for (int k = 0; k < count; k++)
            {
                Write(0);
            }
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
            EnsureAvailable(Bits.CHAR_SIZE_IN_BYTES);
            Bits.WriteChar(buffer, pos, (char) v, bigEndian);
            pos += Bits.CHAR_SIZE_IN_BYTES;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteChar(int position, int v)
        {
            Bits.WriteChar(buffer, position, (char) v, bigEndian);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteChars(string s)
        {
            int len = s.Length;
            EnsureAvailable(len*Bits.CHAR_SIZE_IN_BYTES);
            for (int i = 0; i < len; i++)
            {
                int v = s[i];
                WriteChar(pos, v);
                pos += Bits.CHAR_SIZE_IN_BYTES;
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
            EnsureAvailable(Bits.INT_SIZE_IN_BYTES);
            Bits.WriteInt(buffer, pos, v, bigEndian);
            pos += Bits.INT_SIZE_IN_BYTES;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteInt(int position, int v)
        {
            Bits.WriteInt(buffer, position, v, bigEndian);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteLong(long v)
        {
            EnsureAvailable(Bits.LONG_SIZE_IN_BYTES);
            Bits.WriteLong(buffer, pos, v, bigEndian);
            pos += Bits.LONG_SIZE_IN_BYTES;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteLong(int position, long v)
        {
            Bits.WriteLong(buffer, position, v, bigEndian);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteShort(int v)
        {
            EnsureAvailable(Bits.SHORT_SIZE_IN_BYTES);
            Bits.WriteShort(buffer, pos, (short) v, bigEndian);
            pos += Bits.SHORT_SIZE_IN_BYTES;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteShort(int position, int v)
        {
            Bits.WriteShort(buffer, position, (short) v, bigEndian);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteUTF(string str)
        {
            if (utfBuffer == null)
            {
                utfBuffer = new byte[UTFEncoderDecoder.UTF_BUFFER_SIZE];
            }
            UTFEncoderDecoder.WriteUTF(this, str, utfBuffer);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteByteArray(byte[] bytes)
        {
            int len = (bytes == null) ? 0 : bytes.Length;
            WriteInt(len);
            if (len > 0)
            {
                Write(bytes, 0, len);
            }
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
        public virtual void WriteObject(object @object)
        {
            service.WriteObject(this, @object);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteData(IData data)
        {
            service.WriteData(this, data);
        }

        /// <summary>Returns this buffer's position.</summary>
        public virtual int Position()
        {
            return pos;
        }

        public virtual void Position(int newPos)
        {
            if ((newPos > buffer.Length) || (newPos < 0))
            {
                throw new ArgumentException();
            }
            pos = newPos;
        }

        public virtual byte[] ToByteArray()
        {
            if (buffer == null || pos == 0)
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
            if (header != null)
            {
                header.Clear();
            }
        }

        public void Dispose()
        {
            Close();
        }

        public virtual ByteOrder GetByteOrder()
        {
            return bigEndian ? ByteOrder.BigEndian : ByteOrder.LittleEndian;
        }

        public virtual DynamicByteBuffer GetHeaderBuffer()
        {
            if (header == null)
            {
                header = new DynamicByteBuffer(new byte[64]).Order(GetByteOrder());
            }
            return header;
        }

        public virtual byte[] GetPortableHeader()
        {
            if (header == null)
            {
                return null;
            }
            header.Flip();
            if (!header.HasRemaining())
            {
                return null;
            }
            var buff = new byte[header.Limit];
            header.Get(buff);
            header.Clear();
            return buff;
        }

        public void Write(int b)
        {
            EnsureAvailable(1);
            buffer[pos++] = unchecked((byte) (b));
        }

        public void Write(byte[] b)
        {
            Write(b, 0, b.Length);
        }

        public virtual void Write(byte[] b, int off, int len)
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

        public virtual void Close()
        {
            pos = 0;
            buffer = null;
            if (header != null)
            {
                header.Close();
                header = null;
            }
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