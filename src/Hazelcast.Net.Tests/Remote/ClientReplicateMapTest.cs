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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.DistributedObjects;
using Hazelcast.Predicates;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Remote
{
    [TestFixture]
    public class ClientReplicateMapTest : SingleMemberClientRemoteTestBase
    {
        private class Subscription<TKey, TValue> : IAsyncDisposable
        {
            private readonly IHReplicatedMap<TKey, TValue> _map;

            public int EntryAddedCount;
            public int EntryUpdatedCount;
            public int EntryRemovedCount;
            public int ClearedCount;

            private Guid _sid;

            public Subscription(IHReplicatedMap<TKey, TValue> map)
            {
                _map = map;
            }

            public async Task SubscribeAsync()
            {
                _sid = await _map.SubscribeAsync(SubscribeEvents);
            }

            public async Task SubscribeAsync(TKey key)
            {
                _sid = await _map.SubscribeAsync(SubscribeEvents, key);
            }

            public async Task SubscribeAsync(IPredicate predicate)
            {
                _sid = await _map.SubscribeAsync(SubscribeEvents, predicate);
            }

            public async Task SubscribeAsync(TKey key, IPredicate predicate)
            {
                _sid = await _map.SubscribeAsync(SubscribeEvents, key, predicate);
            }

            private void SubscribeEvents(ReplicatedMapEventHandlers<TKey, TValue> events)
            {
                // TODO when event states are available, re-implement as ...?
                /*
                await dictionary.SubscribeAsync(events => events
                    .EntryAdded((sender, args, state) => { state.Foo++; }), state);
                */

                events
                    .EntryAdded((sender, args) => Interlocked.Increment(ref EntryAddedCount))
                    .EntryUpdated((sender, args) => Interlocked.Increment(ref EntryUpdatedCount))
                    .EntryRemoved((sender, args) => Interlocked.Increment(ref EntryRemovedCount))
                    .Cleared((sender, args) => Interlocked.Increment(ref ClearedCount));
            }

            public async ValueTask DisposeAsync()
            {
                await _map.UnsubscribeAsync(_sid);
            }

            public ValueTask AssertCountEventually(Func<int> count, int value)
                => AssertEx.SucceedsEventually(() => Assert.That(count(), Is.EqualTo(value)), 5000, 500);

            public async ValueTask AssertNoCountEventually(Func<int> count)
            {
                await Task.Delay(1000);
                Assert.That(count(), Is.Zero);
            }
        }

        private static async Task<Subscription<TKey, TValue>> SubscribeAsync<TKey, TValue>(IHReplicatedMap<TKey, TValue> map)
        {
            var s = new Subscription<TKey, TValue>(map);
            await s.SubscribeAsync();
            return s;
        }

        private static async Task<Subscription<TKey, TValue>> SubscribeAsync<TKey, TValue>(IHReplicatedMap<TKey, TValue> map, TKey key)
        {
            var s = new Subscription<TKey, TValue>(map);
            await s.SubscribeAsync(key);
            return s;
        }

        private static async Task<Subscription<TKey, TValue>> SubscribeAsync<TKey, TValue>(IHReplicatedMap<TKey, TValue> map, IPredicate predicate)
        {
            var s = new Subscription<TKey, TValue>(map);
            await s.SubscribeAsync(predicate);
            return s;
        }

        private static async Task<Subscription<TKey, TValue>> SubscribeAsync<TKey, TValue>(IHReplicatedMap<TKey, TValue> map, TKey key, IPredicate predicate)
        {
            var s = new Subscription<TKey, TValue>(map);
            await s.SubscribeAsync(key, predicate);
            return s;
        }

        [Test]
        public async Task TestAddEntryListener()
        {
            var dictionary = await Client.GetReplicatedMapAsync<int?, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await using var s = await SubscribeAsync(dictionary);

            await dictionary.GetAndSetAsync(1, "value1");
            await s.AssertCountEventually(() => s.EntryAddedCount, 1);
            await dictionary.GetAndSetAsync(1, "value1");
            await s.AssertCountEventually(() => s.EntryUpdatedCount, 1);
            await dictionary.GetAndRemoveAsync(1);
            await s.AssertCountEventually(() => s.EntryRemovedCount, 1);
            await dictionary.ClearAsync();
            await s.AssertCountEventually(() => s.ClearedCount, 1);
        }

        [Test]
        public async Task TestAddEntryListener_key()
        {
            var dictionary = await Client.GetReplicatedMapAsync<int?, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await using var s = await SubscribeAsync(dictionary, 1);

            await dictionary.GetAndSetAsync(1, "value1");
            await s.AssertCountEventually(() => s.EntryAddedCount, 1);
        }

        [Test]
        public async Task TestAddEntryListener_key_other()
        {
            var dictionary = await Client.GetReplicatedMapAsync<int?, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await using var s = await SubscribeAsync(dictionary, 1);
            await dictionary.GetAndSetAsync(2, "value2");

            await s.AssertNoCountEventually(() => s.EntryAddedCount);
        }

        [Test]
        public async Task TestAddEntryListener_predicate()
        {
            var dictionary = await Client.GetReplicatedMapAsync<int?, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await using var s = await SubscribeAsync(dictionary, Predicate.Key().LessThan(5));
            await FillValues(dictionary);
            await s.AssertCountEventually(() => s.EntryAddedCount, 5);
        }

        [Test]
        public async Task TestAddEntryListener_predicate_key()
        {
            var dictionary = await Client.GetReplicatedMapAsync<int?, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await using var s1 = await SubscribeAsync(dictionary, 2, Predicate.Key().LessThan(5));
            await using var s2 = await SubscribeAsync(dictionary, 6, Predicate.Key().LessThan(5));

            await FillValues(dictionary);
            await s1.AssertCountEventually(() => s1.EntryAddedCount, 1);
            await s2.AssertNoCountEventually(() => s2.EntryAddedCount);
        }

        [Test]
        public async Task TestClear()
        {
            var dictionary = await Client.GetReplicatedMapAsync<int?, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await dictionary.GetAndSetAsync(1, "value1");
            await dictionary.GetAndSetAsync(2, "value2");
            await dictionary.ClearAsync();
            Assert.AreEqual(0, await dictionary.CountAsync());
        }

        [Test]
        public async Task TestContainsKey()
        {
            var dictionary = await Client.GetReplicatedMapAsync<int?, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await dictionary.GetAndSetAsync(1, "value1");
            await dictionary.GetAndSetAsync(2, "value2");

            Assert.True(await dictionary.ContainsKeyAsync(1));
            Assert.True(await dictionary.ContainsKeyAsync(2));
            Assert.False(await dictionary.ContainsKeyAsync(3));
        }

        [Test]
        public async Task TestContainsValue()
        {
            var dictionary = await Client.GetReplicatedMapAsync<int?, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await dictionary.GetAndSetAsync(1, "value1");
            await dictionary.GetAndSetAsync(2, "value2");

            Assert.True(await dictionary.ContainsValueAsync("value1"));
            Assert.True(await dictionary.ContainsValueAsync("value2"));
            Assert.False(await dictionary.ContainsValueAsync("value3"));
        }

        [Test]
        public async Task TestGet()
        {
            var dictionary = await Client.GetReplicatedMapAsync<int?, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await dictionary.GetAndSetAsync(1, "value1");
            var value = await dictionary.GetAsync(1);
            Assert.AreEqual(value, "value1");
        }

        [Test]
        public async Task TestIsEmpty()
        {
            var dictionary = await Client.GetReplicatedMapAsync<int?, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            Assert.True(await dictionary.IsEmptyAsync());
        }

        [Test]
        public async Task TestEntrySet()
        {
            var dictionary = await Client.GetReplicatedMapAsync<int?, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await FillValues(dictionary);
            var keyValuePairs = await dictionary.GetEntriesAsync();
            for (var i = 0; i < 10; i++)
            {
                Assert.True(keyValuePairs.Contains(new KeyValuePair<int?, string>(i, "value" + i)));
            }
        }

        [Test]
        public async Task TestKeySet()
        {
            var dictionary = await Client.GetReplicatedMapAsync<int?, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await FillValues(dictionary);
            var keyset = await dictionary.GetKeysAsync();
            for (var i = 0; i < 10; i++)
            {
                Assert.True(keyset.Contains(i));
            }
        }

        [Test]
        public async Task TestPut()
        {
            var dictionary = await Client.GetReplicatedMapAsync<int?, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await dictionary.GetAndSetAsync(1, "value1");
            var value = await dictionary.GetAsync(1);
            Assert.AreEqual(value, "value1");
        }

        [Test]
		public async Task TestPut_null()
		{
            var dictionary = await Client.GetReplicatedMapAsync<int?, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await AssertEx.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await dictionary.GetAndSetAsync(1, null);
            });
		}

        [Test]
        public async Task TestPut_ttl()
        {
            var dictionary = await Client.GetReplicatedMapAsync<int?, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await dictionary.GetAndSetAsync(1, "value1", TimeSpan.FromSeconds(2));
            var value = await dictionary.GetAsync(1);
            Assert.AreEqual(value, "value1");
            await AssertEx.SucceedsEventually(async () =>
            {
                Assert.That((await dictionary.GetAsync(1)).IsNone);
            }, 10000, 500);
        }

        [Test]
        public async Task TestPutAll()
        {
            var dictionary = await Client.GetReplicatedMapAsync<int?, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            var entries = new Dictionary<int?, string>();
            for (var i = 0; i < 10; i++)
            {
                entries.Add(i, "value" + i);
            }
            await dictionary.SetAllAsync(entries);
            for (var i = 0; i < 10; i++)
            {
                Assert.AreEqual("value"+i, await dictionary.GetAsync(i));
            }
            Assert.AreEqual(10, await dictionary.CountAsync());
        }

        [Test]
        public async Task TestRemove()
        {
            var dictionary = await Client.GetReplicatedMapAsync<int?, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await dictionary.GetAndSetAsync(1, "value1");
            var value = await dictionary.GetAndRemoveAsync(1);
            Assert.AreEqual(value, "value1");
            Assert.AreEqual(0, await dictionary.CountAsync());

        }

        [Test]
        public async Task TestRemoveEntryListener()
        {
            var dictionary = await Client.GetReplicatedMapAsync<int?, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            var s = await SubscribeAsync(dictionary);
            await s.DisposeAsync(); // unsubscribes

            // unsubscribed = does not count
            await dictionary.GetAndSetAsync(1, "value1");
            await s.AssertNoCountEventually(() => s.EntryAddedCount);

            var invalidRegistrationId = Guid.NewGuid();
            Assert.IsFalse(await dictionary.UnsubscribeAsync(invalidRegistrationId));
        }

        [Test]
        public async Task TestSize()
        {
            var dictionary = await Client.GetReplicatedMapAsync<int?, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await FillValues(dictionary);
            Assert.AreEqual(10, await dictionary.CountAsync());
        }

        [Test]
        public async Task TestValues()
        {
            var dictionary = await Client.GetReplicatedMapAsync<int?, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await FillValues(dictionary);
            var values = await dictionary.GetValuesAsync();
            for (var i = 0; i < 10; i++)
            {
                Assert.IsTrue(values.Contains("value" + i));
            }
            Assert.AreEqual(10, values.Count);
        }

        private async Task FillValues(IHReplicatedMap<int?, string> map)
        {
            for (var i = 0; i < 10; i++)
            {
                await map.GetAndSetAsync(i, "value" + i);
            }
        }
    }
}