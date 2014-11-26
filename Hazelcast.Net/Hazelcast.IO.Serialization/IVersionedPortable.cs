namespace Hazelcast.IO.Serialization
{
	/// <summary>
	/// VersionedPortable is an extension to
	/// <see cref="IPortable">IPortable</see>
	/// to support per class version instead of a global serialization version.
	/// </summary>
	/// <seealso cref="IPortable">IPortable</seealso>
	/// <seealso cref="IPortableFactory">IPortableFactory</seealso>
	public interface IVersionedPortable : IPortable
	{
		/// <summary>Returns version for this Portable class</summary>
		/// <returns>class version</returns>
		int GetClassVersion();
	}
}
