using System;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class HazelcastJsonValueTests
    {
        [Test]
        public void Test()
        {
            Assert.Throws<ArgumentNullException>(() => _ = new HazelcastJsonValue(null));

            var jsonValue = new HazelcastJsonValue("{ \"key\": \"value\" }");
            Assert.That(jsonValue.ToString(), Is.EqualTo("{ \"key\": \"value\" }"));

            var otherValue = new HazelcastJsonValue("{ \"key\": \"value\" }");
            Assert.That(jsonValue.Equals(otherValue), Is.True);
            Assert.That(jsonValue == otherValue, Is.False);
            Assert.That(jsonValue.GetHashCode() == otherValue.GetHashCode(), Is.True);

            Assert.That(jsonValue.Equals(jsonValue), Is.True);
            Assert.That(jsonValue.Equals(null), Is.False);
            Assert.That(jsonValue.Equals("foo"), Is.False);
        }
    }
}
