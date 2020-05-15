using Hazelcast.Partitioning.Strategies;

namespace Hazelcast.Partitioning
{
    /// <summary>
    /// Defines objects that provide their own partition hash.
    /// </summary>
    public interface IHavePartitionHash
    {
        /// <summary>Gets the partition hash of the serialized object.</summary>
        /// <remarks>
        /// <para>The partition hash is used to determine the partition of the data and is
        /// calculated using an <see cref="IPartitionAware"/> during serialization.</para>
        /// <para>If the partition hash is not set, then the standard hash code is used.</para>
        /// </remarks>
        /// <returns>The partition hash.</returns>
        int PartitionHash { get; }
    }
}
