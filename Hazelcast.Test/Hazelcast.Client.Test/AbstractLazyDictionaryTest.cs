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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Hazelcast.IO.Serialization;
using Hazelcast.Util;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public abstract class AbstractLazyDictionaryTest
    {
        internal abstract AbstractLazyDictionary<int, string> TestCollection { get; }

        [Test]
        public void Test_GetEnumerator()
        {
            var enumerator = TestCollection.GetEnumerator();
            var ix = 0;
            while (enumerator.MoveNext())
            {
                var pair = enumerator.Current;
                Assert.AreEqual(pair.Key, ix);
                Assert.AreEqual(pair.Value, ix.ToString());
                ix++;
            }
        }

        [Test]
        public void Test_IEnumerable_GetEnumerator()
        {
            var enumerator = ((IEnumerable) TestCollection).GetEnumerator();
            var ix = 0;
            while (enumerator.MoveNext())
            {
                var pair = (KeyValuePair<int, string>) enumerator.Current;
                Assert.AreEqual(pair.Key, ix);
                Assert.AreEqual(pair.Value, ix.ToString());
                ix++;
            }
        }

        [Test]
        public void Test_Add_Pair()
        {
            Assert.Throws<NotSupportedException>(() => { TestCollection.Add(new KeyValuePair<int, string>(1, "")); });
        }

        [Test]
        public void Test_Clear()
        {
            Assert.Throws<NotSupportedException>(() => TestCollection.Clear());
        }

        [Test]
        public void Test_Contains()
        {
            for (var i = 0; i < 4; i++)
            {
                Assert.True(TestCollection.Contains(new KeyValuePair<int, string>(i, i.ToString())));
            }
        }

        [Test]
        public void Test_CopyTo()
        {
            var copyArray = new KeyValuePair<int, string>[TestCollection.Count + 5];
            TestCollection.CopyTo(copyArray, 1);
            for (var i = 0; i < 4; i++)
            {
                Assert.AreEqual(copyArray[i+1].Key, i);
                Assert.AreEqual(copyArray[i+1].Value, i.ToString());
            }
        }

        [Test]
        public void Test_Remove_pair()
        {
            Assert.Throws<NotSupportedException>(() => TestCollection.Remove(2));
        }

        [Test]
        public void Test_Count()
        {
            Assert.AreEqual(4, TestCollection.Count);
        }

        [Test]
        public void Test_IsReadOnly()
        {
            Assert.True(TestCollection.IsReadOnly);
        }

        [Test]
        public void Test_Add_key_value()
        {
            Assert.Throws<NotSupportedException>(() => { TestCollection.Add(1, ""); });
        }

        [Test]
        public void Test_ContainsKey()
        {
            for (var i = 0; i < 4; i++)
            {
                Assert.True(TestCollection.ContainsKey(i));
            }
        }

        [Test]
        public void Test_Remove_key()
        {
            Assert.Throws<NotSupportedException>(() => { TestCollection.Remove(1); });
        }

        [Test]
        public void Test_TryGetValue()
        {
            for (int i = 0; i < 5; i++)
            {
                string val;
                if (TestCollection.TryGetValue(i, out val))
                {
                    Assert.AreEqual(val, i.ToString());
                }
            }
        }

        [Test]
        public void Test_Index_operator()
        {
            for (int i = 0; i < 4; i++)
            {
                string val = TestCollection[i];
                Assert.AreEqual(val, i.ToString());
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