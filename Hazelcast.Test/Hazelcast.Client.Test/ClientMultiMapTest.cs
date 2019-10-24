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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientMultiMapTest : SingleMemberBaseTest
    {
        [SetUp]
        public void Init()
        {
            _mm = Client.GetMultiMap<object, object>(TestSupport.RandomString());
        }

        [TearDown]
        public void Destroy()
        {
            _mm.Destroy();
        }

        private static IMultiMap<object, object> _mm;

        [Test]
        public void Clear()
        {
            _mm.Put("a", "b");
            _mm.Put("a", "c");
            _mm.Clear();
            Assert.AreEqual(0, _mm.Size());
        }

        [Test]
        public void Contains()
        {
            Assert.IsTrue(_mm.Put("key1", "value1"));
            Assert.IsTrue(_mm.Put("key1", "value2"));
            Assert.IsTrue(_mm.Put("key1", "value3"));
            Assert.IsTrue(_mm.Put("key2", "value4"));
            Assert.IsTrue(_mm.Put("key2", "value5"));
            Assert.IsFalse(_mm.ContainsKey("key3"));
            Assert.IsTrue(_mm.ContainsKey("key1"));
            Assert.IsFalse(_mm.ContainsValue("value6"));
            Assert.IsTrue(_mm.ContainsValue("value4"));
            Assert.IsFalse(_mm.ContainsEntry("key1", "value4"));
            Assert.IsFalse(_mm.ContainsEntry("key2", "value3"));
            Assert.IsTrue(_mm.ContainsEntry("key1", "value1"));
            Assert.IsTrue(_mm.ContainsEntry("key2", "value5"));
        }

        [Test]
        public void ForceUnlock()
        {
            _mm.Lock("key1");
            var latch = new CountdownEvent(1);

            TestSupport.Run(() =>
            {
                _mm.ForceUnlock("key1");
                latch.Signal();
            });

            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(100)));
            Assert.IsFalse(_mm.IsLocked("key1"));
        }

        [Test]
        public void KeySetEntrySetAndValues()
        {
            Assert.IsTrue(_mm.Put("key1", "value1"));
            Assert.IsTrue(_mm.Put("key1", "value2"));
            Assert.IsTrue(_mm.Put("key1", "value3"));
            Assert.IsTrue(_mm.Put("key2", "value4"));
            Assert.IsTrue(_mm.Put("key2", "value5"));
            Assert.AreEqual(2, _mm.KeySet().Count);
            Assert.AreEqual(5, _mm.Values().Count);
            Assert.AreEqual(5, _mm.EntrySet().Count);
        }

        [Test]
        public void Listener()
        {
            var latch1Add = new CountdownEvent(8);
            var latch1Remove = new CountdownEvent(4);
            var latch2Add = new CountdownEvent(3);
            var latch2Remove = new CountdownEvent(3);
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

            _mm.AddEntryListener(listener1, true);
            _mm.AddEntryListener(listener2, "key3", true);

            _mm.Put("key1", "value1");
            _mm.Put("key1", "value2");
            _mm.Put("key1", "value3");
            _mm.Put("key2", "value4");
            _mm.Put("key2", "value5");
            _mm.Remove("key1", "value2");
            _mm.Put("key3", "value6");
            _mm.Put("key3", "value7");
            _mm.Put("key3", "value8");
            _mm.Remove("key3");
            Assert.IsTrue(latch1Add.Wait(TimeSpan.FromSeconds(20)));
            Assert.IsTrue(latch1Remove.Wait(TimeSpan.FromSeconds(20)));
            Assert.IsTrue(latch2Add.Wait(TimeSpan.FromSeconds(20)));
            Assert.IsTrue(latch2Remove.Wait(TimeSpan.FromSeconds(20)));
        }

        [Test]
        public void Lock()
        {
            _mm.Lock("key1");

            var latch = new CountdownEvent(1);
            TestSupport.Run(() => {
                if (!_mm.TryLock("key1"))
                {
                    latch.Signal();
                }
            });

            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(5)));
            _mm.ForceUnlock("key1");
        }

        [Test]
        public void LockTtl()
        {
            _mm.Lock("key1", 1, TimeUnit.Seconds);
            var latch = new CountdownEvent(2);
            TestSupport.Run(() =>
            {
                if (!_mm.TryLock("key1"))
                {
                    latch.Signal();
                }
                try
                {
                    if (_mm.TryLock("key1", 2, TimeUnit.Seconds))
                    {
                        latch.Signal();
                    }
                }
                catch
                {
                }
            });

            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(10)));
            _mm.ForceUnlock("key1");
        }

        [Test]
        public void PutGetRemove()
        {
            Assert.IsTrue(_mm.Put("key1", "value1"));
            Assert.IsTrue(_mm.Put("key1", "value2"));
            Assert.IsTrue(_mm.Put("key1", "value3"));
            Assert.IsTrue(_mm.Put("key2", "value4"));
            Assert.IsTrue(_mm.Put("key2", "value5"));
            Assert.AreEqual(3, _mm.ValueCount("key1"));
            Assert.AreEqual(2, _mm.ValueCount("key2"));
            Assert.AreEqual(5, _mm.Size());
            var coll = _mm.Get("key1");
            Assert.AreEqual(3, coll.Count);
            coll = _mm.Remove("key2");
            Assert.AreEqual(2, coll.Count);
            Assert.AreEqual(0, _mm.ValueCount("key2"));
            Assert.AreEqual(0, _mm.Get("key2").Count);
            Assert.IsFalse(_mm.Remove("key1", "value4"));
            Assert.AreEqual(3, _mm.Size());
            Assert.IsTrue(_mm.Remove("key1", "value2"));
            Assert.AreEqual(2, _mm.Size());
            Assert.IsTrue(_mm.Remove("key1", "value1"));
            Assert.AreEqual(1, _mm.Size());

            var first = _mm.Get("key1").First();
            Assert.AreEqual("value3", first);
        }

        [Test]
        public void RemoveListener()
        {
            var latch1Add = new CountdownEvent(1);
            var latch1Remove = new CountdownEvent(1);
            var listener1 = new EntryAdapter<object, object>(
                delegate { latch1Add.Signal(); },
                delegate { latch1Remove.Signal(); },
                delegate { },
                delegate { });

            var listenerId = _mm.AddEntryListener(listener1, true);

            _mm.Put("key1", "value1");
            Assert.IsTrue(latch1Add.Wait(TimeSpan.FromSeconds(10)));

            Assert.IsTrue(_mm.RemoveEntryListener(listenerId));
            _mm.Remove("key1");
            Assert.IsFalse(latch1Remove.Wait(TimeSpan.FromSeconds(10)));
        }

        [Test]
        public void TryLock()
        {
            Assert.IsTrue(_mm.TryLock("key1", 200, TimeUnit.Milliseconds));
            var latch = new CountdownEvent(1);
            TestSupport.Run(() =>
            {
                try
                {
                    if (!_mm.TryLock("key1", 200, TimeUnit.Milliseconds))
                    {
                        latch.Signal();
                    }
                }
                catch
                {
                }
            });

            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(10)));
            Assert.IsTrue(_mm.IsLocked("key1"));

            var latch2 = new CountdownEvent(1);
            TestSupport.Run(() =>
            {
                try
                {
                    if (_mm.TryLock("key1", 20, TimeUnit.Seconds))
                    {
                        latch2.Signal();
                    }
                }
                catch
                {
                }
            });

            Thread.Sleep(100);
            _mm.Unlock("key1");

            Assert.IsTrue(latch2.Wait(TimeSpan.FromSeconds(10)));
            Assert.IsTrue(_mm.IsLocked("key1"));
            _mm.ForceUnlock("key1");
        }
    }
}