// Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.
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
    public class ClientSetTest : SingleMemberBaseTest
    {
        [SetUp]
        public void Init()
        {
            set = Client.GetSet<object>(TestSupport.RandomString());
        }

        [TearDown]
        public static void Destroy()
        {
            set.Clear();
        }

        //internal const string name = "test";

        internal static IHSet<object> set;

        [Test]
        public virtual void RemoveRetainAll()
        {
            Assert.IsTrue(set.Add("item1"));
            Assert.IsTrue(set.Add("item2"));
            Assert.IsTrue(set.Add("item3"));
            Assert.IsTrue(set.Add("item4"));
            var l = new List<object>();
            l.Add("item4");
            l.Add("item3");
            Assert.IsTrue(set.RemoveAll(l));
            Assert.AreEqual(2, set.Count);
            Assert.IsFalse(set.RemoveAll(l));
            Assert.AreEqual(2, set.Count);
            l.Clear();
            l.Add("item1");
            l.Add("item2");
            Assert.IsFalse(set.RetainAll(l));
            Assert.AreEqual(2, set.Count);
            l.Clear();
            Assert.IsTrue(set.RetainAll(l));
            Assert.AreEqual(0, set.Count);
        }

        [Test]
        public virtual void TestAddAll()
        {
            var l = new List<object>();
            l.Add("item1");
            l.Add("item2");
            Assert.IsTrue(set.AddAll(l));
            Assert.AreEqual(2, set.Count);
            Assert.IsFalse(set.AddAll(l));
            Assert.AreEqual(2, set.Count);
        }

        [Test]
        public virtual void TestAddRemove()
        {
            Assert.IsTrue(set.Add("item1"));
            Assert.IsTrue(set.Add("item2"));
            Assert.IsTrue(set.Add("item3"));
            Assert.AreEqual(3, set.Count);
            Assert.IsFalse(set.Add("item3"));
            Assert.AreEqual(3, set.Count);
            Assert.IsFalse(set.Remove("item4"));
            Assert.IsTrue(set.Remove("item3"));
        }

        [Test]
        public virtual void TestContains()
        {
            Assert.IsTrue(set.Add("item1"));
            Assert.IsTrue(set.Add("item2"));
            Assert.IsTrue(set.Add("item3"));
            Assert.IsTrue(set.Add("item4"));
            Assert.IsFalse(set.Contains("item5"));
            Assert.IsTrue(set.Contains("item2"));
            var l = new List<object>();
            l.Add("item6");
            l.Add("item3");
            Assert.IsFalse(set.ContainsAll(l));
            Assert.IsTrue(set.Add("item6"));
            Assert.IsTrue(set.ContainsAll(l));
        }

        [Test]
        public void TestIsEmpty()
        {
            Assert.IsTrue(set.IsEmpty());
            set.Add("item1");
            Assert.IsFalse(set.IsEmpty());
            set.Clear();
            Assert.IsTrue(set.IsEmpty());
        }

        [Test]
        public virtual void TestIterator()
        {
            Assert.IsTrue(set.Add("item1"));
            Assert.IsTrue(set.Add("item2"));
            Assert.IsTrue(set.Add("item3"));
            Assert.IsTrue(set.Add("item4"));
            IEnumerator iter = set.GetEnumerator();

            iter.MoveNext();
            Assert.IsTrue(((string) iter.Current).StartsWith("item"));
            iter.MoveNext();
            Assert.IsTrue(((string) iter.Current).StartsWith("item"));
            iter.MoveNext();
            Assert.IsTrue(((string) iter.Current).StartsWith("item"));
            iter.MoveNext();
            Assert.IsTrue(((string) iter.Current).StartsWith("item"));
            Assert.IsFalse(iter.MoveNext());
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestListener()
        {
            ////        final ISet tempSet = server.getSet(name);
            //ISet tempSet = set;
            //CountDownLatch latch = new CountDownLatch(6);
            //ItemListener listener = new _ItemListener_149(latch);
            //string registrationId = tempSet.AddListener(listener, true);
            //new _Thread_160(tempSet).Start();
            //Assert.IsTrue(latch.Await(20, TimeUnit.Seconds));

            var tempSet = set;

            var latch = new CountdownEvent(6);

            var listener = new ClientListTest.Listener<object>(latch);
            var registrationId = tempSet.AddItemListener(listener, true);

            var t = new Thread(delegate(object o)
            {
                for (var i = 0; i < 5; i++)
                {
                    tempSet.Add("item" + i);
                }
                tempSet.Add("done");
            });
            t.Start();
            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(20)));
        }

        [Test]
        public virtual void TestRemoveListener()
        {
            var tempSet = set;
            var latch = new CountdownEvent(1);

            var listener = new ClientListTest.Listener<object>(latch);
            var registrationId = tempSet.AddItemListener(listener, true);

            Assert.IsTrue(tempSet.RemoveItemListener(registrationId));

            var t = new Thread(o => tempSet.Add("item"));
            t.Start();

            Assert.IsFalse(latch.Wait(TimeSpan.FromSeconds(10)));
        }
    }
}