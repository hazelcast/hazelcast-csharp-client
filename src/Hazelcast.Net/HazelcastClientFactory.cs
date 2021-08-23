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
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Aggregation;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Partitioning.Strategies;
using Hazelcast.Projection;
using Hazelcast.Query;
using Hazelcast.Serialization;
using Hazelcast.Serialization.ConstantSerializers;
using Hazelcast.Serialization.DefaultSerializers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hazelcast
{
    /// <summary>
    /// Creates <see cref="IHazelcastClient"/> instances.
    /// </summary>
    public static class HazelcastClientFactory
    {
        /// <summary>
        /// Starts a new <see cref="IHazelcastClient"/> instance with automatic options.
        /// </summary>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>A new <see cref="IHazelcastClient"/> instance.</returns>
        /// <remarks>
        /// <para>Options are built via the <see cref="HazelcastOptionsBuilder.Build()"/> method.</para>
        /// <para>By default, the client connection timeout is infinite. If this method cannot establish
        /// a connection to a cluster at the configured addresses, it may appear to hang as it retries
        /// forever. You may want to configure a timeout via the options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds
        /// configuration option.</para>
        /// </remarks>
        public static ValueTask<IHazelcastClient> StartNewClientAsync(CancellationToken cancellationToken = default)
            => StartNewClientAsync(new HazelcastOptionsBuilder().Build(), cancellationToken);

        /// <summary>
        /// Starts a new <see cref="IHazelcastClient"/> instance with configured options.
        /// </summary>
        /// <param name="configure">A <see cref="HazelcastOptions"/> configuration delegate.</param>
        /// <param name="cancellationToken">A optional cancellation token.</param>
        /// <returns>A new <see cref="IHazelcastClient"/> instance.</returns>
        /// <remarks>
        /// <para>Options are built via the <see cref="HazelcastOptionsBuilder.Build()"/> method and passed to the <paramref name="configure"/> method,
        /// where they can be refined and adjusted, before being used to create the client.</para>
        /// <para>By default, the client connection timeout is infinite. If this method cannot establish
        /// a connection to a cluster at the configured addresses, it may appear to hang as it retries
        /// forever. You may want to configure a timeout via the options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds
        /// configuration option.</para>
        /// </remarks>
        public static ValueTask<IHazelcastClient> StartNewClientAsync(Action<HazelcastOptions> configure, CancellationToken cancellationToken = default)
            => StartNewClientAsync(GetOptions(configure ?? throw new ArgumentNullException(nameof(configure))), cancellationToken);

        /// <summary>
        /// Starts a new <see cref="IHazelcastClient"/> instance with options.
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="cancellationToken">A optional cancellation token.</param>
        /// <returns>A new <see cref="IHazelcastClient"/> instance.</returns>
        /// <remarks>
        /// <para>By default, the client connection timeout is infinite. If this method cannot establish
        /// a connection to a cluster at the configured addresses, it may appear to hang as it retries
        /// forever. You may want to configure a timeout via the options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds
        /// configuration option.</para>
        /// </remarks>
        public static ValueTask<IHazelcastClient> StartNewClientAsync(HazelcastOptions options, CancellationToken cancellationToken = default)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            // every async operations using this client will need a proper async context
            // and, we *must* do this in a non-async method for the change to bubble up!
            AsyncContext.Ensure();

            return StartNewClientAsyncInternal(options, cancellationToken);
        }

        // implements the async part of StartNewClientAsync w/ cancellation
        private static async ValueTask<IHazelcastClient> StartNewClientAsyncInternal(HazelcastOptions options, CancellationToken cancellationToken)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            var client = CreateClient(options);
            await client.StartAsync(cancellationToken).CfAwait();
            return client;
        }

        /// <summary>
        /// Gets a new starting <see cref="IHazelcastClient"/> instance with automatic options.
        /// </summary>
        /// <param name="cancellationToken">A optional cancellation token.</param>
        /// <returns>A <see cref="HazelcastClientStart"/> instance which exposes the <see cref="IHazelcastClient"/> itself,
        /// along with a <see cref="Task"/> representing the start operation.</returns>
        /// <remarks>
        /// <para>The <see cref="IHazelcastClient"/> instance is starting, but not started yet. Its start operation is represented by the returned
        /// <see cref="Task"/>, which will complete when the client has started, or when starting has failed. Trying to use the client before the
        /// start <see cref="Task"/> has completed can have unspecified results, including throwing exceptions. Make sure that the start
        /// <see cref="Task"/> has actually completed before using the client.</para>
        /// <para>In any case, the start <see cref="Task"/> must be awaited, as it may fail with an exception that must be observed.</para>
        /// <para>Options are built via the <see cref="HazelcastOptionsBuilder.Build()"/> method.</para>
        /// <para>By default, the client connection timeout is infinite. If this method cannot establish
        /// a connection to a cluster at the configured addresses, it may appear to hang as it retries
        /// forever. You may want to configure a timeout via the options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds
        /// configuration option.</para>
        /// </remarks>
        public static HazelcastClientStart GetNewStartingClient(CancellationToken cancellationToken = default)
            => GetNewStartingClient(new HazelcastOptionsBuilder().Build(), cancellationToken);

        /// <summary>
        /// Gets a new starting <see cref="IHazelcastClient"/> instance with configured options.
        /// </summary>
        /// <param name="configure">A <see cref="HazelcastOptions"/> configuration delegate.</param>
        /// <param name="cancellationToken">A optional cancellation token.</param>
        /// <returns>A <see cref="HazelcastClientStart"/> instance which exposes the <see cref="IHazelcastClient"/> itself,
        /// along with a <see cref="Task"/> representing the start operation.</returns>
        /// <remarks>
        /// <para>The <see cref="IHazelcastClient"/> instance is starting, but not started yet. Its start operation is represented by the returned
        /// <see cref="Task"/>, which will complete when the client has started, or when starting has failed. Trying to use the client before the
        /// start <see cref="Task"/> has completed can have unspecified results, including throwing exceptions. Make sure that the start
        /// <see cref="Task"/> has actually completed before using the client.</para>
        /// <para>In any case, the start <see cref="Task"/> must be awaited, as it may fail with an exception that must be observed.</para>
        /// <para>Options are built via the <see cref="HazelcastOptionsBuilder.Build()"/> method and passed to the <paramref name="configure"/> method,
        /// where they can be refined and adjusted, before being used to create the client.</para>
        /// <para>By default, the client connection timeout is infinite. If this method cannot establish
        /// a connection to a cluster at the configured addresses, it may appear to hang as it retries
        /// forever. You may want to configure a timeout via the options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds
        /// configuration option.</para>
        /// </remarks>
        public static HazelcastClientStart GetNewStartingClient(Action<HazelcastOptions> configure, CancellationToken cancellationToken = default)
            => GetNewStartingClient(GetOptions(configure ?? throw new ArgumentNullException(nameof(configure))), cancellationToken);

        /// <summary>
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="cancellationToken">A optional cancellation token.</param>
        /// <returns>A <see cref="HazelcastClientStart"/> instance which exposes the <see cref="IHazelcastClient"/> itself,
        /// <remarks>
        /// <para>The <see cref="IHazelcastClient"/> instance is starting, but not started yet. Its start operation is represented by the returned
        /// <see cref="Task"/>, which will complete when the client has started, or when starting has failed. Trying to use the client before the
        /// start <see cref="Task"/> has completed can have unspecified results, including throwing exceptions. Make sure that the start
        /// <see cref="Task"/> has actually completed before using the client.</para>
        /// <para>In any case, the start <see cref="Task"/> must be awaited, as it may fail with an exception that must be observed.</para>
        /// <para>By default, the client connection timeout is infinite. If this method cannot establish
        /// a connection to a cluster at the configured addresses, it may appear to hang as it retries
        /// forever. You may want to configure a timeout via the options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds
        /// configuration option.</para>
        /// </remarks>
        public static HazelcastClientStart GetNewStartingClient(HazelcastOptions options, CancellationToken cancellationToken = default)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            // every async operations using this client will need a proper async context
            // and, we *must* do this in a non-async method for the change to bubble up!
            AsyncContext.Ensure();

            var client = CreateClient(options);
            return new HazelcastClientStart(client, client.StartAsync(cancellationToken));
        }

        private static HazelcastOptions GetOptions(Action<HazelcastOptions> configure)
        {
            return new HazelcastOptionsBuilder().With(configure).Build();
        }

        // (internal for tests only) creates the serialization service
        internal static SerializationService CreateSerializationService(SerializationOptions options, ILoggerFactory loggerFactory)
        {
            // TODO: refactor serialization service entirely
            // there should not be a 'builder'
            // it's all configuration or service
            var serializationServiceBuilder = new SerializationServiceBuilder(loggerFactory);
            serializationServiceBuilder
                .SetConfig(options)
                .SetPartitioningStrategy(new PartitionAwarePartitioningStragegy()) // TODO: should be configure-able
                .SetVersion(SerializationService.SerializerVersion) // uh? else default is wrong?
                .AddHook<PredicateDataSerializerHook>() // shouldn't they be configurable?
                .AddHook<AggregatorDataSerializerHook>()
                .AddHook<ProjectionDataSerializerHook>()
                .AddDefinitions(new ConstantSerializerDefinitions())
                .AddDefinitions(new DefaultSerializerDefinitions())
                ;

            return serializationServiceBuilder.Build();
        }

        // creates the client
        private static HazelcastClient CreateClient(HazelcastOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            // clone the options - we don't want any change to the original options to impact this client
            options = options.Clone();

            // this ensures that the clock is correctly configured before anything else
            // happens - remember the clock is static - so we are doing it here - and
            // the clock will actually initialize once
            // TODO: make the clock non-static and pass it to the client + where it's needed
            Clock.Initialize(options.Core.Clock);

            var loggerFactory = options.LoggerFactory.Service ?? new NullLoggerFactory();
            var serializationService = CreateSerializationService(options.Serialization, loggerFactory);
            var cluster = new Cluster(options, serializationService, loggerFactory);
            var client = new HazelcastClient(options, cluster, serializationService, loggerFactory);

            return client;
        }
    }
}
