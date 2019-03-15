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

using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using NUnit.Framework;
using System;

namespace Hazelcast.Client.Test.Serialization
{
    [TestFixture]
    [Category("3.12")]
    public class HazelcastJsonValueSerializationTest
    {
        private ISerializationService _serializationService;

        [SetUp]
        public virtual void Setup()
        {
            _serializationService = new SerializationServiceBuilder().Build();
        }

        [TearDown]
        public virtual void TearDown()
        {
            _serializationService.Destroy();
        }

        [Test]
        public void TestSerializeDeserializeJsonValue()
        {
            var jsonValue = new HazelcastJsonValue("{ \"key\": \"value\" }");
            var jsonData = _serializationService.ToData(jsonValue);
            var jsonDeserialized = _serializationService.ToObject<HazelcastJsonValue>(jsonData);

            Assert.AreEqual(jsonValue, jsonDeserialized);
        }

        [Test]
        public void FromStringShouldThrowExceptionOnNullArgument()
        {
            Assert.Throws<NullReferenceException>(() => new HazelcastJsonValue(null));
        }
    }
}
