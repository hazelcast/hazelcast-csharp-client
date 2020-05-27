using System;

namespace Hazelcast.Clustering
{
    public static class ClusterMemberLifecycleEventExtensions
    {
        public static ClusterEventHandlers MemberAdded(this ClusterEventHandlers handlers, Action<Cluster, ClusterMemberLifecycleEventArgs> handler)
        {
            handlers.Add(new ClusterMemberLifecycleEventHandler(ClusterMemberLifecycleEventType.Added, handler));
            return handlers;
        }

        public static ClusterEventHandlers MemberRemoved(this ClusterEventHandlers handlers, Action<Cluster, ClusterMemberLifecycleEventArgs> handler)
        {
            handlers.Add(new ClusterMemberLifecycleEventHandler(ClusterMemberLifecycleEventType.Removed, handler));
            return handlers;
        }
    }
}
