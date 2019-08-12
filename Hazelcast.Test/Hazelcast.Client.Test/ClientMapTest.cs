// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Client.Model;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Map;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientMapTest : SingleMemberBaseTest
    {
        [SetUp]
        public void Init()
        {
            _map = Client.GetMap<object, object>(TestSupport.RandomString());
        }

        [TearDown]
        public void Destroy()
        {
            _map.Clear();
        }

        protected override void ConfigureClient(ClientConfig config)
        {
            base.ConfigureClient(config);
            config.GetSerializationConfig().AddPortableFactory(1, new PortableFactory());
            config.GetSerializationConfig()
                .AddDataSerializableFactory(IdentifiedFactory.FactoryId, new IdentifiedFactory());
        }

        IMap<object, object> _map;

        void FillMap()
        {
            for (var i = 0; i < 10; i++)
            {
                _map.Put("key" + i, "value" + i);
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

            public int GetId()
            {
                return id;
            }

            public void SetId(int id)
            {
                this.id = id;
            }
        }

        class Interceptor : IMapInterceptor, IIdentifiedDataSerializable
        {
            public void WriteData(IObjectDataOutput output)
            {
            }

            public int GetFactoryId() => 1;
            public int GetId() => 0;

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

        [Test]
        public void AddIndex()
        {
            _map.AddIndex("name", true);
        }

        [Ignore("not currently possible to test this")]
        [Test]
        public void AddInterceptor()
        {
            Assert.Throws<HazelcastException>(() =>
            {
                //TODO: not currently possible to test this

                var id = _map.AddInterceptor(new Interceptor());
            });
        }

        [Test]
        public async Task AsyncGet()
        {
            FillMap();
            var value = await _map.GetAsync("key1");

            Assert.AreEqual("value1", value);
        }

        [Test]
        public async Task AsyncPut()
        {
            FillMap();
            var o = await _map.PutAsync("key3", "value");

            Assert.AreEqual("value3", o);
            Assert.AreEqual("value", _map.Get("key3"));
        }

        [Test]
        public async Task AsyncPutWithTtl()
        {
            var latch = new SemaphoreSlim(0);

            _map.AddEntryListener(new EntryAdapter<object, object>(
                delegate { },
                delegate { },
                delegate { },
                delegate { latch.Release(); }
            ), true);

            await _map.PutAsync("key", "value1", 1, TimeUnit.Seconds);

            var actual = await _map.GetAsync("key");
            Assert.AreEqual("value1", actual);

            Assert.IsTrue(await latch.WaitAsync(TimeSpan.FromSeconds(10)));

            // TODO: consider async Get
            TestSupport.AssertTrueEventually(() => { Assert.IsNull(_map.Get("key")); });
        }

        [Test]
        public async Task AsyncRemove()
        {
            FillMap();
            var o = await _map.RemoveAsync("key4");

            Assert.AreEqual("value4", o);
            Assert.AreEqual(9, _map.Size());
        }

        [Test]
        public void Contains()
        {
            FillMap();
            Assert.IsFalse(_map.ContainsKey("key10"));
            Assert.IsTrue(_map.ContainsKey("key1"));
            Assert.IsFalse(_map.ContainsValue("value10"));
            Assert.IsTrue(_map.ContainsValue("value1"));
        }

        [Test]
        public void EntrySet()
        {
            _map.Put("key1", "value1");
            _map.Put("key2", "value2");
            _map.Put("key3", "value3");

            var keyValuePairs = _map.EntrySet();

            IDictionary<object, object> tempDict = new Dictionary<object, object>();
            foreach (var keyValuePair in keyValuePairs)
            {
                tempDict.Add(keyValuePair);
            }

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
        public void EntrySetPredicate()
        {
            _map.Put("key1", "value1");
            _map.Put("key2", "value2");
            _map.Put("key3", "value3");

            var keyValuePairs = _map.EntrySet(new SqlPredicate("this == value1"));
            Assert.AreEqual(1, keyValuePairs.Count);

            var kvp = keyValuePairs.First();
            Assert.AreEqual("key1", kvp.Key);
            Assert.AreEqual("value1", kvp.Value);
        }

        [Test]
        public void EntryView()
        {
            var item = ItemGenerator.GenerateItem(1);
            _map.Put("key1", item);
            _map.Get("key1");
            _map.Get("key1");

            var entryView = _map.GetEntryView("key1");
            var value = entryView.GetValue() as Item;

            Assert.AreEqual("key1", entryView.GetKey());
            Assert.True(item.Equals(value));
            //Assert.AreEqual(2, entryview.GetHits());
            //Assert.True(entryview.GetCreationTime() > 0);
            //Assert.True(entryview.GetLastAccessTime() > 0);
            //Assert.True(entryview.GetLastUpdateTime() > 0);
        }

        [Test]
        public void TestEvict()
        {
            _map.Put("key1", "value1");
            Assert.AreEqual("value1", _map.Get("key1"));

            _map.Evict("key1");

            Assert.AreEqual(0, _map.Size());
            Assert.AreNotEqual("value1", _map.Get("key1"));
        }

        [Test]
        public void EvictAll()
        {
            _map.Put("key1", "value1");
            _map.Put("key2", "value2");
            _map.Put("key3", "value3");

            Assert.AreEqual(3, _map.Size());

            _map.Lock("key3");
            _map.EvictAll();

            Assert.AreEqual(1, _map.Size());
            Assert.AreEqual("value3", _map.Get("key3"));
        }

        [Test]
        public void Flush()
        {
            _map.Flush();
        }

        [Test]
        public void ExecuteOnKey()
        {
            FillMap();
            const string key = "key1";
            const string value = "value10";
            var entryProcessor = new IdentifiedEntryProcessor(value);
            var result = _map.ExecuteOnKey(key, entryProcessor);
            Assert.AreEqual(result, value);
            Assert.AreEqual(result, _map.Get(key));
        }

        [Test]
        public void TestExecuteOnKey_nullKey()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                FillMap();
                const string key = null;
                const string value = "value10";
                var entryProcessor = new IdentifiedEntryProcessor(value);
                _map.ExecuteOnKey(key, entryProcessor);
            });
        }

        [Test]
        public void ExecuteOnKeys()
        {
            FillMap();
            var keys = new HashSet<object> { "key1", "key5" };
            const string value = "valueX";
            var entryProcessor = new IdentifiedEntryProcessor(value);
            var result = _map.ExecuteOnKeys(keys, entryProcessor);
            foreach (var kvp in result)
            {
                Assert.AreEqual(kvp.Value, value);
                Assert.AreEqual(value, _map.Get(kvp.Key));
            }
        }

        [Test]
        public void ExecuteOnKeys_keysNotNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                FillMap();
                const string value = "valueX";
                var entryProcessor = new IdentifiedEntryProcessor(value);
                _map.ExecuteOnKeys(null, entryProcessor);
            });
        }

        [Test]
        public void ExecuteOnKeys_keysEmpty()
        {
            FillMap();
            var keys = new HashSet<object>();
            const string value = "valueX";
            var entryProcessor = new IdentifiedEntryProcessor(value);
            var result = _map.ExecuteOnKeys(keys, entryProcessor);
            Assert.AreEqual(result.Count, 0);
        }

        [Test]
        public void ExecuteOnEntries()
        {
            FillMap();
            const string value = "valueX";
            var entryProcessor = new IdentifiedEntryProcessor(value);
            var result = _map.ExecuteOnEntries(entryProcessor);
            foreach (var kvp in result)
            {
                Assert.AreEqual(value, kvp.Value);
                Assert.AreEqual(value, _map.Get(kvp.Key));
            }
        }

        [Test]
        public void ExecuteOnEntriesWithPredicate()
        {
            FillMap();
            const string value = "valueX";
            var entryProcessor = new IdentifiedEntryProcessor(value);
            var result = _map.ExecuteOnEntries(entryProcessor, Predicates.Sql("this == value5"));
            Assert.AreEqual(result.Count, 1);
            foreach (var kvp in result)
            {
                Assert.AreEqual(value, kvp.Value);
                Assert.AreEqual(value, _map.Get(kvp.Key));
            }
        }

        [Test]
        public async Task SubmitToKey()
        {
            FillMap();
            const string key = "key1";
            const string value = "value10";
            var entryProcessor = new IdentifiedEntryProcessor(value);
            var result = await _map.SubmitToKey(key, entryProcessor);
            Assert.AreEqual(value, result);
            Assert.AreEqual(_map.Get(key), result);
        }

        [Test]
        public void SubmitToKey_NullKey()
        {
            const string key = null;
            const string value = "value10";
            var entryProcessor = new IdentifiedEntryProcessor(value);

            Assert.Throws<ArgumentNullException>(() => _map.SubmitToKey(key, entryProcessor), "Should have thrown an exception");
        }

        [Test]
        public async Task ForceUnlock()
        {
            _map.Lock("key1");
            var sem = new SemaphoreSlim(0);

            var t = Task.Run(() =>
            {
                _map.ForceUnlock("key1");
                sem.Release();
            });

            Assert.IsTrue(await sem.WaitAsync(TimeSpan.FromSeconds(100)));
            Assert.IsFalse(_map.IsLocked("key1"));
        }

        [Test]
        public void Get()
        {
            FillMap();
            for (var i = 0; i < 10; i++)
            {
                var o = _map.Get("key" + i);
                Assert.AreEqual("value" + i, o);
            }
        }

        [Test, Repeat(100)]
        public void GetAllExtreme()
        {
            var mm = new Dictionary<object, object>();
            const int count = 1000;

            //insert dummy keys and values 
            foreach (var itemIndex in Enumerable.Range(0, count))
            {
                mm.Add(itemIndex.ToString(), itemIndex.ToString());
            }

            _map.PutAll(mm);
            Assert.AreEqual(_map.Size(), count);

            var dictionary = _map.GetAll(mm.Keys);
            Assert.AreEqual(count, dictionary.Count);
            foreach (var pair in dictionary)
            {
                Assert.AreEqual(mm[pair.Key], pair.Value);
            }
        }

        [Test]
        public void GetAllPutAll()
        {
            var mm = new Dictionary<object, object>();
            for (var i = 0; i < 100; i++)
            {
                mm.Add(i, i);
            }
            _map.PutAll(mm);
            Assert.AreEqual(_map.Size(), 100);
            for (var j = 0; j < 100; j++)
            {
                Assert.AreEqual(_map.Get(j), j);
            }
            var ss = new HashSet<object> { 1, 3 };

            var m2 = _map.GetAll(ss);
            Assert.AreEqual(m2.Count, 2);

            m2.TryGetValue(1, out var gv);
            Assert.AreEqual(1, gv);

            m2.TryGetValue(3, out gv);
            Assert.AreEqual(3, gv);
        }

        [Test]
        public void GetEntryView()
        {
            _map.Put("item0", "value0");
            _map.Put("item1", "value1");
            _map.Put("item2", "value2");

            var entryView = _map.GetEntryView("item1");

            Assert.AreEqual(0, entryView.GetHits());

            Assert.AreEqual("item1", entryView.GetKey());
            Assert.AreEqual("value1", entryView.GetValue());
        }

        [Test]
        public void IsEmpty()
        {
            Assert.IsTrue(_map.IsEmpty());
            _map.Put("key1", "value1");
            Assert.IsFalse(_map.IsEmpty());
        }

        [Test]
        public void KeySet()
        {
            _map.Put("key1", "value1");

            var keySet = _map.KeySet();

            var value = keySet.First();
            Assert.AreEqual("key1", value);
        }

        [Test]
        public void KeySetPredicate()
        {
            FillMap();

            var values = _map.KeySet(new SqlPredicate("this == value1"));
            Assert.AreEqual(1, values.Count);

            Assert.AreEqual("key1", values.First());
        }

        [Test]
        public void Listener()
        {
            var latch1Add = new CountdownEvent(5);
            var latch1Remove = new CountdownEvent(2);
            var latch2Add = new CountdownEvent(1);
            var latch2Remove = new CountdownEvent(1);
            var listener1 = new EntryAdapter<object, object>(
                delegate { latch1Add.Signal(); },
                delegate { latch1Remove.Signal(); },
                delegate { },
                delegate { });

            var listener2 = new EntryAdapter<object, object>(
                delegate { latch2Add.Signal(); },
                delegate { latch2Remove.Signal(); },
                delegate { },
                delegate { });

            var reg1 = _map.AddEntryListener(listener1, false);
            var reg2 = _map.AddEntryListener(listener2, "key3", true);


            _map.Put("key1", "value1");
            _map.Put("key2", "value2");
            _map.Put("key3", "value3");
            _map.Put("key4", "value4");
            _map.Put("key5", "value5");
            _map.Remove("key1");
            _map.Remove("key3");

            Assert.IsTrue(latch1Add.Wait(TimeSpan.FromSeconds(10)));
            Assert.IsTrue(latch1Remove.Wait(TimeSpan.FromSeconds(10)));
            Assert.IsTrue(latch2Add.Wait(TimeSpan.FromSeconds(5)));
            Assert.IsTrue(latch2Remove.Wait(TimeSpan.FromSeconds(5)));

            Assert.IsTrue(_map.RemoveEntryListener(reg1));
            Assert.IsTrue(_map.RemoveEntryListener(reg2));
        }

        [Test]
        public void Listener_SingleEventListeners()
        {
            var listener = new ListenerImpl<object, object>();
            var reg1 = _map.AddEntryListener(listener, false);

            _map.Put("key1", "value1");
            Assert.IsTrue(listener.GetLatch(EntryEventType.Added).WaitOne(TimeSpan.FromSeconds(10)));

            _map.Put("key1", "value2");
            Assert.IsTrue(listener.GetLatch(EntryEventType.Updated).WaitOne(TimeSpan.FromSeconds(10)));

            _map.Remove("key1");
            Assert.IsTrue(listener.GetLatch(EntryEventType.Removed).WaitOne(TimeSpan.FromSeconds(10)));

            _map.Put("key1", "value2");
            _map.Clear();
            Assert.IsTrue(listener.GetLatch(EntryEventType.ClearAll).WaitOne(TimeSpan.FromSeconds(10)));

            _map.Put("key1", "value2");
            _map.EvictAll();
            Assert.IsTrue(listener.GetLatch(EntryEventType.EvictAll).WaitOne(TimeSpan.FromSeconds(10)));

            _map.Put("key2", "value2");
            _map.Evict("key2");
            Assert.IsTrue(listener.GetLatch(EntryEventType.Evicted).WaitOne(TimeSpan.FromSeconds(10)));

            _map.Put("key3", "value2", 1L, TimeUnit.Seconds);
            Assert.IsTrue(listener.GetLatch(EntryEventType.Expired).WaitOne(TimeSpan.FromSeconds(10)));

            Assert.IsTrue(_map.RemoveEntryListener(reg1));
        }

        [Test]
        public void ListenerClearAll()
        {
            var latchClearAll = new CountdownEvent(1);

            var listener1 = new EntryAdapter<object, object>(
                delegate { },
                delegate { },
                delegate { },
                delegate { },
                delegate { },
                delegate { latchClearAll.Signal(); });

            var reg1 = _map.AddEntryListener(listener1, false);

            _map.Put("key1", "value1");

            _map.Clear();

            Assert.IsTrue(latchClearAll.Wait(TimeSpan.FromSeconds(15)));
        }

        [Test]
        public void ListenerEventOrder()
        {
            const int maxSize = 10000;
            var map2 = Client.GetMap<int, int>(TestSupport.RandomString());
            map2.Put(1, 0);

            var eventDataReceived = new Queue<int>();

            var listener = new EntryAdapter<int, int>(
                e => { },
                e => { },
                e =>
                {
                    var value = e.GetValue();
                    eventDataReceived.Enqueue(value);
                },
                e => { });

            map2.AddEntryListener(listener, true);

            for (var i = 1; i < maxSize; i++)
            {
                map2.Put(1, i);
            }

            TestSupport.AssertTrueEventually(() => Assert.AreEqual(maxSize - 1, eventDataReceived.Count));

            var oldEventData = -1;
            foreach (var eventData in eventDataReceived)
            {
                Assert.Less(oldEventData, eventData);
                oldEventData = eventData;
            }
        }

        [Test]
        public void ListenerExtreme()
        {
            const int TestItemCount = 1 * 1000;
            var latch = new CountdownEvent(TestItemCount);
            var listener = new EntryAdapter<object, object>(
                delegate { },
                delegate { latch.Signal(); },
                delegate { },
                delegate { });

            for (var i = 0; i < TestItemCount; i++)
            {
                _map.Put("key" + i, new[] { byte.MaxValue });
            }

            Assert.AreEqual(_map.Size(), TestItemCount);

            for (var i = 0; i < TestItemCount; i++)
            {
                _map.AddEntryListener(listener, "key" + i, false);
            }

            for (var i = 0; i < TestItemCount; i++)
            {
                var o = _map.RemoveAsync("key" + i).Result;
            }

            latch.Wait(TimeSpan.FromSeconds(10));
            //Console.WriteLine(latch.CurrentCount);
            latch.Wait(TimeSpan.FromSeconds(10));
            //Console.WriteLine(latch.CurrentCount);
            latch.Wait(TimeSpan.FromSeconds(10));
            //Console.WriteLine(latch.CurrentCount);
            latch.Wait(TimeSpan.FromSeconds(10));
            //Console.WriteLine(latch.CurrentCount);
            latch.Wait(TimeSpan.FromSeconds(10));
            //Console.WriteLine(latch.CurrentCount);
            latch.Wait(TimeSpan.FromSeconds(10));
            //Console.WriteLine(latch.CurrentCount);
            latch.Wait(TimeSpan.FromSeconds(10));
            //Console.WriteLine(latch.CurrentCount);
            Assert.True(latch.Wait(TimeSpan.FromSeconds(100)));
        }

        [Test]
        public void ListenerPredicate()
        {
            var latch1Add = new CountdownEvent(1);
            var latch1Remove = new CountdownEvent(1);
            var latch2Add = new CountdownEvent(1);
            var latch2Remove = new CountdownEvent(1);
            var listener1 = new EntryAdapter<object, object>(
                delegate { latch1Add.Signal(); },
                delegate { latch1Remove.Signal(); },
                delegate { },
                delegate { });

            var listener2 = new EntryAdapter<object, object>(
                delegate { latch2Add.Signal(); },
                delegate { latch2Remove.Signal(); },
                delegate { },
                delegate { });

            _map.AddEntryListener(listener1, new SqlPredicate("this == value1"), false);
            _map.AddEntryListener(listener2, new SqlPredicate("this == value3"), "key3", true);

            _map.Put("key1", "value1");
            _map.Put("key2", "value2");
            _map.Put("key3", "value3");
            _map.Put("key4", "value4");
            _map.Put("key5", "value5");

            _map.Remove("key1");
            _map.Remove("key3");

            Assert.IsTrue(latch1Add.Wait(TimeSpan.FromSeconds(10)));
            Assert.IsTrue(latch1Remove.Wait(TimeSpan.FromSeconds(10)));
            Assert.IsTrue(latch2Add.Wait(TimeSpan.FromSeconds(5)));
            Assert.IsTrue(latch2Remove.Wait(TimeSpan.FromSeconds(5)));
        }

        [Test]
        public void ListenerRemove()
        {
            var latch1Add = new CountdownEvent(1);
            var listener1 = new EntryAdapter<object, object>(
                delegate { latch1Add.Signal(); },
                delegate { },
                delegate { },
                delegate { });

            var reg1 = _map.AddEntryListener(listener1, false);

            Assert.IsTrue(_map.RemoveEntryListener(reg1));

            _map.Put("key1", "value1");

            Assert.IsFalse(latch1Add.Wait(TimeSpan.FromSeconds(1)));
        }

        [Test]
        public void Lock()
        {
            _map.Put("key1", "value1");
            Assert.AreEqual("value1", _map.Get("key1"));
            _map.Lock("key1");
            var latch = new CountdownEvent(1);

            var t = Task.Run(() =>
            {
                _map.TryPut("key1", "value2", 1, TimeUnit.Seconds);
                latch.Signal();
            });

            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(5)));
            Assert.AreEqual("value1", _map.Get("key1"));
            _map.ForceUnlock("key1");
        }

        [Test]
        public void TestLockTtl()
        {
            _map.Put("key1", "value1");
            Assert.AreEqual("value1", _map.Get("key1"));
            const int leaseTime = 500;

            _map.Lock("key1", leaseTime, TimeUnit.Milliseconds);
            var latch = new CountdownEvent(1);
            var t = Task.Run(() =>
            {
                _map.TryPut("key1", "value2", 2000, TimeUnit.Milliseconds);
                latch.Signal();
            });

            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(10)));
            Assert.IsFalse(_map.IsLocked("key1"));
            Assert.AreEqual("value2", _map.Get("key1"));
            _map.ForceUnlock("key1");
        }

        [Test]
        public void LockTtl2()
        {
            _map.Lock("key1", 1, TimeUnit.Seconds);
            var latch = new CountdownEvent(2);

            var t = Task.Run(() =>
            {
                if (!_map.TryLock("key1"))
                {
                    latch.Signal();
                }
                try
                {
                    if (_map.TryLock("key1", 2, TimeUnit.Seconds))
                    {
                        latch.Signal();
                    }
                }
                catch
                {
                }
            });

            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(10)));
            _map.ForceUnlock("key1");
        }

        [Test]
        public void PutBigData()
        {
            const int dataSize = 128000;
            var largeString = string.Join(",", Enumerable.Range(0, dataSize));

            _map.Put("large_value", largeString);
            Assert.AreEqual(_map.Size(), 1);
        }

        [Test]
        public void PutIfAbsent()
        {
            Assert.IsNull(_map.PutIfAbsent("key1", "value1"));
            Assert.AreEqual("value1", _map.PutIfAbsent("key1", "value3"));
        }

        [Test]
        public void PutIfAbsentNewValueTTL_whenKeyPresent()
        {
            object key = "Key";
            object value = "Value";
            object newValue = "newValue";

            _map.Put(key, value);
            var result = _map.PutIfAbsent(key, newValue, 5, TimeUnit.Minutes);

            Assert.AreEqual(value, result);
            Assert.AreEqual(value, _map.Get(key));
        }

        [Test]
        public void PutIfAbsentTtl()
        {
            object key = "Key";
            object value = "Value";

            var result = _map.PutIfAbsent(key, value, 5, TimeUnit.Minutes);

            Assert.AreEqual(null, result);
            Assert.AreEqual(value, _map.Get(key));
        }

        [Test]
        public void PutIfAbsentTTL_whenExpire()
        {
            object key = "Key";
            object value = "Value";

            const int ttl = 100;
            var result = _map.PutIfAbsent(key, value, ttl, TimeUnit.Milliseconds);

            TestSupport.AssertTrueEventually(() =>
            {
                Assert.AreEqual(null, result);
                Assert.AreEqual(null, _map.Get(key));
            });
        }

        [Test]
        public void PutIfAbsentTTL_whenKeyPresent()
        {
            object key = "Key";
            object value = "Value";

            _map.Put(key, value);
            var result = _map.PutIfAbsent(key, value, 5, TimeUnit.Minutes);

            Assert.AreEqual(value, result);
            Assert.AreEqual(value, _map.Get(key));
        }

        [Test]
        public void PutIfAbsentTTL_whenKeyPresentAfterExpire()
        {
            object key = "Key";
            object value = "Value";

            _map.Put(key, value);
            var result = _map.PutIfAbsent(key, value, 1, TimeUnit.Seconds);

            Assert.AreEqual(value, result);
            Assert.AreEqual(value, _map.Get(key));
        }

        [Test]
        public void PutTransient()
        {
            Assert.AreEqual(0, _map.Size());
            _map.PutTransient("key1", "value1", 100, TimeUnit.Milliseconds);
            Assert.AreEqual("value1", _map.Get("key1"));

            TestSupport.AssertTrueEventually(() => { Assert.AreNotEqual("value1", _map.Get("key1")); });
        }

        [Test]
        public void TestPutTtl()
        {
            const int ttl = 100;
            _map.Put("key1", "value1", ttl, TimeUnit.Milliseconds);
            Assert.IsNotNull(_map.Get("key1"));

            TestSupport.AssertTrueEventually(() => { Assert.IsNull(_map.Get("key1")); });
        }

        [Test]
        public void RemoveAndDelete()
        {
            FillMap();
            Assert.IsNull(_map.Remove("key10"));
            _map.Delete("key9");
            Assert.AreEqual(9, _map.Size());
            for (var i = 0; i < 9; i++)
            {
                var o = _map.Remove("key" + i);
                Assert.AreEqual("value" + i, o);
            }
            Assert.AreEqual(0, _map.Size());
        }

        [Test]
        public void RemoveIfSame()
        {
            FillMap();
            Assert.IsFalse(_map.Remove("key2", "value"));
            Assert.AreEqual(10, _map.Size());
            Assert.IsTrue(_map.Remove("key2", "value2"));
            Assert.AreEqual(9, _map.Size());
        }

        [Test]
        public void RemoveInterceptor()
        {
            _map.RemoveInterceptor("interceptor");
        }

        [Test]
        [Category("3.8")]
        public void RemoveAllWithPredicate()
        {
            FillMap();

            _map.RemoveAll(new SqlPredicate("this != value1"));
            Assert.AreEqual(1, _map.Values().Count);
            Assert.AreEqual("value1", _map.Get("key1"));
        }

        [Test]
        public void Replace()
        {
            Assert.IsNull(_map.Replace("key1", "value1"));
            _map.Put("key1", "value1");
            Assert.AreEqual("value1", _map.Replace("key1", "value2"));
            Assert.AreEqual("value2", _map.Get("key1"));
            Assert.IsFalse(_map.Replace("key1", "value1", "value3"));
            Assert.AreEqual("value2", _map.Get("key1"));
            Assert.IsTrue(_map.Replace("key1", "value2", "value3"));
            Assert.AreEqual("value3", _map.Get("key1"));
        }

        [Test]
        public void Set()
        {
            _map.Set("key1", "value1");
            Assert.AreEqual("value1", _map.Get("key1"));
            _map.Set("key1", "value2");
            Assert.AreEqual("value2", _map.Get("key1"));
            _map.Set("key1", "value3", 100, TimeUnit.Milliseconds);
            Assert.AreEqual("value3", _map.Get("key1"));

            TestSupport.AssertTrueEventually(() => Assert.IsNull(_map.Get("key1")));
        }

        [Test]
        public void TryPutRemove()
        {
            Assert.IsTrue(_map.TryPut("key1", "value1", 1, TimeUnit.Seconds));
            Assert.IsTrue(_map.TryPut("key2", "value2", 1, TimeUnit.Seconds));
            _map.Lock("key1");
            _map.Lock("key2");
            var latch = new CountdownEvent(2);

            var t1 = Task.Run(() =>
            {
                var result = _map.TryPut("key1", "value3", 1, TimeUnit.Seconds);
                if (!result)
                {
                    latch.Signal();
                }
            });

            var t2 = Task.Run(() =>
            {
                var result = _map.TryRemove("key2", 1, TimeUnit.Seconds);
                if (!result)
                {
                    latch.Signal();
                }
            });

            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(20)));
            Assert.AreEqual("value1", _map.Get("key1"));
            Assert.AreEqual("value2", _map.Get("key2"));

            _map.ForceUnlock("key1");
            _map.ForceUnlock("key2");
        }

        [Test]
        public void Unlock()
        {
            _map.ForceUnlock("key1");
            _map.Put("key1", "value1");
            Assert.AreEqual("value1", _map.Get("key1"));
            _map.Lock("key1");
            Assert.IsTrue(_map.IsLocked("key1"));
            _map.Unlock("key1");
            Assert.IsFalse(_map.IsLocked("key1"));
            _map.ForceUnlock("key1");
        }

        [Test]
        public void Values()
        {
            _map.Put("key1", "value1");
            var first = _map.Values().First();
            Assert.AreEqual("value1", first);
        }

        [Test]
        public void ValuesPredicate()
        {
            FillMap();

            var values = _map.Values(new SqlPredicate("this == value1"));
            Assert.AreEqual(1, values.Count);
            var first = values.First();
            Assert.AreEqual("value1", first);
        }

        class ListenerImpl<TKey, TValue> : EntryAddedListener<TKey, TValue>,
            EntryUpdatedListener<TKey, TValue>, EntryRemovedListener<TKey, TValue>, EntryEvictedListener<TKey, TValue>,
            MapClearedListener, MapEvictedListener,
            EntryMergedListener<TKey, TValue>, EntryExpiredListener<TKey, TValue>
        {
            readonly ConcurrentDictionary<EntryEventType, AutoResetEvent> _latches;

            public ListenerImpl()
            {
                _latches = new ConcurrentDictionary<EntryEventType, AutoResetEvent>();
                foreach (EntryEventType et in Enum.GetValues(typeof(EntryEventType)))
                {
                    _latches.TryAdd(et, new AutoResetEvent(false));
                }
            }

            public void EntryAdded(EntryEvent<TKey, TValue> @event)
            {
                _latches[EntryEventType.Added].Set();
            }

            public void EntryUpdated(EntryEvent<TKey, TValue> @event)
            {
                _latches[EntryEventType.Updated].Set();
            }

            public void EntryRemoved(EntryEvent<TKey, TValue> @event)
            {
                _latches[EntryEventType.Removed].Set();
            }

            public void EntryEvicted(EntryEvent<TKey, TValue> @event)
            {
                _latches[EntryEventType.Evicted].Set();
            }

            public void MapCleared(MapEvent @event)
            {
                _latches[EntryEventType.ClearAll].Set();
            }

            public void MapEvicted(MapEvent @event)
            {
                _latches[EntryEventType.EvictAll].Set();
            }

            public void EntryMerged(EntryEvent<TKey, TValue> @event)
            {
                _latches[EntryEventType.Merged].Set();
            }

            public void EntryExpired(EntryEvent<TKey, TValue> @event)
            {
                _latches[EntryEventType.Expired].Set();
            }

            public AutoResetEvent GetLatch(EntryEventType key) => _latches[key];
        }
    }
}