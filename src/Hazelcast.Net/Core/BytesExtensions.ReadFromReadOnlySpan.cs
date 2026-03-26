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
using Hazelcast.Exceptions;
using Hazelcast.Models;

namespace Hazelcast.Core
{
    internal static partial class BytesExtensions // Read from ReadOnlySpan
    {
        /// <summary>
        /// Reads a <see cref="short"/> value from a span of bytes.
        /// </summary>
        /// <param name="bytes">The span of bytes to read from.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        public static short ReadShort(this ReadOnlySpan<byte> bytes, Endianness endianness)
        {
            const byte length = sizeof(ushort);

            if (bytes.Length < length)
                throw new ArgumentException(ExceptionMessages.NotEnoughBytes, nameof(bytes));

            unchecked
            {
                return (short)(endianness.IsBigEndian()

                    ? bytes[0] << 8 | bytes[1]
                    : bytes[0]      | bytes[1] << 8);
            }
        }

        /// <summary>
        /// Reads an <see cref="ushort"/> value from a span of bytes.
        /// </summary>
        /// <param name="bytes">The span of bytes to read from.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        public static ushort ReadUShort(this ReadOnlySpan<byte> bytes, Endianness endianness)
        {
            const byte length = sizeof(ushort);

            if (bytes.Length < length)
                throw new ArgumentException(ExceptionMessages.NotEnoughBytes, nameof(bytes));

            unchecked
            {
                return (ushort)(endianness.IsBigEndian()

                    ? bytes[0] << 8 | bytes[1]
                    : bytes[0]      | bytes[1] << 8);
            }
        }

        /// <summary>
        /// Reads an <see cref="int"/> value from a span of bytes.
        /// </summary>
        /// <param name="bytes">The span of bytes to read from.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        public static int ReadInt(this ReadOnlySpan<byte> bytes, Endianness endianness)
        {
            const byte length = sizeof(int);

            if (bytes.Length < length)
                throw new ArgumentException(ExceptionMessages.NotEnoughBytes, nameof(bytes));

            return endianness.IsBigEndian()

                ? bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8  | bytes[3]
                : bytes[0]       | bytes[1] << 8  | bytes[2] << 16 | bytes[3] << 24;
        }

        /// <summary>
        /// Reads a <see cref="byte"/> value from a span of bytes.
        /// </summary>
        /// <param name="bytes">The span of bytes to read from.</param>
        /// <returns>The value.</returns>
        public static byte ReadByte(this ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length < 1)
                throw new ArgumentException(ExceptionMessages.NotEnoughBytes, nameof(bytes));

            return bytes[0];
        }

        /// <summary>
        /// Reads an <see cref="sbyte"/> value from a span of bytes.
        /// </summary>
        /// <param name="bytes">The span of bytes to read from.</param>
        /// <returns>The value.</returns>
        public static sbyte ReadSByte(this ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length < 1)
                throw new ArgumentException(ExceptionMessages.NotEnoughBytes, nameof(bytes));

            return (sbyte) bytes[0];
        }

        /// <summary>
        /// Reads a <see cref="long"/> value from a span of bytes.
        /// </summary>
        /// <param name="bytes">The span of bytes to read from.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        public static long ReadLong(this ReadOnlySpan<byte> bytes, Endianness endianness)
        {
            const byte length = sizeof(long);

            if (bytes.Length < length)
                throw new ArgumentException(ExceptionMessages.NotEnoughBytes, nameof(bytes));

            unchecked
            {
                return endianness.IsBigEndian()

                    ? (long) bytes[0] << 56 | (long) bytes[1] << 48 |
                      (long) bytes[2] << 40 | (long) bytes[3] << 32 |
                      (long) bytes[4] << 24 | (long) bytes[5] << 16 |
                      (long) bytes[6] << 8  |        bytes[7]

                    :  bytes[0]        | (long) bytes[1] << 8  |
                       (long) bytes[2] << 16  | (long) bytes[3] << 24 |
                       (long) bytes[4] << 32  | (long) bytes[5] << 40 |
                       (long) bytes[6] << 48  | (long) bytes[7] << 56;
            }
        }

        /// <summary>
        /// Reads a <see cref="float"/> value from a span of bytes.
        /// </summary>
        /// <param name="bytes">The span of bytes to read from.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        public static float ReadFloat(this ReadOnlySpan<byte> bytes, Endianness endianness)
        {
            const byte length = sizeof(float);

            if (bytes.Length < length)
                throw new ArgumentException(ExceptionMessages.NotEnoughBytes, nameof(bytes));

            var value = bytes.ReadInt(endianness);

#if NETSTANDARD2_0
            return BitConverter.ToSingle(BitConverter.GetBytes(value), 0);
#else
            return BitConverter.Int32BitsToSingle(value);
#endif
        }

        /// <summary>
        /// Reads a <see cref="double"/> value from a span of bytes.
        /// </summary>
        /// <param name="bytes">The span of bytes to read from.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        public static double ReadDouble(this ReadOnlySpan<byte> bytes, Endianness endianness)
        {
            const byte length = sizeof(double);

            if (bytes.Length < length)
                throw new ArgumentException(ExceptionMessages.NotEnoughBytes, nameof(bytes));

            var value = bytes.ReadLong(endianness);
            return BitConverter.Int64BitsToDouble(value);
        }

        /// <summary>
        /// Reads a <see cref="bool"/> value from a span of bytes.
        /// </summary>
        /// <param name="bytes">The span of bytes to read from.</param>
        /// <returns>The value.</returns>
        public static bool ReadBool(this ReadOnlySpan<byte> bytes)
            => bytes.ReadByte() != 0;

        /// <summary>
        /// Reads a <see cref="char"/> value from a span of bytes.
        /// </summary>
        /// <param name="bytes">The span of bytes to read from.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        public static char ReadChar(this ReadOnlySpan<byte> bytes, Endianness endianness)
        {
            const byte length = sizeof(char);

            if (bytes.Length < length)
                throw new ArgumentException(ExceptionMessages.NotEnoughBytes, nameof(bytes));

            unchecked
            {
                return (char) (endianness.IsBigEndian()

                    ? bytes[0] << 8 | bytes[1]
                    : bytes[0]      | bytes[1] << 8);
            }
        }

        /// <summary>
        /// Reads a <see cref="HLocalDate"/> value from a span of bytes.
        /// </summary>
        /// <param name="bytes">The span of bytes to read from.</param>
        /// <returns>The value.</returns>
        public static HLocalDate ReadLocalDate(this ReadOnlySpan<byte> bytes)
        {
            var year = bytes.ReadInt(Endianness.LittleEndian);
            var month = bytes[SizeOfInt];
            var date = bytes[SizeOfInt + SizeOfByte];
            return new HLocalDate(year, month, date);
        }

        /// <summary>
        /// Reads a <see cref="HLocalTime"/> value from a span of bytes.
        /// </summary>
        /// <param name="bytes">The span of bytes to read from.</param>
        /// <returns>The value.</returns>
        public static HLocalTime ReadLocalTime(this ReadOnlySpan<byte> bytes)
        {
            var hour = bytes[0];
            var minute = bytes[1];
            var second = bytes[2];
            var nano = bytes.Slice(SizeOfByte * 3).ReadInt(Endianness.LittleEndian);
            return new HLocalTime(hour, minute, second, nano);
        }

        /// <summary>
        /// Reads a <see cref="HLocalDateTime"/> value from a span of bytes.
        /// </summary>
        /// <param name="bytes">The span of bytes to read from.</param>
        /// <returns>The value.</returns>
        public static HLocalDateTime ReadLocalDateTime(this ReadOnlySpan<byte> bytes)
        {
            var date = bytes.ReadLocalDate();
            var time = bytes.Slice(SizeOfLocalDate).ReadLocalTime();
            return new HLocalDateTime(date, time);
        }

        /// <summary>
        /// Reads a <see cref="HOffsetDateTime"/> value from a span of bytes.
        /// </summary>
        /// <param name="bytes">The span of bytes to read from.</param>
        /// <returns>The value.</returns>
        public static HOffsetDateTime ReadOffsetDateTime(this ReadOnlySpan<byte> bytes)
        {
            var localDateTime = bytes.ReadLocalDateTime();
            var offsetSeconds = bytes.Slice(SizeOfLocalDateTime).ReadInt(Endianness.LittleEndian);
            return new HOffsetDateTime(localDateTime, TimeSpan.FromSeconds(offsetSeconds));
        }
    }
}
