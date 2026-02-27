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
    internal static partial class BytesExtensions // Write to Memory<byte>
    {
        /// <summary>
        /// Writes a bits value to a memory of bytes.
        /// </summary>
        /// <param name="bytes">The memory of bytes to write to.</param>
        /// <param name="position">The position in the memory where the value should be written.</param>
        /// <param name="bits">The bits to write.</param>
        /// <param name="mask">The mask for selecting which bits to write.</param>
        public static void WriteBits(this Memory<byte> bytes, int position, byte bits, byte mask)
            => bytes.Span.Slice(position).WriteBits(bits, mask);

        /// <summary>
        /// Writes a <see cref="byte"/> value to a memory of bytes.
        /// </summary>
        /// <param name="bytes">The memory of bytes to write to.</param>
        /// <param name="position">The position in the memory where the value should be written.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteByte(this Memory<byte> bytes, int position, byte value)
            => bytes.Span.Slice(position).WriteByte(value);

        /// <summary>
        /// Writes a <see cref="sbyte"/> value to a memory of bytes.
        /// </summary>
        /// <param name="bytes">The memory of bytes to write to.</param>
        /// <param name="position">The position in the memory where the value should be written.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteSbyte(this Memory<byte> bytes, int position, sbyte value)
            => bytes.Span.Slice(position).WriteSbyte(value);

        /// <summary>
        /// Writes a <see cref="short"/> value to a memory of bytes.
        /// </summary>
        /// <param name="bytes">The memory of bytes to write to.</param>
        /// <param name="position">The position in the memory where the value should be written.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="endianness">The endianness.</param>
        public static void WriteShort(this Memory<byte> bytes, int position, short value, Endianness endianness)
            => bytes.Span.Slice(position).WriteShort(value, endianness);

        /// <summary>
        /// Writes an <see cref="ushort"/> value to a memory of bytes.
        /// </summary>
        /// <param name="bytes">The memory of bytes to write to.</param>
        /// <param name="position">The position in the memory where the value should be written.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="endianness">The endianness.</param>
        public static void WriteUShort(this Memory<byte> bytes, int position, ushort value, Endianness endianness)
            => bytes.Span.Slice(position).WriteUShort(value, endianness);

        /// <summary>
        /// Writes an <see cref="int"/> value to a memory of bytes.
        /// </summary>
        /// <param name="bytes">The memory of bytes to write to.</param>
        /// <param name="position">The position in the memory where the value should be written.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="endianness">The endianness.</param>
        public static void WriteInt(this Memory<byte> bytes, int position, int value, Endianness endianness)
            => bytes.Span.Slice(position).WriteInt(value, endianness);

        /// <summary>
        /// Writes a <see cref="long"/> value to a memory of bytes.
        /// </summary>
        /// <param name="bytes">The memory of bytes to write to.</param>
        /// <param name="position">The position in the memory where the value should be written.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="endianness">The endianness.</param>
        public static void WriteLong(this Memory<byte> bytes, int position, long value, Endianness endianness)
            => bytes.Span.Slice(position).WriteLong(value, endianness);

        /// <summary>
        /// Writes a <see cref="float"/> value to a memory of bytes.
        /// </summary>
        /// <param name="bytes">The memory of bytes to write to.</param>
        /// <param name="position">The position in the memory where the value should be written.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="endianness">The endianness.</param>
        public static void WriteFloat(this Memory<byte> bytes, int position, float value, Endianness endianness)
            => bytes.Span.Slice(position).WriteFloat(value, endianness);

        /// <summary>
        /// Writes a <see cref="double"/> value to a memory of bytes.
        /// </summary>
        /// <param name="bytes">The memory of bytes to write to.</param>
        /// <param name="position">The position in the memory where the value should be written.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="endianness">The endianness.</param>
        public static void WriteDouble(this Memory<byte> bytes, int position, double value, Endianness endianness)
            => bytes.Span.Slice(position).WriteDouble(value, endianness);

        /// <summary>
        /// Writes a <see cref="bool"/> value to a memory of bytes.
        /// </summary>
        /// <param name="bytes">The memory of bytes to write to.</param>
        /// <param name="position">The position in the memory where the value should be written.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteBool(this Memory<byte> bytes, int position, bool value)
            => bytes.Span.Slice(position).WriteBool(value);

        /// <summary>
        /// Writes a <see cref="char"/> value to a memory of bytes.
        /// </summary>
        /// <param name="bytes">The memory of bytes to write to.</param>
        /// <param name="position">The position in the memory where the value should be written.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="endianness">The endianness.</param>
        public static void WriteChar(this Memory<byte> bytes, int position, char value, Endianness endianness)
            => bytes.Span.Slice(position).WriteChar(value, endianness);

        public static void WriteLocalDate(this Memory<byte> bytes, int position, HLocalDate localDate)
            => bytes.Span.Slice(position).WriteLocalDate(localDate);

        public static void WriteLocalTime(this Memory<byte> bytes, int position, HLocalTime localTime)
            => bytes.Span.Slice(position).WriteLocalTime(localTime);

        public static void WriteLocalDateTime(this Memory<byte> bytes, int position, HLocalDateTime localDateTime)
            => bytes.Span.Slice(position).WriteLocalDateTime(localDateTime);

        public static void WriteOffsetDateTime(this Memory<byte> bytes, int position, HOffsetDateTime offsetDateTime)
            => bytes.Span.Slice(position).WriteOffsetDateTime(offsetDateTime);
    }
}
