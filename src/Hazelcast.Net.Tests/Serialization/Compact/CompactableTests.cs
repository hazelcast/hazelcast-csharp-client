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
using Hazelcast.Messaging;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization.Compact
{
    // FIXME - dead code
    /*
    [TestFixture]
    internal class CompactableTests
    {
        [Test]
        public void LocalFromInterface()
        {
            var options = new SerializationOptions { Compact = { Enabled = true } };
            var messaging = Mock.Of<IClusterMessaging>();
            var serialization = HazelcastClientFactory.CreateSerializationService(options, messaging, new NullLoggerFactory());
            var obj = new ThingCompactableInterface { Name = "foo", Value = 42 };

            // serializer will be obtained via the ICompactable interface
            // type name will be derived from the class name
            // schema will be obtained via the schema builder
            var data = serialization.ToData(obj);
            Console.WriteLine(data.ToByteArray().Dump());

            var obj2 = serialization.ToObject<ThingCompactableInterface>(data);
            Assert.That(obj2.Name, Is.EqualTo(obj.Name));
            Assert.That(obj2.Value, Is.EqualTo(obj.Value));
        }

        [Test]
        public void LocalFromInterfaceWithTypeName()
        {
            var options = new SerializationOptions { Compact = { Enabled = true } };
            var messaging = Mock.Of<IClusterMessaging>();
            var serialization = HazelcastClientFactory.CreateSerializationService(options, messaging, new NullLoggerFactory());
            var obj = new ThingCompactableInterfaceWithTypeName { Name = "foo", Value = 42 };

            // serializer will be obtained via the ICompactable interface
            // type name will be obtained via the ICompactableWithTypeName interface
            // schema will be obtained via the schema builder
            var data = serialization.ToData(obj);
            Console.WriteLine(data.ToByteArray().Dump());

            var obj2 = serialization.ToObject<ThingCompactableInterfaceWithTypeName>(data);
            Assert.That(obj2.Name, Is.EqualTo(obj.Name));
            Assert.That(obj2.Value, Is.EqualTo(obj.Value));
        }
    }
    */
}
