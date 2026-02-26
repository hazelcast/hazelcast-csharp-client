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
    internal static partial class BytesExtensions // Read from ReadOnlyMemory<byte>
    {
        /// <summary>
        /// Verifies that it is possible to read <paramref name="count"/> at position <paramref name="position"/>
        /// of the <paramref name="bytes"/> memory.
        /// </summary>
        /// <param name="bytes">The memory of bytes to read from.</param>
        /// <param name="position">The position.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The memory of bytes to read from.</returns>
        public static ReadOnlyMemory<byte> CanRead(this ReadOnlyMemory<byte> bytes, int position, int count)
        {
            if (position + count <= bytes.Length) return bytes;
            throw new OverflowException($"Cannot read {count} bytes at {position} of {bytes.Length} bytes array.");
        }

        /// <summary>
        /// Reads a <see cref="byte"/> value from a memory of bytes.
        /// </summary>
        /// <param name="bytes">The memory of bytes to read from.</param>
        /// <param name="position">The position in the memory where the value should be read.</param>
        /// <returns>The value.</returns>
        public static byte ReadByte(this ReadOnlyMemory<byte> bytes, int position)
            => bytes.Span.Slice(position).ReadByte();

        public static sbyte ReadSByte(this ReadOnlyMemory<byte> bytes, int position)
            => bytes.Span.Slice(position).ReadSByte();

        /// <summary>
        /// Reads a <see cref="short"/> value from a memory of bytes.
        /// </summary>
        /// <param name="bytes">The memory of bytes to read from.</param>
        /// <param name="position">The position in the memory where the value should be read.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        public static short ReadShort(this ReadOnlyMemory<byte> bytes, int position, Endianness endianness)
            => bytes.Span.Slice(position).ReadShort(endianness);

        /// <summary>
        /// Reads an <see cref="ushort"/> value from a memory of bytes.
        /// </summary>
        /// <param name="bytes">The memory of bytes to read from.</param>
        /// <param name="position">The position in the memory where the value should be read.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        public static ushort ReadUShort(this ReadOnlyMemory<byte> bytes, int position, Endianness endianness)
            => bytes.Span.Slice(position).ReadUShort(endianness);

        /// <summary>
        /// Reads an <see cref="int"/> value from a memory of bytes.
        /// </summary>
        /// <param name="bytes">The memory of bytes to read from.</param>
        /// <param name="position">The position in the memory where the value should be read.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        public static int ReadInt(this ReadOnlyMemory<byte> bytes, int position, Endianness endianness)
            => bytes.Span.Slice(position).ReadInt(endianness);

        /// <summary>
        /// Reads a <see cref="long"/> value from a memory of bytes.
        /// </summary>
        /// <param name="bytes">The memory of bytes to read from.</param>
        /// <param name="position">The position in the memory where the value should be read.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        public static long ReadLong(this ReadOnlyMemory<byte> bytes, int position, Endianness endianness)
            => bytes.Span.Slice(position).ReadLong(endianness);

        /// <summary>
        /// Reads a <see cref="float"/> value from a memory of bytes.
        /// </summary>
        /// <param name="bytes">The memory of bytes to read from.</param>
        /// <param name="position">The position in the memory where the value should be read.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        public static float ReadFloat(this ReadOnlyMemory<byte> bytes, int position, Endianness endianness)
            => bytes.Span.Slice(position).ReadFloat(endianness);

        /// <summary>
        /// Reads a <see cref="double"/> value from a memory of bytes.
        /// </summary>
        /// <param name="bytes">The memory of bytes to read from.</param>
        /// <param name="position">The position in the memory where the value should be read.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        public static double ReadDouble(this ReadOnlyMemory<byte> bytes, int position, Endianness endianness)
            => bytes.Span.Slice(position).ReadDouble(endianness);

        /// <summary>
        /// Reads a <see cref="bool"/> value from a memory of bytes.
        /// </summary>
        /// <param name="bytes">The memory of bytes to read from.</param>
        /// <param name="position">The position in the memory where the value should be read.</param>
        /// <returns>The value.</returns>
        public static bool ReadBool(this ReadOnlyMemory<byte> bytes, int position)
            => bytes.Span.Slice(position).ReadBool();

        /// <summary>
        /// Reads a <see cref="char"/> value from a memory of bytes.
        /// </summary>
        /// <param name="bytes">The memory of bytes to read from.</param>
        /// <param name="position">The position in the memory where the value should be read.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        public static char ReadChar(this ReadOnlyMemory<byte> bytes, int position, Endianness endianness)
            => bytes.Span.Slice(position).ReadChar(endianness);

        public static HLocalDate ReadLocalDate(this ReadOnlyMemory<byte> bytes, int position)
            => bytes.Span.Slice(position).ReadLocalDate();

        public static HLocalTime ReadLocalTime(this ReadOnlyMemory<byte> bytes, int position)
            => bytes.Span.Slice(position).ReadLocalTime();

        public static HLocalDateTime ReadLocalDateTime(this ReadOnlyMemory<byte> bytes, int position)
            => bytes.Span.Slice(position).ReadLocalDateTime();

        public static HOffsetDateTime ReadOffsetDateTime(this ReadOnlyMemory<byte> bytes, int position)
            => bytes.Span.Slice(position).ReadOffsetDateTime();
    }
}
