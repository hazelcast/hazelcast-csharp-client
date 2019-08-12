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
    public class ClientSetTest : SingleMemberBaseTest
    {
        [SetUp]
        public void Init()
        {
            _set = Client.GetSet<object>(TestSupport.RandomString());
        }

        [TearDown]
        public void Destroy()
        {
            _set.Clear();
        }

        IHSet<object> _set;
        static readonly string[] FourItems = { "item1", "item2", "item3", "item4" };

        void AddFourItems()
        {
            foreach (var item in FourItems)
            {
                Assert.IsTrue(_set.Add(item));
            }
        }

        [Test]
        public void RemoveRetainAll()
        {
            AddFourItems();

            var l = new[] { FourItems[3], FourItems[2] };

            Assert.IsTrue(_set.RemoveAll(l));
            Assert.AreEqual(2, _set.Count);
            Assert.IsFalse(_set.RemoveAll(l));
            Assert.AreEqual(2, _set.Count);

            l = new[] { FourItems[1], FourItems[0] };
            Assert.IsFalse(_set.RetainAll(l));
            Assert.AreEqual(2, _set.Count);

            Assert.IsTrue(_set.RetainAll(Enumerable.Empty<object>()));
            Assert.AreEqual(0, _set.Count);
        }

        [Test]
        public void AddAll()
        {
            var l = new List<object> { "item1", "item2" };

            Assert.IsTrue(_set.AddAll(l));
            Assert.AreEqual(2, _set.Count);
            Assert.IsFalse(_set.AddAll(l));
            Assert.AreEqual(2, _set.Count);
        }

        [Test]
        public void AddRemove()
        {
            Assert.IsTrue(_set.Add("item1"));
            Assert.IsTrue(_set.Add("item2"));
            Assert.IsTrue(_set.Add("item3"));
            Assert.AreEqual(3, _set.Count);
            Assert.IsFalse(_set.Add("item3"));
            Assert.AreEqual(3, _set.Count);
            Assert.IsFalse(_set.Remove("item4"));
            Assert.IsTrue(_set.Remove("item3"));
        }

        [Test]
        public void Contains()
        {
            AddFourItems();

            Assert.IsFalse(_set.Contains("item5"));
            Assert.IsTrue(_set.Contains("item2"));

            var l = new List<object> { "item6", "item3" };
            Assert.IsFalse(_set.ContainsAll(l));
            Assert.IsTrue(_set.Add("item6"));
            Assert.IsTrue(_set.ContainsAll(l));
        }

        [Test]
        public void IsEmpty()
        {
            Assert.IsTrue(_set.IsEmpty());
            _set.Add("item1");
            Assert.IsFalse(_set.IsEmpty());
            _set.Clear();
            Assert.IsTrue(_set.IsEmpty());
        }

        [Test]
        public void Enumeration()
        {
            AddFourItems();

            var count = 0;
            foreach (var item in _set)
            {
                Assert.That((string)item, Does.StartWith("item"));
                count++;
            }

            Assert.AreEqual(4, count);
        }

        [Test]
        public void Listener()
        {
            var tempSet = _set;

            var latch = new CountdownEvent(6);

            var listener = new ClientListTest.Listener<object>(latch);
            var registrationId = tempSet.AddItemListener(listener, true);

            var t = Task.Run(() =>
            {
                for (var i = 0; i < 5; i++)
                {
                    tempSet.Add("item" + i);
                }
                tempSet.Add("done");
            });
            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(20)));
        }

        [Test]
        public void RemoveListener()
        {
            var tempSet = _set;
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