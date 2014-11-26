
namespace Hazelcast.IO.Serialization
{
	/// <summary>
	/// For sample usage custom serialization and other way of custom serialization
	/// see
	/// <see cref="IStreamSerializer{T}">IStreamSerializer&lt;T&gt;</see>
	/// .
	/// Note that read and write methods should be compatible
	/// </summary>
	public interface IByteArraySerializer<T> : ISerializer
	{
		/// <summary>Converts given object to byte array</summary>
        /// <param name="obj">that will be serialized</param>
		/// <returns>byte array that object is serialized into</returns>
		/// <exception cref="System.IO.IOException">System.IO.IOException</exception>
		byte[] Write(T obj);

		/// <summary>Converts given byte array to object</summary>
		/// <param name="buffer">that object will be read from</param>
		/// <returns>deserialized object</returns>
		/// <exception cref="System.IO.IOException">System.IO.IOException</exception>
		T Read(byte[] buffer);
	}
}
