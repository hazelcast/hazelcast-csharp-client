
namespace Hazelcast.IO.Serialization
{
	/// <summary>PortableFactory is used to create Portable instances during de-serialization.
	/// 	</summary>
	/// <remarks>PortableFactory is used to create Portable instances during de-serialization.
	/// 	</remarks>
	/// <seealso cref="IPortable">IPortable</seealso>
	/// <seealso cref="IVersionedPortable">IVersionedPortable</seealso>
	public interface IPortableFactory
	{
		/// <summary>Creates a Portable instance using given class id</summary>
		/// <param name="classId">portable class id</param>
		/// <returns>portable instance or null if class id is not known by this factory</returns>
		IPortable Create(int classId);
	}
}
