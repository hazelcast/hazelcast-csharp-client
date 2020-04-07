using Hazelcast.Networking;
using NUnit.Framework;

namespace AsyncTests1
{
    [TestFixture]
    public class NetworkAddressTests
    {
        [Test]
        public void PortIs5701()
        {
            Assert.AreEqual(5701, NetworkAddress.DefaultPort);
        }

        [TestCase("127.0.0.1", true, "127.0.0.1:5701")]
        [TestCase("127.0.0.1:81", true, "127.0.0.1:81")]
        [TestCase("1", true, "0.0.0.1:5701")]
        [TestCase(":82", false, "")]
        [TestCase("666", true, "0.0.2.154:5701")]
        [TestCase("::1", true, "[::1]:5701")]
        [TestCase("::1%33", true, "[::1%33]:5701")]
        [TestCase("[::1]:81", true, "[::1]:81")]
        [TestCase("[::1%33]:81", true, "[::1%33]:81")]
        [TestCase("[65535]", false, "")]
        [TestCase("www.hazelcast.com", true, "www.hazelcast.com:5701")]
        [TestCase("www.hazelcast.com:81", true, "www.hazelcast.com:81")]
        [TestCase("www.hazelcast", false, "")]
        public void CanTryParse(string s, bool succeeds, string toString)
        {
            var result = NetworkAddress.TryParse(s, out var networkAddress);
            if (succeeds) Assert.IsTrue(result); else Assert.IsFalse(result);
            if (succeeds) Assert.AreEqual(toString, networkAddress.ToString());
        }

        [Test]
        public void EqualAndHash()
        {
            Assert.AreEqual(new NetworkAddress("127.0.0.1"), new NetworkAddress("127.0.0.1"));
            Assert.AreEqual(new NetworkAddress("127.0.0.1").GetHashCode(), new NetworkAddress("127.0.0.1").GetHashCode());
        }
    }
}
