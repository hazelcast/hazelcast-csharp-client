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
using System.Threading.Tasks;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientListTest : SingleMemberBaseTest
    {
        [SetUp]
        public void Init()
        {
            _list = Client.GetList<object>(TestSupport.RandomString());
        }

        [TearDown]
        public void Destroy()
        {
            _list.Destroy();
        }

        private IHList<object> _list;

        private static readonly string[] FourItems = { "item1", "item2", "item1", "item4", };

        private void AddFourItems()
        {
            foreach (var item in FourItems)
            {
                Assert.IsTrue(_list.Add(item));
            }
        }

        internal sealed class Listener<T> : IItemListener<T>
        {
            readonly CountdownEvent _latch;

            public Listener(CountdownEvent latch)
            {
                _latch = latch;
            }

            public void ItemAdded(ItemEvent<T> item)
            {
                _latch.Signal();
            }

            public void ItemRemoved(ItemEvent<T> item)
            {
            }
        }

        [Test]
        public void RemoveRetainAll()
        {
            AddFourItems();

            var l = new List<object> { "item4", "item3" };

            Assert.IsTrue(_list.RemoveAll(l));
            Assert.AreEqual(3, _list.Count);
            Assert.IsFalse(_list.RemoveAll(l));
            Assert.AreEqual(3, _list.Count);
            l.Clear();
            l.Add("item1");
            l.Add("item2");
            Assert.IsFalse(_list.RetainAll(l));
            Assert.AreEqual(3, _list.Count);
            l.Clear();
            Assert.IsTrue(_list.RetainAll(l));
            Assert.AreEqual(0, _list.Count);
        }

        [Test]
        public void AddAll()
        {
            var l = new List<object> { "item1", "item2" };
            Assert.IsTrue(_list.AddAll(l));

            Assert.AreEqual(2, _list.Count);
            Assert.IsTrue(_list.AddAll(1, l));
            Assert.AreEqual(4, _list.Count);
            Assert.AreEqual("item1", _list[0]);
            Assert.AreEqual("item1", _list[1]);
            Assert.AreEqual("item2", _list[2]);
            Assert.AreEqual("item2", _list[3]);
        }

        [Test]
        public void AddSetRemove()
        {
            Assert.IsTrue(_list.Add("item1"));
            Assert.IsTrue(_list.Add("item2"));
            _list.Add(0, "item3");
            Assert.AreEqual(3, _list.Count);
            var o = _list.Set(2, "item4");
            Assert.AreEqual("item2", o);
            Assert.AreEqual(3, _list.Count);
            Assert.AreEqual("item3", _list[0]);
            Assert.AreEqual("item1", _list[1]);
            Assert.AreEqual("item4", _list[2]);
            Assert.IsFalse(_list.Remove("item2"));
            Assert.IsTrue(_list.Remove("item3"));
            o = _list.Remove(1);
            Assert.AreEqual("item4", o);
            Assert.AreEqual(1, _list.Count);
            Assert.AreEqual("item1", _list[0]);

            _list[0] = "itemMod";
            Assert.AreEqual("itemMod", _list.Get(0));
        }

        [Test]
        public void Contains()
        {
            Assert.IsTrue(_list.Add("item1"));
            Assert.IsTrue(_list.Add("item2"));
            Assert.IsTrue(_list.Add("item1"));
            Assert.IsTrue(_list.Add("item4"));
            Assert.IsFalse(_list.Contains("item3"));
            Assert.IsTrue(_list.Contains("item2"));

            var l = new List<object> { "item4", "item3" };

            Assert.IsFalse(_list.ContainsAll(l));
            Assert.IsTrue(_list.Add("item3"));
            Assert.IsTrue(_list.ContainsAll(l));
        }

        [Test]
        public void IndexOf()
        {
            Assert.IsTrue(_list.Add("item1"));
            Assert.IsTrue(_list.Add("item2"));
            Assert.IsTrue(_list.Add("item1"));
            Assert.IsTrue(_list.Add("item4"));
            Assert.AreEqual(-1, _list.IndexOf("item5"));
            Assert.AreEqual(0, _list.IndexOf("item1"));
            Assert.AreEqual(-1, _list.LastIndexOf("item6"));
            Assert.AreEqual(2, _list.LastIndexOf("item1"));
        }

        [Test]
        public void Insert()
        {
            _list.Add("item0");
            _list.Add("item1");
            _list.Add("item2");
            _list.Insert(1, "item1Mod");
            Assert.AreEqual("item1Mod", _list[1]);
            _list.RemoveAt(0);
            Assert.AreEqual("item1Mod", _list[0]);
            Assert.AreEqual("item1", _list[1]);
        }

        [Test]
        public void IsEmpty()
        {
            Assert.IsTrue(_list.IsEmpty());
            _list.Add("item1");
            Assert.IsFalse(_list.IsEmpty());
            _list.Clear();
            Assert.IsTrue(_list.IsEmpty());
        }

        [Test]
        public void Enumerator()
        {
            AddFourItems();

            var i = 0;
            foreach (var item in _list)
            {
                Assert.AreEqual(FourItems[i++], item);
            }

            var l = _list.SubList(1, 3);
            Assert.AreEqual(2, l.Count);
            Assert.AreEqual("item2", l[0]);
            Assert.AreEqual("item1", l[1]);
        }

        [Test]
        public void AddListener()
        {
            var tempList = _list;

            var latch = new CountdownEvent(6);

            var listener = new Listener<object>(latch);
            var registrationId = tempList.AddItemListener(listener, true);

            var t = Task.Run(() =>
            {
                for (var i = 0; i < 5; i++)
                {
                    tempList.Add("item" + i);
                }

                tempList.Add("done");
            });

            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(20)));
        }

        [Test]
        public void RemoveListener()
        {
            var latch = new CountdownEvent(1);

            var listener = new Listener<object>(latch);
            var registrationId = _list.AddItemListener(listener, true);

            Assert.IsTrue(_list.RemoveItemListener(registrationId));

            var t = new Thread(o => _list.Add("item"));
            t.Start();

            Assert.IsFalse(latch.Wait(TimeSpan.FromSeconds(10)));
        }
    }
}