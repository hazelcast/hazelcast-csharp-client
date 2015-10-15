namespace Hazelcast.Core
{
    /// <summary>
    /// Using this policy one can control the behavior what should to be done when an item is about to be added to the ringbuffer,
    /// but there is 0 remaining capacity.
    /// </summary>
    /// <remarks>
    /// Using this policy one can control the behavior what should to be done when an item is about to be added to the ringbuffer,
    /// but there is 0 remaining capacity.
    /// Overflowing happens when a time-to-live is set and the oldest item in the ringbuffer (the head) is not old enough to expire.
    /// </remarks>
    /// <seealso cref="Ringbuffer{E}.AddAsync(object, OverflowPolicy)"/>
    /// <seealso cref="Ringbuffer{E}.AddAllAsync(System.Collections.ICollection{E}, OverflowPolicy)"/>
    public enum OverflowPolicy
    {
        /// <summary>Using this policy the oldest item is overwritten no matter it is not old enough to retire.</summary>
        /// <remarks>
        /// Using this policy the oldest item is overwritten no matter it is not old enough to retire. Using this policy you are
        /// sacrificing the time-to-live in favor of being able to write.
        /// Example: if there is a time-to-live of 30 seconds, the buffer is full and the oldest item in the ring has been placed a
        /// second ago, then there are 29 seconds remaining for that item. Using this policy you are going to overwrite no matter
        /// what.
        /// </remarks>
        Overwrite = 0,
        /// <summary>
        /// Using this policy the call will fail immediately and the oldest item will not be overwritten before it is old enough
        /// to retire.
        /// </summary>
        /// <remarks>
        /// Using this policy the call will fail immediately and the oldest item will not be overwritten before it is old enough
        /// to retire. So this policy sacrificing the ability to write in favor of time-to-live.
        /// The advantage of fail is that the caller can decide what to do since it doesn't trap the thread due to backoff.
        /// Example: if there is a time-to-live of 30 seconds, the buffer is full and the oldest item in the ring has been placed a
        /// second ago, then there are 29 seconds remaining for that item. Using this policy you are not going to overwrite that
        /// item for the next 29 seconds.
        /// </remarks>
        Fail = 1
    }
}
