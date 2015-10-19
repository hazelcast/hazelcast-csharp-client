using System;
using System.IO;
using System.Text;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
    internal class ByteArrayObjectDataInput : InputStream, IBufferObjectDataInput
    {

        private readonly bool bigEndian;
        internal readonly ISerializationService service;

        internal readonly int size;
        internal byte[] data;
        private char[] charBuffer;

        internal int mark;
        internal int pos;

        internal ByteArrayObjectDataInput(byte[] data, ISerializationService service, ByteOrder byteOrder)
            : this(data, 0, service, byteOrder)
        {
        }

        internal ByteArrayObjectDataInput(byte[] data, int offset, ISerializationService service, ByteOrder byteOrder)
        {
            this.data = data;
            size = data != null ? data.Length : 0;
            this.service = service;
            this.pos = offset;
            bigEndian = byteOrder == ByteOrder.BigEndian;  
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual int Read()
        {
            return (pos < size) ? (data[pos++] & unchecked(0xff)) : -1;
        }

        public virtual int Read(byte[] b)
        {
            return Read(b, 0, b.Length);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public int Read(byte[] b, int off, int len)
        {
            if (b == null)
            {
                throw new ArgumentNullException();
            }
            if ((off < 0) || (off > b.Length) || (len < 0) || ((off + len) > b.Length) || ((off + len) < 0))
            {
                throw new IndexOutOfRangeException();
            }
            if (len <= 0)
            {
                return 0;
            }
            if (pos >= size)
            {
                return -1;
            }
            if (pos + len > size)
            {
                len = size - pos;
            }
            Array.Copy(data, pos, b, off, len);
            pos += len;
            return len;
        }

        public long Skip(long n)
        {
            if (n <= 0 || n >= int.MaxValue)
            {
                return 0L;
            }
            return SkipBytes((int) n);
        }

        public int Available()
        {
            return size - pos;
        }

        public bool MarkSupported()
        {
            return true;
        }

        public void Mark(int readlimit)
        {
            mark = pos;
        }

        public void Reset()
        {
            pos = mark;
        }

        public void Close()
        {
            data = null;
            charBuffer = null;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual int Read(int position)
        {
            return (position < size) ? (data[position] & unchecked(0xff)) : -1;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual bool ReadBoolean()
        {
            int ch = Read();
            if (ch < 0)
            {
                throw new EndOfStreamException();
            }
            return (ch != 0);
        }

        public long ReadLong(int position, ByteOrder byteOrder)
        {
            throw new NotImplementedException();
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual bool ReadBoolean(int position)
        {
            int ch = Read(position);
            if (ch < 0)
            {
                throw new EndOfStreamException();
            }
            return (ch != 0);
        }

        /// <summary>
        ///     See the general contract of the <code>readByte</code> method of
        ///     <code>DataInput</code>.
        /// </summary>
        /// <remarks>
        ///     See the general contract of the <code>readByte</code> method of
        ///     <code>DataInput</code>.
        ///     <p />
        ///     Bytes for this operation are read from the contained input stream.
        /// </remarks>
        /// <returns>
        ///     the next byte of this input stream as a signed 8-bit
        ///     <code>byte</code>.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        ///     if this input stream has reached the end.
        /// </exception>
        /// <exception cref="System.IO.IOException">if an I/O error occurs.</exception>
        public virtual byte ReadByte()
        {
            int ch = Read();
            if (ch < 0)
            {
                throw new EndOfStreamException();
            }
            return unchecked((byte) (ch));
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual byte ReadByte(int position)
        {
            int ch = Read(position);
            if (ch < 0)
            {
                throw new EndOfStreamException();
            }
            return unchecked((byte) (ch));
        }

        /// <summary>
        ///     See the general contract of the <code>readChar</code> method of
        ///     <code>DataInput</code>.
        /// </summary>
        /// <remarks>
        ///     See the general contract of the <code>readChar</code> method of
        ///     <code>DataInput</code>.
        ///     <p />
        ///     Bytes for this operation are read from the contained input stream.
        /// </remarks>
        /// <returns>the next two bytes of this input stream as a Unicode character.</returns>
        /// <exception cref="System.IO.EndOfStreamException">
        ///     if this input stream reaches the end before reading two
        ///     bytes.
        /// </exception>
        /// <exception cref="System.IO.IOException">if an I/O error occurs.</exception>
        public virtual char ReadChar()
        {
            char c = ReadChar(pos);
            pos += Bits.CharSizeInBytes;
            return c;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual char ReadChar(int position)
        {
            CheckAvailable(position, Bits.CharSizeInBytes);
            return Bits.ReadChar(data, position, bigEndian);
        }

        /// <summary>
        ///     See the general contract of the <code>readDouble</code> method of
        ///     <code>DataInput</code>.
        /// </summary>
        /// <remarks>
        ///     See the general contract of the <code>readDouble</code> method of
        ///     <code>DataInput</code>.
        ///     <p />
        ///     Bytes for this operation are read from the contained input stream.
        /// </remarks>
        /// <returns>
        ///     the next eight bytes of this input stream, interpreted as a
        ///     <code>double</code>.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        ///     if this input stream reaches the end before reading eight
        ///     bytes.
        /// </exception>
        /// <exception cref="System.IO.IOException">if an I/O error occurs.</exception>
        public virtual double ReadDouble()
        {
            return BitConverter.Int64BitsToDouble(ReadLong());
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual double ReadDouble(int position)
        {
            return BitConverter.Int64BitsToDouble(ReadLong(position));
        }

        public double ReadDouble(ByteOrder byteOrder)
        {
            throw new NotImplementedException();
        }

        public double ReadDouble(int position, ByteOrder byteOrder)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     See the general contract of the <code>readFloat</code> method of
        ///     <code>DataInput</code>.
        /// </summary>
        /// <remarks>
        ///     See the general contract of the <code>readFloat</code> method of
        ///     <code>DataInput</code>.
        ///     <p />
        ///     Bytes for this operation are read from the contained input stream.
        /// </remarks>
        /// <returns>
        ///     the next four bytes of this input stream, interpreted as a
        ///     <code>float</code>.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        ///     if this input stream reaches the end before reading four
        ///     bytes.
        /// </exception>
        /// <exception cref="System.IO.IOException">if an I/O error occurs.</exception>
        /// <seealso cref="System.IO.DataInputStream.ReadInt()">
        ///     System.IO.DataInputStream.ReadInt()
        /// </seealso>
        /// <seealso cref="Sharpen.Runtime.IntBitsToFloat(int)">
        ///     Sharpen.Runtime.IntBitsToFloat(int)
        /// </seealso>
        public virtual float ReadFloat()
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(ReadInt()), 0);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual float ReadFloat(int position)
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(ReadInt(position)), 0);
        }

        public float ReadFloat(ByteOrder byteOrder)
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(ReadInt(byteOrder)), 0);
        }

        public float ReadFloat(int position, ByteOrder byteOrder)
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(ReadInt(position, byteOrder)), 0);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadFully(byte[] b)
        {
            if (Read(b, 0, b.Length) == -1)
            {
                throw new EndOfStreamException("End of stream reached");
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadFully(byte[] b, int off, int len)
        {
            if (Read(b, off, len) == -1)
            {
                throw new EndOfStreamException("End of stream reached");
            }
        }

        /// <summary>
        ///     See the general contract of the <code>readInt</code> method of
        ///     <code>DataInput</code>.
        /// </summary>
        /// <remarks>
        ///     See the general contract of the <code>readInt</code> method of
        ///     <code>DataInput</code>.
        ///     <p />
        ///     Bytes for this operation are read from the contained input stream.
        /// </remarks>
        /// <returns>
        ///     the next four bytes of this input stream, interpreted as an
        ///     <code>int</code>.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        ///     if this input stream reaches the end before reading four
        ///     bytes.
        /// </exception>
        /// <exception cref="System.IO.IOException">if an I/O error occurs.</exception>
        /// <seealso cref="java.io.FilterInputStream#in">java.io.FilterInputStream#in</seealso>
        public virtual int ReadInt()
        {
            int i = ReadInt(pos);
            pos += Bits.IntSizeInBytes;
            return i;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual int ReadInt(int position)
        {
            CheckAvailable(position, Bits.IntSizeInBytes);
            return Bits.ReadInt(data, position, bigEndian);
        }

        public int ReadInt(ByteOrder byteOrder)
        {
            int i = ReadInt(pos, byteOrder);
            pos += Bits.IntSizeInBytes;
            return i;
        }

        public int ReadInt(int position, ByteOrder byteOrder)
        {
            CheckAvailable(position, Bits.IntSizeInBytes);
            return Bits.ReadInt(data, position, byteOrder == ByteOrder.BigEndian);
        }

        /// <summary>
        ///     See the general contract of the <code>readLong</code> method of
        ///     <code>DataInput</code>.
        /// </summary>
        /// <remarks>
        ///     See the general contract of the <code>readLong</code> method of
        ///     <code>DataInput</code>.
        ///     <p />
        ///     Bytes for this operation are read from the contained input stream.
        /// </remarks>
        /// <returns>
        ///     the next eight bytes of this input stream, interpreted as a
        ///     <code>long</code>.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        ///     if this input stream reaches the end before reading eight
        ///     bytes.
        /// </exception>
        /// <exception cref="System.IO.IOException">if an I/O error occurs.</exception>
        /// <seealso cref="java.io.FilterInputStream#in">java.io.FilterInputStream#in</seealso>
        public virtual long ReadLong()
        {
            long l = ReadLong(pos);
            pos += Bits.LongSizeInBytes;
            return l;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual long ReadLong(int position)
        {
            CheckAvailable(position, Bits.LongSizeInBytes);
            return Bits.ReadLong(data, position, bigEndian);
        }

        public long ReadLong(ByteOrder byteOrder)
        {
            long l = ReadLong(pos, byteOrder);
            pos += Bits.LongSizeInBytes;
            return l;
        }

        /// <summary>
        ///     See the general contract of the <code>readShort</code> method of
        ///     <code>DataInput</code>.
        /// </summary>
        /// <remarks>
        ///     See the general contract of the <code>readShort</code> method of
        ///     <code>DataInput</code>.
        ///     <p />
        ///     Bytes for this operation are read from the contained input stream.
        /// </remarks>
        /// <returns>
        ///     the next two bytes of this input stream, interpreted as a signed
        ///     16-bit number.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        ///     if this input stream reaches the end before reading two
        ///     bytes.
        /// </exception>
        /// <exception cref="System.IO.IOException">if an I/O error occurs.</exception>
        /// <seealso cref="java.io.FilterInputStream#in">java.io.FilterInputStream#in</seealso>
        public virtual short ReadShort()
        {
            short s = ReadShort(pos);
            pos += Bits.ShortSizeInBytes;
            return s;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual short ReadShort(int position)
        {
            CheckAvailable(position, Bits.ShortSizeInBytes);
            return Bits.ReadShort(data, position, bigEndian);
        }

        public short ReadShort(ByteOrder byteOrder)
        {
            short s = ReadShort(pos, byteOrder);
            pos += Bits.ShortSizeInBytes;
            return s;
        }

        public short ReadShort(int position, ByteOrder byteOrder)
        {
            CheckAvailable(position, Bits.ShortSizeInBytes);
            return Bits.ReadShort(data, position, byteOrder == ByteOrder.BigEndian);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual byte[] ReadByteArray()
        {
            int len = ReadInt();
            if (len == Bits.NullArray) return null;

            if (len > 0)
            {
                var b = new byte[len];
                ReadFully(b);
                return b;
            }
            return new byte[0];
        }

        public virtual bool[] ReadBooleanArray()
        {
            int len = ReadInt();
            if (len == Bits.NullArray) return null;

            if (len > 0)
            {
                var values = new bool[len];
                for (int i = 0; i < len; i++)
                {
                    values[i] = ReadBoolean();
                }
                return values;
            }
            return new bool[0];
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual char[] ReadCharArray()
        {
            int len = ReadInt();
            if (len == Bits.NullArray) return null;

            if (len > 0)
            {
                var values = new char[len];
                for (int i = 0; i < len; i++)
                {
                    values[i] = ReadChar();
                }
                return values;
            }
            return new char[0];
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual int[] ReadIntArray()
        {
            int len = ReadInt();
            if (len == Bits.NullArray) return null;

            if (len > 0)
            {
                var values = new int[len];
                for (int i = 0; i < len; i++)
                {
                    values[i] = ReadInt();
                }
                return values;
            }
            return new int[0];
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual long[] ReadLongArray()
        {
            int len = ReadInt();
            if (len == Bits.NullArray) return null;

            if (len > 0)
            {
                var values = new long[len];
                for (int i = 0; i < len; i++)
                {
                    values[i] = ReadLong();
                }
                return values;
            }
            return new long[0];
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual double[] ReadDoubleArray()
        {
            int len = ReadInt();
            if (len == Bits.NullArray) return null;

            if (len > 0)
            {
                var values = new double[len];
                for (int i = 0; i < len; i++)
                {
                    values[i] = ReadDouble();
                }
                return values;
            }
            return new double[0];
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual float[] ReadFloatArray()
        {
            int len = ReadInt();
            if (len == Bits.NullArray) return null;

            if (len > 0)
            {
                var values = new float[len];
                for (int i = 0; i < len; i++)
                {
                    values[i] = ReadFloat();
                }
                return values;
            }
            return new float[0];
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual short[] ReadShortArray()
        {
            int len = ReadInt();
            if (len == Bits.NullArray) return null;

            if (len > 0)
            {
                var values = new short[len];
                for (int i = 0; i < len; i++)
                {
                    values[i] = ReadShort();
                }
                return values;
            }
            return new short[0];
        }

        public virtual string[] ReadUTFArray()
        {
            int len = ReadInt();
            if (len == Bits.NullArray) return null;

            if (len > 0)
            {
                var values = new string[len];
                for (int i = 0; i < len; i++)
                {
                    values[i] = ReadUTF();
                }
                return values;
            }
            return new string[0];
        }

        public T ReadObject<T>()
        {
            return service.ReadObject<T>(this);
        }

        /// <summary>
        ///     See the general contract of the <code>readUnsignedByte</code> method of
        ///     <code>DataInput</code>.
        /// </summary>
        /// <remarks>
        ///     See the general contract of the <code>readUnsignedByte</code> method of
        ///     <code>DataInput</code>.
        ///     <p />
        ///     Bytes for this operation are read from the contained input stream.
        /// </remarks>
        /// <returns>
        ///     the next byte of this input stream, interpreted as an unsigned
        ///     8-bit number.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        ///     if this input stream has reached the end.
        /// </exception>
        /// <exception cref="System.IO.IOException">if an I/O error occurs.</exception>
        /// <seealso cref="java.io.FilterInputStream#in">java.io.FilterInputStream#in</seealso>
        public virtual int ReadUnsignedByte()
        {
            return ReadByte() & 0xFF;
        }

        /// <summary>
        ///     See the general contract of the <code>readUnsignedShort</code> method of
        ///     <code>DataInput</code>.
        /// </summary>
        /// <remarks>
        ///     See the general contract of the <code>readUnsignedShort</code> method of
        ///     <code>DataInput</code>.
        ///     <p />
        ///     Bytes for this operation are read from the contained input stream.
        /// </remarks>
        /// <returns>
        ///     the next two bytes of this input stream, interpreted as an
        ///     unsigned 16-bit integer.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        ///     if this input stream reaches the end before reading two
        ///     bytes.
        /// </exception>
        /// <exception cref="System.IO.IOException">if an I/O error occurs.</exception>
        /// <seealso cref="java.io.FilterInputStream#in">java.io.FilterInputStream#in</seealso>
        public virtual int ReadUnsignedShort()
        {
            return ReadShort() & 0xffff;
        }

        /// <summary>
        ///     See the general contract of the <code>readUTF</code> method of
        ///     <code>DataInput</code>.
        /// </summary>
        /// <remarks>
        ///     See the general contract of the <code>readUTF</code> method of
        ///     <code>DataInput</code>.
        ///     <p />
        ///     Bytes for this operation are read from the contained input stream.
        /// </remarks>
        /// <returns>a Unicode string.</returns>
        /// <exception cref="System.IO.EndOfStreamException">
        ///     if this input stream reaches the end before reading all
        ///     the bytes.
        /// </exception>
        /// <exception cref="System.IO.IOException">if an I/O error occurs.</exception>
        /// <exception cref="System.IO.UTFDataFormatException">
        ///     if the bytes do not represent a valid modified UTF-8
        ///     encoding of a string.
        /// </exception>
        /// <seealso cref="System.IO.DataInputStream.ReadUTF(System.IO.IDataInput)">
        ///     System.IO.DataInputStream.ReadUTF(System.IO.IDataInput)
        /// </seealso>
        public virtual string ReadUTF()
        {
            int charCount = ReadInt();
            if (charCount == Bits.NullArray)
            {
                return null;
            }
            if (charBuffer == null || charCount > charBuffer.Length)
            {
                charBuffer = new char[charCount];
            }
            for (int i = 0; i < charCount; i++)
            {
                var b = ReadByte();
                if (b > 127)
                {
                    charBuffer[i] = Bits.ReadUtf8Char(this, b);
                }
                else
                {
                    charBuffer[i] = (char)b;
                }
            }
            return new string(charBuffer, 0, charCount);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public IData ReadData()
        {
            byte[] bytes = ReadByteArray();
            var data = bytes != null ? new HeapData(bytes) : null;
            return data;
        }

        public virtual int SkipBytes(int n)
        {
            if (n <= 0)
            {
                return 0;
            }
            int skip = n;
            int pos = Position();
            if (pos + skip > size)
            {
                skip = size - pos;
            }
            Position(pos + skip);
            return skip;
        }

        /// <summary>Returns this buffer's position.</summary>
        /// <remarks>Returns this buffer's position.</remarks>
        public virtual int Position()
        {
            return pos;
        }

        public virtual void Position(int newPos)
        {
            if ((newPos > size) || (newPos < 0))
            {
                throw new ArgumentException();
            }
            pos = newPos;
            if (mark > pos)
            {
                mark = -1;
            }
        }

        public virtual ByteOrder GetByteOrder()
        {
            return bigEndian ? ByteOrder.BigEndian : ByteOrder.LittleEndian;
        }

        public void Dispose()
        {
            Close();
        }

        /// <exception cref="System.IO.IOException"></exception>
        [Obsolete]
        public string ReadLine()
        {
            throw new NotSupportedException();
        }

        /// <exception cref="System.IO.IOException"></exception>
        internal void CheckAvailable(int pos, int k)
        {
            if (pos < 0)
            {
                throw new ArgumentException("Negative pos! -> " + pos);
            }
            if ((size - pos) < k)
            {
                throw new EndOfStreamException("Cannot read " + k + " bytes!");
            }
        }

        public virtual string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("ByteArrayObjectDataInput");
            sb.Append("{size=").Append(size);
            sb.Append(", pos=").Append(pos);
            sb.Append(", mark=").Append(mark);
            sb.Append('}');
            return sb.ToString();
        }
    }
}