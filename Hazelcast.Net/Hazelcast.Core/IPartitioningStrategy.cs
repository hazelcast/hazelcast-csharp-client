using System;

namespace Hazelcast.Core
{
    /// <summary>
    ///     IPartitioningStrategy allows keys to be located on the same member
    ///     This makes related data to be stored in the same location.
    /// </summary>
    /// <remarks>
    ///     IPartitioningStrategy allows keys to be located on the same member
    ///     This makes related data to be stored in the same location. (See data-affinity.)
    /// </remarks>
    /// <?></?>
    public interface IPartitioningStrategy
    {
        /// <summary>Returns the key object that will be used by Hazelcast to specify the partition.</summary>
        /// <remarks>Returns the key object that will be used by Hazelcast to specify the partition.</remarks>
        /// <param name="key">actual key object</param>
        /// <returns>partition key object or null to fallback to default partition calculation</returns>
        object GetPartitionKey(Object key);
    }
}