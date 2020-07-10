using NUnit.Framework;
using Hazelcast.Core;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class EndiannessExtensionsTests
    {
        [Test]
        public void IsBigOrLittleEndian()
        {
            Assert.That(Endianness.BigEndian.IsBigEndian(), Is.True);
            Assert.That(Endianness.BigEndian.IsLittleEndian(), Is.False);

            Assert.That(Endianness.LittleEndian.IsBigEndian(), Is.False);
            Assert.That(Endianness.LittleEndian.IsLittleEndian(), Is.True);
        }
    }
}
