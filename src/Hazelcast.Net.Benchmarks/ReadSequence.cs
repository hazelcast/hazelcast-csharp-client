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
using System.Buffers;
using BenchmarkDotNet.Attributes;

namespace Hazelcast.Benchmarks
{
    public class ReadSequence
    {
        public ReadOnlySequence<byte> CreateSequence()
        {
            var segment1 = new Segment<byte>(new byte[] { 0, 0 });
            var segment2 = segment1.Append(new byte[] { 0, 0 });
            var bytes = new ReadOnlySequence<byte>(segment1, 0, segment2, 2);
            return bytes;
        }

        public void CreateSequence(out ReadOnlySequence<byte> bytes)
        {
            var segment1 = new Segment<byte>(new byte[] { 0, 0 });
            var segment2 = segment1.Append(new byte[] { 0, 0 });
            bytes = new ReadOnlySequence<byte>(segment1, 0, segment2, 2);
        }

        // so ... bench A is fastest

        [Benchmark]
        public void BenchA()
        {
            var bytes = CreateSequence();
            var value = ReadInt32A(ref bytes);
        }

        [Benchmark]
        public void BenchB()
        {
            var bytes = CreateSequence();
            var value = ReadInt32B(ref bytes);
        }

        [Benchmark]
        public void BenchC()
        {
            var bytes = CreateSequence();
            var value = ReadInt32C(ref bytes);
        }

        public class Segment<T> : ReadOnlySequenceSegment<T>
        {
            public Segment(ReadOnlyMemory<T> memory)
                => Memory = memory;

            public Segment<T> Append(ReadOnlyMemory<T> memory)
            {
                var segment = new Segment<T>(memory);
                segment.RunningIndex = RunningIndex + Memory.Length;
                Next = segment;
                return segment;
            }
        }

        private int ReadInt32A(ref ReadOnlySequence<byte> bytes)
        {
            if (bytes.Length < 4)
                throw new ArgumentException("Not enough bytes.", nameof(bytes));

            var slice = bytes.Slice(bytes.Start, 4); // slice the required bytes
            int value;
            if (slice.IsSingleSegment)
                throw new NotSupportedException(); // just to be sure

            // copy to a temp buffer
            Span<byte> stackSpan = stackalloc byte[4];
            slice.CopyTo(stackSpan);
            value = ((ReadOnlySpan<byte>)stackSpan).ReadInt32();

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
                throw new NotSupportedException(); // just to be sure

            // enumerate sequence
            value = bytes.ReadInt32();

            // consume the slice
            bytes = bytes.Slice(slice.End);

            return value;
        }

        private int ReadInt32C(ref ReadOnlySequence<byte> bytes)
        {
            if (bytes.Length < 4)
                throw new ArgumentException("Not enough bytes.", nameof(bytes));

            var slice = bytes.Slice(bytes.Start, 4); // slice the required bytes
            int value;
            if (slice.IsSingleSegment)
                throw new NotSupportedException(); // just to be sure

            // use a reader
            var reader = new SequenceReader<byte>(bytes);
            reader.TryReadLittleEndian(out value);

            // consume the slice
            bytes = bytes.Slice(slice.End);

            return value;
        }
    }

    public static class Extensions
    {
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

        public static int ReadInt32(this byte[] bytes, int position, bool bigEndian = false)
        {
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
    }
}
