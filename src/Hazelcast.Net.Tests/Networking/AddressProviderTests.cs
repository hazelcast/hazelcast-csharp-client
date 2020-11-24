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
using System.Linq;
using Hazelcast.Configuration;
using Hazelcast.Networking;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Hazelcast.Tests.Networking
{
    [TestFixture]
    public class AddressProviderTests
    {
        [Test]
        public void Test()
        {
            var options = new NetworkingOptions();
            var loggerFactory = new NullLoggerFactory();

            var addressProvider = new AddressProvider(options, loggerFactory);

            Assert.That(addressProvider.GetAddresses().Count(), Is.EqualTo(3));
            Assert.That(addressProvider.GetAddresses(), Does.Contain(new NetworkAddress("127.0.0.1", 5701)));
            Assert.That(addressProvider.GetAddresses(), Does.Contain(new NetworkAddress("127.0.0.1", 5702)));
            Assert.That(addressProvider.GetAddresses(), Does.Contain(new NetworkAddress("127.0.0.1", 5703)));

            options.Addresses.Clear();
            options.Addresses.Add("##??##");

            addressProvider = new AddressProvider(options, loggerFactory);

            Assert.Throws<FormatException>(() => _ = addressProvider.GetAddresses());

            options.Addresses.Clear();
            options.Addresses.Add("192.0.0.1:5701");
            options.Addresses.Add("192.0.0.1:5702");
            options.Addresses.Add("192.0.0.1:5703");

            addressProvider = new AddressProvider(options, loggerFactory);

            Assert.That(addressProvider.GetAddresses().Count(), Is.EqualTo(3));
            Assert.That(addressProvider.GetAddresses(), Does.Contain(new NetworkAddress("192.0.0.1", 5701)));
            Assert.That(addressProvider.GetAddresses(), Does.Contain(new NetworkAddress("192.0.0.1", 5702)));
            Assert.That(addressProvider.GetAddresses(), Does.Contain(new NetworkAddress("192.0.0.1", 5703)));

            var address = new NetworkAddress("192.0.0.1");
            Assert.That(addressProvider.Map(address), Is.SameAs(address));

            address = new NetworkAddress("192.168.0.4");
            Assert.That(addressProvider.Map(address), Is.SameAs(address));
        }

        [Test]
        public void ArgumentExceptions()
        {
            Assert.Throws<ArgumentNullException>(() => _ = new AddressProvider(null, new NullLoggerFactory()));
            Assert.Throws<ArgumentNullException>(() => _ = new AddressProvider(new NetworkingOptions(), null));
        }

        [Test]
        public void Cloud()
        {
            var options = new NetworkingOptions();
            var loggerFactory = new NullLoggerFactory();

            options.Addresses.Clear();

            options.Cloud.Enabled = true;

            options.Cloud.DiscoveryToken = null;
            Assert.Throws<ArgumentException>(() => _ = new AddressProvider(options, loggerFactory));
            options.Cloud.DiscoveryToken = "*****";

            options.Cloud.UrlBase = null;
            Assert.Throws<ArgumentNullException>(() => _ = new AddressProvider(options, loggerFactory));
            options.Cloud.UrlBase = new Uri("http://xxxxx");

            options.Addresses.Add("192.0.0.1:5701");
            Assert.Throws<ConfigurationException>(() => _ = new AddressProvider(options, loggerFactory));

            options.Addresses.Clear();

            CloudDiscovery.SetResponse(@"[
    { ""private-address"":""192.0.0.7"", ""public-address"":""192.147.0.7"" },
    { ""private-address"":""192.0.0.8"", ""public-address"":""192.147.0.8"" },
    { ""private-address"":""192.0.0.9"", ""public-address"":""192.147.0.9:5707"" },
]");

            try
            {
                var addressProvider = new AddressProvider(options, loggerFactory);

                Assert.That(addressProvider.Map(new NetworkAddress("192.0.0.7")), Is.EqualTo(new NetworkAddress("192.147.0.7")));
                Assert.That(addressProvider.Map(new NetworkAddress("192.0.0.8")), Is.EqualTo(new NetworkAddress("192.147.0.8")));
                Assert.That(addressProvider.Map(new NetworkAddress("192.0.0.9", 5707)), Is.EqualTo(new NetworkAddress("192.147.0.9", 5707)));
                Assert.That(addressProvider.Map(new NetworkAddress("192.0.0.10")), Is.Null);

                Assert.That(addressProvider.GetAddresses().Count(), Is.EqualTo(3));
                Assert.That(addressProvider.GetAddresses(), Does.Contain(new NetworkAddress("192.0.0.7")));
                Assert.That(addressProvider.GetAddresses(), Does.Contain(new NetworkAddress("192.0.0.8")));
                Assert.That(addressProvider.GetAddresses(), Does.Contain(new NetworkAddress("192.0.0.9", 5707)));

                addressProvider = new AddressProvider(options, loggerFactory);
                Assert.That(addressProvider.Map(new NetworkAddress("192.0.0.10")), Is.Null);
                Assert.That(addressProvider.Map(new NetworkAddress("192.0.0.10")), Is.Null);
            }
            finally
            {
                CloudDiscovery.SetResponse(null);
            }
        }
    }
}
