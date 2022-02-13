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

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Hazelcast.Networking
{
    /// <summary>
    /// Implements <see cref="IAddressProviderSource"/> using <see cref="CloudDiscovery"/>.
    /// </summary>
    internal class CloudAddressProviderSource : IAddressProviderSource
    {
        private readonly CloudDiscovery _cloudDiscovery;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudAddressProviderSource"/> class.
        /// </summary>
        /// <param name="networkingOptions">Networking options.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public CloudAddressProviderSource(NetworkingOptions networkingOptions, ILoggerFactory loggerFactory)
        {
            if (networkingOptions == null) throw new ArgumentNullException(nameof(networkingOptions));
            if (!networkingOptions.Cloud.Enabled) throw new ArgumentException("Cloud is not enabled.", nameof(networkingOptions));

            var cloudOptions = networkingOptions.Cloud;
            var token = cloudOptions.DiscoveryToken;
            var urlBase = cloudOptions.Url;
            var connectionTimeoutMilliseconds = networkingOptions.ConnectionTimeoutMilliseconds;
            connectionTimeoutMilliseconds = connectionTimeoutMilliseconds == 0 ? int.MaxValue : connectionTimeoutMilliseconds;
            _cloudDiscovery = new CloudDiscovery(token, connectionTimeoutMilliseconds, urlBase, networkingOptions.DefaultPort, loggerFactory);
        }

        /// <inheritdoc />
        public IDictionary<NetworkAddress, NetworkAddress> CreateInternalToPublicMap()
            => _cloudDiscovery.Scan();

        /// <inheritdoc />
        public bool Maps => true;
    }
}
