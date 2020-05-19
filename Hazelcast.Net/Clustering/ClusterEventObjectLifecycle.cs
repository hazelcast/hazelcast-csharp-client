using System;
using Hazelcast.Data;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Defines the types of object lifecycle events.
    /// </summary>
    public enum ClusterObjectLifecycleEventType
    {
        /// <summary>
        /// Nothing (default)
        /// </summary>
        Nothing = 0,

        /// <summary>
        /// The object was created.
        /// </summary>
        Created,

        /// <summary>
        /// The object was destroyed.
        /// </summary>
        Destroyed
    }

    /// <summary>
    /// Represents event data for a cluster object lifecycle event.
    /// </summary>
    public class ClusterObjectLifecycleEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterObjectLifecycleEventArgs"/> class.
        /// </summary>
        /// <param name="serviceName">The service unique name.</param>
        /// <param name="name">The object unique name.</param>
        /// <param name="sourceMemberId">The unique identifier of the source member.</param>
        public ClusterObjectLifecycleEventArgs(string serviceName, string name, Guid sourceMemberId)
        {
            ServiceName = serviceName;
            Name = name;
            SourceMemberId = sourceMemberId;
        }

        /// <summary>
        /// Gets the name of the service handling the impacted object.
        /// </summary>
        public string ServiceName { get; }

        /// <summary>
        /// Gets the name of the impacted object.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the unique identifier of the source member.
        /// </summary>
        public Guid SourceMemberId { get; }
    }

    /// <summary>
    /// Represents a handler for a cluster object lifecycle event.
    /// </summary>
    internal class ClusterObjectLifecycleEventHandler : IClusterEventHandler
    {
        private readonly Action<Cluster, ClusterObjectLifecycleEventArgs> _handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterObjectLifecycleEventHandler"/> class.
        /// </summary>
        /// <param name="eventType">The type of the event.</param>
        /// <param name="handler">An action to execute</param>
        public ClusterObjectLifecycleEventHandler(ClusterObjectLifecycleEventType eventType, Action<Cluster, ClusterObjectLifecycleEventArgs> handler)
        {
            EventType = eventType;
            _handler = handler;
        }

        /// <summary>
        /// Gets the type of the event.
        /// </summary>
        public ClusterObjectLifecycleEventType EventType { get; }

        /// <summary>
        /// Handles the event.
        /// </summary>
        /// <param name="sender">The originating cluster.</param>
        /// <param name="member">The member.</param>
        /// <param name="serviceName">The unique name of the service managing the object.</param>
        /// <param name="name">The unique name of the object.</param>
        public void Handle(Cluster sender, MemberInfo member, string serviceName, string name)
            => _handler(sender, CreateEventArgs(member, serviceName, name));

        /// <summary>
        /// Handle the event.
        /// </summary>
        /// <param name="sender">The originating cluster.</param>
        /// <param name="args">The event arguments.</param>
        public void Handle(Cluster sender, ClusterObjectLifecycleEventArgs args)
            => _handler(sender, args);

        /// <summary>
        /// Creates event arguments.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="serviceName">The unique name of the service managing the object.</param>
        /// <param name="name">The unique name of the object.</param>
        /// <returns>Event arguments.</returns>
        private static ClusterObjectLifecycleEventArgs CreateEventArgs(MemberInfo member, string serviceName, string name)
            => new ClusterObjectLifecycleEventArgs(serviceName, name, member.Id);
    }


    /// <summary>
    /// Provides extension to the <see cref="ClusterEventHandlers"/> class.
    /// </summary>
    public static partial class Extensions
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
