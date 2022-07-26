// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Exceptions;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Networking
{
    /// <summary>
    /// Provides addresses to connect to a cluster.
    /// </summary>
    /// <remarks>
    /// <para>The addresses can either come from configuration, or from a discovery
    /// service such as the Cloud Discovery service. They can be retrieved, in order to
    /// establish the very first to the cluster, via <see cref="GetAddresses"/>.</para>
    /// <para>When using a discovery service, it may be that the members are only
    /// aware of their own internal address, but the discovery service knows that they
    /// can only be reached through their public address. In which case, the address
    /// provider also provides a map from internal to public. When receiving members
    /// through the members view event, this map can be used to assign public addresses
    /// to members.</para>
    /// </remarks>
    internal class AddressProvider
    {
        private IAddressProviderSource _source;
        private readonly ILogger _logger;

        // maps internal addresses to public addresses
        private IDictionary<NetworkAddress, NetworkAddress> _internalToPublicMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressProvider"/> class.
        /// </summary>
        /// <param name="networkingOptions">The networking configuration.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public AddressProvider(IAddressProviderSource source, ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            _source = source ?? throw new ArgumentNullException(nameof(source));
            _logger = loggerFactory.CreateLogger<AddressProvider>();
        }

        /// <summary>
        /// Obtains the <see cref="IAddressProviderSource"/> as per configuration.
        /// </summary>
        /// <param name="networkingOptions">The networking options.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        /// <returns>The <see cref="IAddressProviderSource"/> corresponding the the configuration.</returns>
        public static IAddressProviderSource GetSource(NetworkingOptions networkingOptions, ILoggerFactory loggerFactory)
        {
            if (networkingOptions.Cloud.Enabled)
            {
                if (networkingOptions.Addresses.Count > 0)
                    throw new ConfigurationException("Only one address configuration method can be enabled at a time.");
                return new CloudAddressProviderSource(networkingOptions, loggerFactory);
            }

            return new ConfigurationAddressProviderSource(networkingOptions, loggerFactory);
        }

        /// <summary>
        /// Whether the address provider has a map of internal addresses to public addresses.
        /// </summary>
        public bool HasMap => _source.Maps;

        // ensures that we have a map, returns the map + whether it's a new map
        private (bool NewMap, IDictionary<NetworkAddress, NetworkAddress> Map) EnsureMap(bool forceRenew)
        {
            if (!forceRenew && _internalToPublicMap != null) return (false, _internalToPublicMap);
            _internalToPublicMap = _source.CreateInternalToPublicMap() ?? throw new HazelcastException("Failed to obtain addresses.");
            return (true, _internalToPublicMap);
        }

        /// <summary>
        /// Gets known possible addresses for a cluster.
        /// </summary>
        /// <returns>All addresses.</returns>
        public IEnumerable<NetworkAddress> GetAddresses() => EnsureMap(true).Map.Values;

        /// <summary>
        /// Gets <see cref="IAddressProviderSource"/>
        /// </summary>
        public IAddressProviderSource AddressProviderSource
        {
            get => _source;
            internal set { 
                _source = value;
                EnsureMap(true);
            }
        }

        /// <summary>
        /// Maps an internal address to a public address.
        /// </summary>
        /// <param name="address">The private address.</param>
        /// <returns>The public address, or null if no address was found.</returns>
        public NetworkAddress Map(NetworkAddress address)
        {
            if (address == null || !HasMap)
                return address;

            var (fresh, map) = EnsureMap(false);

            // if we can map, return
            if (map.TryGetValue(address, out var publicAddress))
                return publicAddress;

            if (fresh)
            {
                // if we just created the map, no point re-creating it
                _logger.LogDebug($"Address {address} was not found in the map.");
                return null;
            }

            // otherwise, re-scan
            _logger.LogDebug($"Address {address} was not found in the map, re-scanning.");

            // if the map is not 'fresh' force-recreate the map and try again, else give up
            // TODO: throttle?
            map = EnsureMap(true).Map;

            // now try again
            if (map.TryGetValue(address, out publicAddress))
                return publicAddress;

            _logger.LogDebug($"Address {address} was not found in the map.");
            return null;
        }
    }
}
