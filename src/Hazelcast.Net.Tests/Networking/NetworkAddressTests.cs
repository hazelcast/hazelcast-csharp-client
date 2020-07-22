// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Networking;
using NUnit.Framework;

namespace Hazelcast.Tests.Networking
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

        [Test]
        public void Parse()
        {
            Assert.Throws<FormatException>(() => _ = NetworkAddress.Parse("[::1]:uh"));

            var address = NetworkAddress.Parse("127.0.0.1:5701");
            Assert.That(address.HostName, Is.EqualTo("127.0.0.1"));
            Assert.That(address.Port, Is.EqualTo(5701));

            Assert.That(NetworkAddress.TryParse("712.548", out IEnumerable<NetworkAddress> _), Is.False);

            Assert.That(NetworkAddress.TryParse("127.0.0.1", out IEnumerable<NetworkAddress> addresses), Is.True);
            Assert.That(addresses.Count(), Is.EqualTo(3));

            Assert.That(NetworkAddress.TryParse("::1", out addresses), Is.True);
            var array = addresses.ToArray();
            Assert.That(array.Length, Is.EqualTo(3));
            Assert.That(array[0].ToString(), Is.EqualTo("[::1]:5701"));
            Assert.That(array[0].IsIpV6, Is.True);
            Assert.That(array[1].ToString(), Is.EqualTo("[::1]:5702"));
            Assert.That(array[1].IsIpV6, Is.True);
            Assert.That(array[2].ToString(), Is.EqualTo("[::1]:5703"));
            Assert.That(array[2].IsIpV6, Is.True);

            Assert.That(NetworkAddress.TryParse("fe80::bd0f:a8bc:6480:238b", out addresses), Is.True);
            Assert.That(addresses.Count(), Is.GreaterThanOrEqualTo(3)); // depends on local NICs
            Assert.That(addresses.First().IsIpV6, Is.True);
            foreach (var a in addresses) Console.WriteLine(a);
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
            Assert.That(address.Port, Is.EqualTo(5701));

            address = new NetworkAddress(IPAddress.Parse("127.0.0.1"), 5702);
            Assert.That(address.HostName, Is.EqualTo("127.0.0.1"));
            Assert.That(address.Port, Is.EqualTo(5702));

            var ipAddress = IPAddress.Parse("127.0.0.1");
            var ipEndpoint = new IPEndPoint(ipAddress, 5701);
            address = new NetworkAddress(ipEndpoint);
            Assert.That(address.HostName, Is.EqualTo("127.0.0.1"));
            Assert.That(address.Port, Is.EqualTo(5701));

            Assert.Throws<ArgumentNullException>(() => _ = new NetworkAddress((IPAddress) null));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = new NetworkAddress(ipAddress, -1));

            ipEndpoint.Address = null;
            Assert.Throws<ArgumentException>(() => _ = new NetworkAddress(ipEndpoint));

            Assert.Throws<ArgumentNullException>(() => _ = new NetworkAddress((NetworkAddress)null, 5701));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = new NetworkAddress(address, -1));
        }

        [Test]
        public void Misc()
        {
            Assert.That(NetworkAddress.GetIPAddressByName("0.0.0.0"), Is.EqualTo(IPAddress.Any));
        }

        [Test]
        public void Lock()
        {
            var address = new NetworkAddress(IPAddress.Parse("127.0.0.1"));

            var semaphore = address.Lock;
            Assert.That(semaphore, Is.Not.Null);

            var semaphoreAgain = address.Lock;
            Assert.That(semaphoreAgain, Is.SameAs(semaphore));
        }
    }
}
