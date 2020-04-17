using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering.LoadBalancing;
using Hazelcast.Core;
using Hazelcast.Data;
using Hazelcast.Eventing;
using Hazelcast.Exceptions;
using Hazelcast.Logging;
using Hazelcast.Messaging;
using Hazelcast.Networking;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Security;
using Hazelcast.Partitioning;
using Hazelcast.Serialization;
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
        private readonly ConcurrentDictionary<long, Action<ClientMessage>> _eventHandlers
            = new ConcurrentDictionary<long, Action<ClientMessage>>();

        private readonly ISequence<long> _correlationIdSequence;

        private readonly Partitioner _partitioner;
        private readonly ILoadBalancer _loadBalancer;

        private Client _clusterEventsClient;
        private MemberTable _memberTable;

        public Cluster()
        {
            // TODO: get isSmartRouting from configuration
            // TODO: can we avoid passing the serializationService?

            ISerializationService serializationService = null;

            // _localOnly is defined in ListenerService and initialized with IsSmartRouting so it's the same
            // it is used by ProxyManager to AddDistributedObjectListener - passing that value
            var isSmartRouting = true;

            _correlationIdSequence = new Int64Sequence();
            IsSmartRouting = isSmartRouting;
            _partitioner = new Partitioner(serializationService, isSmartRouting);
            _loadBalancer = new RandomLoadBalancer();
        }

        /// <summary>
        /// Occurs when a member has been added to or removed from the cluster.
        /// </summary>
        public MixedEvent<MembershipEventArgs> MemberAddedOrRemoved { get; } = new MixedEvent<MembershipEventArgs>();

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
        }



    }
}
