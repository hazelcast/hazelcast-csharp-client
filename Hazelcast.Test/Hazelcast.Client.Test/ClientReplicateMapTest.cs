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
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientReplicateMapTest : SingleMemberBaseTest
    {
        [SetUp]
        public void Init()
        {
            ReplicatedMap = Client.GetReplicatedMap<int?, string>(TestSupport.RandomString());
        }

        [TearDown]
        public static void Destroy()
        {
            ReplicatedMap.Destroy();
        }

        private static IReplicatedMap<int?, string> ReplicatedMap;

        private sealed class ReplicatedMapListener : IEntryListener<int?, string>
        {
            public CountdownEvent Add { get; set; }
            public CountdownEvent Update { get; set; }
            public CountdownEvent Remove { get; set; }
            public CountdownEvent Evict { get; set; }
            public CountdownEvent ClearAll { get; set; }

            public void EntryAdded(EntryEvent<int?, string> @event)
            {
                Add.Signal();
            }

            public void EntryUpdated(EntryEvent<int?, string> @event)
            {
                Update.Signal();
            }

            public void EntryRemoved(EntryEvent<int?, string> @event)
            {
                Remove.Signal();
            }

            public void EntryEvicted(EntryEvent<int?, string> @event)
            {
                Evict.Signal();
            }

            public void MapCleared(MapEvent @event)
            {
                ClearAll.Signal();
            }

            public void MapEvicted(MapEvent @event)
            {
                Assert.Fail("Replicated map should not receive evict all event!!!");
            }
        }

        private ReplicatedMapListener  CreateEventListener(int eventCount)
        {
            return new ReplicatedMapListener
            {
                Add = new CountdownEvent(eventCount),
                Update = new CountdownEvent(eventCount),
                Remove = new CountdownEvent(eventCount),
                Evict = new CountdownEvent(eventCount),
                ClearAll = new CountdownEvent(eventCount),
            };
        }

        [Test]
        public void TestAddEntryListener()
        {
            var listener = CreateEventListener(1);
            ReplicatedMap.AddEntryListener(listener);
            ReplicatedMap.Put(1, "value1");
            Assert.IsTrue(listener.Add.Wait(TimeSpan.FromSeconds(5)));
            ReplicatedMap.Put(1, "value1");
            Assert.IsTrue(listener.Update.Wait(TimeSpan.FromSeconds(5)));
            ReplicatedMap.Remove(1);
            Assert.IsTrue(listener.Remove.Wait(TimeSpan.FromSeconds(5)));
            ReplicatedMap.Clear();
            Assert.IsTrue(listener.ClearAll.Wait(TimeSpan.FromSeconds(5)));
        }

        [Test]
        public void TestAddEntryListener_key()
        {
            var listener = CreateEventListener(1);
            ReplicatedMap.AddEntryListener(listener, 1);
            ReplicatedMap.Put(1, "value1");
            Assert.IsTrue(listener.Add.Wait(TimeSpan.FromSeconds(5)));
        }

        [Test]
        public void TestAddEntryListener_key_other()
        {
            var listener = CreateEventListener(1);
            ReplicatedMap.AddEntryListener(listener, 1);
            ReplicatedMap.Put(2, "value2");
            Assert.IsFalse(listener.Add.Wait(TimeSpan.FromSeconds(5)));
        }

        [Test]
        public void TestAddEntryListener_predicate()
        {
            var listener = CreateEventListener(5);
            ReplicatedMap.AddEntryListener(listener, Predicates.Key().LessThan(5));
            FillValues();
            Assert.IsTrue(listener.Add.Wait(TimeSpan.FromSeconds(10)));
        }

        [Test]
        public void TestAddEntryListener_predicate_key()
        {
            var listener = CreateEventListener(1);
            var listener2 = CreateEventListener(1);
            ReplicatedMap.AddEntryListener(listener, Predicates.Key().LessThan(5), 2);
            ReplicatedMap.AddEntryListener(listener2, Predicates.Key().LessThan(5), 6);
            FillValues();
            Assert.IsTrue(listener.Add.Wait(TimeSpan.FromSeconds(5)));
            Assert.IsFalse(listener2.Add.Wait(TimeSpan.FromSeconds(5)));
        }

        [Test]
        public void TestClear()
        {
            ReplicatedMap.Put(1, "value1");
            ReplicatedMap.Put(2, "value2");
            ReplicatedMap.Clear();
            Assert.AreEqual(0, ReplicatedMap.Size());
        }

        [Test]
        public void TestContainsKey()
        {
            ReplicatedMap.Put(1, "value1");
            ReplicatedMap.Put(2, "value2");
            Assert.True(ReplicatedMap.ContainsKey(1));
            Assert.True(ReplicatedMap.ContainsKey(2));
            Assert.False(ReplicatedMap.ContainsKey(3));
        }

        [Test]
        public void TestContainsValue()
        {
            ReplicatedMap.Put(1, "value1");
            ReplicatedMap.Put(2, "value2");
            Assert.True(ReplicatedMap.ContainsValue("value1"));
            Assert.True(ReplicatedMap.ContainsValue("value2"));
            Assert.False(ReplicatedMap.ContainsValue("value3"));
        }

        [Test]
        public void TestGet()
        {
            ReplicatedMap.Put(1, "value1");
            var value = ReplicatedMap.Get(1);
            Assert.AreEqual(value, "value1");
        }

        [Test]
        public void TestIsEmpty()
        {
            Assert.True(ReplicatedMap.IsEmpty());
        }

        [Test]
        public void TestEntrySet()
        {
            FillValues();
            var keyValuePairs = ReplicatedMap.EntrySet();
            for (int i = 0; i < 10; i++)
            {
                Assert.True(keyValuePairs.Contains(new KeyValuePair<int?, string>(i, "value" + i)));
            }
        }

        [Test]
        public void TestKeySet()
        {
            FillValues();
            var keyset = ReplicatedMap.KeySet();
            for (int i = 0; i < 10; i++)
            {
                Assert.True(keyset.Contains(i));
            }
        }

        [Test]
        public void TestPut()
        {
            ReplicatedMap.Put(1, "value1");
            var value = ReplicatedMap.Get(1);
            Assert.AreEqual(value, "value1");
        }

        [Test]
		public void TestPut_null()
		{
			Assert.Throws<NullReferenceException>(() =>
        {
            ReplicatedMap.Put(1, null);
        });
		}

        [Test]
        public void TestPut_ttl()
        {
            ReplicatedMap.Put(1, "value1", 5, TimeUnit.Seconds);
            var value = ReplicatedMap.Get(1);
            Assert.AreEqual(value, "value1");
            TestSupport.AssertTrueEventually(() => { Assert.Null(ReplicatedMap.Get(1)); });
        }

        [Test]
        public void TestPutAll()
        {
            var entries = new Dictionary<int?, string>();
            for (int i = 0; i < 10; i++)
            {
                entries.Add(i, "value" + i);
            }
            ReplicatedMap.PutAll(entries);
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual("value"+i, ReplicatedMap.Get(i));
            }
            Assert.AreEqual(10, ReplicatedMap.Size());
        }

        [Test]
        public void TestRemove()
        {
            ReplicatedMap.Put(1, "value1");
            var value =ReplicatedMap.Remove(1);
            Assert.AreEqual(value, "value1");
            Assert.AreEqual(0, ReplicatedMap.Size());

        }

        [Test]
        public void TestRemoveEntryListener()
        {
            var listener = CreateEventListener(1);
            var regId = ReplicatedMap.AddEntryListener(listener, 1);
            Assert.IsTrue(ReplicatedMap.RemoveEntryListener(regId));
            var invalidRegistrationId = Guid.NewGuid();
            Assert.IsFalse(ReplicatedMap.RemoveEntryListener(invalidRegistrationId));
        }

        [Test]
        public void TestSize()
        {
            FillValues();
            Assert.AreEqual(10, ReplicatedMap.Size());
        }

        [Test]
        public void TestValues()
        {
            FillValues();
            var values = ReplicatedMap.Values();
            for (int i = 0; i < 10; i++)
            {
                Assert.IsTrue(values.Contains("value" + i));
            }
            Assert.AreEqual(10, values.Count);
        }

        private void FillValues()
        {
            for (int i = 0; i < 10; i++)
            {
                ReplicatedMap.Put(i, "value" + i);
            }
        }
    }
}