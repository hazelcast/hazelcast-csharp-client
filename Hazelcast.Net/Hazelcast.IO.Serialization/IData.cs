using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
    /// <summary>Data is basic unit of serialization.</summary>
    /// <remarks>
    /// Data is basic unit of serialization. It stores binary form of an object serialized
    /// </remarks>
    public interface IData
    {
        /// <summary>Returns byte array representation of internal binary format.</summary>
        /// <returns>binary data</returns>
        byte[] ToByteArray();

        /// <summary>Returns serialization type of binary form.</summary>
        /// <remarks>
        /// Returns serialization type of binary form. It's defined by
        /// <see cref="ISerializer.GetTypeId()"/>
        /// </remarks>
        /// <returns>serializer type id</returns>
        int GetTypeId();

        /// <summary>Returns the total size of Data in bytes</summary>
        /// <returns>total size</returns>
        int TotalSize();

        /// <summary>Returns size of internal binary data in bytes</summary>
        /// <returns>internal data size</returns>
        int DataSize();

        /// <summary>Returns approximate heap cost of this Data object in bytes.</summary>
        /// <returns>approximate heap cost</returns>
        int GetHeapCost();

        /// <summary>Returns partition hash calculated for serialized object.</summary>
        /// <remarks>
        /// Returns partition hash calculated for serialized object.
        /// Partition hash is used to determine partition of a Data and is calculated using
        /// <see cref="Hazelcast.Core.IPartitioningStrategy"/>
        /// during serialization.
        /// <p/>
        /// If partition hash is not set then standard <tt>hashCode()</tt> is used.
        /// </remarks>
        /// <returns>partition hash</returns>
        /// <seealso cref="Hazelcast.Core.IPartitionAware{T}"/>
        /// <seealso cref="Hazelcast.Core.IPartitioningStrategy"/>
        /// <seealso cref="SerializationService.ToData(object, Hazelcast.Core.IPartitioningStrategy)"/>
        int GetPartitionHash();

        /// <summary>Returns true if Data has partition hash, false otherwise.</summary>
        /// <returns>true if Data has partition hash, false otherwise.</returns>
        bool HasPartitionHash();

        /// <summary>Returns 64-bit hash code for this Data object.</summary>
        /// <returns>64-bit hash code</returns>
        long Hash64();

        /// <summary>
        /// Returns true if this Data is created from a
        /// <see cref="IPortable"/>
        /// object,
        /// false otherwise.
        /// </summary>
        /// <returns>true if source object is <tt>Portable</tt>, false otherwise.</returns>
        bool IsPortable();
    }
}
