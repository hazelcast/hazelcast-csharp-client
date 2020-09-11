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

using System.Collections.Generic;
using Hazelcast.Clustering;
using Hazelcast.Clustering.LoadBalancing;
using Hazelcast.Messaging;
using Hazelcast.Networking;

namespace Hazelcast
{
    public partial class HazelcastOptions : IClusterOptions // ClusterName, Subscribers
    {
        /// <summary>
        /// Gets the default cluster name.
        /// </summary>
        public const string DefaultClusterName = "dev";

        /// <summary>
        /// Gets the default client name prefix.
        /// </summary>
        public const string DefaultClientNamePrefix = "hz.client_";

        /// <summary>
        /// Gets or sets the cluster name.
        /// </summary>
        public string ClusterName { get; set; } = DefaultClusterName;

        /// <summary>
        /// Gets the client name.
        /// </summary>
        /// <remarks>
        /// <para>When <c>null</c>, the client name is derived from <see cref="ClientNamePrefix"/>.</para>
        /// </remarks>
        public string ClientName { get; set; } // null by default

        /// <summary>
        /// Gets or sets the client name prefix.
        /// </summary>
        public string ClientNamePrefix
        {
            get => string.IsNullOrWhiteSpace(_clientNamePrefix) ? DefaultClientNamePrefix : _clientNamePrefix;
            set => _clientNamePrefix = value;
        }

        /// <summary>
        /// Gets the client labels.
        /// </summary>
        public ISet<string> Labels { get; } = new HashSet<string>();

        /// <summary>
        /// Gets the authentication options.
        /// </summary>
        public AuthenticationOptions Authentication { get; } = new AuthenticationOptions();

        /// <summary>
        /// Gets the load balancing options.
        /// </summary>
        public LoadBalancingOptions LoadBalancing { get; } = new LoadBalancingOptions();

        /// <summary>
        /// Gets the heartbeat options.
        /// </summary>
        public HeartbeatOptions Heartbeat { get; } = new HeartbeatOptions();

        /// <summary>
        /// Gets the messaging options.
        /// </summary>
        public MessagingOptions Messaging { get; } = new MessagingOptions();

        /// <summary>
        /// Gets the networking options.
        /// </summary>
        public NetworkingOptions Networking { get; } = new NetworkingOptions();
    }
}
