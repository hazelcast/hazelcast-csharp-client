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
using System.Threading.Tasks;
using Hazelcast.Core;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Testing
{
    /// <summary>
    /// Provides a base class for Hazelcast tests that require a remote environment.
    /// </summary>
    public abstract partial class RemoteTestBase : HazelcastTestBase
    {
        /// <summary>
        /// Creates the Hazelcast options.
        /// </summary>
        protected virtual HazelcastOptions CreateHazelcastOptions()
        {
            var options = HazelcastOptions.Build();

            options.AsyncStart = false;

            options.Networking.Addresses.Clear();
            options.Networking.Addresses.Add("127.0.0.1:5701");

            //n.ReconnectMode = ReconnectMode.ReconnectSync;

            //var r = n.ConnectionRetry;
            //r.ClusterConnectionTimeoutMilliseconds = 60 * 1000;
            //r.InitialBackoffMilliseconds = 2 * 1000;

            options.Logging.LoggerFactory.Creator = () => LoggerFactory;

            return options;
        }

        /// <summary>
        /// Gets the timeout for creating and opening a client.
        /// </summary>
        protected virtual TimeSpan CreateOpenClientTimeout { get; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// Creates a client.
        /// </summary>
        /// <returns>A client.</returns>
        protected virtual async ValueTask<IHazelcastClient> CreateOpenClientAsync()
        {
            Logger.LogInformation("Create new client");

            var client = new HazelcastClientFactory(CreateHazelcastOptions()).CreateClient();
            await client.OpenAsync(CreateOpenClientTimeout).CAF();
            return client;
        }

        /// <summary>
        /// Creates a client.
        /// </summary>
        /// <returns>A client.</returns>
        protected virtual async ValueTask<IHazelcastClient> CreateOpenClientAsync(Action<HazelcastOptions> configure)
        {
            Logger.LogInformation("Create new client");

            var client = new HazelcastClientFactory(CreateHazelcastOptions()).CreateClient(configure);
            await client.OpenAsync(CreateOpenClientTimeout).CAF();
            return client;
        }

        /// <summary>
        /// Generates a random key that maps to a partition.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="partitionId">The identifier of the partition.</param>
        /// <returns>A random key that maps to the specified partition.</returns>
        protected object GenerateKeyForPartition(IHazelcastClient client, int partitionId)
        {
            var clientInternal = (HazelcastClient) client;

            while (true)
            {
                var randomKey = TestUtils.RandomString();
                var randomKeyData = clientInternal.SerializationService.ToData(randomKey);
                if (clientInternal.Cluster.Partitioner.GetPartitionId(randomKeyData) == partitionId)
                    return randomKey;
            }
        }
    }
}
