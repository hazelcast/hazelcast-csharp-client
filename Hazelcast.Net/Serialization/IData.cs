using Hazelcast.Partitioning;

namespace Hazelcast.Serialization
{
    /// <summary>
    /// Represents the basic unit of serialization.
    /// </summary>
    public interface IData : IHavePartitionHash
    {
        /// <summary>Gets the size of the data contained in this instance.</summary>
        int DataSize { get; }

        /// <summary>Gets the total size of this instance in bytes.</summary>
        int TotalSize { get; }

        /// <summary>Gets the approximate heap cost of this instance in bytes.</summary>
        int HeapCost { get; }

        /// <summary>Returns serialization type of binary form.</summary>
        /// <remarks>
        /// Returns serialization type of binary form. It's defined by
        /// <see cref="ISerializer.GetTypeId()"/>
        /// </remarks>
        /// <returns>serializer type id</returns>
        int TypeId { get; }

        /// <summary>Determines whether this instance has a partition hash.</summary>
        bool HasPartitionHash { get; }

        /// <summary>Determines whether this instance was created from an <see cref="IPortable{T}"/> instance.</summary>
        bool IsPortable { get; }

        /// <summary>Gets the byte array representation of this instance.</summary>
        byte[] ToByteArray();
    }
}
