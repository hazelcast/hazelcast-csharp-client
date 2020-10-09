using System.Collections.Generic;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.DotNet
{
    [TestFixture]
    public class TypeToStringTests
    {
        [Test]
        public void Test()
        {
            Assert.That(typeof(IList<int>).ToString(), Is.EqualTo("System.Collections.Generic.IList`1[System.Int32]"));
            Assert.That(typeof(IList<int>).ToCsString(true), Is.EqualTo("System.Collections.Generic.IList<int>"));
            Assert.That(typeof(IList<int>).ToCsString(), Is.EqualTo("IList<int>"));

            Assert.That(typeof(Nested).ToCsString(true), Is.EqualTo("Hazelcast.Tests.DotNet.TypeToStringTests.Nested"));
            Assert.That(typeof(Nested).ToCsString(), Is.EqualTo("TypeToStringTests.Nested"));
        }

        private class Nested
        { }
    }
}