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
            _replicatedMap = Client.GetReplicatedMap<int?, string>(TestSupport.RandomString());
        }

        [TearDown]
        public void Destroy()
        {
            _replicatedMap.Destroy();
        }

        IReplicatedMap<int?, string> _replicatedMap;

        sealed class ReplicatedMapListener : IEntryListener<int?, string>
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

        static ReplicatedMapListener CreateEventListener(int eventCount)
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
        public void AddEntryListener()
        {
            var listener = CreateEventListener(1);
            _replicatedMap.AddEntryListener(listener);
            _replicatedMap.Put(1, "value1");
            Assert.IsTrue(listener.Add.Wait(TimeSpan.FromSeconds(5)));
            _replicatedMap.Put(1, "value1");
            Assert.IsTrue(listener.Update.Wait(TimeSpan.FromSeconds(5)));
            _replicatedMap.Remove(1);
            Assert.IsTrue(listener.Remove.Wait(TimeSpan.FromSeconds(5)));
            _replicatedMap.Clear();
            Assert.IsTrue(listener.ClearAll.Wait(TimeSpan.FromSeconds(5)));
        }

        [Test]
        public void AddEntryListener_key()
        {
            var listener = CreateEventListener(1);
            _replicatedMap.AddEntryListener(listener, 1);
            _replicatedMap.Put(1, "value1");
            Assert.IsTrue(listener.Add.Wait(TimeSpan.FromSeconds(5)));
        }

        [Test]
        public void AddEntryListener_key_other()
        {
            var listener = CreateEventListener(1);
            _replicatedMap.AddEntryListener(listener, 1);
            _replicatedMap.Put(2, "value2");
            Assert.IsFalse(listener.Add.Wait(TimeSpan.FromSeconds(5)));
        }

        [Test]
        public void AddEntryListener_predicate()
        {
            var listener = CreateEventListener(5);
            _replicatedMap.AddEntryListener(listener, Predicates.Key().LessThan(5));
            FillValues();
            Assert.IsTrue(listener.Add.Wait(TimeSpan.FromSeconds(10)));
        }

        [Test]
        public void AddEntryListener_predicate_key()
        {
            var listener = CreateEventListener(1);
            var listener2 = CreateEventListener(1);
            _replicatedMap.AddEntryListener(listener, Predicates.Key().LessThan(5), 2);
            _replicatedMap.AddEntryListener(listener2, Predicates.Key().LessThan(5), 6);
            FillValues();
            Assert.IsTrue(listener.Add.Wait(TimeSpan.FromSeconds(5)));
            Assert.IsFalse(listener2.Add.Wait(TimeSpan.FromSeconds(5)));
        }

        [Test]
        public void Clear()
        {
            _replicatedMap.Put(1, "value1");
            _replicatedMap.Put(2, "value2");
            _replicatedMap.Clear();
            Assert.AreEqual(0, _replicatedMap.Size());
        }

        [Test]
        public void ContainsKey()
        {
            _replicatedMap.Put(1, "value1");
            _replicatedMap.Put(2, "value2");
            Assert.True(_replicatedMap.ContainsKey(1));
            Assert.True(_replicatedMap.ContainsKey(2));
            Assert.False(_replicatedMap.ContainsKey(3));
        }

        [Test]
        public void ContainsValue()
        {
            _replicatedMap.Put(1, "value1");
            _replicatedMap.Put(2, "value2");
            Assert.True(_replicatedMap.ContainsValue("value1"));
            Assert.True(_replicatedMap.ContainsValue("value2"));
            Assert.False(_replicatedMap.ContainsValue("value3"));
        }

        [Test]
        public void Get()
        {
            _replicatedMap.Put(1, "value1");
            var value = _replicatedMap.Get(1);
            Assert.AreEqual(value, "value1");
        }

        [Test]
        public void IsEmpty()
        {
            Assert.True(_replicatedMap.IsEmpty());
        }

        [Test]
        public void EntrySet()
        {
            FillValues();
            var keyValuePairs = _replicatedMap.EntrySet();
            for (var i = 0; i < 10; i++)
            {
                Assert.True(keyValuePairs.Contains(new KeyValuePair<int?, string>(i, "value" + i)));
            }
        }

        [Test]
        public void KeySet()
        {
            FillValues();
            var set = _replicatedMap.KeySet();
            for (var i = 0; i < 10; i++)
            {
                Assert.True(set.Contains(i));
            }
        }

        [Test]
        public void Put()
        {
            _replicatedMap.Put(1, "value1");
            var value = _replicatedMap.Get(1);
            Assert.AreEqual(value, "value1");
        }

        [Test]
        public void Put_null()
        {
            Assert.Throws<NullReferenceException>(() =>
            {
                _replicatedMap.Put(1, null);
            });
        }

        [Test]
        public void Put_ttl()
        {
            _replicatedMap.Put(1, "value1", 5, TimeUnit.Seconds);
            var value = _replicatedMap.Get(1);
            Assert.AreEqual(value, "value1");
            TestSupport.AssertTrueEventually(() => { Assert.Null(_replicatedMap.Get(1)); });
        }

        [Test]
        public void PutAll()
        {
            var entries = new Dictionary<int?, string>();
            for (var i = 0; i < 10; i++)
            {
                entries.Add(i, "value" + i);
            }
            _replicatedMap.PutAll(entries);

            for (var i = 0; i < 10; i++)
            {
                Assert.AreEqual("value" + i, _replicatedMap.Get(i));
            }
            Assert.AreEqual(10, _replicatedMap.Size());
        }

        [Test]
        public void Remove()
        {
            _replicatedMap.Put(1, "value1");
            var value = _replicatedMap.Remove(1);
            Assert.AreEqual(value, "value1");
            Assert.AreEqual(0, _replicatedMap.Size());

        }

        [Test]
        public void RemoveEntryListener()
        {
            var listener = CreateEventListener(1);
            var regId = _replicatedMap.AddEntryListener(listener, 1);
            Assert.IsTrue(_replicatedMap.RemoveEntryListener(regId));
            Assert.IsFalse(_replicatedMap.RemoveEntryListener("Invalid Registration Id"));
        }

        [Test]
        public void Size()
        {
            FillValues();
            Assert.AreEqual(10, _replicatedMap.Size());
        }

        [Test]
        public void Values()
        {
            FillValues();
            var values = _replicatedMap.Values();
            for (var i = 0; i < 10; i++)
            {
                Assert.IsTrue(values.Contains("value" + i));
            }
            Assert.AreEqual(10, values.Count);
        }

        void FillValues()
        {
            for (var i = 0; i < 10; i++)
            {
                _replicatedMap.Put(i, "value" + i);
            }
        }
    }
}