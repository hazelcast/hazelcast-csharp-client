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
    internal static partial class BytesExtensions // Write to Span<byte>
    {
        /// <summary>
        /// Writes a bits value to a span of bytes.
        /// </summary>
        /// <param name="bytes">The span of bytes to write to.</param>
        /// <param name="bits">The bits to write.</param>
        /// <param name="mask">The mask for selecting which bits to write.</param>
        public static void WriteBits(this Span<byte> bytes, byte bits, byte mask)
        {
            if (bytes.Length < SizeOfByte)
                throw new ArgumentException(ExceptionMessages.NotEnoughBytes, nameof(bytes));

            bytes[0] &= (byte)(bits | ~mask); // clear
            bytes[0] |= (byte)(bits & mask);  // set
        }

        /// <summary>
        /// Writes a <see cref="byte"/> value to a span of bytes.
        /// </summary>
        /// <param name="bytes">The span of bytes to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteByte(this Span<byte> bytes, byte value)
        {
            if (bytes.Length < SizeOfByte)
                throw new ArgumentException(ExceptionMessages.NotEnoughBytes, nameof(bytes));

            bytes[0] = value;
        }

        /// <summary>
        /// Writes a <see cref="sbyte"/> value to a span of bytes.
        /// </summary>
        /// <param name="bytes">The span of bytes to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteSbyte(this Span<byte> bytes, sbyte value)
        {
            if (bytes.Length < SizeOfByte)
                throw new ArgumentException(ExceptionMessages.NotEnoughBytes, nameof(bytes));

            bytes[0] = (byte)value;
        }

        /// <summary>
        /// Writes a <see cref="short"/> value to a span of bytes.
        /// </summary>
        /// <param name="bytes">The span of bytes to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="endianness">The endianness.</param>
        public static void WriteShort(this Span<byte> bytes, short value, Endianness endianness)
            => bytes.WriteUShort((ushort)value, endianness);

        /// <summary>
        /// Writes an <see cref="ushort"/> value to a span of bytes.
        /// </summary>
        /// <param name="bytes">The span of bytes to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="endianness">The endianness.</param>
        public static void WriteUShort(this Span<byte> bytes, ushort value, Endianness endianness)
        {
            if (bytes.Length < SizeOfShort)
                throw new ArgumentException(ExceptionMessages.NotEnoughBytes, nameof(bytes));

            unchecked
            {
                if (endianness.IsBigEndian())
                {
                    bytes[0] = (byte)(value >> 8);
                    bytes[1] = (byte)value;
                }
                else
                {
                    bytes[0] = (byte)value;
                    bytes[1] = (byte)(value >> 8);
                }
            }
        }

        /// <summary>
        /// Writes an <see cref="int"/> value to a span of bytes.
        /// </summary>
        /// <param name="bytes">The span of bytes to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="endianness">The endianness.</param>
        public static void WriteInt(this Span<byte> bytes, int value, Endianness endianness)
        {
            if (bytes.Length < SizeOfInt)
                throw new ArgumentException(ExceptionMessages.NotEnoughBytes, nameof(bytes));

            var unsigned = (uint)value;
            unchecked
            {
                if (endianness.IsBigEndian())
                {
                    bytes[0] = (byte)(unsigned >> 24);
                    bytes[1] = (byte)(unsigned >> 16);
                    bytes[2] = (byte)(unsigned >> 8);
                    bytes[3] = (byte)unsigned;
                }
                else
                {
                    bytes[0] = (byte)unsigned;
                    bytes[1] = (byte)(unsigned >> 8);
                    bytes[2] = (byte)(unsigned >> 16);
                    bytes[3] = (byte)(unsigned >> 24);
                }
            }
        }

        /// <summary>
        /// Writes a <see cref="long"/> value to a span of bytes.
        /// </summary>
        /// <param name="bytes">The span of bytes to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="endianness">The endianness.</param>
        public static void WriteLong(this Span<byte> bytes, long value, Endianness endianness)
        {
            if (bytes.Length < SizeOfLong)
                throw new ArgumentException(ExceptionMessages.NotEnoughBytes, nameof(bytes));

            var unsigned = (ulong)value;
            unchecked
            {
                if (endianness.IsBigEndian())
                {
                    bytes[0] = (byte)(unsigned >> 56);
                    bytes[1] = (byte)(unsigned >> 48);
                    bytes[2] = (byte)(unsigned >> 40);
                    bytes[3] = (byte)(unsigned >> 32);
                    bytes[4] = (byte)(unsigned >> 24);
                    bytes[5] = (byte)(unsigned >> 16);
                    bytes[6] = (byte)(unsigned >> 8);
                    bytes[7] = (byte)unsigned;
                }
                else
                {
                    bytes[0] = (byte)unsigned;
                    bytes[1] = (byte)(unsigned >> 8);
                    bytes[2] = (byte)(unsigned >> 16);
                    bytes[3] = (byte)(unsigned >> 24);
                    bytes[4] = (byte)(unsigned >> 32);
                    bytes[5] = (byte)(unsigned >> 40);
                    bytes[6] = (byte)(unsigned >> 48);
                    bytes[7] = (byte)(unsigned >> 56);
                }
            }
        }

        /// <summary>
        /// Writes a <see cref="float"/> value to a span of bytes.
        /// </summary>
        /// <param name="bytes">The span of bytes to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="endianness">The endianness.</param>
        public static void WriteFloat(this Span<byte> bytes, float value, Endianness endianness)
        {
            if (bytes.Length < SizeOfFloat)
                throw new ArgumentException(ExceptionMessages.NotEnoughBytes, nameof(bytes));

#if NETSTANDARD2_0
            var unsigned = (uint)BitConverter.ToInt32(BitConverter.GetBytes(value), 0);
#else
            // this is essentially an unsafe *((int*)&value)
            var unsigned = (uint)BitConverter.SingleToInt32Bits(value);
#endif
            unchecked
            {
                if (endianness.IsBigEndian())
                {
                    bytes[0] = (byte)(unsigned >> 24);
                    bytes[1] = (byte)(unsigned >> 16);
                    bytes[2] = (byte)(unsigned >> 8);
                    bytes[3] = (byte)unsigned;
                }
                else
                {
                    bytes[0] = (byte)unsigned;
                    bytes[1] = (byte)(unsigned >> 8);
                    bytes[2] = (byte)(unsigned >> 16);
                    bytes[3] = (byte)(unsigned >> 24);
                }
            }
        }

        /// <summary>
        /// Writes a <see cref="double"/> value to a span of bytes.
        /// </summary>
        /// <param name="bytes">The span of bytes to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="endianness">The endianness.</param>
        public static void WriteDouble(this Span<byte> bytes, double value, Endianness endianness)
        {
            if (bytes.Length < SizeOfDouble)
                throw new ArgumentException(ExceptionMessages.NotEnoughBytes, nameof(bytes));

            // this is essentially an unsafe *((long*)&value)
            var unsigned = (ulong)BitConverter.DoubleToInt64Bits(value);
            unchecked
            {
                if (endianness.IsBigEndian())
                {
                    bytes[0] = (byte)(unsigned >> 56);
                    bytes[1] = (byte)(unsigned >> 48);
                    bytes[2] = (byte)(unsigned >> 40);
                    bytes[3] = (byte)(unsigned >> 32);
                    bytes[4] = (byte)(unsigned >> 24);
                    bytes[5] = (byte)(unsigned >> 16);
                    bytes[6] = (byte)(unsigned >> 8);
                    bytes[7] = (byte)unsigned;
                }
                else
                {
                    bytes[0] = (byte)unsigned;
                    bytes[1] = (byte)(unsigned >> 8);
                    bytes[2] = (byte)(unsigned >> 16);
                    bytes[3] = (byte)(unsigned >> 24);
                    bytes[4] = (byte)(unsigned >> 32);
                    bytes[5] = (byte)(unsigned >> 40);
                    bytes[6] = (byte)(unsigned >> 48);
                    bytes[7] = (byte)(unsigned >> 56);
                }
            }
        }

        /// <summary>
        /// Writes a <see cref="bool"/> value to a span of bytes.
        /// </summary>
        /// <param name="bytes">The span of bytes to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteBool(this Span<byte> bytes, bool value)
            => bytes.WriteByte(value ? (byte)0x01 : (byte)0x00);

        /// <summary>
        /// Writes a <see cref="char"/> value to a span of bytes.
        /// </summary>
        /// <param name="bytes">The span of bytes to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="endianness">The endianness.</param>
        public static void WriteChar(this Span<byte> bytes, char value, Endianness endianness)
        {
            if (bytes.Length < SizeOfChar)
                throw new ArgumentException(ExceptionMessages.NotEnoughBytes, nameof(bytes));

            var unsigned = value;
            unchecked
            {
                if (endianness.IsBigEndian())
                {
                    bytes[0] = (byte)(unsigned >> 8);
                    bytes[1] = (byte)unsigned;
                }
                else
                {
                    bytes[0] = (byte)unsigned;
                    bytes[1] = (byte)(unsigned >> 8);
                }
            }
        }

        public static void WriteLocalDate(this Span<byte> bytes, HLocalDate localDate)
        {
            if (bytes.Length < SizeOfLocalDate)
                throw new ArgumentException(ExceptionMessages.NotEnoughBytes, nameof(bytes));

            bytes.WriteInt(localDate.Year, Endianness.LittleEndian);
            bytes[SizeOfInt] = localDate.Month;
            bytes[SizeOfInt + SizeOfByte] = localDate.Day;
        }

        public static void WriteLocalTime(this Span<byte> bytes, HLocalTime localTime)
        {
            if (bytes.Length < SizeOfLocalTime)
                throw new ArgumentException(ExceptionMessages.NotEnoughBytes, nameof(bytes));

            bytes[0] = localTime.Hour;
            bytes[1] = localTime.Minute;
            bytes[2] = localTime.Second;
            bytes.Slice(SizeOfByte * 3).WriteInt(localTime.Nanosecond, Endianness.LittleEndian);
        }

        public static void WriteLocalDateTime(this Span<byte> bytes, HLocalDateTime localDateTime)
        {
            if (bytes.Length < SizeOfLocalDateTime)
                throw new ArgumentException(ExceptionMessages.NotEnoughBytes, nameof(bytes));

            bytes.WriteLocalDate(localDateTime.Date);
            bytes.Slice(SizeOfLocalDate).WriteLocalTime(localDateTime.Time);
        }

        public static void WriteOffsetDateTime(this Span<byte> bytes, HOffsetDateTime offsetDateTime)
        {
            if (bytes.Length < SizeOfOffsetDateTime)
                throw new ArgumentException(ExceptionMessages.NotEnoughBytes, nameof(bytes));

            bytes.WriteLocalDateTime(offsetDateTime.LocalDateTime);
            bytes.Slice(SizeOfLocalDateTime).WriteInt((int)offsetDateTime.Offset.TotalSeconds, Endianness.LittleEndian);
        }
    }
}
