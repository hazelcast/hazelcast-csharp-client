namespace Hazelcast.Core
{
    /// <summary>IEntryView represents a readonly view of a map entry.</summary>
    /// <?></?>
    public interface IEntryView<K, V>
    {
        /// <summary>Returns the key of the entry.</summary>
        /// <remarks>Returns the key of the entry.</remarks>
        /// <returns>key</returns>
        K GetKey();

        /// <summary>Returns the value of the entry.</summary>
        /// <remarks>Returns the value of the entry.</remarks>
        /// <returns>value</returns>
        V GetValue();

        /// <summary>Returns the cost (in bytes) of the entry.</summary>
        /// <remarks>
        ///     Returns the cost (in bytes) of the entry.
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     <p>
        ///         This method returns -1 if statistics is not enabled.
        ///     </p>
        /// </remarks>
        /// <returns>cost in bytes</returns>
        long GetCost();

        /// <summary>Returns the creation time of the entry.</summary>
        /// <remarks>
        ///     Returns the creation time of the entry.
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     <p>
        ///         This method returns -1 if statistics is not enabled.
        ///     </p>
        /// </remarks>
        /// <returns>creation time</returns>
        long GetCreationTime();

        /// <summary>Returns the expiration time of the entry.</summary>
        /// <remarks>Returns the expiration time of the entry.</remarks>
        /// <returns>expiration time</returns>
        long GetExpirationTime();

        /// <summary>Returns number of hits of the entry.</summary>
        /// <remarks>
        ///     Returns number of hits of the entry.
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     <p>
        ///         This method returns -1 if statistics is not enabled.
        ///     </p>
        /// </remarks>
        /// <returns>hits</returns>
        long GetHits();

        /// <summary>Returns the last access time to the entry.</summary>
        /// <remarks>
        ///     Returns the last access time to the entry.
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     <p>
        ///         This method returns -1 if statistics is not enabled.
        ///     </p>
        /// </remarks>
        /// <returns>last access time</returns>
        long GetLastAccessTime();

        /// <summary>Returns the last time value is flushed to mapstore.</summary>
        /// <remarks>
        ///     Returns the last time value is flushed to mapstore.
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     <p>
        ///         This method returns -1 if statistics is not enabled.
        ///     </p>
        /// </remarks>
        /// <returns>last store time</returns>
        long GetLastStoredTime();

        /// <summary>Returns the last time value is updated.</summary>
        /// <remarks>
        ///     Returns the last time value is updated.
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     <p>
        ///         This method returns -1 if statistics is not enabled.
        ///     </p>
        /// </remarks>
        /// <returns>last update time</returns>
        long GetLastUpdateTime();

        /// <summary>Returns the version of the entry</summary>
        /// <returns>version</returns>
        long GetVersion();
    }
}