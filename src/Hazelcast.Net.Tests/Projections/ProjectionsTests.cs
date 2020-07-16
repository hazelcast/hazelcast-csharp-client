using System;
using Hazelcast.Core;
using Hazelcast.Projections;
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
            Assert.Throws<ArgumentException>(() => _ = new SingleAttributeProjection(""));

            var p = new SingleAttributeProjection();

            Assert.That(p.FactoryId, Is.EqualTo(FactoryIds.ProjectionDsFactoryId));
            Assert.That(p.ClassId, Is.EqualTo(ProjectionDataSerializerHook.SingleAttribute));

            p = new SingleAttributeProjection("attribute");

            Assert.That(p.FactoryId, Is.EqualTo(FactoryIds.ProjectionDsFactoryId));
            Assert.That(p.ClassId, Is.EqualTo(ProjectionDataSerializerHook.SingleAttribute));

            Assert.Throws<ArgumentNullException>(() => p.WriteData(null));
            Assert.Throws<ArgumentNullException>(() => p.ReadData(null));

            using var output = new ByteArrayObjectDataOutput(256, null, Endianness.Unspecified);
            p.WriteData(output);

            using var input = new ByteArrayObjectDataInput(output.Buffer, null, Endianness.Unspecified);

            p = new SingleAttributeProjection();
            p.ReadData(input);

            Assert.That(p.AttributePath, Is.EqualTo("attribute"));
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
