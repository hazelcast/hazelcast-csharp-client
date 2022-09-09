// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.DistributedObjects.Impl;
using Hazelcast.Testing;
using Hazelcast.Tests.FlakeId.BatchTests_;
using NUnit.Framework;

namespace Hazelcast.Tests.FlakeId
{
    namespace BatchTests_
    {
        internal static class BatchExtensions
        {
            public static IEnumerable<long> Enumerate(this Batch batch)
            {
                while (batch.TryGetNextId(out var id))
                    yield return id;
            }
        }
    }

    [TestFixture]
    public class BatchTests
    {
        [Test]
        [TestCase(1, 1)]
        [TestCase(2, 10)]
        public void TryGetNextId_Sequence(int increment, int batchSize)
        {
            var batch = new Batch(0, increment, batchSize, Timeout.InfiniteTimeSpan);

            CollectionAssert.AreEqual(
                expected: Enumerable.Range(0, batchSize).Select(i => i * increment),
                actual: batch.Enumerate()
            );
            Assert.IsFalse(batch.TryGetNextId(out _));
        }

        private class TestClockSource: IClockSource
        {
            public DateTime Now { get; set; }
        }

        [Test]
        public void TryGetNextId_ValidityPeriod()
        {
            var timeUnit = TimeSpan.FromMilliseconds(1000);

            var clock = new TestClockSource();
            using var clockOverride = Clock.Override(clock);

            clock.Now = DateTime.UtcNow;

            var batch = new Batch(0, 1, int.MaxValue, timeUnit);
            Assert.IsTrue(batch.TryGetNextId(out _));
            Assert.IsTrue(batch.TryGetNextId(out _));

            clock.Now += timeUnit;
            clock.Now += TimeSpan.FromMilliseconds(1);
            Assert.IsFalse(batch.TryGetNextId(out _));
            Assert.IsFalse(batch.TryGetNextId(out _));

            clock.Now += TimeSpan.FromMilliseconds(1);
            Assert.IsFalse(batch.TryGetNextId(out _));
            Assert.IsFalse(batch.TryGetNextId(out _));
        }

        [Test]
        [TestCase(5)]
        [TestCase(100)]
        public async Task TryGetNextId_Concurrent(int batchSize)
        {
            Batch BatchFactory() => new Batch(0, 1, batchSize, Timeout.InfiniteTimeSpan);

            var batch = BatchFactory();
            var ids = await TaskEx.Parallel(
                _ => batch.TryGetNextId(out var id) ? id : (long?)null,
                batchSize
            );

            CollectionAssert.AreEquivalent(
                expected: BatchFactory().Enumerate(),
                actual: ids
            );
        }
    }
}
