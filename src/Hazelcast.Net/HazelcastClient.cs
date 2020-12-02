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
        private readonly DistributedObjectFactory _distributedObjectFactory;
        private readonly NearCacheManager _nearCacheManager;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

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

            _distributedObjectFactory = new DistributedObjectFactory(Cluster, serializationService, loggerFactory);
            Cluster.Connections.OnConnectingToNewCluster = cancellationToken => _distributedObjectFactory.CreateAllAsync(cancellationToken);

            _nearCacheManager = new NearCacheManager(cluster, serializationService, loggerFactory, options.NearCache);

            // wire events
            // this way, the cluster does not need to know about the hazelcast client,
            // and we don't have circular dependencies everywhere - cleaner
            cluster.ClusterEvents.OnObjectLifecycleEvent = OnObjectLifecycleEvent;
            cluster.ClusterEvents.OnPartitionLost = OnPartitionLost;
            cluster.ClusterEvents.OnMemberLifecycleEvent = OnMemberLifecycleEvent;
            cluster.ClusterEvents.OnClientLifecycleEvent = OnClientLifecycleEvent;
            cluster.ClusterEvents.OnPartitionsUpdated = OnPartitionsUpdated;
            cluster.ClusterEvents.OnConnectionAdded = OnConnectionAdded;
            cluster.ClusterEvents.OnConnectionRemoved = OnConnectionRemoved;
            cluster.ClusterEvents.OnConnectionRemoved = OnConnectionRemoved;

            cluster.Connections.OnFirstConnection = async cancellationToken =>
            {
                foreach (var subscriber in options.Subscribers)
                    await subscriber.SubscribeAsync(this, cancellationToken).CAF();
            };
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
        /// Gets the <see cref="SerializationService"/>.
        /// </summary>
        public SerializationService SerializationService { get; }

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
            var timeoutMs = timeout.TimeoutMilliseconds(_options.Networking.ConnectionTimeoutMilliseconds);
            var task = TaskEx.RunWithTimeout((c, t) => c.Connections.ConnectAsync(t), Cluster, timeoutMs);

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
            var task = Cluster.Connections.ConnectAsync(cancellationToken);

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
                await _distributedObjectFactory.DisposeAsync().CAF();
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
