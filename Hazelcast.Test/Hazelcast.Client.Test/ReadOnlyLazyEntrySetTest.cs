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

using System.Collections.Concurrent;
using System.Collections.Generic;
using Hazelcast.IO.Serialization;
using Hazelcast.Util;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ReadOnlyLazyEntrySetTest : AbstractLazyDictionaryTest
    {
        private ReadOnlyLazyEntrySet<int, string> _testCollection;

        [SetUp]
        public void Init()
        {
            var _ss = new SerializationServiceBuilder().Build();
            var dataList = new ConcurrentQueue<KeyValuePair<IData, object>>();
            dataList.Enqueue(new KeyValuePair<IData, object>(_ss.ToData(0), "0"));
            dataList.Enqueue(new KeyValuePair<IData, object>(_ss.ToData(1), _ss.ToData("1")));
            dataList.Enqueue(new KeyValuePair<IData, object>(_ss.ToData(2), _ss.ToData("2")));
            dataList.Enqueue(new KeyValuePair<IData, object>(_ss.ToData(3), "3"));
            _testCollection = new ReadOnlyLazyEntrySet<int, string>(dataList, _ss);
        }

        internal override AbstractLazyDictionary<int, string> TestCollection
        {
            get { return _testCollection; }
        }

        [Test]
        public void Test_SetEquals()
        {
            ISet<KeyValuePair<int, string>> otherSet = new HashSet<KeyValuePair<int, string>>
            {
                new KeyValuePair<int, string>(0, "0"),
                new KeyValuePair<int, string>(1, "1"),
                new KeyValuePair<int, string>(2, "2"),
                new KeyValuePair<int, string>(3, "3")
            };
            Assert.True(_testCollection.SetEquals(otherSet));
        }
    }
}