﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering.LoadBalancing;
using Hazelcast.Core;
using Hazelcast.Data;
using Hazelcast.Exceptions;
using Hazelcast.Logging;
using Hazelcast.Networking;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;
using Partitioner = Hazelcast.Partitioning.Partitioner;

namespace Hazelcast.Clustering
{
    public partial class Cluster : IAsyncDisposable
    {
        // generates unique cluster identifiers
        private static readonly ISequence<int> ClusterIdSequence = new Int32Sequence();

        // member id -> client
        // the master clients list
        private readonly ConcurrentDictionary<Guid, Client> _clients = new ConcurrentDictionary<Guid, Client>();

        // address -> client
        // used for fast querying of _memberClients by network address
        private readonly ConcurrentDictionary<NetworkAddress, Client> _addressClients = new ConcurrentDictionary<NetworkAddress, Client>();

        // subscription id -> subscription
        // the master subscriptions list
        private readonly ConcurrentDictionary<Guid, ClusterSubscription> _subscriptions = new ConcurrentDictionary<Guid, ClusterSubscription>();

        // correlation id -> subscription
        // used to match a subscription to an incoming event message
        // each client has its own correlation id, so there can be many entries per cluster subscription
        private readonly ConcurrentDictionary<long, ClusterSubscription> _correlatedSubscriptions = new ConcurrentDictionary<long, ClusterSubscription>();

        // subscription id -> event handlers
        // for cluster client-level events (not wired to the server)
        private readonly ConcurrentDictionary<Guid, ClusterEventHandlers> _clusterHandlers = new ConcurrentDictionary<Guid, ClusterEventHandlers>();

        private readonly ISequence<long> _correlationIdSequence;
        private readonly ILoadBalancer _loadBalancer;
        private readonly IAuthenticator _authenticator;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private readonly RetryConfiguration _retryConfiguration;
        private readonly ISet<string> _labels;
        private readonly AddressProvider _addressProvider;
        private readonly ISerializationService _serializationService;
        private readonly ReconnectMode _reconnectMode;
        private readonly bool _retryOnTargetDisconnected;
        private readonly Heartbeat _heartbeat;

        // events subscriptions
        private readonly ObjectLifecycleEventSubscription _objectLifecycleEventSubscription;
        private readonly PartitionLostEventSubscription _partitionLostEventSubscription;

        // configured subscribers
        private IList<IClusterEventSubscriber> _clusterEventSubscribers;

        // _onXxx
        private bool _readonlyProperties; // whether some properties (_onXxx) are readonly
        private Func<CancellationToken, ValueTask> _onConnectionToNewCluster;

        // general cluster lifecycle
        private readonly CancellationTokenSource _clusterCancellation = new CancellationTokenSource(); // general kill switch
        private readonly SemaphoreSlim _clusterLock = new SemaphoreSlim(1, 1); // general lock
        private volatile ClusterState _clusterState = ClusterState.NotConnected; // cluster state
        private volatile bool _disposed; // disposed flag
        private Task _clusterConnectTask; // the task that connects the cluster
        private Task _clusterEventsTask; // the task that ensures there is a client to handle 'cluster events'

        private Client _clusterEventsClient; // the client which handles 'cluster events'
        private long _clusterEventsCorrelationId; // the correlation id of the 'cluster events'

        private MemberTable _memberTable;
        private Guid _clusterServerSideId; // the server-side identifier of the cluster

        private volatile int _firstMembersViewed;
        //private volatile int _firstPartitionsViewed;
        private SemaphoreSlim _firstMembersView = new SemaphoreSlim(0, 1);
        //private SemaphoreSlim _firstPartitionsView = new SemaphoreSlim(0, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="Cluster"/> class.
        /// </summary>
        /// <param name="clusterName">The cluster name.</param>
        /// <param name="clientName">The client name.</param>
        /// <param name="labels">The client labels.</param>
        /// <param name="clusterConfiguration">The cluster configuration.</param>
        /// <param name="networkingConfiguration">The networking configuration.</param>
        /// <param name="loadBalancingConfiguration">The load-balancing configuration.</param>
        /// <param name="securityConfiguration">The security configuration.</param>
        /// <param name="serializationService">The serialization service.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public Cluster(
            string clusterName,
            string clientName,
            ISet<string> labels,

            ClusterConfiguration clusterConfiguration,
            NetworkingConfiguration networkingConfiguration,
            LoadBalancingConfiguration loadBalancingConfiguration,
            SecurityConfiguration securityConfiguration,

            ISerializationService serializationService,
            ILoggerFactory loggerFactory)
        {
            if (clusterConfiguration == null) throw new ArgumentNullException(nameof(clusterConfiguration));
            if (networkingConfiguration == null) throw new ArgumentNullException(nameof(networkingConfiguration));
            if (loadBalancingConfiguration == null) throw new ArgumentNullException(nameof(loadBalancingConfiguration));
            if (securityConfiguration == null) throw new ArgumentNullException(nameof(securityConfiguration));

            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = _loggerFactory.CreateLogger<Cluster>();
            _labels = labels ?? throw new ArgumentNullException(nameof(labels));
            _serializationService = serializationService ?? throw new ArgumentNullException(nameof(serializationService));

            _clusterEventSubscribers = clusterConfiguration.EventSubscribers;
            IsSmartRouting = networkingConfiguration.SmartRouting;
            _retryConfiguration = networkingConfiguration.ConnectionRetry;
            _authenticator = securityConfiguration.Authenticator.Create();
            _loadBalancer = loadBalancingConfiguration.LoadBalancer.Create();
            _addressProvider = new AddressProvider(networkingConfiguration, loggerFactory);
            _reconnectMode = networkingConfiguration.ReconnectMode;
            _retryOnTargetDisconnected = networkingConfiguration.RedoOperation;
            _heartbeat = new Heartbeat(this, loggerFactory);

            // _localOnly is defined in ListenerService and initialized with IsSmartRouting so it's the same
            // it is used by ProxyManager to AddDistributedObjectListener - passing that value

            _correlationIdSequence = new Int64Sequence();
            Partitioner = new Partitioner();

            Name = string.IsNullOrWhiteSpace(clusterName) ? "dev" : clusterName;

            ClientName = string.IsNullOrWhiteSpace(clientName)
                ? "hz.client_" + ClusterIdSequence.Next
                : clientName;

            // setup events
            _objectLifecycleEventSubscription = InitializeObjectLifecycleEventSubscription();
            _partitionLostEventSubscription = InitializePartitionLostEventSubscription();

            HzConsole.Configure(this, config => config.SetIndent(2).SetPrefix("CLUSTER"));
        }

        /// <summary>
        /// Gets the cluster instrumentation.
        /// </summary>
        public ClusterInstrumentation Instrumentation { get; } = new ClusterInstrumentation();

        /// <summary>
        /// Gets the unique identifier of the cluster, as assigned by the client.
        /// </summary>
        public Guid ClientId { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets the name of this cluster client, as assigned by the client.
        /// </summary>
        public string ClientName { get; }

        /// <summary>
        /// Gets the name of the cluster, as assigned by the client.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets an action that will be executed when connecting to a new cluster.
        /// </summary>
        public Func<CancellationToken, ValueTask> OnConnectingToNewCluster
        {
            get => _onConnectionToNewCluster;
            set
            {
                if (_readonlyProperties)
                    throw new InvalidOperationException(ExceptionMessages.PropertyIsNowReadOnly);
                _onConnectionToNewCluster = value;
            }
        }

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
        public bool IsSmartRouting { get; }

        /// <summary>
        /// Gets the partitioner.
        /// </summary>
        public Partitioner Partitioner { get; }

        /// <summary>
        /// Gets the lite members.
        /// </summary>
        public IEnumerable<MemberInfo> LiteMembers => _memberTable.Members.Values.Where(x => x.IsLite);

        /// <summary>
        /// Throws if the cluster is disconnected (wait if it is connecting).
        /// </summary>
        public Task ThrowIfDisconnected()
        {
            if (_disposed || _clusterState != ClusterState.Connected)
                throw new HazelcastClientNotConnectedException();

            // TODO: if connecting, wait?
            return Task.CompletedTask;
        }

        /// <summary>
        /// Dies (dispose without exceptions).
        /// </summary>
        /// <returns>A task that completes whe, the cluster has died.</returns>
        private async ValueTask DieAsync()
        {
            try
            {
                await DisposeAsync().CAF();
            }
            catch (Exception e)
            {
                // that's all we can do really
                _logger.LogWarning(e, "Caught an exception while dying.");
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            var tasks = new List<Task>();

            using (await _clusterLock.AcquireAsync().CAF())
            {
                if (_disposed) return;
                _disposed = true;

                _clusterCancellation.Cancel();

                if (_clusterConnectTask != null)
                    tasks.Add(_clusterConnectTask);
                if (_clusterEventsTask != null)
                    tasks.Add(_clusterEventsTask);
            }

            foreach (var (_, client) in _clients)
            {
                try
                {
                    await client.DisposeAsync().CAF();
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Caught an exception while disposing a client.");
                }
            }

            try
            {
                await Task.WhenAll(tasks).CAF();
            }
            catch (OperationCanceledException)
            {
                // expected
            }

            try
            {
                await _heartbeat.DisposeAsync();
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Caught an exception while disposing the heartbeat.");
            }
        }
    }
}
