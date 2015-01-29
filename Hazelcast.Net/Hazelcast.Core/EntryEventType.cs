namespace Hazelcast.Core
{
    /// <summary>Type of entry event.</summary>
    /// <remarks>Type of entry event.</remarks>
    public enum EntryEventType
    {
        Added = 1,
        Removed = 2,
        Updated = 3,
        Evicted = 4,
        EvictAll = 5,
        ClearAll = 6
    }
}