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
using Hazelcast.Clustering.LoadBalancing;
using Hazelcast.Core;
using Hazelcast.Events;
using Hazelcast.Messaging;
using Hazelcast.Networking;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Represents cluster-level options.
    /// </summary>
    internal interface IClusterOptions
    {
        /// <summary>
        /// Gets or sets the cluster name.
        /// </summary>
        string ClusterName { get; set; }

        /// <summary>
        /// Gets or sets the client name.
        /// </summary>
        string ClientName { get; set; }

        /// <summary>
        /// Gets or sets the client name prefix.
        /// </summary>
        string ClientNamePrefix { get; set; }

        /// <summary>
        /// Gets or sets the delay to pause for when looking for a client
        /// to handle cluster view events and no client is available.
        /// </summary>
        /// <remarks>
        /// <para>In some situations the cluster needs a connection: to subscribe to cluster-level
        /// events (members view, partitions view), to begin a transaction... It then invokes
        /// <see cref="ClusterMembers.WaitRandomConnection"/> which will try to return an available
        /// connection, else will wait for <see cref="WaitForConnectionMilliseconds"/> before retrying.</para>
        /// </remarks>
        int WaitForConnectionMilliseconds { get; set; }

        /// <summary>
        /// Gets the client labels.
        /// </summary>
        ISet<string> Labels { get; }

        /// <summary>
        /// Gets the service factory for <see cref="ILoadBalancer"/>.
        /// </summary>
        /// <returns>The service factory for <see cref="ILoadBalancer"/>.</returns>
        /// <remarks>
        /// <para>Load balancing determines how the Hazelcast client selects the member to
        /// talk to, when it could talk to any member. By default it uses a round-robin
        /// mechanism (see <see cref="RoundRobinLoadBalancer"/>), but it can also be random
        /// (see <see cref="RandomLoadBalancer"/>) or static (see <see cref="StaticLoadBalancer"/>)
        /// or any type that implements <see cref="ILoadBalancer"/>. In the configuration file,
        /// the short names <c>roundrobin</c>, <c>random</c> and <c>static</c> can be used
        /// in place of the full type name.</para>
        /// </remarks>
        SingletonServiceFactory<ILoadBalancer> LoadBalancer { get; }

        /// <summary>
        /// Gets the authentication options.
        /// </summary>
        AuthenticationOptions Authentication { get; }

        /// <summary>
        /// Gets the heartbeat options.
        /// </summary>
        HeartbeatOptions Heartbeat { get; }

        /// <summary>
        /// Gets the messaging options.
        /// </summary>
        MessagingOptions Messaging { get; }

        /// <summary>
        /// Gets the networking options.
        /// </summary>
        NetworkingOptions Networking { get; }

        /// <summary>
        /// Gets the events options.
        /// </summary>
        EventsOptions Events { get; }
    }
}
