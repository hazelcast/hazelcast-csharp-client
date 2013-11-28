namespace Hazelcast.Core
{
    /// <summary>Listener object for lisntening lifecycle events of hazelcast instance</summary>
    /// <seealso cref="LifecycleEvent">LifecycleEvent</seealso>
    /// <seealso cref="IHazelcastInstance.GetLifecycleService()">IHazelcastInstance.GetLifecycleService()</seealso>
    public interface ILifecycleListener : IEventListener
    {
        /// <summary>Called when instance's state changes</summary>
        /// <param name="event">Lifecycle event</param>
        void StateChanged(LifecycleEvent lifecycleEvent);
    }
}