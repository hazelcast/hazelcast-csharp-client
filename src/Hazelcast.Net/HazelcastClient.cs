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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Hazelcast.Exceptions;
using Hazelcast.Models;
using Hazelcast.NearCaching;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast
{
    /// <summary>
    /// Represents the Hazelcast client.
    /// </summary>
    internal partial class HazelcastClient : IHazelcastClient
    {
        private readonly HazelcastOptions _options;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        private readonly DistributedObjectFactory _distributedOjects;
        private readonly NearCacheManager _nearCacheManager;

        private int _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastClient"/> class.
        /// </summary>
        /// <param name="options">The client configuration.</param>
        /// <param name="cluster">A cluster.</param>
        /// <param name="serializationService">A serialization service.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public HazelcastClient(HazelcastOptions options, Cluster cluster, ISerializationService serializationService, ILoggerFactory loggerFactory)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            Cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
            SerializationService = serializationService ?? throw new ArgumentNullException(nameof(serializationService));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<IHazelcastClient>();

            _distributedOjects = new DistributedObjectFactory(Cluster, serializationService, loggerFactory);
            _nearCacheManager = new NearCacheManager(cluster, serializationService, loggerFactory, options.NearCache);

            // wire events - clean

            // NOTE: each event that is a Func<..., ValueTask> *must* be invoked
            // through the special FuncExtensions.AwaitEach() method, see notes in
            // FuncExtensions.cs

            // when an object is created/destroyed, OnObject... triggers the user-level events
            cluster.ClusterEvents.ObjectCreated = OnObjectCreated;
            cluster.ClusterEvents.ObjectDestroyed = OnObjectDestroyed;

            // when client/cluster state changes, OnStateChanged triggers user-level events
            cluster.State.StateChanged += OnStateChanged;

            // when partitions are updated, OnPartitionUpdated triggers the user-level event
            cluster.Events.PartitionsUpdated += OnPartitionsUpdated;

            // when a partition is lost, OnPartitionLost triggers the user-level event
            cluster.ClusterEvents.PartitionLost = OnPartitionLost;

            // when members are updated, Connections.OnMembersUpdated queues the added
            // members so that a connection is opened to each of them.
            cluster.Events.MembersUpdated += cluster.Connections.OnMembersUpdated;

            // when members are updated, OnMembersUpdated triggers the user-level event
            cluster.Events.MembersUpdated += OnMembersUpdated;

            // when a connection is created, Events.OnConnectionCreated wires the connection
            // ReceivedEvent event to Events.OnReceivedEvent, in order to handle events
            cluster.Connections.ConnectionCreated += cluster.Events.OnConnectionCreated;

            // when connecting to a new cluster, DistributedObjects.OnConnectingToNewCluster
            // re-creates all the known distributed object so far on the new cluster
            cluster.Connections.ConnectionOpened += _distributedOjects.OnConnectionOpened;

            // when a connection is added, OnConnectionOpened triggers the user-level event
            // and, if it is the first connection, subscribes to events according to the
            // subscribers defined in options.
            cluster.Connections.ConnectionOpened += OnConnectionOpened;

            // when a connection is opened, Events.OnConnectionOpened ensures that the
            // cluster connection (handling member/partition views) is set, and installs
            // subscriptions on this new connection.
            cluster.Connections.ConnectionOpened += cluster.Events.OnConnectionOpened;

            // when a connection is closed, client.OnConnectionClosed triggers the user-level event
            cluster.Connections.ConnectionClosed += OnConnectionClosed;

            // when a connection is closed, Events.OnConnectionClosed clears subscriptions
            // (cannot unsubscribe since the connection is closed) and ensures that the
            // cluster connection (handling member/partition views) is set.
            cluster.Connections.ConnectionClosed += cluster.Events.OnConnectionClosed;
        }

        /// <summary>
        /// Gets the <see cref="Cluster"/>.
        /// </summary>
        public Cluster Cluster { get; }

        /// <inheritdoc />
        public string Name => Cluster.ClientName;

        /// <inheritdoc />
        public Guid Id => Cluster.ClientId;

        /// <inheritdoc />
        public string ClusterName => Cluster.Name;

        /// <inheritdoc />
        public IReadOnlyCollection<MemberInfo> Members => Cluster.Members.SnapshotMembers().ToList();

        /// <inheritdoc />
        public bool IsActive => Cluster.IsActive;

        /// <inheritdoc />
        public bool IsConnected => Cluster.IsConnected;

        /// <summary>
        /// Gets the <see cref="ISerializationService"/>.
        /// </summary>
        public ISerializationService SerializationService { get; }

        /// <summary>
        /// Starts the client by connecting to the remote cluster.
        /// </summary>
        /// <param name="timeout">A timeout.</param>
        /// <returns>A task that will complete when the client is connected.</returns>
        /// <exception cref="TaskTimeoutException">Failed to connect within the specified timeout.</exception>
        /// <remarks>
        /// <para>If the timeout is omitted, then the timeout configured in the options is used.</para>
        /// </remarks>
        public
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task StartAsync(TimeSpan timeout = default)
        {
            var timeoutMs = timeout.RoundedMilliseconds()
                .ClampToInt32()
                .ZeroAs(_options.Networking.ConnectionTimeoutMilliseconds) // default
                .NegativeAs(-1); // infinite

            var task = TaskEx.RunWithTimeout((c, t) => c.ConnectAsync(t), Cluster, timeoutMs);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }

        /// <summary>
        /// Starts the client by connecting to the remote cluster.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the client is connected.</returns>
        public
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task StartAsync(CancellationToken cancellationToken)
        {
            var task = Cluster.ConnectAsync(cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
                return;

            // order is important, must dispose the cluster last, as it will tear down
            // connections that may be required by other things being disposed

            // FIXME - cleanup shutdown & reconnect
            // are we reconnecting correctly?
            // what about immediately terminating the client?
            // where and when should we lock the cluster? before dispose? prepareForDispose?

            // the client is Active when the cluster is Active
            // the cluster is Active from creation (even before it is connected) until it is disposed
            //
            // the client is Connected when the cluster is Connected
            // the cluster is Connected when its ConnectionState is Connected
            //
            // ConnectionState diagram:
            // creation -> NotConnected
            // NotConnected -> ConnectAsync() -> Connecting
            // Connecting
            //   -> ConnectFirstAsync(), TryConnectAsync(), ConnectWithLockAsync() -> Connected
            //   unless error -> NotConnected
            // Connected -> HandleConnectionTermination()
            //   ReconnectMode.DoNotReconnect -> Disconnected
            //   ReconnectMode.ReconnectAsync (or Sync) -> Connecting
            //
            // and then there is ClientLifecycleState with events
            // - Starting         Connecting
            // - Started          n/a
            // - ShuttingDown     Disconnecting
            // - Shutdown
            // - Connected        Connected
            // - Disconnected     ReConnecting
            //
            // TODO
            // - are we firing the ClientStateChanged events?
            // - merge ClientLifecycleState and cluster ConnectionState
            // - how is Disconnected different from NotConnected?
            // - could we try to connect a Disconnected cluster again?

            try
            {
                await _nearCacheManager.DisposeAsync().CAF();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Caught exception while disposing the near cache manager.");
            }

            try
            {
                await _distributedOjects.DisposeAsync().CAF();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Caught exception while disposing the distributed object factory.");
            }

            try
            {
                await Cluster.DisposeAsync().CAF();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Caught exception while disposing the cluster.");
            }
        }
    }
}
