using System;
using System.Collections.Generic;
using System.Text;
using Hazelcast.Data;

namespace Hazelcast.Clustering
{
    public enum ClusterObjectLifecycleEventType
    {
        Nothing,
        Created,
        Destroyed
    }

    /// <summary>
    /// TODO: document
    /// </summary>
    public class ClusterObjectLifecycleEventArgs
    {
        /// <summary>
        /// TODO: document
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="name"></param>
        /// <param name="source"></param>
        public ClusterObjectLifecycleEventArgs(string serviceName, string name, Guid source)
        {
            ServiceName = serviceName;
            Name = name;
            Source = source;
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
        /// Gets the source of the event. FIXME: member?
        /// </summary>
        public Guid Source { get; }
    }

    /// <summary>
    /// Represents a handler for a cluster object lifecycle event.
    /// </summary>
    /// <typeparam name="T">The topic object type.</typeparam>
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
