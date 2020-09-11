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
using System.Collections.Generic;
using Hazelcast.Configuration;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Networking
{
    internal class AddressProvider
    {
        private readonly Func<IDictionary<NetworkAddress, NetworkAddress>> _createMap;
        private readonly IList<string> _configurationAddresses;

        private IDictionary<NetworkAddress, NetworkAddress> _privateToPublic;
        private readonly bool _isMapping;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressProvider"/> class.
        /// </summary>
        /// <param name="networkingOptions">The networking configuration.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public AddressProvider(NetworkingOptions networkingOptions, ILoggerFactory loggerFactory)
        {
            if(networkingOptions == null) throw new ArgumentNullException(nameof(networkingOptions));
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            var cloudConfiguration = networkingOptions.Cloud;
            var addresses = networkingOptions.Addresses;

            if (cloudConfiguration != null && cloudConfiguration.Enabled)
            {
                // fail fast
                if (addresses.Count > 0)
                    throw new ConfigurationException("Only one address configuration method can be enabled at a time.");

                // initialize cloud discovery
                var token = cloudConfiguration.DiscoveryToken;
                var urlBase = cloudConfiguration.UrlBase;
                var connectionTimeoutMilliseconds = networkingOptions.ConnectionTimeoutMilliseconds;
                connectionTimeoutMilliseconds = connectionTimeoutMilliseconds == 0 ? int.MaxValue : connectionTimeoutMilliseconds;
                var cloudScanner = new CloudDiscovery(token, connectionTimeoutMilliseconds, urlBase, loggerFactory);

                _createMap = () => cloudScanner.Scan();
                _isMapping = true;
            }
            else
            {
                _configurationAddresses = addresses.Count > 0 ? addresses : new List<string> { "localhost" };
                _createMap = CreateMapFromConfiguration;
            }
        }

        /// <summary>
        /// Gets known possible addresses for a cluster.
        /// </summary>
        /// <returns>All addresses.</returns>
        public IEnumerable<NetworkAddress> GetAddresses()
        {
            return (_privateToPublic ??= _createMap()).Keys;
        }

        /// <summary>
        /// Maps a private address to a public address.
        /// </summary>
        /// <param name="address">The private address.</param>
        /// <returns>The public address, or null if no address was found.</returns>
        public NetworkAddress Map(NetworkAddress address)
        {
            if (address == null || !_isMapping)
                return address;

            var fresh = false;
            if (_privateToPublic == null)
            {
                _privateToPublic = _createMap();
                fresh = true;
            }

            // if we can map, return
            if (_privateToPublic.TryGetValue(address, out var publicAddress))
                return publicAddress;

            // if the map is not 'fresh' recreate the map and try again, else give up
            // TODO: throttle?
            if (fresh) return null;
            _privateToPublic = _createMap();

            return _privateToPublic.TryGetValue(address, out publicAddress) ? publicAddress : null;
        }

        private IDictionary<NetworkAddress, NetworkAddress> CreateMapFromConfiguration()
        {
            var addresses = new Dictionary<NetworkAddress, NetworkAddress>();
            foreach (var configurationAddress in _configurationAddresses)
            {
                if (!NetworkAddress.TryParse(configurationAddress, out IEnumerable<NetworkAddress> networkAddresses))
                    throw new FormatException($"The string \"{configurationAddress}\" does not represent a valid network address.");

                foreach (var networkAddress in networkAddresses)
                    addresses.Add(networkAddress, networkAddress);
            }

            return addresses;
        }
    }
}
