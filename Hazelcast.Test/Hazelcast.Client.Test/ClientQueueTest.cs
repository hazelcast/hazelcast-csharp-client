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
            q = Client.GetQueue<object>(queueName + TestSupport.RandomString());
        }

        [TearDown]
        public static void Destroy()
        {
            q.Clear();
            q.Destroy();
        }

        internal const string queueName = "ClientQueueTest";

        internal static IQueue<object> q;

        [Test]
        public virtual void TestAdd()
        {
            Assert.IsTrue(q.Add("item1"));
            Assert.IsTrue(q.Add("item2"));
            Assert.IsTrue(q.Add("item3"));
            Assert.IsTrue(q.Add("item4"));
            Assert.IsTrue(q.Add("item5"));
            Assert.AreEqual(5, q.Count);
        }

        /// <exception cref="System.IO.IOException"></exception>
        [Test]
        public virtual void TestAddAll()
        {
            var coll = new List<object>();
            coll.Add("item1");
            coll.Add("item2");
            coll.Add("item3");
            coll.Add("item4");
            Assert.IsTrue(q.AddAll(coll));
            var size = q.Count;
            Assert.AreEqual(size, coll.Count);
        }

        [Test]
        public virtual void TestClear()
        {
            Assert.IsTrue(q.Offer("item1"));
            Assert.IsTrue(q.Offer("item2"));
            Assert.IsTrue(q.Offer("item3"));
            Assert.IsTrue(q.Offer("item4"));
            Assert.IsTrue(q.Offer("item5"));
            q.Clear();
            Assert.AreEqual(0, q.Count);
            Assert.IsNull(q.Poll());
        }

        [Test]
        public virtual void TestContain()
        {
            Assert.IsTrue(q.Add("item1"));
            Assert.IsTrue(q.Add("item2"));
            Assert.IsTrue(q.Add("item3"));
            Assert.IsTrue(q.Add("item4"));
            Assert.IsTrue(q.Add("item5"));

            Assert.IsTrue(q.Contains("item3"));
        }

        [Test]
        public virtual void TestContains()
        {
            Assert.IsTrue(q.Offer("item1"));
            Assert.IsTrue(q.Offer("item2"));
            Assert.IsTrue(q.Offer("item3"));
            Assert.IsTrue(q.Offer("item4"));
            Assert.IsTrue(q.Offer("item5"));
            Assert.IsTrue(q.Contains("item3"));
            Assert.IsFalse(q.Contains("item"));
            var list = new List<string>(2);
            list.Add("item4");
            list.Add("item2");
            Assert.IsTrue(q.ContainsAll(list));
            list.Add("item");
            Assert.IsFalse(q.ContainsAll(list));
        }

        [Test]
        public virtual void TestCopyto()
        {
            Assert.IsTrue(q.Offer("item1"));
            Assert.IsTrue(q.Offer("item2"));
            Assert.IsTrue(q.Offer("item3"));
            Assert.IsTrue(q.Offer("item4"));
            Assert.IsTrue(q.Offer("item5"));

            var objects = new string[q.Count];
            q.CopyTo(objects, 0);

            Assert.AreEqual(objects.Length, q.Count);
        }

        [Test]
        public virtual void TestDrain()
        {
            Assert.IsTrue(q.Offer("item1"));
            Assert.IsTrue(q.Offer("item2"));
            Assert.IsTrue(q.Offer("item3"));
            Assert.IsTrue(q.Offer("item4"));
            Assert.IsTrue(q.Offer("item5"));
            var list = new List<string>();
            var result = q.DrainTo(list, 2);
            Assert.AreEqual(2, result);
            Assert.AreEqual("item1", list[0]);
            Assert.AreEqual("item2", list[1]);
            list = new List<string>();
            result = q.DrainTo(list);
            Assert.AreEqual(3, result);
            Assert.AreEqual("item3", list[0]);
            Assert.AreEqual("item4", list[1]);
            Assert.AreEqual("item5", list[2]);
        }

        [Test]
        public virtual void TestElement()
        {
            Assert.IsTrue(q.Offer("item1"));
            Assert.AreEqual("item1", q.Element());
        }

        [Test]
        public virtual void TestEnumaration()
        {
            Assert.IsTrue(q.Offer("item1"));
            var enumerator = q.GetEnumerator();
            enumerator.MoveNext();

            Assert.AreEqual("item1", enumerator.Current);
        }

        [Test]
        public virtual void TestIsEmpty()
        {
            Assert.IsTrue(q.IsEmpty());
        }

        [Test]
        public virtual void TestIsReadOnly()
        {
            Assert.IsFalse(q.IsReadOnly);
        }

        [Test]
        public virtual void TestIterator()
        {
            Assert.IsTrue(q.Offer("item1"));
            Assert.IsTrue(q.Offer("item2"));
            Assert.IsTrue(q.Offer("item3"));
            Assert.IsTrue(q.Offer("item4"));
            Assert.IsTrue(q.Offer("item5"));
            var i = 0;
            foreach (var o in q)
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
            Assert.AreEqual(0, q.Count);
            var latch = new CountdownEvent(5);
            var listener = new ClientListTest.Listener<object>(latch);
            var id = q.AddItemListener(listener, true);

            var t1 = new Thread(delegate(object o)
            {
                for (var i = 0; i < 5; i++)
                {
                    if (!q.Offer("event_item" + i))
                    {
                        throw new SystemException();
                    }
                }
            });
            t1.Start();

            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(5)));
            q.RemoveItemListener(id);
        }

        [Test]
        public virtual void TestListenerExtreme()
        {
            var qX = Client.GetQueue<object>(TestSupport.RandomString());

            const int TestItemCount = 1*100;

            Assert.AreEqual(0, qX.Count);

            for (var i = 0; i < TestItemCount; i++)
            {
                qX.Offer("ali");
            }

            Assert.AreEqual(TestItemCount, qX.Count);

            var latch = new CountdownEvent(TestItemCount*TestItemCount);
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
                var result = q.Offer("item");
                if (i < 6)
                {
                    Assert.IsTrue(result);
                }
                else
                {
                    Assert.IsFalse(result);
                }
            }
            Assert.AreEqual(6, q.Count);

            var t1 = new Thread(delegate(object o)
            {
                try
                {
                    Thread.Sleep(100);
                }
                catch
                {
                }
                q.Poll();
            });
            t1.Start();

            var result_1 = q.Offer("item", 200, TimeUnit.Milliseconds);
            Assert.IsTrue(result_1);
            for (var i_1 = 0; i_1 < 10; i_1++)
            {
                var o = q.Poll();
                if (i_1 < 6)
                {
                    Assert.IsNotNull(o);
                }
                else
                {
                    Assert.IsNull(o);
                }
            }
            Assert.AreEqual(0, q.Count);


            var t2 = new Thread(delegate(object o)
            {
                try
                {
                    Thread.Sleep(200);
                }
                catch
                {
                }
                q.Offer("item1");
            });
            t2.Start();

            var o_1 = q.Poll(300, TimeUnit.Milliseconds);
            Assert.AreEqual("item1", o_1);
            t1.Join(10000);
            t2.Join(10000);
        }

        [Test]
        public virtual void TestPeek()
        {
            Assert.IsTrue(q.Offer("item1"));
            Assert.AreEqual("item1", q.Peek());
            Assert.AreEqual(1, q.Count);
        }

        [Test]
        public virtual void TestPut()
        {
            q.Put("item1");
            Assert.AreEqual(1, q.Count);
        }


        /// <exception cref="System.IO.IOException"></exception>
        [Test]
        public virtual void TestRemainingCapacity()
        {
            Assert.AreEqual(6, q.RemainingCapacity());
            q.Offer("item");
            Assert.AreEqual(5, q.RemainingCapacity());
        }

        /// <exception cref="System.IO.IOException"></exception>
        [Test]
        public virtual void TestRemove()
        {
            Assert.IsTrue(q.Offer("item1"));
            Assert.IsTrue(q.Offer("item2"));
            Assert.IsTrue(q.Offer("item3"));
            Assert.IsFalse(q.Remove("item4"));
            Assert.AreEqual(3, q.Count);
            Assert.IsTrue(q.Remove("item2"));
            Assert.AreEqual(2, q.Count);
            Assert.AreEqual("item1", q.Poll());
            Assert.AreEqual("item3", q.Poll());

            Assert.IsFalse(q.Remove("itemX"));
            Assert.IsTrue(q.Offer("itemX"));
            Assert.IsTrue(((ICollection<object>) q).Remove("itemX"));

            Assert.IsTrue(q.Offer("itemY"));
            Assert.AreEqual("itemY", q.Remove());
        }

        [Test]
        public virtual void TestRemoveRetain()
        {
            Assert.IsTrue(q.Offer("item1"));
            Assert.IsTrue(q.Offer("item2"));
            Assert.IsTrue(q.Offer("item3"));
            Assert.IsTrue(q.Offer("item4"));
            Assert.IsTrue(q.Offer("item5"));
            var list = new List<string>();
            list.Add("item8");
            list.Add("item9");
            Assert.IsFalse(q.RemoveAll(list));
            Assert.AreEqual(5, q.Count);
            list.Add("item3");
            list.Add("item4");
            list.Add("item1");
            Assert.IsTrue(q.RemoveAll(list));
            Assert.AreEqual(2, q.Count);
            list.Clear();
            list.Add("item2");
            list.Add("item5");
            Assert.IsFalse(q.RetainAll(list));
            Assert.AreEqual(2, q.Count);
            list.Clear();
            Assert.IsTrue(q.RetainAll(list));
            Assert.AreEqual(0, q.Count);
        }


        [Test]
        public virtual void TestTake()
        {
            Assert.IsTrue(q.Offer("item1"));
            Assert.AreEqual("item1", q.Take());
        }

        [Test]
        public virtual void TestToArray()
        {
            Assert.IsTrue(q.Offer("item1"));
            Assert.IsTrue(q.Offer("item2"));
            Assert.IsTrue(q.Offer("item3"));
            Assert.IsTrue(q.Offer("item4"));
            Assert.IsTrue(q.Offer("item5"));
            var array = q.ToArray();
            var i = 0;
            foreach (var o in array)
            {
                i++;
                Assert.AreEqual("item" + i, o);
            }
            var objects = q.ToArray(new object[2]);
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
            var qc = (ICollection<object>) q;

            qc.Add("asd");
            Assert.IsTrue(qc.Contains("asd"));

            var enumerator = qc.GetEnumerator();

            enumerator.MoveNext();
            Assert.AreEqual("asd", enumerator.Current);

            var enuma = ((IEnumerable) qc).GetEnumerator();
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