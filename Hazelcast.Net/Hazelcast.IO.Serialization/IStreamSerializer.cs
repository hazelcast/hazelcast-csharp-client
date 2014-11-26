namespace Hazelcast.IO.Serialization
{
	/// <summary>A base class for custom serialization.</summary>
	/// <remarks>
	/// A base class for custom serialization. User can register custom serializer.
	/// </remarks>
	public interface IStreamSerializer<T> : ISerializer
	{
		/// <summary>This method writes object to ObjectDataOutput</summary>
		/// <param name="output">ObjectDataOutput stream that object will be written to</param>
		/// <param name="obj">that will be written to out</param>
		/// <exception cref="System.IO.IOException">in case of failure to write</exception>
		void Write(IObjectDataOutput output, T obj);

		/// <summary>Reads object from objectDataInputStream</summary>
		/// <param name="input">ObjectDataInput stream that object will read from</param>
		/// <returns>read object</returns>
		/// <exception cref="System.IO.IOException">in case of failure to read</exception>
		T Read(IObjectDataInput input);
	}
}
