namespace Hazelcast.Core
{
    /// <summary>
    ///     Map Entry listener to get notified when a map entry
    ///     is added, removed, updated or evicted.
    /// </summary>
    /// <remarks>
    ///     Map Entry listener to get notified when a map entry
    ///     is added, removed, updated or evicted.
    /// </remarks>
    public interface IEntryListener<K, V> : IEventListener
    {
        /// <summary>Invoked when an entry is added.</summary>
        /// <remarks>Invoked when an entry is added.</remarks>
        /// <param name="event">entry event</param>
        void EntryAdded(EntryEvent<K, V> @event);

        /// <summary>Invoked when an entry is removed.</summary>
        /// <remarks>Invoked when an entry is removed.</remarks>
        /// <param name="event">entry event</param>
        void EntryRemoved(EntryEvent<K, V> @event);

        /// <summary>Invoked when an entry is updated.</summary>
        /// <remarks>Invoked when an entry is updated.</remarks>
        /// <param name="event">entry event</param>
        void EntryUpdated(EntryEvent<K, V> @event);

        /// <summary>Invoked when an entry is evicted.</summary>
        /// <remarks>Invoked when an entry is evicted.</remarks>
        /// <param name="event">entry event</param>
        void EntryEvicted(EntryEvent<K, V> @event);
    }
}