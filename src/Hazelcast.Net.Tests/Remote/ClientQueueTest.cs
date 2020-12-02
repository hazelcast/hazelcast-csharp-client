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
using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.DistributedObjects;
using NUnit.Framework;

namespace Hazelcast.Tests.Remote
{
    [TestFixture]
    public class ClientQueueTest : ClientCollectionTestBase
    {
        // important to stick with this name as it is configured in hazelcast.xml
        // with a corresponding queue max size of 6 items
        private const string QueueNameBase = "ClientQueueTest";

        protected override async Task<IHCollection<string>> GetHCollectionAsync(string baseName = default, bool isUnique = true)
        {
            var name = baseName ?? QueueNameBase;
            if (isUnique) name += "_" + CreateUniqueName();

            return await Client.GetQueueAsync<string>(name);
        }

        [Test]
        public async Task TestEnqueueAsync()
        {
            var queue = await Client.GetQueueAsync<string>(QueueNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(queue);

            await queue.EnqueueAsync("item0");

            Assert.That(await queue.CountAsync(), Is.EqualTo(1));
        }

        [Test]
        public async Task TestTryEnqueueAsync_waitZero()
        {
            var queue = await Client.GetQueueAsync<string>(QueueNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(queue);

            //configured max capacity in server config.
            const int maxCapacity = 6;
            await FillCollection(queue, maxCapacity-1);

            Assert.True(await queue.TryEnqueueAsync("item6"));
            Assert.False(await queue.TryEnqueueAsync("item6"));
        }

        [Test]
        public async Task TestTryEnqueueAsync_waitNonZero()
        {
            var queue = await Client.GetQueueAsync<string>(QueueNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(queue);

            //configured max capacity in server config.
            const int maxCapacity = 6;
            await FillCollection(queue, maxCapacity);

            Assert.False(await queue.TryEnqueueAsync("item", TimeSpan.FromSeconds(2)));
        }

        [Test]
        public async Task TestDequeueAsync()
        {
            var queue = await Client.GetQueueAsync<string>(QueueNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(queue);

            var dequeueTask = queue.DequeueAsync();
            await queue.TryEnqueueAsync("item0");
            Assert.That(await dequeueTask, Is.EqualTo("item0"));
        }

        [Test]
        public async Task TestTryDequeueAsync_waitZero()
        {
            var queue = await Client.GetQueueAsync<string>(QueueNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(queue);

            await queue.TryEnqueueAsync("item0");
            Assert.That(await queue.TryDequeueAsync(), Is.EqualTo("item0"));
        }

        [Test]
        public async Task TestTryDequeueAsync_waitNonZero()
        {
            var queue = await Client.GetQueueAsync<string>(QueueNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(queue);

            await queue.TryEnqueueAsync("item0");
            Assert.That(await queue.TryDequeueAsync(TimeSpan.FromSeconds(2)), Is.EqualTo("item0"));
            Assert.IsNull(await queue.TryDequeueAsync(TimeSpan.FromSeconds(2)));
        }

        [Test]
        public async Task TestDrainToAsync()
        {
            var queue = await Client.GetQueueAsync<string>(QueueNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(queue);

            const int count = 5;
            await FillCollection(queue, count);

            var list = new List<string>();
            var drainToAsync = await queue.DrainToAsync(list);
            Assert.That(drainToAsync, Is.EqualTo(count));
            for (var i = 0; i < count; i++)
            {
                Assert.That(list[i], Is.EqualTo("item" + i));
            }
        }

        [Test]
        public async Task TestDrainToAsync_witMaxSize()
        {
            var queue = await Client.GetQueueAsync<string>(QueueNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(queue);

            const int count = 5;
            await FillCollection(queue, count);

            var list = new List<string>();
            Assert.That(await queue.DrainToAsync(list, 2), Is.EqualTo(2));
            for (var i = 0; i < 2; i++)
            {
                Assert.That(list[i], Is.EqualTo("item" + i));
            }
        }

        [Test]
        public async Task TestTryPeekAsync()
        {
            var queue = await Client.GetQueueAsync<string>(QueueNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(queue);

            Assert.IsNull(await queue.TryPeekAsync());
        }

        [Test]
        public async Task TestGetRemainingCapacityAsync()
        {
            var queue = await Client.GetQueueAsync<string>(QueueNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(queue);

            Assert.That(await queue.GetRemainingCapacityAsync(), Is.EqualTo(6));
            Assert.That(await queue.TryEnqueueAsync("item_0"));
            Assert.That(await queue.GetRemainingCapacityAsync(), Is.EqualTo(5));
        }
    }
}
