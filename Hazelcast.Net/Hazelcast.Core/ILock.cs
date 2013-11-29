using Hazelcast.Net.Ext;

namespace Hazelcast.Core
{
    /// <summary>
    ///     Distributed implementation of Lock
    /// </summary>
    public interface ILock : IDistributedObject
    {
        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        void Lock();

        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        bool TryLock();

        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        /// <exception cref="System.Exception"></exception>
        bool TryLock(long time, TimeUnit? unit);

        /// <summary>Releases the lock.</summary>
        /// <remarks>Releases the lock.</remarks>
        void Unlock();

        /// <summary>Acquires the lock for the specified lease time.</summary>
        /// <remarks>
        ///     Acquires the lock for the specified lease time.
        ///     <p>
        ///         After lease time, lock will be released..
        ///         <p />
        ///         <p>
        ///             If the lock is not available then
        ///             the current thread becomes disabled for thread scheduling
        ///             purposes and lies dormant until the lock has been acquired.
        ///             <p />
        /// </remarks>
        /// <param name="leaseTime">time to wait before releasing the lock.</param>
        /// <param name="timeUnit">unit of time to specify lease time.</param>
        void Lock(long leaseTime, TimeUnit? timeUnit);

        /// <summary>Releases the lock regardless of the lock owner.</summary>
        /// <remarks>
        ///     Releases the lock regardless of the lock owner.
        ///     It always successfully unlocks, never blocks  and returns immediately.
        /// </remarks>
        void ForceUnlock();

        /// <summary>
        ///     This method is not implemented! Use
        ///     <see cref="NewCondition(string)">NewCondition(string)</see>
        ///     instead.
        /// </summary>
        /// <exception cref="System.NotSupportedException">System.NotSupportedException</exception>
        ICondition NewCondition();

        /// <summary>
        ///     Returns a new
        ///     <see cref="ICondition">ICondition</see>
        ///     instance that is bound to this
        ///     <code>ILock</code>
        ///     instance with given name.
        ///     <p>
        ///         Before waiting on the condition the lock must be held by the
        ///         current thread.
        ///         A call to
        ///         <see cref="ICondition.Await()">ICondition.Await()</see>
        ///         will atomically release the lock
        ///         before waiting and re-acquire the lock before the wait returns.
        /// </summary>
        /// <param name="name">identifier of the new condition instance</param>
        /// <returns>
        ///     A new
        ///     <see cref="ICondition">ICondition</see>
        ///     instance for this
        ///     <code>ILock</code>
        ///     instance
        /// </returns>
        ICondition NewCondition(string name);

        /// <summary>Returns whether this lock is locked or not.</summary>
        /// <remarks>Returns whether this lock is locked or not.</remarks>
        /// <returns>
        ///     <code>true</code>
        ///     if this lock is locked,
        ///     <code>false</code>
        ///     otherwise.
        /// </returns>
        bool IsLocked();

        /// <summary>Returns whether this lock is locked by current thread or not.</summary>
        /// <remarks>Returns whether this lock is locked by current thread or not.</remarks>
        /// <returns>
        ///     <code>true</code>
        ///     if this lock is locked by current thread,
        ///     <code>false</code>
        ///     otherwise.
        /// </returns>
        bool IsLockedByCurrentThread();

        /// <summary>Returns re-entrant lock hold count, regardless of lock ownership.</summary>
        /// <remarks>Returns re-entrant lock hold count, regardless of lock ownership.</remarks>
        /// <returns>lock hold count.</returns>
        int GetLockCount();

        /// <summary>Returns remaining lease time in milliseconds.</summary>
        /// <remarks>
        ///     Returns remaining lease time in milliseconds.
        ///     If the lock is not locked then -1 will be returned.
        /// </remarks>
        /// <returns>remaining lease time in milliseconds.</returns>
        long GetRemainingLeaseTime();
    }
}