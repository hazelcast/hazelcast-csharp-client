// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading;
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
            mm = Client.GetMultiMap<object, object>(TestSupport.RandomString());
        }

        [TearDown]
        public static void Destroy()
        {
            mm.Destroy();
        }

        internal const string name = "ClientMultiMapTest";

        internal static IMultiMap<object, object> mm;

        [Test]
        public virtual void TestClear()
        {
            mm.Put("a", "b");
            mm.Put("a", "c");
            mm.Clear();
            Assert.AreEqual(0, mm.Size());
        }

        [Test]
        public virtual void TestContains()
        {
            Assert.IsTrue(mm.Put("key1", "value1"));
            Assert.IsTrue(mm.Put("key1", "value2"));
            Assert.IsTrue(mm.Put("key1", "value3"));
            Assert.IsTrue(mm.Put("key2", "value4"));
            Assert.IsTrue(mm.Put("key2", "value5"));
            Assert.IsFalse(mm.ContainsKey("key3"));
            Assert.IsTrue(mm.ContainsKey("key1"));
            Assert.IsFalse(mm.ContainsValue("value6"));
            Assert.IsTrue(mm.ContainsValue("value4"));
            Assert.IsFalse(mm.ContainsEntry("key1", "value4"));
            Assert.IsFalse(mm.ContainsEntry("key2", "value3"));
            Assert.IsTrue(mm.ContainsEntry("key1", "value1"));
            Assert.IsTrue(mm.ContainsEntry("key2", "value5"));
        }


        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestForceUnlock()
        {
            mm.Lock("key1");
            var latch = new CountdownEvent(1);

            var t = new Thread(delegate(object o)
            {
                mm.ForceUnlock("key1");
                latch.Signal();
            });
            t.Start();

            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(100)));
            Assert.IsFalse(mm.IsLocked("key1"));
        }

        [Test]
        public virtual void TestKeySetEntrySetAndValues()
        {
            Assert.IsTrue(mm.Put("key1", "value1"));
            Assert.IsTrue(mm.Put("key1", "value2"));
            Assert.IsTrue(mm.Put("key1", "value3"));
            Assert.IsTrue(mm.Put("key2", "value4"));
            Assert.IsTrue(mm.Put("key2", "value5"));
            Assert.AreEqual(2, mm.KeySet().Count);
            Assert.AreEqual(5, mm.Values().Count);
            Assert.AreEqual(5, mm.EntrySet().Count);
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestListener()
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

            mm.AddEntryListener(listener1, true);
            mm.AddEntryListener(listener2, "key3", true);

            mm.Put("key1", "value1");
            mm.Put("key1", "value2");
            mm.Put("key1", "value3");
            mm.Put("key2", "value4");
            mm.Put("key2", "value5");
            mm.Remove("key1", "value2");
            mm.Put("key3", "value6");
            mm.Put("key3", "value7");
            mm.Put("key3", "value8");
            mm.Remove("key3");
            Assert.IsTrue(latch1Add.Wait(TimeSpan.FromSeconds(20)));
            Assert.IsTrue(latch1Remove.Wait(TimeSpan.FromSeconds(20)));
            Assert.IsTrue(latch2Add.Wait(TimeSpan.FromSeconds(20)));
            Assert.IsTrue(latch2Remove.Wait(TimeSpan.FromSeconds(20)));
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestLock()
        {
            mm.Lock("key1");

            var latch = new CountdownEvent(1);
            var t = new Thread(delegate(object o)
            {
                if (!mm.TryLock("key1"))
                {
                    latch.Signal();
                }
            });
            t.Start();

            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(5)));
            mm.ForceUnlock("key1");
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestLockTtl()
        {
            mm.Lock("key1", 1, TimeUnit.Seconds);
            var latch = new CountdownEvent(2);
            var t = new Thread(delegate(object o)
            {
                if (!mm.TryLock("key1"))
                {
                    latch.Signal();
                }
                try
                {
                    if (mm.TryLock("key1", 2, TimeUnit.Seconds))
                    {
                        latch.Signal();
                    }
                }
                catch
                {
                }
            });
            t.Start();

            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(10)));
            mm.ForceUnlock("key1");
        }


        [Test]
        public virtual void TestPutGetRemove()
        {
            Assert.IsTrue(mm.Put("key1", "value1"));
            Assert.IsTrue(mm.Put("key1", "value2"));
            Assert.IsTrue(mm.Put("key1", "value3"));
            Assert.IsTrue(mm.Put("key2", "value4"));
            Assert.IsTrue(mm.Put("key2", "value5"));
            Assert.AreEqual(3, mm.ValueCount("key1"));
            Assert.AreEqual(2, mm.ValueCount("key2"));
            Assert.AreEqual(5, mm.Size());
            var coll = mm.Get("key1");
            Assert.AreEqual(3, coll.Count);
            coll = mm.Remove("key2");
            Assert.AreEqual(2, coll.Count);
            Assert.AreEqual(0, mm.ValueCount("key2"));
            Assert.AreEqual(0, mm.Get("key2").Count);
            Assert.IsFalse(mm.Remove("key1", "value4"));
            Assert.AreEqual(3, mm.Size());
            Assert.IsTrue(mm.Remove("key1", "value2"));
            Assert.AreEqual(2, mm.Size());
            Assert.IsTrue(mm.Remove("key1", "value1"));
            Assert.AreEqual(1, mm.Size());
            var enumerator = mm.Get("key1").GetEnumerator();
            enumerator.MoveNext();
            Assert.AreEqual("value3", enumerator.Current);
        }

        [Test]
        public virtual void TestRemoveListener()
        {
            var latch1Add = new CountdownEvent(1);
            var latch1Remove = new CountdownEvent(1);
            var listener1 = new EntryAdapter<object, object>(
                delegate { latch1Add.Signal(); },
                delegate { latch1Remove.Signal(); },
                delegate { },
                delegate { });

            var listenerId = mm.AddEntryListener(listener1, true);

            mm.Put("key1", "value1");
            Assert.IsTrue(latch1Add.Wait(TimeSpan.FromSeconds(10)));

            Assert.IsTrue(mm.RemoveEntryListener(listenerId));
            mm.Remove("key1");
            Assert.IsFalse(latch1Remove.Wait(TimeSpan.FromSeconds(10)));
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestTryLock()
        {
            Assert.IsTrue(mm.TryLock("key1", 200, TimeUnit.Milliseconds));
            var latch = new CountdownEvent(1);
            var t = new Thread(delegate(object o)
            {
                try
                {
                    if (!mm.TryLock("key1", 200, TimeUnit.Milliseconds))
                    {
                        latch.Signal();
                    }
                }
                catch
                {
                }
            });
            t.Start();


            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(10)));
            Assert.IsTrue(mm.IsLocked("key1"));


            var latch2 = new CountdownEvent(1);
            var t2 = new Thread(delegate(object o)
            {
                try
                {
                    if (mm.TryLock("key1", 20, TimeUnit.Seconds))
                    {
                        latch2.Signal();
                    }
                }
                catch
                {
                }
            });
            t2.Start();

            Thread.Sleep(100);
            mm.Unlock("key1");


            Assert.IsTrue(latch2.Wait(TimeSpan.FromSeconds(10)));
            Assert.IsTrue(mm.IsLocked("key1"));
            mm.ForceUnlock("key1");
        }
    }
}