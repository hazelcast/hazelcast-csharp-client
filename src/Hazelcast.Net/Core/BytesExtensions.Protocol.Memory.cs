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
using Hazelcast.Models;

namespace Hazelcast.Core
{
    internal static partial class BytesExtensions // Protocol for Memory<byte>
    {
        // Write shortcuts — delegate to WriteToMemory.cs methods

        public static void WriteLongL(this Memory<byte> bytes, int position, long value)
            => bytes.WriteLong(position, value, Endianness.LittleEndian);

        public static void WriteIntL(this Memory<byte> bytes, int position, int value)
            => bytes.WriteInt(position, value, Endianness.LittleEndian);

        public static void WriteIntL(this Memory<byte> bytes, int position, Enum value)
            => bytes.WriteInt(position, (int)(object)value, Endianness.LittleEndian);

        public static void WriteShortL(this Memory<byte> bytes, int position, short value)
            => bytes.WriteShort(position, value, Endianness.LittleEndian);

        public static void WriteBoolL(this Memory<byte> bytes, int position, bool value)
            => bytes.WriteBool(position, value);

        public static void WriteByteL(this Memory<byte> bytes, int position, byte value)
            => bytes.WriteByte(position, value);

        public static void WriteFloatL(this Memory<byte> bytes, int position, float value)
            => bytes.WriteFloat(position, value, Endianness.LittleEndian);

        public static void WriteGuidL(this Memory<byte> bytes, int position, Guid value, bool withEmptyFlag = true)
            => bytes.WriteGuid(position, value, Endianness.LittleEndian, withEmptyFlag);

        public static void WriteGuid(this Memory<byte> bytes, int position, Guid value, Endianness endianness, bool withEmptyFlag = true)
        {
            if (withEmptyFlag)
            {
                bytes.WriteBool(position++, value == Guid.Empty);
                if (value == Guid.Empty) return;
            }

            new JavaUuidOrder { Value = value }.WriteBytes(bytes.Span, position, endianness);
        }

        // Basic read methods — delegate via ReadOnlySpan<byte>

        public static byte ReadByte(this Memory<byte> bytes, int position)
        {
            ReadOnlySpan<byte> span = bytes.Span;
            return span.Slice(position).ReadByte();
        }

        public static int ReadInt(this Memory<byte> bytes, int position, Endianness endianness)
        {
            ReadOnlySpan<byte> span = bytes.Span;
            return span.Slice(position).ReadInt(endianness);
        }

        public static long ReadLong(this Memory<byte> bytes, int position, Endianness endianness)
        {
            ReadOnlySpan<byte> span = bytes.Span;
            return span.Slice(position).ReadLong(endianness);
        }

        public static float ReadFloat(this Memory<byte> bytes, int position, Endianness endianness)
        {
            ReadOnlySpan<byte> span = bytes.Span;
            return span.Slice(position).ReadFloat(endianness);
        }

        public static bool ReadBool(this Memory<byte> bytes, int position)
        {
            ReadOnlySpan<byte> span = bytes.Span;
            return span.Slice(position).ReadBool();
        }

        // Read shortcuts — delegate via ReadOnlySpan<byte>

        public static short ReadShortL(this Memory<byte> bytes, int position)
        {
            ReadOnlySpan<byte> span = bytes.Span;
            return span.Slice(position).ReadShort(Endianness.LittleEndian);
        }

        public static long ReadLongL(this Memory<byte> bytes, int position)
        {
            ReadOnlySpan<byte> span = bytes.Span;
            return span.Slice(position).ReadLong(Endianness.LittleEndian);
        }

        public static int ReadIntL(this Memory<byte> bytes, int position)
        {
            ReadOnlySpan<byte> span = bytes.Span;
            return span.Slice(position).ReadInt(Endianness.LittleEndian);
        }

        public static float ReadFloatL(this Memory<byte> bytes, int position)
        {
            ReadOnlySpan<byte> span = bytes.Span;
            return span.Slice(position).ReadFloat(Endianness.LittleEndian);
        }

        public static double ReadDoubleL(this Memory<byte> bytes, int position)
        {
            ReadOnlySpan<byte> span = bytes.Span;
            return span.Slice(position).ReadDouble(Endianness.LittleEndian);
        }

        public static bool ReadBoolL(this Memory<byte> bytes, int position)
        {
            ReadOnlySpan<byte> span = bytes.Span;
            return span.Slice(position).ReadBool();
        }

        public static byte ReadByteL(this Memory<byte> bytes, int position)
        {
            ReadOnlySpan<byte> span = bytes.Span;
            return span.Slice(position).ReadByte();
        }

        public static Guid ReadGuidL(this Memory<byte> bytes, int position, bool withEmptyFlag = true)
            => bytes.ReadGuid(position, Endianness.LittleEndian, withEmptyFlag);

        public static Guid ReadGuid(this Memory<byte> bytes, int position, Endianness endianness, bool withEmptyFlag = true)
        {
            ReadOnlySpan<byte> span = bytes.Span;
            if (withEmptyFlag)
            {
                if (span.Slice(position++).ReadBool()) return Guid.Empty;
            }

            return new JavaUuidOrder().ReadBytes(span, position, endianness).Value;
        }

        // Date/time overloads matching DecodeBytesDelegate<T>(Memory<byte>, int)

        public static HLocalDate ReadLocalDate(this Memory<byte> bytes, int position)
            => ((ReadOnlyMemory<byte>)bytes).ReadLocalDate(position);

        public static HLocalTime ReadLocalTime(this Memory<byte> bytes, int position)
            => ((ReadOnlyMemory<byte>)bytes).ReadLocalTime(position);

        public static HLocalDateTime ReadLocalDateTime(this Memory<byte> bytes, int position)
            => ((ReadOnlyMemory<byte>)bytes).ReadLocalDateTime(position);

        public static HOffsetDateTime ReadOffsetDateTime(this Memory<byte> bytes, int position)
            => ((ReadOnlyMemory<byte>)bytes).ReadOffsetDateTime(position);
    }
}
