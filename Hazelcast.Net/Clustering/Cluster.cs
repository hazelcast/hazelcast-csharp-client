using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Hazelcast.Clustering.Events;
using Hazelcast.Clustering.LoadBalancing;
using Hazelcast.Configuration;
using Hazelcast.Core;
using Hazelcast.Data;
using Hazelcast.Eventing;
using Hazelcast.Exceptions;
using Hazelcast.Logging;
using Hazelcast.Messaging;
using Hazelcast.Networking;
using Hazelcast.Security;
using Hazelcast.Partitioning;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;
using Partitioner = Hazelcast.Partitioning.Partitioner;

namespace Hazelcast.Clustering
{
    public partial class Cluster
    {
        private readonly ConcurrentDictionary<NetworkAddress, Client> _clientsBAD
            = new ConcurrentDictionary<NetworkAddress, Client>();
        private readonly ConcurrentDictionary<Guid, Client> _clients
            = new ConcurrentDictionary<Guid, Client>();
        private readonly ConcurrentDictionary<Guid, ClusterEventSubscription> _eventSubscriptions
            = new ConcurrentDictionary<Guid, ClusterEventSubscription>();
        private readonly ConcurrentDictionary<long, ClusterEventSubscription> _correlatedSubscriptions
            = new ConcurrentDictionary<long, ClusterEventSubscription>();
        private readonly ConcurrentDictionary<Guid, ClusterEvents> _clusterEvents
            = new ConcurrentDictionary<Guid, ClusterEvents>();

        private readonly ISequence<long> _correlationIdSequence;
        private readonly ILoadBalancer _loadBalancer;
        private readonly IAuthenticator _authenticator;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ObjectLifecycleEvent _objectLifecycleEvent;
        private readonly IList<IClusterEventSubscriber> _clusterEventSubscribers;

        private Client _clusterEventsClient;
        private MemberTable _memberTable;



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
            _objectLifecycleEvent = InitializeObjectLifecycleEvent();

            XConsole.Configure(this, config => config.SetIndent(2).SetPrefix("CLUSTER"));
        }

        /// <summary>
        /// Gets the unique identifier of the cluster, as assigned by the client.
        /// </summary>
        // TODO: are we getting an identifier from the server, for the cluster?
        public Guid ClientId { get; } = Guid.NewGuid();

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

        // TODO: what's the point of this?
        public async ValueTask Connect()
        {
            await ConnectToCluster();

            // FIXME and also for each new client etc?!
            foreach (var subscriber in _clusterEventSubscribers)
                await subscriber.SubscribeAsync(this);
        }

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
