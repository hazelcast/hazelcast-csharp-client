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
        /// Gets the cluster name.
        /// </summary>
        string ClusterName { get; }

        /// <summary>
        /// Gets the client name.
        /// </summary>
        string ClientName { get; }

        /// <summary>
        /// Gets the client name prefix.
        /// </summary>
        string ClientNamePrefix { get; }

        /// <summary>
        /// Gets the client labels.
        /// </summary>
        ISet<string> Labels { get; }

        /// <summary>
        /// Gets the service factory for <see cref="ILoadBalancer"/>.
        /// </summary>
        /// <returns>The service factory for <see cref="ILoadBalancer"/>.</returns>
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
