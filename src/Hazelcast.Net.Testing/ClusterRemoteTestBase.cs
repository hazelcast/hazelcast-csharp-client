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

using System.Threading.Tasks;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Testing
{
    /// <summary>
    /// Provides a base class for Hazelcast tests that require a remote environment with a cluster.
    /// </summary>
    public abstract class ClusterRemoteTestBase : RemoteTestBase
    {
        [OneTimeSetUp]
        public async Task ClusterOneTimeSetUp()
        {
            // create remote client and cluster
            RcClient = await CreateRemoteControllerAsync().CAF();
            RcCluster = await RcClient.CreateClusterAsync(RcClusterConfiguration).CAF();
        }

        [OneTimeTearDown]
        public async Task ClusterOneTimeTearDown()
        {
            // terminate & remove client and cluster
            if (RcClient != null)
            {
                if (RcCluster != null)
                    await RcClient.ShutdownClusterAsync(RcCluster).CAF();
                await RcClient.ExitAsync().CAF();
            }
        }

        /// <inheritdoc />
        protected override HazelcastOptions CreateHazelcastOptions()
        {
            var options = base.CreateHazelcastOptions();
            options.ClusterName = RcCluster?.Id ?? options.ClusterName;
            return options;
        }

        /// <summary>
        /// Gets the remote cluster configuration.
        /// </summary>
        protected virtual string RcClusterConfiguration => Remote.Resources.hazelcast;

        /// <summary>
        /// Gets the remote controller client.
        /// </summary>
        protected Remote.IRemoteControllerClient RcClient { get; private set; }

        /// <summary>
        /// Gets the remote controller cluster.
        /// </summary>
        protected Remote.Cluster RcCluster { get; private set; }

    }
}
