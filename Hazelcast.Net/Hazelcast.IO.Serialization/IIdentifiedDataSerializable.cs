
using System;

namespace Hazelcast.IO.Serialization
{
	/// <summary>
	/// IdentifiedDataSerializable is an extension to
	/// <see cref="IDataSerializable">IDataSerializable</see>
	/// to avoid reflection during de-serialization.
	/// Each IdentifiedDataSerializable is
	/// created by a registered
	/// <see cref="IDataSerializableFactory">IDataSerializableFactory</see>
	/// .
	/// </summary>
	/// <seealso cref="IDataSerializable">IDataSerializable</seealso>
	/// <seealso cref="IPortable">IPortable</seealso>
	/// <seealso cref="IDataSerializableFactory">IDataSerializableFactory</seealso>
    public interface IIdentifiedDataSerializable : IDataSerializable
	{
		/// <summary>Returns DataSerializableFactory factory id for this class.</summary>
		/// <remarks>Returns DataSerializableFactory factory id for this class.</remarks>
		/// <returns>factory id</returns>
		int GetFactoryId();

		/// <summary>Returns type identifier for this class.</summary>
		/// <remarks>Returns type identifier for this class. Id should be unique per DataSerializableFactory.
		/// 	</remarks>
		/// <returns>type id</returns>
		int GetId();
	}
}