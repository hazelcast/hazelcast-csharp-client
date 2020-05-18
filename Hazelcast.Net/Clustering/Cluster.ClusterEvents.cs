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
        public async Task<Guid> SubscribeAsync(Action<ClusterEventHandlers> on)
        {
            if (on == null) throw new ArgumentNullException(nameof(on));

            var handlers = new ClusterEventHandlers();
            on(handlers);

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var handler in handlers)
            {
                if (handler is ClusterObjectLifecycleEventHandler)
                    await _objectLifecycleEventSubscription.AddSubscription();
                if (handler is PartitionLostEventHandler)
                    await _partitionLostEventSubscription.AddSubscription();
            }

            var id = Guid.NewGuid();
            _clusterHandlers[id] = handlers;
            return id;
        }

        /// <summary>
        /// Unsubscribe from events.
        /// </summary>
        /// <param name="subscriptionId">The unique identifier of the subscription.</param>
        /// <returns>Whether the un-registration was successful.</returns>
        public async Task UnsubscribeAsync(Guid subscriptionId)
        {
            if (!_clusterHandlers.TryRemove(subscriptionId, out var clusterHandlers))
                return;

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var handler in clusterHandlers)
            {
                switch (handler)
                {
                    case ClusterObjectLifecycleEventHandler _:
                        await _objectLifecycleEventSubscription.RemoveSubscription();
                        break;

                    case PartitionLostEventHandler _:
                        await _partitionLostEventSubscription.RemoveSubscription();
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
        }
    }
}
