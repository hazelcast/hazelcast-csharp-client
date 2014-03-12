using System;
using System.IO;
using System.Text;

namespace Hazelcast.IO.Serialization
{
    internal class ByteArrayObjectDataInput : PortableContextAwareInputStream, IBufferObjectDataInput,
        ISerializationContextAware
    {
        internal readonly ISerializationService service;
        internal readonly int size;
        internal byte[] buffer;

        internal int mark = 0;
        internal int pos = 0;

        internal ByteArrayObjectDataInput(Data data, ISerializationService service) : this(data.buffer, service)
        {
            IClassDefinition cd = data.classDefinition;
            SetClassDefinition(cd);
        }

        internal ByteArrayObjectDataInput(byte[] buffer, ISerializationService service)
        {
            this.buffer = buffer;
            size = buffer != null ? buffer.Length : 0;
            this.service = service;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual int Read(int position)
        {
            return (position < size) ? (buffer[position] & unchecked(0xff)) : -1;
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
        /// <exception cref="System.IO.EndOfStreamException">if this input stream has reached the end.</exception>
        /// <exception cref="System.IO.IOException">if an I/O error occurs.</exception>
        /// <seealso cref="java.io.FilterInputStream#in">java.io.FilterInputStream#in</seealso>
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
        /// <seealso cref="java.io.FilterInputStream#in">java.io.FilterInputStream#in</seealso>
        public virtual char ReadChar()
        {
            char c = ReadChar(pos);
            pos += 2;
            return c;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual char ReadChar(int position)
        {
            int ch1 = Read(position);
            int ch2 = Read(position + 1);
            if ((ch1 | ch2) < 0)
            {
                throw new EndOfStreamException();
            }
            return (char) ((ch1 << 8) + (ch2 << 0));
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
        /// <seealso cref="System.IO.DataInputStream.ReadLong()">System.IO.DataInputStream.ReadLong()</seealso>
        /// <seealso cref="double.LongBitsToDouble(long)">double.LongBitsToDouble(long)</seealso>
        public virtual double ReadDouble()
        {
            return BitConverter.Int64BitsToDouble(ReadLong());
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual double ReadDouble(int position)
        {
            return BitConverter.Int64BitsToDouble(ReadLong(position));
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
        /// <seealso cref="System.IO.DataInputStream.ReadInt()">System.IO.DataInputStream.ReadInt()</seealso>
        public virtual float ReadFloat()
        {
            //TODO BURASI DOGRU MU? BILL GATES'E SORALIM :)
            return BitConverter.ToSingle(BitConverter.GetBytes(ReadInt()), 0);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual float ReadFloat(int position)
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(ReadInt(position)), 0);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadFully(byte[] b)
        {
            Read(b, 0, b.Length);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadFully(byte[] b, int off, int len)
        {
            Read(b, off, len);
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
        public virtual int ReadInt()
        {
            int i = ReadInt(pos);
            pos += 4;
            return i;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual int ReadInt(int position)
        {
            int ch1 = Read(position);
            int ch2 = Read(position + 1);
            int ch3 = Read(position + 2);
            int ch4 = Read(position + 3);
            if ((ch1 | ch2 | ch3 | ch4) < 0)
            {
                throw new EndOfStreamException();
            }
            return ((ch1 << 24) + (ch2 << 16) + (ch3 << 8) + (ch4 << 0));
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
        public virtual long ReadLong()
        {
            long l = ReadLong(pos);
            pos += 8;
            return l;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual long ReadLong(int position)
        {
            return (((long) buffer[position] << 56) + ((long) (buffer[position + 1] & 255) << 48) +
                    ((long) (buffer[position + 2] & 255) << 40) + ((long) (buffer[position + 3] & 255) << 32) +
                    ((long) (buffer[position + 4] & 255) << 24) + ((buffer[position + 5] & 255) << 16) +
                    ((buffer[position + 6] & 255) << 8) + ((buffer[position + 7] & 255) << 0));
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
            int ch1 = Read();
            int ch2 = Read();
            if ((ch1 | ch2) < 0)
            {
                throw new EndOfStreamException();
            }
            return (short) ((ch1 << 8) + (ch2 << 0));
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual short ReadShort(int position)
        {
            int ch1 = Read(position);
            int ch2 = Read(position + 1);
            if ((ch1 | ch2) < 0)
            {
                throw new EndOfStreamException();
            }
            return (short) ((ch1 << 8) + (ch2 << 0));
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual char[] ReadCharArray()
        {
            int len = ReadInt();
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

        public T ReadObject<T>()
        {
            return (T) ReadObject();
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
        /// <exception cref="System.IO.EndOfStreamException">if this input stream has reached the end.</exception>
        /// <exception cref="System.IO.IOException">if an I/O error occurs.</exception>
        /// <seealso cref="java.io.FilterInputStream#in">java.io.FilterInputStream#in</seealso>
        public virtual int ReadUnsignedByte()
        {
            return ReadByte();
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
            return ReadShort();
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
        /// <seealso cref="System.IO.DataInputStream.ReadUTF(System.IO.DataInput)">System.IO.DataInputStream.ReadUTF(System.IO.DataInput)</seealso>
        public virtual string ReadUTF()
        {
            return UTFUtil.ReadUTF(this);
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

        public override void Reset()
        {
            pos = mark;
        }

        public virtual bool IsBigEndian()
        {
            //TODO FORCED BIG ENDIAN
            return true;
        }

        public void Dispose()
        {
            Close();
        }

        public virtual ISerializationContext GetSerializationContext()
        {
            return service.GetSerializationContext();
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override int Read()
        {
            return (pos < size) ? (buffer[pos++] & unchecked(0xff)) : -1;
        }

        public override int Read(byte[] b)
        {
            return Read(b, 0, b.Length);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override int Read(byte[] b, int off, int len)
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
            Array.Copy(buffer, pos, b, off, len);
            pos += len;
            return len;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual object ReadObject()
        {
            return service.ReadObject(this);
        }

        public override long Skip(long n)
        {
            if (n <= 0 || n >= int.MaxValue)
            {
                return 0L;
            }
            return SkipBytes((int) n);
        }

        public virtual byte[] GetBuffer()
        {
            return buffer;
        }

        public override int Available()
        {
            return size - pos;
        }

        public override bool MarkSupported()
        {
            return true;
        }

        public override void Mark(int readlimit)
        {
            mark = pos;
        }

        public override void Close()
        {
            buffer = null;
        }

        public override string ToString()
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