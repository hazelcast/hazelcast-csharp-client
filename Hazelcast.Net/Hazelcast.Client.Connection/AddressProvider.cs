// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Client.Spi;
using Hazelcast.Config;
using Hazelcast.IO;
using Hazelcast.Logging;
using Hazelcast.Util;

namespace Hazelcast.Client.Connection
{
    internal class AddressProvider
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof(AddressProvider));

        private readonly HazelcastCloudDiscovery _hzCloudDiscovery;

        // A function to create address translation dictionary (to convert from private address to a public one version)
        private readonly GetAddressDictionary _addressProviderFn;

        // An address list from the configuration info
        private readonly IList<string> _configAddresses;

        // Collection of the addresses divided between primary (first to try) and secondary addresses
        private readonly Addresses _prioritizedAddresses = new Addresses();

        // A dictionary to translate private address to a public one version
        private IDictionary<Address, Address> _privateToPublic;
        private readonly bool _canTranslate;


        public AddressProvider(ClientConfig clientConfig)
        {
            var networkConfig = clientConfig.GetNetworkConfig();
            var configAddressList = networkConfig.GetAddresses();
            var cloudConfig = networkConfig.GetCloudConfig();

            //Fail fast validate multiple setup
            if (configAddressList.Count > 0 && cloudConfig != null && cloudConfig.IsEnabled())
            {
                throw new ConfigurationException("Only one address configuration method can be enabled at a time.");
            }

            if (cloudConfig != null && cloudConfig.IsEnabled())
            {
                var token = cloudConfig.GetDiscoveryToken();
                var connectionTimeoutInMillis = networkConfig.GetConnectionTimeout();
                connectionTimeoutInMillis = connectionTimeoutInMillis == 0 ? int.MaxValue : connectionTimeoutInMillis;
                    
                var urlBase = Environment.GetEnvironmentVariable(HazelcastCloudDiscovery.CloudUrlBaseProperty);
                
                _hzCloudDiscovery = new HazelcastCloudDiscovery(token, connectionTimeoutInMillis, urlBase??HazelcastCloudDiscovery.CloudUrlBase);
                _addressProviderFn = GetHzCloudConfigAddresses;
                _canTranslate = true;
            }
            else
            {
                _configAddresses = configAddressList.Count > 0 ? configAddressList : new List<string>{"localhost"};
                _addressProviderFn = GetHzConfigAddresses;
            }
        }

        public Addresses GetAddresses()
        {
            if (_privateToPublic == null)
            {
                Refresh();
            }
            return _prioritizedAddresses;
        }

        private void Refresh()
        {
            try
            {
                _privateToPublic = _addressProviderFn();
            }
            catch (Exception e)
            {
                Logger.Warning("Address provider failed to load addresses: ", e);
            }
        }

        public Address TranslateToPublic(Address address)
        {
            if (!_canTranslate || address == null)
            {
                return address;
            }
            Address publicAddress;
            if (_privateToPublic != null && _privateToPublic.TryGetValue(address, out publicAddress))
            {
                return publicAddress;
            }
            Refresh();
            return _privateToPublic != null && _privateToPublic.TryGetValue(address, out publicAddress) ? publicAddress : null;
        }

        // Config address provider
        private IDictionary<Address, Address> GetHzConfigAddresses()
        {
            // Remove all the previous addresses
            _prioritizedAddresses.Clear();

            foreach (var cfgAddress in _configAddresses)
            {
                var parsedAddresses = AddressUtil.ParsePossibleAddresses(cfgAddress);
                var primaryAddress = parsedAddresses.FirstOrDefault();
                if (primaryAddress != null)
                {
                    // Add the address to the primary list
                    _prioritizedAddresses.Primary.Add(primaryAddress);

                    // Remove the primary address from the temporary list
                    parsedAddresses.Remove(primaryAddress);

                    // Add rest of the addresses to the secondary list
                    _prioritizedAddresses.Secondary.AddRange(parsedAddresses);
                }
            }

            // Construct a dictionary from obtained addresses
            var allAddresses = new Dictionary<Address, Address>();
            _prioritizedAddresses.Primary.ForEach(address=> allAddresses.Add(address, address));
            _prioritizedAddresses.Secondary.ForEach(address => allAddresses.Add(address, address));

            return allAddresses;
        }

        //Hz cloud address provider
        private IDictionary<Address, Address> GetHzCloudConfigAddresses()
        {
            // Remove all the previous addresses
            _prioritizedAddresses.Clear();

            // Obtain list of the addresses from the cloud using disco
            var result = _hzCloudDiscovery.DiscoverNodes();

            // Register new addresses
            _prioritizedAddresses.Primary.AddRange(result.Keys);

            return result;
        }
    }
}