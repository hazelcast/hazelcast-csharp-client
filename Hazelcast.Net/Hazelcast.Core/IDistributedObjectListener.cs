namespace Hazelcast.Core
{
    /// <summary>
    ///     IDistributedObjectListener allows to get notified when a
    ///     <see cref="IDistributedObject">IDistributedObject</see>
    ///     is created or destroyed cluster-wide.
    /// </summary>
    /// <seealso cref="IDistributedObject">IDistributedObject</seealso>
    /// <seealso cref="IHazelcastInstance.AddDistributedObjectListener(IDistributedObjectListener)">IHazelcastInstance.AddDistributedObjectListener(IDistributedObjectListener)</seealso>
    public interface IDistributedObjectListener : IEventListener
    {
        /// <summary>Invoked when a IDistributedObject is created.</summary>
        /// <remarks>Invoked when a IDistributedObject is created.</remarks>
        /// <param name="event">event</param>
        void DistributedObjectCreated(DistributedObjectEvent @event);

        /// <summary>Invoked when a IDistributedObject is destroyed.</summary>
        /// <remarks>Invoked when a IDistributedObject is destroyed.</remarks>
        /// <param name="event">event</param>
        void DistributedObjectDestroyed(DistributedObjectEvent @event);
    }
}