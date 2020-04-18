using System;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Specifies the entries event types.
    /// </summary>
    [Flags]
    public enum EntryEventType
    {
        /// <summary>
        /// The entry was added.
        /// </summary>
        Added = 1,

        /// <summary>
        /// The entry was removed.
        /// </summary>
        Removed = 1 << 1,

        /// <summary>
        /// The entry was updated.
        /// </summary>
        Updated = 1 << 2,

        /// <summary>
        /// The entry was evicted.
        /// </summary>
        Evicted = 1 << 3,

        /// <summary>
        /// The entry has expired.
        /// </summary>
        Expired = 1 << 4,

        /// <summary>
        /// All entries were evicted.
        /// </summary>
        EvictAll = 1 << 5,

        /// <summary>
        /// All entries were cleared.
        /// </summary>
        ClearAll = 1 << 6,

        /// <summary>
        /// Merged. todo: ?
        /// </summary>
        Merged = 1 << 7,

        /// <summary>
        /// Invalidation. todo: ?
        /// </summary>
        Invalidation = 1 << 8,

        /// <summary>
        /// Loaded. todo: ?
        /// </summary>
        Loaded = 1 << 9
    }
}
