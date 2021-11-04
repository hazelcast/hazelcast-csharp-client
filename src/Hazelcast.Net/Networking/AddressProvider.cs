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
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
        private readonly Func<IDictionary<NetworkAddress, NetworkAddress>> _createMap;
        private readonly IList<string> _configurationAddresses;
        private readonly NetworkingOptions _networkingOptions;
        private readonly ILogger _logger;

        // maps internal addresses to public addresses
        private IDictionary<NetworkAddress, NetworkAddress> _internalToPublicMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressProvider"/> class.
        /// </summary>
        /// <param name="networkingOptions">The networking configuration.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public AddressProvider(NetworkingOptions networkingOptions, ILoggerFactory loggerFactory)
        {
            _networkingOptions = networkingOptions ?? throw new ArgumentNullException(nameof(networkingOptions));
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            _logger = loggerFactory.CreateLogger<AddressProvider>();

            var cloudConfiguration = networkingOptions.Cloud;
            var addresses = networkingOptions.Addresses;

            if (cloudConfiguration != null && cloudConfiguration.Enabled)
            {
                // fail fast
                if (addresses.Count > 0)
                    throw new ConfigurationException("Only one address configuration method can be enabled at a time.");

                // initialize cloud discovery
                var token = cloudConfiguration.DiscoveryToken;
                var urlBase = cloudConfiguration.Url;
                var connectionTimeoutMilliseconds = networkingOptions.ConnectionTimeoutMilliseconds;
                connectionTimeoutMilliseconds = connectionTimeoutMilliseconds == 0 ? int.MaxValue : connectionTimeoutMilliseconds;
                var cloudScanner = new CloudDiscovery(token, connectionTimeoutMilliseconds, urlBase, networkingOptions.DefaultPort, loggerFactory);

                _createMap = () => cloudScanner.Scan();
                HasMap = true;
            }
            else
            {
                _configurationAddresses = addresses.Count > 0 ? addresses : new List<string> { "localhost" };
                _createMap = CreateMapFromConfiguration;
            }
        }

        /// <summary>
        /// Whether the address provider has a map of internal addresses to public addresses.
        /// </summary>
        public bool HasMap { get; }

        /// <summary>
        /// Gets known possible addresses for a cluster.
        /// </summary>
        /// <returns>All addresses.</returns>
        public IEnumerable<NetworkAddress> GetAddresses()
        {
            _internalToPublicMap ??= _createMap() ?? throw new HazelcastException("Failed to obtain addresses.");
            return _internalToPublicMap.Values;
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

            var fresh = false;
            if (_internalToPublicMap == null)
            {
                _internalToPublicMap = _createMap() ?? throw new HazelcastException("Failed to obtain addresses.");
                fresh = true;
            }

            // if we can map, return
            if (_internalToPublicMap.TryGetValue(address, out var publicAddress))
                return publicAddress;

            if (fresh)
            {
                // if we just created the map, no point re-creating it
                _logger.LogDebug($"Address {address} was not found in the map.");
                return null;
            }

            // otherwise, re-scan
            _logger.LogDebug($"Address {address} was not found in the map, re-scanning.");

            // if the map is not 'fresh' recreate the map and try again, else give up
            // TODO: throttle?
            _internalToPublicMap = _createMap();

            if (_internalToPublicMap == null) throw new HazelcastException("Failed to obtain addresses.");

            // now try again
            if (_internalToPublicMap.TryGetValue(address, out publicAddress))
                return publicAddress;

            _logger.LogDebug($"Address {address} was not found in the map.");
            return null;
        }

        /// <summary>
        /// (internal for tests only)
        /// Creates an address map from the configuration.
        /// </summary>
        /// <returns>An address map.</returns>
        /// <remarks>When the address map is created from the configuration, keys and values
        /// are identical, since no mapping is actually required.</remarks>
        internal IDictionary<NetworkAddress, NetworkAddress> CreateMapFromConfiguration()
        {
            var addresses = new Dictionary<NetworkAddress, NetworkAddress>();
            foreach (var configurationAddressString in _configurationAddresses)
            {
                if (!NetworkAddress.TryParse(configurationAddressString, out var configurationAddress))
                    throw new FormatException($"The string \"{configurationAddressString}\" does not represent a valid network address.");

                // got to be v6 - cannot get IPAddress to parse anything that would not be v4 or v6
                //if (!address.IsIpV6)
                //    throw new NotSupportedException($"Address family {address.IPAddress.AddressFamily} is not supported.");

                // see https://4sysops.com/archives/ipv6-tutorial-part-6-site-local-addresses-and-link-local-addresses/
                // loopback - is ::1 exclusively
                // site-local - equivalent to private IP addresses in v4 = fe:c0:...
                // link-local - hosts on the link
                // global - globally route-able addresses

                IEnumerable<NetworkAddress> networkAddresses;
                if (configurationAddress.IsIpV4 || configurationAddress.IsIpV6GlobalOrScoped)
                {
                    // v4, or v6 global or has a scope = qualified, can return
                    networkAddresses = ExpandPorts(configurationAddress);
                }
                else
                {
                    // address is v6 site-local or link-local, and has no scopeId
                    // get localhost addresses
                    networkAddresses = GetV6LocalAddresses()
                        .SelectMany(x => ExpandPorts(configurationAddress, x));
                }

                foreach (var networkAddress in networkAddresses)
                    addresses.Add(networkAddress, networkAddress);
            }

            return addresses;
        }

        /// <summary>
        /// (internal for tests only)
        /// Gets all scoped IP addresses corresponding to a non-scoped IP v6 local address.
        /// </summary>
        /// <returns>All scoped IP addresses corresponding to the specified address.</returns>
        private static IEnumerable<IPAddress> GetV6LocalAddresses()
        {
            // if the address is IP v6 local without a scope,
            // resolve -> the local address, with all avail scopes?

            var hostname = HDns.GetHostName();
            var entry = HDns.GetHostEntry(hostname);
            foreach (var address in entry.AddressList)
            {
                if (address.AddressFamily == AddressFamily.InterNetworkV6)
                    yield return address;
            }
        }

        /// <summary>
        /// Expands the port of an address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="ipAddress"></param>
        /// <returns>Addresses with ports.</returns>
        /// <remarks>
        /// <para>If the <paramref name="address"/> has a specified port, this yields the address, and
        /// only the address. But if it does not have a specified port, this yields the address with
        /// different ports obtained through the <see cref="NetworkingOptions.DefaultPort"/> and
        /// <see cref="NetworkingOptions.PortRange"/> configuration options.</para>
        /// </remarks>
        private IEnumerable<NetworkAddress> ExpandPorts(NetworkAddress address, IPAddress ipAddress = null)
        {
            if (address.Port > 0)
            {
                // qualified with a port = can only be this address
                yield return address;
            }
            else
            {
                // not qualified with a port = can be a port range
                for (var port = _networkingOptions.DefaultPort;
                    port < _networkingOptions.DefaultPort + _networkingOptions.PortRange;
                    port++)
                    yield return ipAddress == null
                        ? new NetworkAddress(address, port)
                        : new NetworkAddress(address.HostName, ipAddress, port);
            }
        }
    }
}
