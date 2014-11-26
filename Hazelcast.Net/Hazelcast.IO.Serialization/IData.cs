using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
	/// <summary>Data is basic unit of serialization.</summary>
	/// <remarks>
	/// Data is basic unit of serialization. It stores binary form of an object serialized
	/// by
	/// <see cref="ISerializationService.ToData{B}(object)">ISerializationService.ToData&lt;B&gt;(object)
	/// 	</see>
	/// .
	/// </remarks>
	public interface IData
	{
		/// <summary>Returns byte array representation of internal binary format.</summary>
		/// <returns>binary data</returns>
		byte[] GetData();

		/// <summary>Returns serialization type of binary form.</summary>
		/// <remarks>
		/// Returns serialization type of binary form. It's defined by
		/// <see cref="ISerializer.GetTypeId()">ISerializer.GetTypeId()</see>
		/// </remarks>
		/// <returns>serializer type id</returns>
		int GetType();

		/// <summary>Returns size of binary data</summary>
		/// <returns>data size</returns>
		int DataSize();

		/// <summary>Returns approximate heap cost of this Data object in bytes.</summary>
		/// <remarks>Returns approximate heap cost of this Data object in bytes.</remarks>
		/// <returns>approximate heap cost</returns>
		int GetHeapCost();

		/// <summary>Returns partition hash calculated for serialized object.</summary>
		/// <remarks>
		/// Returns partition hash calculated for serialized object.
		/// Partition hash is used to determine partition of a Data and is calculated using
		/// <see cref="Hazelcast.Core.IPartitioningStrategy">Hazelcast.Core.IPartitioningStrategy&lt;K&gt;
		/// 	</see>
		/// during serialization.
		/// <p/>
		/// If partition hash is not set then standard <tt>hashCode()</tt> is used.
		/// </remarks>
		/// <returns>partition hash</returns>
		/// <seealso cref="Hazelcast.Core.IPartitionAware{T}">Hazelcast.Core.IPartitionAware&lt;T&gt;</seealso>
		/// <seealso cref="Hazelcast.Core.IPartitioningStrategy">Hazelcast.Core.IPartitioningStrategy</seealso>
		/// <seealso cref="ISerializationService.ToData{B}(object, Hazelcast.Core.IPartitioningStrategy)">ISerializationService.ToData&lt;B&gt;(object, Hazelcast.Core.IPartitioningStrategy)</seealso>
		int GetPartitionHash();

		/// <summary>Returns true if Data has partition hash, false otherwise.</summary>
		/// <returns>true if Data has partition hash, false otherwise.</returns>
		bool HasPartitionHash();

		/// <summary>Returns 64-bit hash code for this Data object.</summary>
		/// <returns>64-bit hash code</returns>
		long Hash64();

		/// <summary>
		/// Returns true if this Data is created from a
		/// <see cref="IPortable">IPortable</see>
		/// object,
		/// false otherwise.
		/// </summary>
		/// <returns>true if source object is <tt>Portable</tt>, false otherwise.</returns>
		bool IsPortable();

		/// <summary>Returns byte array representation of header.</summary>
		/// <remarks>
		/// Returns byte array representation of header. Header is used to store <tt>Portable</tt> metadata
		/// during serialization and consists of <tt>factoryId</tt>, <tt>classId</tt> and <tt>version</tt> for each
		/// <tt>Portable</tt> field that source object contains.
		/// </remarks>
		/// <returns>header</returns>
		byte[] GetHeader();

		/// <summary>Returns size of header.</summary>
		/// <returns>size of header</returns>
		int HeaderSize();

		/// <summary>Reads an integer header from given offset using given <tt>ByteOrder</tt>.</summary>
		/// <param name="offset">offset of integer header</param>
		/// <param name="order">byte order</param>
		/// <returns>integer header</returns>
		int ReadIntHeader(int offset, ByteOrder order);
	}
}
