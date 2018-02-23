// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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

using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Hazelcast.Remote;
using Hazelcast.Test;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    [Ignore("IPv6 not configured")]
    internal class ClientIpV6AddressTest : HazelcastTestSupport
    {
        [SetUp]
        public void Setup()
        {
            _remoteController = CreateRemoteController();
            var configStr = Resources.hazelcast_ipv6;
            configStr=configStr.Replace("PUBLIC_IP", GetLocalIpV6Address());

            _cluster = CreateCluster(_remoteController, configStr);
            StartMember(_remoteController, _cluster);
        }

        [TearDown]
        public void TearDown()
        {
            HazelcastClient.ShutdownAll();
            _remoteController.shutdownCluster(_cluster.Id);
            StopRemoteController(_remoteController);
        }

        private RemoteController.Client _remoteController;
        private Cluster _cluster;

        private static void AssertClientWithAddress(string address)
        {
            var client =
                new HazelcastClientFactory().CreateClient(config => { config.GetNetworkConfig().AddAddress(address); });

            var map = client.GetMap<string, string>("ipv6");
            map.Put("key", "val");
            Assert.AreEqual("val", map.Get("key"));
            map.Destroy();
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
    }
}