namespace Hazelcast.Core
{
    /// <summary>
    ///     IAtomicLong is a redundant and highly available distributed Atomic Long to the
    /// </summary>
    public interface IAtomicLong : IDistributedObject
    {
        /// <summary>Returns the name of this IAtomicLong instance.</summary>
        /// <remarks>Returns the name of this IAtomicLong instance.</remarks>
        /// <returns>name of this instance</returns>
        //string GetName();

        /// <summary>Atomically adds the given value to the current value.</summary>
        /// <remarks>Atomically adds the given value to the current value.</remarks>
        /// <param name="delta">the value to add</param>
        /// <returns>the updated value</returns>
        long AddAndGet(long delta);

        /// <summary>
        ///     Atomically sets the value to the given updated value
        ///     only if the current value
        ///     <code>==</code>
        ///     the expected value.
        /// </summary>
        /// <param name="expect">the expected value</param>
        /// <param name="update">the new value</param>
        /// <returns>
        ///     true if successful; or false if the actual value
        ///     was not equal to the expected value.
        /// </returns>
        bool CompareAndSet(long expect, long update);

        /// <summary>Atomically decrements the current value by one.</summary>
        /// <remarks>Atomically decrements the current value by one.</remarks>
        /// <returns>the updated value</returns>
        long DecrementAndGet();

        /// <summary>Gets the current value.</summary>
        /// <remarks>Gets the current value.</remarks>
        /// <returns>the current value</returns>
        long Get();

        /// <summary>Atomically adds the given value to the current value.</summary>
        /// <remarks>Atomically adds the given value to the current value.</remarks>
        /// <param name="delta">the value to add</param>
        /// <returns>the old value before the add</returns>
        long GetAndAdd(long delta);

        /// <summary>Atomically sets the given value and returns the old value.</summary>
        /// <remarks>Atomically sets the given value and returns the old value.</remarks>
        /// <param name="newValue">the new value</param>
        /// <returns>the old value</returns>
        long GetAndSet(long newValue);

        /// <summary>Atomically increments the current value by one.</summary>
        /// <remarks>Atomically increments the current value by one.</remarks>
        /// <returns>the updated value</returns>
        long IncrementAndGet();

        /// <summary>Atomically increments the current value by one.</summary>
        /// <remarks>Atomically increments the current value by one.</remarks>
        /// <returns>the old value</returns>
        long GetAndIncrement();

        /// <summary>Atomically sets the given value.</summary>
        /// <remarks>Atomically sets the given value.</remarks>
        /// <param name="newValue">the new value</param>
        void Set(long newValue);
    }
}