using System;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.DotNet
{
    [TestFixture]
    public class EnumTests
    {
        [Test]
        public void HasTests()
        {
            Assert.IsTrue(TestEnum.A.HasAll(TestEnum.A));
            Assert.IsFalse(TestEnum.A.HasAll(TestEnum.B));
            Assert.IsFalse(TestEnum.A.HasAll(TestEnum.A | TestEnum.B));
            Assert.IsTrue(TestEnum.A.HasAny(TestEnum.A | TestEnum.B));
            Assert.IsTrue((TestEnum.A | TestEnum.B).HasAll(TestEnum.A | TestEnum.B));
            Assert.IsTrue((TestEnum.A | TestEnum.B).HasAll(TestEnum.A));
        }

        [Flags]
        public enum TestEnum
        {
            A,
            B,
            C,
            D
        }
    }
}
