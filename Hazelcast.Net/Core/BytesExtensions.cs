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
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Hazelcast.Exceptions;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension methods to byte buffers.
    /// </summary>
    public static class BytesExtensions
    {
        /// <summary>
        /// Gets the native endianness of the computer architecture where the code is executing.
        /// </summary>
        public static Endianness NativeEndianness => BitConverter.IsLittleEndian ? Endianness.LittleEndian : Endianness.BigEndian;

        /// <summary>
        /// Resolves the endianness.
        /// </summary>
        /// <param name="endianness">The endianness, which can be 'native'.</param>
        /// <returns>The resolved endianness, i.e. either 'big-endian' or 'little-endian'.</returns>
        private static Endianness ResolveEndianness(Endianness endianness)
        {
            switch (endianness)
            {
                case Endianness.Native:
                    return NativeEndianness;
                case Endianness.LittleEndian:
                case Endianness.BigEndian:
                    return endianness;
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Reads an <see cref="Int32"/> (int) value from a sequence of bytes, and slices the sequence accordingly.
        /// </summary>
        /// <param name="bytes">The sequence of bytes to read from.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        public static int ReadInt32(ref ReadOnlySequence<byte> bytes, Endianness endianness = Endianness.Native)
        {
            if (bytes.Length < 4)
                throw new ArgumentException(ExceptionMessages.NotEnoughBytes, nameof(bytes));

            var slice = bytes.Slice(bytes.Start, 4);
            int value;
            if (slice.IsSingleSegment)
            {
                var span = slice.FirstSpan();
                value = span.ReadInt32(endianness);
            }
            else
            {
                Span<byte> stackSpan = stackalloc byte[4];
                slice.Fill(stackSpan);
                value = ((ReadOnlySpan<byte>) stackSpan).ReadInt32(endianness);
            }

            bytes = bytes.Slice(slice.End);
            return value;
        }

        /// <summary>
        /// Reads an <see cref="UInt16"/> (ushort) value from a sequence of bytes, and slices the sequence accordingly.
        /// </summary>
        /// <param name="bytes">The sequence of bytes to read from.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        public static ushort ReadUInt16(ref ReadOnlySequence<byte> bytes, Endianness endianness = Endianness.Native)
        {
            const byte length = sizeof(ushort);

            if (bytes.Length < length)
                throw new ArgumentException(ExceptionMessages.NotEnoughBytes, nameof(bytes));

            var slice = bytes.Slice(bytes.Start, length);
            ushort value;
            if (slice.IsSingleSegment)
            {
                var span = slice.FirstSpan();
                value = span.ReadUInt16(endianness);
            }
            else
            {
                Span<byte> stackSpan = stackalloc byte[length];
                slice.Fill(stackSpan);
                value = ((ReadOnlySpan<byte>) stackSpan).ReadUInt16(endianness);
            }

            bytes = bytes.Slice(slice.End);
            return value;
        }

        /// <summary>
        /// Reads an <see cref="Int32"/> (int) value from a span of bytes.
        /// </summary>
        /// <param name="bytes">The span of bytes to read from.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        public static int ReadInt32(this ReadOnlySpan<byte> bytes, Endianness endianness = Endianness.Native)
        {
            const byte length = sizeof(int);

            if (bytes.Length < length)
                throw new ArgumentException(ExceptionMessages.NotEnoughBytes, nameof(bytes));

            return ResolveEndianness(endianness).IsBigEndian()

                ? bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3]
                : bytes[0] | bytes[1] << 8 | bytes[2] << 16 | bytes[3] << 24;
        }

        /// <summary>
        /// Reads an <see cref="UInt16"/> (ushort) value from a span of bytes.
        /// </summary>
        /// <param name="bytes">The span of bytes to read from.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        public static ushort ReadUInt16(this ReadOnlySpan<byte> bytes, Endianness endianness = Endianness.Native)
        {
            const byte length = sizeof(ushort);

            if (bytes.Length < length)
                throw new ArgumentException(ExceptionMessages.NotEnoughBytes, nameof(bytes));

            unchecked
            {
                return (ushort) (ResolveEndianness(endianness).IsBigEndian()

                    ? bytes[0] << 8 | bytes[1]
                    : bytes[0] | bytes[1] << 8);
            }
        }

        /// <summary>
        /// Reads an <see cref="Int32"/> (int) value from an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to read from.</param>
        /// <param name="position">The position in the array where the value should be read.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        public static int ReadInt32(this byte[] bytes, int position, Endianness endianness = Endianness.Native)
        {
            if (bytes.Length < position + sizeof(int))
                throw new ArgumentOutOfRangeException(nameof(position));

            unchecked
            {
                return ResolveEndianness(endianness).IsBigEndian()

                    ? bytes[position] << 24     | bytes[position + 1] << 16 |
                      bytes[position + 2] << 8  | bytes[position + 3]

                    : bytes[position]           | bytes[position + 1] << 8 |
                      bytes[position + 2] << 16 | bytes[position + 3] << 24;
            }
        }

        /// <summary>
        /// Reads an <see cref="Int64"/> (long) value from an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to read from.</param>
        /// <param name="position">The position in the array where the value should be read.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        public static long ReadInt64(this byte[] bytes, int position, Endianness endianness = Endianness.Native)
        {
            if (bytes.Length < position + sizeof(long))
                throw new ArgumentOutOfRangeException(nameof(position));

            unchecked
            {
                return ResolveEndianness(endianness).IsBigEndian()

                    ? (long) bytes[position]     << 56 | (long) bytes[position + 1] << 48 |
                      (long) bytes[position + 2] << 40 | (long) bytes[position + 3] << 32 |
                      (long) bytes[position + 4] << 24 | (long) bytes[position + 5] << 16 |
                      (long) bytes[position + 6] << 8  | (long) bytes[position + 7]

                    : (long) bytes[position]           | (long) bytes[position + 1] << 8  |
                      (long) bytes[position + 2] << 16 | (long) bytes[position + 3] << 24 |
                      (long) bytes[position + 4] << 32 | (long) bytes[position + 5] << 40 |
                      (long) bytes[position + 6] << 48 | (long) bytes[position + 7] << 56;
            }
        }

        /// <summary>
        /// Reads a <see cref="Byte"/> (byte) value from an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to read from.</param>
        /// <param name="position">The position in the array where the value should be read.</param>
        /// <returns>The value.</returns>
        public static byte ReadByte(this byte[] bytes, int position)
        {
            if (bytes.Length < position + sizeof(byte))
                throw new ArgumentOutOfRangeException(nameof(position));

            return bytes[position];
        }

        /// <summary>
        /// Writes a <see cref="Byte"/> (byte) value to an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to write to.</param>
        /// <param name="position">The position in the array where the value should be written.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteByte(this byte[] bytes, int position, byte value)
        {
            if (bytes.Length < position + sizeof(byte))
                throw new ArgumentOutOfRangeException(nameof(position));

            bytes[position] = value;
        }

        /// <summary>
        /// Writes an <see cref="UInt16"/> (ushort) value to an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to write to.</param>
        /// <param name="position">The position in the array where the value should be written.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="endianness">The endianness.</param>
        public static void WriteUInt16(this byte[] bytes, int position, ushort value, Endianness endianness = Endianness.Native)
        {
            if (bytes.Length < position + sizeof(ushort))
                throw new ArgumentOutOfRangeException(nameof(position));

            var unsigned = value;

            unchecked
            {
                if (ResolveEndianness(endianness).IsBigEndian())
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

        /// <summary>
        /// Writes an <see cref="Int32"/> (int) value to an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to write to.</param>
        /// <param name="position">The position in the array where the value should be written.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="endianness">The endianness.</param>
        public static void WriteInt32(this byte[] bytes, int position, int value, Endianness endianness = Endianness.Native)
        {
            if (bytes.Length < position + sizeof(int))
                throw new ArgumentOutOfRangeException(nameof(position));

            var unsigned = (uint) value;

            unchecked
            {
                if (ResolveEndianness(endianness).IsBigEndian())
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
        /// Writes an <see cref="Int64"/> (long) value to an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to write to.</param>
        /// <param name="position">The position in the array where the value should be written.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="endianness">The endianness.</param>
        public static void WriteInt64(this byte[] bytes, int position, long value, Endianness endianness = Endianness.Native)
        {
            if (bytes.Length < position + sizeof(long))
                throw new ArgumentOutOfRangeException(nameof(position));

            var unsigned = (ulong) value;

            unchecked
            {
                if (ResolveEndianness(endianness).IsBigEndian())
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
        /// Reads a <see cref="Boolean"/> (bool) value from an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to read from.</param>
        /// <param name="position">The position in the array where the value should be read.</param>
        /// <returns>The value.</returns>
        public static bool ReadBool(this byte[] bytes, int position)
            => bytes.ReadByte(position) == 0x01;

        /// <summary>
        /// Writes a <see cref="Boolean"/> (bool) value to an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to write to.</param>
        /// <param name="position">The position in the array where the value should be written.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteBool(this byte[] bytes, int position, bool value)
            => bytes.WriteByte(position, value ? (byte) 0x01 : (byte) 0x00);

        /// <summary>
        /// Gets the size of a <see cref="Guid"/> in arrays or sequences of bytes.
        /// </summary>
        public const int SizeOfGuid = (1 + 16) * SizeOfByte;

        /// <summary>
        /// Gets the size of a <see cref="Byte"/> (byte) in arrays or sequences of bytes.
        /// </summary>
        public const int SizeOfByte = 1;

        /// <summary>
        /// Gets the size of a <see cref="Boolean"/> (bool) in arrays or sequences of bytes.
        /// </summary>
        public const int SizeOfBool = 1;

        /// <summary>
        /// Gets the size of a <see cref="Int32"/> (int) in arrays or sequences of bytes.
        /// </summary>
        public const int SizeOfInt32 = 4;

        /// <summary>
        /// Gets the size of an <see cref="Int64"/> (long) in arrays or sequences of bytes.
        /// </summary>
        public const int SizeOfInt64 = 8;

        /// <summary>
        /// Reads a <see cref="Guid"/> value from an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to read from.</param>
        /// <param name="position">The position in the array where the value should be read.</param>
        /// <returns></returns>
        public static Guid ReadGuid(this byte[] bytes, int position)
        {
            if (bytes.Length < position + SizeOfGuid)
                throw new ArgumentOutOfRangeException(nameof(position));

            if (bytes.ReadBool(position)) return Guid.Empty;
            position += SizeOfByte;

            // ReSharper disable once UseObjectOrCollectionInitializer
#pragma warning disable IDE0017 // Simplify object initialization
            var v = new JavaUUIDOrder();
#pragma warning restore IDE0017 // Simplify object initialization

            v.X0 = bytes[position]; position += SizeOfByte;
            v.X1 = bytes[position]; position += SizeOfByte;
            v.X2 = bytes[position]; position += SizeOfByte;
            v.X3 = bytes[position]; position += SizeOfByte;

            v.X4 = bytes[position]; position += SizeOfByte;
            v.X5 = bytes[position]; position += SizeOfByte;
            v.X6 = bytes[position]; position += SizeOfByte;
            v.X7= bytes[position]; position += SizeOfByte;

            v.X8= bytes[position]; position += SizeOfByte;
            v.X9 = bytes[position]; position += SizeOfByte;
            v.XA = bytes[position]; position += SizeOfByte;
            v.XB = bytes[position]; position += SizeOfByte;

            v.XC = bytes[position]; position += SizeOfByte;
            v.XD = bytes[position]; position += SizeOfByte;
            v.XE = bytes[position]; position += SizeOfByte;
            v.XF = bytes[position]; //position += SizeOfByte;

            return v.Value;
        }

        /// <summary>
        /// Writes a <see cref="Guid"/> value to an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to write to.</param>
        /// <param name="position">The position in the array where the value should be written.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteGuid(this byte[] bytes, int position, Guid value)
        {
            if (bytes.Length < position + SizeOfGuid)
                throw new ArgumentOutOfRangeException(nameof(position));

            bytes.WriteBool(position, value == Guid.Empty);
            if (value == Guid.Empty) return;
            position += SizeOfByte;

            var v = new JavaUUIDOrder { Value = value };

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
            bytes[position] = v.XF; //position += SizeOfByte;
        }

        // the following GUID: "00010203-0405-0607-0809-0a0b0c0d0e0f" is:
        // in .NET ToArray   as 3, 2, 1, 0,     5, 4, 7, 6,     8,   9, 10, 11,     12, 13, 14, 15
        // in Java UUIDCodec as 7, 6, 5, 4,     3, 2, 1, 0,     15, 14, 13, 12,     11, 10, 9,  8
        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        internal struct JavaUUIDOrder
        {
            [FieldOffset(0)] public Guid Value;

            [FieldOffset(6)] public byte X0;
            [FieldOffset(7)] public byte X1;
            [FieldOffset(4)] public byte X2;
            [FieldOffset(5)] public byte X3;

            [FieldOffset(0)] public byte X4;
            [FieldOffset(1)] public byte X5;
            [FieldOffset(2)] public byte X6;
            [FieldOffset(3)] public byte X7;

            [FieldOffset(15)] public byte X8;
            [FieldOffset(14)] public byte X9;
            [FieldOffset(13)] public byte XA;
            [FieldOffset(12)] public byte XB;

            [FieldOffset(11)] public byte XC;
            [FieldOffset(10)] public byte XD;
            [FieldOffset(9)] public byte XE;
            [FieldOffset(8)] public byte XF;
        }

        /// <summary>
        /// Copies a sequence of <typeparamref name="T"/> to a span of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the items in the sequence and span.</typeparam>
        /// <param name="source">The sequence of <typeparamref name="T"/> to copy from.</param>
        /// <param name="destination">The span of <typeparamref name="T"/> to copy to.</param>
        /// <remarks>
        /// <para>There must be enough items in the sequence to fill the span. There can be more
        /// items in the sequence than in the span, and extra items will be ignored.</para>
        /// FIXME should advance the sequence
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Fill<T>(in this ReadOnlySequence<T> source, Span<T> destination)
        {
            if (source.Length < destination.Length)
                throw new ArgumentOutOfRangeException(ExceptionMessages.NotEnoughBytes, nameof(source));

            if (source.IsSingleSegment)
            {
                var span = source.First.Span;
                if (span.Length > destination.Length)
                    span = span.Slice(0, destination.Length);
                span.CopyTo(destination);
            }
            else
            {
                FillMultiSegment(source, destination);
            }
        }

        /// <summary>
        /// Copies a multi-segment sequence of <typeparamref name="T"/> to a span of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the items in the sequence and span.</typeparam>
        /// <param name="source">The sequence of <typeparamref name="T"/> to copy from.</param>
        /// <param name="destination">The span of <typeparamref name="T"/> to copy to.</param>
        private static void FillMultiSegment<T>(in ReadOnlySequence<T> source, Span<T> destination)
        {
            //if (sequence.Length < destination.Length)
            //    throw new ArgumentOutOfRangeException(ExceptionMessage.NotEnoughBytes, nameof(sequence));

            var position = source.Start;
            var byteCount = destination.Length;
            while (source.TryGet(ref position, out var memory))
            {
                var span = memory.Span;
                if (span.Length > byteCount)
                {
                    span.Slice(0, byteCount).CopyTo(destination);
                    break;
                }

                span.CopyTo(destination);
                destination = destination.Slice(span.Length);
            }
        }
    }
}