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

using System.Collections.Concurrent;
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
    public class FlakeIdGeneratorTests: SingleMemberClientRemoteTestBase
    {
        private const int BatchSize = 10;

        protected override HazelcastOptions CreateHazelcastOptions()
        {
            var options = base.CreateHazelcastOptions();

            options.FlakeIdGenerators["*"] = new FlakeIdGeneratorOptions
            {
                PrefetchCount = BatchSize,
                PrefetchValidityPeriod = Timeout.InfiniteTimeSpan
            };

            return options;
        }

        [Test]
        public async Task Name()
        {
            var name = CreateUniqueName();
            await using var generator = await Client.GetFlakeIdGeneratorAsync(name);

            Assert.That(generator.Name, Is.EqualTo(name));

            await generator.DestroyAsync();
        }

        [Test]
        public async Task GetNewId()
        {
            await using var generator = await Client.GetFlakeIdGeneratorAsync(CreateUniqueName());

            var ids = new ConcurrentBag<long>();
            await TaskEx.RunConcurrently(async _ =>
            {
                ids.Add(await generator.GetNewIdAsync());
            }, BatchSize * 10);

            var batches = ids.OrderBy(x => x).Batch(BatchSize).ToList();

            IList<long> prevBatch = null;
            foreach (var batch in batches)
            {
                var increment = batch[1] - batch[0];

                CollectionAssert.AreEqual(
                    expected: Enumerable.Range(0, BatchSize).Select(i => batch[0] + i * increment),
                    actual: batch
                );

                prevBatch = batch;
            }

            await generator.DestroyAsync();
        }
    }
}
