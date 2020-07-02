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
using Hazelcast.Aggregating;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Partitioning.Strategies;
using Hazelcast.Predicates;
using Hazelcast.Projections;
using Hazelcast.Serialization;
using Hazelcast.Serialization.ConstantSerializers;
using Hazelcast.Serialization.DefaultSerializers;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hazelcast
{
    /// <summary>
    /// Creates <see cref="IHazelcastClient"/> instances.
    /// </summary>
    public sealed class HazelcastClientFactory
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
            // TODO: make the clock non-static (low priority)
            Clock.Initialize(options.Core.Clock);
        }

        /// <summary>
        /// Creates an <see cref="IHazelcastClient"/> instance with the factory options.
        /// </summary>
        /// <returns>A new <see cref="IHazelcastClient"/> instance.</returns>
        public IHazelcastClient CreateClient()
            => CreateClient(_options);

        /// <summary>
        /// Creates an <see cref="IHazelcastClient"/> instance with options derived from the factory options.
        /// </summary>
        /// <param name="configure">An <see cref="HazelcastOptions"/> configuration delegate.</param>
        /// <returns>A new <see cref="IHazelcastClient"/> instance.</returns>
        /// <remarks>
        /// <para>The factory options are cloned and passed to the <paramref name="configure"/>
        /// method, where they can be refined and adjusted, before being used to create
        /// the client.</para>
        /// </remarks>
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
        /// Creates an <see cref="IHazelcastClient"/> instance with options.
        /// </summary>
        /// <param name="options">Options.</param>
        /// <returns>A new <see cref="IHazelcastClient"/> instance.</returns>
        /// <remarks>
        /// <para>When options are supplied, the factory options are completely ignored.</para>
        /// </remarks>
#pragma warning disable CA1822 // Mark members as static - no
        public IHazelcastClient CreateClient(HazelcastOptions options)
#pragma warning restore CA1822
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
                .AddDefinitions(new ConstantSerializerDefinitions()) // constant serializers
                .AddDefinitions(new DefaultSerializerDefinitions()) // default serializers
                ;
            var serializationService = serializationServiceBuilder.Build();

            var cluster = new Cluster(options.ClientName, null,
                options,
                serializationService,
                loggerFactory);

            var client = new HazelcastClient(options, cluster, serializationService, loggerFactory);

            // wire events
            // this way, the cluster does not need to know about the hazelcast client
            cluster.OnObjectLifecycleEvent = client.OnObjectLifecycleEvent;
            cluster.OnMemberLifecycleEvent = client.OnMemberLifecycleEvent;
            cluster.OnClientLifecycleEvent = client.OnClientLifecycleEvent;
            cluster.OnPartitionsUpdated = client.OnPartitionsUpdated;
            cluster.OnPartitionLost = client.OnPartitionLost;
            cluster.OnConnectionAdded = client.OnConnectionAdded;
            cluster.OnConnectionRemoved = client.OnConnectionRemoved;
            cluster.OnConnectionRemoved = client.OnConnectionRemoved;

            // TODO: this belongs to the client actually
            cluster.OnFirstClientConnected = async cancellationToken =>
            {
                foreach (var subscriber in options.Subscribers)
                    await subscriber.SubscribeAsync(client, cancellationToken).CAF();
            };

            return client;
        }
    }
}
