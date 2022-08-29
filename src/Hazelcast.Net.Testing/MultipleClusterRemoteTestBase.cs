// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Testing.Remote;
using NUnit.Framework;

namespace Hazelcast.Testing
{
    // TODO: merge this in the FailoverTests, does not belong here
    internal class MultipleClusterRemoteTestBase : RemoteTestBase
    {
        [SetUp]
        public async Task ClusterOneTimeSetUp()
        {
            // create the RC client
            RcClient = await ConnectToRemoteControllerAsync().CfAwait();

            // create clusters
            RcClusterPrimary = await RcClient.CreateClusterAsync(RcClusterConfiguration).CfAwait();
            RcClusterAlternative = await RcClient.CreateClusterAsync(RcAlternativeClusterConfiguration).CfAwait();
            RcClusterPartition = await RcClient.CreateClusterAsync(RcPartitionClusterConfiguration).CfAwait();
        }

        [TearDown]
        public async Task ClusterOneTimeTearDown()
        {
            // terminate & remove client and cluster
            if (RcClient != null)
            {
                if (RcClusterPrimary != null)
                    await RcClient.ShutdownClusterAsync(RcClusterPrimary).CfAwait();

                if (RcClusterAlternative != null)
                    await RcClient.ShutdownClusterAsync(RcClusterAlternative).CfAwait();

                if (RcClusterPartition != null)
                    await RcClient.ShutdownClusterAsync(RcClusterPartition).CfAwait();

                await RcClient.ExitAsync().CfAwait();
            }
        }

        /// <inheritdoc />
        protected override HazelcastOptions CreateHazelcastOptions()
        {
            var options = base.CreateHazelcastOptions();
            var clusterOptions = (IClusterOptions) options;
            clusterOptions.ClusterName = RcClusterPrimary?.Id ?? clusterOptions.ClusterName;
            return options;
        }

        /// <summary>
        /// Gets the primary cluster configuration.
        /// </summary>
        protected virtual string RcClusterConfiguration => Resources.hazelcast;

        /// <summary>
        /// Gets an alternative cluster configuration with username and password authentication.
        /// </summary>
        protected virtual string RcAlternativeClusterConfiguration => Resources.alternative;

        /// <summary>
        /// Gets an alternative cluster configuration with a different number of partitions.
        /// </summary>
        protected virtual string RcPartitionClusterConfiguration => Resources.partition;

        /// <summary>
        /// Gets the remote controller client.
        /// </summary>
        protected IRemoteControllerClient RcClient { get; private set; }

        /// <summary>
        /// Gets the primary cluster.
        /// </summary>
        protected Remote.Cluster RcClusterPrimary { get; private set; }

        /// <summary>
        /// Gets an alternative cluster which requires username and password authentication.
        /// </summary>
        protected Remote.Cluster RcClusterAlternative { get; private set; }

        /// <summary>
        /// Gets an alternative cluster which has a different number of partitions.
        /// </summary>
        protected Remote.Cluster RcClusterPartition { get; private set; }

        /// <summary>
        /// Kills the specified cluster members.
        /// </summary>
        protected async Task KillMembersAsync(Remote.Cluster cluster, Member[] members)
        {
            foreach (var member in members)
            {
                await RcClient.ShutdownMemberAsync(cluster.Id, member.Uuid);
            }
        }

        /// <summary>
        /// Starts members on a cluster.
        /// </summary>
        protected async Task<Member[]> StartMembersAsync(Remote.Cluster cluster, int count)
        {
            var members = new Member[count];
            for (var i = 0; i < count; i++)
            {
                members[i] = await RcClient.StartMemberAsync(cluster.Id);
            }
            return members;
        }
    }
}
