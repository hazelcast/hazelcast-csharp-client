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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Configuration;
using Hazelcast.Core;
using Hazelcast.CP;
using Hazelcast.DistributedObjects;
using Hazelcast.Events;
using Hazelcast.Metrics;
using Hazelcast.Models;
using Hazelcast.NearCaching;
using Hazelcast.Serialization;
using Hazelcast.Sql;
using Microsoft.Extensions.Logging;

namespace Hazelcast
{
    /// <summary>
    /// Represents the Hazelcast client.
    /// </summary>
    internal partial class HazelcastClient : IHazelcastClient
    {
        private readonly HazelcastOptions _options;
        private HazelcastOptions _optionsClone;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        private readonly DistributedObjectFactory _distributedOjects;
        private readonly NearCacheManager _nearCacheManager;
        private readonly MetricsPublisher _metricsPublisher;

        private int _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastClient"/> class.
        /// </summary>
        /// <param name="options">The client configuration.</param>
        /// <param name="cluster">A cluster.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public HazelcastClient(HazelcastOptions options, Cluster cluster, ILoggerFactory loggerFactory)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            Cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
            SerializationService = Cluster.SerializationService;
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<IHazelcastClient>();

            _distributedOjects = new DistributedObjectFactory(Cluster, SerializationService, loggerFactory);
            _nearCacheManager = new NearCacheManager(cluster, SerializationService, loggerFactory, options.NearCache);
            CPSubsystem = new CPSubsystem(cluster, SerializationService, loggerFactory);

            // This option is internal and bound to SmartRouting for now.
            options.Sql.ArgumentIndexCachingEnabled = options.Networking.SmartRouting;
            Sql = new SqlService(options.Sql, cluster, SerializationService, loggerFactory);

            if (options.Metrics.Enabled)
            {
                _metricsPublisher = new MetricsPublisher(cluster, options.Metrics, loggerFactory);
                _metricsPublisher.AddSource(new ClientMetricSource(cluster, loggerFactory));
                _metricsPublisher.AddSource(_nearCacheManager);
            }

            // wire components
            WireComponents();
        }

        private void WireComponents()
        {
            // this is the *only* place where independent components are wired together
            //
            // assigning multi-cast handlers *must* use +=
            // and then, handlers will run in the order they have been assigned
            //
            // this order is *important* for instance when a connection is opened we *must*
            // publish compact schemas (if it is the first connection to a cluster) *before*
            // notifying ClusterMembers, i.e. before the client state turns to 'connected'
            // and client-level invocations (e.g. map.put) are allowed.
            //
            // the .NET client does not implement "urgent" invocations - properly ordering
            // handlers here achieve the same result: separating "system-level" invocations
            // from "user-level" invocations -- which require the client to be in the
            // 'connected' state to run.

            // before sending a message, ensure we send the required compact schemas, if any
            Cluster.Messaging.SendingMessage += SerializationService.CompactSerializer.BeforeSendingMessage;

            // when a connection is created, wire its ReceivedEvent -> Events.OnReceivedEvent in order to handle events
            Cluster.Connections.ConnectionCreated += Cluster.Events.OnConnectionCreated;

            // when a distributed object is created, trigger the user-level event
            Cluster.Events.ObjectCreated += Trigger<DistributedObjectCreatedEventHandler, DistributedObjectCreatedEventArgs>;

            // when a distributed object is destroyed, trigger the user-level event
            Cluster.Events.ObjectDestroyed += Trigger<DistributedObjectDestroyedEventHandler, DistributedObjectDestroyedEventArgs>;

            // when client/cluster state changes, trigger user-level events
            Cluster.State.StateChanged += state => Trigger<StateChangedEventHandler, StateChangedEventArgs>(new StateChangedEventArgs(state));

            // when partitions are updated, trigger the user-level event
            Cluster.Events.PartitionsUpdated += Trigger<PartitionsUpdatedEventHandler>;

            // when a partition is lost, trigger the user-level event
            Cluster.Events.PartitionLost += Trigger<PartitionLostEventHandler, PartitionLostEventArgs>;

            // when members are updated, trigger the user-level event
            Cluster.Events.MembersUpdated += Trigger<MembersUpdatedEventHandler, MembersUpdatedEventArgs>;

            // -- ConnectionClosed -- order is *important* --

            // when a connection is closed, notify the heartbeat service
            Cluster.Connections.ConnectionClosed += conn => { Cluster.Heartbeat.RemoveConnection(conn); return default; };

            // when a connection is closed, notify the members service (may change the client state)
            Cluster.Connections.ConnectionClosed += async conn => { await Cluster.Members.RemoveConnectionAsync(conn).CfAwait(); };

            // when a connection is closed, clears the associated subscriptions + ensure there is a cluster views connection
            //
            Cluster.Connections.ConnectionClosed += Cluster.Events.OnConnectionClosed;

            // when a connection is closed, trigger the user-level event
            Cluster.Connections.ConnectionClosed += conn => Trigger<ConnectionClosedEventHandler, ConnectionClosedEventArgs>(new ConnectionClosedEventArgs(conn));

            // -- ConnectionOpened -- order is *important* --

            // when the first connection to a cluster is opened, distributed objects may be re-created, compact
            // schemas may be published, etc. this is all performed using the new connection, which should be
            // stable. if it dies, then the client ends up disconnected again, and everything starts from scratch.
            //
            // while attempting the first connection, there is no members-connection task running, so there cannot
            // be any other connection: it is not possible that this new connection dies and the client ends up
            // running with other connections, but not fully initialized.
            //
            // note: because event subscriptions are installed before Cluster.Members is notified of the connection
            // and can change the client state to 'connected', the client can potentially receive events before
            // it is considered 'connected' and can issue user-level invocations. This happens too in Java.

            // when the first connection to a cluster is opened, make sure we recreate the distributed objects
            Cluster.Connections.ConnectionOpened += _distributedOjects.OnConnectionOpened;

            // when the first connection to a cluster is opened, make sure we republish all schemas
            Cluster.Connections.ConnectionOpened += Cluster.SerializationService.CompactSerializer.Schemas.OnConnectionOpened;

            // when a connection is opened, install event subscriptions + ensure there is a cluster views connection
            Cluster.Connections.ConnectionOpened += Cluster.Events.OnConnectionOpened;
            
            // when a connection is opened, notify the heartbeat service
            Cluster.Connections.ConnectionOpened += (conn, _, _, _, _) => { Cluster.Heartbeat.AddConnection(conn); return default; };

            // when the first connection is opened, make sure version is set.
            Cluster.Connections.ConnectionOpened += (_, isFirstEver, _, _, clusterVersion) =>
            {
                if(isFirstEver)
                    Cluster.State.ChangeClusterVersion(clusterVersion);
                return default;
            };
            
            // and now, we can change the client state and let user-level invocations go through

            // when a connection is opened, notify the members service (may change the client state)
            Cluster.Connections.ConnectionOpened += (conn, _, _, isNewCluster, _) => { Cluster.Members.AddConnection(conn, isNewCluster); return default; };

            // when a connection is opened, trigger the user-level event.
            Cluster.Connections.ConnectionOpened += (conn, _, _, isNewCluster, _) => Trigger<ConnectionOpenedEventHandler, ConnectionOpenedEventArgs>(new ConnectionOpenedEventArgs(conn, isNewCluster));
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
        public ClusterVersion ClusterVersion => Cluster.State.ClusterVersion;

        /// <inheritdoc />
        // yes this is not really thread-safe but we don't care
        public HazelcastOptions Options => _optionsClone ??= _options.Clone();

        /// <inheritdoc />
        public IReadOnlyCollection<MemberInfoState> Members => Cluster.Members.GetMembersAndState().ToList();

        /// <inheritdoc />
        public bool IsActive => Cluster.IsActive;

        /// <inheritdoc />
        public bool IsConnected => Cluster.IsConnected;

        /// <inheritdoc />
        public ClientState State => Cluster.State.ClientState;

        /// <inheritdoc />
        public DynamicOptions DynamicOptions => new(this);

        /// <summary>
        /// Gets the <see cref="SerializationService"/>.
        /// </summary>
        public SerializationService SerializationService { get; }

        /// <summary>
        /// Starts the client by connecting to the remote cluster.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the client is connected.</returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // before anything else, run all subscribers - we don't have connections yet
            // but that does not matter, the subscriptions will be registered (for server-side
            // events) and added when possible - for local (client) events such as 'member
            // updated'... no server interaction is required

            // subscribe
            foreach (var subscriber in _options.Subscribers)
            {
                // NOTE: consider storing the id for later removal?
                var subscriptionId = await SubscribeAsync(subscriber.Build).CfAwait();
            }

            // connect the cluster
            await Cluster.ConnectAsync(cancellationToken).CfAwait();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
                return;

            // order is important, must dispose the cluster last, as it will tear down
            // connections that may be required by other things being disposed

            try
            {
                if (_metricsPublisher != null) await _metricsPublisher.DisposeAsync().CfAwait();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Caught exception while disposing the metrics publisher.");
            }

            try
            {
                await _nearCacheManager.DisposeAsync().CfAwait();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Caught exception while disposing the near cache manager.");
            }

            try
            {
                await ((CPSubsystem)CPSubsystem).DisposeAsync().CfAwait();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Caught exception while disposing the CPSubsystem.");
            }

            try
            {
                await _distributedOjects.DisposeAsync().CfAwait();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Caught exception while disposing the distributed object factory.");
            }

            try
            {
                await Cluster.DisposeAsync().CfAwait();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Caught exception while disposing the cluster.");
            }
        }
    }
}
