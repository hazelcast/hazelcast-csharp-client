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
    public class ReadOnlyLazySetTest
    {
        private ReadOnlyLazySet<int> _testSet;
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
            _testSet = new ReadOnlyLazySet<int>(dataList, _ss);
        }

        [TearDown]
        public void Destroy()
        {
            _ss.Destroy();
        }

        [Test]
        public void Count()
        {
            Assert.AreEqual(_testSet.Count, 5);
        }

        [Test]
        public void IsReadOnly()
        {
            Assert.True(_testSet.IsReadOnly);
        }

        [Test]
        public void Contains()
        {
            Assert.True(_testSet.Contains(1));
        }

        [Test]
        public void CopyTo()
        {
            var copyArray = new int[_testSet.Count + 5];
            _testSet.CopyTo(copyArray, 1);
            for (var i = 0; i < 5; i++)
            {
                Assert.True(_testSet.Contains(copyArray[i + 1]));
            }
        }

        [Test]
        public void Enumeration()
        {
            var ix = 0;
            foreach (var value in _testSet)
            {
                Assert.AreEqual(ix, value);
                ix++;
            }
        }

        [Test]
        public void Add()
        {
            Assert.Throws<NotSupportedException>(() => { _testSet.Add(4); });
        }

        [Test]
        public void Remove()
        {
            Assert.Throws<NotSupportedException>(() => { _testSet.Remove(4); });
        }

        [Test]
        public void Clear()
        {
            Assert.Throws<NotSupportedException>(() => { _testSet.Clear(); });
        }

        [Test]
        public void ExceptWith()
        {
            Assert.Throws<NotSupportedException>(() => { _testSet.ExceptWith(null); });
        }

        [Test]
        public void IntersectWith()
        {
            Assert.Throws<NotSupportedException>(() => { _testSet.IntersectWith(null); });
        }

        [Test]
        public void IsProperSubsetOf()
        {
            Assert.Throws<NotSupportedException>(() => { _testSet.IsProperSubsetOf(null); });
        }

        [Test]
        public void IsProperSupersetOf()
        {
            Assert.Throws<NotSupportedException>(() => { _testSet.IsProperSupersetOf(null); });
        }

        [Test]
        public void IsSubsetOf()
        {
            Assert.Throws<NotSupportedException>(() => { _testSet.IsSubsetOf(null); });
        }

        [Test]
        public void IsSupersetOf()
        {
            Assert.Throws<NotSupportedException>(() => { _testSet.IsSupersetOf(null); });
        }

        [Test]
        public void Overlaps()
        {
            Assert.Throws<NotSupportedException>(() => { _testSet.Overlaps(null); });
        }

        [Test]
        public void SymmetricExceptWith()
        {
            Assert.Throws<NotSupportedException>(() => { _testSet.SymmetricExceptWith(null); });
        }

        [Test]
        public void UnionWith()
        {
            Assert.Throws<NotSupportedException>(() => { _testSet.UnionWith(null); });
        }

        [Test]
        public void SetEquals()
        {
            Assert.Throws<NotSupportedException>(() => { _testSet.SetEquals(null); });
        }
    }
}