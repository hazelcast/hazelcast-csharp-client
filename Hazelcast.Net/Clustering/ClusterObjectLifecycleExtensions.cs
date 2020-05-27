using System;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Provides extension to the <see cref="ClusterEventHandlers"/> class.
    /// </summary>
    public static class ClusterObjectLifecycleExtensions
    {
        /// <summary>
        /// Adds an handler for cluster object creation events.
        /// </summary>
        /// <returns>The cluster events.</returns>
        public static ClusterEventHandlers ObjectCreated(this ClusterEventHandlers handlers, Action<Cluster, ClusterObjectLifecycleEventArgs> handler)
        {
            handlers.Add(new ClusterObjectLifecycleEventHandler(ClusterObjectLifecycleEventType.Created, handler));
            return handlers;
        }

        /// <summary>
        /// Adds an handler for cluster object destruction events.
        /// </summary>
        /// <returns>The cluster events.</returns>
        public static ClusterEventHandlers ObjectDestroyed(this ClusterEventHandlers handlers, Action<Cluster, ClusterObjectLifecycleEventArgs> handler)
        {
            handlers.Add(new ClusterObjectLifecycleEventHandler(ClusterObjectLifecycleEventType.Destroyed, handler));
            return handlers;
        }
    }
}
