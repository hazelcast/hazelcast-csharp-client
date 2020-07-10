using System;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class Murmur3HashCodeTests
    {
        [Test]
        public void Test()
        {
            _ = Murmur3HashCode.Hash(new byte[100], 0, 100);
            _ = Murmur3HashCode.Hash(new byte[100], 0, 99);
            _ = Murmur3HashCode.Hash(new byte[100], 0, 98);
        }

        [Test]
        public void ArgumentExceptions()
        {
            Assert.Throws<ArgumentNullException>(() => Murmur3HashCode.Hash(null, 0, 100));
            Assert.Throws<ArgumentOutOfRangeException>(() => Murmur3HashCode.Hash(new byte[10], -1, 100));
            Assert.Throws<ArgumentOutOfRangeException>(() => Murmur3HashCode.Hash(new byte[10], 11, 100));
            Assert.Throws<ArgumentOutOfRangeException>(() => Murmur3HashCode.Hash(new byte[10], 0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => Murmur3HashCode.Hash(new byte[10], 5, 6));
        }
    }
}
