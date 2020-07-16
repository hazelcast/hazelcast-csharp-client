using System.Linq;
using System.Net;
using Hazelcast.Networking;
using NUnit.Framework;

namespace Hazelcast.Tests.Networking
{
    [TestFixture]
    public class DnsExTests
    {
        [Test]
        public void Test()
        {
            Assert.That(DnsEx.GetHostName(), Is.EqualTo(Dns.GetHostName()));

            Assert.That(AreEqual(DnsEx.GetHostEntry("127.0.0.1"), Dns.GetHostEntry("127.0.0.1")), Is.True);
            Assert.That(AreEqual(DnsEx.GetHostEntry("www.hazelcast.com"), Dns.GetHostEntry("www.hazelcast.com")), Is.True);

            Assert.That(AreEqual(DnsEx.GetHostEntry(IPAddress.Parse("127.0.0.1")), Dns.GetHostEntry(IPAddress.Parse("127.0.0.1"))), Is.True);

            Assert.That(AreEqual(DnsEx.GetHostAddresses("127.0.0.1"), Dns.GetHostAddresses("127.0.0.1")), Is.True);
            Assert.That(AreEqual(DnsEx.GetHostAddresses("www.hazelcast.com"), Dns.GetHostAddresses("www.hazelcast.com")), Is.True);
        }

        private static bool AreEqual(IPAddress[] left, IPAddress[] right)
        {
            if (left.Length != right.Length) return false;
            foreach (var x in left)
                if (!right.Contains(x))
                    return false;

            return true;
        }

        private static bool AreEqual(IPHostEntry left, IPHostEntry right)
        {
            if (left.HostName != right.HostName) return false;

            if (left.Aliases.Length != right.Aliases.Length) return false;
            foreach (var x in left.Aliases)
                if (!right.Aliases.Contains(x))
                    return false;

            if (left.AddressList.Length != right.AddressList.Length) return false;
            foreach (var x in left.AddressList)
                if (!right.AddressList.Contains(x))
                    return false;

            return true;
        }
    }
}
