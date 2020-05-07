using System;
using System.Threading.Tasks;
using Hazelcast.Clustering.Events;
using Hazelcast.Core;

namespace Hazelcast.Clustering
{
    // partial: cluster events
    public partial class Cluster
    {
        /// <summary>
        /// Initializes the object lifecycle event.
        /// </summary>
        /// <returns>The object lifecycle event manager.</returns>
        private ObjectLifecycleEvent InitializeObjectLifecycleEvent()
        {
            return new ObjectLifecycleEvent(this, _loggerFactory)
            {
                Handle = (eventType, args) =>
                {
                    foreach (var (_, clusterEvents) in _clusterEvents)
                    foreach (var handler in clusterEvents.Handlers)
                    {
                        if (handler is ClusterObjectLifecycleEventHandler objectEventHandler &&
                            objectEventHandler.EventType == eventType)
                        {
                            objectEventHandler.Handle(this, args);
                        }
                    }
                }
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
                    await _objectLifecycleEvent.AddSubscription();
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
                    await _objectLifecycleEvent.RemoveSubscription();
            }
        }
    }
}
