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

namespace Hazelcast.Core
{
    public static partial class BytesExtensions // Read from byte[]
    {
        /// <summary>
        /// Reads a <see cref="byte"/> value from an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to read from.</param>
        /// <param name="position">The position in the array where the value should be read.</param>
        /// <returns>The value.</returns>
        public static byte ReadByte(this byte[] bytes, int position)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            if (position < 0 || bytes.Length < position + SizeOfByte)
                throw new ArgumentOutOfRangeException(nameof(position));

            return bytes[position];
        }

        /// <summary>
        /// Reads a <see cref="short"/> value from an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to read from.</param>
        /// <param name="position">The position in the array where the value should be read.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        public static short ReadShort(this byte[] bytes, int position, Endianness endianness = Endianness.Unspecified)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            if (position < 0 || bytes.Length < position + SizeOfShort)
                throw new ArgumentOutOfRangeException(nameof(position));

            unchecked
            {
                return endianness.Resolve().IsBigEndian()

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
        public static ushort ReadUShort(this byte[] bytes, int position, Endianness endianness = Endianness.Unspecified)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            if (position < 0 || bytes.Length < position + SizeOfShort)
                throw new ArgumentOutOfRangeException(nameof(position));

            unchecked
            {
                return endianness.Resolve().IsBigEndian()

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
        public static int ReadInt(this byte[] bytes, int position, Endianness endianness = Endianness.Unspecified)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            if (position < 0 || bytes.Length < position + SizeOfInt)
                throw new ArgumentOutOfRangeException(nameof(position));

            unchecked
            {
                return endianness.Resolve().IsBigEndian()

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
        public static long ReadLong(this byte[] bytes, int position, Endianness endianness = Endianness.Unspecified)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            if (position < 0 || bytes.Length < position + SizeOfLong)
                throw new ArgumentOutOfRangeException(nameof(position));

            unchecked
            {
                return endianness.Resolve().IsBigEndian()

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



        /// <summary>
        /// Reads a <see cref="float"/> value from an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to read from.</param>
        /// <param name="position">The position in the array where the value should be read.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        public static float ReadFloat(this byte[] bytes, int position, Endianness endianness = Endianness.Unspecified)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            if (position < 0 || bytes.Length < position + SizeOfFloat)
                throw new ArgumentOutOfRangeException(nameof(position));

            int value;
            unchecked
            {
                value = endianness.Resolve().IsBigEndian()

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
        public static double ReadDouble(this byte[] bytes, int position, Endianness endianness = Endianness.Unspecified)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            if (position < 0 || bytes.Length < position + SizeOfDouble)
                throw new ArgumentOutOfRangeException(nameof(position));

            long value;
            unchecked
            {
                value = endianness.Resolve().IsBigEndian()

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
        public static char ReadChar(this byte[] bytes, int position, Endianness endianness = Endianness.Unspecified)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            if (position < 0 || bytes.Length < position + SizeOfChar)
                throw new ArgumentOutOfRangeException(nameof(position));

            unchecked
            {
                return (char)(endianness.Resolve().IsBigEndian()

                    ? bytes[position] << 8 | bytes[position + 1]
                    : bytes[position]      | bytes[position + 1] << 8);
            }
        }

        /// <summary>
        /// Reads a <see cref="char"/> value encoded on 1, 2 or 3 bytes from an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to read from.</param>
        /// <param name="position">The position in the array where the value should be read.</param>
        /// <returns>The value.</returns>
        /// <remarks>
        /// <para>The position is incremented with the number of bytes read.</para>
        /// </remarks>
        public static char ReadUtf8Char(this byte[] bytes, ref int position)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            if (position < 0 || bytes.Length < position + SizeOfByte)
                throw new ArgumentOutOfRangeException(nameof(position));

            unchecked
            {
                var b = bytes[position];
                var x = b >> 4;

                if (x >= 0 && x <= 7)
                {
                    position += 1;
                    return (char)b;
                }

                if (x == 12 || x == 13)
                {
                    if (bytes.Length < position + 2 * SizeOfByte)
                        throw new ArgumentOutOfRangeException(nameof(position));

                    var first = (b & 0x1f) << 6;
                    var second = bytes[position + 1] & 0x3f;
                    position += 2;
                    return (char)(first | second);
                }

                if (x == 14)
                {
                    if (bytes.Length < position + 3 * SizeOfByte)
                        throw new ArgumentOutOfRangeException(nameof(position));

                    var first = (b & 0x0f) << 12;
                    var second = (bytes[position + 1] & 0x3f) << 6;
                    var third = bytes[position + 2] & 0x3f;
                    position += 3;
                    return (char)(first | second | third);
                }

                throw new InvalidOperationException("Cannot read surrogate pairs.");
            }
        }



        /// <summary>
        /// Reads a <see cref="Guid"/> value from an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to read from.</param>
        /// <param name="position">The position in the array where the value should be read.</param>
        /// <returns></returns>
        public static Guid ReadGuid(this byte[] bytes, int position)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            // must be at least able to read a bool
            if (position < 0 || bytes.Length < position + SizeOfBool)
                throw new ArgumentOutOfRangeException(nameof(position));

            // read the 'empty' bool
            if (bytes.ReadBool(position)) return Guid.Empty;

            // not 'empty', must be able to read a full guid
            if (bytes.Length < position + SizeOfGuid)
                throw new ArgumentOutOfRangeException(nameof(position));

            position += SizeOfByte;

            // ReSharper disable once UseObjectOrCollectionInitializer
#pragma warning disable IDE0017 // Simplify object initialization
            var v = new JavaUuidOrder();
#pragma warning restore IDE0017 // Simplify object initialization

            v.X0 = bytes[position]; position += SizeOfByte;
            v.X1 = bytes[position]; position += SizeOfByte;
            v.X2 = bytes[position]; position += SizeOfByte;
            v.X3 = bytes[position]; position += SizeOfByte;

            v.X4 = bytes[position]; position += SizeOfByte;
            v.X5 = bytes[position]; position += SizeOfByte;
            v.X6 = bytes[position]; position += SizeOfByte;
            v.X7 = bytes[position]; position += SizeOfByte;

            v.X8 = bytes[position]; position += SizeOfByte;
            v.X9 = bytes[position]; position += SizeOfByte;
            v.XA = bytes[position]; position += SizeOfByte;
            v.XB = bytes[position]; position += SizeOfByte;

            v.XC = bytes[position]; position += SizeOfByte;
            v.XD = bytes[position]; position += SizeOfByte;
            v.XE = bytes[position]; position += SizeOfByte;
            v.XF = bytes[position];

            return v.Value;
        }
    }
}
