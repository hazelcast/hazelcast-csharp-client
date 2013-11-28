using System;
using Hazelcast.Net.Ext;

namespace Hazelcast.Core
{
    /// <summary>
    ///     Distributed implementation of
    ///     <see cref="Hazelcast.Net.Ext.Condition">Hazelcast.Net.Ext.Condition</see>
    ///     .
    ///     An ICondition instance is created using
    ///     <see cref="ILock.NewCondition(string)">ILock.NewCondition(string)</see>
    ///     <pre>
    ///         IHazelcastInstance instance = ...;
    ///         ILock lock = instance.getLock("lock");
    ///         ICondition cond = lock.newCondition("condition");
    ///         ...
    ///     </pre>
    /// </summary>
    /// <seealso cref="Hazelcast.Net.Ext.Condition">Hazelcast.Net.Ext.Condition</seealso>
    /// <seealso cref="ILock">ILock</seealso>
    public interface ICondition
    {
        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        /// <exception cref="System.Exception"></exception>
        void Await();

        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        void AwaitUninterruptibly();

        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        /// <exception cref="System.Exception"></exception>
        long AwaitNanos(long nanosTimeout);

        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        /// <exception cref="System.Exception"></exception>
        bool Await(long time, TimeUnit unit);

        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        /// <exception cref="System.Exception"></exception>
        bool AwaitUntil(DateTime deadline);

        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        void Signal();

        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        void SignalAll();
    }
}

//public interface java.ICondition {

//    /// <exception cref="System.Exception"></exception>
//    void Await();

//    /// <exception cref="System.Exception"></exception>
//    void AwaitUninterruptibly();

//    /// <exception cref="System.Exception"></exception>
//    long AwaitNanos(long nanosTimeout);

//    /// <exception cref="System.Exception"></exception>
//    bool Await(long time, TimeUnit unit);

//    /// <exception cref="System.Exception"></exception>
//    bool AwaitUntil(DateTime deadline);

//    void Signal();


//    void SignalAll();
//}