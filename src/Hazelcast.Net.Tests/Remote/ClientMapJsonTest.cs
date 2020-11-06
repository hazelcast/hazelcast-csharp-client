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

using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Hazelcast.Predicates;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Remote
{
    [TestFixture]
    public class ClientMapJsonTest : SingleMemberClientRemoteTestBase
    {
        private IHMap<string, HazelcastJsonValue> _dictionary;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            _dictionary = await Client.GetMapAsync<string, HazelcastJsonValue>(CreateUniqueName());
        }

        [TearDown]
        public async Task TearDown()
        {
            await _dictionary.ClearAsync();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await _dictionary.DestroyAsync();
            await _dictionary.DisposeAsync();
        }

        private async Task FillAsync()
        {
            for (var i = 1; i < 30; i++)
            {
                await _dictionary.SetAsync("key-" + i, new HazelcastJsonValue("{ \"age\": " + i + " }"));
            }
        }

        [Test]
        public async Task PutJsonValue_Succeeded()
        {
            var value = new HazelcastJsonValue("{ \"age\": 20 }");
            await _dictionary.SetAsync("key-1", value);
        }

        [Test]
        public async Task GetJsonValue_Succeeded()
        {
            var value = new HazelcastJsonValue("{ \"age\": 20 }");
            await _dictionary.SetAsync("key-1", value);

            var result = await _dictionary.GetAsync("key-1");

            Assert.That(result.Success);
            Assert.AreEqual(value, result.Value);
        }

        [Test]
        public async Task QueryOnNumberProperty_Succeeded()
        {
            await FillAsync();
            var result = await _dictionary.GetValuesAsync(Predicate.IsLessThan("age", 20));
            Assert.AreEqual(19, result.Count);
        }

        [Test]
        public async Task QueryOnTextAndNumberProperty_WhenSomeEntriesDoNotHaveTheField_ShouldNotFail()
        {
            var value = new HazelcastJsonValue("{ \"email\": \"a@b.com\" }");
            await _dictionary.SetAsync("key-a", value);

            await FillAsync();

            var result = await _dictionary.GetValuesAsync(Predicate.IsEqual("email", "a@b.com"));

            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public async Task QueryOnNestedProperty_Succeeded()
        {
            var value = new HazelcastJsonValue("{ \"outer\": {\"inner\": 24} }");
            await _dictionary.SetAsync("key-a", value);

            await FillAsync();

            var result = await _dictionary.GetValuesAsync(Predicate.IsEqual("outer.inner", 24));

            Assert.AreEqual(1, result.Count);
        }
    }
}