using System;
using System.Buffers;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.DotNet
{
    [TestFixture]
    public class SequenceTests
    {
        // read https://www.stevejgordon.co.uk/creating-a-readonlysequence-from-array-data-in-dotnet

        [Test]
        public void CreateReadOnlySequence()
        {
            var arrayOne = new[] { 0, 1, 2, 3, 4 };
            var arrayTwo = new[] { 5, 6, 7, 8, 9 };
            var arrayThree = new[] { 10, 11, 12, 13, 14 };

            var firstSegment = new MemorySegment<int>(arrayOne);
            var lastSegment = firstSegment.Append(arrayTwo).Append(arrayThree);

            var sequence = new ReadOnlySequence<int>(firstSegment, 0, lastSegment, lastSegment.Memory.Length);

            var ints = new Span<int>(new int[arrayOne.Length + arrayTwo.Length + arrayThree.Length]);
            sequence.Fill(ints);

            var i = 0;
            for (var j = 0; j < arrayOne.Length; j++, i++)
                Assert.AreEqual(arrayOne[j], ints[i]);
            for (var j = 0; j < arrayTwo.Length; j++, i++)
                Assert.AreEqual(arrayTwo[j], ints[i]);
            for (var j = 0; j < arrayThree.Length; j++, i++)
                Assert.AreEqual(arrayThree[j], ints[i]);

            // but maybe creating segments is expensive and a pipe would be better...
        }

        private class MemorySegment<T> : ReadOnlySequenceSegment<T>
        {
            public MemorySegment(ReadOnlyMemory<T> memory)
            {
                Memory = memory;
            }

            public MemorySegment<T> Append(ReadOnlyMemory<T> memory)
            {
                var segment = new MemorySegment<T>(memory)
                {
                    RunningIndex = RunningIndex + Memory.Length
                };

                Next = segment;

                return segment;
            }
        }
    }
}
