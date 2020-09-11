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
using Hazelcast.Exceptions;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension methods to byte buffers.
    /// </summary>
    public static partial class BytesExtensions
    {
        /// <summary>
        /// Gets the size of a <see cref="byte"/> value in arrays or sequences of bytes.
        /// </summary>
        public const int SizeOfByte = 1;

        /// <summary>
        /// Gets the size of a <see cref="short"/> value in arrays or sequences of bytes.
        /// </summary>
        public const int SizeOfShort = 2;

        /// <summary>
        /// Gets the size of an <see cref="ushort"/> value in arrays or sequences of bytes.
        /// </summary>
        public const int SizeOfUnsignedShort = 2;

        /// <summary>
        /// Gets the size of an <see cref="int"/> value in arrays or sequences of bytes.
        /// </summary>
        public const int SizeOfInt = 4;

        /// <summary>
        /// Gets the size of an <see cref="uint"/> value in arrays or sequences of bytes.
        /// </summary>
        public const int SizeOfUnsignedInt = 4;

        /// <summary>
        /// Gets the size of a <see cref="float"/> value in arrays or sequences of bytes.
        /// </summary>
        public const int SizeOfFloat = 4;

        /// <summary>
        /// Gets the size of a <see cref="long"/> value in arrays or sequences of bytes.
        /// </summary>
        public const int SizeOfLong = 8;

        /// <summary>
        /// Gets the size of an <see cref="ulong"/> value in arrays or sequences of bytes.
        /// </summary>
        public const int SizeOfUnsignedLong = 8;

        /// <summary>
        /// Gets the size of a <see cref="double"/> value in arrays or sequences of bytes.
        /// </summary>
        public const int SizeOfDouble = 8;

        /// <summary>
        /// Gets the size of a <see cref="decimal"/> value in arrays or sequences of bytes.
        /// </summary>
        public const int SizeOfDecimal = 16;



        /// <summary>
        /// Gets the size of a <see cref="bool"/> value in arrays or sequences of bytes.
        /// </summary>
        public const int SizeOfBool = 1;



        /// <summary>
        /// Gets the size of a <see cref="char"/> value in arrays or sequences of bytes.
        /// </summary>
        public const int SizeOfChar = 2;



        /// <summary>
        /// Gets the size of a <see cref="Guid"/> value in arrays or sequences of bytes.
        /// </summary>
        public const int SizeOfGuid = (1 + 16) * SizeOfByte;






        /// <summary>
        /// Copies a sequence of <typeparamref name="T"/> to a span of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the items in the sequence and span.</typeparam>
        /// <param name="source">The sequence of <typeparamref name="T"/> to copy from.</param>
        /// <param name="destination">The span of <typeparamref name="T"/> to copy to.</param>
        /// <remarks>
        /// <para>There must be enough items in the sequence to fill the span. There can be more
        /// items in the sequence than in the span, and extra items will be ignored.</para>
        /// <para>Slices the sequence of the used items.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Fill<T>(ref this ReadOnlySequence<T> source, Span<T> destination)
        {
            if (source.Length < destination.Length)
                throw new ArgumentOutOfRangeException(nameof(source), ExceptionMessages.NotEnoughBytes);

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

            source = source.Slice(destination.Length);
        }

        /// <summary>
        /// Fills a span of <typeparamref name="T"/> from a sequence.
        /// </summary>
        /// <typeparam name="T">The type of the items in the sequence and span.</typeparam>
        /// <param name="destination">The span of <typeparamref name="T"/> to copy to.</param>
        /// <param name="source">The sequence of <typeparamref name="T"/> to copy from.</param>
        /// <remarks>
        /// <para>There must be enough items in the sequence to fill the span. There can be more
        /// items in the sequence than in the span, and extra items will be ignored.</para>
        /// <para>Slices the sequence of the used items.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Fill<T>(this Span<T> destination, ref ReadOnlySequence<T> source)
        {
            if (source.Length < destination.Length)
                throw new ArgumentOutOfRangeException(nameof(source), ExceptionMessages.NotEnoughBytes);

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

            source = source.Slice(destination.Length);
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
                byteCount -= span.Length;
            }
        }
    }
}
