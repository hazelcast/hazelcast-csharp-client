// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Models;

namespace Hazelcast.Serialization
{
    internal partial class ObjectDataOutput //public api
    {
        public Endianness Endianness { get; }

        public void WriteBoolean(bool value)
        {
            WriteByte((byte) (value ? 1 : 0));
        }

        public void WriteByte(byte value)
        {
            EnsureAvailable(BytesExtensions.SizeOfByte);
            _buffer.WriteByte(_position, value);
            _position += BytesExtensions.SizeOfByte;
        }

        public void WriteSByte(sbyte value)
        {
            EnsureAvailable(BytesExtensions.SizeOfByte);
            _buffer.WriteSbyte(_position, value);
            _position += BytesExtensions.SizeOfByte;
        }

        public void WriteChar(char value)
        {
            EnsureAvailable(BytesExtensions.SizeOfChar);
            _buffer.WriteChar(_position, value, Endianness);
            _position += BytesExtensions.SizeOfChar;
        }

        public void WriteShort(short value)
        {
            EnsureAvailable(BytesExtensions.SizeOfShort);
            _buffer.WriteShort(_position, value, Endianness);
            _position += BytesExtensions.SizeOfShort;
        }

        public void WriteUShort(ushort value)
        {
            EnsureAvailable(BytesExtensions.SizeOfShort);
            _buffer.WriteUShort(_position, value, Endianness);
            _position += BytesExtensions.SizeOfShort;
        }

        public void WriteInt(int value)
        {
            EnsureAvailable(BytesExtensions.SizeOfInt);
            _buffer.WriteInt(_position, value, Endianness);
            _position += BytesExtensions.SizeOfInt;
        }

        public void WriteLong(long value)
        {
            EnsureAvailable(BytesExtensions.SizeOfLong);
            _buffer.WriteLong(_position, value, Endianness);
            _position += BytesExtensions.SizeOfLong;
        }

        public void WriteFloat(float value)
        {
            EnsureAvailable(BytesExtensions.SizeOfFloat);
            _buffer.WriteFloat(_position, value, Endianness);
            _position += BytesExtensions.SizeOfFloat;
        }

        public void WriteDouble(double value)
        {
            EnsureAvailable(BytesExtensions.SizeOfDouble);
            _buffer.WriteDouble(_position, value, Endianness);
            _position += BytesExtensions.SizeOfDouble;
        }

        public void WriteBigDecimal(decimal value)
        {
            throw new NotImplementedException(); // FIXME - implement WriteBigDecimal
        }

        public void WriteBigDecimal(HBigDecimal value)
        {
            throw new NotImplementedException(); // FIXME - implement WriteBigDecimal
        }

        public void WriteString(string value)
        {
            var byteCount = value != null ? Encoding.UTF8.GetByteCount(value) : BytesExtensions.SizeOfNullArray;
            WriteInt(byteCount);
            if (byteCount <= 0) return;

            EnsureAvailable(byteCount);
            _position += Encoding.UTF8.GetBytes(value, 0, value.Length, _buffer, _position);
        }

        public void WriteChars(string value)
        {
            var len = value.Length;
            EnsureAvailable(len * BytesExtensions.SizeOfChar);
            for (var i = 0; i < len; i++)
            {
                _buffer.WriteChar(_position, value[i], Endianness);
                _position += BytesExtensions.SizeOfChar;
            }
        }

        public void WriteBytes(string value)
        {
            var len = value.Length;
            EnsureAvailable(len);
            for (var i = 0; i < len; i++)
            {
                _buffer.WriteByte(_position, (byte) value[i]);
                _position += BytesExtensions.SizeOfByte;
            }
        }

        public void WriteBooleanArray(bool[] values)
        {
            var length = values?.Length ?? BytesExtensions.SizeOfNullArray;
            WriteInt(length);
            if (length <= 0) return;

            EnsureAvailable(length * BytesExtensions.SizeOfBool);
            foreach (var value in values)
            {
                _buffer.WriteBool(_position, value);
                _position += BytesExtensions.SizeOfBool;
            }
        }

        public void WriteByteArray(byte[] bytes)
        {
            var length = bytes?.Length ?? BytesExtensions.SizeOfNullArray;
            WriteInt(length);
            if (bytes == null || length <= 0) return;

            EnsureAvailable(length * BytesExtensions.SizeOfByte);
            Write(bytes);
        }

        public void WriteSByteArray(sbyte[] bytes)
        {
            var length = bytes?.Length ?? BytesExtensions.SizeOfNullArray;
            WriteInt(length);
            if (bytes == null || length <= 0) return;

            EnsureAvailable(length * BytesExtensions.SizeOfByte);
            Write(bytes);
        }

        public void WriteCharArray(char[] values)
        {
            var length = values?.Length ?? BytesExtensions.SizeOfNullArray;
            WriteInt(length);
            if (length <= 0) return;

            EnsureAvailable(length * BytesExtensions.SizeOfChar);
            foreach (var value in values)
            {
                _buffer.WriteChar(_position, value, Endianness);
                _position += BytesExtensions.SizeOfChar;
            }
        }

        public void WriteShortArray(short[] values)
        {
            var length = values?.Length ?? BytesExtensions.SizeOfNullArray;
            WriteInt(length);
            if (length <= 0) return;

            EnsureAvailable(length * BytesExtensions.SizeOfShort);
            foreach (var value in values)
            {
                _buffer.WriteShort(_position, value, Endianness);
                _position += BytesExtensions.SizeOfShort;
            }
        }

        public void WriteIntArray(int[] values)
        {
            var length = values?.Length ?? BytesExtensions.SizeOfNullArray;
            WriteInt(length);
            if (length <= 0) return;

            EnsureAvailable(length * BytesExtensions.SizeOfInt);
            foreach (var value in values)
            {
                _buffer.WriteInt(_position, value, Endianness);
                _position += BytesExtensions.SizeOfInt;
            }
        }

        public void WriteLongArray(long[] values)
        {
            var length = values?.Length ?? BytesExtensions.SizeOfNullArray;
            WriteInt(length);
            if (length <= 0) return;

            EnsureAvailable(length * BytesExtensions.SizeOfLong);
            foreach (var value in values)
            {
                _buffer.WriteLong(_position, value, Endianness);
                _position += BytesExtensions.SizeOfLong;
            }
        }

        public void WriteFloatArray(float[] values)
        {
            var length = values?.Length ?? BytesExtensions.SizeOfNullArray;
            WriteInt(length);
            if (length <= 0) return;

            EnsureAvailable(length * BytesExtensions.SizeOfFloat);
            foreach (var value in values)
            {
                _buffer.WriteFloat(_position, value, Endianness);
                _position += BytesExtensions.SizeOfFloat;
            }
        }

        public void WriteDoubleArray(double[] values)
        {
            var length = values?.Length ?? BytesExtensions.SizeOfNullArray;
            WriteInt(length);
            if (length <= 0) return;

            EnsureAvailable(length * BytesExtensions.SizeOfDouble);
            foreach (var value in values)
            {
                _buffer.WriteDouble(_position, value, Endianness);
                _position += BytesExtensions.SizeOfDouble;
            }
        }

        public void WriteStringArray(string[] values)
        {
            var length = values?.Length ?? BytesExtensions.SizeOfNullArray;
            WriteInt(length);
            if (length <= 0) return;

            foreach (var value in values)
            {
                WriteString(value);
            }
        }

        public void WriteObject(object value)
        {
            _serializationService.WriteObject(this, value, false);
        }

        public void Write(byte[] bytes)
        {
            Write(bytes, 0, bytes.Length);
        }

        public void Write(byte[] bytes, int offset, int count)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            if (offset < 0 || offset > bytes.Length) throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0 || offset + count > bytes.Length) throw new ArgumentOutOfRangeException(nameof(count));

            if (count <= 0) return;

            EnsureAvailable(count);
            System.Buffer.BlockCopy(bytes, offset, _buffer, _position, count);
            _position += count;
        }

        public void Write(sbyte[] bytes)
        {
            Write(bytes, 0, bytes.Length);
        }

        public void Write(sbyte[] bytes, int offset, int count)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            if (offset < 0 || offset > bytes.Length) throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0 || offset + count > bytes.Length) throw new ArgumentOutOfRangeException(nameof(count));

            if (count <= 0) return;

            EnsureAvailable(count);
            System.Buffer.BlockCopy(bytes, offset, _buffer, _position, count);
            _position += count;
        }

        public byte[] ToByteArray(int padding = 0)
        {
            if (_buffer == null || _position == 0)
                return Array.Empty<byte>();

            // TODO: inefficient
            var newData = new byte[_position];
            System.Buffer.BlockCopy(_buffer, 0, newData, 0, _position);
            return newData;
        }
    }
}
