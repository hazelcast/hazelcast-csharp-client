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

using Hazelcast.Config;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    [Category("3.12")]
    public class ClientMapJsonTest : SingleMemberBaseTest
    {
        internal IMap<string, HazelcastJsonValue> _map;

        [SetUp]
        public void Init()
        {
            _map = Client.GetMap<string, HazelcastJsonValue>(TestSupport.RandomString());
        }

        [TearDown]
        public void Destroy()
        {
            _map.Clear();
        }

        void FillMap()
        {
            for (var pos = 1; pos < 30; pos++)
            {
                var value = new HazelcastJsonValue("{ \"age\": " + pos + " }");
                _map.Put("key-" + pos, value);
            }
        }

        [Test]
        public void PutJsonValue_Succeeded()
        {
            var value = new HazelcastJsonValue("{ \"age\": 20 }");
            _map.Put("key-1", value);
        }

        [Test]
        public void GetJsonValue_Succeeded()
        {
            var value = new HazelcastJsonValue("{ \"age\": 20 }");

            _map.Put("key-1", value);
            var result = _map.Get("key-1");

            Assert.AreEqual(value, result);
        }

        [Test]
        public void QueryOnNumberProperty_Succeeded()
        {
            FillMap();
            var result = _map.Values(Predicates.IsLessThan("age", 20));
            Assert.AreEqual(19, result.Count);
        }

        [Test]
        public void QueryOnTextAndNumberProperty_WhenSomeEntriesDoNotHaveTheField_ShouldNotFail()
        {
            var value = new HazelcastJsonValue("{ \"email\": \"a@b.com\" }");
            _map.Put("key-001", value);

            FillMap();
            var result = _map.Values(Predicates.IsEqual("email", "a@b.com"));

            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void QueryOnNestedProperty_Succeeded()
        {
            var value = new HazelcastJsonValue("{ \"outer\": {\"inner\": 24} }");
            _map.Put("key-001", value);

            FillMap();
            var result = _map.Values(Predicates.IsEqual("outer.inner", 24));

            Assert.AreEqual(1, result.Count);
        }
    }
}