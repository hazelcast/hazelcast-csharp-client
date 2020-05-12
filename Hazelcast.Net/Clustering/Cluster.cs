using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering.Events;
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
    public partial class Cluster
    {
        private readonly ConcurrentDictionary<Guid, Client> _memberClients
            = new ConcurrentDictionary<Guid, Client>();
        private readonly ConcurrentDictionary<NetworkAddress, Client> _addressClients
            = new ConcurrentDictionary<NetworkAddress, Client>();
        private readonly ConcurrentDictionary<Guid, ClusterSubscription> _eventSubscriptions
            = new ConcurrentDictionary<Guid, ClusterSubscription>();
        private readonly ConcurrentDictionary<long, ClusterSubscription> _correlatedSubscriptions
            = new ConcurrentDictionary<long, ClusterSubscription>();
        private readonly ConcurrentDictionary<Guid, ClusterEvents> _clusterEvents
            = new ConcurrentDictionary<Guid, ClusterEvents>();

        private readonly ISequence<long> _correlationIdSequence;
        private readonly ILoadBalancer _loadBalancer;
        private readonly IAuthenticator _authenticator;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ObjectLifecycleEventSubscription _objectLifecycleEventSubscription;
        private readonly PartitionLostEventSubscription _partitionLostEventSubscription;
        private readonly IList<IClusterEventSubscriber> _clusterEventSubscribers;

        private bool _readonlyProperties;
        private Func<ValueTask> _onConnectionToNewCluster;
        private Client _clusterEventsClient;
        private MemberTable _memberTable;
        private Guid _clusterId;
        private ClusterState _state;
        private volatile int _firstMembersViewed;
        private volatile int _firstpartitionsViewed;
        private SemaphoreSlim _firstMembersView = new SemaphoreSlim(0);
        private SemaphoreSlim _firstPartitionsView = new SemaphoreSlim(0);

        private enum ClusterState // FIXME move to partial, etc
        {
            Unknown = 0,
            NotConnected,
            Connecting,
            Connected
        }

        public Cluster(IAuthenticator authenticator, IList<IClusterEventSubscriber> clusterEventSubscribers, ILoggerFactory loggerFactory)
        {
            _authenticator = authenticator ?? throw new ArgumentNullException(nameof(authenticator));
            _clusterEventSubscribers = clusterEventSubscribers ?? throw new ArgumentNullException(nameof(clusterEventSubscribers));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

            // TODO: get isSmartRouting from configuration
            // TODO: can we avoid passing the serializationService?

            ISerializationService serializationService = null;

            // _localOnly is defined in ListenerService and initialized with IsSmartRouting so it's the same
            // it is used by ProxyManager to AddDistributedObjectListener - passing that value
            var isSmartRouting = true;

            _correlationIdSequence = new Int64Sequence();
            IsSmartRouting = isSmartRouting;
            Partitioner = new Partitioner(serializationService, isSmartRouting);
            _loadBalancer = new RandomLoadBalancer();

            // setup events
            _objectLifecycleEventSubscription = InitializeObjectLifecycleEventSubscription();
            _partitionLostEventSubscription = InitializePartitionLostEventSubscription();

            XConsole.Configure(this, config => config.SetIndent(2).SetPrefix("CLUSTER"));
        }

        /// <summary>
        /// Gets the unique identifier of the cluster, as assigned by the client.
        /// </summary>
        // TODO: are we getting an identifier from the server, for the cluster?
        public Guid ClientId { get; } = Guid.NewGuid();

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
