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
using Hazelcast.Networking;
using Hazelcast.Security;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Defines cluster options.
    /// </summary>
    public interface IClusterOptions : IHaveSecurityOptions, IHaveNetworkingOptions
    {
        /// <summary>
        /// Gets or sets the cluster name.
        /// </summary>
        string ClusterName { get; set; }

        /// <summary>
        /// Gets the client labels.
        /// </summary>
        ISet<string> Labels { get; }

        /// <summary>
        /// Gets the subscribers.
        /// </summary>
        IList<IClusterEventSubscriber> Subscribers { get; }

        /// <summary>
        /// Gets the authentication options.
        /// </summary>
        AuthenticationOptions Authentication { get; }

        /// <summary>
        /// Gets the load balancing options.
        /// </summary>
        LoadBalancingOptions LoadBalancer { get; }
    }
}
