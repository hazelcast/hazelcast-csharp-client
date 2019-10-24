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
using Hazelcast.Util;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public abstract class AbstractLazyDictionaryTest
    {
        internal abstract AbstractLazyDictionary<int, string> TestCollection { get; }

        [Test]
        public void GetEnumerator()
        {
            var ix = 0;
            foreach (var kvp in TestCollection)
            {
                Assert.AreEqual(ix, kvp.Key);
                Assert.AreEqual(ix.ToString(), kvp.Value);
                ix++;
            }
        }

        [Test]
        public void IEnumerable_GetEnumerator()
        {
            var enumerable = (IEnumerable)TestCollection;
            var ix = 0;

            foreach (var o in enumerable)
            {
                var kvp = (KeyValuePair<int, string>)o;
                Assert.AreEqual(kvp.Key, ix);
                Assert.AreEqual(kvp.Value, ix.ToString());
                ix++;
            }
        }

        [Test]
        public void Add_Pair()
        {
            Assert.Throws<NotSupportedException>(() => { TestCollection.Add(new KeyValuePair<int, string>(1, "")); });
        }

        [Test]
        public void Clear()
        {
            Assert.Throws<NotSupportedException>(() => TestCollection.Clear());
        }

        [Test]
        public void Contains()
        {
            for (var i = 0; i < 4; i++)
            {
                Assert.True(TestCollection.Contains(new KeyValuePair<int, string>(i, i.ToString())));
            }
        }

        [Test]
        public void CopyTo()
        {
            var copyArray = new KeyValuePair<int, string>[TestCollection.Count + 5];
            TestCollection.CopyTo(copyArray, 1);
            for (var i = 0; i < 4; i++)
            {
                Assert.AreEqual(copyArray[i + 1].Key, i);
                Assert.AreEqual(copyArray[i + 1].Value, i.ToString());
            }
        }

        [Test]
        public void Remove_pair()
        {
            Assert.Throws<NotSupportedException>(() => TestCollection.Remove(2));
        }

        [Test]
        public void Count()
        {
            Assert.AreEqual(4, TestCollection.Count);
        }

        [Test]
        public void IsReadOnly()
        {
            Assert.True(TestCollection.IsReadOnly);
        }

        [Test]
        public void Add_key_value()
        {
            Assert.Throws<NotSupportedException>(() => { TestCollection.Add(1, ""); });
        }

        [Test]
        public void ContainsKey()
        {
            for (var i = 0; i < 4; i++)
            {
                Assert.True(TestCollection.ContainsKey(i));
            }
        }

        [Test]
        public void Remove_key()
        {
            Assert.Throws<NotSupportedException>(() => { TestCollection.Remove(1); });
        }

        [Test]
        public void TryGetValue()
        {
            for (var i = 0; i < 5; i++)
            {
                if (TestCollection.TryGetValue(i, out var val))
                {
                    Assert.AreEqual(val, i.ToString());
                }
            }
        }

        [Test]
        public void Index_operator()
        {
            for (var i = 0; i < 4; i++)
            {
                var val = TestCollection[i];
                Assert.AreEqual(i.ToString(), val);
            }

            Assert.Throws<KeyNotFoundException>(() =>
            {
                var x = TestCollection[99];
            });
            Assert.Throws<NotSupportedException>(() =>
            {
                TestCollection[99] = "99";
            });
        }
    }
}