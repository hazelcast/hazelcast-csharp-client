// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.Serialization;
using Hazelcast.Serialization.ConstantSerializers;
using Hazelcast.Serialization.DefaultSerializers;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization
{
    [TestFixture]
    public class HazelcastJsonValueSerializationTest
    {
        private SerializationService _serializationService;

        [SetUp]
        public virtual void Setup()
        {
            _serializationService = new SerializationServiceBuilder(new NullLoggerFactory())
                .AddDefinitions(new ConstantSerializerDefinitions())
                .AddDefinitions(new DefaultSerializerDefinitions())
                .Build();
        }

        [TearDown]
        public virtual void TearDown()
        {
            _serializationService.Dispose();
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
            Assert.Throws<ArgumentNullException>(() => new HazelcastJsonValue(null));
        }
    }
}
