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
using Hazelcast.Core;
using Hazelcast.Projection;
using Hazelcast.Serialization;
using NUnit.Framework;

namespace Hazelcast.Tests.Projections
{
    [TestFixture]
    public class ProjectionsTests
    {
        [Test]
        public void SingleAttributeProjectionTest()
        {
            Assert.Throws<ArgumentException>(() => _ = Hazelcast.Projection.Projections.SingleAttribute(""));

            var x = Hazelcast.Projection.Projections.SingleAttribute("attribute");

            Assert.That(x, Is.InstanceOf<SingleAttributeProjection>());
            var p = (SingleAttributeProjection) x;

            Assert.That(p.FactoryId, Is.EqualTo(FactoryIds.ProjectionDsFactoryId));
            Assert.That(p.ClassId, Is.EqualTo(ProjectionDataSerializerHook.SingleAttribute));

            Assert.Throws<ArgumentNullException>(() => p.WriteData(null));
            Assert.Throws<ArgumentNullException>(() => p.ReadData(null));

            using var output = new ObjectDataOutput(256, null, Endianness.BigEndian);
            p.WriteData(output);

            using var input = new ObjectDataInput(output.Buffer, null, Endianness.BigEndian);

            p = new SingleAttributeProjection();
            p.ReadData(input);

            Assert.That(p.AttributePath, Is.EqualTo("attribute"));
        }
        
        [Test]
        public void MultipleAttributeProjectionTest()
        {
            Assert.Throws<ArgumentException>(() => _ = Hazelcast.Projection.Projections.MultipleAttribute(""));

            var x = Hazelcast.Projection.Projections.MultipleAttribute("attribute1","attribute2");

            Assert.That(x, Is.InstanceOf<MultiAttributeProjection>());
            var p = (MultiAttributeProjection) x;

            Assert.That(p.FactoryId, Is.EqualTo(FactoryIds.ProjectionDsFactoryId));
            Assert.That(p.ClassId, Is.EqualTo(ProjectionDataSerializerHook.MultiAttribute));

            Assert.Throws<ArgumentNullException>(() => p.WriteData(null));
            Assert.Throws<ArgumentNullException>(() => p.ReadData(null));

            using var output = new ObjectDataOutput(256, null, Endianness.BigEndian);
            p.WriteData(output);

            using var input = new ObjectDataInput(output.Buffer, null, Endianness.BigEndian);

            p = new MultiAttributeProjection();
            p.ReadData(input);

            Assert.That(p.AttributePaths[0], Is.EqualTo("attribute1"));
            Assert.That(p.AttributePaths[1], Is.EqualTo("attribute2"));
        }

        [Test]
        public void SerializerHookTest()
        {
            var hook = new ProjectionDataSerializerHook();

            Assert.That(hook.FactoryId, Is.EqualTo(FactoryIds.ProjectionDsFactoryId));

            var factory = hook.CreateFactory();
            var attribute = factory.Create(ProjectionDataSerializerHook.SingleAttribute);
            Assert.That(attribute, Is.InstanceOf<SingleAttributeProjection>());
        }
    }
}
