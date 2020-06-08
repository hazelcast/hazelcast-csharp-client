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
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Hazelcast.Core;

#pragma warning disable

namespace AsyncTests1.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("AsyncBench");
            BenchmarkRunner.Run<Bench3>();
        }
    }

    [MemoryDiagnoser]
    public class Bench3
    {
        public int I;
        public int J;

        /*
            |  Method |     Mean |     Error |    StdDev |   Median |  Gen 0 | Gen 1 | Gen 2 | Allocated |
            |-------- |---------:|----------:|----------:|---------:|-------:|------:|------:|----------:|
            | BenchW1 | 2.672 us | 0.0512 us | 0.0548 us | 2.678 us | 0.2060 |     - |     - |     863 B |
            | BenchW2 | 2.657 us | 0.0528 us | 0.0740 us | 2.624 us | 0.2060 |     - |     - |     864 B |
            | BenchW3 | 2.466 us | 0.0424 us | 0.0709 us | 2.464 us | 0.1755 |     - |     - |     742 B |
            | BenchN1 | 2.709 us | 0.0534 us | 0.0730 us | 2.705 us | 0.2136 |     - |     - |     900 B |
            | BenchN2 | 2.680 us | 0.0497 us | 0.0788 us | 2.642 us | 0.2136 |     - |     - |     901 B |
            | BenchN3 | 2.497 us | 0.0493 us | 0.0782 us | 2.493 us | 0.1831 |     - |     - |     772 B |
        */

        // lessons:
        // Bench?3 generally faster & allocates less = best to avoid the async/await state machine whenever possible
        // BenchW? slightly faster  so could make sense
        //
        // when there is a state machine the compiler seems clever enough to use it also for the capture
        // so BenchN3 is the only one that creates a capture class, others capture the values in the
        // state, but yet avoiding this seems slightly faster + allocating slightly less

        [Benchmark]
        public async Task BenchW1() => await Args1(1, 1);

        [Benchmark]
        public async Task BenchW2() => await Args2(1, 1);

        [Benchmark]
        public async Task BenchW3() => await Args3(1, 1);

        [Benchmark]
        public async Task BenchN1() => await NoArgs1(1, 1);

        [Benchmark]
        public async Task BenchN2() => await NoArgs2(1, 1);

        [Benchmark]
        public async Task BenchN3() => await NoArgs3(1, 1);

        public async Task Args1(int i, int j)
            => await TaskEx.WithTimeout(F, i, j, TimeSpan.Zero, 1);

        public async Task Args2(int i, int j)
            => await TaskEx.WithTimeout(F, i, j, TimeSpan.Zero, 1);

        public Task Args3(int i, int j)
            => TaskEx.WithTimeout(F, i, j, TimeSpan.Zero, 1);

        public async Task NoArgs1(int i, int j)
            => TaskEx.WithTimeout((token) => F(i, j, token), TimeSpan.Zero, 1);

        public async Task NoArgs2(int i, int j)
            => TaskEx.WithTimeout((token) => F(i, j, token), TimeSpan.Zero, 1);

        public Task NoArgs3(int i, int j)
            => TaskEx.WithTimeout((token) => F(i, j, token), TimeSpan.Zero, 1);

        public async Task<int> F(int i, int j, CancellationToken cancellationToken)
        {
            await Task.Yield();
            return 3;
        }
    }

    [MemoryDiagnoser]
    public class Bench2
    {
        private readonly object _lock = new object();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private int i;

        // so... semaphore would be 3x slower that plain lock
        // but plain lock cannot contain async code ;(

        [Benchmark]
        public Task Lock()
        {
            lock (_lock)
            {
                i++;
            }

            return Task.CompletedTask;
        }

        [Benchmark]
        public async Task Semaphore()
        {
            await _semaphore.WaitAsync(); //.CAF();
            i++;
            _semaphore.Release();
        }
    }

    [MemoryDiagnoser]
    public class Bench
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
