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
using System.Threading;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientQueueTest : SingleMemberBaseTest
    {
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

        static readonly IEnumerable<object> FiveItems = new []{"item1", "item2", "item3", "item4", "item5"};

        IQueue<object> _q;

        [Test]
        public void TestAdd()
        {
            foreach (var item in FiveItems)
            {
                Assert.IsTrue(_q.Add(item));
            }
            
            Assert.AreEqual(5, _q.Count);
        }

        [Test]
        public void TestAddAll()
        {
            var coll = new List<object> { "item1", "item2", "item3", "item4" };

            Assert.IsTrue(_q.AddAll(coll));
            var size = _q.Count;
            Assert.AreEqual(size, coll.Count);
        }

        [Test]
        public void TestClear()
        {
            Assert.IsTrue(_q.Offer("item1"));
            Assert.IsTrue(_q.Offer("item2"));
            Assert.IsTrue(_q.Offer("item3"));
            Assert.IsTrue(_q.Offer("item4"));
            Assert.IsTrue(_q.Offer("item5"));

            _q.Clear();
            Assert.AreEqual(0, _q.Count);
            Assert.IsNull(_q.Poll());
        }

        [Test]
        public void TestContain()
        {
            Assert.IsTrue(_q.Add("item1"));
            Assert.IsTrue(_q.Add("item2"));
            Assert.IsTrue(_q.Add("item3"));
            Assert.IsTrue(_q.Add("item4"));
            Assert.IsTrue(_q.Add("item5"));

            Assert.IsTrue(_q.Contains("item3"));
        }

        [Test]
        public void TestContains()
        {
            Assert.IsTrue(_q.Offer("item1"));
            Assert.IsTrue(_q.Offer("item2"));
            Assert.IsTrue(_q.Offer("item3"));
            Assert.IsTrue(_q.Offer("item4"));
            Assert.IsTrue(_q.Offer("item5"));
            Assert.IsTrue(_q.Contains("item3"));
            Assert.IsFalse(_q.Contains("item"));
            var list = new List<string>(2);
            list.Add("item4");
            list.Add("item2");
            Assert.IsTrue(_q.ContainsAll(list));
            list.Add("item");
            Assert.IsFalse(_q.ContainsAll(list));
        }

        [Test]
        public virtual void TestCopyto()
        {
            Assert.IsTrue(_q.Offer("item1"));
            Assert.IsTrue(_q.Offer("item2"));
            Assert.IsTrue(_q.Offer("item3"));
            Assert.IsTrue(_q.Offer("item4"));
            Assert.IsTrue(_q.Offer("item5"));

            var objects = new string[_q.Count];
            _q.CopyTo(objects, 0);

            Assert.AreEqual(objects.Length, _q.Count);
        }

        [Test]
        public virtual void TestDrain()
        {
            Assert.IsTrue(_q.Offer("item1"));
            Assert.IsTrue(_q.Offer("item2"));
            Assert.IsTrue(_q.Offer("item3"));
            Assert.IsTrue(_q.Offer("item4"));
            Assert.IsTrue(_q.Offer("item5"));
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
        public virtual void TestElement()
        {
            Assert.IsTrue(_q.Offer("item1"));
            Assert.AreEqual("item1", _q.Element());
        }

        [Test]
        public virtual void TestEnumaration()
        {
            Assert.IsTrue(_q.Offer("item1"));
            var enumerator = _q.GetEnumerator();
            enumerator.MoveNext();

            Assert.AreEqual("item1", enumerator.Current);
        }

        [Test]
        public virtual void TestIsEmpty()
        {
            Assert.IsTrue(_q.IsEmpty());
        }

        [Test]
        public virtual void TestIsReadOnly()
        {
            Assert.IsFalse(_q.IsReadOnly);
        }

        [Test]
        public virtual void TestIterator()
        {
            Assert.IsTrue(_q.Offer("item1"));
            Assert.IsTrue(_q.Offer("item2"));
            Assert.IsTrue(_q.Offer("item3"));
            Assert.IsTrue(_q.Offer("item4"));
            Assert.IsTrue(_q.Offer("item5"));
            var i = 0;
            foreach (var o in _q)
            {
                i++;
                Assert.AreEqual("item" + i, o);
            }
        }

        //    @BeforeClass
        //    public static void init(){
        //        Config config = new Config();
        //        QueueConfig queueConfig = config.getQueueConfig(queueName);
        //        queueConfig.setMaxSize(6);
        //        server = Hazelcast.newHazelcastInstance(config);
        //        hz = HazelcastClient.newHazelcastClient(null);
        //        q = hz.getQueue(queueName);
        //    }
        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestListener()
        {
            Assert.AreEqual(0, _q.Count);
            var latch = new CountdownEvent(5);
            var listener = new ClientListTest.Listener<object>(latch);
            var id = _q.AddItemListener(listener, true);

            var t1 = new Thread(delegate (object o)
            {
                for (var i = 0; i < 5; i++)
                {
                    if (!_q.Offer("event_item" + i))
                    {
                        throw new SystemException();
                    }
                }
            });
            t1.Start();

            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(5)));
            _q.RemoveItemListener(id);
        }

        [Test]
        public virtual void TestListenerExtreme()
        {
            var qX = Client.GetQueue<object>(TestSupport.RandomString());

            const int TestItemCount = 1 * 100;

            Assert.AreEqual(0, qX.Count);

            for (var i = 0; i < TestItemCount; i++)
            {
                qX.Offer("ali");
            }

            Assert.AreEqual(TestItemCount, qX.Count);

            var latch = new CountdownEvent(TestItemCount * TestItemCount);
            for (var j = 0; j < TestItemCount; j++)
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
        public virtual void TestOfferPoll()
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

            var t1 = new Thread(delegate (object o)
            {
                try
                {
                    Thread.Sleep(100);
                }
                catch
                {
                }
                _q.Poll();
            });
            t1.Start();

            var result_1 = _q.Offer("item", 200, TimeUnit.Milliseconds);
            Assert.IsTrue(result_1);
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


            var t2 = new Thread(delegate (object o)
            {
                try
                {
                    Thread.Sleep(200);
                }
                catch
                {
                }
                _q.Offer("item1");
            });
            t2.Start();

            var o_1 = _q.Poll(300, TimeUnit.Milliseconds);
            Assert.AreEqual("item1", o_1);
            t1.Join(10000);
            t2.Join(10000);
        }

        [Test]
        public virtual void TestPeek()
        {
            Assert.IsTrue(_q.Offer("item1"));
            Assert.AreEqual("item1", _q.Peek());
            Assert.AreEqual(1, _q.Count);
        }

        [Test]
        public virtual void TestPut()
        {
            _q.Put("item1");
            Assert.AreEqual(1, _q.Count);
        }


        /// <exception cref="System.IO.IOException"></exception>
        [Test]
        public virtual void TestRemainingCapacity()
        {
            Assert.AreEqual(6, _q.RemainingCapacity());
            _q.Offer("item");
            Assert.AreEqual(5, _q.RemainingCapacity());
        }

        /// <exception cref="System.IO.IOException"></exception>
        [Test]
        public virtual void TestRemove()
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
        public virtual void TestRemoveRetain()
        {
            Assert.IsTrue(_q.Offer("item1"));
            Assert.IsTrue(_q.Offer("item2"));
            Assert.IsTrue(_q.Offer("item3"));
            Assert.IsTrue(_q.Offer("item4"));
            Assert.IsTrue(_q.Offer("item5"));
            var list = new List<string>();
            list.Add("item8");
            list.Add("item9");
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
        public virtual void TestTake()
        {
            Assert.IsTrue(_q.Offer("item1"));
            Assert.AreEqual("item1", _q.Take());
        }

        [Test]
        public virtual void TestToArray()
        {
            Assert.IsTrue(_q.Offer("item1"));
            Assert.IsTrue(_q.Offer("item2"));
            Assert.IsTrue(_q.Offer("item3"));
            Assert.IsTrue(_q.Offer("item4"));
            Assert.IsTrue(_q.Offer("item5"));
            var array = _q.ToArray();
            var i = 0;
            foreach (var o in array)
            {
                i++;
                Assert.AreEqual("item" + i, o);
            }
            var objects = _q.ToArray(new object[2]);
            i = 0;
            foreach (var o_1 in objects)
            {
                i++;
                Assert.AreEqual("item" + i, o_1);
            }
        }

        [Test]
        public void TestWrapperMethods()
        {
            var qc = (ICollection<object>)_q;

            qc.Add("asd");
            Assert.IsTrue(qc.Contains("asd"));

            var enumerator = qc.GetEnumerator();

            enumerator.MoveNext();
            Assert.AreEqual("asd", enumerator.Current);

            var enuma = ((IEnumerable)qc).GetEnumerator();
            enuma.MoveNext();
            Assert.AreEqual("asd", enuma.Current);
        }
    }

    internal class ItemListener<T> : IItemListener<T>
    {
        private readonly CountdownEvent latchAdd;
        private readonly CountdownEvent latchRemove;

        public ItemListener(CountdownEvent latchAdd, CountdownEvent latchRemove)
        {
            this.latchAdd = latchAdd;
            this.latchRemove = latchRemove;
        }

        public void ItemAdded(ItemEvent<T> item)
        {
            if (latchAdd != null) latchAdd.Signal();
        }

        public void ItemRemoved(ItemEvent<T> item)
        {
            if (latchRemove != null) latchRemove.Signal();
        }
    }
}