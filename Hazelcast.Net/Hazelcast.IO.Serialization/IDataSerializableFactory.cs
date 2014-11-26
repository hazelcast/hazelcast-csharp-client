namespace Hazelcast.IO.Serialization
{
	/// <summary>DataSerializableFactory is used to create IdentifiedDataSerializable instances during de-serialization.</summary>
	/// <seealso cref="IIdentifiedDataSerializable">IIdentifiedDataSerializable</seealso>
	public interface IDataSerializableFactory
	{
		/// <summary>Creates an IdentifiedDataSerializable instance using given type id</summary>
		/// <param name="typeId">IdentifiedDataSerializable type id</param>
		/// <returns>IdentifiedDataSerializable instance or null if type id is not known by this factory</returns>
		IIdentifiedDataSerializable Create(int typeId);
	}
}
