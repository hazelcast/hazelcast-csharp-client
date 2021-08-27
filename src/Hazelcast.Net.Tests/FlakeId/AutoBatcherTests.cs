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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.FlakeId;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.FlakeId
{
    [TestFixture]
    public class AutoBatcherTests
    {
        [Test]
        [TestCase(1, 10)]
        [TestCase(10, 1)]
        [TestCase(5, 5)]
        [TestCase(100, 10)]
        public async Task GetNextId_Concurrent(int batchSize, int batchesCount)
        {
            Batch[] BatchesFactory() => Enumerable.Range(0, batchesCount)
                .Select(i => new Batch(i * batchSize, 1, batchSize, Timeout.InfiniteTimeSpan))
                .ToArray();

            var batches = BatchesFactory();
            var supplyCallCount = 0;

            var autoBatcher = new AutoBatcher(() =>
            {
                var callNumber = Interlocked.Increment(ref supplyCallCount) - 1;
                return Task.FromResult(batches[callNumber]);
            });

            var ids = await TaskEx.RunConcurrently(
                _ => autoBatcher.GetNextIdAsync(),
                batchesCount * batchSize
            );

            Assert.AreEqual(batchesCount, supplyCallCount);
            CollectionAssert.AreEquivalent(
                expected: BatchesFactory().SelectMany(b => b.Enumerate()),
                actual: ids
            );
        }

        [Test]
        [TestCase(5, 5)]
        [TestCase(100, 10)]
        public async Task GetNextId_Exception(int batchSize, int batchesCount)
        {
            var supplyCallCount = 0;
            var context = new AsyncLocal<bool>();

            var autoBatcher = new AutoBatcher(() =>
            {
                var callNumber = Interlocked.Increment(ref supplyCallCount) - 1;
                if (callNumber % 2 == 0) throw new Exception($"Test exception on call #{callNumber}");

                return Task.FromResult(new Batch(0, 1, batchSize, Timeout.InfiniteTimeSpan));
            });

            var ids = new List<long>();
            var exceptions = new List<Exception>();
            for (var i = 0; i < batchesCount * batchSize;)
            {
                try
                {
                    ids.Add(await autoBatcher.GetNextIdAsync());
                    i++;
                }
                catch (Exception exception)
                {
                    exceptions.Add(exception);
                    i += batchSize; // skip one single batch
                }
            }

            Assert.AreEqual(batchesCount, supplyCallCount);
            Assert.AreEqual((batchesCount + 1) / 2, exceptions.Count);
            Assert.AreEqual(batchSize * (batchesCount - exceptions.Count), ids.Count);
        }
    }
}
