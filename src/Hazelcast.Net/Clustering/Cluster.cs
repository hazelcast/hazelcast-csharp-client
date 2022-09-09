// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Exceptions;
using Hazelcast.Partitioning;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    internal class Cluster : IAsyncDisposable
    {
        // generates unique cluster identifiers
        private static readonly ISequence<int> ClusterIdSequence = new Int32Sequence();

        private readonly TerminateConnections _terminateConnections;
        private readonly Heartbeat _heartbeat;

        private volatile int _disposed; // disposed flag

        /// <summary>
        /// Initializes a new instance of the <see cref="Cluster"/> class.
        /// </summary>
        /// <param name="options">The cluster configuration.</param>
        /// <param name="serializationServiceFactory">The serialization service factory.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        // FIXME the point of IClusterOptions was to avoid passing HazelcastOptions here so we need to rethink it all
        public Cluster(HazelcastOptions options, Func<ClusterMessaging, SerializationService> serializationServiceFactory, ILoggerFactory loggerFactory)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (serializationServiceFactory == null) throw new ArgumentNullException(nameof(serializationServiceFactory));
            if (loggerFactory is null) throw new ArgumentNullException(nameof(loggerFactory));

            var clientName = string.IsNullOrWhiteSpace(options.ClientName)
                ? ((IClusterOptions)options).ClientNamePrefix + ClusterIdSequence.GetNext()
                : options.ClientName;

            var clusterName = string.IsNullOrWhiteSpace(options.ClusterName) ? "dev" : options.ClusterName;

            State = new ClusterState(options, clusterName, clientName, new Partitioner(), loggerFactory);
            State.ShutdownRequested += () =>
            {
                // yes, we are starting a fire-and-forget task
                // but, DisposeAsync should never throw
                // yet we add a CfAwaitNoThrow() for more safety
                DisposeAsync().CfAwaitNoThrow();
            };

            // create components
            _terminateConnections = new TerminateConnections(loggerFactory);
            Members = new ClusterMembers(State, _terminateConnections);
            Messaging = new ClusterMessaging(State, Members);
            Events = new ClusterEvents(State, Messaging, _terminateConnections, Members);
            SerializationService = serializationServiceFactory(Messaging);
            Connections = new ClusterConnections(State, Members, SerializationService);
            _heartbeat = new Heartbeat(State, Messaging, options.Heartbeat, _terminateConnections);

            // wire components
            WireComponents();

            HConsole.Configure(x => x.Configure<Cluster>().SetIndent(2).SetPrefix("CLUSTER"));
        }

        private void WireComponents()
        {
            // beware! assigning multicast handlers *must* use +=

            // wire members
            Connections.ConnectionOpened += (conn, isFirstEver, isFirst, isNewCluster) => { Members.AddConnection(conn, isNewCluster); return default; };
            Connections.ConnectionClosed += async conn => { await Members.RemoveConnectionAsync(conn).CfAwait(); };

            // wire events
            // connection created = wire connection.ReceivedEvent -> Events.OnReceivedEvent in order to handle events
            // connection opened = install subscriptions on new connection + ensure there is a cluster views connection
            // connection closed = clears subscriptions + ensure there is a cluster views connection
            Connections.ConnectionCreated += Events.OnConnectionCreated;
            Connections.ConnectionOpened += Events.OnConnectionOpened;
            Connections.ConnectionClosed += Events.OnConnectionClosed;

            // wire heartbeat
            Connections.ConnectionOpened += (conn, isFirstEver, isFirst, isNewCluster) => { _heartbeat.AddConnection(conn); return default; };
            Connections.ConnectionClosed += conn => { _heartbeat.RemoveConnection(conn); return default; };
        }

        /// <summary>
        /// Gets the serialization service.
        /// </summary>
        public SerializationService SerializationService { get; }

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
        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            // change state to Starting if it is zero aka New
            var changed = await State.ChangeStateAndWait(ClientState.Starting, 0 /* ClientState.New */).CfAwait();
            if (!changed)
                throw new ConnectionException("Failed to connected (aborted).");

            // connect
            await Connections.ConnectAsync(cancellationToken).CfAwait();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
                return;

            // disposing the cluster terminates all operations
            HConsole.WriteLine(this, "Shutting down");

            // notify we are shutting down - the user may have things to do that
            // require talking to the cluster, so we *wait* for the corresponding
            // event handlers to run before proceeding
            await State.ChangeStateAndWait(ClientState.ShuttingDown).CfAwait();

            // at that point,
            // - the state is 'ShuttingDown'
            // - all user code handling the state change event has run
            // - it is still possible to talk to the cluster

            // the user *should* have shut their own operations down, and if they
            // still try to talk to the cluster, we cannot guarantee anything

            // stop terminating connections, heart-beating
            HConsole.WriteLine(this, "Dispose TerminateConnections");
            await _terminateConnections.DisposeAsync().CfAwait();
            HConsole.WriteLine(this, "Dispose Heartbeat");
            await _heartbeat.DisposeAsync().CfAwait();

            // these elements below *will* talk to the cluster when shutting down,
            // as they will want to unsubscribe in order to shutdown as nicely
            // as possible

            // ClusterMessaging has nothing to dispose

            // ClusterEvents need to shutdown
            // - the events scheduler (always running)
            // - the task that ensures there is a cluster events connection (if it's running)
            // - the task that deals with ghost subscriptions (if it's running)
            // - the two ObjectLifeCycle and PartitionLost subscriptions
            HConsole.WriteLine(this, "Dispose Events");
            await Events.DisposeAsync().CfAwait();

            // for all it matters, we are now down - the final state change to
            // 'Shutdown' will be performed by Connections when the last connection
            // goes down

            // now it's time to dispose the connections, ie close all of them
            // and shutdown
            // - the reconnect task (if it's running)
            // - the task that connects members (always running)
            HConsole.WriteLine(this, "Dispose Connections");
            await Connections.DisposeAsync().CfAwait();

            // connections are gone, we are down
            HConsole.WriteLine(this, "Connections disposed, down");
            await State.ChangeStateAndWait(ClientState.Shutdown).CfAwait();

            // at that point we can get rid of members
            HConsole.WriteLine(this, "Dispose Members");
            await Members.DisposeAsync().CfAwait();

            HConsole.WriteLine(this, "Dispose SerializationService");
            SerializationService.Dispose();

            // and finally, of the state itself
            // which will shutdown
            // - the state changed queue (always running)
            //   (after it has been drained, so last 'Shutdown' even is processed)
            HConsole.WriteLine(this, "Dispose State");
            await State.DisposeAsync().CfAwait();
        }
    }
}
