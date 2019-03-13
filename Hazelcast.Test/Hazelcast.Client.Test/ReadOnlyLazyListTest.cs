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
using Hazelcast.IO.Serialization;
using Hazelcast.Util;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ReadOnlyLazyListTest
    {
        private ReadOnlyLazyList<int> testList;
        private ISerializationService _ss;

        [SetUp]
        public void Init()
        {
            _ss = new SerializationServiceBuilder().Build();
            var dataList = new List<IData> {_ss.ToData(0), _ss.ToData(1), _ss.ToData(2), _ss.ToData(3), _ss.ToData(4)};
            testList = new ReadOnlyLazyList<int>(dataList, _ss);
        }

        [TearDown]
        public void Destroy()
        {
            _ss.Destroy();
        }

        [Test]
        public virtual void TestListGet()
        {
            for (var i = 0; i < 5; i++)
            {
                Assert.AreEqual(testList[i], i);
            }
        }

        [Test]
        public virtual void TestListCount()
        {
            Assert.AreEqual(testList.Count, 5);
        }

        [Test]
        public virtual void TestListIsReadOnly()
        {
            Assert.True(testList.IsReadOnly);
        }

        [Test]
        public virtual void TestListContains()
        {
            Assert.True(testList.Contains(1));
        }

        [Test]
        public virtual void TestListCopyTo()
        {
            var copyArray = new int[testList.Count + 5];
            testList.CopyTo(copyArray, 1);
            for (var i = 0; i < 5; i++)
            {
                Assert.AreEqual(copyArray[i + 1], testList[i]);
            }
        }

        [Test]
        public virtual void TestListIndexOf()
        {
            Assert.AreEqual(testList.IndexOf(4), 4);
        }

        [Test]
        public virtual void TestListEnum()
        {
            var enumerator = testList.GetEnumerator();
            var ix = 0;
            while (enumerator.MoveNext())
            {
                Assert.AreEqual(enumerator.Current, ix);
                ix++;
            }
        }

        [Test]
        public void TestListAdd()
        {
            Assert.Throws<NotSupportedException>(() => { testList.Add(4); });
        }

        [Test]
        public void TestListRemove()
        {
            Assert.Throws<NotSupportedException>(() => { testList.Remove(4); });
        }

        [Test]
        public void TestListRemoveAt()
        {
            Assert.Throws<NotSupportedException>(() => { testList.RemoveAt(4); });
        }

        [Test]
        public void TestListClear()
        {
            Assert.Throws<NotSupportedException>(() => { testList.Clear(); });
        }

        [Test]
        public void TestListInsert()
        {
            Assert.Throws<NotSupportedException>(() => { testList.Insert(1, 1); });
        }
    }
}