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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Aggregating;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Partitioning.Strategies;
using Hazelcast.Predicates;
using Hazelcast.Projections;
using Hazelcast.Serialization;
using Hazelcast.Serialization.ConstantSerializers;
using Hazelcast.Serialization.DefaultSerializers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hazelcast
{
    /// <summary>
    /// Creates <see cref="IHazelcastClient"/> instances.
    /// </summary>
    public static class HazelcastClientFactory
    {
        /// <summary>
        /// Starts a new <see cref="IHazelcastClient"/> instance with the automatic options.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A new <see cref="IHazelcastClient"/> instance.</returns>
        /// <remarks>
        /// <para>Options are built via HazelcastOptions.Build method.</para>
        /// </remarks>
        public static ValueTask<IHazelcastClient> StartClientAsync(CancellationToken cancellationToken)
            => StartClientAsync(HazelcastOptions.Build(), cancellationToken);

        /// <summary>
        /// Starts a new <see cref="IHazelcastClient"/> instance with the automatic options.
        /// </summary>
        /// <param name="timeout">A timeout.</param>
        /// <returns>A new <see cref="IHazelcastClient"/> instance.</returns>
        /// <exception cref="TaskTimeoutException">Failed to connect within the specified timeout.</exception>
        /// <remarks>
        /// <para>Options are built via HazelcastOptions.Build method.</para>
        /// <para>If the timeout is omitted, then the timeout configured in the options is used.</para>
        /// </remarks>
        public static ValueTask<IHazelcastClient> StartClientAsync(TimeSpan timeout = default)
            => StartClientAsync(HazelcastOptions.Build(), timeout);

        /// <summary>
        /// Starts a new <see cref="IHazelcastClient"/> instance with configured options.
        /// </summary>
        /// <param name="configure">A <see cref="HazelcastOptions"/> configuration delegate.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A new <see cref="IHazelcastClient"/> instance.</returns>
        /// <remarks>
        /// <para>Options are built via the <see cref="HazelcastOptions.Build(string[], IEnumerable{KeyValuePair{string, string}}, string, string, string, Action{IConfiguration, HazelcastOptions})"/>
        /// method and passed to the <paramref name="configure"/> method, where they can be refined and adjusted, before being used to create the client.</para>
        /// </remarks>
        public static ValueTask<IHazelcastClient> StartClientAsync(Action<HazelcastOptions> configure, CancellationToken cancellationToken)
            => StartClientAsync(GetOptions(configure ?? throw new ArgumentNullException(nameof(configure))), cancellationToken);

        /// <summary>
        /// Starts a new <see cref="IHazelcastClient"/> instance with configured options.
        /// </summary>
        /// <param name="configure">A <see cref="HazelcastOptions"/> configuration delegate.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>A new <see cref="IHazelcastClient"/> instance.</returns>
        /// <exception cref="TaskTimeoutException">Failed to connect within the specified timeout.</exception>
        /// <remarks>
        /// <para>Options are built via the <see cref="HazelcastOptions.Build(string[], IEnumerable{KeyValuePair{string, string}}, string, string, string, Action{IConfiguration, HazelcastOptions})"/>
        /// method and passed to the <paramref name="configure"/> method, where they can be refined and adjusted, before being used to create the client.</para>
        /// <para>If the timeout is omitted, then the timeout configured in the options is used.</para>
        /// </remarks>
        public static ValueTask <IHazelcastClient> StartClientAsync(Action<HazelcastOptions> configure, TimeSpan timeout = default)
            => StartClientAsync(GetOptions(configure ?? throw new ArgumentNullException(nameof(configure))), timeout);

        /// <summary>
        /// Starts a new <see cref="IHazelcastClient"/> instance with options.
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A new <see cref="IHazelcastClient"/> instance.</returns>
        public static async ValueTask<IHazelcastClient> StartClientAsync(HazelcastOptions options, CancellationToken cancellationToken)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            var client = CreateClient(options);
            await client.StartAsync(cancellationToken).CAF();
            return client;
        }

        /// <summary>
        /// Starts a new <see cref="IHazelcastClient"/> instance with options.
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>A new <see cref="IHazelcastClient"/> instance.</returns>
        /// <exception cref="TaskTimeoutException">Failed to connect within the specified timeout.</exception>
        /// <remarks>
        /// <para>If the timeout is omitted, then the timeout configured in the options is used.</para>
        /// </remarks>
        public static async ValueTask<IHazelcastClient> StartClientAsync(HazelcastOptions options, TimeSpan timeout = default)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            var client = CreateClient(options);
            await client.StartAsync(timeout).CAF();
            return client;
        }

        private static HazelcastOptions GetOptions(Action<HazelcastOptions> configure)
        {
            var options = HazelcastOptions.Build();
            configure(options);
            return options;
        }

        private static HazelcastClient CreateClient(HazelcastOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            // this ensures that the clock is correctly configured before anything else
            // happens - remember the clock is static - so we are doing it here - and
            // the clock will actually initialize once
            // TODO: make the clock non-static and pass it to the client + where it's needed
            Clock.Initialize(options.Core.Clock);

            var loggerFactory = options.Logging.LoggerFactory.Service ?? new NullLoggerFactory();

            // TODO: refactor serialization service entirely
            // there should not be a 'builder'
            // it's all configuration or service
            var serializationServiceBuilder = new SerializationServiceBuilder(loggerFactory);
            serializationServiceBuilder
                .SetConfig(options.Serialization)
                .SetPartitioningStrategy(new PartitionAwarePartitioningStragegy()) // TODO: should be configure-able
                .SetVersion(SerializationService.SerializerVersion) // uh? else default is wrong?
                .AddHook<PredicateDataSerializerHook>() // shouldn't they be configurable?
                .AddHook<AggregatorDataSerializerHook>()
                .AddHook<ProjectionDataSerializerHook>()
                .AddDefinitions(new ConstantSerializerDefinitions())
                .AddDefinitions(new DefaultSerializerDefinitions())
                ;

            var serializationService = serializationServiceBuilder.Build();

            var cluster = new Cluster(options, serializationService, loggerFactory);
            var client = new HazelcastClient(options, cluster, serializationService, loggerFactory);

            return client;
        }
    }
}
