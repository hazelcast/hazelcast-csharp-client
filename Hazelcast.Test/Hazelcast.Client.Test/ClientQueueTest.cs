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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientQueueTest : SingleMemberBaseTest
    {
        static readonly IEnumerable<object> FiveItems = new[] { "item1", "item2", "item3", "item4", "item5" };

        void AddFiveItems()
        {
            foreach (var item in FiveItems)
            {
                Assert.IsTrue(_q.Add(item));
            }
        }

        [SetUp]
        public void Init()
        {
            //CAUTION TEST SERVER SHOULD CONFIGURE A QUEUE WITH NAME starts with queueName
            //which is configured as;
            //QueueConfig queueConfig = config.getQueueConfig(queueName);
            //queueConfig.setMaxSize(6);
            //
            _q = Client.GetQueue<object>(QueueName + TestSupport.RandomString());
        }

        [TearDown]
        public void Destroy()
        {
            _q.Clear();
            _q.Destroy();
        }

        const string QueueName = "ClientQueueTest";

        IQueue<object> _q;

        [Test]
        public void Add()
        {
            AddFiveItems();

            Assert.AreEqual(5, _q.Count);
        }

        [Test]
        public void AddAll()
        {
            Assert.IsTrue(_q.AddAll(FiveItems));
            var size = _q.Count;
            Assert.AreEqual(FiveItems.Count(), size);
        }

        [Test]
        public void Contains()
        {
            AddFiveItems();

            Assert.IsTrue(_q.Contains("item3"));
        }

        [Test]
        public void ContainsAll()
        {
            AddFiveItems();

            Assert.IsTrue(_q.Contains("item3"));
            Assert.IsFalse(_q.Contains("item"));
            var list = new List<string>() { "item4", "item2" };
            Assert.IsTrue(_q.ContainsAll(list));

            list.Add("item");
            Assert.IsFalse(_q.ContainsAll(list));
        }

        [Test]
        public void CopyTo()
        {
            AddFiveItems();

            var objects = new string[_q.Count];
            _q.CopyTo(objects, 0);

            Assert.AreEqual(objects.Length, _q.Count);
        }

        [Test]
        public void IsEmpty()
        {
            Assert.IsTrue(_q.IsEmpty());
        }

        [Test]
        public void IsReadOnly()
        {
            Assert.IsFalse(_q.IsReadOnly);
        }

        [Test]
        public void Enumeration()
        {
            AddFiveItems();

            var actual = Enumerable.ToArray(_q);
            CollectionAssert.AreEqual(FiveItems, actual);
        }

        [Test]
        public void Listener()
        {
            Assert.AreEqual(0, _q.Count);
            var latch = new CountdownEvent(5);
            var listener = new ClientListTest.Listener<object>(latch);
            var id = _q.AddItemListener(listener, true);

            var t = Task.Run(() =>
            {
                for (var i = 0; i < 5; i++)
                {
                    if (!_q.Add("event_item" + i))
                    {
                        throw new SystemException();
                    }
                }
            });

            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(5)));
            _q.RemoveItemListener(id);
        }

        [Test]
        public void RemoveRetain()
        {
            AddFiveItems();

            var list = new List<string> { "item8", "item9" };

            Assert.IsFalse(_q.RemoveAll(list));
            Assert.AreEqual(5, _q.Count);
            list.Add("item3");
            list.Add("item4");
            list.Add("item1");
            Assert.IsTrue(_q.RemoveAll(list));
            Assert.AreEqual(2, _q.Count);
            list.Clear();
            list.Add("item2");
            list.Add("item5");
            Assert.IsFalse(_q.RetainAll(list));
            Assert.AreEqual(2, _q.Count);
            list.Clear();
            Assert.IsTrue(_q.RetainAll(list));
            Assert.AreEqual(0, _q.Count);
        }

        [Test]
        public void ToArray()
        {
            AddFiveItems();

            var array = _q.ToArray();
            CollectionAssert.AreEqual(FiveItems, array);

            var objects = _q.ToArray(new object[2]);
            CollectionAssert.AreEqual(FiveItems, objects);
        }

        [Test]
        public void WrapperMethods()
        {
            var qc = (ICollection<object>)_q;

            qc.Add("asd");
            Assert.IsTrue(qc.Contains("asd"));

            var value = qc.First();
            Assert.AreEqual("asd", value);

            var enuma = ((IEnumerable)qc).GetEnumerator();
            enuma.MoveNext();
            Assert.AreEqual("asd", enuma.Current);
        }

        [Test]
        public void Clear()
        {
            foreach (var item in FiveItems)
            {
                Assert.IsTrue(_q.Offer(item));
            }

            _q.Clear();
            Assert.AreEqual(0, _q.Count);
            Assert.IsNull(_q.Poll());
        }

        [Test]
        public void Drain()
        {
            AddFiveItems();

            var list = new List<string>();
            var result = _q.DrainTo(list, 2);
            Assert.AreEqual(2, result);
            Assert.AreEqual("item1", list[0]);
            Assert.AreEqual("item2", list[1]);

            list = new List<string>();
            result = _q.DrainTo(list);
            Assert.AreEqual(3, result);
            Assert.AreEqual("item3", list[0]);
            Assert.AreEqual("item4", list[1]);
            Assert.AreEqual("item5", list[2]);
        }

        [Test]
        public void Element()
        {
            Assert.IsTrue(_q.Offer("item1"));
            Assert.AreEqual("item1", _q.Element());
        }

        [Test]
        public void ListenerExtreme()
        {
            var qX = Client.GetQueue<object>(TestSupport.RandomString());

            const int testItemCount = 1 * 100;

            Assert.AreEqual(0, qX.Count);

            for (var i = 0; i < testItemCount; i++)
            {
                qX.Offer("ali");
            }

            Assert.AreEqual(testItemCount, qX.Count);

            var latch = new CountdownEvent(testItemCount * testItemCount);
            for (var j = 0; j < testItemCount; j++)
            {
                var listener = new ItemListener<object>(null, latch);

                var id = qX.AddItemListener(listener, true);
                Assert.NotNull(id);
            }

            qX.Clear();

            Assert.AreEqual(0, qX.Count);

            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(100)));
        }

        [Test]
        public void OfferPoll()
        {
            for (var i = 0; i < 10; i++)
            {
                var result = _q.Offer("item");
                if (i < 6)
                {
                    Assert.IsTrue(result);
                }
                else
                {
                    Assert.IsFalse(result);
                }
            }

            Assert.AreEqual(6, _q.Count);

            var t1 = Task.Run(async () =>
            {
                await Task.Delay(100);

                _q.Poll();
            });

            var result1 = _q.Offer("item", 200, TimeUnit.Milliseconds);
            Assert.IsTrue(result1);
            for (var i_1 = 0; i_1 < 10; i_1++)
            {
                var o = _q.Poll();
                if (i_1 < 6)
                {
                    Assert.IsNotNull(o);
                }
                else
                {
                    Assert.IsNull(o);
                }
            }
            Assert.AreEqual(0, _q.Count);

            var t2 = Task.Run(async () =>
            {
                await Task.Delay(200);
                _q.Offer("item1");
            });

            var o_1 = _q.Poll(300, TimeUnit.Milliseconds);
            Assert.AreEqual("item1", o_1);

            var delay = Task.Delay(10000);
            Task.WhenAny(delay, t1, t2).GetAwaiter().GetResult();
        }

        [Test]
        public void Peek()
        {
            Assert.IsTrue(_q.Offer("item1"));
            Assert.AreEqual("item1", _q.Peek());
            Assert.AreEqual(1, _q.Count);
        }

        [Test]
        public void Put()
        {
            _q.Put("item1");
            Assert.AreEqual(1, _q.Count);
        }

        [Test]
        public void RemainingCapacity()
        {
            Assert.AreEqual(6, _q.RemainingCapacity());
            _q.Offer("item");
            Assert.AreEqual(5, _q.RemainingCapacity());
        }

        [Test]
        public void Remove()
        {
            Assert.IsTrue(_q.Offer("item1"));
            Assert.IsTrue(_q.Offer("item2"));
            Assert.IsTrue(_q.Offer("item3"));
            Assert.IsFalse(_q.Remove("item4"));
            Assert.AreEqual(3, _q.Count);
            Assert.IsTrue(_q.Remove("item2"));
            Assert.AreEqual(2, _q.Count);
            Assert.AreEqual("item1", _q.Poll());
            Assert.AreEqual("item3", _q.Poll());

            Assert.IsFalse(_q.Remove("itemX"));
            Assert.IsTrue(_q.Offer("itemX"));
            Assert.IsTrue(((ICollection<object>)_q).Remove("itemX"));

            Assert.IsTrue(_q.Offer("itemY"));
            Assert.AreEqual("itemY", _q.Remove());
        }

        [Test]
        public void Take()
        {
            Assert.IsTrue(_q.Offer("item1"));
            Assert.AreEqual("item1", _q.Take());
        }

        class ItemListener<T> : IItemListener<T>
        {
            readonly CountdownEvent _latchAdd;
            readonly CountdownEvent _latchRemove;

            public ItemListener(CountdownEvent latchAdd, CountdownEvent latchRemove)
            {
                _latchAdd = latchAdd;
                _latchRemove = latchRemove;
            }

            public void ItemAdded(ItemEvent<T> item)
            {
                _latchAdd?.Signal();
            }

            public void ItemRemoved(ItemEvent<T> item)
            {
                _latchRemove?.Signal();
            }
        }
    }
}