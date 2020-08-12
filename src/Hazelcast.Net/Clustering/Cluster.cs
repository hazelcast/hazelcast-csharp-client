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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering.LoadBalancing;
using Hazelcast.Core;
using Hazelcast.Data;
using Hazelcast.Exceptions;
using Hazelcast.Networking;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;
using Partitioner = Hazelcast.Partitioning.Partitioner;

namespace Hazelcast.Clustering
{
    internal partial class Cluster : IAsyncDisposable
    {
        // generates unique cluster identifiers
        private static readonly ISequence<int> ClusterIdSequence = new Int32Sequence();

        // member id -> client
        // the master clients list
        private readonly ConcurrentDictionary<Guid, ClientConnection> _clientConnections = new ConcurrentDictionary<Guid, ClientConnection>();

        // address -> client
        // used for fast querying of _memberClients by network address
        private readonly ConcurrentDictionary<NetworkAddress, ClientConnection> _addressClientConnections = new ConcurrentDictionary<NetworkAddress, ClientConnection>();

        // subscription id -> subscription
        // the master subscriptions list
        private readonly ConcurrentDictionary<Guid, ClusterSubscription> _subscriptions = new ConcurrentDictionary<Guid, ClusterSubscription>();

        // correlation id -> subscription
        // used to match a subscription to an incoming event message
        // each client has its own correlation id, so there can be many entries per cluster subscription
        private readonly ConcurrentDictionary<long, ClusterSubscription> _correlatedSubscriptions = new ConcurrentDictionary<long, ClusterSubscription>();

        private readonly IClusterOptions _options;

        private readonly ISequence<long> _correlationIdSequence;
        private readonly ILoadBalancer _loadBalancer;
        private readonly IAuthenticator _authenticator;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private readonly AddressProvider _addressProvider;
        private readonly ISerializationService _serializationService;
        private readonly Heartbeat _heartbeat;

        // events subscriptions
        private readonly ObjectLifecycleEventSubscription _objectLifecycleEventSubscription;
        private readonly PartitionLostEventSubscription _partitionLostEventSubscription;

        // _onXxx
        private bool _readonlyProperties; // whether some properties (_onXxx) are readonly
        private Func<CancellationToken, ValueTask> _onConnectionToNewCluster;

        // general cluster lifecycle
        private readonly CancellationTokenSource _clusterCancellation = new CancellationTokenSource(); // general kill switch
        private readonly SemaphoreSlim _clusterLock = new SemaphoreSlim(1, 1); // general lock
        private volatile ClusterState _clusterState = ClusterState.NotConnected; // cluster state
        private volatile int _disposed; // disposed flag
        private Task _clusterConnectTask; // the task that connects the first client of the cluster
        private Task _clusterEventsTask; // the task that ensures there is a client to handle 'cluster events'
        private Task _clusterMembersTask; // the task that connects clients for all members of the cluster

        private ClientConnection _clusterEventsClientConnection; // the client which handles 'cluster events'
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
        /// <param name="options">The cluster configuration.</param>
        /// <param name="serializationService">The serialization service.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public Cluster(
            IClusterOptions options,
            ISerializationService serializationService,
            ILoggerFactory loggerFactory)
        {

            _options = options ?? throw new ArgumentNullException(nameof(options));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _serializationService = serializationService ?? throw new ArgumentNullException(nameof(serializationService));

            _logger = _loggerFactory.CreateLogger<Cluster>();
            _correlationIdSequence = new Int64Sequence();

            DefaultOperationTimeoutMilliseconds = options.Messaging.DefaultOperationTimeoutMilliseconds;
            Partitioner = new Partitioner();

            _authenticator = options.Authentication.Authenticator.Service ?? new Authenticator(options.Authentication);
            _loadBalancer = options.LoadBalancing.LoadBalancer.Service ?? new RoundRobinLoadBalancer();

            _addressProvider = new AddressProvider(options.Networking, loggerFactory);
            _heartbeat = new Heartbeat(this, options.Heartbeat, loggerFactory);
            _heartbeat.Start(_clusterCancellation.Token);

            Name = string.IsNullOrWhiteSpace(options.ClusterName) ? "dev" : options.ClusterName;
            ClientName = string.IsNullOrWhiteSpace(options.ClientName)
                ? options.ClientNamePrefix + ClusterIdSequence.GetNext()
                : options.ClientName;

            // setup events
            _objectLifecycleEventSubscription = InitializeObjectLifecycleEventSubscription();
            _partitionLostEventSubscription = InitializePartitionLostEventSubscription();

            HConsole.Configure(this, config => config.SetIndent(2).SetPrefix("CLUSTER"));
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
        /// Gets the default operation timeout in milliseconds.
        /// </summary>
        public int DefaultOperationTimeoutMilliseconds { get; }

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
        public bool IsSmartRouting => _options.Networking.SmartRouting;

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
            if (_disposed == 1 || _clusterState != ClusterState.Connected)
                throw new ClientNotConnectedException();

            // TODO: if connecting, wait?
            return Task.CompletedTask;
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

            // stop heartbeat
            try
            {
                // TODO: make it a task just like the others
                await _heartbeat.DisposeAsync().CAF();
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Caught an exception while disposing the heartbeat.");
            }

            // stop background tasks
            var tasks = new List<Task>();
            using (await _clusterLock.AcquireAsync().CAF())
            {
                _clusterCancellation.Cancel();

                if (_clusterConnectTask != null)
                    tasks.Add(_clusterConnectTask);
                if (_clusterEventsTask != null)
                    tasks.Add(_clusterEventsTask);
                if (_clusterMembersTask != null)
                    tasks.Add(_clusterMembersTask);
            }
            try
            {
                await Task.WhenAll(tasks).CAF();
            }
            catch (OperationCanceledException)
            {
                // expected
            }

            // events
            await _objectLifecycleEventSubscription.DisposeAsync();
            await _partitionLostEventSubscription.DisposeAsync();

            // terminate all clients
            foreach (var (_, clientConnection) in _clientConnections)
            {
                try
                {
                    await clientConnection.DisposeAsync().CAF();
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Caught an exception while disposing a client.");
                }
            }

            _clusterCancellation.Dispose();
            _clusterLock.Dispose();
            var clusterEventsClientConnection = _clusterEventsClientConnection;
            if (clusterEventsClientConnection != null)
                await clusterEventsClientConnection.DisposeAsync().CAF();
            _firstMembersView?.Dispose();
        }
    }
}
