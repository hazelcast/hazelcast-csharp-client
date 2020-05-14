using System;
using System.Threading.Tasks;

namespace Hazelcast.Clustering
{
    // partial: cluster events
    public partial class Cluster
    {
        /// <summary>
        /// Initializes the object lifecycle event.
        /// </summary>
        /// <returns>The object lifecycle event manager.</returns>
        private ObjectLifecycleEventSubscription InitializeObjectLifecycleEventSubscription()
        {
            return new ObjectLifecycleEventSubscription(this, _loggerFactory, IsSmartRouting)
            {
                Handle = OnObjectLifecycleEvent
            };
        }

        /// <summary>
        /// Initializes the partition lost event.
        /// </summary>
        /// <returns>The object lifecycle event manager.</returns>
        private PartitionLostEventSubscription InitializePartitionLostEventSubscription()
        {
            return new PartitionLostEventSubscription(this, _loggerFactory, IsSmartRouting)
            {
                Handle = OnPartitionLost
            };
        }

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="on">An event handlers collection builder.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        public async Task<Guid> SubscribeAsync(Action<ClusterEvents> on)
        {
            if (on == null) throw new ArgumentNullException(nameof(on));

            var subscriber = new ClusterEvents();
            on(subscriber);

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var handler in subscriber.Handlers)
            {
                if (handler is ClusterObjectLifecycleEventHandler)
                    await _objectLifecycleEventSubscription.AddSubscription();
                if (handler is PartitionLostEventHandler)
                    await _partitionLostEventSubscription.AddSubscription();
            }

            var id = Guid.NewGuid();
            _clusterEvents[id] = subscriber;
            return id;
        }

        /// <summary>
        /// Unsubscribe from events.
        /// </summary>
        /// <param name="subscriptionId">The unique identifier of the subscription.</param>
        /// <returns>A task that will complete when ???</returns> FIXME map returns a bool???
        public async Task UnsubscribeAsync(Guid subscriptionId)
        {
            if (!_clusterEvents.TryRemove(subscriptionId, out var clusterEvents))
                return; // or throw?

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var handler in clusterEvents.Handlers)
            {
                if (handler is ClusterObjectLifecycleEventHandler)
                    await _objectLifecycleEventSubscription.RemoveSubscription();
                if (handler is PartitionLostEventHandler)
                    await _partitionLostEventSubscription.RemoveSubscription();
            }
        }
    }
}
