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
using Hazelcast.Aggregators;
using Hazelcast.Clustering;
using Hazelcast.Configuration;
using Hazelcast.Core;
using Hazelcast.Partitioning.Strategies;
using Hazelcast.Predicates;
using Hazelcast.Projections;
using Hazelcast.Serialization;

namespace Hazelcast
{
    /// <summary>
    /// Creates <see cref="IHazelcastClient"/> instances.
    /// </summary>
    public class HazelcastClientFactory
    {
        /// <summary>
        /// Creates an <see cref="IHazelcastClient"/> instance.
        /// </summary>
        /// <returns>A new <see cref="IHazelcastClient"/> instance.</returns>
        public IHazelcastClient CreateClient()
            => CreateClient(XmlClientConfigBuilder.Build());

        /// <summary>
        /// Creates an <see cref="IHazelcastClient"/> instance.
        /// </summary>
        /// <returns>A new <see cref="IHazelcastClient"/> instance.</returns>
        public IHazelcastClient CreateClient(string configurationFilepath)
            => CreateClient(XmlClientConfigBuilder.Build(configurationFilepath));

        /// <summary>
        /// Creates an <see cref="IHazelcastClient"/> instance.
        /// </summary>
        /// <returns>A new <see cref="IHazelcastClient"/> instance.</returns>
        public IHazelcastClient CreateClient(Action<HazelcastConfiguration> configure)
            => CreateClient(BuildConfiguration(configure));

        /// <summary>
        /// Creates an <see cref="IHazelcastClient"/> instance.
        /// </summary>
        /// <returns>A new <see cref="IHazelcastClient"/> instance.</returns>
        public IHazelcastClient CreateClient(HazelcastConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            var loggerFactory = configuration.Logging.LoggerFactory;

            // TODO: refactor serialization service entirely
            // there should not be a 'builder'
            // it's all configuration or service
            var serializationServiceBuilder = new SerializationServiceBuilder(loggerFactory);
            serializationServiceBuilder
                .SetConfig(configuration.Serialization)
                .SetPartitioningStrategy(new NullPartitioningStrategy()) // should be configure-able
                .SetVersion(SerializationService.SerializerVersion) // uh? else default is wrong?
                .AddHook<PredicateDataSerializerHook>() // shouldn't they be configurable?
                .AddHook<AggregatorDataSerializerHook>()
                .AddHook<ProjectionDataSerializerHook>()
                .SetEndianness(Endianness.LittleEndian)
                ;
            var serializationService = serializationServiceBuilder.Build();

            var authenticator = Services.CreateInstance<IAuthenticator>(configuration.Security.AuthenticatorType, configuration, serializationService);
            var clusterEventSubscribers = configuration.ClusterEventSubscribers;

            var cluster = new Cluster(configuration, serializationService, authenticator, clusterEventSubscribers, loggerFactory);

            return new HazelcastClient(configuration, cluster, serializationService, loggerFactory);
        }

        /// <summary>
        /// Builds the configuration.
        /// </summary>
        /// <param name="configure">A configuration builder.</param>
        /// <returns>The configuration.</returns>
        internal static HazelcastConfiguration BuildConfiguration(Action<HazelcastConfiguration> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));
            var configuration = new HazelcastConfiguration();
            configure(configuration);
            return configuration;
        }
    }
}