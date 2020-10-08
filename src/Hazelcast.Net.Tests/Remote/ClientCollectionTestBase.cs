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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.DistributedObjects;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Remote
{
    public abstract class ClientCollectionTestBase : SingleMemberClientRemoteTestBase
    {
        protected abstract Task<IHCollection<string>> GetHCollectionAsync(string baseName = default, bool isUnique = true);

        public static async Task<IList<string>> FillCollection(IHCollection<string> collection, int count)
        {
            var items = new List<string>();
            for (var i = 0; i < count; i++)
            {
                var itemValue = "item" + i;
                items.Add(itemValue);
                await collection.AddAsync(itemValue);
            }

            return items;
        }

        [Test]
        public async Task TestCollectionAdd()
        {
            var collection = await GetHCollectionAsync();
            await using var _ = DestroyAndDispose(collection);

            await FillCollection(collection, 5);
            Assert.AreEqual(5, await collection.CountAsync());
        }

        [Test]
        public async Task TestCollectionAddRange()
        {
            var collection = await GetHCollectionAsync();
            await using var _ = DestroyAndDispose(collection);

            var coll = new List<string> {"item1", "item2", "item3", "item4"};
            Assert.IsTrue(await collection.AddRangeAsync(coll));
            Assert.That(await collection.CountAsync(), Is.EqualTo(coll.Count));
        }

        [Test]
        public async Task TestCollectionGetAll()
        {
            var collection = await GetHCollectionAsync();
            await using var _ = DestroyAndDispose(collection);

            const int testCount = 5;
            await FillCollection(collection, testCount);

            var actualItems = new HashSet<string>();
            for (var i = 0; i < testCount; i++)
            {
                actualItems.Add("item" + i);
            }

            var allItems = await collection.GetAllAsync();
            for (var i = 0; i < testCount; i++)
            {
                Assert.True(actualItems.Contains(allItems[i]));
            }

            Assert.That(await collection.CountAsync(), Is.EqualTo(testCount));
        }

        [Test]
        public async Task TestCollectionCount()
        {
            var collection = await GetHCollectionAsync();
            await using var _ = DestroyAndDispose(collection);

            await FillCollection(collection, 1);
            Assert.AreEqual(1, await collection.CountAsync());
        }

        [Test]
        public async Task TestCollectionIsEmpty()
        {
            var collection = await GetHCollectionAsync();
            await using var _ = DestroyAndDispose(collection);

            Assert.True(await collection.IsEmptyAsync());
        }

        [Test]
        public async Task TestCollectionContainsAll()
        {
            var collection = await GetHCollectionAsync();
            await using var _ = DestroyAndDispose(collection);

            await FillCollection(collection, 5);
            var list = new List<string>(2) {"item4", "item2"};
            Assert.That(await collection.ContainsAllAsync(list));
            list.Add("item");
            Assert.That(await collection.ContainsAllAsync(list), Is.False);
        }

        [Test]
        public async Task TestCollectionContains()
        {
            var collection = await GetHCollectionAsync();
            await using var _ = DestroyAndDispose(collection);

            await FillCollection(collection, 5);
            Assert.That(await collection.ContainsAsync("item3"));
        }

        [Test]
        public async Task TestCollectionRemove()
        {
            var collection = await GetHCollectionAsync();
            await using var _ = DestroyAndDispose(collection);

            await FillCollection(collection, 5);

            Assert.That(await collection.RemoveAsync("item2"));
            Assert.That(await collection.RemoveAsync("itemX"), Is.False);
        }

        [Test]
        public async Task TestCollectionRemoveAll()
        {
            var collection = await GetHCollectionAsync();
            await using var _ = DestroyAndDispose(collection);

            await FillCollection(collection, 5);
            var list = new List<string>(2) {"item4", "item2"};
            Assert.True(await collection.RemoveAllAsync(list));
            Assert.False(await collection.RemoveAllAsync(list));
            Assert.True(await collection.ContainsAllAsync(new[] {"item0", "item1", "item3"}));
        }

        [Test]
        public async Task TestCollectionRetainAll()
        {
            var collection = await GetHCollectionAsync();
            await using var _ = DestroyAndDispose(collection);

            await FillCollection(collection, 5);
            var list = new List<string>(2) {"item4", "item2"};
            Assert.True(await collection.RetainAllAsync(list));
            Assert.False(await collection.RetainAllAsync(list));
            Assert.True(await collection.ContainsAllAsync(new[] {"item2", "item4"}));
        }

        [Test]
        public async Task TestCollectionClear()
        {
            var collection = await GetHCollectionAsync();
            await using var _ = DestroyAndDispose(collection);

            await FillCollection(collection, 5);
            await collection.ClearAsync();
            Assert.True(await collection.IsEmptyAsync());
        }

        [Test]
        public async Task TestCollectionSubscribe()
        {
            var collection = await GetHCollectionAsync();
            await using var _ = DestroyAndDispose(collection);

            const int testCount = 5;
            var eventsCount = 0;
            await collection.SubscribeAsync(events => events
                .ItemAdded((sender, args) => { Interlocked.Increment(ref eventsCount); })
            );

            await Task.Run(async () =>
            {
                for (var i = 0; i < testCount; i++)
                {
                    await collection.AddAsync("item" + i);
                }
            });

            await AssertEx.SucceedsEventually(() => Assert.AreEqual(eventsCount, testCount),
                4000, 500);
        }

        [Test]
        public async Task TestCollectionSubscribeMany()
        {
            var collection = await GetHCollectionAsync("AnotherQueue");
            await using var _ = DestroyAndDispose(collection);

            const int testItemCount = 40;

            await FillCollection(collection, testItemCount);

            var eventsCount = 0;
            var sids = new List<Guid>();

            for (var i = 0; i < testItemCount; i++)
            {
                var sid = await collection.SubscribeAsync(events => events
                    .ItemRemoved((sender, args) => { Interlocked.Increment(ref eventsCount); }));
                sids.Add(sid);
            }

            await collection.ClearAsync();

            await AssertEx.SucceedsEventually(() =>
                    Assert.AreEqual(eventsCount, testItemCount * testItemCount),
                66000, 100); // can take a *lot* of time

            // foreach (var sid in sids)
            //     await collection.UnsubscribeAsync(sid);
        }

        [Test]
        public async Task TestCollectionUnsubscribe()
        {
            var collection = await GetHCollectionAsync();
            await using var _ = DestroyAndDispose(collection);

            var eventsCount = 0;
            var sid = await collection.SubscribeAsync(events => events
                .ItemAdded((sender, args) => { Interlocked.Increment(ref eventsCount); }));

            await collection.UnsubscribeAsync(sid);
            await collection.AddAsync("item");
            await Task.Delay(4_000);
            Assert.That(eventsCount, Is.Zero);
        }

        [Test]
        [Ignore("DisposeAsync should unsubscribe events")]
        public async Task TestCollectionUnsubscribeWitDispose()
        {
            var collection = await GetHCollectionAsync();
            var collection2 = await GetHCollectionAsync(collection.Name, false);

            var eventsCount = 0;
            var sid = await collection.SubscribeAsync(events => events
                .ItemAdded((sender, args) => { Interlocked.Increment(ref eventsCount); }));

            await using (collection)
            {
                Assert.AreEqual(0, await collection.CountAsync());
            }

            await collection2.AddAsync("item");

            await Task.Delay(4_000);
            Assert.That(eventsCount, Is.Zero);
        }

        [Test]
        public async Task TestCollectionGetAsyncEnumerator()
        {
            var collection = await GetHCollectionAsync();
            await using var _ = DestroyAndDispose(collection);

            var items = await FillCollection(collection, 5);
            var asyncEnumerator = collection.GetAsyncEnumerator();
            while (await asyncEnumerator.MoveNextAsync())
            {
                var itemValue = asyncEnumerator.Current;
                Assert.True(items.Remove(itemValue));
            }

            //check if all items match and no items left
            Assert.AreEqual(items.Count, 0);
        }

        private static void HandleItemAdded(IHCollection<string> sender, CollectionItemEventArgs<string> args)
        {
            var state = (EventState)args.State;
            Interlocked.Increment(ref state.EventsCount);
        }

        [Test]
        public async Task TestCollectionSubscribeWithState()
        {
            var list = await GetHCollectionAsync();
            await using var _ = DestroyAndDispose(list);

            var eventState = new EventState();
            var sid = await list.SubscribeAsync(handle => handle
                    .ItemAdded(HandleItemAdded), // thanks to state, can be anything without capture
                state: eventState);

            for (var i = 0; i < 5; i++)
                await list.AddAsync("item" + i);
            await list.AddAsync("done");

            await AssertEx.SucceedsEventually(() => Assert.That(eventState.EventsCount, Is.EqualTo(6)), 4000, 500);

            await list.UnsubscribeAsync(sid);
        }

        private class EventState
        {
            public int EventsCount;
        }
    }
}