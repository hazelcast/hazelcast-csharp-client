// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
    internal partial class ObjectDataInput //public api
    {
        public Endianness Endianness { get; }

        public bool ReadBoolean()
        {
            CheckAvailable(Position, BytesExtensions.SizeOfBool);
            var value = _buffer.ReadBool(Position);
            Position += BytesExtensions.SizeOfBool;
            return value;
        }

        public byte ReadByte()
        {
            CheckAvailable(Position, BytesExtensions.SizeOfByte);
            var value = _buffer.ReadByte(Position);
            Position += BytesExtensions.SizeOfByte;
            return value;
        }

        public sbyte ReadSByte()
        {
            CheckAvailable(Position, BytesExtensions.SizeOfByte);
            var value = _buffer.ReadSByte(Position);
            Position += BytesExtensions.SizeOfByte;
            return value;
        }

        public char ReadChar()
        {
            CheckAvailable(Position, BytesExtensions.SizeOfChar);
            var value = _buffer.ReadChar(Position, Endianness);
            Position += BytesExtensions.SizeOfChar;
            return value;
        }

        public short ReadShort()
        {
            CheckAvailable(Position, BytesExtensions.SizeOfShort);
            var value = _buffer.ReadShort(Position, Endianness);
            Position += BytesExtensions.SizeOfShort;
            return value;
        }

        public ushort ReadUShort()
        {
            CheckAvailable(Position, BytesExtensions.SizeOfShort);
            var value = _buffer.ReadUShort(Position, Endianness);
            Position += BytesExtensions.SizeOfShort;
            return value;
        }

        public int ReadInt()
        {
            CheckAvailable(Position, BytesExtensions.SizeOfInt);
            var value = _buffer.ReadInt(Position, Endianness);
            Position += BytesExtensions.SizeOfInt;
            return value;
        }

        public long ReadLong()
        {
            CheckAvailable(Position, BytesExtensions.SizeOfLong);
            var value = _buffer.ReadLong(Position, Endianness);
            Position += BytesExtensions.SizeOfLong;
            return value;
        }

        public float ReadFloat()
        {
            CheckAvailable(Position, BytesExtensions.SizeOfFloat);
            var value = _buffer.ReadFloat(Position, Endianness);
            Position += BytesExtensions.SizeOfFloat;
            return value;
        }

        public double ReadDouble()
        {
            CheckAvailable(Position, BytesExtensions.SizeOfDouble);
            var value = _buffer.ReadDouble(Position, Endianness);
            Position += BytesExtensions.SizeOfDouble;
            return value;
        }

        public string ReadString()
        {
            var numberOfBytes = ReadInt();
            if (numberOfBytes == BytesExtensions.SizeOfNullArray) return null;

            var result = Encoding.UTF8.GetString(_buffer, Position, numberOfBytes);
            Position += numberOfBytes;
            return result;
        }

        public bool[] ReadBooleanArray()
        {
            var length = ReadInt();
            if (length == BytesExtensions.SizeOfNullArray) return null;
            if (length <= 0) return Array.Empty<bool>();

            CheckAvailable(Position, length * BytesExtensions.SizeOfBool);

            var values = new bool[length];
            for (var i = 0; i < length; i++)
            {
                values[i] = _buffer.ReadBool(Position);
                Position += BytesExtensions.SizeOfBool;
            }

            return values;
        }

        public byte[] ReadByteArray()
        {
            var length = ReadInt();
            if (length == BytesExtensions.SizeOfNullArray) return null;
            if (length <= 0) return Array.Empty<byte>();

            var values = new byte[length];
            Read(values);
            return values;
        }

        public sbyte[] ReadSByteArray()
        {
            var length = ReadInt();
            if (length == BytesExtensions.SizeOfNullArray) return null;
            if (length <= 0) return Array.Empty<sbyte>();

            var values = new sbyte[length];
            Read(values);
            return values;
        }

        public char[] ReadCharArray()
        {
            var length = ReadInt();
            if (length == BytesExtensions.SizeOfNullArray) return null;
            if (length <= 0) return Array.Empty<char>();

            CheckAvailable(Position, length * BytesExtensions.SizeOfChar);

            var values = new char[length];
            for (var i = 0; i < length; i++)
            {
                values[i] = _buffer.ReadChar(Position, Endianness);
                Position += BytesExtensions.SizeOfChar;
            }

            return values;
        }

        public short[] ReadShortArray()
        {
            var length = ReadInt();
            if (length == BytesExtensions.SizeOfNullArray) return null;
            if (length <= 0) return Array.Empty<short>();

            CheckAvailable(Position, length * BytesExtensions.SizeOfShort);

            var values = new short[length];
            for (var i = 0; i < length; i++)
            {
                values[i] = _buffer.ReadShort(Position, Endianness);
                Position += BytesExtensions.SizeOfShort;
            }

            return values;
        }

        public int[] ReadIntArray()
        {
            var length = ReadInt();
            if (length == BytesExtensions.SizeOfNullArray) return null;
            if (length <= 0) return Array.Empty<int>();

            CheckAvailable(Position, length * BytesExtensions.SizeOfInt);

            var values = new int[length];
            for (var i = 0; i < length; i++)
            {
                values[i] = _buffer.ReadInt(Position, Endianness);
                Position += BytesExtensions.SizeOfInt;
            }

            return values;
        }

        public long[] ReadLongArray()
        {
            var length = ReadInt();
            if (length == BytesExtensions.SizeOfNullArray) return null;
            if (length <= 0) return Array.Empty<long>();

            CheckAvailable(Position, length * BytesExtensions.SizeOfLong);

            var values = new long[length];
            for (var i = 0; i < length; i++)
            {
                values[i] = _buffer.ReadLong(Position, Endianness);
                Position += BytesExtensions.SizeOfLong;
            }

            return values;
        }

        public float[] ReadFloatArray()
        {
            var length = ReadInt();
            if (length == BytesExtensions.SizeOfNullArray) return null;
            if (length <= 0) return Array.Empty<float>();

            CheckAvailable(Position, length * BytesExtensions.SizeOfFloat);

            var values = new float[length];
            for (var i = 0; i < length; i++)
            {
                values[i] = _buffer.ReadFloat(Position, Endianness);
                Position += BytesExtensions.SizeOfFloat;
            }

            return values;
        }

        public double[] ReadDoubleArray()
        {
            var length = ReadInt();
            if (length == BytesExtensions.SizeOfNullArray) return null;
            if (length <= 0) return Array.Empty<double>();

            CheckAvailable(Position, length * BytesExtensions.SizeOfDouble);

            var values = new double[length];
            for (var i = 0; i < length; i++)
            {
                values[i] = _buffer.ReadDouble(Position, Endianness);
                Position += BytesExtensions.SizeOfDouble;
            }

            return values;
        }

        public string[] ReadStringArray()
        {
            var length = ReadInt();
            if (length == BytesExtensions.SizeOfNullArray) return null;
            if (length <= 0) return Array.Empty<string>();

            var values = new string[length];
            for (var i = 0; i < length; i++)
            {
                values[i] = ReadString();
            }

            return values;
        }

        public T ReadObject<T>() => _serializationService.ReadObject<T>(this);

        public int Read(byte[] bytes)
        {
            return Read(bytes, 0, bytes.Length);
        }

        public int Read(byte[] bytes, int offset, int count)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            if (offset < 0 || offset >= bytes.Length) throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0 || offset + count > bytes.Length) throw new ArgumentOutOfRangeException(nameof(count));

            if (count == 0) return 0;
            if (Position >= _length) return -1;

            count = Math.Min(count, _length - Position);

            System.Buffer.BlockCopy(_buffer, Position, bytes, offset, count);
            Position += count;
            return count;
        }

        public int Read(sbyte[] bytes)
        {
            return Read(bytes, 0, bytes.Length);
        }

        public int Read(sbyte[] bytes, int offset, int count)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            if (offset < 0 || offset >= bytes.Length) throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0 || offset + count > bytes.Length) throw new ArgumentOutOfRangeException(nameof(count));

            if (count == 0) return 0;
            if (Position >= _length) return -1;

            count = Math.Min(count, _length - Position);

            System.Buffer.BlockCopy(_buffer, Position, bytes, offset, count);
            Position += count;
            return count;
        }

        public int SkipBytes(int count)
        {
            if (count <= 0) return 0;

            count = Math.Min(count, _length - Position);
            Position += count;
            return count;
        }
    }
}
