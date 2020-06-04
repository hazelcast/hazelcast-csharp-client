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
using Microsoft.Extensions.Logging.Abstractions;

namespace Hazelcast
{
    /// <summary>
    /// Creates <see cref="IHazelcastClient"/> instances.
    /// </summary>
    public class HazelcastClientFactory
    {
        private readonly HazelcastOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastClientFactory"/> class.
        /// </summary>
        /// <param name="options">Options to use by default for all clients.</param>
        public HazelcastClientFactory(HazelcastOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            // this ensures that the clock is correctly configured before anything else
            // happens - remember the clock is static - so we are doing it here - and
            // the clock will actually initialize once
            // TODO: make the clock non-static (low)
            Clock.Initialize(options.Core.Clock);
        }

        /// <summary>
        /// Creates an <see cref="IHazelcastClient"/> instance.
        /// </summary>
        /// <returns>A new <see cref="IHazelcastClient"/> instance.</returns>
        public IHazelcastClient CreateClient()
            => CreateClient(_options);

        /// <summary>
        /// Creates an <see cref="IHazelcastClient"/> instance.
        /// </summary>
        /// <returns>A new <see cref="IHazelcastClient"/> instance.</returns>
        public IHazelcastClient CreateClient(Action<HazelcastOptions> configure)
            => CreateClient(GetClientOptions(configure));

        /// <summary>
        /// Builds the configuration.
        /// </summary>
        /// <param name="configure">A configuration builder.</param>
        /// <returns>The configuration.</returns>
        private HazelcastOptions GetClientOptions(Action<HazelcastOptions> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));
            var options = _options.Clone(); // ensure we don't modify the original options
            configure(options);
            return options;
        }

        /// <summary>
        /// Creates an <see cref="IHazelcastClient"/> instance.
        /// </summary>
        /// <returns>A new <see cref="IHazelcastClient"/> instance.</returns>
        public IHazelcastClient CreateClient(HazelcastOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            var loggerFactory = options.Logging.LoggerFactory.Service ?? new NullLoggerFactory();

            // TODO: refactor serialization service entirely (medium)
            // there should not be a 'builder'
            // it's all configuration or service
            var serializationServiceBuilder = new SerializationServiceBuilder(loggerFactory);
            serializationServiceBuilder
                .SetConfig(options.Serialization)
                .SetPartitioningStrategy(new NullPartitioningStrategy()) // should be configure-able
                .SetVersion(SerializationService.SerializerVersion) // uh? else default is wrong?
                .AddHook<PredicateDataSerializerHook>() // shouldn't they be configurable?
                .AddHook<AggregatorDataSerializerHook>()
                .AddHook<ProjectionDataSerializerHook>()
                .SetEndianness(Endianness.LittleEndian)
                ;
            var serializationService = serializationServiceBuilder.Build();

            var cluster = new Cluster(options.ClientName, null,
                options,
                serializationService,
                loggerFactory);

            return new HazelcastClient(options, cluster, serializationService, loggerFactory);
        }
    }
}
