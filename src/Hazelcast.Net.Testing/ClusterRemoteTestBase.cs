// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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

using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Testing.Conditions;
using Hazelcast.Testing.Remote;
using NUnit.Framework;

namespace Hazelcast.Testing
{
    /// <summary>
    /// Provides a base class for Hazelcast tests that require a remote environment with a cluster.
    /// </summary>
    public abstract class ClusterRemoteTestBase : RemoteTestBase
    {
        [OneTimeSetUp]
        public virtual async Task ClusterOneTimeSetUp()
        {
            // create remote client and cluster
            RcClient = await ConnectToRemoteControllerAsync().CfAwait();
            RcCluster = KeepClusterName
                ? await RcClient.CreateClusterKeepClusterNameAsync(ServerVersion.DefaultVersion.Version.ToString(), RcClusterConfiguration)
                : await RcClient.CreateClusterAsync(RcClusterConfiguration).CfAwait();
        }


        [OneTimeTearDown]
        public virtual async Task ClusterOneTimeTearDown()
        {
            // terminate & remove client and cluster
            if (RcClient != null)
            {
                if (RcCluster != null)
                    await RcClient.ShutdownClusterAsync(RcCluster).CfAwait();
                await RcClient.ExitAsync().CfAwait();
            }
        }

        /// <inheritdoc />
        protected override HazelcastOptions CreateHazelcastOptions()
        {
            var options = base.CreateHazelcastOptions();
            var clusterOptions = (IClusterOptions) options;
            clusterOptions.ClusterName = RcCluster?.Id ?? clusterOptions.ClusterName;
            return options;
        }

        /// <summary>
        /// Gets the remote cluster configuration.
        /// </summary>
        protected virtual string RcClusterConfiguration => Resources.hazelcast;

        /// <summary>
        /// Gets the remote controller client.
        /// </summary>
        protected IRemoteControllerClient RcClient { get; set; }

        /// <summary>
        /// Gets the remote controller cluster.
        /// </summary>
        protected Remote.Cluster RcCluster { get; set; }
        
        /// <summary>
        /// Whether creates the cluster with name provided in the config.
        /// </summary>
        protected virtual bool KeepClusterName { get; set; }
    }
}
