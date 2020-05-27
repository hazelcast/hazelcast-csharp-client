using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Clustering
{
    public partial class Cluster // ClusterEvents
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
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        // TODO: is this a public API? (needing a timeout + etc)
        public async Task<Guid> SubscribeAsync(Action<ClusterEventHandlers> on, CancellationToken cancellationToken)
        {
            if (on == null) throw new ArgumentNullException(nameof(on));

            var handlers = new ClusterEventHandlers();
            on(handlers);

            foreach (var handler in handlers)
            {
                switch (handler)
                {
                    case ClusterObjectLifecycleEventHandler _:
                        await _objectLifecycleEventSubscription.AddSubscription(cancellationToken);
                        break;

                    case PartitionLostEventHandler _:
                        await _partitionLostEventSubscription.AddSubscription(cancellationToken);
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }

            var id = Guid.NewGuid();
            _clusterHandlers[id] = handlers;
            return id;
        }

        /// <summary>
        /// Unsubscribe from events.
        /// </summary>
        /// <param name="subscriptionId">The unique identifier of the subscription.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>Whether the un-registration was successful.</returns>
        // TODO: is this a public API? (needing a timeout + etc)
        public async Task UnsubscribeAsync(Guid subscriptionId, CancellationToken cancellationToken)
        {
            if (!_clusterHandlers.TryRemove(subscriptionId, out var clusterHandlers))
                return;

            foreach (var handler in clusterHandlers)
            {
                switch (handler)
                {
                    case ClusterObjectLifecycleEventHandler _:
                        await _objectLifecycleEventSubscription.RemoveSubscription(cancellationToken);
                        break;

                    case PartitionLostEventHandler _:
                        await _partitionLostEventSubscription.RemoveSubscription(cancellationToken);
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
        }
    }
}
