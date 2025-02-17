// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Models;
using Hazelcast.Testing;
using Hazelcast.Testing.Conditions;
using Hazelcast.Tests.TestObjects;
using NUnit.Framework;
namespace Hazelcast.Tests.Vector
{
    //[ServerCondition("[5.5.0")]
    public class VectorCollectionTests : SingleMemberClientRemoteTestBase
    {
        protected override string RcClusterConfiguration => Resources.Vector;
        private string basicKey = "key";
        private string basicValue = "value";
        private float[] testVector = new float[] { 1.0f, 2.0f, 3.0f };
        private EmployeeTestObject complexKey = new EmployeeTestObject()
        {
            Id = 1,
            Name = "name",
            Salary = 42,
            Started = DateTime.Today,
            StartedAtTimeStamp = DateTime.Now.Ticks,
            Type = 'E'
        };

        private EmployeeTestObject complexValue = new EmployeeTestObject()
        {
            Id = 2,
            Name = "name-value",
            Salary = 17,
            Started = DateTime.Today.AddDays(1),
            StartedAtTimeStamp = DateTime.Now.Ticks,
            Type = 'C'
        };

        
        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            HConsole.Configure(c=> c.ConfigureDefaults(this));
        }

        [Test]
        public async Task TestPutGetBasicDocumentAsync()
        {
            var vectorCollection = await Client.GetVectorCollectionAsync<string, string>(GetRandomName("vector"));
            var vectorDocument = VectorDocument<string>.Of(basicValue, VectorValues.Of(testVector));
            var nullResponse = await vectorCollection.PutAsync(basicKey, vectorDocument);
            Assert.IsNull(nullResponse);
            var result = await vectorCollection.GetAsync(basicKey);
            Assert.AreEqual(vectorDocument, result);
        }

        [Test]
        public async Task TestPutGetComplexDocumentAsync()
        {
            var vectorCollection = await Client.GetVectorCollectionAsync<EmployeeTestObject, EmployeeTestObject>(GetRandomName("vector"));
            var vectorDocument = VectorDocument<EmployeeTestObject>.Of(complexValue, VectorValues.Of(testVector));
            await vectorCollection.PutAsync(complexKey, vectorDocument);
            var result = await vectorCollection.GetAsync(complexKey);
            Assert.AreEqual(vectorDocument, result);
        }

        [Test]
        public async Task TestPutIfAbsentAsync()
        {
            var vectorCollection = await Client.GetVectorCollectionAsync<string, string>(GetRandomName("vector"));
            var vectorDocument = VectorDocument<string>.Of(basicValue, VectorValues.Of(testVector));

            var result = await vectorCollection.PutIfAbsentAsync(basicKey, vectorDocument);
            var result2 = await vectorCollection.PutIfAbsentAsync(basicKey, vectorDocument);

            Assert.IsNull(result);
            Assert.AreEqual(vectorDocument, result2);
        }

        [Test]
        public async Task TestPutAllAsync()
        {
            var vectorCollection = await Client.GetVectorCollectionAsync<string, string>(GetRandomName("vector"));
            var vectorDocument = VectorDocument<string>.Of(basicValue, VectorValues.Of(testVector));
            var vectorDocument2 = VectorDocument<string>.Of(basicValue, VectorValues.Of(testVector));
            var vectorDocumentMap = new System.Collections.Generic.Dictionary<string, VectorDocument<string>>()
            {
                { basicKey, vectorDocument },
                { basicKey + "2", vectorDocument2 }
            };
            await vectorCollection.PutAllAsync(vectorDocumentMap);
            var result = await vectorCollection.GetAsync(basicKey);
            var result2 = await vectorCollection.GetAsync(basicKey + "2");
            Assert.AreEqual(vectorDocument, result);
            Assert.AreEqual(vectorDocument2, result2);
        }

        [Test]
        public async Task TestSetAsync()
        {
            var vectorCollection = await Client.GetVectorCollectionAsync<string, string>(GetRandomName("vector"));
            var vectorDocument = VectorDocument<string>.Of(basicValue, VectorValues.Of(testVector));
            var vectorDocument2 = VectorDocument<string>.Of(basicValue, VectorValues.Of(testVector));
            await vectorCollection.PutAsync(basicKey, vectorDocument);
            await vectorCollection.SetAsync(basicKey, vectorDocument2);
            var result = await vectorCollection.GetAsync(basicKey);
            Assert.AreEqual(vectorDocument2, result);
        }

        [Test]
        public async Task TestRemoveAsync()
        {
            var vectorCollection = await Client.GetVectorCollectionAsync<string, string>(GetRandomName("vector"));
            var vectorDocument = VectorDocument<string>.Of(basicValue, VectorValues.Of(testVector));
            await vectorCollection.PutAsync(basicKey, vectorDocument);
            var result = await vectorCollection.RemoveAsync(basicKey);
            Assert.AreEqual(vectorDocument, result);
        }

        [Test]
        public async Task TestRemoveAsync_WhenKeyDoesNotExist()
        {
            var vectorCollection = await Client.GetVectorCollectionAsync<string, string>(GetRandomName("vector"));
            var result = await vectorCollection.RemoveAsync(basicKey);
            Assert.IsNull(result);
        }

        [Test]
        public async Task TestGetSizeAsync()
        {
            var vectorCollection = await Client.GetVectorCollectionAsync<string, string>(GetRandomName("vector"));
            var vectorDocument = VectorDocument<string>.Of(basicValue, VectorValues.Of(testVector));
            await vectorCollection.PutAsync(basicKey, vectorDocument);
            var result = await vectorCollection.GetSizeAsync();
            Assert.AreEqual(1, result);
        }

        [Test]
        public async Task TestSearchAsync()
        {
            //Put some data
            var vectorCollection = await Client.GetVectorCollectionAsync<string, string>(GetRandomName("vector"));
            var vectorDocument = VectorDocument<string>.Of(basicValue, VectorValues.Of(testVector));
            await vectorCollection.PutAsync(basicKey, vectorDocument);

            //Search
            var result = await vectorCollection.SearchAsync(VectorValues.Of(testVector),
                new VectorSearchOptions(includeValue: true, includeVectors: true, limit: 1));

            Assert.AreEqual(1, result.Size);

            var enumerator = result.Results;
            enumerator.MoveNext();
            Assert.IsNotNull(enumerator.Current);
            Assert.AreEqual(basicValue, enumerator.Current);
        }

    }
}
