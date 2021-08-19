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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.CP;
using Hazelcast.DistributedObjects;
using Hazelcast.Events;
using Hazelcast.Metrics;
using Hazelcast.NearCaching;
using Hazelcast.Serialization;
using Hazelcast.Sql;
using Microsoft.Extensions.Logging;
using MemberInfo = Hazelcast.Models.MemberInfo;

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
        private readonly SqlService _sqlService;

        private int _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastClient"/> class.
        /// </summary>
        /// <param name="options">The client configuration.</param>
        /// <param name="cluster">A cluster.</param>
        /// <param name="serializationService">A serialization service.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public HazelcastClient(HazelcastOptions options, Cluster cluster, SerializationService serializationService, ILoggerFactory loggerFactory)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            Cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
            SerializationService = serializationService ?? throw new ArgumentNullException(nameof(serializationService));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<IHazelcastClient>();

            _distributedOjects = new DistributedObjectFactory(Cluster, serializationService, loggerFactory);
            _nearCacheManager = new NearCacheManager(cluster, serializationService, loggerFactory, options.NearCache);
            CPSubsystem = new CPSubsystem(cluster, serializationService);
            Sql = new SqlService(cluster, serializationService);

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
            // assigning multi-cast handlers *must* use +=

            // when an object is created/destroyed, trigger the user-level events
            Cluster.Events.ObjectCreated
                += Trigger<DistributedObjectCreatedEventHandler, DistributedObjectCreatedEventArgs>;

            Cluster.Events.ObjectDestroyed
                += Trigger<DistributedObjectDestroyedEventHandler, DistributedObjectDestroyedEventArgs>;

            // when client/cluster state changes, trigger user-level events
            Cluster.State.StateChanged
                += state => Trigger<StateChangedEventHandler, StateChangedEventArgs>(new StateChangedEventArgs(state));

            // when partitions are updated, trigger the user-level event
            Cluster.Events.PartitionsUpdated
                += Trigger<PartitionsUpdatedEventHandler>;

            // when a partition is lost, trigger the user-level event
            Cluster.Events.PartitionLost
                += Trigger<PartitionLostEventHandler, PartitionLostEventArgs>;

            // when members are updated, trigger the user-level event
            Cluster.Events.MembersUpdated
                += Trigger<MembersUpdatedEventHandler, MembersUpdatedEventArgs>;

            // when a connection is closed, trigger the user-level event
            Cluster.Connections.ConnectionClosed
                += conn => Trigger<ConnectionClosedEventHandler, ConnectionClosedEventArgs>(new ConnectionClosedEventArgs(conn));

            // when a connection is opened, DistributedObjects.OnConnectionOpened checks
            // whether it is the first connection to a new cluster, and then re-creates
            // all the known distributed object so far on the new cluster.
            Cluster.Connections.ConnectionOpened
                += _distributedOjects.OnConnectionOpened;

            // when a connection is opened, trigger the user-level event.
            Cluster.Connections.ConnectionOpened
                += (conn, isFirstEver, isFirst, isNewCluster) => Trigger<ConnectionOpenedEventHandler, ConnectionOpenedEventArgs>(new ConnectionOpenedEventArgs(conn, isNewCluster));
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
        // yes this is not really thread-safe but we don't care
        public HazelcastOptions Options => _optionsClone ??= _options.Clone();
        
        /// <inheritdoc />
        public IReadOnlyCollection<MemberInfo> Members => Cluster.Members.GetMembers().ToList();

        /// <inheritdoc />
        public bool IsActive => Cluster.IsActive;

        /// <inheritdoc />
        public bool IsConnected => Cluster.IsConnected;

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
