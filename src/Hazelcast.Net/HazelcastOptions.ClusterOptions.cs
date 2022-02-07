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
using Hazelcast.Clustering;
using Hazelcast.Clustering.LoadBalancing;
using Hazelcast.Configuration.Binding;
using Hazelcast.Core;
using Hazelcast.Events;
using Hazelcast.Exceptions;
using Hazelcast.Messaging;
using Hazelcast.Networking;

namespace Hazelcast
{
    // beware! the IClusterOptions is internal, and therefore its /// documentation
    // is NOT inherited = do NOT use <inheritdoc /> here but DO duplicate the docs

    public partial class HazelcastOptions : LoadBalancerOptions, IHazelcastOptions // ClusterName, Subscribers
    {
        private string _clientNamePrefix;

        /// <summary>
        /// Gets the default cluster name, which is <c>"dev"</c>.
        /// </summary>
        /// <returns>The default cluster name, which is <c>"dev"</c>.</returns>
        internal const string DefaultClusterName = "dev";

        /// <summary>
        /// Gets the default client name prefix, which is <c>"hz.client_"</c>.
        /// </summary>
        /// <returns>The default client name prefix, which is <c>"hz.client_"</c>.</returns>
        internal const string DefaultClientNamePrefix = "hz.client_";

        /// <summary>
        /// Gets or sets the name of the cluster.
        /// </summary>
        /// <remarks>
        /// <para>This must match the name of the cluster that the client is going to connect to.</para>
        /// </remarks>
        public string ClusterName { get; set; } = DefaultClusterName;

        /// <summary>
        /// Gets or sets the name of the client.
        /// </summary>
        /// <remarks>
        /// <para>This is optional. If no client name is specified, a name will be generated.</para>
        /// </remarks>
        public string ClientName { get; set; } // null by default

        /// <inheritdoc />
        string IClientOptions.ClientNamePrefix
        {
            get => string.IsNullOrWhiteSpace(_clientNamePrefix) ? DefaultClientNamePrefix : _clientNamePrefix;
            set => _clientNamePrefix = value;
        }

        /// <inheritdoc />
        int IClusterOptions.WaitForConnectionMilliseconds { get; set; } = 1_000;

        /// <summary>
        /// Gets the set of client labels.
        /// </summary>
        public ISet<string> Labels { get; } = new HashSet<string>();

        /// <summary>
        /// Gets the <see cref="AuthenticationOptions"/>.
        /// </summary>
        public AuthenticationOptions Authentication { get; } = new AuthenticationOptions();

        /// <summary>
        /// Gets the <see cref="HeartbeatOptions"/>.
        /// </summary>
        public HeartbeatOptions Heartbeat { get; } = new HeartbeatOptions();

        /// <summary>
        /// Gets the <see cref="MessagingOptions"/>.
        /// </summary>
        public MessagingOptions Messaging { get; } // initialized in ctor

        /// <summary>
        /// Gets the <see cref="NetworkingOptions"/>.
        /// </summary>
        public NetworkingOptions Networking { get; } // initialized in ctor

        /// <summary>
        /// Gets the <see cref="EventsOptions"/>.
        /// </summary>
        public EventsOptions Events { get; } = new EventsOptions();

        /// <summary>
        /// Gets the <see cref="FailoverOptions"/>
        /// </summary>
        public FailoverOptions Failover { get; } = new FailoverOptions();
    }
}
