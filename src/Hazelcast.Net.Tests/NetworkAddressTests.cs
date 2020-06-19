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

using System.Collections.Concurrent;
using System.Net;
using Hazelcast.Networking;
using NUnit.Framework;

namespace Hazelcast.Tests
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
            var result = NetworkAddress.TryParse(s, out NetworkAddress networkAddress);
            if (succeeds) Assert.IsTrue(result); else Assert.IsFalse(result);
            if (succeeds) Assert.AreEqual(toString, networkAddress.ToString());
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
    }
}
