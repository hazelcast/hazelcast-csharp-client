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
using Hazelcast.Clustering;
using Hazelcast.Clustering.LoadBalancing;
using Hazelcast.Logging;
using Hazelcast.NearCaching;
using Hazelcast.Networking;
using Hazelcast.Serialization;

namespace Hazelcast
{
    /// <summary>
    /// Main options to setup a Hazelcast Client.
    /// </summary>
    public sealed partial class HazelcastOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastOptions"/> class.
        /// </summary>
        public HazelcastOptions()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastOptions"/> class.
        /// </summary>
        /// <param name="serviceProvider">A service provider.</param>
        internal HazelcastOptions(IServiceProvider serviceProvider)
        { }

        /// <summary>
        /// Gets or sets the service provider.
        /// </summary>
        internal IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Gets the client name.
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// Gets or sets the client properties.
        /// </summary>
        [Obsolete] // what's the use now?
        public IDictionary<string, string> Properties { get; private set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the client labels.
        /// </summary>
        public ISet<string> Labels { get; private set; } = new HashSet<string>();

        /// <summary>
        /// Whether to start the client asynchronously.
        /// </summary>
        // TODO: should AsyncStart still exist (and what would it mean)?
        public bool AsyncStart { get; set; }

        /// <summary>
        /// Gets or sets the cluster options.
        /// </summary>
        public ClusterOptions Cluster { get; private set; } = new ClusterOptions();

        /// <summary>
        /// Gets the logging options.
        /// </summary>
        public LoggingOptions Logging { get; private set; } = new LoggingOptions();

        /// <summary>
        /// Gets or sets the networking options.
        /// </summary>
        public NetworkingOptions Networking { get; private set; } = new NetworkingOptions();

        /// <summary>
        /// Gets or sets the security options.
        /// </summary>
        public SecurityOptions Security { get; private set; } = new SecurityOptions();

        /// <summary>
        /// Gets or sets the load balancing options.
        /// </summary>
        public LoadBalancingOptions LoadBalancing { get; private set; } = new LoadBalancingOptions();

        /// <summary>
        /// Gets the serialization options.
        /// </summary>
        public SerializationOptions Serialization { get; private set; } = new SerializationOptions();

        /// <summary>
        /// Gets or sets the NearCache options.
        /// </summary>
        public NearCacheOptions NearCache { get; private set; } = new NearCacheOptions();

        /// <summary>
        /// Clones the options.
        /// </summary>
        public HazelcastOptions Clone()
        {
            return new HazelcastOptions
            {
                ClientName = ClientName,
                Properties = new Dictionary<string, string>(Properties),
                Labels = new HashSet<string>(Labels),
                AsyncStart = AsyncStart,
                Cluster = Cluster.Clone(),
                Logging = Logging.Clone(),
                Networking = Networking.Clone(),
                Security = Security.Clone(),
                LoadBalancing = LoadBalancing.Clone(),
                Serialization = Serialization.Clone(),
                NearCache = NearCache.Clone()
            };
        }
    }
}
