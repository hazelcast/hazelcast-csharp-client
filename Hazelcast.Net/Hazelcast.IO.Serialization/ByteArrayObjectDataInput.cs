// Copyright (c) 2008, Hazelcast, Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.IO;
using System.Text;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
    internal class ByteArrayObjectDataInput : IInputStream, IBufferObjectDataInput
    {
        private readonly bool _bigEndian;
        private readonly ISerializationService _service;

        internal ByteArrayObjectDataInput(byte[] data, ISerializationService service, ByteOrder byteOrder)
            : this(data, 0, service, byteOrder)
        {
        }

        internal ByteArrayObjectDataInput(byte[] data, int offset, ISerializationService service, ByteOrder byteOrder)
        {
            Data = data;
            Size = data != null ? data.Length : 0;
            _service = service;
            Pos = offset;
            _bigEndian = byteOrder == ByteOrder.BigEndian;
        }

        internal char[] CharBuffer { get; set; }

        internal byte[] Data { get; set; }

        internal int MarkPos { get; set; }

        internal int Pos { get; set; }

        internal int Size { get; set; }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual int Read(int position)
        {
            return (position < Size) ? (Data[position] & unchecked(0xff)) : -1;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual bool ReadBoolean()
        {
            var ch = Read();
            if (ch < 0)
            {
                throw new EndOfStreamException();
            }
            return (ch != 0);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual bool ReadBoolean(int position)
        {
            var ch = Read(position);
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
            var ch = Read();
            if (ch < 0)
            {
                throw new EndOfStreamException();
            }
            return unchecked((byte) (ch));
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual byte ReadByte(int position)
        {
            var ch = Read(position);
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
            var c = ReadChar(Pos);
            Pos += Bits.CharSizeInBytes;
            return c;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual char ReadChar(int position)
        {
            CheckAvailable(position, Bits.CharSizeInBytes);
            return Bits.ReadChar(Data, position, _bigEndian);
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
            return BitConverter.Int64BitsToDouble(ReadLong(byteOrder));
        }

        public double ReadDouble(int position, ByteOrder byteOrder)
        {
            return BitConverter.Int64BitsToDouble(ReadLong(position, byteOrder));
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
        public virtual int ReadInt()
        {
            var i = ReadInt(Pos);
            Pos += Bits.IntSizeInBytes;
            return i;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual int ReadInt(int position)
        {
            CheckAvailable(position, Bits.IntSizeInBytes);
            return Bits.ReadInt(Data, position, _bigEndian);
        }

        public int ReadInt(ByteOrder byteOrder)
        {
            var i = ReadInt(Pos, byteOrder);
            Pos += Bits.IntSizeInBytes;
            return i;
        }

        public int ReadInt(int position, ByteOrder byteOrder)
        {
            CheckAvailable(position, Bits.IntSizeInBytes);
            return Bits.ReadInt(Data, position, byteOrder == ByteOrder.BigEndian);
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
            var l = ReadLong(Pos);
            Pos += Bits.LongSizeInBytes;
            return l;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual long ReadLong(int position)
        {
            CheckAvailable(position, Bits.LongSizeInBytes);
            return Bits.ReadLong(Data, position, _bigEndian);
        }

        public long ReadLong(ByteOrder byteOrder)
        {
            var l = ReadLong(Pos, byteOrder);
            Pos += Bits.LongSizeInBytes;
            return l;
        }

        public long ReadLong(int position, ByteOrder byteOrder)
        {
            CheckAvailable(position, Bits.LongSizeInBytes);
            var l = Bits.ReadLong(Data, position, byteOrder == ByteOrder.BigEndian);
            Pos += Bits.LongSizeInBytes;
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
        public virtual short ReadShort()
        {
            var s = ReadShort(Pos);
            Pos += Bits.ShortSizeInBytes;
            return s;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual short ReadShort(int position)
        {
            CheckAvailable(position, Bits.ShortSizeInBytes);
            return Bits.ReadShort(Data, position, _bigEndian);
        }

        public short ReadShort(ByteOrder byteOrder)
        {
            var s = ReadShort(Pos, byteOrder);
            Pos += Bits.ShortSizeInBytes;
            return s;
        }

        public short ReadShort(int position, ByteOrder byteOrder)
        {
            CheckAvailable(position, Bits.ShortSizeInBytes);
            return Bits.ReadShort(Data, position, byteOrder == ByteOrder.BigEndian);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual byte[] ReadByteArray()
        {
            var len = ReadInt();
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
            var len = ReadInt();
            if (len == Bits.NullArray) return null;

            if (len > 0)
            {
                var values = new bool[len];
                for (var i = 0; i < len; i++)
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
            var len = ReadInt();
            if (len == Bits.NullArray) return null;

            if (len > 0)
            {
                var values = new char[len];
                for (var i = 0; i < len; i++)
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
            var len = ReadInt();
            if (len == Bits.NullArray) return null;

            if (len > 0)
            {
                var values = new int[len];
                for (var i = 0; i < len; i++)
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
            var len = ReadInt();
            if (len == Bits.NullArray) return null;

            if (len > 0)
            {
                var values = new long[len];
                for (var i = 0; i < len; i++)
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
            var len = ReadInt();
            if (len == Bits.NullArray) return null;

            if (len > 0)
            {
                var values = new double[len];
                for (var i = 0; i < len; i++)
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
            var len = ReadInt();
            if (len == Bits.NullArray) return null;

            if (len > 0)
            {
                var values = new float[len];
                for (var i = 0; i < len; i++)
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
            var len = ReadInt();
            if (len == Bits.NullArray) return null;

            if (len > 0)
            {
                var values = new short[len];
                for (var i = 0; i < len; i++)
                {
                    values[i] = ReadShort();
                }
                return values;
            }
            return new short[0];
        }

        public virtual string[] ReadUTFArray()
        {
            var len = ReadInt();
            if (len == Bits.NullArray) return null;

            if (len > 0)
            {
                var values = new string[len];
                for (var i = 0; i < len; i++)
                {
                    values[i] = ReadUTF();
                }
                return values;
            }
            return new string[0];
        }

        public T ReadObject<T>()
        {
            return _service.ReadObject<T>(this);
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
        /// <exception cref="System.IO.InvalidDataException">
        ///     if the bytes do not represent a valid modified UTF-8
        ///     encoding of a string.
        /// </exception>
        public virtual string ReadUTF()
        {
            var charCount = ReadInt();
            if (charCount == Bits.NullArray)
            {
                return null;
            }
            if (CharBuffer == null || charCount > CharBuffer.Length)
            {
                CharBuffer = new char[charCount];
            }
            for (var i = 0; i < charCount; i++)
            {
                var b = ReadByte();
                if (b > 127)
                {
                    CharBuffer[i] = Bits.ReadUtf8Char(this, b);
                }
                else
                {
                    CharBuffer[i] = (char) b;
                }
            }
            return new string(CharBuffer, 0, charCount);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public IData ReadData()
        {
            var bytes = ReadByteArray();
            var data = bytes != null ? new HeapData(bytes) : null;
            return data;
        }

        public virtual int SkipBytes(int n)
        {
            if (n <= 0)
            {
                return 0;
            }
            var skip = n;
            var pos = Position();
            if (pos + skip > Size)
            {
                skip = Size - pos;
            }
            Position(pos + skip);
            return skip;
        }

        /// <summary>Returns this buffer's position.</summary>
        /// <remarks>Returns this buffer's position.</remarks>
        public virtual int Position()
        {
            return Pos;
        }

        public virtual void Position(int newPos)
        {
            if ((newPos > Size) || (newPos < 0))
            {
                throw new ArgumentException();
            }
            Pos = newPos;
            if (MarkPos > Pos)
            {
                MarkPos = -1;
            }
        }

        public virtual ByteOrder GetByteOrder()
        {
            return _bigEndian ? ByteOrder.BigEndian : ByteOrder.LittleEndian;
        }

        public void Dispose()
        {
            Close();
        }

        public virtual void Init(byte[] data, int offset)
        {
            Data = data;
            Size = data != null ? data.Length : 0;
            Pos = offset;
        }

        public virtual void Clear()
        {
            Data = null;
            Pos = 0;
            Size = 0;
            MarkPos = 0;
            if (CharBuffer != null && CharBuffer.Length > BufferObjectDataInputConstants.UtfBufferSize*8)
            {
                CharBuffer = new char[BufferObjectDataInputConstants.UtfBufferSize*8];
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual int Read()
        {
            return (Pos < Size) ? (Data[Pos++] & unchecked(0xff)) : -1;
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
            if (Pos >= Size)
            {
                return -1;
            }
            if (Pos + len > Size)
            {
                len = Size - Pos;
            }
            Array.Copy(Data, Pos, b, off, len);
            Pos += len;
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
            return Size - Pos;
        }

        public bool MarkSupported()
        {
            return true;
        }

        public void Mark(int readlimit)
        {
            MarkPos = Pos;
        }

        public void Reset()
        {
            Pos = MarkPos;
        }

        public void Close()
        {
            Data = null;
            CharBuffer = null;
        }

        /// <exception cref="System.IO.IOException"></exception>
        [Obsolete]
        public string ReadLine()
        {
            throw new NotSupportedException();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("ByteArrayObjectDataInput");
            sb.Append("{size=").Append(Size);
            sb.Append(", pos=").Append(Pos);
            sb.Append(", mark=").Append(MarkPos);
            sb.Append('}');
            return sb.ToString();
        }

        /// <exception cref="System.IO.IOException"></exception>
        internal void CheckAvailable(int pos, int k)
        {
            if (pos < 0)
            {
                throw new ArgumentException("Negative pos! -> " + pos);
            }
            if ((Size - pos) < k)
            {
                throw new EndOfStreamException("Cannot read " + k + " bytes!");
            }
        }
    }
}