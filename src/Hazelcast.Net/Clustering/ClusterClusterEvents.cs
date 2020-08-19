using System;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Events;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Provides the cluster-level events management service for a cluster.
    /// </summary>
    internal class ClusterClusterEvents : IAsyncDisposable
    {
        private readonly ClusterState _clusterState;
        private readonly ClusterEvents _clusterEvents;

        private readonly ObjectLifecycleEventSubscription _objectLifecycleEventSubscription;
        private readonly PartitionLostEventSubscription _partitionLostEventSubscription;

        private Func<DistributedObjectLifecycleEventType, DistributedObjectLifecycleEventArgs, ValueTask> _onObjectLifeCycleEvent;
        private Func<PartitionLostEventArgs, ValueTask> _onPartitionLost;
        private Func<ClientLifecycleState, ValueTask> _onClientLifecycleEvent;
        private Func<ValueTask> _onConnectionAdded;
        private Func<ValueTask> _onConnectionRemoved;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterClusterEvents"/> class.
        /// </summary>
        /// <param name="clusterState">The cluster state.</param>
        /// <param name="clusterMembers">The cluster members.</param>
        /// <param name="clusterEvents">The cluster events.</param>
        public ClusterClusterEvents(ClusterState clusterState, ClusterMembers clusterMembers, ClusterEvents clusterEvents)
        {
            _clusterState = clusterState;
            _clusterEvents = clusterEvents;

            _objectLifecycleEventSubscription = new ObjectLifecycleEventSubscription(_clusterState, clusterEvents)
            {
                Handle = (eventType, args) => _onObjectLifeCycleEvent(eventType, args)
            };

            _partitionLostEventSubscription = new PartitionLostEventSubscription(_clusterState, clusterEvents, clusterMembers)
            {
                Handle = args => _onPartitionLost(args)
            };
        }

        /// <summary>
        /// Gets or sets the function that triggers an object lifecycle event.
        /// </summary>
        public Func<DistributedObjectLifecycleEventType, DistributedObjectLifecycleEventArgs, ValueTask> OnObjectLifecycleEvent
        {
            get => _onObjectLifeCycleEvent;
            set
            {
                _clusterState.ThrowIfReadOnlyProperties();
                _onObjectLifeCycleEvent = value;
            }
        }

        /// <summary>
        /// Gets or sets the function that triggers a partition list event.
        /// </summary>
        public Func<PartitionLostEventArgs, ValueTask> OnPartitionLost
        {
            get => _onPartitionLost;
            set
            {
                _clusterState.ThrowIfReadOnlyProperties();
                _onPartitionLost = value;
            }
        }

        /// <summary>
        /// Gets or sets the function that triggers a member lifecycle event.
        /// </summary>
        public Func<MemberLifecycleEventType, MemberLifecycleEventArgs, ValueTask> OnMemberLifecycleEvent
        {
            get => _clusterEvents.OnMemberLifecycleEvent;
            set => _clusterEvents.OnMemberLifecycleEvent = value;
        }

        /// <summary>
        /// Gets or sets the function that triggers a client lifecycle event.
        /// </summary>
        public Func<ClientLifecycleState, ValueTask> OnClientLifecycleEvent
        {
            get => _onClientLifecycleEvent;
            set
            {
                _clusterState.ThrowIfReadOnlyProperties();
                _onClientLifecycleEvent = value;
            }
        }

        /// <summary>
        /// Gets or sets the function that triggers a partitions updated event.
        /// </summary>
        public Func<ValueTask> OnPartitionsUpdated
        {
            get => _clusterEvents.OnPartitionsUpdated;
            set => _clusterEvents.OnPartitionsUpdated = value;
        }

        /// <summary>
        /// Gets or sets the function that triggers a connection added event.
        /// </summary>
        public Func<ValueTask> OnConnectionAdded
        {
            get => _onConnectionAdded;
            set
            {
                _clusterState.ThrowIfReadOnlyProperties();
                _onConnectionAdded = value;
            }
        }

        /// <summary>
        /// Gets or sets the function that triggers a connection removed event.
        /// </summary>
        public Func<ValueTask> OnConnectionRemoved
        {
            get => _onConnectionRemoved;
            set
            {
                _clusterState.ThrowIfReadOnlyProperties();
                _onConnectionRemoved = value;
            }
        }

        /// <summary>
        /// Adds an object lifecycle event subscription.
        /// </summary>
        public Task AddObjectLifecycleSubscription()
            => _objectLifecycleEventSubscription.AddSubscription();

        /// <summary>
        /// Adds a partition lost event subscription.
        /// </summary>
        public Task AddPartitionLostSubscription()
            => _partitionLostEventSubscription.AddSubscription();

        /// <summary>
        /// Removes an object lifecycle event subscription.
        /// </summary>
        public ValueTask<bool> RemoveObjectLifecycleSubscription()
            => _objectLifecycleEventSubscription.RemoveSubscription();

        /// <summary>
        /// Removes a partition lost event subscription.
        /// </summary>
        public ValueTask<bool> RemovePartitionLostSubscription()
            => _partitionLostEventSubscription.RemoveSubscription();



        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await _objectLifecycleEventSubscription.DisposeAsync().CAF();
            await _partitionLostEventSubscription.DisposeAsync().CAF();
        }
    }
}
