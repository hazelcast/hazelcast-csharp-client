using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Hazelcast.Client.Model;
using Hazelcast.Config;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    internal class ClientIpV6AddressTest : HazelcastBaseTest
    {
        [Test]
        public void TestIpV6Address()
        {
            var address = GetLocalIpV6Address();
            if (address == null)
            {
                Assert.Pass("No IpV6 available on this machine for testing.");
            }

            AssertClientWithAddress("[" + address + "]:5701");
        }

        private static void AssertClientWithAddress(string address)
        {
            var client = new HazelcastClientFactory().CreateClient(config =>
            {
                config.GetNetworkConfig().AddAddress(address);
            });

            var map = client.GetMap<string, string>("ipv6");
            map.Put("key", "val");
            Assert.AreEqual("val", map.Get("key"));
            map.Destroy();

            client.Shutdown();
        }

        [Test]
        public void TestIpV6AddressWithMissingScope()
        {
            var address = GetLocalIpV6Address();
            if (address == null)
            {
                Assert.Pass("No IpV6 available on this machine for testing.");
            }

            address = Regex.Replace(address, "%.+", "");
            AssertClientWithAddress("[" + address + "]:5701");
        }

        [Test]
        public void TestIpV6AddressWithMissingPort()
        {
            var address = GetLocalIpV6Address();
            if (address == null)
            {
                Assert.Pass("No IpV6 available on this machine for testing.");
            }

            AssertClientWithAddress("[" + address + "]");
        }

        private static string GetLocalIpV6Address()
        {
            var strHostName = Dns.GetHostName();
            var ipEntry = Dns.GetHostEntry(strHostName);
            var addr = ipEntry.AddressList;
            foreach (var address in addr)
            {
                if (address.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    return address.ToString(); //ipv6
                }
            }
            return null;
        }
    }
}