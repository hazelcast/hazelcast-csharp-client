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

using Hazelcast.Clustering;
using Hazelcast.Clustering.LoadBalancing;
using Hazelcast.Logging;
using Hazelcast.Messaging;
using Hazelcast.NearCaching;
using Hazelcast.Networking;
using Hazelcast.Serialization;

namespace Hazelcast
{
    public partial class HazelcastOptions // Misc
    {
        /// <summary>
        /// Gets the client name.
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// Whether to start the client asynchronously.
        /// </summary>
        // TODO: should AsyncStart still exist (and what would it mean)?
        public bool AsyncStart { get; set; }

        /// <summary>
        /// Gets the logging options.
        /// </summary>
        public LoggingOptions Logging { get; } = new LoggingOptions();

        /// <summary>
        /// Gets the heartbeat options.
        /// </summary>
        public HeartbeatOptions Heartbeat { get; } = new HeartbeatOptions();

        /// <summary>
        /// Gets the networking options.
        /// </summary>
        public NetworkingOptions Networking { get; } = new NetworkingOptions();

        /// <summary>
        /// Gets the authentication options.
        /// </summary>
        public AuthenticationOptions Authentication { get; } = new AuthenticationOptions();

        /// <summary>
        /// Gets the load balancing options.
        /// </summary>
        public LoadBalancingOptions LoadBalancing { get; } = new LoadBalancingOptions();

        /// <summary>
        /// Gets the serialization options.
        /// </summary>
        public SerializationOptions Serialization { get; } = new SerializationOptions();

        /// <summary>
        /// Gets  the NearCache options.
        /// </summary>
        public NearCacheOptions NearCache { get; } = new NearCacheOptions();

        /// <summary>
        /// Gets the messaging options.
        /// </summary>
        public MessagingOptions Messaging { get; } = new MessagingOptions();
    }
}
