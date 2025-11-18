// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;

namespace Hazelcast.Networking
{
    /// <summary>
    /// Defines a source of addresses for the <see cref="AddressProvider"/>.
    /// </summary>
    internal interface IAddressProviderSource
    {
      /*  /// <summary>
        /// Provides an internal-to-public addresses map.
        /// </summary>
        /// <returns>An internal-to-public addresses map.</returns>
        //IDictionary<NetworkAddress, NetworkAddress> CreateInternalToPublicMap();*/

        /// <summary>
        /// Determines whether this source maps addresses.
        /// </summary>
        bool Maps { get; }

        //IReadOnlyCollection<NetworkAddress> PrimaryAddresses { get; }

        //IReadOnlyCollection<NetworkAddress> SecondaryAddresses { get; }

        (IReadOnlyCollection<NetworkAddress> Primary, IReadOnlyCollection<NetworkAddress> Secondary) GetAddresses(bool forceRefresh);

        //NetworkAddress Map(NetworkAddress address);

        //(bool Fresh, IDictionary<NetworkAddress, NetworkAddress> Map) EnsureMap(bool forceRenew);

        bool TryMap(NetworkAddress address, bool forceRefreshMap, out NetworkAddress result, out bool freshMap);
    }
}
