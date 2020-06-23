// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;

namespace Hazelcast.Serialization
{
    // TODO: globally refactor byte array management

    internal class ByteArrayObjectDataOutput : ByteArrayObjectDataInputOuputBase, IOutputStream, IBufferObjectDataOutput
    {
        private readonly int _initialSize;

        internal ByteArrayObjectDataOutput(int size, ISerializationService service, Endianness endianness)
            : base(service, endianness)
        {
            _initialSize = size;
            Buffer = new byte[size];
        }

        internal byte[] Buffer { get; set; }

        internal int Pos { get; set; }

        public virtual void Write(int position, int b)
        {
            Buffer[position] = unchecked((byte) b);
        }

        public virtual void WriteBoolean(bool v)
        {
            WriteByte(v ? 1 : 0);
        }

        public virtual void WriteBoolean(int position, bool v)
        {
            Write(position, v ? 1 : 0);
        }

        // TODO: why do we have this?
        void IOutputStream.Write(int v) => WriteByte(v);

        public void WriteByte(int v)
        {
            EnsureAvailable(1);
            Buffer[Pos++] = unchecked((byte)(v));
        }

        public virtual void WriteByte(int position, int v)
        {
            Write(position, v);
        }

        public void WriteZeroBytes(int count)
        {
            for (var k = 0; k < count; k++)
                WriteByte(0);
        }

        public virtual void WriteBytes(string s)
        {
            var len = s.Length;
            EnsureAvailable(len);
            for (var i = 0; i < len; i++)
                Buffer[Pos++] = unchecked((byte) s[i]);
        }

        public virtual void WriteChar(int v)
        {
            EnsureAvailable(BytesExtensions.SizeOfChar);
            Buffer.WriteChar(Pos, (char) v, DefaultEndianness);
            Pos += BytesExtensions.SizeOfChar;
        }

        public virtual void WriteChar(int position, int v)
        {
            Buffer.WriteChar(position, (char) v, DefaultEndianness);
        }

        public virtual void WriteChars(string s)
        {
            var len = s.Length;
            EnsureAvailable(len* BytesExtensions.SizeOfChar);
            for (var i = 0; i < len; i++)
            {
                int v = s[i];
                WriteChar(Pos, v);
                Pos += BytesExtensions.SizeOfChar;
            }
        }

        public void WriteDouble(double v, Endianness endianness = Endianness.Unspecified)
        {
            WriteLong(BitConverter.DoubleToInt64Bits(v), endianness);
        }

        public void WriteDouble(int position, double v, Endianness endianness = Endianness.Unspecified)
        {
            WriteLong(position, BitConverter.DoubleToInt64Bits(v), endianness);
        }

        public void WriteFloat(float v, Endianness endianness = Endianness.Unspecified)
        {
            WriteInt(BitConverter.ToInt32(BitConverter.GetBytes(v), 0), endianness);
        }

        public void WriteFloat(int position, float v, Endianness endianness = Endianness.Unspecified)
        {
            WriteInt(position, BitConverter.ToInt32(BitConverter.GetBytes(v), 0), endianness);
        }

        public void WriteInt(int v, Endianness endianness = Endianness.Unspecified)
        {
            EnsureAvailable(BytesExtensions.SizeOfInt);
            Buffer.WriteInt(Pos, v, ValueOrDefault(endianness));
            Pos += BytesExtensions.SizeOfInt;
        }

        public void WriteInt(int position, int v, Endianness endianness = Endianness.Unspecified)
        {
            Buffer.WriteInt(position, v, ValueOrDefault(endianness));
        }

        public void WriteLong(long v, Endianness endianness = Endianness.Unspecified)
        {
            EnsureAvailable(BytesExtensions.SizeOfLong);
            Buffer.WriteLong(Pos, v, ValueOrDefault(endianness));
            Pos += BytesExtensions.SizeOfLong;
        }

        public void WriteLong(int position, long v, Endianness endianness = Endianness.Unspecified)
        {
            Buffer.WriteLong(position, v, ValueOrDefault(endianness));
        }

        public void WriteShort(int v, Endianness endianness = Endianness.Unspecified)
        {
            EnsureAvailable(BytesExtensions.SizeOfShort);
            Buffer.WriteShort(Pos, (short) v, ValueOrDefault(endianness));
            Pos += BytesExtensions.SizeOfShort;
        }

        public void WriteShort(int position, int v, Endianness endianness = Endianness.Unspecified)
        {
            Buffer.WriteShort(position, (short)v, ValueOrDefault(endianness));
        }

        public virtual void WriteUtf(string str)
        {
            var len = str?.Length ?? ArraySerializer.NullArrayLength;
            WriteInt(len);
            if (str == null || len <= 0) return;

            EnsureAvailable(len*3);
            var pos = Pos;
            for (var i = 0; i < len; i++)
                Buffer.WriteUtf8Char(ref pos, str[i]);
            Pos = pos;
        }

        public virtual void WriteByteArray(byte[] bytes)
        {
            var len = bytes?.Length ?? ArraySerializer.NullArrayLength;
            WriteInt(len);
            if (bytes == null || len <= 0) return;

            Write(bytes, 0, len);
        }

        public virtual void WriteBooleanArray(bool[] bools)
        {
            var len = bools?.Length ?? ArraySerializer.NullArrayLength;
            WriteInt(len);
            if (bools == null || len <= 0) return;

            foreach (var b in bools)
                WriteBoolean(b);
        }

        public virtual void WriteCharArray(char[] chars)
        {
            var len = chars?.Length ?? ArraySerializer.NullArrayLength;
            WriteInt(len);
            if (chars == null || len <= 0) return;

            foreach (var c in chars)
                WriteChar(c);
        }

        public virtual void WriteIntArray(int[] ints)
        {
            var len = ints?.Length ?? ArraySerializer.NullArrayLength;
            WriteInt(len);
            if (ints == null || len <= 0) return;

            foreach (var i in ints)
                WriteInt(i);
        }

        public virtual void WriteLongArray(long[] longs)
        {
            var len = longs?.Length ?? ArraySerializer.NullArrayLength;
            WriteInt(len);
            if (longs == null || len <= 0) return;

            foreach (var l in longs)
                WriteLong(l);
        }

        public virtual void WriteDoubleArray(double[] doubles)
        {
            var len = doubles?.Length ?? ArraySerializer.NullArrayLength;
            WriteInt(len);
            if (doubles == null || len <= 0) return;

            foreach (var d in doubles)
                WriteDouble(d);
        }

        public virtual void WriteFloatArray(float[] floats)
        {
            var len = floats?.Length ?? ArraySerializer.NullArrayLength;
            WriteInt(len);
            if (floats == null || len <= 0) return;

            foreach (var f in floats)
                WriteFloat(f);
        }

        public virtual void WriteShortArray(short[] shorts)
        {
            var len = shorts?.Length ?? ArraySerializer.NullArrayLength;
            WriteInt(len);
            if (shorts == null || len <= 0) return;

            foreach (var s in shorts)
                WriteShort(s);
        }

        public virtual void WriteUtfArray(string[] strings)
        {
            var len = strings?.Length ?? ArraySerializer.NullArrayLength;
            WriteInt(len);
            if (strings == null || len <= 0) return;

            foreach (var s in strings)
                WriteUtf(s);
        }

        public virtual void WriteObject(object @object)
        {
            SerializationService.WriteObject(this, @object);
        }

        public virtual void WriteData(IData data)
        {
            var payload = data?.ToByteArray();
            WriteByteArray(payload);
        }

        public virtual int Position()
        {
            return Pos;
        }

        public virtual void Position(int newPos)
        {
            if (newPos > Buffer.Length || (newPos < 0))
                throw new ArgumentOutOfRangeException(nameof(newPos));

            Pos = newPos;
        }

        public virtual byte[] ToByteArray()
        {
            if (Buffer == null || Pos == 0)
                return Array.Empty<byte>();

            var newBuffer = new byte[Pos];
            System.Buffer.BlockCopy(Buffer, 0, newBuffer, 0, Pos);
            return newBuffer;
        }

        public virtual void Clear()
        {
            Pos = 0;
            if (Buffer != null && Buffer.Length > _initialSize*8)
                Buffer = new byte[_initialSize*8];
        }

        public void Dispose()
        {
            Close();
        }

        public void Write(byte[] b)
        {
            Write(b, 0, b.Length);
        }

        public virtual void Write(byte[] b, int off, int len)
        {
            if (b == null) throw new ArgumentNullException(nameof(b));
            if (off < 0 || off > b.Length) throw new ArgumentOutOfRangeException(nameof(off));
            if (len < 0 || off + len > b.Length || off + len < 0) throw new ArgumentOutOfRangeException(nameof(len));

            if (len <= 0) return;

            EnsureAvailable(len);
            System.Buffer.BlockCopy(b, off, Buffer, Pos, len);
            Pos += len;
        }

        public void Flush()
        { }

        public virtual void Close()
        {
            Pos = 0;
            Buffer = null;
        }

        public virtual int Available()
        {
            return Buffer?.Length - Pos ?? 0;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("ByteArrayObjectDataOutput");
            sb.Append("{size=").Append(Buffer?.Length ?? 0);
            sb.Append(", pos=").Append(Pos);
            sb.Append('}');
            return sb.ToString();
        }

        internal void EnsureAvailable(int len)
        {
            if (Available() >= len) return;

            if (Buffer != null)
            {
                var newCap = Math.Max(Buffer.Length << 1, Buffer.Length + len);
                var newBuffer = new byte[newCap];
                System.Buffer.BlockCopy(Buffer, 0, newBuffer, 0, Pos);
                Buffer = newBuffer;
            }
            else
            {
                Buffer = new byte[len > _initialSize/2 ? len*2 : _initialSize];
            }
        }
    }
}
