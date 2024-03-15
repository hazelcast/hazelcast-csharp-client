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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Aggregation;
using Hazelcast.Clustering;
using Hazelcast.Configuration;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Partitioning.Strategies;
using Hazelcast.Projection;
using Hazelcast.Query;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Compact;
using Hazelcast.Serialization.ConstantSerializers;
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
        [Obsolete("The method is obsolete. Use other overloads.",true)]
        [ExcludeFromCodeCoverage]
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
        /// Starts a new <see cref="IHazelcastClient"/> instance in failover mode with configured options.
        /// </summary>
        /// <param name="configure">A <see cref="HazelcastFailoverOptions"/> configuration delegate.</param>
        /// <param name="cancellationToken">A optional cancellation token.</param>
        /// <returns>A new <see cref="IHazelcastClient"/> instance.</returns>
        /// <remarks>
        /// <para>Options are built via the <see cref="HazelcastFailoverOptionsBuilder.Build()"/> method
        /// and passed to the <paramref name="configure"/> method,
        /// where they can be refined and adjusted, before being used to create the client.</para>
        /// <para>By default, the client connection timeout is 120 seconds. If this method cannot establish
        /// a connection to a cluster at the configured addresses, it may do failover. You may want to configure a timeout via the options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds
        /// configuration option.</para>
        /// </remarks>
        public static ValueTask<IHazelcastClient> StartNewFailoverClientAsync(Action<HazelcastFailoverOptions> configure, CancellationToken cancellationToken = default)
            => StartNewFailoverClientAsync(GetFailoverOptions(configure ?? throw new ArgumentNullException(nameof(configure))), cancellationToken);

        /// <summary>
        /// Starts a new <see cref="IHazelcastClient"/> in failover mode instance with options.
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="cancellationToken">A optional cancellation token.</param>
        /// <returns>A new <see cref="IHazelcastClient"/> instance.</returns>
        /// <remarks>
        /// <para>By default, the client connection timeout is 120 seconds. If this method cannot establish
        /// a connection to a cluster at the configured addresses, it may do failover. You may want to configure a timeout via the options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds
        /// configuration option.</para>
        /// </remarks>
        public static ValueTask<IHazelcastClient> StartNewFailoverClientAsync(HazelcastFailoverOptions options, CancellationToken cancellationToken = default)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            // every async operations using this client will need a proper async context
            // and, we *must* do this in a non-async method for the change to bubble up!
            AsyncContext.Ensure();

            return StartNewFailoverClientAsyncInternal(options, cancellationToken);
        }

        // implements the async part of StartNewClientAsync w/ cancellation
        private static async ValueTask<IHazelcastClient> StartNewFailoverClientAsyncInternal(HazelcastFailoverOptions options, CancellationToken cancellationToken)
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
        [Obsolete("The method is obsolete. Use other overloads.",true)]
        [ExcludeFromCodeCoverage]
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
        /// Gets a new starting <see cref="IHazelcastClient"/> instance with options.
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="cancellationToken">A optional cancellation token.</param>
        /// <returns>A <see cref="HazelcastClientStart"/> instance which exposes the <see cref="IHazelcastClient"/> itself,
        /// along with a <see cref="Task"/> representing the start operation.</returns>
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

        /// <summary>
        /// Gets a new starting <see cref="IHazelcastClient"/> instance in failover mode with configured options.
        /// </summary>
        /// <param name="configure">A <see cref="HazelcastFailoverOptions"/> configuration delegate.</param>
        /// <param name="cancellationToken">A optional cancellation token.</param>
        /// <returns>A <see cref="HazelcastClientStart"/> instance which exposes the <see cref="IHazelcastClient"/> itself,
        /// along with a <see cref="Task"/> representing the start operation.</returns>
        /// <remarks>
        /// <para>The <see cref="IHazelcastClient"/> instance is starting, but not started yet. Its start operation is represented by the returned
        /// <see cref="Task"/>, which will complete when the client has started, or when starting has failed. Trying to use the client before the
        /// start <see cref="Task"/> has completed can have unspecified results, including throwing exceptions. Make sure that the start
        /// <see cref="Task"/> has actually completed before using the client.</para>
        /// <para>In any case, the start <see cref="Task"/> must be awaited, as it may fail with an exception that must be observed.</para>
        /// <para>Options are built via the <see cref="HazelcastFailoverOptionsBuilder.Build()"/> method and passed to
        /// the <paramref name="configure"/> method,
        /// where they can be refined and adjusted, before being used to create the client.</para>
        /// <para>By default, the client connection timeout is 120 seconds. If this method cannot establish
        /// a connection to a cluster at the configured addresses, it may do failover. You may want to configure a timeout via the options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds
        /// configuration option.</para>
        /// </remarks>
        public static HazelcastClientStart GetNewStartingFailoverClient(Action<HazelcastFailoverOptions> configure, CancellationToken cancellationToken = default)
            => GetNewStartingFailoverClient(GetFailoverOptions(configure ?? throw new ArgumentNullException(nameof(configure))), cancellationToken);

        /// <summary>
        /// Gets a new starting <see cref="IHazelcastClient"/> instance in failover mode with options.
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="cancellationToken">A optional cancellation token.</param>
        /// <returns>A <see cref="HazelcastClientStart"/> instance which exposes the <see cref="IHazelcastClient"/> itself,
        /// along with a <see cref="Task"/> representing the start operation.</returns>
        /// <remarks>
        /// <para>The <see cref="IHazelcastClient"/> instance is starting, but not started yet. Its start operation is represented by the returned
        /// <see cref="Task"/>, which will complete when the client has started, or when starting has failed. Trying to use the client before the
        /// start <see cref="Task"/> has completed can have unspecified results, including throwing exceptions. Make sure that the start
        /// <see cref="Task"/> has actually completed before using the client.</para>
        /// <para>In any case, the start <see cref="Task"/> must be awaited, as it may fail with an exception that must be observed.</para>
        /// <para>By default, the client connection timeout is 120 seconds. If this method cannot establish
        /// a connection to a cluster at the configured addresses, it may do failover. You may want to configure a timeout via the options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds
        /// configuration option.</para>
        /// </remarks>
        public static HazelcastClientStart GetNewStartingFailoverClient(HazelcastFailoverOptions options, CancellationToken cancellationToken = default)
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

        private static HazelcastFailoverOptions GetFailoverOptions(Action<HazelcastFailoverOptions> configure)
        {
            return new HazelcastFailoverOptionsBuilder().With(configure).Build();
        }

        // (internal for tests only) creates the default serialization service
        internal static SerializationService CreateSerializationService(SerializationOptions options, IClusterMessaging messaging, ILoggerFactory loggerFactory)
        {
            var schemas = new Schemas(messaging, options.Compact);

            return new SerializationServiceBuilder(options, loggerFactory)

                .SetPartitioningStrategy(new PartitionAwarePartitioningStragegy()) // TODO: should be configure-able
                .SetCompactSchemas(schemas) // transfer ownership, schemas will be disposed by serialization service

                // add hooks that construct and provide IIdentifiedDataSerialization factories for more built-in types
                .AddHook<PredicateDataSerializerHook>()
                .AddHook<AggregatorDataSerializerHook>()
                .AddHook<ProjectionDataSerializerHook>()
                .AddHook<ReliableTopicDataSerializerHook>()
                .AddHook<PublisherAddressDataSerializerHook>()
                .AddHook<ConfigurationDataSerializerHook>()

                // add a hook that constructs and provides the compact IIdentifiedDataSerialization factory
                // because some basic compact functionality such as schema distribution don't use compact
                // serialization themselves.
                .AddHook<CompactSerializationHook>()

                // define serializers for a range of primitive types (int, lists...)
                .AddDefinitions(new ConstantSerializerDefinitions())

                // and build
                .Build()
                ;
        }

        // creates the client
        internal static HazelcastClient CreateClient(HazelcastOptionsBase hazelcastOptions)
        {
            if (hazelcastOptions == null) throw new ArgumentNullException(nameof(hazelcastOptions));

            void QuickLogDebug(HazelcastOptions o, string message)
            {
                var logger = o.LoggerFactory.Service.CreateLogger(typeof(HazelcastClientFactory));
                logger.LogDebug(message);
            }

            if (hazelcastOptions is HazelcastOptions options)
            {
                // clone the options - we don't want any change to the original options to impact this client
                options = options.Clone();
            }
            else
            {
                var opt = (HazelcastFailoverOptions) hazelcastOptions;

                if (!opt.Clients.Any())
                    throw new ConfigurationException("If Failover is enabled, then clusters should be provided.");

                if (opt.Clients[0].Networking.ConnectionTimeoutMilliseconds < 0)
                {
                    opt.Clients[0].Networking.ConnectionTimeoutMilliseconds = 120_000;
                    QuickLogDebug(opt.Clients[0], "Options: Clusters[0].Networking.ConnectionTimeoutMilliseconds is infinite => set it to 120sec.");
                }
                
                opt.Enabled = true;
                options = opt.Clients[0].Clone();
                options.FailoverOptions = opt.Clone(); // safe, no cyclic clone
            }

            // propagate the temp TPC options
            options.Authentication.TpcEnabled = options.Networking.Tpc.Enabled;

            if (options.Networking.Cloud.Enabled)
            {
                // if cloud is enabled we must force usage of public addresses
                // otherwise, ConnectAddressResolver may decide to use internal addresses if SSL is enabled
                // TODO: why?

                if (options.Networking.UsePublicAddresses.HasValue)
                {
                    if (!options.Networking.UsePublicAddresses.Value)
                        throw new ConfigurationException("If CloudDiscovery is enabled, then UsePublicAddress cannot be false.");
                }
                else
                {
                    options.Networking.UsePublicAddresses = true;
                    QuickLogDebug(options, "Options: Networking.Cloud.Enabled is true => set Networking.UsePublicAddress to true.");
                }
            }

            // this ensures that the clock is correctly configured before anything else
            // happens - remember the clock is static - so we are doing it here - and
            // the clock will actually initialize once
            // TODO: make the clock non-static and pass it to the client + where it's needed
            Clock.Initialize(options.Core.Clock);

            var loggerFactory = options.LoggerFactory.Service ?? new NullLoggerFactory();
            var cluster = new Cluster(
                options,
                messaging => CreateSerializationService(options.Serialization, messaging, loggerFactory),
                loggerFactory
            );
            return new HazelcastClient(options, cluster, loggerFactory);
        }
    }
}
