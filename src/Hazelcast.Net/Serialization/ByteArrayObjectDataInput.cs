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
using Hazelcast.Core;
using Hazelcast.Exceptions;

namespace Hazelcast.Serialization
{
    /// <summary>
    /// Implements <see cref="IBufferObjectDataInput"/> on top of a byte array.
    /// </summary>
    internal class ByteArrayObjectDataInput : ByteArrayObjectDataInputOuputBase, IBufferObjectDataInput
    {
        private byte[] _data;
        private int _length;
        private int _position;

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteArrayObjectDataInput"/> class.
        /// </summary>
        /// <param name="data">The buffer data.</param>
        /// <param name="service">The serialization service.</param>
        /// <param name="endianness">The default endianness.</param>
        internal ByteArrayObjectDataInput(byte[] data, ISerializationService service, Endianness endianness)
            : this(data, 0, service, endianness)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteArrayObjectDataInput"/> class.
        /// </summary>
        /// <param name="data">The buffer data.</param>
        /// <param name="offset">The buffer data offset.</param>
        /// <param name="service">The serialization service.</param>
        /// <param name="endianness">The default endianness.</param>
        internal ByteArrayObjectDataInput(byte[] data, int offset, ISerializationService service, Endianness endianness)
            : base(service, endianness)
        {
            _data = data;
            _length = data?.Length ?? 0;

            // testing offset > _length because when data == null, offset == 0 ... weird
            if (offset < 0 || offset > _length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            _position = offset;
        }

        /// <summary>
        /// Gets or sets the current position.
        /// </summary>
        public int Position
        {
            get => _position;
            set
            {
                if (value < 0 || value > _length)
                    throw new ArgumentOutOfRangeException(nameof(value));
                _position = value;
            }
        }

        /// <summary>
        /// Validates that a number of bytes can be read at a given position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="count">The number of bytes.</param>
        internal void Validate(int position, int count)
        {
            if (position < 0)
                throw new ArgumentOutOfRangeException(nameof(position));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (_length - position < count)
                throw new InvalidOperationException(ExceptionMessages.NotEnoughBytes);
        }

        /// <summary>
        /// (internal for tests only)
        /// Gets the data buffer.
        /// </summary>
        internal byte[] Data => _data;

        /// <inheritdoc />
        public byte ReadByte()
        {
            var value = ReadByte(_position);
            _position += BytesExtensions.SizeOfByte;
            return value;
        }

        /// <inheritdoc />
        public byte[] ReadByteArray(Endianness endianness = Endianness.Unspecified)
        {
            var length = ReadInt(endianness);
            if (length == ArraySerializer.NullArrayLength) return null;
            if (length <= 0) return Array.Empty<byte>();

            var values = new byte[length];
            ReadBytes(values);
            return values;
        }

        /// <inheritdoc />
        public byte ReadByte(int position)
        {
            Validate(position, BytesExtensions.SizeOfByte);
            return _data.ReadByte(position);
        }

        /// <inheritdoc />
        public short ReadShort(Endianness endianness = Endianness.Unspecified)
        {
            var value = ReadShort(_position, endianness);
            _position += BytesExtensions.SizeOfShort;
            return value;
        }

        /// <inheritdoc />
        public short ReadShort(int position, Endianness endianness = Endianness.Unspecified)
        {
            Validate(position, BytesExtensions.SizeOfShort);
            return _data.ReadShort(position, endianness.Resolve(DefaultEndianness));
        }

        /// <inheritdoc />
        public short[] ReadShortArray(Endianness endianness = Endianness.Unspecified)
        {
            endianness = endianness.Resolve(DefaultEndianness);

            var length = ReadInt(endianness);
            if (length == ArraySerializer.NullArrayLength) return null;
            if (length <= 0) return Array.Empty<short>();

            Validate(_position, length * BytesExtensions.SizeOfShort);

            var values = new short[length];
            for (var i = 0; i < length; i++)
            {
                values[i] = _data.ReadShort(_position, endianness);
                _position += BytesExtensions.SizeOfShort;
            }
            return values;
        }

        /// <inheritdoc />
        public ushort ReadUnsignedShort(Endianness endianness = Endianness.Unspecified)
        {
            return (ushort) ReadShort(endianness);
        }

        /// <inheritdoc />
        public ushort[] ReadUnsignedShortArray(Endianness endianness = Endianness.Unspecified)
        {
            endianness = endianness.Resolve(DefaultEndianness);

            var length = ReadInt(endianness);
            if (length == ArraySerializer.NullArrayLength) return null;
            if (length <= 0) return Array.Empty<ushort>();

            Validate(_position, length * BytesExtensions.SizeOfUnsignedShort);

            var values = new ushort[length];
            for (var i = 0; i < length; i++)
            {
                values[i] = _data.ReadUShort(_position, endianness);
                _position += BytesExtensions.SizeOfUnsignedShort;
            }
            return values;
        }

        /// <inheritdoc />
        public int ReadInt(Endianness endianness = Endianness.Unspecified)
        {
            var value = ReadInt(_position, endianness);
            _position += BytesExtensions.SizeOfInt;
            return value;
        }

        /// <inheritdoc />
        public int ReadInt(int position, Endianness endianness = Endianness.Unspecified)
        {
            Validate(position, BytesExtensions.SizeOfInt);
            return _data.ReadInt(position, endianness.Resolve(DefaultEndianness));
        }

        /// <inheritdoc />
        public int[] ReadIntArray(Endianness endianness = Endianness.Unspecified)
        {
            endianness = endianness.Resolve(DefaultEndianness);

            var length = ReadInt(endianness);
            if (length == ArraySerializer.NullArrayLength) return null;
            if (length <= 0) return Array.Empty<int>();

            Validate(_position, length * BytesExtensions.SizeOfInt);

            var values = new int[length];
            for (var i = 0; i < length; i++)
            {
                values[i] = _data.ReadInt(_position, endianness);
                _position += BytesExtensions.SizeOfInt;
            }
            return values;
        }

        /// <inheritdoc />
        public long ReadLong(Endianness endianness = Endianness.Unspecified)
        {
            var value = ReadLong(_position, endianness);
            _position += BytesExtensions.SizeOfLong;
            return value;
        }

        /// <inheritdoc />
        public long ReadLong(int position, Endianness endianness = Endianness.Unspecified)
        {
            Validate(position, BytesExtensions.SizeOfLong);
            return _data.ReadLong(position, endianness.Resolve(DefaultEndianness));
        }

        /// <inheritdoc />
        public long[] ReadLongArray(Endianness endianness = Endianness.Unspecified)
        {
            endianness = endianness.Resolve(DefaultEndianness);

            var length = ReadInt(endianness);
            if (length == ArraySerializer.NullArrayLength) return null;
            if (length <= 0) return Array.Empty<long>();

            Validate(_position, length * BytesExtensions.SizeOfLong);

            var values = new long[length];
            for (var i = 0; i < length; i++)
            {
                values[i] = _data.ReadLong(_position, endianness);
                _position += BytesExtensions.SizeOfLong;
            }
            return values;
        }



        /// <inheritdoc />
        public bool ReadBool()
        {
            var value = ReadBool(_position);
            _position += BytesExtensions.SizeOfBool;
            return value;
        }

        /// <inheritdoc />
        public bool ReadBool(int position)
        {
            Validate(position, BytesExtensions.SizeOfBool);
            return _data.ReadBool(position);
        }

        /// <inheritdoc />
        public bool[] ReadBoolArray(Endianness endianness = Endianness.Unspecified)
        {
            endianness = endianness.Resolve(DefaultEndianness);

            var length = ReadInt(endianness);
            if (length == ArraySerializer.NullArrayLength) return null;
            if (length <= 0) return Array.Empty<bool>();

            Validate(_position, length * BytesExtensions.SizeOfBool);

            var values = new bool[length];
            for (var i = 0; i < length; i++)
            {
                values[i] = _data.ReadBool(_position);
                _position += BytesExtensions.SizeOfBool;
            }
            return values;
        }



        /// <inheritdoc />
        public char ReadChar(Endianness endianness = Endianness.Unspecified)
        {
            var value = ReadChar(_position, endianness);
            _position += BytesExtensions.SizeOfChar;
            return value;
        }

        /// <inheritdoc />
        public char ReadChar(int position, Endianness endianness = Endianness.Unspecified)
        {
            Validate(position, BytesExtensions.SizeOfChar);
            return _data.ReadChar(position, endianness.Resolve(DefaultEndianness));
        }

        /// <inheritdoc />
        public char[] ReadCharArray(Endianness endianness = Endianness.Unspecified)
        {
            endianness = endianness.Resolve(DefaultEndianness);

            var length = ReadInt(endianness);
            if (length == ArraySerializer.NullArrayLength) return null;
            if (length <= 0) return Array.Empty<char>();

            Validate(_position, length * BytesExtensions.SizeOfChar);

            var values = new char[length];
            for (var i = 0; i < length; i++)
            {
                values[i] = _data.ReadChar(_position, endianness);
                _position += BytesExtensions.SizeOfChar;

            }
            return values;
        }



        /// <inheritdoc />
        public float ReadFloat(Endianness endianness = Endianness.Unspecified)
        {
            var value = ReadFloat(_position, endianness);
            _position += BytesExtensions.SizeOfFloat;
            return value;
        }

        /// <inheritdoc />
        public float ReadFloat(int position, Endianness endianness = Endianness.Unspecified)
        {
            Validate(position, BytesExtensions.SizeOfFloat);
            return _data.ReadFloat(position, endianness.Resolve(DefaultEndianness));
        }

        /// <inheritdoc />
        public float[] ReadFloatArray(Endianness endianness = Endianness.Unspecified)
        {
            endianness = endianness.Resolve(DefaultEndianness);

            var length = ReadInt(endianness);
            if (length == ArraySerializer.NullArrayLength) return null;
            if (length <= 0) return Array.Empty<float>();

            Validate(_position, length * BytesExtensions.SizeOfFloat);

            var values = new float[length];
            for (var i = 0; i < length; i++)
            {
                //values[i] = ReadFloat(endianness);
                values[i] = _data.ReadFloat(_position, endianness);
                _position += BytesExtensions.SizeOfFloat;
            }
            return values;
        }

        /// <inheritdoc />
        public double ReadDouble(Endianness endianness = Endianness.Unspecified)
        {
            var value = ReadDouble(_position, endianness);
            _position += BytesExtensions.SizeOfDouble;
            return value;
        }

        /// <inheritdoc />
        public double ReadDouble(int position, Endianness endianness = Endianness.Unspecified)
        {
            Validate(position, BytesExtensions.SizeOfDouble);
            return _data.ReadDouble(position, endianness.Resolve(DefaultEndianness));
        }

        /// <inheritdoc />
        public double[] ReadDoubleArray(Endianness endianness = Endianness.Unspecified)
        {
            endianness = endianness.Resolve(DefaultEndianness);

            var length = ReadInt(endianness);
            if (length == ArraySerializer.NullArrayLength) return null;
            if (length <= 0) return Array.Empty<double>();

            Validate(_position, length * BytesExtensions.SizeOfDouble);

            var values = new double[length];
            for (var i = 0; i < length; i++)
            {
                values[i] = _data.ReadDouble(_position, endianness);
                _position += BytesExtensions.SizeOfDouble;
            }
            return values;
        }




        /// <inheritdoc />
        public string ReadString()
        {
            var length = ReadInt();
            if (length == ArraySerializer.NullArrayLength) return null;

            // length is the length of the string, in chars
            // each char can be 1, 2 or 3 bytes - surrogate pairs are reported as 2 chars
            // note: this is consistent with Java

            return length == 0 ? "" : _data.ReadUtf8String(ref _position, length);
        }

        /// <inheritdoc />
        public string[] ReadStringArray(Endianness endianness = Endianness.Unspecified)
        {
            endianness = endianness.Resolve(DefaultEndianness);

            var length = ReadInt(endianness);
            if (length == ArraySerializer.NullArrayLength) return null;
            if (length <= 0) return Array.Empty<string>();

            var values = new string[length];
            for (var i = 0; i < length; i++)
            {
                values[i] = ReadString();
            }
            return values;
        }



        /// <inheritdoc />
        public int ReadBytes(byte[] bytes)
        {
            return ReadBytes(bytes, 0, bytes.Length);
        }

        /// <inheritdoc />
        public int ReadBytes(byte[] bytes, int offset, int count)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            if (offset < 0 || offset >= bytes.Length) throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0 || offset + count > bytes.Length) throw new ArgumentOutOfRangeException(nameof(count));

            if (count == 0) return 0;
            if (_position >= _length) return -1;

            count = Math.Min(count, _length - _position);

            Buffer.BlockCopy(_data, _position, bytes, offset, count);
            _position += count;
            return count;
        }

        /// <inheritdoc />
        public int Skip(int count)
        {
            if (count <= 0) return 0;

            count = Math.Min(count, _length - _position);
            _position += count;
            return count;
        }



        /// <inheritdoc />
        public IData ReadData()
        {
            var bytes = ReadByteArray();
            return bytes == null ? null : new HeapData(bytes);
        }

        /// <inheritdoc />
        public T ReadObject<T>()
        {
            return SerializationService.ReadObject<T>(this);
        }



        /// <summary>
        /// Initializes the buffer.
        /// </summary>
        /// <param name="data">The buffer data.</param>
        /// <param name="offset">The buffer data offset.</param>
        public void Initialize(byte[] data, int offset)
        {
            _data = data;
            _position = offset;
            _length = data?.Length ?? 0;
        }

        /// <summary>
        /// Clears the buffer (releases the inner data).
        /// </summary>
        public void Clear()
        {
            _data = null;
            _position = 0;
            _length = 0;
        }



        /// <inheritdoc />
        public void Dispose()
        {
            Clear();
        }
    }
}
