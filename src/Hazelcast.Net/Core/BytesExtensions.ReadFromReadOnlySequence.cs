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
using Hazelcast.Exceptions;

namespace Hazelcast.Core
{
    internal static partial class BytesExtensions // Read from ReadOnlySequence
    {
        /// <summary>
        /// Reads an <see cref="ushort"/> value from a sequence of bytes, and slices the sequence accordingly.
        /// </summary>
        /// <param name="bytes">The sequence of bytes to read from.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        public static ushort ReadUShort(ref ReadOnlySequence<byte> bytes, Endianness endianness = Endianness.Unspecified)
        {
            const byte length = sizeof(ushort);

            if (bytes.Length < length)
                throw new ArgumentException(ExceptionMessages.NotEnoughBytes, nameof(bytes));

            var slice = bytes.Slice(bytes.Start, length);
            ushort value;
            if (slice.IsSingleSegment)
            {
                var span = slice.FirstSpan();
                value = span.ReadUShort(endianness);
            }
            else
            {
                Span<byte> stackSpan = stackalloc byte[length];
                slice.Fill(stackSpan);
                value = ((ReadOnlySpan<byte>)stackSpan).ReadUShort(endianness);
            }

            bytes = bytes.Slice(slice.End);
            return value;
        }

        /// <summary>
        /// Reads an <see cref="int"/> value from a sequence of bytes, and slices the sequence accordingly.
        /// </summary>
        /// <param name="bytes">The sequence of bytes to read from.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        public static int ReadInt(ref ReadOnlySequence<byte> bytes, Endianness endianness = Endianness.Unspecified)
        {
            if (bytes.Length < 4)
                throw new ArgumentException(ExceptionMessages.NotEnoughBytes, nameof(bytes));

            var slice = bytes.Slice(bytes.Start, 4);
            int value;
            if (slice.IsSingleSegment)
            {
                var span = slice.FirstSpan();
                value = span.ReadInt(endianness);
            }
            else
            {
                Span<byte> stackSpan = stackalloc byte[4];
                slice.Fill(stackSpan);
                value = ((ReadOnlySpan<byte>)stackSpan).ReadInt(endianness);
            }

            bytes = bytes.Slice(slice.End);
            return value;
        }
    }
}
