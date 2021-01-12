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
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Hazelcast.Exceptions;
using Hazelcast.Query;
using Hazelcast.Serialization;
using Hazelcast.Testing;
using Hazelcast.Tests.TestObjects;
using NUnit.Framework;

namespace Hazelcast.Tests.Remote
{
    [TestFixture]
    public class ClientMapTest : SingleMemberClientRemoteTestBase
    {
        protected override HazelcastOptions CreateHazelcastOptions()
        {
            var options = base.CreateHazelcastOptions();
            options.Serialization.AddPortableFactory(1, new PortableFactory());
            options.Serialization.AddDataSerializableFactory(IdentifiedFactory.FactoryId, new IdentifiedFactory());
            return options;
        }

        private static async Task FillAsync(IHMap<string, string> dictionary)
        {
            for (var i = 0; i < 10; i++)
            {
                await dictionary.SetAsync("key" + i, "value" + i);
            }
        }

        [Serializable]
        internal class Deal
        {
            internal int id;

            internal Deal(int id)
            {
                this.id = id;
            }

            public virtual int GetId()
            {
                return id;
            }

            public Task SetId(int id)
            {
                this.id = id;
                return Task.CompletedTask;
            }
        }

        private class Interceptor : IMapInterceptor, IIdentifiedDataSerializable
        {
            public void WriteData(IObjectDataOutput output)
            {
            }

            public int FactoryId => 1;

            public int ClassId => 0;

            public void ReadData(IObjectDataInput input)
            {
            }

            public object InterceptGet(object value)
            {
                throw new NotImplementedException();
            }

            public void AfterGet(object value)
            {
                throw new NotImplementedException();
            }

            public object InterceptPut(object oldValue, object newValue)
            {
                throw new NotImplementedException();
            }

            public void AfterPut(object value)
            {
                throw new NotImplementedException();
            }

            public object InterceptRemove(object removedValue)
            {
                throw new NotImplementedException();
            }

            public void AfterRemove(object value)
            {
                throw new NotImplementedException();
            }
        }

        //TODO FIX Index tests
        //        [Test]
        //        public async Task TestAddIndex()
        //        {
        //            dictionary.AddIndex(IndexType.Sorted, "name");
        //        }

        [Ignore("not currently possible to test this")]
        [Test]
        public async Task TestAddInterceptor()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);
            Console.WriteLine(dictionary.GetSizeAsync());
            Assert.That(dictionary.GetSizeAsync(), Iz.EqualTo(2));
            await AssertEx.ThrowsAsync<HazelcastException>(async () =>
            {
                //TODO: not currently possible to test this

                var id = await dictionary.AddInterceptorAsync(new Interceptor());
            });
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public async Task TestAsyncGet()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await FillAsync(dictionary);

            var value = await dictionary.GetAsync("key1");
            Assert.That(value, Is.EqualTo("value1"));
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public async Task TestAsyncPut()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await FillAsync(dictionary);
            var value = await dictionary.PutAsync("key3", "value");

            Assert.That(value, Is.EqualTo("value3"));
            Assert.That(await dictionary.GetAsync("key3"), Is.EqualTo("value"));
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public async Task TestAsyncPutWithTtl()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            var eventsCount = 0;
            var sid = await dictionary.SubscribeAsync(events => events
                .EntryExpired((sender, args) =>
                {
                    Interlocked.Increment(ref eventsCount);
                }));

            await dictionary.SetAsync("key", "value1", TimeSpan.FromSeconds(3));
            Assert.That(await dictionary.GetAsync("key"), Is.EqualTo("value1"));

            await AssertEx.SucceedsEventually(async () =>
            {
                Assert.That(eventsCount, Is.EqualTo(1));
                Assert.That(await dictionary.GetAsync("key"), Is.Null);
            }, 15000, 500);

            await dictionary.UnsubscribeAsync(sid);
        }

        [Test]
        public async Task TestAsyncRemove()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await FillAsync(dictionary);
            var value = await dictionary.RemoveAsync("key4");

            Assert.AreEqual("value4", value);
            Assert.AreEqual(9, await dictionary.GetSizeAsync());
        }

        [Test]
        public async Task TestContains()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await FillAsync(dictionary);

            Assert.IsFalse(await dictionary.ContainsKeyAsync("key10"));
            Assert.IsTrue(await dictionary.ContainsKeyAsync("key1"));
            Assert.IsFalse(await dictionary.ContainsValueAsync("value10"));
            Assert.IsTrue(await dictionary.ContainsValueAsync("value1"));
        }

        [Test]
        public async Task TestEntrySet()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await dictionary.SetAsync("key1", "value1");
            await dictionary.SetAsync("key2", "value2");
            await dictionary.SetAsync("key3", "value3");

            var tempDict = await dictionary.GetEntriesAsync();

            Assert.True(tempDict.ContainsKey("key1"));
            Assert.True(tempDict.ContainsKey("key2"));
            Assert.True(tempDict.ContainsKey("key3"));

            tempDict.TryGetValue("key1", out var value);
            Assert.AreEqual("value1", value);

            tempDict.TryGetValue("key2", out value);
            Assert.AreEqual("value2", value);

            tempDict.TryGetValue("key3", out value);
            Assert.AreEqual("value3", value);
        }

        [Test]
        public async Task TestEntrySetPredicate()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await dictionary.SetAsync("key1", "value1");
            await dictionary.SetAsync("key2", "value2");
            await dictionary.SetAsync("key3", "value3");

            var tempDict = await dictionary.GetEntriesAsync(new SqlPredicate("this == value1"));
            Assert.AreEqual(1, tempDict.Count);

            Assert.That(tempDict.TryGetValue("key1", out var value));
            Assert.That(value, Is.EqualTo("value1"));
        }

        [Test]
        public async Task TestEntryStats()
        {
            var dictionary = await Client.GetMapAsync<string, Item>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            var item = ItemGenerator.GenerateItem(1);
            await dictionary.SetAsync("key1", item);
            await dictionary.GetAsync("key1");
            await dictionary.GetAsync("key1");

            var entryStats = await dictionary.GetEntryViewAsync("key1");
            var value = entryStats.Value as Item;

            Assert.AreEqual("key1", entryStats.Key);
            Assert.True(item.Equals(value));
            //Assert.AreEqual(2, entryview.GetHits());
            //Assert.True(entryview.GetCreationTime() > 0);
            //Assert.True(entryview.GetLastAccessTime() > 0);
            //Assert.True(entryview.GetLastUpdateTime() > 0);
        }

        [Test]
        public async Task TestEvict()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await dictionary.SetAsync("key1", "value1");
            Assert.That(await dictionary.GetAsync("key1"), Is.EqualTo("value1"));

            await dictionary.EvictAsync("key1");

            Assert.AreEqual(0, await dictionary.GetSizeAsync());
            Assert.That(await dictionary.GetAsync("key1"), Is.Null);
        }

        [Test]
        public async Task TestEvictAll()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await dictionary.SetAsync("key1", "value1");
            await dictionary.SetAsync("key2", "value2");
            await dictionary.SetAsync("key3", "value3");

            Assert.AreEqual(3, await dictionary.GetSizeAsync());

            await dictionary.LockAsync("key3");
            await dictionary.EvictAllAsync();

            Assert.AreEqual(1, await dictionary.GetSizeAsync());
            Assert.That(await dictionary.GetAsync("key3"), Is.EqualTo("value3"));
        }

        [Test]
        public async Task TestFlush()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await dictionary.FlushAsync();
        }

        [Test]
        public async Task TestExecuteOnKey()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await FillAsync(dictionary);
            const string key = "key1";
            const string value = "value10";
            var entryProcessor = new IdentifiedEntryProcessor(value);
            var result = await dictionary.ExecuteAsync(entryProcessor, key);
            Assert.AreEqual(result, value);
            Assert.That(await dictionary.GetAsync(key), Is.EqualTo(result));
        }

        [Test]
        public async Task TestExecuteOnKey_nullKey()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await FillAsync(dictionary);

            const string key = null;
            const string value = "value10";
            var entryProcessor = new IdentifiedEntryProcessor(value);

            await AssertEx.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await dictionary.ExecuteAsync(entryProcessor, key);
            });
        }

        [Test]
        public async Task TestExecuteOnKeys()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await FillAsync(dictionary);

            var keys = new HashSet<string> {"key1", "key5"};
            const string value = "valueX";
            var entryProcessor = new IdentifiedEntryProcessor(value);
            var result = await dictionary.ExecuteAsync(entryProcessor, keys);
            foreach (var resultKV in result)
            {
                Assert.AreEqual(resultKV.Value, value);
                Assert.That(await dictionary.GetAsync(resultKV.Key), Is.EqualTo(value));
            }
        }

        [Test]
        public async Task TestExecuteOnKeys_keysNotNull()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await FillAsync(dictionary);

            ISet<string> keys = null;
            const string value = "valueX";
            var entryProcessor = new IdentifiedEntryProcessor(value);

            await AssertEx.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await dictionary.ExecuteAsync(entryProcessor, keys);
            });
        }

        [Test]
        public async Task TestExecuteOnKeys_keysEmpty()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await FillAsync(dictionary);

            ISet<string> keys = new HashSet<string>();
            const string value = "valueX";
            var entryProcessor = new IdentifiedEntryProcessor(value);
            var result = await dictionary.ExecuteAsync(entryProcessor, keys);
            Assert.AreEqual(result.Count, 0);
        }

        [Test]
        public async Task TestExecuteOnEntries()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await FillAsync(dictionary);

            const string value = "valueX";
            var entryProcessor = new IdentifiedEntryProcessor(value);
            var result = await dictionary.ExecuteAsync(entryProcessor);
            foreach (var resultKV in result)
            {
                Assert.AreEqual(resultKV.Value, value);
                Assert.That(await dictionary.GetAsync(resultKV.Key), Is.EqualTo(value));
            }
        }

        [Test]
        public async Task TestExecuteOnEntriesWithPredicate()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await FillAsync(dictionary);

            const string value = "valueX";
            var entryProcessor = new IdentifiedEntryProcessor(value);
            var result = await dictionary.ExecuteAsync(entryProcessor, Predicates.Sql("this == value5"));
            Assert.AreEqual(result.Count, 1);
            foreach (var resultKV in result)
            {
                Assert.AreEqual(resultKV.Value, value);
                Assert.That(await dictionary.GetAsync(resultKV.Key), Is.EqualTo(value));
            }
        }

        [Test]
        public async Task TestSubmitToKey()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await FillAsync(dictionary);

            const string key = "key1";
            const string value = "value10";
            var entryProcessor = new IdentifiedEntryProcessor(value);
            var result = await dictionary.ExecuteAsync(entryProcessor, key);

            Assert.That(result, Is.EqualTo(value));
            Assert.That(await dictionary.GetAsync(key), Is.EqualTo(value));
        }

        [Test]
        public async Task TestSubmitToKey_nullKey()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            const string key = null;
            const string value = "value10";
            var entryProcessor = new IdentifiedEntryProcessor(value);

            await AssertEx.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await dictionary.ExecuteAsync(entryProcessor, key);
            });
        }

        [Test]
        public async Task TestForceUnlock()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await dictionary.SetAsync("key1", "value1");

            // this context locks the key
            await dictionary.LockAsync("key1");

            // force-unlock in another context
            await AsyncContext.RunWithNew(async () =>
            {
                await dictionary.ForceUnlockAsync("key1");
            });

            // lock has been released here too
            Assert.IsFalse(await dictionary.IsLockedAsync("key1"));
        }

        [Test]
        public async Task TestGet()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await FillAsync(dictionary);

            for (var i = 0; i < 10; i++)
            {
                var o = await dictionary.GetAsync("key" + i);
                Assert.AreEqual("value" + i, o);
            }
        }

        [Test] //, Repeat(100)]
        public async Task TestGetAllExtreme()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            Assert.AreEqual(0, await dictionary.GetSizeAsync());

            IDictionary<string, string> mm = new Dictionary<string, string>();
            const int keycount = 1000;

            //insert dummy keys and values
            foreach (var itemIndex in Enumerable.Range(0, keycount))
            {
                mm.Add(itemIndex.ToString(), itemIndex.ToString());
            }

            await dictionary.SetAllAsync(mm);
            Assert.AreEqual(keycount, await dictionary.GetSizeAsync());

            var all = await dictionary.GetAllAsync(mm.Keys);
            // Assert.AreEqual(keycount, dictionary.Count);
            // foreach (var pair in dictionary)
            // {
            //     Assert.AreEqual(mm[pair.Key] , pair.Value);
            // }
            //
            foreach (var pair in mm)
            {
                if (all.TryGetValue(pair.Key, out var val))
                {
                    Assert.AreEqual(val, pair.Value);
                }
                else
                {
                    Assert.Fail($"{pair.Key} is missing in the result");
                }
            }
        }

        [Test]
        public async Task TestGetAllPutAll()
        {
            var dictionary = await Client.GetMapAsync<int, int>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            IDictionary<int, int> mm = new Dictionary<int, int>();
            for (var i = 0; i < 100; i++)
            {
                mm.Add(i, i);
            }
            await dictionary.SetAllAsync(mm);
            Assert.AreEqual(await dictionary.GetSizeAsync(), 100);
            for (var i_1 = 0; i_1 < 100; i_1++)
            {
                Assert.AreEqual(await dictionary.GetAsync(i_1), i_1);
            }
            var ss = new HashSet<int> {1, 3};

            var m2 = await dictionary.GetAllAsync(ss);
            Assert.AreEqual(m2.Count, 2);

            int gv;
            m2.TryGetValue(1, out gv);
            Assert.AreEqual(gv, 1);

            m2.TryGetValue(3, out gv);
            Assert.AreEqual(gv, 3);
        }

        [Test]
        public async Task TestGetEntryStats()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await dictionary.SetAsync("item0", "value0");
            await dictionary.SetAsync("item1", "value1");
            await dictionary.SetAsync("item2", "value2");

            var entryStats = await dictionary.GetEntryViewAsync("item1");

            Assert.AreEqual(0, entryStats.Hits);

            Assert.AreEqual("item1", entryStats.Key);
            Assert.AreEqual("value1", entryStats.Value);
        }

        [Test]
        public async Task TestIsEmpty()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            Assert.IsTrue(await dictionary.IsEmptyAsync());
            await dictionary.SetAsync("key1", "value1");
            Assert.IsFalse(await dictionary.IsEmptyAsync());
        }

        [Test]
        public async Task TestKeySet()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await dictionary.SetAsync("key1", "value1");

            var keySet = await dictionary.GetKeysAsync();

            var enumerator = keySet.GetEnumerator();

            enumerator.MoveNext();
            Assert.AreEqual("key1", enumerator.Current);
        }

        [Test]
        public async Task TestKeySetPredicate()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await FillAsync(dictionary);

            var values = await dictionary.GetKeysAsync(new SqlPredicate("this == value1"));
            Assert.AreEqual(1, values.Count);
            var enumerator = values.GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual("key1", enumerator.Current);
        }

        [Test]
        public async Task TestListener()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            var added1Count = 0;
            var removed1Count = 0;
            var added2Count = 0;
            var removed2Count = 0;

            var sid1 = await dictionary.SubscribeAsync(events => events
                .EntryAdded((sender, args) => Interlocked.Increment(ref added1Count))
                .EntryRemoved((sender, args) => Interlocked.Increment(ref removed1Count)),
                false);

            var sid2 = await dictionary.SubscribeAsync(events => events
                .EntryAdded((sender, args) => Interlocked.Increment(ref added2Count))
                .EntryRemoved((sender, args) => Interlocked.Increment(ref removed2Count)),
                "key3",
                true);

            await dictionary.SetAsync("key1", "value1");
            await dictionary.SetAsync("key2", "value2");
            await dictionary.SetAsync("key3", "value3");
            await dictionary.SetAsync("key4", "value4");
            await dictionary.SetAsync("key5", "value5");

            await dictionary.DeleteAsync("key1");
            await dictionary.DeleteAsync("key3");

            await AssertEx.SucceedsEventually(() =>
            {
                Assert.That(added1Count, Is.EqualTo(5));
                Assert.That(removed1Count, Is.EqualTo(2));
                Assert.That(added2Count, Is.EqualTo(1));
                Assert.That(removed2Count, Is.EqualTo(1));
            }, 4000, 500);

            Assert.IsTrue(await dictionary.UnsubscribeAsync(sid1));
            Assert.IsTrue(await dictionary.UnsubscribeAsync(sid2));
        }

        [Test]
        public async Task TestListener_SingleEventListeners()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            var entryAdded = 0;
            var entryUpdated = 0;
            var entryRemoved = 0;
            var entryEvicted = 0;
            var cleared = 0;
            var evicted = 0;
            var entryExpired = 0;

            var sid = await dictionary.SubscribeAsync(events => events
                .EntryAdded((sender, args) => Interlocked.Increment(ref entryAdded))
                .EntryUpdated((sender, args) => Interlocked.Increment(ref entryUpdated))
                .EntryRemoved((sender, args) => Interlocked.Increment(ref entryRemoved))
                .Cleared((sender, args) => Interlocked.Increment(ref cleared))
                .Evicted((sender, args) => Interlocked.Increment(ref evicted))
                .EntryEvicted((sender, args) => Interlocked.Increment(ref entryEvicted))
                .EntryExpired((sender, args) => Interlocked.Increment(ref entryExpired)),
                false
            );

            const int delay = 10000; // ms

            await dictionary.SetAsync("key1", "value1");
            await AssertEx.SucceedsEventually(() => Assert.That(entryAdded, Is.EqualTo(1)), delay, 500);

            await dictionary.SetAsync("key1", "value2");
            await AssertEx.SucceedsEventually(() => Assert.That(entryUpdated, Is.EqualTo(1)), delay, 500);

            await dictionary.DeleteAsync("key1");
            await AssertEx.SucceedsEventually(() => Assert.That(entryRemoved, Is.EqualTo(1)), delay, 500);

            await dictionary.SetAsync("key1", "value2");
            await dictionary.ClearAsync();
            await AssertEx.SucceedsEventually(() => Assert.That(cleared, Is.EqualTo(1)), delay, 500);

            await dictionary.SetAsync("key1", "value2");
            await dictionary.EvictAllAsync();
            await AssertEx.SucceedsEventually(() => Assert.That(evicted, Is.EqualTo(1)), delay, 500);

            await dictionary.SetAsync("key2", "value2");
            await dictionary.EvictAsync("key2");
            await AssertEx.SucceedsEventually(() => Assert.That(entryEvicted, Is.EqualTo(1)), delay, 500);

            await dictionary.SetAsync("key3", "value2", TimeSpan.FromSeconds(1));
            await AssertEx.SucceedsEventually(() => Assert.That(entryExpired, Is.EqualTo(1)), delay, 500);

            Assert.IsTrue(await dictionary.UnsubscribeAsync(sid));
        }

        [Test]
        public async Task TestListenerClearAll()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            var cleared = 0;

            var sid = await dictionary.SubscribeAsync(events => events
                .Cleared((sender, args) => Interlocked.Increment(ref cleared)),
                false
            );

            await dictionary.SetAsync("key1", "value1");
            await dictionary.ClearAsync();

            await AssertEx.SucceedsEventually(() => Assert.That(cleared, Is.EqualTo(1)), 4000, 500);
        }

        [Test]
        public async Task TestListenerEventOrder()
        {
            var dictionary = await Client.GetMapAsync<int, int>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await dictionary.SetAsync(1, 0);

            var updated = new Queue<int>();
            var sid = await dictionary.SubscribeAsync(events => events
                .EntryUpdated((sender, args) => updated.Enqueue(args.Value)),
                true);

            const int maxSize = 10000;

            for (var i = 1; i < maxSize; i++)
            {
                await dictionary.SetAsync(1, i);
            }

            await AssertEx.SucceedsEventually(() => Assert.That(updated.Count, Is.EqualTo(maxSize - 1)), 10000, 500);

            var oldEventData = -1;
            foreach (var eventData in updated)
            {
                Assert.Less(oldEventData, eventData);
                oldEventData = eventData;
            }
        }

        [Test]
        public async Task TestListenerExtreme()
        {
            var dictionary = await Client.GetMapAsync<string, byte[]>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            const int testItemCount = 1 * 1000;

            for (var i = 0; i < testItemCount; i++)
            {
                await dictionary.SetAsync("key" + i, new[] { byte.MaxValue });
            }

            Assert.AreEqual(await dictionary.GetSizeAsync(), testItemCount);

            var removed = 0;
            var sids = new Guid[testItemCount];
            for (var i = 0; i < testItemCount; i++)
            {
                sids[i] = await dictionary.SubscribeAsync(events => events
                        .EntryRemoved((sender, args) => Interlocked.Increment(ref removed)),
                    "key" + i,
                    false);
            }

            for (var i = 0; i < testItemCount; i++)
            {
                await dictionary.DeleteAsync("key" + i);
            }

            await AssertEx.SucceedsEventually(() => Assert.That(removed, Is.EqualTo(removed)), 120_000, 500);

             for (var i = 0; i < testItemCount; i++)
            {
                await dictionary.UnsubscribeAsync(sids[i]);
            }
        }

        [Test]
        public async Task TestListenerPredicate()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            var added1 = 0;
            var removed1 = 0;
            var added2 = 0;
            var removed2 = 0;
            var removed3 = 0;

            var sid1 = await dictionary.SubscribeAsync(events => events
                    .EntryAdded((sender, args) => Interlocked.Increment(ref added1))
                    .EntryRemoved((sender, args) => Interlocked.Increment(ref removed1)),
                Predicates.Sql("this == value1"),
                false);

            var sid2 = await dictionary.SubscribeAsync(events => events
                .EntryAdded((sender, args) => Interlocked.Increment(ref added2))
                .EntryRemoved((sender, args) => Interlocked.Increment(ref removed2)),
                "key3",
                Predicates.Sql("this == value3"),
                true);

            var sid3 = await dictionary.SubscribeAsync(events => events
                .EntryRemoved((sender, args) => Interlocked.Increment(ref removed3)),
                true
            );

            Assert.That(await dictionary.GetSizeAsync(), Is.Zero);

            await dictionary.SetAsync("key1", "value1");
            await dictionary.SetAsync("key2", "value2");
            await dictionary.SetAsync("key3", "value3");
            await dictionary.SetAsync("key4", "value4");
            await dictionary.SetAsync("key5", "value5");

            Assert.That(await dictionary.GetSizeAsync(), Is.EqualTo(5));

            // do NOT turn that into RemoveAsync, see tests below!
            await dictionary.RemoveAsync("key1");
            await dictionary.RemoveAsync("key3");

            Assert.That(await dictionary.GetSizeAsync(), Is.EqualTo(3));

            await AssertEx.SucceedsEventually(() =>
            {
                Assert.That(added1, Is.EqualTo(1));
                Assert.That(removed1, Is.EqualTo(1));
                Assert.That(added2, Is.EqualTo(1));
                Assert.That(removed2, Is.EqualTo(1));
                Assert.That(removed3, Is.EqualTo(2));
            }, 10000, 500);

            await dictionary.UnsubscribeAsync(sid1);
            await dictionary.UnsubscribeAsync(sid2);
            await dictionary.UnsubscribeAsync(sid3);
        }

        [Test]
        public async Task TestListenerOnRemove()
        {
            var dictionary = await Client.GetMapAsync<string, string>("NAME");
            await using var _ = DestroyAndDispose(dictionary);

            var added1 = 0;
            var removed1 = 0;
            var added2 = 0;
            var removed2 = 0;

            // NOTE
            //
            // Remove (not GetAndRemove) does NOT consider the removed value at all, and once
            // the value is gone, it cannot be used for instance in predicates that work on
            // the value. So in the code below, the first subscription will trigger on remove,
            // because it has no condition, but the second subscription will NOT trigger on
            // remove, because its condition cannot be met - because the value is gone.

            var sid1 = await dictionary.SubscribeAsync(events => events
                .EntryAdded((sender, args) => Interlocked.Increment(ref added1))
                .EntryRemoved((sender, args) => Interlocked.Increment(ref removed1)),
                false
            );

            var sid2 = await dictionary.SubscribeAsync(events => events
                .EntryAdded((sender, args) => Interlocked.Increment(ref added2))
                .EntryRemoved((sender, args) => Interlocked.Increment(ref removed2)),
                Predicates.Sql("this == value1"),
                false
            );

            await dictionary.SetAsync("key1", "value1");
            await dictionary.DeleteAsync("key1");

            await AssertEx.SucceedsEventually(() =>
            {
                Assert.That(added1, Is.EqualTo(1));
                Assert.That(removed1, Is.EqualTo(1));
                Assert.That(added2, Is.EqualTo(1));
            }, 10000, 1000);

            await Task.Delay(1000);

            Assert.That(removed2, Is.Zero);

            await dictionary.UnsubscribeAsync(sid1);
            await dictionary.UnsubscribeAsync(sid2);
        }

        [Test]
        public async Task TestListenerRemove()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            var addedCount = 0;
            var sid = await dictionary.SubscribeAsync(events => events
                .EntryAdded((sender, args) => Interlocked.Increment(ref addedCount)),
                false);

            Assert.IsTrue(await dictionary.UnsubscribeAsync(sid));

            await dictionary.SetAsync("key1", "value1");
            await Task.Delay(2000);
            Assert.That(addedCount, Is.Zero);
        }

        [Test]
        public async Task TestLock()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await dictionary.SetAsync("key1", "value1");
            Assert.AreEqual("value1", await dictionary.GetAsync("key1"));
            await dictionary.LockAsync("key1");

            var couldSet = false;
            await AsyncContext.RunWithNew(async () =>
            {
                couldSet = await dictionary.TryPutAsync("key1", "value2", TimeSpan.FromSeconds(1));
            });

            Assert.That(couldSet, Is.False);
            Assert.AreEqual("value1", await dictionary.GetAsync("key1"));
            await dictionary.ForceUnlockAsync("key1");
        }

        [Test]
        public async Task TestLockTtl()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await dictionary.SetAsync("key1", "value1");
            Assert.AreEqual("value1", await dictionary.GetAsync("key1"));

            await dictionary.LockAsync("key1", TimeSpan.FromMilliseconds(500));

            var couldSet = false;
            await AsyncContext.RunWithNew(async () =>
            {
                couldSet = await dictionary.TryPutAsync("key1", "value2", TimeSpan.FromSeconds(2));
            });

            Assert.That(couldSet);
            Assert.IsFalse(await dictionary.IsLockedAsync("key1"));
            Assert.AreEqual("value2", await dictionary.GetAsync("key1"));
            await dictionary.ForceUnlockAsync("key1");
        }

        [Test]
        public async Task TestLockTtl2()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await dictionary.LockAsync("key1", leaseTime: TimeSpan.FromSeconds(1));

            Assert.That(await dictionary.IsLockedAsync("key1"));

            var id1 = AsyncContext.CurrentContext.Id;

            await AsyncContext.RunWithNew(async () =>
            {
                var id2 = AsyncContext.CurrentContext.Id;
                Assert.That(id2, Is.Not.EqualTo(id1));

                Assert.That(await dictionary.IsLockedAsync("key1"));

                // cannot lock immediately
                Assert.That(await dictionary.TryLockAsync("key1"), Is.False);

                // can lock eventually
                Assert.That(await dictionary.TryLockAsync("key1", TimeSpan.FromSeconds(4)));
            });

            await dictionary.ForceUnlockAsync("key1");
        }

        [Test]
        public async Task TestPutBigData()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            const int dataSize = 128000;
            var largeString = string.Join(",", Enumerable.Range(0, dataSize));

            await dictionary.SetAsync("large_value", largeString);
            Assert.AreEqual(await dictionary.GetSizeAsync(), 1);
        }

        [Test]
        public async Task TestPutIfAbsent()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            Assert.IsNull(await dictionary.PutIfAbsentAsync("key1", "value1"));
            Assert.AreEqual("value1", await dictionary.PutIfAbsentAsync("key1", "value3"));
        }

        [Test]
        public async Task TestPutIfAbsentNewValueTTL_whenKeyPresent()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            const string key = "Key";
            const string value = "Value";
            const string newValue = "newValue";

            await dictionary.SetAsync(key, value);
            var result = await dictionary.PutIfAbsentAsync(key, newValue, TimeSpan.FromSeconds(5));

            Assert.AreEqual(value, result);
            Assert.AreEqual(value, await dictionary.GetAsync(key));
        }

        [Test]
        public async Task TestPutIfAbsentTtl()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            const string key = "Key";
            const string value = "Value";

            var result = await dictionary.PutIfAbsentAsync(key, value, TimeSpan.FromSeconds(5));

            Assert.AreEqual(null, result);
            Assert.AreEqual(value, await dictionary.GetAsync(key));
        }

        [Test]
        public async Task TestPutIfAbsentTTL_whenExpire()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            const string key = "Key";
            const string value = "Value";

            var result = await dictionary.PutIfAbsentAsync(key, value, TimeSpan.FromMilliseconds(100));
            Assert.That(result, Is.Null);

            await AssertEx.SucceedsEventually(async () =>
            {
                Assert.That(await dictionary.GetAsync(key), Is.Null);
            }, 5000, 500);
        }

        [Test]
        public async Task TestPutIfAbsentTTL_whenKeyPresent()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            const string key = "Key";
            const string value = "Value";

            await dictionary.SetAsync(key, value);
            var result = await dictionary.PutIfAbsentAsync(key, value, TimeSpan.FromMinutes(5));

            Assert.AreEqual(value, result);
            Assert.AreEqual(value, await dictionary.GetAsync(key));
        }

        [Test]
        public async Task TestPutIfAbsentTTL_whenKeyPresentAfterExpire()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            const string key = "Key";
            const string value = "Value";

            await dictionary.SetAsync(key, value);
            var result = await dictionary.PutIfAbsentAsync(key, value, TimeSpan.FromSeconds(1));

            Assert.AreEqual(value, result);
            Assert.AreEqual(value, await dictionary.GetAsync(key));
        }

        [Test]
        public async Task TestPutTransient()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            Assert.AreEqual(0, await dictionary.GetSizeAsync());
            await dictionary.PutTransientAsync("key1", "value1", TimeSpan.FromMilliseconds(100));
            Assert.AreEqual("value1", await dictionary.GetAsync("key1"));

            await AssertEx.SucceedsEventually(async () =>
            {
                Assert.That(await dictionary.GetAsync("key1"), Is.Not.EqualTo("value1"));
            }, 5000, 500);
        }

        [Test]
        public async Task TestPutTtl()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await dictionary.SetAsync("key1", "value1", TimeSpan.FromMilliseconds(100));
            Assert.IsNotNull(await dictionary.GetAsync("key1"));

            await AssertEx.SucceedsEventually(async () =>
            {
                Assert.That(await dictionary.GetAsync("key1"), Is.Null);
            }, 5000, 500);
        }

        [Test]
        public async Task TestRemoveAndDelete()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await FillAsync(dictionary);

            Assert.That(await dictionary.RemoveAsync("key10"), Is.Null);
            await dictionary.DeleteAsync("key9");
            Assert.AreEqual(9, await dictionary.GetSizeAsync());
            for (var i = 0; i < 9; i++)
            {
                var o = await dictionary.RemoveAsync("key" + i);
                Assert.AreEqual("value" + i, o);
            }
            Assert.AreEqual(0, await dictionary.GetSizeAsync());
        }

        [Test]
        public async Task TestRemoveIfSame()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await FillAsync(dictionary);

            Assert.IsFalse(await dictionary.RemoveAsync("key2", "value"));
            Assert.AreEqual(10, await dictionary.GetSizeAsync());
            Assert.IsTrue(await dictionary.RemoveAsync("key2", "value2"));
            Assert.AreEqual(9, await dictionary.GetSizeAsync());
        }

        [Test]
        public async Task TestRemoveInterceptor()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await dictionary.RemoveInterceptorAsync("interceptor");
        }

        [Test]
        public async Task TestRemoveAllWithPredicate()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await FillAsync(dictionary);

            await dictionary.RemoveAllAsync(Predicates.Sql("this != value1"));
            Assert.AreEqual(1, (await dictionary.GetValuesAsync()).Count);
            Assert.AreEqual("value1", await dictionary.GetAsync("key1"));
        }

        [Test]
        public async Task TestReplace()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            Assert.That(await dictionary.ReplaceAsync("key1", "value1"), Is.Null);
            await dictionary.SetAsync("key1", "value1");
            Assert.AreEqual("value1", await dictionary.ReplaceAsync("key1", "value2"));
            Assert.AreEqual("value2", await dictionary.GetAsync("key1"));
            Assert.IsFalse(await dictionary.ReplaceAsync("key1", "value1", "value3"));
            Assert.AreEqual("value2", await dictionary.GetAsync("key1"));
            Assert.IsTrue(await dictionary.ReplaceAsync("key1", "value2", "value3"));
            Assert.AreEqual("value3", await dictionary.GetAsync("key1"));
        }

        [Test]
        public async Task TestSet()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await dictionary.SetAsync("key1", "value1");
            Assert.AreEqual("value1", await dictionary.GetAsync("key1"));
            await dictionary.SetAsync("key1", "value2");
            Assert.AreEqual("value2", await dictionary.GetAsync("key1"));
            await dictionary.SetAsync("key1", "value3", TimeSpan.FromMilliseconds(100));
            Assert.AreEqual("value3", await dictionary.GetAsync("key1"));

            await AssertEx.SucceedsEventually(async () =>
            {
                Assert.That(await dictionary.GetAsync("key1"), Is.Null);
            }, 5000, 500);
        }

        [Test]
        public async Task TestTryPutRemove()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            Assert.IsTrue(await dictionary.TryPutAsync("key1", "value1", TimeSpan.FromSeconds(1)));
            Assert.IsTrue(await dictionary.TryPutAsync("key2", "value2", TimeSpan.FromSeconds(1)));
            await dictionary.LockAsync("key1");
            await dictionary.LockAsync("key2");

            var count = 0;

            var task1 = AsyncContext.RunWithNew(async () =>
            {
                if (!(await dictionary.TryPutAsync("key1", "value1", TimeSpan.FromSeconds(1))))
                    Interlocked.Increment(ref count);
            });

            var task2 = AsyncContext.RunWithNew(async () =>
            {
                if (!(await dictionary.TryRemoveAsync("key2", TimeSpan.FromSeconds(1))))
                    Interlocked.Increment(ref count);
            });

            await Task.WhenAll(task1, task2);

            Assert.That(count, Is.EqualTo(2));
            Assert.AreEqual("value1", await dictionary.GetAsync("key1"));
            Assert.AreEqual("value2", await dictionary.GetAsync("key2"));

            await dictionary.ForceUnlockAsync("key1");
            await dictionary.ForceUnlockAsync("key2");
        }

        [Test]
        public async Task TestUnlock()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await dictionary.ForceUnlockAsync("key1");
            await dictionary.SetAsync("key1", "value1");
            Assert.AreEqual("value1", await dictionary.GetAsync("key1"));
            await dictionary.LockAsync("key1");
            Assert.IsTrue(await dictionary.IsLockedAsync("key1"));
            await dictionary.UnlockAsync("key1");
            Assert.IsFalse(await dictionary.IsLockedAsync("key1"));
            await dictionary.ForceUnlockAsync("key1");
        }

        [Test]
        public async Task TestValues()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await dictionary.SetAsync("key1", "value1");
            var values = await dictionary.GetValuesAsync();
            var enumerator = values.GetEnumerator();
            enumerator.MoveNext();
            Assert.AreEqual("value1", enumerator.Current);
        }

        [Test]
        public async Task TestValuesPredicate()
        {
            var dictionary = await Client.GetMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await FillAsync(dictionary);

            var values = await dictionary.GetValuesAsync(new SqlPredicate("this == value1"));
            Assert.AreEqual(1, values.Count);
            var enumerator = values.GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual("value1", enumerator.Current);
        }
    }
}
