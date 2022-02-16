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
using System.Text;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Testing;
using Hazelcast.Testing.Remote;
using NUnit.Framework;

namespace Hazelcast.Testing
{
    internal class MultipleClusterRemoteTestBase : RemoteTestBase
    {
        [OneTimeSetUp]
        public async Task ClusterOneTimeSetUp()
        {
            // create remote client and cluster
            RcClient = await ConnectToRemoteControllerAsync().CfAwait();
            RcClusterPrimary = await RcClient.CreateClusterAsync(RcClusterConfiguration).CfAwait();
            RcClusterAlternative = await RcClient.CreateClusterAsync(Remote.Resources.alternative).CfAwait();
            RcClusterPartition = await RcClient.CreateClusterAsync(Remote.Resources.partition).CfAwait();
        }

        [OneTimeTearDown]
        public async Task ClusterOneTimeTearDown()
        {
            // terminate & remove client and cluster
            if (RcClient != null)
            {
                if (RcClusterPrimary != null)
                    await RcClient.ShutdownClusterAsync(RcClusterPrimary).CfAwait();

                if (RcClusterAlternative != null)
                    await RcClient.ShutdownClusterAsync(RcClusterAlternative).CfAwait();

                await RcClient.ExitAsync().CfAwait();
            }
        }

        /// <inheritdoc />
        protected override HazelcastOptions CreateHazelcastOptions()
        {
            var options = base.CreateHazelcastOptions();
            var clusterOptions = (IClusterOptions)options;
            clusterOptions.ClusterName = RcClusterPrimary?.Id ?? clusterOptions.ClusterName;
            return options;
        }

        /// <summary>
        /// Gets the remote cluster configuration.
        /// </summary>
        protected virtual string RcClusterConfiguration => Remote.Resources.hazelcast;

        /// <summary>
        /// Gets the alternative remote cluster configuration.
        /// </summary>
        protected virtual string RcAlternativeClusterConfiguration => Remote.Resources.alternative;

        /// <summary>
        /// Cluster has 277 partion
        /// </summary>
        protected virtual string RcPartitionClusterConfiguration => Remote.Resources.partition;

        /// <summary>
        /// Gets the remote controller client.
        /// </summary>
        protected Remote.IRemoteControllerClient RcClient { get; private set; }

        /// <summary>
        /// Gets the remote controller cluster.
        /// </summary>
        protected Remote.Cluster RcClusterPrimary { get; private set; }

        /// <summary>
        /// Uses username password authentication
        /// </summary>
        protected Remote.Cluster RcClusterAlternative { get; private set; }

        protected Remote.Cluster RcClusterPartition { get; private set; }

        /// <summary>
        /// Kills given member list on given cluster
        /// </summary>
        /// <param name="clusterID"></param>
        /// <param name="members"></param>
        /// <returns></returns>
        protected async Task KillMembersOnAsync(string clusterID, Member[] members)
        {
            foreach (var member in members)
            {
                await RcClient.ShutdownMemberAsync(clusterID, member.Uuid);
            }
        }

        /// <summary>
        /// Starts number of members on given cluster
        /// </summary>
        /// <param name="clusterId"></param>
        /// <param name="numberOfMembers"></param>
        /// <returns></returns>
        protected async Task<Member[]> StartMembersOn(string clusterId, int numberOfMembers)
        {
            Member[] members = new Member[numberOfMembers];
            for (int i = 0; i < numberOfMembers; i++)
            {
                members[i] = await RcClient.StartMemberAsync(clusterId);
            }

            return members;
        }
    }
}
