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
using System.Diagnostics;
using Hazelcast.Models;

namespace Hazelcast.Core
{
    internal static partial class BytesExtensions // Read from byte[]
    {
        /// <summary>
        /// Verifies that it is possible to read <param name="count"/> at position <param name="position"/>
        /// of the <param name="bytes"/> array.
        /// </summary>
        /// <param name="bytes">The array of bytes to read from.</param>
        /// <param name="position">The position.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The array of bytes to read from.</returns>
        public static byte[] CanRead(this byte[] bytes, int position, int count)
        {
            if (position + count <= bytes.Length) return bytes;
            throw new OverflowException($"Cannot read {count} bytes at {position} of {bytes.Length} bytes array.");
        }

        /// <summary>
        /// Reads a <see cref="byte"/> value from an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to read from.</param>
        /// <param name="position">The position in the array where the value should be read.</param>
        /// <returns>The value.</returns>
        public static byte ReadByte(this byte[] bytes, int position)
        {
            Debug.Assert(bytes != null && position >= 0 && bytes.Length >= position + SizeOfByte);

            return bytes[position];
        }

        public static sbyte ReadSByte(this byte[] bytes, int position)
        {
            Debug.Assert(bytes != null && position >= 0 && bytes.Length >= position + SizeOfByte);

            return (sbyte) bytes[position];
        }

        /// <summary>
        /// Reads a <see cref="short"/> value from an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to read from.</param>
        /// <param name="position">The position in the array where the value should be read.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        public static short ReadShort(this byte[] bytes, int position, Endianness endianness)
        {
            Debug.Assert(bytes != null && position >= 0 && bytes.Length >= position + SizeOfShort);

            unchecked
            {
                return endianness.IsBigEndian()

                    ? (short) (bytes[position + 0] << 8 | bytes[position + 1])
                    : (short) (bytes[position]          | bytes[position + 1] << 8);
            }
        }

        /// <summary>
        /// Reads an <see cref="ushort"/> value from an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to read from.</param>
        /// <param name="position">The position in the array where the value should be read.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        public static ushort ReadUShort(this byte[] bytes, int position, Endianness endianness)
        {
            Debug.Assert(bytes != null && position >= 0 && bytes.Length >= position + SizeOfShort);

            unchecked
            {
                return endianness.IsBigEndian()

                    ? (ushort) (bytes[position + 0] << 8 | bytes[position + 1])
                    : (ushort) (bytes[position]          | bytes[position + 1] << 8);
            }
        }

        /// <summary>
        /// Reads an <see cref="int"/> value from an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to read from.</param>
        /// <param name="position">The position in the array where the value should be read.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        public static int ReadInt(this byte[] bytes, int position, Endianness endianness)
        {
            Debug.Assert(bytes != null && position >= 0 && bytes.Length >= position + SizeOfInt);

            unchecked
            {
                return endianness.IsBigEndian()

                    ? bytes[position] << 24     | bytes[position + 1] << 16 |
                      bytes[position + 2] << 8  | bytes[position + 3]

                    : bytes[position]           | bytes[position + 1] << 8 |
                      bytes[position + 2] << 16 | bytes[position + 3] << 24;
            }
        }

        /// <summary>
        /// Reads a <see cref="long"/> value from an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to read from.</param>
        /// <param name="position">The position in the array where the value should be read.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        public static long ReadLong(this byte[] bytes, int position, Endianness endianness)
        {
            Debug.Assert(bytes != null && position >= 0 && bytes.Length >= position + SizeOfLong);

            unchecked
            {
                return endianness.IsBigEndian()

                    ? (long) bytes[position] << 56     | (long) bytes[position + 1] << 48 |
                      (long) bytes[position + 2] << 40 | (long) bytes[position + 3] << 32 |
                      (long) bytes[position + 4] << 24 | (long) bytes[position + 5] << 16 |
                      (long) bytes[position + 6] << 8  |        bytes[position + 7]

                    :        bytes[position]           | (long) bytes[position + 1] << 8 |
                      (long) bytes[position + 2] << 16 | (long) bytes[position + 3] << 24 |
                      (long) bytes[position + 4] << 32 | (long) bytes[position + 5] << 40 |
                      (long) bytes[position + 6] << 48 | (long) bytes[position + 7] << 56;
            }
        }

        public static HLocalDate ReadLocalDate(this byte[] bytes, int position)
        {
            var year = bytes.ReadIntL(position);
            var month = bytes.ReadByte(position + SizeOfInt);
            var date = bytes.ReadByte(position + SizeOfInt + SizeOfByte);

            return new HLocalDate(year, month, date);
        }

        public static HLocalTime ReadLocalTime(this byte[] bytes, int position)
        {
            var hour = bytes.ReadByte(position);
            var minute = bytes.ReadByte(position + SizeOfByte);
            var second = bytes.ReadByte(position + SizeOfByte * 2);
            var nano = bytes.ReadIntL(position + SizeOfByte * 3);

            return new HLocalTime(hour, minute, second, nano);
        }

        public static HLocalDateTime ReadLocalDateTime(this byte[] bytes, int position)
        {
            var date = ReadLocalDate(bytes, position);
            var time = ReadLocalTime(bytes, position + SizeOfLocalDate);
            return new HLocalDateTime(date, time);
        }

        public static HOffsetDateTime ReadOffsetDateTime(this byte[] bytes, int position)
        {
            var localDateTime = ReadLocalDateTime(bytes, position);
            var offsetSeconds = ReadIntL(bytes, position + SizeOfLocalDateTime);
            return new HOffsetDateTime(localDateTime, TimeSpan.FromSeconds(offsetSeconds));
        }

        /// <summary>
        /// Reads a <see cref="float"/> value from an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to read from.</param>
        /// <param name="position">The position in the array where the value should be read.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        public static float ReadFloat(this byte[] bytes, int position, Endianness endianness)
        {
            Debug.Assert(bytes != null && position >= 0 && bytes.Length >= position + SizeOfFloat);

            int value;
            unchecked
            {
                value = endianness.IsBigEndian()

                    ? bytes[position] << 24     | bytes[position + 1] << 16 |
                      bytes[position + 2] << 8  | bytes[position + 3]

                    : bytes[position]           | bytes[position + 1] << 8 |
                      bytes[position + 2] << 16 | bytes[position + 3] << 24;
            }

#if NETSTANDARD2_0
            return BitConverter.ToSingle(BitConverter.GetBytes(value), 0);
#else
            // this is essentially an unsafe *((float*)&value)
            return BitConverter.Int32BitsToSingle(value);
#endif
        }

        /// <summary>
        /// Reads a <see cref="double"/> value from an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to read from.</param>
        /// <param name="position">The position in the array where the value should be read.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        public static double ReadDouble(this byte[] bytes, int position, Endianness endianness)
        {
            Debug.Assert(bytes != null && position >= 0 && bytes.Length >= position + SizeOfDouble);

            long value;
            unchecked
            {
                value = endianness.IsBigEndian()

                    ? (long) bytes[position] << 56     | (long) bytes[position + 1] << 48 |
                      (long) bytes[position + 2] << 40 | (long) bytes[position + 3] << 32 |
                      (long) bytes[position + 4] << 24 | (long) bytes[position + 5] << 16 |
                      (long) bytes[position + 6] << 8  |        bytes[position + 7]

                    :        bytes[position]           | (long) bytes[position + 1] << 8 |
                      (long) bytes[position + 2] << 16 | (long) bytes[position + 3] << 24 |
                      (long) bytes[position + 4] << 32 | (long) bytes[position + 5] << 40 |
                      (long) bytes[position + 6] << 48 | (long) bytes[position + 7] << 56;
            }

            // this is essentially an unsafe *((double*)&value)
            return BitConverter.Int64BitsToDouble(value);
        }

        /// <summary>
        /// Reads a <see cref="bool"/> value from an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to read from.</param>
        /// <param name="position">The position in the array where the value should be read.</param>
        /// <returns>The value.</returns>
        public static bool ReadBool(this byte[] bytes, int position)
            => bytes.ReadByte(position) != 0;

        /// <summary>
        /// Reads a <see cref="char"/> value from an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to read from.</param>
        /// <param name="position">The position in the array where the value should be read.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        public static char ReadChar(this byte[] bytes, int position, Endianness endianness)
        {
            Debug.Assert(bytes != null && position >= 0 && bytes.Length >= position + SizeOfChar);
            unchecked
            {
                return (char)(endianness.IsBigEndian()

                    ? bytes[position] << 8 | bytes[position + 1]
                    : bytes[position]      | bytes[position + 1] << 8);
            }
        }
    }
}
