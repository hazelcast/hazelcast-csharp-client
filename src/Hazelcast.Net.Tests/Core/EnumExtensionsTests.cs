using System;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class EnumExtensionsTests
    {
        [Test]
        public void HasAll()
        {
            var v = SomeEnum.A;

            Assert.That(v.HasAll(SomeEnum.A), Is.True);
            Assert.That(v.HasAll(SomeEnum.A | SomeEnum.B), Is.False);
            Assert.That(v.HasAll(SomeEnum.B), Is.False);

            v = SomeEnum.A | SomeEnum.B;

            Assert.That(v.HasAll(SomeEnum.A), Is.True);
            Assert.That(v.HasAll(SomeEnum.A | SomeEnum.B), Is.True);
            Assert.That(v.HasAll(SomeEnum.B), Is.True);
        }

        [Test]
        public void HasAny()
        {
            var v = SomeEnum.A;

            Assert.That(v.HasAny(SomeEnum.A), Is.True);
            Assert.That(v.HasAny(SomeEnum.A | SomeEnum.B), Is.True);
            Assert.That(v.HasAny(SomeEnum.B), Is.False);

            v = SomeEnum.A | SomeEnum.B;

            Assert.That(v.HasAny(SomeEnum.A), Is.True);
            Assert.That(v.HasAny(SomeEnum.A | SomeEnum.B), Is.True);
            Assert.That(v.HasAny(SomeEnum.B), Is.True);
        }

        [Flags]
        private enum SomeEnum
        {
            None = 0, // (default)
            A = 1,
            B = 2,
            C = 4,
            D = 8,
            E = 16
        }
    }
}
