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

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Hazelcast.Networking
{

    /// <summary>
    /// Implements <see cref="IAddressProviderSource"/> using addresses provided in the <see cref="NetworkingOptions"/>.
    /// </summary>
    internal class ConfigurationAddressProviderSource : IAddressProviderSource
    {
        private readonly NetworkingOptions _networkingOptions;
        private readonly IList<string> _addresses;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationAddressProviderSource"/> class.
        /// </summary>
        /// <param name="networkingOptions">Networking options.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public ConfigurationAddressProviderSource(NetworkingOptions networkingOptions, ILoggerFactory loggerFactory)
        {
            if (networkingOptions == null) throw new ArgumentNullException(nameof(networkingOptions));

            _networkingOptions = networkingOptions;

            var addresses = networkingOptions.Addresses;
            if (addresses.Count == 0) addresses = new List<string> { "localhost" };
            _addresses = addresses;
        }

        /// <inheritdoc />
        public IDictionary<NetworkAddress, NetworkAddress> CreateInternalToPublicMap()
        {
            var addresses = new Dictionary<NetworkAddress, NetworkAddress>();
            foreach (var configurationAddressString in _addresses)
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

        /// <inheritdoc />
        public bool Maps => false;

        /// <summary>
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
