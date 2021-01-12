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

namespace Hazelcast.Core
{
    internal static partial class BytesExtensions // Write to byte[]
    {
        /// <summary>
        /// Writes a <see cref="byte"/> value to an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to write to.</param>
        /// <param name="position">The position in the array where the value should be written.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteByte(this byte[] bytes, int position, byte value)
        {
            Debug.Assert(bytes != null && position >= 0 && bytes.Length >= position + SizeOfByte);
            bytes[position] = value;
        }

        public static void WriteSbyte(this byte[] bytes, int position, sbyte value)
        {
            Debug.Assert(bytes != null && position >= 0 && bytes.Length >= position + SizeOfUnsignedByte);
            bytes[position] = (byte) value;
        }

        /// <summary>
        /// Writes a <see cref="short"/> value to an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to write to.</param>
        /// <param name="position">The position in the array where the value should be written.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="endianness">The endianness.</param>
        public static void WriteShort(this byte[] bytes, int position, short value, Endianness endianness)
            => bytes.WriteUShort(position, (ushort)value, endianness);

        /// <summary>
        /// Writes an <see cref="ushort"/> value to an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to write to.</param>
        /// <param name="position">The position in the array where the value should be written.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="endianness">The endianness.</param>
        public static void WriteUShort(this byte[] bytes, int position, ushort value, Endianness endianness)
        {
            Debug.Assert(bytes != null && position >= 0 && bytes.Length >= position + SizeOfUnsignedShort);
            unchecked
            {
                if (endianness.IsBigEndian())
                {
                    bytes[position] = (byte) (value >> 8);
                    bytes[position + 1] = (byte) value;
                }
                else
                {
                    bytes[position] = (byte) value;
                    bytes[position + 1] = (byte) (value >> 8);
                }
            }
        }

        /// <summary>
        /// Writes an <see cref="int"/> value to an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to write to.</param>
        /// <param name="position">The position in the array where the value should be written.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="endianness">The endianness.</param>
        public static void WriteInt(this byte[] bytes, int position, int value, Endianness endianness)
        {
            Debug.Assert(bytes != null && position >= 0 && bytes.Length >= position + SizeOfInt);
            var unsigned = (uint) value;
            unchecked
            {
                if (endianness.IsBigEndian())
                {
                    bytes[position] = (byte) (unsigned >> 24);
                    bytes[position + 1] = (byte) (unsigned >> 16);
                    bytes[position + 2] = (byte) (unsigned >> 8);
                    bytes[position + 3] = (byte) unsigned;
                }
                else
                {
                    bytes[position] = (byte) unsigned;
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
        public static void WriteLong(this byte[] bytes, int position, long value, Endianness endianness)
        {
            Debug.Assert(bytes != null && position >= 0 && bytes.Length >= position + SizeOfLong);
            var unsigned = (ulong) value;
            unchecked
            {
                if (endianness.IsBigEndian())
                {
                    bytes[position] = (byte) (unsigned >> 56);
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
                    bytes[position] = (byte) unsigned;
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
        public static void WriteFloat(this byte[] bytes, int position, float value, Endianness endianness)
        {
            Debug.Assert(bytes != null && position >= 0 && bytes.Length >= position + SizeOfFloat);

#if NETSTANDARD2_0
            var unsigned = (uint) BitConverter.ToInt32(BitConverter.GetBytes(value), 0);
#else
            // this is essentially an unsafe *((int*)&value)
            var unsigned = (uint) BitConverter.SingleToInt32Bits(value);
#endif
            unchecked
            {
                if (endianness.IsBigEndian())
                {
                    bytes[position] = (byte) (unsigned >> 24);
                    bytes[position + 1] = (byte) (unsigned >> 16);
                    bytes[position + 2] = (byte) (unsigned >> 8);
                    bytes[position + 3] = (byte) unsigned;
                }
                else
                {
                    bytes[position] = (byte) unsigned;
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
        public static void WriteDouble(this byte[] bytes, int position, double value, Endianness endianness)
        {
            Debug.Assert(bytes != null && position >= 0 && bytes.Length >= position + SizeOfDouble);
            // this is essentially an unsafe *((long*)&value)
            var unsigned = (ulong) BitConverter.DoubleToInt64Bits(value);
            unchecked
            {
                if (endianness.IsBigEndian())
                {
                    bytes[position] = (byte) (unsigned >> 56);
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
                    bytes[position] = (byte) unsigned;
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
        public static void WriteChar(this byte[] bytes, int position, char value, Endianness endianness)
        {
            Debug.Assert(bytes != null && position >= 0 && bytes.Length >= position + SizeOfChar);
            var unsigned = value;
            unchecked
            {
                if (endianness.IsBigEndian())
                {
                    bytes[position] = (byte) (unsigned >> 8);
                    bytes[position + 1] = (byte) unsigned;
                }
                else
                {
                    bytes[position] = (byte) unsigned;
                    bytes[position + 1] = (byte) (unsigned >> 8);
                }
            }
        }
    }
}
