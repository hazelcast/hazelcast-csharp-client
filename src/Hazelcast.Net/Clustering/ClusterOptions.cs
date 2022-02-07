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

using Hazelcast.Clustering.LoadBalancing;
using Hazelcast.Networking;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Represents cluster-level options. <see cref"LoadBalancerOptions"/> absrtact class provides the creation instance functionalities of <see cref"LoadBalancer"/>
    /// </summary>
    public class ClusterOptions : LoadBalancerOptions, IClusterOptions
    {
        /// <summary>
        /// Initializes <see cref="ClusterOptions"/>
        /// </summary>
        public ClusterOptions()
        {
        }

        private ClusterOptions(ClusterOptions options)
        {
            ClusterName = options.ClusterName;
            WaitForConnectionMilliseconds = options.WaitForConnectionMilliseconds;
            Authentication = options.Authentication.Clone();
            Heartbeat = options.Heartbeat.Clone();
            Networking = options.Networking.Clone();            
        }

        /// <summary>
        /// Gets or sets the cluster name.
        /// </summary>
        public string ClusterName { get; set; }

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
        public int WaitForConnectionMilliseconds { get; set; }
   
        /// <summary>
        /// Gets the authentication options.
        /// </summary>
        public AuthenticationOptions Authentication { get; } = new AuthenticationOptions();

        /// <summary>
        /// Gets the heartbeat options.
        /// </summary>
        public HeartbeatOptions Heartbeat { get; } = new HeartbeatOptions();

        /// <summary>
        /// Gets the networking options.
        /// </summary>
        public NetworkingOptions Networking { get; }= new NetworkingOptions();

        /// <summary>
        /// Clones the options.
        /// </summary>
        /// <returns>A deep clone of the options.</returns>
        public ClusterOptions Clone() => new ClusterOptions(this);
    }
}
