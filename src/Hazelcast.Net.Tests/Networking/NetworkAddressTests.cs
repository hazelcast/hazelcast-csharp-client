// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Hazelcast.Networking;
using Hazelcast.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Hazelcast.Tests.Networking
{
    [TestFixture]
    public class NetworkAddressTests
    {
        [TestCase("127.0.0.1", true, "127.0.0.1:0")]
        [TestCase(" 127.0.0.1", true, "127.0.0.1:0")]
        [TestCase("127.0.0.1 ", true, "127.0.0.1:0")]
        [TestCase(" 127.0.0.1 ", true, "127.0.0.1:0")]
        [TestCase("127.0.0.1:81", true, "127.0.0.1:81")]
        [TestCase("1", true, "0.0.0.1:0")]
        [TestCase(":82", false, "")]
        [TestCase("666", true, "0.0.2.154:0")]
        [TestCase("::1", true, "[::1]:0")]
        [TestCase("::1%33", true, "[::1%33]:0")]
        [TestCase("[::1]:81", true, "[::1]:81")]
        [TestCase("[::1%33]:81", true, "[::1%33]:81")]
        [TestCase("[65535]", false, "")]
        [TestCase("www.hazelcast.com", true, null)] // cannot depend on actual resolution
        [TestCase("www.hazelcast.com:81", true, null)] // cannot depend on actual resolution
        [TestCase("www.hazelcast", false, "")]
        [TestCase("x[::1]:81", false, "")]
        [TestCase("[::81", false, "")]
        [TestCase("[::1]x:81", false, "")]
        [TestCase("[::1]:uh", false, "")]
        [TestCase("[]", false, "")]
        [TestCase("[##::'']:81", false, "")]
        public void CanTryParse(string s, bool succeeds, string toString)
        {
            var result = NetworkAddress.TryParse(s, out NetworkAddress networkAddress);
            if (succeeds) Assert.IsTrue(result); else Assert.IsFalse(result);
            if (succeeds && toString != null) Assert.AreEqual(toString, networkAddress.ToString());
        }

        [Test]
        public void NetworkAddressEqualAndHash()
        {
            Assert.AreEqual(new NetworkAddress("127.0.0.1"), new NetworkAddress("127.0.0.1"));
            Assert.AreEqual(new NetworkAddress("127.0.0.1").GetHashCode(), new NetworkAddress("127.0.0.1").GetHashCode());

            // ReSharper disable once EqualExpressionComparison
            Assert.IsTrue(new NetworkAddress("127.0.0.1") == new NetworkAddress("127.0.0.1"));
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void IPEndPointEqualAndHash()
        {
            var d = new ConcurrentDictionary<IPEndPoint, string>();
            d[new IPEndPoint(IPAddress.Parse("127.0.0.1"), 666)] = "a";
            d[new IPEndPoint(IPAddress.Parse("127.0.0.1"), 666)] = "b";
            Assert.AreEqual(1, d.Count);
            Assert.AreEqual("b", d[new IPEndPoint(IPAddress.Parse("127.0.0.1"), 666)]);
        }

        private static void AssertAddresses(IReadOnlyCollection<NetworkAddress> addresses, int count, string n, int port, bool isIpV6)
        {
            foreach (var address in addresses) Console.WriteLine("  " + address);
            Assert.That(addresses.Count, Is.EqualTo(count));
            foreach (var address in addresses)
            {
                if (n == "*")
                    Assert.That(address.ToString(), Does.EndWith(":" + port++));
                else
                    Assert.That(address.ToString(), Is.EqualTo(n + ":" + port++));
                Assert.That(address.IsIpV6, Is.EqualTo(isIpV6));
            }
        }

        private static (IReadOnlyCollection<NetworkAddress> Primary, IReadOnlyCollection<NetworkAddress> Secondary) GetAddresses(string address)
        {
            var options = new NetworkingOptions();
            options.Addresses.Clear();
            options.Addresses.Add(address);
            var addressProviderSource = new ConfigurationAddressProviderSource(options, new NullLoggerFactory());
            return addressProviderSource.GetAddresses(false);
        }

        [Test]
        public void Parse()
        {
            Assert.Throws<FormatException>(() => _ = NetworkAddress.Parse("[::1]:uh"));

            var address = NetworkAddress.Parse("127.0.0.1:5701");
            Assert.That(address.HostName, Is.EqualTo("127.0.0.1"));
            Assert.That(address.Port, Is.EqualTo(5701));

            Assert.That(NetworkAddress.TryParse("712.548", out var _), Is.False);

            var addresses = GetAddresses("127.0.0.1");
            Console.WriteLine("127.0.0.1");
            AssertAddresses(addresses.Primary, 1, "127.0.0.1", 5701, false);
            AssertAddresses(addresses.Secondary, 2, "127.0.0.1", 5702, false);

            addresses = GetAddresses("localhost");
            Console.WriteLine("localhost");
            AssertAddresses(addresses.Primary, 1, "127.0.0.1", 5701, false);
            AssertAddresses(addresses.Secondary, 2, "127.0.0.1", 5702, false);

            // on Windows, this gets 127.0.0.1 but on Linux it gets what the host name
            // maps to in /etc/hosts and by default on some systems (eg Debian) it can
            // be 127.0.1.1 instead of 127.0.0.1
            //
            addresses = GetAddresses(Dns.GetHostName());
            Console.Write(Dns.GetHostName());
            var n = Dns.GetHostAddresses(Dns.GetHostName()).First(x => x.AddressFamily == AddressFamily.InterNetwork).ToString();
            Console.WriteLine(" -> " + n);
            AssertAddresses(addresses.Primary, 1, n, 5701, false);
            AssertAddresses(addresses.Secondary, 2, n, 5702, false);

            addresses = GetAddresses("::1");
            Console.WriteLine("::1");
            AssertAddresses(addresses.Primary, 1, "[::1]", 5701, true);
            AssertAddresses(addresses.Secondary, 2, "[::1]", 5702, true);

            // on Windows, this gets the various fe80 local addresses (but not the random one
            // that we specified) - on Linux this gets nothing and it may eventually be an issue?
            // there are various issues corresponding to this situation,
            // see https://github.com/dotnet/runtime/issues/27534
            // and fixes seem to be in the 5.0 milestone = n/a yet.

            addresses = GetAddresses("fe80::bd0f:a8bc:6480:238b");
            Console.WriteLine("fe80::bd0f:a8bc:6480:238b");
            if (OS.IsWindows)
            {
                // test the first, we might get more depending on NICs
                AssertAddresses(addresses.Primary.Take(1).ToList(), 1, "*", 5701, true);
                AssertAddresses(addresses.Secondary.Take(2).ToList(), 2, "*", 5702, true);
            }
            else
            {
                foreach (var a in addresses.Primary) Console.WriteLine("  " + a);
                foreach (var a in addresses.Secondary) Console.WriteLine("  " + a);
            }
        }

        [Test]
        public void Equality()
        {
            var address1 = NetworkAddress.Parse("127.0.0.1:5701");
            var address2 = NetworkAddress.Parse("127.0.0.1:5702");
            var address3 = NetworkAddress.Parse("127.0.0.1:5701");

            Assert.That(address1 == address2, Is.False);
            Assert.That(address1 != address2, Is.True);

            Assert.That(address1 == address3, Is.True);
            Assert.That(address1 != address3, Is.False);
        }

        [Test]
        public void Constructors()
        {
            var address = new NetworkAddress(IPAddress.Parse("127.0.0.1"));
            Assert.That(address.HostName, Is.EqualTo("127.0.0.1"));
            Assert.That(address.Port, Is.EqualTo(0));

            address = new NetworkAddress(IPAddress.Parse("127.0.0.1"), 5702);
            Assert.That(address.HostName, Is.EqualTo("127.0.0.1"));
            Assert.That(address.Port, Is.EqualTo(5702));

            var ipAddress = IPAddress.Parse("127.0.0.1");
            var ipEndpoint = new IPEndPoint(ipAddress, 0);
            address = new NetworkAddress(ipEndpoint);
            Assert.That(address.HostName, Is.EqualTo("127.0.0.1"));
            Assert.That(address.Port, Is.EqualTo(0));

            Assert.Throws<ArgumentNullException>(() => _ = new NetworkAddress((IPAddress) null));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = new NetworkAddress(ipAddress, -1));

#if !NET5_0_OR_GREATER
            ipEndpoint.Address = null; // this is not even legal in NET 5+
            Assert.Throws<ArgumentException>(() => _ = new NetworkAddress(ipEndpoint));
#endif

            Assert.Throws<ArgumentNullException>(() => _ = new NetworkAddress((NetworkAddress)null, 5701));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = new NetworkAddress(address, -1));
        }

        [Test]
        public void Misc()
        {
            Assert.That(NetworkAddress.GetIPAddressByName("0.0.0.0"), Is.EqualTo(IPAddress.Any));
        }
    }
}
