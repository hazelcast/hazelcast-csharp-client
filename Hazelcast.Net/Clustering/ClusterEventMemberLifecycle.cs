using System;
using Hazelcast.Data;

namespace Hazelcast.Clustering
{
    public enum ClusterMemberLifecycleEventType
    {
        Nothing,
        Added,
        Removed
    }

    public class ClusterMemberLifecycleEventArgs
    {
        public ClusterMemberLifecycleEventArgs(MemberInfo member)
        {
            Member = member;
        }

        public MemberInfo Member { get; }

        // TODO: original event args (MembershipEvent class) also carried the complete list of members
    }

    internal class ClusterMemberLifecycleEventHandler : IClusterEventHandler
    {
        private readonly Action<Cluster, ClusterMemberLifecycleEventArgs> _handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterObjectLifecycleEventHandler"/> class.
        /// </summary>
        /// <param name="eventType">The type of the event.</param>
        /// <param name="handler">An action to execute</param>
        public ClusterMemberLifecycleEventHandler(ClusterMemberLifecycleEventType eventType, Action<Cluster, ClusterMemberLifecycleEventArgs> handler)
        {
            EventType = eventType;
            _handler = handler;
        }

        /// <summary>
        /// Gets the type of the event.
        /// </summary>
        public ClusterMemberLifecycleEventType EventType { get; }

        /// <summary>
        /// Handles the event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="member">The member.</param>
        /// <param name="serviceName">The unique name of the service managing the object.</param>
        /// <param name="name">The unique name of the object.</param>
        public void Handle(Cluster sender, MemberInfo member)
            => _handler(sender, CreateEventArgs(member));

        public void Handle(Cluster sender, ClusterMemberLifecycleEventArgs args)
            => _handler(sender, args);

        /// <summary>
        /// Creates event arguments.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns>Event arguments.</returns>
        private static ClusterMemberLifecycleEventArgs CreateEventArgs(MemberInfo member)
            => new ClusterMemberLifecycleEventArgs(member);
    }

    public static partial class Extensions
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
