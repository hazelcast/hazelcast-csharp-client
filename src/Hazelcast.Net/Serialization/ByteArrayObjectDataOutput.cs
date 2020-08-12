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

namespace Hazelcast.Serialization
{
    // TODO: globally refactor byte array management

    internal class ByteArrayObjectDataOutput : ByteArrayObjectDataInputOuputBase, IBufferObjectDataOutput
    {
        private byte[] _data;
        private int _length;
        private int _position;
        private readonly int _initialLength;

        internal ByteArrayObjectDataOutput(int size, ISerializationService service, Endianness endianness)
            : base(service, endianness)
        {
            if (size < 0) throw new ArgumentOutOfRangeException(nameof(size));

            _length = _initialLength = size;
            _data = new byte[size];
        }

        // for tests
        internal byte[] Buffer => _data;

        public int Position => _position;

        /// <summary>
        /// Validates that a number of bytes can be written at a given position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="count">The number of bytes.</param>
        private void Validate(int position, int count)
        {
            if (position < 0)
                throw new ArgumentOutOfRangeException(nameof(position));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (_length - position >= count)
                return;

            if (_data == null)
            {
                _length = count > _initialLength / 2 ? count * 2 : _initialLength;
                _data = new byte[_length];
                return;
            }

            // must allocate a new, larger buffer
            // TODO: this is mostly inefficient!
            _length = Math.Max(_length * 2, _length + count);
            var newData = new byte[_length];
            System.Buffer.BlockCopy(_data, 0, newData, 0, _position);
            _data = newData;
        }

        /// <inheritdoc />
        public void Write(byte value)
        {
            Write(_position, value);
            _position += BytesExtensions.SizeOfByte;
        }

        /// <inheritdoc />
        public void Write(int position, byte value)
        {
            Validate(_position, value);
            _data.WriteByte(_position, value);
        }

        /// <inheritdoc />
        public void WriteArray(byte[] bytes, Endianness endianness = Endianness.Unspecified)
        {
            endianness = endianness.Resolve(DefaultEndianness);

            var length = bytes?.Length ?? ArraySerializer.NullArrayLength;
            Write(length, endianness);
            if (bytes == null || length <= 0) return;

            Validate(_position, length * BytesExtensions.SizeOfByte);
            WriteBytes(bytes, 0, length);
        }

        /// <inheritdoc />
        public void Write(short value, Endianness endianness = Endianness.Unspecified)
        {
            Write(_position, value, endianness);
            _position += BytesExtensions.SizeOfShort;
        }

        /// <inheritdoc />
        public void Write(int position, short value, Endianness endianness = Endianness.Unspecified)
        {
            Validate(position, BytesExtensions.SizeOfShort);
            _data.WriteShort(position, value, endianness.Resolve(DefaultEndianness));
        }

        /// <inheritdoc />
        public void WriteArray(short[] values, Endianness endianness = Endianness.Unspecified)
        {
            endianness = endianness.Resolve(DefaultEndianness);

            var length = values?.Length ?? ArraySerializer.NullArrayLength;
            Write(length, endianness);
            if (values == null || length <= 0) return;

            Validate(_position, length * BytesExtensions.SizeOfShort);
            foreach (var value in values)
            {
                _data.WriteShort(_position, value, endianness);
                _position += BytesExtensions.SizeOfShort;
            }
        }

        /// <inheritdoc />
        public void Write(int value, Endianness endianness = Endianness.Unspecified)
        {
            Write(_position, value, endianness);
            _position += BytesExtensions.SizeOfInt;
        }

        /// <inheritdoc />
        public void Write(int position, int value, Endianness endianness = Endianness.Unspecified)
        {
            Validate(position, BytesExtensions.SizeOfInt);
            _data.WriteInt(position, value, endianness.Resolve(DefaultEndianness));
        }

        /// <inheritdoc />
        public void WriteArray(int[] values, Endianness endianness = Endianness.Unspecified)
        {
            endianness = endianness.Resolve(DefaultEndianness);

            var length = values?.Length ?? ArraySerializer.NullArrayLength;
            Write(length, endianness);
            if (values == null || length <= 0) return;

            Validate(_position, length * BytesExtensions.SizeOfInt);
            foreach (var value in values)
            {
                _data.WriteInt(_position, value, endianness);
                _position += BytesExtensions.SizeOfInt;
            }
        }

        /// <inheritdoc />
        public void Write(long value, Endianness endianness = Endianness.Unspecified)
        {
            Write(_position, value, endianness);
            _position += BytesExtensions.SizeOfLong;
        }

        /// <inheritdoc />
        public void Write(int position, long value, Endianness endianness = Endianness.Unspecified)
        {
            Validate(position, BytesExtensions.SizeOfLong);
            _data.WriteLong(position, value, endianness.Resolve(DefaultEndianness));
        }

        /// <inheritdoc />
        public void WriteArray(long[] values, Endianness endianness = Endianness.Unspecified)
        {
            endianness = endianness.Resolve(DefaultEndianness);

            var length = values?.Length ?? ArraySerializer.NullArrayLength;
            Write(length, endianness);
            if (values == null || length <= 0) return;

            foreach (var value in values)
            {
                _data.WriteLong(_position, value, endianness);
                _position += BytesExtensions.SizeOfLong;
            }
        }



        /// <inheritdoc />
        public void Write(float value, Endianness endianness = Endianness.Unspecified)
        {
            Write(_position, value, endianness);
            _position += BytesExtensions.SizeOfFloat;
        }

        /// <inheritdoc />
        public void Write(int position, float value, Endianness endianness = Endianness.Unspecified)
        {
            Validate(position, BytesExtensions.SizeOfFloat);
            _data.WriteFloat(position, value, endianness);
        }

        /// <inheritdoc />
        public void Write(double value, Endianness endianness = Endianness.Unspecified)
        {
            Write(_position, value, endianness);
        }

        /// <inheritdoc />
        public void Write(int position, double value, Endianness endianness = Endianness.Unspecified)
        {
            Validate(position, BytesExtensions.SizeOfDouble);
            _data.WriteDouble(position, value, endianness);
        }

        /// <inheritdoc />
        public void WriteArray(double[] values, Endianness endianness = Endianness.Unspecified)
        {
            endianness = endianness.Resolve(DefaultEndianness);

            var length = values?.Length ?? ArraySerializer.NullArrayLength;
            Write(length, endianness);
            if (values == null || length <= 0) return;

            Validate(_position, length * BytesExtensions.SizeOfDouble);
            foreach (var value in values)
            {
                _data.WriteDouble(_position, value, endianness);
                _position += BytesExtensions.SizeOfDouble;
            }
        }

        /// <inheritdoc />
        public void WriteArray(float[] values, Endianness endianness = Endianness.Unspecified)
        {
            endianness = endianness.Resolve(DefaultEndianness);

            var length = values?.Length ?? ArraySerializer.NullArrayLength;
            Write(length, endianness);
            if (values == null || length <= 0) return;

            Validate(_position, length * BytesExtensions.SizeOfFloat);
            foreach (var value in values)
            {
                _data.WriteFloat(_position, value, endianness);
                _position += BytesExtensions.SizeOfDouble;
            }
        }



        /// <inheritdoc />
        public void Write(bool value)
        {
            Write(_position, value);
        }

        /// <inheritdoc />
        public void Write(int position, bool value)
        {
            Write(position, (byte) (value ? 1 : 0));
        }

        /// <inheritdoc />
        public void WriteArray(bool[] values, Endianness endianness = Endianness.Unspecified)
        {
            endianness = endianness.Resolve(DefaultEndianness);

            var length = values?.Length ?? ArraySerializer.NullArrayLength;
            Write(length, endianness);
            if (values == null || length <= 0) return;

            Validate(_position, length * BytesExtensions.SizeOfBool);
            foreach (var value in values)
            {
                _data.WriteBool(_position, value);
                _position += BytesExtensions.SizeOfBool;
            }
        }



        /// <inheritdoc />
        public void Write(char value, Endianness endianness = Endianness.Unspecified)
        {
            Write(_position, value, endianness);
            _position += BytesExtensions.SizeOfChar;
        }

        /// <inheritdoc />
        public void Write(int position, char value, Endianness endianness = Endianness.Unspecified)
        {
            Validate(position, BytesExtensions.SizeOfChar);
            _data.WriteChar(position, value, endianness);
        }

        /// <inheritdoc />
        public void WriteArray(char[] values, Endianness endianness = Endianness.Unspecified)
        {
            endianness = endianness.Resolve(DefaultEndianness);

            var length = values?.Length ?? ArraySerializer.NullArrayLength;
            Write(length, endianness);
            if (values == null || length <= 0) return;

            Validate(_position, length * BytesExtensions.SizeOfChar);
            foreach (var value in values)
            {
                _data.WriteChar(_position, value, endianness);
                _position += BytesExtensions.SizeOfChar;
            }
        }



        /// <inheritdoc />
        public void WriteAsChars(string value, Endianness endianness = Endianness.Unspecified)
        {
            var length = value.Length;
            Validate(_position, length * BytesExtensions.SizeOfChar);
            for (var i = 0; i < length; i++)
            {
                _data.WriteChar(_position, value[i], endianness);
                _position += BytesExtensions.SizeOfChar;
            }
        }

        /// <inheritdoc />
        public void WriteAsBytes(string s)
        {
            var length = s.Length;
            Validate(_position, length * BytesExtensions.SizeOfByte);
            for (var i = 0; i < length; i++)
            {
                var b = unchecked((byte) s[i]); // TODO: losing data here?!
                _data.WriteByte(_position, b);
                _position += BytesExtensions.SizeOfByte;
            }
        }

        /// <inheritdoc />
        public void Write(string value, Endianness endianness = Endianness.Unspecified)
        {
            endianness = endianness.Resolve(DefaultEndianness);

            var length = value?.Length ?? ArraySerializer.NullArrayLength;
            Write(length, endianness);
            if (value == null || length <= 0) return;

            Validate(_position, length * 3 * BytesExtensions.SizeOfByte); // UTF8 char is max 3 bytes
            for (var i = 0; i < length; i++)
                _data.WriteUtf8Char(ref _position, value[i]);
        }

        /// <inheritdoc />
        public void WriteArray(string[] values, Endianness endianness = Endianness.Unspecified)
        {
            endianness = endianness.Resolve(DefaultEndianness);

            var length = values?.Length ?? ArraySerializer.NullArrayLength;
            Write(length, endianness);
            if (values == null || length <= 0) return;

            foreach (var value in values)
                Write(value);
        }



        /// <inheritdoc />
        public void WriteData(IData data)
        {
            WriteArray(data?.ToByteArray());
        }

        /// <inheritdoc />
        public void WriteObject(object o)
        {
            SerializationService.WriteObject(this, o);
        }

        /// <inheritdoc />
        public byte[] ToByteArray()
        {
            if (_data == null || _position == 0)
                return Array.Empty<byte>();

            // TODO: inefficient
            var newData = new byte[_position];
            System.Buffer.BlockCopy(_data, 0, newData, 0, _position);
            return newData;
        }




        /// <inheritdoc />
        public void WriteBytes(byte[] bytes)
        {
            WriteBytes(bytes, 0, bytes.Length);
        }

        /// <inheritdoc />
        public void WriteBytes(byte[] bytes, int offset, int count)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            if (offset < 0 || offset > bytes.Length) throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0 || offset + count > bytes.Length) throw new ArgumentOutOfRangeException(nameof(count));

            if (count <= 0) return;

            Validate(offset, count * BytesExtensions.SizeOfByte);
            System.Buffer.BlockCopy(bytes, offset, _data, _position, count);
            _position += count * BytesExtensions.SizeOfByte;
        }

        /// <inheritdoc />
        public void WriteZeroBytes(int count)
        {
            Validate(_position, count * BytesExtensions.SizeOfByte);
            for (var i = 0; i < count; i++)
            {
                _data.WriteByte(_position, 0);
                _position += BytesExtensions.SizeOfByte;
            }
        }



        /// <summary>
        /// Clears the buffer.
        /// </summary>
        public void Clear()
        {
            _position = 0;
            // TODO: review byte buffer usage! should be sequences
            if (_data != null && _data.Length > _initialLength * 8)
                _data = new byte[_initialLength * 8];
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Clear();
        }
    }
}
