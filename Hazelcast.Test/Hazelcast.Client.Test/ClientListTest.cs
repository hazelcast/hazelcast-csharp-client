/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientListTest : HazelcastBaseTest
    {
        [SetUp]
        public void Init()
        {
            list = Client.GetList<object>(TestSupport.RandomString());
        }

        [TearDown]
        public static void Destroy()
        {
            list.Destroy();
        }

        internal static IHList<object> list;

        [Test]
        public virtual void RemoveRetainAll()
        {
            Assert.IsTrue(list.Add("item1"));
            Assert.IsTrue(list.Add("item2"));
            Assert.IsTrue(list.Add("item1"));
            Assert.IsTrue(list.Add("item4"));
            var l = new List<object>();
            l.Add("item4");
            l.Add("item3");
            Assert.IsTrue(list.RemoveAll(l));
            Assert.AreEqual(3, list.Count);
            Assert.IsFalse(list.RemoveAll(l));
            Assert.AreEqual(3, list.Count);
            l.Clear();
            l.Add("item1");
            l.Add("item2");
            Assert.IsFalse(list.RetainAll(l));
            Assert.AreEqual(3, list.Count);
            l.Clear();
            Assert.IsTrue(list.RetainAll(l));
            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public virtual void TestAddAll()
        {
            IList<object> l = new List<object>();
            l.Add("item1");
            l.Add("item2");
            Assert.IsTrue(list.AddAll(l));

            Assert.AreEqual(2, list.Count);
            Assert.IsTrue(list.AddAll(1, l));
            Assert.AreEqual(4, list.Count);
            Assert.AreEqual("item1", list[0]);
            Assert.AreEqual("item1", list[1]);
            Assert.AreEqual("item2", list[2]);
            Assert.AreEqual("item2", list[3]);
        }

        [Test]
        public virtual void TestAddSetRemove()
        {
            Assert.IsTrue(list.Add("item1"));
            Assert.IsTrue(list.Add("item2"));
            list.Add(0, "item3");
            Assert.AreEqual(3, list.Count);
            var o = list.Set(2, "item4");
            Assert.AreEqual("item2", o);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual("item3", list[0]);
            Assert.AreEqual("item1", list[1]);
            Assert.AreEqual("item4", list[2]);
            Assert.IsFalse(list.Remove("item2"));
            Assert.IsTrue(list.Remove("item3"));
            o = list.Remove(1);
            Assert.AreEqual("item4", o);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("item1", list[0]);

            list[0] = "itemMod";
            Assert.AreEqual("itemMod", list.Get(0));
        }

        [Test]
        public virtual void TestContains()
        {
            Assert.IsTrue(list.Add("item1"));
            Assert.IsTrue(list.Add("item2"));
            Assert.IsTrue(list.Add("item1"));
            Assert.IsTrue(list.Add("item4"));
            Assert.IsFalse(list.Contains("item3"));
            Assert.IsTrue(list.Contains("item2"));
            var l = new List<object>();
            l.Add("item4");
            l.Add("item3");
            Assert.IsFalse(list.ContainsAll(l));
            Assert.IsTrue(list.Add("item3"));
            Assert.IsTrue(list.ContainsAll(l));
        }

        [Test]
        public virtual void TestIndexOf()
        {
            Assert.IsTrue(list.Add("item1"));
            Assert.IsTrue(list.Add("item2"));
            Assert.IsTrue(list.Add("item1"));
            Assert.IsTrue(list.Add("item4"));
            Assert.AreEqual(-1, list.IndexOf("item5"));
            Assert.AreEqual(0, list.IndexOf("item1"));
            Assert.AreEqual(-1, list.LastIndexOf("item6"));
            Assert.AreEqual(2, list.LastIndexOf("item1"));
        }

        [Test]
        public virtual void TestInsert()
        {
            list.Add("item0");
            list.Add("item1");
            list.Add("item2");
            list.Insert(1, "item1Mod");
            Assert.AreEqual("item1Mod", list[1]);
            list.RemoveAt(0);
            Assert.AreEqual("item1Mod", list[0]);
            Assert.AreEqual("item1", list[1]);
        }

        [Test]
        public void TestIsEmpty()
        {
            Assert.IsTrue(list.IsEmpty());
            list.Add("item1");
            Assert.IsFalse(list.IsEmpty());
            list.Clear();
            Assert.IsTrue(list.IsEmpty());
        }

        [Test]
        public virtual void TestIterator()
        {
            Assert.IsTrue(list.Add("item1"));
            Assert.IsTrue(list.Add("item2"));
            Assert.IsTrue(list.Add("item1"));
            Assert.IsTrue(list.Add("item4"));
            var iter = list.GetEnumerator();
            Assert.IsTrue(iter.MoveNext());
            Assert.AreEqual("item1", iter.Current);
            Assert.IsTrue(iter.MoveNext());
            Assert.AreEqual("item2", iter.Current);
            Assert.IsTrue(iter.MoveNext());
            Assert.AreEqual("item1", iter.Current);
            Assert.IsTrue(iter.MoveNext());
            Assert.AreEqual("item4", iter.Current);
            Assert.IsFalse(iter.MoveNext());

            var l = list.SubList(1, 3);
            Assert.AreEqual(2, l.Count);
            Assert.AreEqual("item2", l[0]);
            Assert.AreEqual("item1", l[1]);
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestListener()
        {
            //        final ISet tempSet = server.getSet(name);
            var tempList = list;

            var latch = new CountdownEvent(6);

            var listener = new Listener<object>(latch);
            var registrationId = tempList.AddItemListener(listener, true);

            var t = new Thread(delegate(object o)
            {
                for (var i = 0; i < 5; i++)
                {
                    tempList.Add("item" + i);
                }
                tempList.Add("done");
            });
            t.Start();
            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(20)));
        }

        [Test]
        public virtual void TestRemoveListener()
        {
            var latch = new CountdownEvent(1);

            var listener = new Listener<object>(latch);
            var registrationId = list.AddItemListener(listener, true);

            Assert.IsTrue(list.RemoveItemListener(registrationId));

            var t = new Thread(o => list.Add("item"));
            t.Start();

            Assert.IsFalse(latch.Wait(TimeSpan.FromSeconds(10)));
        }

        internal sealed class Listener<T> : IItemListener<T>
        {
            private readonly CountdownEvent latch;

            public Listener(CountdownEvent latch)
            {
                this.latch = latch;
            }

            public void ItemAdded(ItemEvent<T> item)
            {
                latch.Signal();
            }

            public void ItemRemoved(ItemEvent<T> item)
            {
            }
        }

    }
}