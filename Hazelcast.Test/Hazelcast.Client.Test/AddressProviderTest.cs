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

using System.Linq;
using Hazelcast.Client.Network;
using Hazelcast.Config;
using Hazelcast.IO;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class AddressProviderTest
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void TestConfigAddressProvider()
        {
            var cfg = new ClientConfig();
            cfg.GetNetworkConfig().AddAddress("10.0.0.1:5701", "10.0.0.2:5702", "10.0.0.3:5703");

            var addressProvider = new AddressProvider(cfg);
            var addresses = addressProvider.GetAddresses().ToList();
            Assert.AreEqual(addresses[0], new Address("10.0.0.1", 5701));
            Assert.AreEqual(addresses[1], new Address("10.0.0.2", 5702));
            Assert.AreEqual(addresses[2], new Address("10.0.0.3", 5703));
        }

        [Test]
        public void TestConfigAddressProvider_emptyAddress()
        {
            var cfg = new ClientConfig();

            var addressProvider = new AddressProvider(cfg);
            var addresses = addressProvider.GetAddresses().ToList();
            Assert.AreEqual(addresses[0], new Address("localhost", 5701));
            Assert.AreEqual(addresses[1], new Address("localhost", 5702));
            Assert.AreEqual(addresses[2], new Address("localhost", 5703));
        }

        [Test]
        public void TestMultipleAddressProvider()
        {
            var cfg = new ClientConfig();
            cfg.GetNetworkConfig().AddAddress("10.0.0.1:5701", "10.0.0.2:5702", "10.0.0.3:5703");
            cfg.GetNetworkConfig().GetCloudConfig().SetEnabled(true).SetDiscoveryToken("TOKEN");

            Assert.Catch<InvalidConfigurationException>(() =>
            {
                var addressProvider = new AddressProvider(cfg);
                addressProvider.GetAddresses();
            });
        }
    }
}