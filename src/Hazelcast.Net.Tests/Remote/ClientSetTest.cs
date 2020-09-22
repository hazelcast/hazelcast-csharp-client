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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Remote
{
    [TestFixture]
    public class ClientSetTest : SingleMemberClientRemoteTestBase
    {
        private const string SetNameBase = "Set";

        [Test]
        public async Task RemoveRetainAll()
        {
            var set = await Client.GetSetAsync<string>(SetNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(set);

            Assert.IsTrue(await set.AddAsync("item1"));
            Assert.IsTrue(await set.AddAsync("item2"));
            Assert.IsTrue(await set.AddAsync("item3"));
            Assert.IsTrue(await set.AddAsync("item4"));
            var l = new List<string> { "item4", "item3" };
            Assert.IsTrue(await set.RemoveAllAsync(l));
            Assert.That(await set.CountAsync(), Is.EqualTo(2));
            Assert.IsFalse(await set.RemoveAllAsync(l));
            Assert.That(await set.CountAsync(), Is.EqualTo(2));
            l.Clear();
            l.Add("item1");
            l.Add("item2");
            Assert.IsFalse(await set.RetainAllAsync(l));
            Assert.That(await set.CountAsync(), Is.EqualTo(2));
            l.Clear();
            Assert.IsTrue(await set.RetainAllAsync(l));
            Assert.That(await set.CountAsync(), Is.EqualTo(0));
        }

        [Test]
        public async Task TestAddAll()
        {
            var set = await Client.GetSetAsync<string>(SetNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(set);

            var l = new List<string> { "item1", "item2" };
            Assert.IsTrue(await set.AddRangeAsync(l));
            Assert.That(await set.CountAsync(), Is.EqualTo(2));
            Assert.IsFalse(await set.AddRangeAsync(l));
            Assert.That(await set.CountAsync(), Is.EqualTo(2));
        }

        [Test]
        public async Task TestAddRemove()
        {
            var set = await Client.GetSetAsync<string>(SetNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(set);

            Assert.IsTrue(await set.AddAsync("item1"));
            Assert.IsTrue(await set.AddAsync("item2"));
            Assert.IsTrue(await set.AddAsync("item3"));
            Assert.That(await set.CountAsync(), Is.EqualTo(3));
            Assert.IsFalse(await set.AddAsync("item3"));
            Assert.That(await set.CountAsync(), Is.EqualTo(3));
            Assert.IsFalse(await set.RemoveAsync("item4"));
            Assert.IsTrue(await set.RemoveAsync("item3"));
        }

        [Test]
        public async Task TestContains()
        {
            var set = await Client.GetSetAsync<string>(SetNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(set);

            Assert.IsTrue(await set.AddAsync("item1"));
            Assert.IsTrue(await set.AddAsync("item2"));
            Assert.IsTrue(await set.AddAsync("item3"));
            Assert.IsTrue(await set.AddAsync("item4"));
            Assert.IsFalse(await set.ContainsAsync("item5"));
            Assert.IsTrue(await set.ContainsAsync("item2"));
            var l = new List<string> { "item6", "item3" };
            Assert.IsFalse(await set.ContainsAllAsync(l));
            Assert.IsTrue(await set.AddAsync("item6"));
            Assert.IsTrue(await set.ContainsAllAsync(l));
        }

        [Test]
        public async Task TestIsEmpty()
        {
            var set = await Client.GetSetAsync<string>(SetNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(set);

            Assert.IsTrue(await set.IsEmptyAsync());
            await set.AddAsync("item1");
            Assert.IsFalse(await set.IsEmptyAsync());
            await set.ClearAsync();
            Assert.IsTrue(await set.IsEmptyAsync());
        }

        [Test]
        public async Task TestIterator()
        {
            var set = await Client.GetSetAsync<string>(SetNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(set);

            Assert.IsTrue(await set.AddAsync("item1"));
            Assert.IsTrue(await set.AddAsync("item2"));
            Assert.IsTrue(await set.AddAsync("item3"));
            Assert.IsTrue(await set.AddAsync("item4"));

            var iter = set.GetAsyncEnumerator();

            await iter.MoveNextAsync();
            Assert.IsTrue(iter.Current.StartsWith("item"));
            await iter.MoveNextAsync();
            Assert.IsTrue(iter.Current.StartsWith("item"));
            await iter.MoveNextAsync();
            Assert.IsTrue(iter.Current.StartsWith("item"));
            await iter.MoveNextAsync();
            Assert.IsTrue(iter.Current.StartsWith("item"));
            Assert.IsFalse(await iter.MoveNextAsync());
        }
        
        [Test]
        public async Task TestGetAll()
        {
            var set = await Client.GetSetAsync<string>(SetNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(set);
            var actualItems = new HashSet<string>();
            for (var i = 0; i < 10; i++)
            {
                actualItems.Add("item-" + i);
            }

            foreach (var item in actualItems)
            {
                await set.AddAsync(item);
            }

            var allItems = await set.GetAllAsync();
            for (var i = 0; i < 10; i++)
            {
                Assert.True(actualItems.Contains(allItems[i]));
            }
        }


        [Test]
        public async Task TestListener()
        {
            var set = await Client.GetSetAsync<string>(SetNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(set);

            var eventsCount = 0;
            var sid = await set.SubscribeAsync(handle => handle
                .ItemAdded((sender, args) =>
                {
                    Interlocked.Increment(ref eventsCount);
                }));

            for (var i = 0; i < 5; i++)
                await set.AddAsync("item" + i);
            await set.AddAsync("done");

            await AssertEx.SucceedsEventually(() => Assert.That(eventsCount, Is.EqualTo(6)), 4000, 500);

            await set.UnsubscribeAsync(sid);
        }

        [Test]
        public async Task TestRemoveListener()
        {
            var set = await Client.GetSetAsync<string>(SetNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(set);

            var eventsCount = 0;
            var sid = await set.SubscribeAsync(handle => handle
                .ItemAdded((sender, args) =>
                {
                    Interlocked.Increment(ref eventsCount);
                }));

            await set.UnsubscribeAsync(sid);

            await set.AddAsync("item");

            await Task.Delay(4_000);
            Assert.That(eventsCount, Is.Zero);
        }
    }
}