namespace Hazelcast.Core
{
    /// <summary>
    ///     ILifecycleService allows you to shutdown, terminate and listen to
    ///     <see cref="LifecycleEvent">LifecycleEvent</see>
    ///     's
    ///     on IHazelcastInstance.
    /// </summary>
    public interface ILifecycleService
    {
        /// <summary>whether the instance is running</summary>
        /// <returns>true if instance is active and running</returns>
        bool IsRunning();

        /// <summary>gracefully shutdowns IHazelcastInstance.</summary>
        /// <remarks>
        ///     gracefully shutdowns IHazelcastInstance. Different from
        ///     <see cref="Terminate()">Terminate()</see>
        ///     , waits partition operations to be completed.
        /// </remarks>
        void Shutdown();

        /// <summary>terminate IHazelcastInstance ungracefully.</summary>
        /// <remarks>terminate IHazelcastInstance ungracefully. Does not wait partition operations, forces immediate shutdown.</remarks>
        void Terminate();

        /// <summary>Add listener object to listen lifecycle events.</summary>
        /// <remarks>Add listener object to listen lifecycle events.</remarks>
        /// <param name="lifecycleListener">Listener object</param>
        /// <returns>listener id</returns>
        string AddLifecycleListener(ILifecycleListener lifecycleListener);

        /// <summary>Remove lifecycle listener</summary>
        /// <param name="registrationId">
        ///     The listener id returned by
        ///     <see cref="AddLifecycleListener(ILifecycleListener)">AddLifecycleListener(ILifecycleListener)</see>
        /// </param>
        /// <returns>true if removed successfully</returns>
        bool RemoveLifecycleListener(string registrationId);
    }
}