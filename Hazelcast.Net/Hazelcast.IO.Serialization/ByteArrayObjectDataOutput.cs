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
using System.Text;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
    internal class ByteArrayObjectDataOutput : IOutputStream, IBufferObjectDataOutput
    {
        private readonly int _initialSize;
        private readonly bool _isBigEndian;
        private readonly ISerializationService _service;

        internal ByteArrayObjectDataOutput(int size, ISerializationService service, ByteOrder byteOrder)
        {
            _initialSize = size;
            Buffer = new byte[size];
            _service = service;
            _isBigEndian = byteOrder == ByteOrder.BigEndian;
        }

        internal byte[] Buffer { get; set; }

        internal int Pos { get; set; }

        public virtual void Write(int position, int b)
        {
            Buffer[position] = unchecked((byte) b);
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
            for (var k = 0; k < count; k++)
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
            var len = s.Length;
            EnsureAvailable(len);
            for (var i = 0; i < len; i++)
            {
                Buffer[Pos++] = unchecked((byte) s[i]);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteChar(int v)
        {
            EnsureAvailable(Bits.CharSizeInBytes);
            Bits.WriteChar(Buffer, Pos, (char) v, _isBigEndian);
            Pos += Bits.CharSizeInBytes;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteChar(int position, int v)
        {
            Bits.WriteChar(Buffer, position, (char) v, _isBigEndian);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteChars(string s)
        {
            var len = s.Length;
            EnsureAvailable(len*Bits.CharSizeInBytes);
            for (var i = 0; i < len; i++)
            {
                int v = s[i];
                WriteChar(Pos, v);
                Pos += Bits.CharSizeInBytes;
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

        public void WriteDouble(double v, ByteOrder byteOrder)
        {
            WriteLong(BitConverter.DoubleToInt64Bits(v), byteOrder);
        }

        public void WriteDouble(int position, double v, ByteOrder byteOrder)
        {
            WriteLong(position, BitConverter.DoubleToInt64Bits(v), byteOrder);
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

        public void WriteFloat(float v, ByteOrder byteOrder)
        {
            WriteInt(BitConverter.ToInt32(BitConverter.GetBytes(v), 0), byteOrder);
        }

        public void WriteFloat(int position, float v, ByteOrder byteOrder)
        {
            WriteInt(position, BitConverter.ToInt32(BitConverter.GetBytes(v), 0), byteOrder);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteInt(int v)
        {
            EnsureAvailable(Bits.IntSizeInBytes);
            Bits.WriteInt(Buffer, Pos, v, _isBigEndian);
            Pos += Bits.IntSizeInBytes;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteInt(int position, int v)
        {
            Bits.WriteInt(Buffer, position, v, _isBigEndian);
        }

        public void WriteInt(int v, ByteOrder byteOrder)
        {
            EnsureAvailable(Bits.IntSizeInBytes);
            Bits.WriteInt(Buffer, Pos, v, byteOrder == ByteOrder.BigEndian);
            Pos += Bits.IntSizeInBytes;
        }

        public void WriteInt(int position, int v, ByteOrder byteOrder)
        {
            Bits.WriteInt(Buffer, position, v, byteOrder == ByteOrder.BigEndian);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteLong(long v)
        {
            EnsureAvailable(Bits.LongSizeInBytes);
            Bits.WriteLong(Buffer, Pos, v, _isBigEndian);
            Pos += Bits.LongSizeInBytes;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteLong(int position, long v)
        {
            Bits.WriteLong(Buffer, position, v, _isBigEndian);
        }

        public void WriteLong(long v, ByteOrder byteOrder)
        {
            EnsureAvailable(Bits.LongSizeInBytes);
            Bits.WriteLong(Buffer, Pos, v, byteOrder == ByteOrder.BigEndian);
            Pos += Bits.LongSizeInBytes;
        }

        public void WriteLong(int position, long v, ByteOrder byteOrder)
        {
            Bits.WriteLong(Buffer, position, v, byteOrder == ByteOrder.BigEndian);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteShort(int v)
        {
            EnsureAvailable(Bits.ShortSizeInBytes);
            Bits.WriteShort(Buffer, Pos, (short) v, _isBigEndian);
            Pos += Bits.ShortSizeInBytes;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteShort(int position, int v)
        {
            Bits.WriteShort(Buffer, position, (short) v, _isBigEndian);
        }

        public void WriteShort(int v, ByteOrder byteOrder)
        {
            EnsureAvailable(Bits.ShortSizeInBytes);
            Bits.WriteShort(Buffer, Pos, (short) v, byteOrder == ByteOrder.BigEndian);
            Pos += Bits.ShortSizeInBytes;
        }

        public void WriteShort(int position, int v, ByteOrder byteOrder)
        {
            Bits.WriteShort(Buffer, position, (short) v, byteOrder == ByteOrder.BigEndian);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteUTF(string str)
        {
            var len = (str != null) ? str.Length : Bits.NullArray;
            WriteInt(len);
            if (len > 0)
            {
                EnsureAvailable(len*3);
                for (var i = 0; i < len; i++)
                {
                    Pos += Bits.WriteUtf8Char(Buffer, Pos, str[i]);
                }
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteByteArray(byte[] bytes)
        {
            var len = (bytes != null) ? bytes.Length : Bits.NullArray;
            WriteInt(len);
            if (len > 0)
            {
                Write(bytes, 0, len);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteBooleanArray(bool[] bools)
        {
            var len = (bools != null) ? bools.Length : Bits.NullArray;
            WriteInt(len);
            if (len > 0)
            {
                foreach (var b in bools)
                {
                    WriteBoolean(b);
                }
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteCharArray(char[] chars)
        {
            var len = (chars != null) ? chars.Length : Bits.NullArray;
            WriteInt(len);
            if (len > 0)
            {
                foreach (var c in chars)
                {
                    WriteChar(c);
                }
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteIntArray(int[] ints)
        {
            var len = (ints != null) ? ints.Length : Bits.NullArray;
            WriteInt(len);
            if (len > 0)
            {
                foreach (var i in ints)
                {
                    WriteInt(i);
                }
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteLongArray(long[] longs)
        {
            var len = (longs != null) ? longs.Length : Bits.NullArray;
            WriteInt(len);
            if (len > 0)
            {
                foreach (var l in longs)
                {
                    WriteLong(l);
                }
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteDoubleArray(double[] doubles)
        {
            var len = (doubles != null) ? doubles.Length : Bits.NullArray;
            WriteInt(len);
            if (len > 0)
            {
                foreach (var d in doubles)
                {
                    WriteDouble(d);
                }
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteFloatArray(float[] floats)
        {
            var len = (floats != null) ? floats.Length : Bits.NullArray;
            WriteInt(len);
            if (len > 0)
            {
                foreach (var f in floats)
                {
                    WriteFloat(f);
                }
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteShortArray(short[] shorts)
        {
            var len = (shorts != null) ? shorts.Length : Bits.NullArray;
            WriteInt(len);
            if (len > 0)
            {
                foreach (var s in shorts)
                {
                    WriteShort(s);
                }
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteUTFArray(string[] strings)
        {
            var len = (strings != null) ? strings.Length : Bits.NullArray;
            WriteInt(len);
            if (len > 0)
            {
                foreach (var s in strings)
                {
                    WriteUTF(s);
                }
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteObject(object @object)
        {
            _service.WriteObject(this, @object);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteData(IData data)
        {
            var payload = data != null ? data.ToByteArray() : null;
            WriteByteArray(payload);
        }

        /// <summary>Returns this buffer's position.</summary>
        public virtual int Position()
        {
            return Pos;
        }

        public virtual void Position(int newPos)
        {
            if ((newPos > Buffer.Length) || (newPos < 0))
            {
                throw new ArgumentException();
            }
            Pos = newPos;
        }

        public virtual byte[] ToByteArray()
        {
            if (Buffer == null || Pos == 0)
            {
                return new byte[0];
            }
            var newBuffer = new byte[Pos];
            Array.Copy(Buffer, 0, newBuffer, 0, Pos);
            return newBuffer;
        }

        public virtual void Clear()
        {
            Pos = 0;
            if (Buffer != null && Buffer.Length > _initialSize*8)
            {
                Buffer = new byte[_initialSize*8];
            }
        }

        public void Dispose()
        {
            Close();
        }

        public virtual ByteOrder GetByteOrder()
        {
            return _isBigEndian ? ByteOrder.BigEndian : ByteOrder.LittleEndian;
        }

        public void Write(int b)
        {
            EnsureAvailable(1);
            Buffer[Pos++] = unchecked((byte) (b));
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
            Array.Copy(b, off, Buffer, Pos, len);
            Pos += len;
        }

        public void Flush()
        {
        }

        public virtual void Close()
        {
            Pos = 0;
            Buffer = null;
        }

        public virtual int Available()
        {
            return Buffer != null ? Buffer.Length - Pos : 0;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("ByteArrayObjectDataOutput");
            sb.Append("{size=").Append(Buffer != null ? Buffer.Length : 0);
            sb.Append(", pos=").Append(Pos);
            sb.Append('}');
            return sb.ToString();
        }

        internal void EnsureAvailable(int len)
        {
            if (Available() < len)
            {
                if (Buffer != null)
                {
                    var newCap = Math.Max(Buffer.Length << 1, Buffer.Length + len);
                    var newBuffer = new byte[newCap];
                    Array.Copy(Buffer, 0, newBuffer, 0, Pos);
                    Buffer = newBuffer;
                }
                else
                {
                    Buffer = new byte[len > _initialSize/2 ? len*2 : _initialSize];
                }
            }
        }
    }
}