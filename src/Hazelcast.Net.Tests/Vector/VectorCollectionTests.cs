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
using Hazelcast.Models;
using Hazelcast.Testing;
using Hazelcast.Tests.TestObjects;
using NUnit.Framework;
namespace Hazelcast.Tests.Vector
{
    public class VectorCollectionTests : SingleMemberClientRemoteTestBase
    {

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


        [Test]
        public async Task TestPutGetBasicDocumentAsync()
        {
            // Arrange
            var vectorCollection = await Client.GetVectorCollectionAsync<string, string>(GetRandomName("vector"));

            var vectorDocument = VectorDocument<string>.Of(basicValue, VectorValues.Of(testVector));
            // Act
            await vectorCollection.PutAsync(basicKey, vectorDocument);
            var result = await vectorCollection.GetAsync(basicKey);
            // Assert
            Assert.AreEqual(vectorDocument, result);
        }

        [Test]
        public async Task TestPutGetComplexDocumentAsync()
        {
            // Arrange
            var vectorCollection = await Client.GetVectorCollectionAsync<EmployeeTestObject, EmployeeTestObject>(GetRandomName("vector"));

            var vectorDocument = VectorDocument<EmployeeTestObject>.Of(complexValue, VectorValues.Of(testVector));
            // Act
            await vectorCollection.PutAsync(complexKey, vectorDocument);
            var result = await vectorCollection.GetAsync(complexKey);
            // Assert
            Assert.AreEqual(vectorDocument, result);
        }

    }
}
