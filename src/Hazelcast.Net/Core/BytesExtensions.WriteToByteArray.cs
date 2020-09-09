﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
    public static partial class BytesExtensions // Write to byte[]
    {
        /// <summary>
        /// Writes a <see cref="byte"/> value to an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to write to.</param>
        /// <param name="position">The position in the array where the value should be written.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteByte(this byte[] bytes, int position, byte value)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            if (position < 0 || bytes.Length < position + SizeOfByte)
                throw new ArgumentOutOfRangeException(nameof(position));

            bytes[position] = value;
        }

        /// <summary>
        /// Writes a <see cref="short"/> value to an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to write to.</param>
        /// <param name="position">The position in the array where the value should be written.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="endianness">The endianness.</param>
        public static void WriteShort(this byte[] bytes, int position, short value, Endianness endianness = Endianness.Unspecified)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            if (position < 0 || bytes.Length < position + SizeOfShort)
                throw new ArgumentOutOfRangeException(nameof(position));

            var unsigned = (ushort)value;

            unchecked
            {
                if (endianness.Resolve().IsBigEndian())
                {
                    bytes[position]     = (byte) (unsigned >> 8);
                    bytes[position + 1] = (byte) unsigned;
                }
                else
                {
                    bytes[position]     = (byte) unsigned;
                    bytes[position + 1] = (byte) (unsigned >> 8);
                }
            }
        }

        /// <summary>
        /// Writes an <see cref="ushort"/> value to an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to write to.</param>
        /// <param name="position">The position in the array where the value should be written.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="endianness">The endianness.</param>
        public static void WriteUShort(this byte[] bytes, int position, ushort value, Endianness endianness = Endianness.Unspecified)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            if (position < 0 || bytes.Length < position + SizeOfUnsignedShort)
                throw new ArgumentOutOfRangeException(nameof(position));

            var unsigned = value;

            unchecked
            {
                if (endianness.Resolve().IsBigEndian())
                {
                    bytes[position]     = (byte) (unsigned >> 8);
                    bytes[position + 1] = (byte) unsigned;
                }
                else
                {
                    bytes[position]     = (byte) unsigned;
                    bytes[position + 1] = (byte) (unsigned >> 8);
                }
            }
        }

        /// <summary>
        /// Writes an <see cref="int"/> enum value to an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to write to.</param>
        /// <param name="position">The position in the array where the value should be written.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="endianness">The endianness.</param>
        public static void WriteInt(this byte[] bytes, int position, Enum value, Endianness endianness = Endianness.Unspecified)
            => bytes.WriteInt(position, (int)(object)value, endianness);

        /// <summary>
        /// Writes an <see cref="int"/> value to an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to write to.</param>
        /// <param name="position">The position in the array where the value should be written.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="endianness">The endianness.</param>
        public static void WriteInt(this byte[] bytes, int position, int value, Endianness endianness = Endianness.Unspecified)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            if (position < 0 || bytes.Length < position + SizeOfInt)
                throw new ArgumentOutOfRangeException(nameof(position));

            var unsigned = (uint)value;

            unchecked
            {
                if (endianness.Resolve().IsBigEndian())
                {
                    bytes[position]     = (byte) (unsigned >> 24);
                    bytes[position + 1] = (byte) (unsigned >> 16);
                    bytes[position + 2] = (byte) (unsigned >> 8);
                    bytes[position + 3] = (byte) unsigned;
                }
                else
                {
                    bytes[position]     = (byte) unsigned;
                    bytes[position + 1] = (byte) (unsigned >> 8);
                    bytes[position + 2] = (byte) (unsigned >> 16);
                    bytes[position + 3] = (byte) (unsigned >> 24);
                }
            }
        }

        /// <summary>
        /// Writes a <see cref="long"/> value to an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to write to.</param>
        /// <param name="position">The position in the array where the value should be written.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="endianness">The endianness.</param>
        public static void WriteLong(this byte[] bytes, int position, long value, Endianness endianness = Endianness.Unspecified)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            if (position < 0 || bytes.Length < position + SizeOfLong)
                throw new ArgumentOutOfRangeException(nameof(position));

            var unsigned = (ulong)value;

            unchecked
            {
                if (endianness.Resolve().IsBigEndian())
                {
                    bytes[position]     = (byte) (unsigned >> 56);
                    bytes[position + 1] = (byte) (unsigned >> 48);
                    bytes[position + 2] = (byte) (unsigned >> 40);
                    bytes[position + 3] = (byte) (unsigned >> 32);
                    bytes[position + 4] = (byte) (unsigned >> 24);
                    bytes[position + 5] = (byte) (unsigned >> 16);
                    bytes[position + 6] = (byte) (unsigned >> 8);
                    bytes[position + 7] = (byte) unsigned;
                }
                else
                {
                    bytes[position]     = (byte) unsigned;
                    bytes[position + 1] = (byte) (unsigned >> 8);
                    bytes[position + 2] = (byte) (unsigned >> 16);
                    bytes[position + 3] = (byte) (unsigned >> 24);
                    bytes[position + 4] = (byte) (unsigned >> 32);
                    bytes[position + 5] = (byte) (unsigned >> 40);
                    bytes[position + 6] = (byte) (unsigned >> 48);
                    bytes[position + 7] = (byte) (unsigned >> 56);
                }
            }
        }



        /// <summary>
        /// Writes a <see cref="float"/> value to an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to write to.</param>
        /// <param name="position">The position in the array where the value should be written.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="endianness">The endianness.</param>
        public static void WriteFloat(this byte[] bytes, int position, float value, Endianness endianness = Endianness.Unspecified)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            if (position < 0 || bytes.Length < position + SizeOfFloat)
                throw new ArgumentOutOfRangeException(nameof(position));

#if NETSTANDARD2_0
            var unsigned = (uint) BitConverter.ToInt32(BitConverter.GetBytes(value), 0);
#else
            // this is essentially an unsafe *((int*)&value)
            var unsigned = (uint) BitConverter.SingleToInt32Bits(value);
#endif
            unchecked
            {
                if (endianness.Resolve().IsBigEndian())
                {
                    bytes[position]     = (byte) (unsigned >> 24);
                    bytes[position + 1] = (byte) (unsigned >> 16);
                    bytes[position + 2] = (byte) (unsigned >> 8);
                    bytes[position + 3] = (byte) unsigned;
                }
                else
                {
                    bytes[position]     = (byte) unsigned;
                    bytes[position + 1] = (byte) (unsigned >> 8);
                    bytes[position + 2] = (byte) (unsigned >> 16);
                    bytes[position + 3] = (byte) (unsigned >> 24);
                }
            }

        }

        /// <summary>
        /// Writes a <see cref="double"/> value to an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to write to.</param>
        /// <param name="position">The position in the array where the value should be written.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="endianness">The endianness.</param>
        public static void WriteDouble(this byte[] bytes, int position, double value, Endianness endianness = Endianness.Unspecified)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            if (position < 0 || bytes.Length < position + SizeOfDouble)
                throw new ArgumentOutOfRangeException(nameof(position));

            // this is essentially an unsafe *((long*)&value)
            var unsigned = (ulong) BitConverter.DoubleToInt64Bits(value);

            unchecked
            {
                if (endianness.Resolve().IsBigEndian())
                {
                    bytes[position]     = (byte) (unsigned >> 56);
                    bytes[position + 1] = (byte) (unsigned >> 48);
                    bytes[position + 2] = (byte) (unsigned >> 40);
                    bytes[position + 3] = (byte) (unsigned >> 32);
                    bytes[position + 4] = (byte) (unsigned >> 24);
                    bytes[position + 5] = (byte) (unsigned >> 16);
                    bytes[position + 6] = (byte) (unsigned >> 8);
                    bytes[position + 7] = (byte) unsigned;
                }
                else
                {
                    bytes[position]     = (byte) unsigned;
                    bytes[position + 1] = (byte) (unsigned >> 8);
                    bytes[position + 2] = (byte) (unsigned >> 16);
                    bytes[position + 3] = (byte) (unsigned >> 24);
                    bytes[position + 4] = (byte) (unsigned >> 32);
                    bytes[position + 5] = (byte) (unsigned >> 40);
                    bytes[position + 6] = (byte) (unsigned >> 48);
                    bytes[position + 7] = (byte) (unsigned >> 56);
                }
            }
        }



        /// <summary>
        /// Writes a <see cref="bool"/> value to an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to write to.</param>
        /// <param name="position">The position in the array where the value should be written.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteBool(this byte[] bytes, int position, bool value)
            => bytes.WriteByte(position, value ? (byte) 0x01 : (byte) 0x00);



        /// <summary>
        /// Writes a <see cref="char"/> value to an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to write to.</param>
        /// <param name="position">The position in the array where the value should be written.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="endianness">The endianness.</param>
        public static void WriteChar(this byte[] bytes, int position, char value, Endianness endianness = Endianness.Unspecified)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            if (position < 0 || bytes.Length < position + SizeOfChar)
                throw new ArgumentOutOfRangeException(nameof(position));

            var unsigned = value;

            unchecked
            {
                if (endianness.Resolve().IsBigEndian())
                {
                    bytes[position]     = (byte) (unsigned >> 8);
                    bytes[position + 1] = (byte) unsigned;
                }
                else
                {
                    bytes[position]     = (byte) unsigned;
                    bytes[position + 1] = (byte) (unsigned >> 8);
                }
            }
        }

        /// <summary>
        /// Writes a <see cref="char"/> value to an array of bytes, encoded on 1, 2 or 3 bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to write to.</param>
        /// <param name="position">The position in the array where the value should be written.</param>
        /// <param name="value">The value to write.</param>
        /// <remarks>
        /// <para>The position is incremented with the number of bytes written.</para>
        /// </remarks>
        public static void WriteUtf8Char(this byte[] bytes, ref int position, char value)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            if (position < 0)
                throw new ArgumentOutOfRangeException(nameof(position));

            if (value <= 0x007f)
            {
                if (bytes.Length < position + SizeOfByte)
                    throw new ArgumentOutOfRangeException(nameof(position));

                bytes[position] = (byte)value;
                position += 1;
                return;
            }

            if (value <= 0x07ff)
            {
                if (bytes.Length < position + 2 * SizeOfByte)
                    throw new ArgumentOutOfRangeException(nameof(position));

                bytes[position] = (byte)(0xc0 | value >> 6 & 0x1f);
                bytes[position + 1] = (byte)(0x80 | value & 0x3f);
                position += 2;
                return;
            }

            if (bytes.Length < position + 3 * SizeOfByte)
                throw new ArgumentOutOfRangeException(nameof(position));

            if (value > 0xd7ff)
                throw new InvalidOperationException("Cannot write surrogate pairs.");

            bytes[position] = (byte)(0xe0 | value >> 12 & 0x0f);
            bytes[position + 1] = (byte)(0x80 | value >> 6 & 0x3f);
            bytes[position + 2] = (byte)(0x80 | value & 0x3f);
            position += 3;
        }



        /// <summary>
        /// Writes a <see cref="Guid"/> value to an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to write to.</param>
        /// <param name="position">The position in the array where the value should be written.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteGuid(this byte[] bytes, int position, Guid value)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            // must at least be able to write a bool
            if (position < 0 || bytes.Length < position + SizeOfBool)
                throw new ArgumentOutOfRangeException(nameof(position));

            // write the 'empty' bool
            bytes.WriteBool(position, value == Guid.Empty);
            if (value == Guid.Empty) return;

            // if not empty, must be able to write the full guid
            if (bytes.Length < position + SizeOfGuid)
                throw new ArgumentOutOfRangeException(nameof(position));

            position += SizeOfByte;

            var v = new JavaUuidOrder { Value = value };

            bytes[position] = v.X0; position += SizeOfByte;
            bytes[position] = v.X1; position += SizeOfByte;
            bytes[position] = v.X2; position += SizeOfByte;
            bytes[position] = v.X3; position += SizeOfByte;

            bytes[position] = v.X4; position += SizeOfByte;
            bytes[position] = v.X5; position += SizeOfByte;
            bytes[position] = v.X6; position += SizeOfByte;
            bytes[position] = v.X7; position += SizeOfByte;

            bytes[position] = v.X8; position += SizeOfByte;
            bytes[position] = v.X9; position += SizeOfByte;
            bytes[position] = v.XA; position += SizeOfByte;
            bytes[position] = v.XB; position += SizeOfByte;

            bytes[position] = v.XC; position += SizeOfByte;
            bytes[position] = v.XD; position += SizeOfByte;
            bytes[position] = v.XE; position += SizeOfByte;
            bytes[position] = v.XF;
        }
    }
}
