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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering.LoadBalancing;
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

        private readonly IClusterOptions _options;
        private readonly ClusterState _clusterState;

        private readonly ILogger _logger;
        private readonly Heartbeat _heartbeat;

        // general cluster lifecycle
        private volatile int _disposed; // disposed flag

        /// <summary>
        /// Initializes a new instance of the <see cref="Cluster"/> class.
        /// </summary>
        /// <param name="options">The cluster configuration.</param>
        /// <param name="serializationService">The serialization service.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public Cluster(
            IClusterOptions options,
            ISerializationService serializationService,
            ILoggerFactory loggerFactory)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            if (serializationService == null) throw new ArgumentNullException(nameof(serializationService));
            if (loggerFactory is null) throw new ArgumentNullException(nameof(loggerFactory));

            _logger = loggerFactory.CreateLogger<Cluster>();

            DefaultOperationTimeoutMilliseconds = options.Messaging.DefaultOperationTimeoutMilliseconds;
            Partitioner = new Partitioner();

            var loadBalancer = options.LoadBalancing.LoadBalancer.Service ?? new RoundRobinLoadBalancer();

            var clientName = string.IsNullOrWhiteSpace(options.ClientName)
                ? options.ClientNamePrefix + ClusterIdSequence.GetNext()
                : options.ClientName;

            var clusterName = string.IsNullOrWhiteSpace(options.ClusterName) ? "dev" : options.ClusterName;

            _clusterState = new ClusterState(options, clusterName, clientName, Partitioner, loadBalancer, loggerFactory);

            Members = new ClusterMembers(_clusterState);
            Messaging = new ClusterMessaging(_clusterState, Members);
            Events = new ClusterEvents(_clusterState, Messaging, Members);
            ClusterEvents = new ClusterClusterEvents(_clusterState, Members, Events);
            Connections = new ClusterConnections(_clusterState, ClusterEvents, Events, Members, serializationService, TerminateAsync);

            _heartbeat = new Heartbeat(_clusterState, Members, Messaging, options.Heartbeat, loggerFactory);
            _heartbeat.Start();

            HConsole.Configure(x => x.Set(this, config => config.SetIndent(2).SetPrefix("CLUSTER")));
        }

        /// <summary>
        /// Gets the unique identifier of the cluster, as assigned by the client.
        /// </summary>
        public Guid ClientId => _clusterState.ClientId;

        /// <summary>
        /// Gets the cluster instrumentation.
        /// </summary>
        public ClusterInstrumentation Instrumentation => _clusterState.Instrumentation;

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
        /// Gets the default operation timeout in milliseconds.
        /// </summary>
        public int DefaultOperationTimeoutMilliseconds { get; }

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
        public bool IsSmartRouting => _options.Networking.SmartRouting;

        /// <summary>
        /// Whether the cluster is connected.
        /// </summary>
        public bool IsConnected => _clusterState.IsConnected;

        /// <summary>
        /// Whether the cluster is active.
        /// </summary>
        public bool IsActive => _disposed == 0;

        /// <summary>
        /// Gets the partitioner.
        /// </summary>
        public Partitioner Partitioner { get; }

        /// <summary>
        /// Throws if the cluster is disconnected (wait if it is connecting).
        /// </summary>
        public ValueTask ThrowIfNotConnected()
        {
            // method is async because eventually it should or at least may wait
            // if the cluster is (re) connecting, to block calls somehow until the
            // cluster is (re) connected.

            _clusterState.ThrowIfNotConnected();
            return default;
        }

        /// <summary>
        /// Terminates (dispose without exceptions).
        /// </summary>
        /// <returns>A task that completes when the cluster has terminated.</returns>
        private async ValueTask TerminateAsync()
        {
            try
            {
                await DisposeAsync().CAF();
            }
            catch (Exception e)
            {
                // that's all we can do really
                _logger.LogWarning(e, "Caught an exception while terminating.");
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
                return;

            // cancel operations,
            // stops background tasks
            // TODO: explain the lock
            using (await _clusterState.ClusterLock.AcquireAsync().CAF())
            {
                _clusterState.CancelOperations();
            }

            await Connections.DisposeAsync().CAF();
            await ClusterEvents.DisposeAsync().CAF();
            await Events.DisposeAsync().CAF();
            await Members.DisposeAsync().CAF();
            await _heartbeat.DisposeAsync().CAF();

            _clusterState.Dispose();
        }
    }
}
