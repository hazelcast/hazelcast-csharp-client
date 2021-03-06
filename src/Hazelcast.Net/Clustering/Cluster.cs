﻿// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Partitioning;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    internal class Cluster : IAsyncDisposable
    {
        // generates unique cluster identifiers
        private static readonly ISequence<int> ClusterIdSequence = new Int32Sequence();

        private readonly ILogger _logger;
        private readonly Heartbeat _heartbeat;
        private volatile int _disposed; // disposed flag

        /// <summary>
        /// Initializes a new instance of the <see cref="Cluster"/> class.
        /// </summary>
        /// <param name="options">The cluster configuration.</param>
        /// <param name="serializationService">The serialization service.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public Cluster(
            IClusterOptions options,
            SerializationService serializationService,
            ILoggerFactory loggerFactory)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (serializationService == null) throw new ArgumentNullException(nameof(serializationService));
            if (loggerFactory is null) throw new ArgumentNullException(nameof(loggerFactory));

            _logger = loggerFactory.CreateLogger<Cluster>();

            var clientName = string.IsNullOrWhiteSpace(options.ClientName)
                ? options.ClientNamePrefix + ClusterIdSequence.GetNext()
                : options.ClientName;

            var clusterName = string.IsNullOrWhiteSpace(options.ClusterName) ? "dev" : options.ClusterName;

            State = new ClusterState(options, clusterName, clientName, new Partitioner(), loggerFactory);

            Members = new ClusterMembers(State);
            Messaging = new ClusterMessaging(State, Members);
            Events = new ClusterEvents(State, Messaging, Members);
            ClusterEvents = new ClusterClusterEvents(State, Members, Events);
            Connections = new ClusterConnections(State, Members, serializationService);

            _heartbeat = new Heartbeat(State, Members, Messaging, options.Heartbeat, loggerFactory);

            HConsole.Configure(x => x.Set(this, config => config.SetIndent(2).SetPrefix("CLUSTER")));
        }

        // throws if this instance has been disposed
        private void ThrowIfDisposed()
        {
            if (_disposed > 0) throw new ObjectDisposedException(nameof(Cluster));
        }

        /// <summary>
        /// Gets the cluster state.
        /// </summary>
        public ClusterState State { get; }

        /// <summary>
        /// Gets the client name.
        /// </summary>
        public string ClientName => State.ClientName;

        /// <summary>
        /// Gets the unique identifier of the cluster, as assigned by the client.
        /// </summary>
        public Guid ClientId => State.ClientId;

        /// <summary>
        /// Gets the cluster name;
        /// </summary>
        public string Name => State.ClusterName;

        /// <summary>
        /// Gets the cluster instrumentation.
        /// </summary>
        public ClusterInstrumentation Instrumentation => State.Instrumentation;

        /// <summary>
        /// Gets the connections service.
        /// </summary>
        public ClusterConnections Connections { get; }

        /// <summary>
        /// Gets the messaging service.
        /// </summary>
        public ClusterMessaging Messaging { get; }

        /// <summary>
        /// Gets the members service.
        /// </summary>
        public ClusterMembers Members { get; }

        /// <summary>
        /// Gets the cluster-level events management service.
        /// </summary>
        public ClusterClusterEvents ClusterEvents { get; }

        /// <summary>
        /// Gets the cluster events service.
        /// </summary>
        public ClusterEvents Events { get; }

        /// <summary>
        /// Determines whether the cluster is using smart routing.
        /// </summary>
        /// <remarks>
        /// <para>In "smart mode" the clients connect to each member of the cluster. Since each
        /// data partition uses the well known and consistent hashing algorithm, each client
        /// can send an operation to the relevant cluster member, which increases the
        /// overall throughput and efficiency. Smart mode is the default mode.</para>
        /// <para>In "uni-socket mode" the clients is required to connect to a single member, which
        /// then behaves as a gateway for the other members. Firewalls, security, or some
        /// custom networking issues can be the reason for these cases.</para>
        /// </remarks>
        public bool IsSmartRouting => State.IsSmartRouting;

        /// <summary>
        /// Whether the cluster is connected.
        /// </summary>
        public bool IsConnected => State.IsConnected;

        /// <summary>
        /// Whether the cluster is active.
        /// </summary>
        public bool IsActive => _disposed == 0;

        /// <summary>
        /// Gets the partitioner.
        /// </summary>
        public Partitioner Partitioner => State.Partitioner;

        /// <summary>
        /// Connects the cluster.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the cluster is connected.</returns>
        public Task ConnectAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            return Connections.ConnectAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
                return;

            // disposing the cluster terminates all operations

            // change the state - the final NotConnected state will
            // be set by Connections when the last one goes down, and
            // the cluster is not active anymore
            State.NotifyState(ClientState.ShuttingDown);

            // we don't need heartbeat anymore
            await _heartbeat.DisposeAsync().CfAwait();

            // ClusterMessaging has nothing to dispose

            // ClusterEvents need to shutdown
            // - the events scheduler (always running)
            // - the task that ensures FIXME DOCUMENT
            // - the task that deals with ghost subscriptions (if it's running)
            await Events.DisposeAsync().CfAwait();

            // ClusterClusterEvents need to shutdown
            // - the two ObjectLifeCycle and PartitionLost subscriptions
            await ClusterEvents.DisposeAsync().CfAwait();

            // now it's time to dispose the connections, ie close all of them
            // and shutdown
            // - the reconnect task (if it's running)
            // - the task that connects members (always running)
            await Connections.DisposeAsync().CfAwait();

            // at that point we can get rid of members
            await Members.DisposeAsync().CfAwait();

            // and finally, of the state itself
            // which will shutdown
            // - the state changed queue (always running)
            await State.DisposeAsync().CfAwait();
        }
    }
}
