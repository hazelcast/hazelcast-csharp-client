// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
    public class ReadOnlyLazySetTest
    {
        private ReadOnlyLazySet<int> testSet;
        private ISerializationService _ss;

        [SetUp]
        public void Init()
        {
            _ss = new SerializationServiceBuilder().Build();
            var dataList = new List<IData>
            {
                _ss.ToData(0),
                _ss.ToData(1),
                _ss.ToData(2),
                _ss.ToData(3),
                _ss.ToData(4)
            };
            testSet = new ReadOnlyLazySet<int>(dataList, _ss);
        }

        [TearDown]
        public void Destroy()
        {
            _ss.Destroy();
        }

        [Test]
        public void TestCount()
        {
            Assert.AreEqual(testSet.Count, 5);
        }

        [Test]
        public void TestIsReadOnly()
        {
            Assert.True(testSet.IsReadOnly);
        }

        [Test]
        public void TestContains()
        {
            Assert.True(testSet.Contains(1));
        }

        [Test]
        public void TestCopyTo()
        {
            var copyArray = new int[testSet.Count + 5];
            testSet.CopyTo(copyArray, 1);
            for (var i = 0; i < 5; i++)
            {
                Assert.True(testSet.Contains(copyArray[i + 1]));
            }
        }

        [Test]
        public void TestEnumerator()
        {
            var enumerator = testSet.GetEnumerator();
            var ix = 0;
            while (enumerator.MoveNext())
            {
                Assert.AreEqual(enumerator.Current, ix);
                ix++;
            }
        }

        [Test]
        public void TestAdd()
        {
            Assert.Throws<NotSupportedException>(() => { testSet.Add(4); });
        }

        [Test]
        public void TestRemove()
        {
            Assert.Throws<NotSupportedException>(() => { testSet.Remove(4); });
        }

        [Test]
        public void TestClear()
        {
            Assert.Throws<NotSupportedException>(() => { testSet.Clear(); });
        }

        [Test]
        public void TestExceptWith()
        {
            Assert.Throws<NotSupportedException>(() => { testSet.ExceptWith(null); });
        }

        [Test]
        public void TestIntersectWith()
        {
            Assert.Throws<NotSupportedException>(() => { testSet.IntersectWith(null); });
        }

        [Test]
        public void TestIsProperSubsetOf()
        {
            Assert.Throws<NotSupportedException>(() => { testSet.IsProperSubsetOf(null); });
        }

        [Test]
        public void TestIsProperSupersetOf()
        {
            Assert.Throws<NotSupportedException>(() => { testSet.IsProperSupersetOf(null); });
        }

        [Test]
        public void TestIsSubsetOf()
        {
            Assert.Throws<NotSupportedException>(() => { testSet.IsSubsetOf(null); });
        }

        [Test]
        public void TestIsSupersetOf()
        {
            Assert.Throws<NotSupportedException>(() => { testSet.IsSupersetOf(null); });
        }

        [Test]
        public void TestOverlaps()
        {
            Assert.Throws<NotSupportedException>(() => { testSet.Overlaps(null); });
        }

        [Test]
        public void TestSymmetricExceptWith()
        {
            Assert.Throws<NotSupportedException>(() => { testSet.SymmetricExceptWith(null); });
        }

        [Test]
        public void TestUnionWith()
        {
            Assert.Throws<NotSupportedException>(() => { testSet.UnionWith(null); });
        }

        [Test]
        public void TestSetEquals()
        {
            Assert.Throws<NotSupportedException>(() => { testSet.SetEquals(null); });
        }
    }
}