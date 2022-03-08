﻿// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Hazelcast.Configuration;
using Hazelcast.Exceptions;
using Hazelcast.Networking;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Hazelcast.Tests.Networking
{
    [TestFixture]
    public class AddressProviderTests
    {
        private class TestAddressProviderSource : IAddressProviderSource
        {
            private readonly Func<IDictionary<NetworkAddress, NetworkAddress>> _createInternalToPublicMap;

            public TestAddressProviderSource(Func<IDictionary<NetworkAddress, NetworkAddress>> createInternalToPublicMap = null)
            {
                _createInternalToPublicMap = createInternalToPublicMap;
            }

            public IDictionary<NetworkAddress, NetworkAddress> CreateInternalToPublicMap()
            {
                if (_createInternalToPublicMap == null)
                    throw new InvalidOperationException("Cannot create a map.");

                return _createInternalToPublicMap();
            }

            public bool Maps => true;
        }

        [Test]
        public void Test()
        {
            var options = new NetworkingOptions();
            var loggerFactory = new NullLoggerFactory();

            var addressProviderSource = new ConfigurationAddressProviderSource(options, loggerFactory);
            var addressProvider = new AddressProvider(addressProviderSource, loggerFactory);

            Assert.That(addressProvider.GetAddresses().Count(), Is.EqualTo(3));
            Assert.That(addressProvider.GetAddresses(), Does.Contain(new NetworkAddress("127.0.0.1", 5701)));
            Assert.That(addressProvider.GetAddresses(), Does.Contain(new NetworkAddress("127.0.0.1", 5702)));
            Assert.That(addressProvider.GetAddresses(), Does.Contain(new NetworkAddress("127.0.0.1", 5703)));

            options.Addresses.Clear();
            options.Addresses.Add("##??##");

            addressProviderSource = new ConfigurationAddressProviderSource(options, loggerFactory);
            addressProvider = new AddressProvider(addressProviderSource, loggerFactory);

            Assert.Throws<FormatException>(() => _ = addressProvider.GetAddresses());

            options.Addresses.Clear();
            options.Addresses.Add("192.0.0.1:5701");
            options.Addresses.Add("192.0.0.1:5702");
            options.Addresses.Add("192.0.0.1:5703");

            addressProviderSource = new ConfigurationAddressProviderSource(options, loggerFactory);
            addressProvider = new AddressProvider(addressProviderSource, loggerFactory);

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
            Assert.Throws<ArgumentNullException>(() => _ = new AddressProvider(new TestAddressProviderSource(), null));
        }

        [Test]
        public void Cloud()
        {
            var options = new NetworkingOptions();
            var loggerFactory = new NullLoggerFactory();

            options.Addresses.Clear();

            options.Cloud.DiscoveryToken = null;
            Assert.That(options.Cloud.Enabled, Is.False);

            options.Cloud.DiscoveryToken = "*****";
            Assert.That(options.Cloud.Enabled, Is.True);

            options.Cloud.Url = null;
            Assert.Throws<ArgumentNullException>(() => _ = new CloudAddressProviderSource(options, loggerFactory));
            options.Cloud.Url = new Uri("http://xxxxx");

            options.Addresses.Add("192.0.0.1:5701");
            Assert.Throws<ConfigurationException>(() => _ = AddressProvider.GetSource(options, loggerFactory));

            options.Addresses.Clear();

            CloudDiscovery.SetResponse(@"[
    { ""private-address"":""192.0.0.6:5788"", ""public-address"":""192.147.0.6"" },
    { ""private-address"":""192.0.0.7"", ""public-address"":""192.147.0.7"" },
    { ""private-address"":""192.0.0.8:5777"", ""public-address"":""192.147.0.8:5703"" },
    { ""private-address"":""192.0.0.9"", ""public-address"":""192.147.0.9:5707"" },
]");

            static void AssertMap(AddressProvider ap, string priv, string pub)
                => Assert.That(ap.Map(NetworkAddress.Parse(priv)), Is.EqualTo(NetworkAddress.Parse(pub)));

            var addressProviderSource = new CloudAddressProviderSource(options, loggerFactory);

            try
            {
                var addressProvider = new AddressProvider(addressProviderSource, loggerFactory);

                AssertMap(addressProvider, "192.0.0.6:5788", "192.147.0.6:5701");
                AssertMap(addressProvider, "192.0.0.7:5701", "192.147.0.7:5701");
                AssertMap(addressProvider, "192.0.0.8:5777", "192.147.0.8:5703");
                AssertMap(addressProvider, "192.0.0.9:5707", "192.147.0.9:5707");

                Assert.That(addressProvider.Map(new NetworkAddress("192.0.0.10")), Is.Null);

                Assert.That(addressProvider.GetAddresses().Count(), Is.EqualTo(4));
                Assert.That(addressProvider.GetAddresses(), Does.Contain(NetworkAddress.Parse("192.147.0.6:5701")));
                Assert.That(addressProvider.GetAddresses(), Does.Contain(NetworkAddress.Parse("192.147.0.7:5701")));
                Assert.That(addressProvider.GetAddresses(), Does.Contain(NetworkAddress.Parse("192.147.0.8:5703")));
                Assert.That(addressProvider.GetAddresses(), Does.Contain(NetworkAddress.Parse("192.147.0.9:5707")));

                addressProvider = new AddressProvider(addressProviderSource, loggerFactory);
                Assert.That(addressProvider.Map(new NetworkAddress("192.0.0.10")), Is.Null);
                Assert.That(addressProvider.Map(new NetworkAddress("192.0.0.10")), Is.Null);
            }
            finally
            {
                CloudDiscovery.SetResponse(null);
            }
        }

        [Test]
        public void BogusCreateMapThrows()
        {
            var options = new NetworkingOptions();
            var addressProvider = new AddressProvider(new TestAddressProviderSource(() => null), new NullLoggerFactory());

            // if the source returns null we get an exception
            Assert.Throws<HazelcastException>(() => addressProvider.GetAddresses());
        }

        [Test]
        public void CreateMapInvokedOnEachGetAddresses()
        {
            var count = 0;

            var options = new NetworkingOptions();
            var addressProviderSource = new TestAddressProviderSource(() =>
            {
                Interlocked.Increment(ref count);
                return new Dictionary<NetworkAddress, NetworkAddress>();
            });
            var addressProvider = new AddressProvider(addressProviderSource, new NullLoggerFactory());

            Assert.That(count, Is.EqualTo(0));

            var addresses1 = addressProvider.GetAddresses();
            Assert.That(count, Is.EqualTo(1));

            var addresses2 = addressProvider.GetAddresses();
            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        public void CreateMapCachedForMapping()
        {
            var count = 0;

            var options = new NetworkingOptions();
            var addressProviderSource = new TestAddressProviderSource(() =>
            {
                Interlocked.Increment(ref count);

                return new Dictionary<NetworkAddress, NetworkAddress>
                {
                    { NetworkAddress.Parse("192.168.0.1"), NetworkAddress.Parse("192.168.1.1") }
                };
            });
            var addressProvider = new AddressProvider(addressProviderSource, new NullLoggerFactory());

            Assert.That(count, Is.EqualTo(0));

            var addresses1 = addressProvider.GetAddresses();
            Assert.That(count, Is.EqualTo(1));

            var address1 = addressProvider.Map(NetworkAddress.Parse("192.168.0.1"));
            Assert.That(count, Is.EqualTo(1)); // successful Map = use cached map
        }

        [Test]
        public void CreateMapReInvokedIfMappingFails()
        {
            var count = 0;

            var options = new NetworkingOptions();
            var addressProviderSource = new TestAddressProviderSource(() =>
            {
                Interlocked.Increment(ref count);

                return new Dictionary<NetworkAddress, NetworkAddress>
                {
                    { NetworkAddress.Parse("192.168.0.1"), NetworkAddress.Parse("192.168.1.1") }
                };
            });
            var addressProvider = new AddressProvider(addressProviderSource, new NullLoggerFactory());

            Assert.That(count, Is.EqualTo(0));

            var addresses1 = addressProvider.GetAddresses();
            Assert.That(count, Is.EqualTo(1));

            var address1 = addressProvider.Map(NetworkAddress.Parse("192.168.0.2"));
            Assert.That(count, Is.EqualTo(2)); // failed Map = tried again
        }

        [Test]
        public void CreateMapInvokedOnFirstMapping()
        {
            var count = 0;

            var options = new NetworkingOptions();
            var addressProviderSource = new TestAddressProviderSource(() =>
            {
                Interlocked.Increment(ref count);

                return new Dictionary<NetworkAddress, NetworkAddress>
                {
                    { NetworkAddress.Parse("192.168.0.1"), NetworkAddress.Parse("192.168.1.1") }
                };
            });
            var addressProvider = new AddressProvider(addressProviderSource, new NullLoggerFactory());

            Assert.That(count, Is.EqualTo(0));

            var address1 = addressProvider.Map(NetworkAddress.Parse("192.168.0.1"));
            Assert.That(count, Is.EqualTo(1)); // created a map, then successful Map
        }

        [Test]
        public void CreateMapInvokedOnceOnFirstMapping()
        {
            var count = 0;

            var options = new NetworkingOptions();
            var addressProviderSource = new TestAddressProviderSource(() =>
            {
                Interlocked.Increment(ref count);

                return new Dictionary<NetworkAddress, NetworkAddress>
                {
                    { NetworkAddress.Parse("192.168.0.1"), NetworkAddress.Parse("192.168.1.1") }
                };
            });
            var addressProvider = new AddressProvider(addressProviderSource, new NullLoggerFactory());

            Assert.That(count, Is.EqualTo(0));

            var address1 = addressProvider.Map(NetworkAddress.Parse("192.168.0.2"));
            Assert.That(count, Is.EqualTo(1)); // created a map, then failed Map, no point re-creating
        }
    }
}
