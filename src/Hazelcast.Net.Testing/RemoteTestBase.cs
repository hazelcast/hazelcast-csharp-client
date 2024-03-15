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

using System;
using System.Threading.Tasks;
using Hazelcast.Configuration;
using Hazelcast.Core;
using Microsoft.Extensions.Configuration;
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
            return CreateHazelcastOptionsBuilder().Build();
        }

        /// <summary>
        /// Creates the Hazelcast options builder.
        /// </summary>
        protected virtual HazelcastOptionsBuilder CreateHazelcastOptionsBuilder()
        {
            return new HazelcastOptionsBuilder()
                .ConfigureBuilder(builder =>
                {
                    builder.AddHazelcastAndDefaults(null);
                    builder.AddUserSecrets(GetType().Assembly, true);
                })
                .WithAltKey(ConfigurationSecretsKey)
                .With(options =>
                {
                    options.Networking.Addresses.Clear();
                    options.Networking.Addresses.Add("127.0.0.1:5701");
                    options.LoggerFactory.Creator = () => LoggerFactory;
                });
        }

        /// <summary>
        /// Gets the configuration secrets key.
        /// </summary>
        /// <remarks>
        /// <para>By default this is "hazelcast", which means that any secrets configuration
        /// options named "hazelcast:something" will be merged into configuration, but a
        /// different value e.g. "hazelcast-tests-something" can be specified to select
        /// different groups of secrets for different tests.</para>
        /// </remarks>
        protected virtual string ConfigurationSecretsKey { get; } = HazelcastOptions.SectionNameConstant;

        /// <summary>
        /// Creates a client.
        /// </summary>
        /// <returns>A client.</returns>
        protected virtual async ValueTask<IHazelcastClient> CreateAndStartClientAsync()
        {
            Logger.LogInformation("Create new client");

            var client = await HazelcastClientFactory.StartNewClientAsync(CreateHazelcastOptions()).CfAwait();
            return client;
        }

        /// <summary>
        /// Creates a client.
        /// </summary>
        /// <returns>A client.</returns>
        protected virtual async ValueTask<IHazelcastClient> CreateAndStartClientAsync(Action<HazelcastOptions> configure)
        {
            Logger.LogInformation("Create new client");

            var options = CreateHazelcastOptions();
            configure(options);
            var client = await HazelcastClientFactory.StartNewClientAsync(options).CfAwait();
            return client;
        }

        /// <summary>
        /// Creates a starting client.
        /// </summary>
        /// <returns>A client.</returns>
        protected virtual HazelcastClientStart CreateStartingClientAsync(Action<HazelcastOptions> configure)
        {
            Logger.LogInformation("Create new client");

            var options = CreateHazelcastOptions();
            configure(options);
            return HazelcastClientFactory.GetNewStartingClient(options);
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
                if (clientInternal.Cluster.Partitioner.GetPartitionId(randomKeyData.PartitionHash) == partitionId)
                    return randomKey;
            }
        }
    }
}
