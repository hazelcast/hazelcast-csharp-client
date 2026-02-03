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
using System.Collections.Generic;
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
            span.WriteByte(value, 0);
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
        { }

        public void WriteBits(byte bits, byte mask)
        { }

        public void WriteZeroBytes(int count)
        { }

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
                    Encoding.UTF8.GetBytes(value.AsSpan(),  span);
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
            var span = GetSpan(value.Length);
            for (var i = 0; i < value.Length; i++)
            {
                span.WriteByte(i, (byte) value[i]);
            }
        }
        public void WriteBooleanArray(bool[] values)
        {
            throw new System.NotImplementedException();
        }
        public void WriteByteArray(byte[] values)
        {
            throw new System.NotImplementedException();
        }
        public void WriteCharArray(char[] values)
        {
            throw new System.NotImplementedException();
        }
        public void WriteShortArray(short[] values)
        {
            throw new System.NotImplementedException();
        }
        public void WriteIntArray(int[] values)
        {
            throw new System.NotImplementedException();
        }
        public void WriteLongArray(long[] values)
        {
            throw new System.NotImplementedException();
        }
        public void WriteFloatArray(float[] values)
        {
            throw new System.NotImplementedException();
        }
        public void WriteDoubleArray(double[] values)
        {
            throw new System.NotImplementedException();
        }
        public void WriteStringArray(string[] values)
        {
            throw new System.NotImplementedException();
        }
        public void WriteObject(object value)
        {
            throw new System.NotImplementedException();
        }
        public void Write(byte[] bytes)
        {
            throw new System.NotImplementedException();
        }
        public void Write(byte[] bytes, int offset, int count)
        {
            throw new System.NotImplementedException();
        }
        public byte[] ToByteArray(int padding = 0)
            => throw new System.NotImplementedException();
        public bool HasSchemas
        {
            get;
        }
        public HashSet<long> SchemaIds
        {
            get;
        }
    }
}
