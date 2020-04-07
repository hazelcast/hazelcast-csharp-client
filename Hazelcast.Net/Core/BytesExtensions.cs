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
using System.Linq;
using System.Runtime.CompilerServices;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension methods to byte buffers.
    /// </summary>
    public static class BytesExtensions
    {
        /// <summary>
        /// Determines whether the default endian-ness is big-endian.
        /// </summary>
        public const bool BigEndian = false;

        /// <summary>
        /// Defines exception messages.
        /// </summary>
        private static class ExceptionMessage
        {
            public const string NotEnoughBytes = "Not enough bytes.";
        }

        /// <summary>
        /// Dumps an array of bytes into a readable string.
        /// </summary>
        /// <param name="bytes">The array of bytes.</param>
        /// <param name="prefix">A prefix.</param>
        /// <param name="length">The number of bytes to dump, or zero to dump all bytes in the array.</param>
        /// <returns>A readable string representation of the array of bytes.</returns>
        public static string Dump(this byte[] bytes, string prefix, int length = 0)
        {
            if (length > bytes.Length)
                throw new InvalidOperationException(ExceptionMessage.NotEnoughBytes);

            return prefix + string.Join(" ", bytes.Take(length > 0 ? length : bytes.Length).Select(x => $"{x:x2}"));
        }

        /// <summary>
        /// Dumps an sequence of bytes into a readable string.
        /// </summary>
        /// <param name="bytes">The sequence of bytes.</param>
        /// <param name="prefix">A prefix.</param>
        /// <param name="length">The number of bytes to dump, or zero to dump all bytes in the sequence.</param>
        /// <returns>A readable string representation of the sequence of bytes.</returns>
        public static string Dump(this ReadOnlySequence<byte> bytes, string prefix, int length = 0)
        {
            var a = new byte[bytes.Length];
            bytes.CopyTo(a);
            return prefix + string.Join(" ", a.Take(length > 0 ? length : (int) bytes.Length).Select(x => $"{x:x2}"));
        }

        /// <summary>
        /// Reads an <see cref="Int32"/> (int) value from a sequence of bytes, and slices the sequence accordingly.
        /// </summary>
        /// <param name="bytes">The sequence of bytes to read from.</param>
        /// <param name="bigEndian">Whether to use big-endian.</param>
        /// <returns>The value.</returns>
        public static int ReadInt32(ref ReadOnlySequence<byte> bytes, bool bigEndian = BigEndian)
        {
            if (bytes.Length < 4)
                throw new ArgumentException(ExceptionMessage.NotEnoughBytes, nameof(bytes));

            var slice = bytes.Slice(bytes.Start, 4);
            int value;
            if (slice.IsSingleSegment)
            {
                var span = slice.FirstSpan;
                value = span.ReadInt32(bigEndian);
            }
            else
            {
                Span<byte> stackSpan = stackalloc byte[4];
                slice.Fill(stackSpan);
                value = ((ReadOnlySpan<byte>) stackSpan).ReadInt32(bigEndian);
            }

            bytes = bytes.Slice(slice.End);
            return value;
        }

        /// <summary>
        /// Reads an <see cref="UInt16"/> (ushort) value from a sequence of bytes, and slices the sequence accordingly.
        /// </summary>
        /// <param name="bytes">The sequence of bytes to read from.</param>
        /// <param name="bigEndian">Whether to use big-endian.</param>
        /// <returns>The value.</returns>
        public static ushort ReadUInt16(ref ReadOnlySequence<byte> bytes, bool bigEndian = BigEndian)
        {
            const byte length = sizeof(ushort);

            if (bytes.Length < length)
                throw new ArgumentException(ExceptionMessage.NotEnoughBytes, nameof(bytes));

            var slice = bytes.Slice(bytes.Start, length);
            ushort value;
            if (slice.IsSingleSegment)
            {
                var span = slice.FirstSpan;
                value = span.ReadUInt16(bigEndian);
            }
            else
            {
                Span<byte> stackSpan = stackalloc byte[length];
                slice.Fill(stackSpan);
                value = ((ReadOnlySpan<byte>) stackSpan).ReadUInt16(bigEndian);
            }

            bytes = bytes.Slice(slice.End);
            return value;
        }

        /// <summary>
        /// Reads an <see cref="Int32"/> (int) value from a span of bytes.
        /// </summary>
        /// <param name="bytes">The span of bytes to read from.</param>
        /// <param name="bigEndian">Whether to use big-endian.</param>
        /// <returns>The value.</returns>
        public static int ReadInt32(this ReadOnlySpan<byte> bytes, bool bigEndian = BigEndian)
        {
            const byte length = sizeof(int);

            if (bytes.Length < length)
                throw new ArgumentException(ExceptionMessage.NotEnoughBytes, nameof(bytes));

            return bigEndian

                ? bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3]
                : bytes[0] | bytes[1] << 8 | bytes[2] << 16 | bytes[3] << 24;
        }

        /// <summary>
        /// Reads an <see cref="UInt16"/> (ushort) value from a span of bytes.
        /// </summary>
        /// <param name="bytes">The span of bytes to read from.</param>
        /// <param name="bigEndian">Whether to use big-endian.</param>
        /// <returns>The value.</returns>
        public static ushort ReadUInt16(this ReadOnlySpan<byte> bytes, bool bigEndian = BigEndian)
        {
            const byte length = sizeof(ushort);

            if (bytes.Length < length)
                throw new ArgumentException(ExceptionMessage.NotEnoughBytes, nameof(bytes));

            unchecked
            {
                return (ushort) (bigEndian

                    ? bytes[0] << 8 | bytes[1]
                    : bytes[0] | bytes[1] << 8);
            }
        }

        /// <summary>
        /// Reads an <see cref="Int32"/> (int) value from an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to read from.</param>
        /// <param name="bigEndian">Whether to use big-endian.</param>
        /// <returns>The value.</returns>
        public static int ReadInt32(this byte[] bytes, int position, bool bigEndian = BigEndian)
        {
            if (bytes.Length < position + sizeof(int))
                throw new ArgumentOutOfRangeException(nameof(position));

            unchecked
            {
                return bigEndian

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
        /// <param name="bigEndian">Whether to use big-endian.</param>
        /// <returns>The value.</returns>
        public static long ReadInt64(this byte[] bytes, int position, bool bigEndian = BigEndian)
        {
            if (bytes.Length < position + sizeof(long))
                throw new ArgumentOutOfRangeException(nameof(position));

            unchecked
            {
                return bigEndian

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
        /// Writes an <see cref="UInt16"/> (ushort) value to an array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to write to.</param>
        /// <param name="position">The position in the array where the value should be written.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="bigEndian">Whether to use big-endian.</param>
        public static void WriteUInt16(this byte[] bytes, int position, ushort value, bool bigEndian = BigEndian)
        {
            if (bytes.Length < position + sizeof(ushort))
                throw new ArgumentOutOfRangeException(nameof(position));

            var unsigned = value;

            unchecked
            {
                if (bigEndian)
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
        /// <param name="bigEndian">Whether to use big-endian.</param>
        public static void WriteInt32(this byte[] bytes, int position, int value, bool bigEndian = BigEndian)
        {
            if (bytes.Length < position + sizeof(int))
                throw new ArgumentOutOfRangeException(nameof(position));

            var unsigned = (uint) value;

            unchecked
            {
                if (bigEndian)
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
        /// <param name="bigEndian">Whether to use big-endian.</param>
        public static void WriteInt64(this byte[] bytes, int position, long value, bool bigEndian = BigEndian)
        {
            if (bytes.Length < position + sizeof(long))
                throw new ArgumentOutOfRangeException(nameof(position));

            var unsigned = (ulong) value;

            unchecked
            {
                if (bigEndian)
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
                throw new ArgumentOutOfRangeException(ExceptionMessage.NotEnoughBytes, nameof(source));

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