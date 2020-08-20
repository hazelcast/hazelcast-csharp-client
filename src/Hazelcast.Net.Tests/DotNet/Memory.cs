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
using System.Buffers;
using NUnit.Framework;

namespace Hazelcast.Tests.DotNet
{
    [TestFixture]
    public class Memory
    {
        // purpose:
        // understand Span<>, Memory<>, ReadOnlyMemory<>, ReadOnlySequence<> etc.
        // understand how to efficiently de-serialize from the ReadOnlySequence<> obtained from the PipeReader

        // https://stackoverflow.com/questions/56979469/parse-utf8-string-from-readonlysequencebyte

        // read https://github.com/davidfowl/DocsStaging/blob/master/Buffers.md
        // ie https://docs.microsoft.com/en-us/dotnet/standard/io/buffers
        // and https://www.codemag.com/Article/1807051/Introducing-.NET-Core-2.1-Flagship-Types-Span-T-and-Memory-T
        //
        // ReadOnlySequence<T>
        //  T[]
        //  ReadOnlyMemory<T>
        //  linked list of ReadOnlySequenceSegment<T> + index start/end
        // exposes indexes as SequencePosition
        // exposes data as an enumerable of ReadOnlyMemory<T>

        // ReadOnlyMemory<byte> segment = sequence.First();
        // ReadOnlySpan<byte> span = segment.Span;
        //
        // SequencePosition position = sequence.Start;
        // sequence.TryGet(ref position, out ReadOnlyMemory<T> segment);
        // sequence.GetPosition(position, index); // offsets
        //
        // split code:
        //  fast, 1 single segment
        //  slow, multiple segments
        //    use SequenceReader<T>
        //    parse data segment by segment, reduce allocations, can be inefficient on small buffers
        //    copy to contiguous array
        //      less than 256 bytes, can be stack-allocated
        //      copy to a pooled array w/ ArrayPool<T>.Shared
        //
        // gotcha
        //  ReadOnlySequence<T> is bigger than an object reference and should be passed by in or ref
        //   where possible. This reduces copies of the struct.
        //  There can be empty segments
        //
        // can use SequenceReader, mutable struct, pass by ref, is a ref-struct = cannot be used in async = ?
        //
        // now, why would processing bytes be async?
        //  this is a buffer/cpu operation, no IO so can be purely sync

        //
        // there is a Utf8Parser
        // spans live on execution stack, can't be boxed nor put on heap
        // spans therefore cannot be used on async
        //
        // Memory<T> is like span but is not a ref type = higher level
        //
        // see also MemoryMarshal class

        [Test]
        public void Test()
        {

        }

        private void Parse(ref ReadOnlySequence<byte> bytes)
        {
            var slice = bytes.Slice(bytes.Start, 4); // slice the required bytes
            if (slice.IsSingleSegment)
            {
                var span = slice.FirstSpan();
                // and we can read from the span
            }
            else
            {
                Span<byte> stackBuffer = stackalloc byte[4];
                slice.CopyTo(stackBuffer);
                // and we can read from the buffer
            }

            // consume the slice
            bytes = bytes.Slice(slice.End);
        }

        // benchmarking = this is the fastest way to do it
        private int ReadInt32A(ref ReadOnlySequence<byte> bytes)
        {
            if (bytes.Length < 4)
                throw new ArgumentException("Not enough bytes.", nameof(bytes));

            var slice = bytes.Slice(bytes.Start, 4); // slice the required bytes
            int value;
            if (slice.IsSingleSegment)
            {
                var span = slice.FirstSpan();
                value = span.ReadInt32();
            }
            else
            {
                // copy to a temp buffer
                Span<byte> stackSpan = stackalloc byte[4];
                slice.CopyTo(stackSpan);
                value = ((ReadOnlySpan<byte>) stackSpan).ReadInt32();
            }

            // consume the slice
            bytes = bytes.Slice(slice.End);

            return value;
        }

        private int ReadInt32B(ref ReadOnlySequence<byte> bytes)
        {
            if (bytes.Length < 4)
                throw new ArgumentException("Not enough bytes.", nameof(bytes));

            var slice = bytes.Slice(bytes.Start, 4); // slice the required bytes
            int value;
            if (slice.IsSingleSegment)
            {
                var span = slice.FirstSpan();
                value = span.ReadInt32();
            }
            else
            {
                // enumerate sequence
                value = bytes.ReadInt32();
            }

            // consume the slice
            bytes = bytes.Slice(slice.End);

            return value;
        }

#if NETCOREAPP3_1 // SequenceReader is n/a in 2.1
        private int ReadInt32C(ref ReadOnlySequence<byte> bytes)
        {
            if (bytes.Length < 4)
                throw new ArgumentException("Not enough bytes.", nameof(bytes));

            var slice = bytes.Slice(bytes.Start, 4); // slice the required bytes
            int value;
            if (slice.IsSingleSegment)
            {
                var span = slice.FirstSpan();
                value = span.ReadInt32();
            }
            else
            {
                // use a reader
                var reader = new SequenceReader<byte>(bytes);
                reader.TryReadLittleEndian(out value);
            }

            // consume the slice
            bytes = bytes.Slice(slice.End);

            return value;
        }
#endif
    }

    public static class Extensions
    {
        //public static int ReadInt32(this Span<byte> bytes)
        //{
        //    return 33;
        //}

        // duplicating code from ByteArrayExtensions, we can do better!

        // works but slow
        public static int ReadInt32(this ReadOnlySequence<byte> buffer)
        {
            var value = 0;
            var bufferEnumerator = buffer.GetEnumerator();
            var byteCount = 0;
            while (byteCount < 4)
            {
                bufferEnumerator.MoveNext();
                var m = bufferEnumerator.Current;
                var spanIndex = 0;
                var spanLength = m.Span.Length;
                while (spanIndex < spanLength && byteCount < 4)
                {
                    value <<= 8;
                    value |= m.Span[spanIndex++];
                    byteCount++;
                }
            }

            return value;
        }


        public static int ReadInt32(this ReadOnlySpan<byte> bytes, bool bigEndian = false)
        {
            var position = 0;

            if (bytes.Length < position + sizeof(int))
                throw new ArgumentOutOfRangeException(nameof(position));

            unchecked
            {
                return bigEndian

                    ? bytes[position] << 24 | bytes[position + 1] << 16 |
                      bytes[position + 2] << 8 | bytes[position + 3]

                    : bytes[position] | bytes[position + 1] << 8 |
                      bytes[position + 2] << 16 | bytes[position + 3] << 24;
            }
        }
    }
}
