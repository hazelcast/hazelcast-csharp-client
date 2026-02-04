// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Hazelcast.Core;
using Hazelcast.Polyfills;
namespace Hazelcast.Serialization
{
    internal sealed partial class SegmentedObjectDataOutput //APIs
    {

        public void WriteBoolean(bool value)
        {
            WriteByte((byte) (value ? 1 : 0));
        }
        public void WriteByte(byte value)
        {
            var span = GetSpanForPrimitive(BytesExtensions.SizeOfByte);
            span.WriteByte(0, value);
        }
        public void WriteSByte(sbyte value)
        {
            var span = GetSpanForPrimitive(BytesExtensions.SizeOfByte);
            span.WriteSbyte(0, value);
        }
        public void WriteChar(char value)
        {
            var span = GetSpanForPrimitive(BytesExtensions.SizeOfChar);
            span.WriteChar(0, value, Endianness);
        }
        public void WriteShort(short value)
        {
            var span = GetSpanForPrimitive(BytesExtensions.SizeOfShort);
            span.WriteShort(0, value, Endianness);
        }
        public void WriteUShort(ushort value)
        {
            var span = GetSpanForPrimitive(BytesExtensions.SizeOfShort);
            span.WriteUShort(0, value, Endianness);
        }
        public void WriteInt(int value)
        {
            var span = GetSpanForPrimitive(BytesExtensions.SizeOfInt);
            span.WriteInt(0, value, Endianness);
        }

        public void WriteInt(int position, int value)
        {
            //requirement: position is absolute position from the beginning of the output
        }
        public void WriteIntBigEndian(int value)
        {
            var span = GetSpanForPrimitive(BytesExtensions.SizeOfInt);
            span.WriteInt(0, value, Endianness.BigEndian);
        }

        public void WriteBits(byte bits, byte mask)
        {
            var span = GetSpanForPrimitive(BytesExtensions.SizeOfByte);
            span.WriteBits(0, bits, mask);
        }

        public void WriteZeroBytes(int count)
        {
            var span = GetSpan(BytesExtensions.SizeOfByte * count);
            span.Slice(0, count).Clear();
            Advance(count * BytesExtensions.SizeOfByte);
        }

        public void WriteLong(long value)
        {
            var span = GetSpanForPrimitive(BytesExtensions.SizeOfLong);
            span.WriteLong(0, value, Endianness);
        }
        public void WriteFloat(float value)
        {
            var span = GetSpanForPrimitive(BytesExtensions.SizeOfFloat);
            span.WriteFloat(0, value, Endianness);
        }
        public void WriteDouble(double value)
        {
            var span = GetSpanForPrimitive(BytesExtensions.SizeOfDouble);
            span.WriteDouble(0, value, Endianness);
        }
        public void WriteString(string value)
        {
            var byteCount = value != null ? Encoding.UTF8.GetByteCount(value) : BytesExtensions.SizeOfNullArray;
            WriteInt(byteCount);

            if (byteCount > 0)
            {
                // We want to write big strings into multiple segments if needed

                if (byteCount <= SizeLeftInCurrentChunk)
                {
                    // Fits in current chunk
                    var span = GetSpan(byteCount);
                    Encoding.UTF8.GetBytes(value.AsSpan(), span);
                    Advance(byteCount);
                }
                else
                {
                    // Does not fit in current chunk
                    // Write in fragments
                    WriteFragmentedString(value.AsSpan());
                }
            }
        }
        public void WriteChars(string value)
        {
            var span = GetSpan(value.Length * BytesExtensions.SizeOfChar);
            for (var i = 0; i < value.Length; i++)
            {
                // TODO: possible optimization  with BinaryPrimitives.ReverseEndianness and MemoryMarshal.Cast
                span.WriteChar(i * BytesExtensions.SizeOfChar, value[i], Endianness);
            }
        }
        public void WriteBytes(string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            var source = value.AsSpan();

            while (!source.IsEmpty)
            {
                var destination = GetSpan();

                if (destination.Length == 0)
                {
                    destination = GetSpan(BytesExtensions.SizeOfChar); // size of char
                }

                var count = Math.Min(source.Length, destination.Length);
                CastAndCopyCharsToBytes(source.Slice(0, count), destination.Slice(0, count));
                Advance(count);
                source = source.Slice(count);
            }
        }
        public void WriteBooleanArray(bool[] values)
        {
            var length = values?.Length ?? BytesExtensions.SizeOfNullArray;
            WriteInt(length);
            if (length <= 0) return;

            // Boolean is already one byte in .NET, so we can write directly
            ReadOnlySpan<bool> boolSpan = values.AsSpan();
            ReadOnlySpan<byte> byteSpan = MemoryMarshal.AsBytes(boolSpan);
            Write(byteSpan);
        }
        public void WriteByteArray(byte[] bytes)
        {
            var length = bytes?.Length ?? BytesExtensions.SizeOfNullArray;
            WriteInt(length);
            if (bytes == null || length <= 0) return;

            Write(bytes.AsSpan());
        }
        public void WriteCharArray(char[] values)
        {
            var length = values?.Length ?? BytesExtensions.SizeOfNullArray;
            WriteInt(length);
            if (length <= 0) return;

            var spanValues = values.AsSpan();

            while (!spanValues.IsEmpty)
            {
                var destination = GetSpan();

                if (destination.Length == 0 || destination.Length < BytesExtensions.SizeOfChar)
                {
                    destination = GetSpan(BytesExtensions.SizeOfChar);
                }

                var count = Math.Min(spanValues.Length, destination.Length / BytesExtensions.SizeOfChar);
                for (var i = 0; i < count; i++)
                {
                    destination.WriteChar(i * BytesExtensions.SizeOfChar, spanValues[i], Endianness);
                }
                Advance(count * BytesExtensions.SizeOfChar);
                spanValues = spanValues.Slice(count);
            }
        }
        public void WriteShortArray(short[] values)
        {
            var length = values?.Length ?? BytesExtensions.SizeOfNullArray;
            WriteInt(length);
            if (length <= 0) return;

            var spanValues = values.AsSpan();

            while (!spanValues.IsEmpty)
            {
                var destination = GetSpan();

                if (destination.Length == 0 || destination.Length < BytesExtensions.SizeOfShort)
                {
                    destination = GetSpan(BytesExtensions.SizeOfShort);
                }

                var count = Math.Min(spanValues.Length, destination.Length / BytesExtensions.SizeOfShort);
                for (var i = 0; i < count; i++)
                {
                    destination.WriteShort(i * BytesExtensions.SizeOfShort, spanValues[i], Endianness);
                }
                Advance(count * BytesExtensions.SizeOfShort);
                spanValues = spanValues.Slice(count);
            }
        }
        public void WriteIntArray(int[] values)
        {
            var length = values?.Length ?? BytesExtensions.SizeOfNullArray;
            WriteInt(length);
            if (length <= 0) return;

            var spanValues = values.AsSpan();

            while (!spanValues.IsEmpty)
            {
                var destination = GetSpan();

                if (destination.Length == 0 || destination.Length < BytesExtensions.SizeOfInt)
                {
                    destination = GetSpan(BytesExtensions.SizeOfInt);
                }

                var count = Math.Min(spanValues.Length, destination.Length / BytesExtensions.SizeOfInt);
                for (var i = 0; i < count; i++)
                {
                    destination.WriteInt(i * BytesExtensions.SizeOfInt, spanValues[i], Endianness);
                }
                Advance(count * BytesExtensions.SizeOfInt);
                spanValues = spanValues.Slice(count);
            }
        }
        public void WriteLongArray(long[] values)
        {
            var length = values?.Length ?? BytesExtensions.SizeOfNullArray;
            WriteInt(length);
            if (length <= 0) return;

            var spanValues = values.AsSpan();

            while (!spanValues.IsEmpty)
            {
                var destination = GetSpan();

                if (destination.Length == 0 || destination.Length < BytesExtensions.SizeOfLong)
                {
                    destination = GetSpan(BytesExtensions.SizeOfLong);
                }

                var count = Math.Min(spanValues.Length, destination.Length / BytesExtensions.SizeOfLong);
                for (var i = 0; i < count; i++)
                {
                    destination.WriteLong(i * BytesExtensions.SizeOfLong, spanValues[i], Endianness);
                }
                Advance(count * BytesExtensions.SizeOfLong);
                spanValues = spanValues.Slice(count);
            }
        }
        public void WriteFloatArray(float[] values)
        {
            var length = values?.Length ?? BytesExtensions.SizeOfNullArray;
            WriteInt(length);
            if (length <= 0) return;

            var spanValues = values.AsSpan();

            while (!spanValues.IsEmpty)
            {
                var destination = GetSpan();

                if (destination.Length == 0 || destination.Length < BytesExtensions.SizeOfFloat)
                {
                    destination = GetSpan(BytesExtensions.SizeOfFloat);
                }

                var count = Math.Min(spanValues.Length, destination.Length / BytesExtensions.SizeOfFloat);
                for (var i = 0; i < count; i++)
                {
                    destination.WriteFloat(i * BytesExtensions.SizeOfFloat, spanValues[i], Endianness);
                }
                Advance(count * BytesExtensions.SizeOfFloat);
                spanValues = spanValues.Slice(count);
            }
        }
        public void WriteDoubleArray(double[] values)
        {
            var length = values?.Length ?? BytesExtensions.SizeOfNullArray;
            WriteInt(length);
            if (length <= 0) return;

            var spanValues = values.AsSpan();

            while (!spanValues.IsEmpty)
            {
                var destination = GetSpan();

                if (destination.Length == 0 || destination.Length < BytesExtensions.SizeOfDouble)
                {
                    destination = GetSpan(BytesExtensions.SizeOfDouble);
                }

                var count = Math.Min(spanValues.Length, destination.Length / BytesExtensions.SizeOfDouble);
                for (var i = 0; i < count; i++)
                {
                    destination.WriteDouble(i * BytesExtensions.SizeOfDouble, spanValues[i], Endianness);
                }
                Advance(count * BytesExtensions.SizeOfDouble);
                spanValues = spanValues.Slice(count);
            }
        }
        public void WriteStringArray(string[] values)
        {
            var length = values?.Length ?? BytesExtensions.SizeOfNullArray;
            WriteInt(length);
            if (length <= 0) return;

            foreach (var str in values!)
            {
                WriteString(str);
            }
        }
        public void WriteObject(object value)
        {
            _objectsWriter.Write(this, value);
        }
        public void Write(byte[] bytes)
        {
            Write(bytes.AsSpan());
        }
        public void Write(byte[] bytes, int offset, int count)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            if (offset < 0 || offset > bytes.Length) throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0 || offset + count > bytes.Length) throw new ArgumentOutOfRangeException(nameof(count));

            if (count <= 0) return;

            Write(bytes.AsSpan(offset, count));
        }
        public byte[] ToByteArray(int padding = 0) => GetSequence().ToArray();
        public bool HasSchemas => _schemaIds != null;
        public HashSet<long> SchemaIds => _schemaIds ??= new HashSet<long>();
    }
}
