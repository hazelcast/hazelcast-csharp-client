namespace Hazelcast.Core
{
    /// <summary>
    ///     IPartitionAware allows implementing keys to be located on the same member
    ///     or implementing tasks to be executed on
    ///     <see cref="IIPartitionAware{T}.GetPartitionKey()">IPartitionAware&lt;T&gt;.GetPartitionKey()</see>
    ///     's owner member.
    ///     This makes related data to be stored in the same location. (See data-affinity.)
    /// </summary>
    public interface IPartitionAware<T>
    {
        /// <summary>The key object that will be used by Hazelcast to specify the partition.</summary>
        /// <remarks>
        ///     The key object that will be used by Hazelcast to specify the partition.
        ///     You should give the same key for objects that you want them to locate in the same partition.
        /// </remarks>
        /// <returns>partition key</returns>
        T GetPartitionKey();
    }
}