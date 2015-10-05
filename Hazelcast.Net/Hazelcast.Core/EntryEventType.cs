using System;

namespace Hazelcast.Core
{
    /// <summary>Type of entry event.</summary>
    /// <remarks>Type of entry event.</remarks>
    [Flags]
    public enum EntryEventType
    {
        Added = 1,
        Removed = 1 << 1,
        Updated = 1 << 2,
        Evicted = 1 << 3,
        EvictAll = 1 << 4,
        ClearAll = 1 << 5
    }
}