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
        private readonly HazelcastConfiguration _configuration;
        private readonly Action<HazelcastConfiguration> _configure;

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastClientFactory"/> class.
        /// </summary>
        public HazelcastClientFactory()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastClientFactory"/> class.
        /// </summary>
        /// <param name="configuration">A configuration to use by default for all clients.</param>
        public HazelcastClientFactory(HazelcastConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastClientFactory"/> class.
        /// </summary>
        /// <param name="configure">A configure action to apply to all clients.</param>
        public HazelcastClientFactory(Action<HazelcastConfiguration> configure)
        {
            _configure = configure ?? throw new ArgumentNullException(nameof(configure));
        }

        private HazelcastConfiguration CreateConfiguration()
        {
            if (_configuration != null) return _configuration;
            var configuration = HazelcastConfiguration.CreateDefault();
            _configure?.Invoke(configuration);
            return configuration;
        }

        /// <summary>
        /// Creates an <see cref="IHazelcastClient"/> instance.
        /// </summary>
        /// <returns>A new <see cref="IHazelcastClient"/> instance.</returns>
        public IHazelcastClient CreateClient()
            => CreateClient(CreateConfiguration());

        /// <summary>
        /// Creates an <see cref="IHazelcastClient"/> instance.
        /// </summary>
        /// <returns>A new <see cref="IHazelcastClient"/> instance.</returns>
        public IHazelcastClient CreateClient(string configurationFilepath)
            => CreateClient(HazelcastConfiguration.Parse(configurationFilepath));

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

            var loggerFactory = configuration.Logging.LoggerFactory.Create();

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

            var cluster = new Cluster(configuration.ClientName, null,
                configuration.Labels,
                configuration.Cluster,
                configuration.Networking,
                configuration.LoadBalancing,
                configuration.Security,
                serializationService,
                loggerFactory);

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