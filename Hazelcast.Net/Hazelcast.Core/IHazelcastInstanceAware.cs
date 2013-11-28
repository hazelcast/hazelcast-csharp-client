namespace Hazelcast.Core
{
    /// <summary>Used to get IHazelcastInstance reference when submitting a Runnable/Callable using Hazelcast ExecutorService.</summary>
    /// <remarks>
    ///     Used to get IHazelcastInstance reference when submitting a Runnable/Callable using Hazelcast ExecutorService.
    ///     Before executing the Runnable/Callable Hazelcast will invoke
    ///     <see cref="SetHazelcastInstance(IHazelcastInstance)">SetHazelcastInstance(IHazelcastInstance)</see>
    ///     method with
    ///     the reference to IHazelcastInstance that is executing. This way the implementer will have a chance to get the
    ///     reference to IHazelcastInstance.
    /// </remarks>
    public interface IHazelcastInstanceAware
    {
        void SetHazelcastInstance(IHazelcastInstance hazelcastInstance);
    }
}