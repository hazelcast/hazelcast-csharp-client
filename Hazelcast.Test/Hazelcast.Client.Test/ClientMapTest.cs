// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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
            map = Client.GetMap<object, object>(TestSupport.RandomString());
        }

        [TearDown]
        public static void Destroy()
        {
            map.Clear();
        }

        protected override void ConfigureClient(ClientConfig config)
        {
            base.ConfigureClient(config);
            config.GetSerializationConfig().AddPortableFactory(1, new PortableFactory());
            config.GetSerializationConfig()
                .AddDataSerializableFactory(IdentifiedFactory.FactoryId, new IdentifiedFactory());
        }

        //internal const string name = "test";

        internal static IMap<object, object> map;

        private void FillMap()
        {
            for (var i = 0; i < 10; i++)
            {
                map.Put("key" + i, "value" + i);
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

            public virtual void SetId(int id)
            {
                this.id = id;
            }
        }

        private class Interceptor : IMapInterceptor, IIdentifiedDataSerializable
        {
            public void WriteData(IObjectDataOutput output)
            {
            }

            public int GetFactoryId()
            {
                return 1;
            }

            public int GetId()
            {
                return 0;
            }

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
        public virtual void TestAddIndex()
        {
            map.AddIndex("name", true);
        }

        [Test, Ignore, ExpectedException(typeof(HazelcastException))]
        public void TestAddInterceptor()
        {
            //TODO: not currently possible to test this

            var id = map.AddInterceptor(new Interceptor());
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestAsyncGet()
        {
            FillMap();
            var f = map.GetAsync("key1");

            var o = f.Result;
            Assert.AreEqual("value1", o);
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestAsyncPut()
        {
            FillMap();
            var f = map.PutAsync("key3", "value");

            Assert.False(f.IsCompleted);

            var o = f.Result;
            Assert.AreEqual("value3", o);
            Assert.AreEqual("value", map.Get("key3"));
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestAsyncPutWithTtl()
        {
            var latch = new CountdownEvent(1);

            map.AddEntryListener(new EntryAdapter<object, object>(
                delegate { },
                delegate { },
                delegate { },
                delegate { latch.Signal(); }
            ), true);

            var f1 = map.PutAsync("key", "value1", 1, TimeUnit.Seconds);
            Assert.IsNull(f1.Result);
            Assert.AreEqual("value1", map.Get("key"));

            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(10)));

            TestSupport.AssertTrueEventually(() => { Assert.IsNull(map.Get("key")); });
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestAsyncRemove()
        {
            FillMap();
            var f = map.RemoveAsync("key4");
            Assert.False(f.IsCompleted);

            var o = f.Result;
            Assert.AreEqual("value4", o);
            Assert.AreEqual(9, map.Size());
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestContains()
        {
            FillMap();
            Assert.IsFalse(map.ContainsKey("key10"));
            Assert.IsTrue(map.ContainsKey("key1"));
            Assert.IsFalse(map.ContainsValue("value10"));
            Assert.IsTrue(map.ContainsValue("value1"));
        }

        [Test]
        public virtual void TestEntrySet()
        {
            map.Put("key1", "value1");
            map.Put("key2", "value2");
            map.Put("key3", "value3");

            var keyValuePairs = map.EntrySet();

            IDictionary<object, object> tempDict = new Dictionary<object, object>();
            foreach (var keyValuePair in keyValuePairs)
            {
                tempDict.Add(keyValuePair);
            }

            Assert.True(tempDict.ContainsKey("key1"));
            Assert.True(tempDict.ContainsKey("key2"));
            Assert.True(tempDict.ContainsKey("key3"));

            object value;
            tempDict.TryGetValue("key1", out value);
            Assert.AreEqual("value1", value);

            tempDict.TryGetValue("key2", out value);
            Assert.AreEqual("value2", value);

            tempDict.TryGetValue("key3", out value);
            Assert.AreEqual("value3", value);
        }

        [Test]
        public virtual void TestEntrySetPredicate()
        {
            map.Put("key1", "value1");
            map.Put("key2", "value2");
            map.Put("key3", "value3");

            var keyValuePairs = map.EntrySet(new SqlPredicate("this == value1"));
            Assert.AreEqual(1, keyValuePairs.Count);

            var enumerator = keyValuePairs.GetEnumerator();
            enumerator.MoveNext();
            Assert.AreEqual("key1", enumerator.Current.Key);
            Assert.AreEqual("value1", enumerator.Current.Value);
        }

        [Test]
        public virtual void TestEntryView()
        {
            var item = ItemGenerator.GenerateItem(1);
            map.Put("key1", item);
            map.Get("key1");
            map.Get("key1");


            var entryview = map.GetEntryView("key1");
            var value = entryview.GetValue() as Item;

            Assert.AreEqual("key1", entryview.GetKey());
            Assert.True(item.Equals(value));
            //Assert.AreEqual(2, entryview.GetHits());
            //Assert.True(entryview.GetCreationTime() > 0);
            //Assert.True(entryview.GetLastAccessTime() > 0);
            //Assert.True(entryview.GetLastUpdateTime() > 0);
        }

        [Test]
        public virtual void TestEvict()
        {
            map.Put("key1", "value1");
            Assert.AreEqual("value1", map.Get("key1"));

            map.Evict("key1");

            Assert.AreEqual(0, map.Size());
            Assert.AreNotEqual("value1", map.Get("key1"));
        }

        [Test]
        public void TestEvictAll()
        {
            map.Put("key1", "value1");
            map.Put("key2", "value2");
            map.Put("key3", "value3");

            Assert.AreEqual(3, map.Size());

            map.Lock("key3");
            map.EvictAll();

            Assert.AreEqual(1, map.Size());
            Assert.AreEqual("value3", map.Get("key3"));
        }

        [Test]
        public virtual void TestFlush()
        {
            map.Flush();
        }

        [Test]
        public virtual void TestExecuteOnKey()
        {
            FillMap();
            const string key = "key1";
            const string value = "value10";
            var entryProcessor = new IdentifiedEntryProcessor(value);
            var result = map.ExecuteOnKey(key, entryProcessor);
            Assert.AreEqual(result, value);
            Assert.AreEqual(result, map.Get(key));
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public virtual void TestExecuteOnKey_nullKey()
        {
            FillMap();
            const string key = null;
            const string value = "value10";
            var entryProcessor = new IdentifiedEntryProcessor(value);
            map.ExecuteOnKey(key, entryProcessor);
        }

        [Test]
        public virtual void TestExecuteOnKeys()
        {
            FillMap();
            var keys = new HashSet<object> {"key1", "key5"};
            const string value = "valueX";
            var entryProcessor = new IdentifiedEntryProcessor(value);
            var result = map.ExecuteOnKeys(keys, entryProcessor);
            foreach (var resultKV in result)
            {
                Assert.AreEqual(resultKV.Value, value);
                Assert.AreEqual(value, map.Get(resultKV.Key));
            }
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public virtual void TestExecuteOnKeys_keysNotNull()
        {
            FillMap();
            ISet<object> keys = null;
            const string value = "valueX";
            var entryProcessor = new IdentifiedEntryProcessor(value);
            map.ExecuteOnKeys(keys, entryProcessor);
        }

        [Test]
        public virtual void TestExecuteOnKeys_keysEmpty()
        {
            FillMap();
            ISet<object> keys = new HashSet<object>();
            const string value = "valueX";
            var entryProcessor = new IdentifiedEntryProcessor(value);
            var result = map.ExecuteOnKeys(keys, entryProcessor);
            Assert.AreEqual(result.Count, 0);
        }

        [Test]
        public virtual void TestExecuteOnEntries()
        {
            FillMap();
            const string value = "valueX";
            var entryProcessor = new IdentifiedEntryProcessor(value);
            var result = map.ExecuteOnEntries(entryProcessor);
            foreach (var resultKV in result)
            {
                Assert.AreEqual(resultKV.Value, value);
                Assert.AreEqual(value, map.Get(resultKV.Key));
            }
        }

        [Test]
        public virtual void TestExecuteOnEntriesWithPredicate()
        {
            FillMap();
            const string value = "valueX";
            var entryProcessor = new IdentifiedEntryProcessor(value);
            var result = map.ExecuteOnEntries(entryProcessor, Predicates.Sql("this == value5"));
            Assert.AreEqual(result.Count, 1);
            foreach (var resultKV in result)
            {
                Assert.AreEqual(resultKV.Value, value);
                Assert.AreEqual(value, map.Get(resultKV.Key));
            }
        }

        [Test]
        public virtual void TestSubmitToKey()
        {
            FillMap();
            const string key = "key1";
            const string value = "value10";
            var entryProcessor = new IdentifiedEntryProcessor(value);
            var task = map.SubmitToKey(key, entryProcessor);
            Assert.AreEqual(task.Result, value);
            Assert.AreEqual(task.Result, map.Get(key));
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public virtual void TestSubmitToKey_nullKey()
        {
            const string key = null;
            const string value = "value10";
            var entryProcessor = new IdentifiedEntryProcessor(value);
            map.SubmitToKey(key, entryProcessor);
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestForceUnlock()
        {
            map.Lock("key1");
            var latch = new CountdownEvent(1);

            var t1 = new Thread(delegate(object o)
            {
                map.ForceUnlock("key1");
                latch.Signal();
            });

            t1.Start();

            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(100)));
            Assert.IsFalse(map.IsLocked("key1"));
        }

        [Test]
        public virtual void TestGet()
        {
            FillMap();
            for (var i = 0; i < 10; i++)
            {
                var o = map.Get("key" + i);
                Assert.AreEqual("value" + i, o);
            }
        }

        [Test]
        public virtual void TestGetAllExtreme()
        {
            IDictionary<object, object> mm = new Dictionary<object, object>();
            const int keycount = 1000;

            //insert dummy keys and values 
            foreach (var itemIndex in Enumerable.Range(0, keycount))
            {
                mm.Add(itemIndex.ToString(), itemIndex.ToString());
            }

            map.PutAll(mm);
            Assert.AreEqual(map.Size(), keycount);

            var dictionary = map.GetAll(mm.Keys);
            Assert.AreEqual(dictionary.Count, keycount);
        }

        [Test]
        public virtual void TestGetAllPutAll()
        {
            IDictionary<object, object> mm = new Dictionary<object, object>();
            for (var i = 0; i < 100; i++)
            {
                mm.Add(i, i);
            }
            map.PutAll(mm);
            Assert.AreEqual(map.Size(), 100);
            for (var i_1 = 0; i_1 < 100; i_1++)
            {
                Assert.AreEqual(map.Get(i_1), i_1);
            }
            var ss = new HashSet<object> {1, 3};

            var m2 = map.GetAll(ss);
            Assert.AreEqual(m2.Count, 2);

            object gv;
            m2.TryGetValue(1, out gv);
            Assert.AreEqual(gv, 1);

            m2.TryGetValue(3, out gv);
            Assert.AreEqual(gv, 3);
        }

        [Test]
        public virtual void TestGetEntryView()
        {
            map.Put("item0", "value0");
            map.Put("item1", "value1");
            map.Put("item2", "value2");

            var entryView = map.GetEntryView("item1");

            Assert.AreEqual(0, entryView.GetHits());

            Assert.AreEqual("item1", entryView.GetKey());
            Assert.AreEqual("value1", entryView.GetValue());
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestIsEmpty()
        {
            Assert.IsTrue(map.IsEmpty());
            map.Put("key1", "value1");
            Assert.IsFalse(map.IsEmpty());
        }

        [Test]
        public virtual void TestKeySet()
        {
            map.Put("key1", "value1");

            var keySet = map.KeySet();

            var enumerator = keySet.GetEnumerator();

            enumerator.MoveNext();
            Assert.AreEqual("key1", enumerator.Current);
        }

        [Test]
        public void TestKeySetPredicate()
        {
            FillMap();

            var values = map.KeySet(new SqlPredicate("this == value1"));
            Assert.AreEqual(1, values.Count);
            var enumerator = values.GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual("key1", enumerator.Current);
        }

        [Test]
        public void TestListener()
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

            var reg1 = map.AddEntryListener(listener1, false);
            var reg2 = map.AddEntryListener(listener2, "key3", true);


            map.Put("key1", "value1");
            map.Put("key2", "value2");
            map.Put("key3", "value3");
            map.Put("key4", "value4");
            map.Put("key5", "value5");
            map.Remove("key1");
            map.Remove("key3");

            Assert.IsTrue(latch1Add.Wait(TimeSpan.FromSeconds(10)));
            Assert.IsTrue(latch1Remove.Wait(TimeSpan.FromSeconds(10)));
            Assert.IsTrue(latch2Add.Wait(TimeSpan.FromSeconds(5)));
            Assert.IsTrue(latch2Remove.Wait(TimeSpan.FromSeconds(5)));

            Assert.IsTrue(map.RemoveEntryListener(reg1));
            Assert.IsTrue(map.RemoveEntryListener(reg2));
        }

        [Test]
        public void TestListenerClearAll()
        {
            var latchClearAll = new CountdownEvent(1);

            var listener1 = new EntryAdapter<object, object>(
                delegate { },
                delegate { },
                delegate { },
                delegate { },
                delegate { },
                delegate { latchClearAll.Signal(); });

            var reg1 = map.AddEntryListener(listener1, false);

            map.Put("key1", "value1");
            //map.Put("key2", "value2");
            //map.Put("key3", "value3");
            //map.Put("key4", "value4");
            //map.Put("key5", "value5");

            map.Clear();

            Assert.IsTrue(latchClearAll.Wait(TimeSpan.FromSeconds(15)));
        }

        [Test]
        public void TestListenerEventOrder()
        {
            const int maxSize = 10000;
            var map2 = Client.GetMap<int, int>(TestSupport.RandomString());
            map2.Put(1, 0);

            var eventDataReceived = new Queue<int>();

            var listener = new EntryAdapter<int, int>(
                e => { },
                e => { },
                e => eventDataReceived.Enqueue(e.GetValue()),
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
        public void TestListenerExtreme()
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
                map.Put("key" + i, new[] {byte.MaxValue});
            }

            Assert.AreEqual(map.Size(), TestItemCount);

            for (var i = 0; i < TestItemCount; i++)
            {
                map.AddEntryListener(listener, "key" + i, false);
            }

            for (var i = 0; i < TestItemCount; i++)
            {
                var o = map.RemoveAsync("key" + i).Result;
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
        public void TestListenerPredicate()
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

            map.AddEntryListener(listener1, new SqlPredicate("this == value1"), false);
            map.AddEntryListener(listener2, new SqlPredicate("this == value3"), "key3", true);

            map.Put("key1", "value1");
            map.Put("key2", "value2");
            map.Put("key3", "value3");
            map.Put("key4", "value4");
            map.Put("key5", "value5");

            map.Remove("key1");
            map.Remove("key3");

            Assert.IsTrue(latch1Add.Wait(TimeSpan.FromSeconds(10)));
            Assert.IsTrue(latch1Remove.Wait(TimeSpan.FromSeconds(10)));
            Assert.IsTrue(latch2Add.Wait(TimeSpan.FromSeconds(5)));
            Assert.IsTrue(latch2Remove.Wait(TimeSpan.FromSeconds(5)));
        }

        [Test]
        public void TestListenerRemove()
        {
            var latch1Add = new CountdownEvent(1);
            var listener1 = new EntryAdapter<object, object>(
                delegate { latch1Add.Signal(); },
                delegate { },
                delegate { },
                delegate { });

            var reg1 = map.AddEntryListener(listener1, false);

            Assert.IsTrue(map.RemoveEntryListener(reg1));

            map.Put("key1", "value1");

            Assert.IsFalse(latch1Add.Wait(TimeSpan.FromSeconds(1)));
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestLock()
        {
            map.Put("key1", "value1");
            Assert.AreEqual("value1", map.Get("key1"));
            map.Lock("key1");
            var latch = new CountdownEvent(1);

            var t1 = new Thread(delegate(object o)
            {
                map.TryPut("key1", "value2", 1, TimeUnit.Seconds);
                latch.Signal();
            });
            t1.Start();
            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(5)));
            Assert.AreEqual("value1", map.Get("key1"));
            map.ForceUnlock("key1");
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestLockTtl()
        {
            map.Put("key1", "value1");
            Assert.AreEqual("value1", map.Get("key1"));
            var leaseTime = 500;
            map.Lock("key1", leaseTime, TimeUnit.Milliseconds);
            var latch = new CountdownEvent(1);
            var t1 = new Thread(delegate(object o)
            {
                map.TryPut("key1", "value2", 2000, TimeUnit.Milliseconds);
                latch.Signal();
            });
            t1.Start();
            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(10)));
            Assert.IsFalse(map.IsLocked("key1"));
            Assert.AreEqual("value2", map.Get("key1"));
            map.ForceUnlock("key1");
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestLockTtl2()
        {
            map.Lock("key1", 1, TimeUnit.Seconds);
            var latch = new CountdownEvent(2);
            var t1 = new Thread(delegate(object o)
            {
                if (!map.TryLock("key1"))
                {
                    latch.Signal();
                }
                try
                {
                    if (map.TryLock("key1", 2, TimeUnit.Seconds))
                    {
                        latch.Signal();
                    }
                }
                catch
                {
                }
            });

            t1.Start();
            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(10)));
            map.ForceUnlock("key1");
        }

        [Test]
        public virtual void TestPutBigData()
        {
            const int dataSize = 128000;
            var largeString = string.Join(",", Enumerable.Range(0, dataSize));

            map.Put("large_value", largeString);
            Assert.AreEqual(map.Size(), 1);
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestPutIfAbsent()
        {
            Assert.IsNull(map.PutIfAbsent("key1", "value1"));
            Assert.AreEqual("value1", map.PutIfAbsent("key1", "value3"));
        }

        [Test]
        public void TestPutIfAbsentNewValueTTL_whenKeyPresent()
        {
            object key = "Key";
            object value = "Value";
            object newValue = "newValue";

            map.Put(key, value);
            var result = map.PutIfAbsent(key, newValue, 5, TimeUnit.Minutes);

            Assert.AreEqual(value, result);
            Assert.AreEqual(value, map.Get(key));
        }

        [Test]
        public void TestPutIfAbsentTtl()
        {
            object key = "Key";
            object value = "Value";

            var result = map.PutIfAbsent(key, value, 5, TimeUnit.Minutes);

            Assert.AreEqual(null, result);
            Assert.AreEqual(value, map.Get(key));
        }

        [Test]
        public void TestPutIfAbsentTTL_whenExpire()
        {
            object key = "Key";
            object value = "Value";

            var ttl = 100;
            var result = map.PutIfAbsent(key, value, ttl, TimeUnit.Milliseconds);

            TestSupport.AssertTrueEventually(() =>
            {
                Assert.AreEqual(null, result);
                Assert.AreEqual(null, map.Get(key));
            });
        }

        [Test]
        public void TestPutIfAbsentTTL_whenKeyPresent()
        {
            object key = "Key";
            object value = "Value";

            map.Put(key, value);
            var result = map.PutIfAbsent(key, value, 5, TimeUnit.Minutes);

            Assert.AreEqual(value, result);
            Assert.AreEqual(value, map.Get(key));
        }

        [Test]
        public void TestPutIfAbsentTTL_whenKeyPresentAfterExpire()
        {
            object key = "Key";
            object value = "Value";

            map.Put(key, value);
            var result = map.PutIfAbsent(key, value, 1, TimeUnit.Seconds);

            Assert.AreEqual(value, result);
            Assert.AreEqual(value, map.Get(key));
        }

        [Test]
        public virtual void TestPutTransient()
        {
            Assert.AreEqual(0, map.Size());
            map.PutTransient("key1", "value1", 100, TimeUnit.Milliseconds);
            Assert.AreEqual("value1", map.Get("key1"));

            TestSupport.AssertTrueEventually(() => { Assert.AreNotEqual("value1", map.Get("key1")); });
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestPutTtl()
        {
            var ttl = 100;
            map.Put("key1", "value1", ttl, TimeUnit.Milliseconds);
            Assert.IsNotNull(map.Get("key1"));

            TestSupport.AssertTrueEventually(() => { Assert.IsNull(map.Get("key1")); });
        }

        [Test]
        public virtual void TestRemoveAndDelete()
        {
            FillMap();
            Assert.IsNull(map.Remove("key10"));
            map.Delete("key9");
            Assert.AreEqual(9, map.Size());
            for (var i = 0; i < 9; i++)
            {
                var o = map.Remove("key" + i);
                Assert.AreEqual("value" + i, o);
            }
            Assert.AreEqual(0, map.Size());
        }

        [Test]
        public virtual void TestRemoveIfSame()
        {
            FillMap();
            Assert.IsFalse(map.Remove("key2", "value"));
            Assert.AreEqual(10, map.Size());
            Assert.IsTrue(map.Remove("key2", "value2"));
            Assert.AreEqual(9, map.Size());
        }

        [Test]
        public void TestRemoveInterceptor()
        {
            map.RemoveInterceptor("interceptor");
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestReplace()
        {
            Assert.IsNull(map.Replace("key1", "value1"));
            map.Put("key1", "value1");
            Assert.AreEqual("value1", map.Replace("key1", "value2"));
            Assert.AreEqual("value2", map.Get("key1"));
            Assert.IsFalse(map.Replace("key1", "value1", "value3"));
            Assert.AreEqual("value2", map.Get("key1"));
            Assert.IsTrue(map.Replace("key1", "value2", "value3"));
            Assert.AreEqual("value3", map.Get("key1"));
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestSet()
        {
            map.Set("key1", "value1");
            Assert.AreEqual("value1", map.Get("key1"));
            map.Set("key1", "value2");
            Assert.AreEqual("value2", map.Get("key1"));
            map.Set("key1", "value3", 100, TimeUnit.Milliseconds);
            Assert.AreEqual("value3", map.Get("key1"));

            TestSupport.AssertTrueEventually(() => Assert.IsNull(map.Get("key1")));
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestTryPutRemove()
        {
            Assert.IsTrue(map.TryPut("key1", "value1", 1, TimeUnit.Seconds));
            Assert.IsTrue(map.TryPut("key2", "value2", 1, TimeUnit.Seconds));
            map.Lock("key1");
            map.Lock("key2");
            var latch = new CountdownEvent(2);

            var t1 = new Thread(delegate(object o)
            {
                var result = map.TryPut("key1", "value3", 1, TimeUnit.Seconds);
                if (!result)
                {
                    latch.Signal();
                }
            });

            var t2 = new Thread(delegate(object o)
            {
                var result = map.TryRemove("key2", 1, TimeUnit.Seconds);
                if (!result)
                {
                    latch.Signal();
                }
            });

            t1.Start();
            t2.Start();

            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(20)));
            Assert.AreEqual("value1", map.Get("key1"));
            Assert.AreEqual("value2", map.Get("key2"));

            map.ForceUnlock("key1");
            map.ForceUnlock("key2");
        }

        [Test]
        public virtual void TestUnlock()
        {
            map.ForceUnlock("key1");
            map.Put("key1", "value1");
            Assert.AreEqual("value1", map.Get("key1"));
            map.Lock("key1");
            Assert.IsTrue(map.IsLocked("key1"));
            map.Unlock("key1");
            Assert.IsFalse(map.IsLocked("key1"));
            map.ForceUnlock("key1");
        }

        [Test]
        public virtual void TestValues()
        {
            map.Put("key1", "value1");
            var values = map.Values();
            var enumerator = values.GetEnumerator();
            enumerator.MoveNext();
            Assert.AreEqual("value1", enumerator.Current);
        }

        [Test]
        public void TestValuesPredicate()
        {
            FillMap();

            var values = map.Values(new SqlPredicate("this == value1"));
            Assert.AreEqual(1, values.Count);
            var enumerator = values.GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual("value1", enumerator.Current);
        }
    }

    internal class IdentifiedFactory : IDataSerializableFactory
    {
        internal const int FactoryId = 66;

        public IIdentifiedDataSerializable Create(int typeId)
        {
            if (typeId == IdentifiedEntryProcessor.ClassId)
            {
                return new IdentifiedEntryProcessor();
            }
            return null;
        }
    }

    internal class IdentifiedEntryProcessor : IIdentifiedDataSerializable, IEntryProcessor
    {
        internal const int ClassId = 1;

        private string value;

        public IdentifiedEntryProcessor(string value = null)
        {
            this.value = value;
        }

        public void ReadData(IObjectDataInput input)
        {
            value = input.ReadUTF();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteUTF(value);
        }

        public int GetFactoryId()
        {
            return IdentifiedFactory.FactoryId;
        }

        public int GetId()
        {
            return ClassId;
        }
    }
}