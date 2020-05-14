using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering.LoadBalancing;
using Hazelcast.Configuration;
using Hazelcast.Core;
using Hazelcast.Data;
using Hazelcast.Exceptions;
using Hazelcast.Logging;
using Hazelcast.Networking;
using Hazelcast.Security;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;
using Partitioner = Hazelcast.Partitioning.Partitioner;

namespace Hazelcast.Clustering
{
    public partial class Cluster
    {
        private static readonly ISequence<int> ClusterIdSequence = new Int32Sequence();

        // member id -> client
        private readonly ConcurrentDictionary<Guid, Client> _memberClients = new ConcurrentDictionary<Guid, Client>();

        // address -> client
        private readonly ConcurrentDictionary<NetworkAddress, Client> _addressClients = new ConcurrentDictionary<NetworkAddress, Client>();

        // subscription id -> subscription
        private readonly ConcurrentDictionary<Guid, ClusterSubscription> _eventSubscriptions = new ConcurrentDictionary<Guid, ClusterSubscription>();

        // correlation id -> subscription
        private readonly ConcurrentDictionary<long, ClusterSubscription> _correlatedSubscriptions = new ConcurrentDictionary<long, ClusterSubscription>();

        // subscription id -> event handlers
        private readonly ConcurrentDictionary<Guid, ClusterEvents> _clusterEvents = new ConcurrentDictionary<Guid, ClusterEvents>();

        private readonly ISequence<long> _correlationIdSequence;
        private readonly ILoadBalancer _loadBalancer;
        private readonly IAuthenticator _authenticator;
        private readonly ILoggerFactory _loggerFactory;
        private readonly RetryConfiguration _retryConfiguration;
        private readonly ISet<string> _labels;
        private readonly AddressProvider _addressProvider;

        private readonly ObjectLifecycleEventSubscription _objectLifecycleEventSubscription;
        private readonly PartitionLostEventSubscription _partitionLostEventSubscription;
        private readonly IList<IClusterEventSubscriber> _clusterEventSubscribers;

        private bool _readonlyProperties; // whether some properties (_onXxx) are readonly
        private Func<ValueTask> _onConnectionToNewCluster;

        private Client _clusterEventsClient; // the client which handles 'cluster events'
        private MemberTable _memberTable;
        private Guid _clusterServerSideId; // the server-side identifier of the cluster
        private ClusterState _state;

        private volatile int _firstMembersViewed;
        private volatile int _firstpartitionsViewed;
        private SemaphoreSlim _firstMembersView = new SemaphoreSlim(0);
        private SemaphoreSlim _firstPartitionsView = new SemaphoreSlim(0);

        private enum ClusterState // FIXME move to partial, etc + usage?
        {
            Unknown = 0,
            NotConnected,
            Connecting,
            Connected
        }

        // TODO: refactor this ctor entirely
        // avoid passing serialization service, etc
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
            _labels = labels ?? throw new ArgumentNullException(nameof(labels));

            _clusterEventSubscribers = clusterConfiguration.EventSubscribers;
            IsSmartRouting = networkingConfiguration.SmartRouting;
            _retryConfiguration = networkingConfiguration.ConnectionRetry;
            _authenticator = securityConfiguration.Authenticator.Create();
            _loadBalancer = loadBalancingConfiguration.LoadBalancer.Create();
            _addressProvider = new AddressProvider(networkingConfiguration, loggerFactory);

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

            XConsole.Configure(this, config => config.SetIndent(2).SetPrefix("CLUSTER"));
        }

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
        public Func<ValueTask> OnConnectingToNewCluster
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
        public bool IsSmartRouting { get; } = true;

        /// <summary>
        /// Gets the partitioner.
        /// </summary>
        public Partitioner Partitioner { get; }

        /// <summary>
        /// Gets the lite members.
        /// </summary>
        public IEnumerable<MemberInfo> LiteMembers => _memberTable.Members.Values.Where(x => x.IsLite);
    }
}
