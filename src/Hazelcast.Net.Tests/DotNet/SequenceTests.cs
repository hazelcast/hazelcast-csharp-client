// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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

            Assert.AreEqual(0, sequence.Length);

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
