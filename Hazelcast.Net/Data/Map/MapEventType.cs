using System;

namespace Hazelcast.Data.Map
{
    /// <summary>
    /// Specifies the map and entries event types.
    /// </summary>
    [Flags]
    public enum MapEventType
    {
        /// <summary>
        /// Nothing (default value).
        /// </summary>
        Nothing = 0,

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
        AllEvicted = 1 << 5,

        /// <summary>
        /// All entries were cleared.
        /// </summary>
        AllCleared = 1 << 6,

        /// <summary>
        /// The entry was merged.
        /// </summary>
        Merged = 1 << 7,

        /// <summary>
        /// The entry was invalidated.
        /// </summary>
        Invalidated = 1 << 8,

        /// <summary>
        /// The entry was loaded.
        /// </summary>
        Loaded = 1 << 9
    }
}
