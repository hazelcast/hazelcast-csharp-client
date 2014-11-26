namespace Hazelcast.IO.Serialization
{
	/// <summary>DataSerializable is a serialization method as an alternative to standard serialization.
	/// 	</summary>
	/// <remarks>
	/// DataSerializable is a serialization method as an alternative to standard serialization.
	/// </remarks>
	/// <seealso cref="IIdentifiedDataSerializable">IIdentifiedDataSerializable</seealso>
	/// <seealso cref="IPortable">IPortable</seealso>
	/// <seealso cref="IVersionedPortable">IVersionedPortable</seealso>
	public interface IDataSerializable
	{
		/// <summary>Writes object fields to output stream</summary>
        /// <param name="output">output</param>
		/// <exception cref="System.IO.IOException">System.IO.IOException</exception>
		void WriteData(IObjectDataOutput output);

		/// <summary>Reads fields from the input stream</summary>
        /// <param name="input">input</param>
		/// <exception cref="System.IO.IOException">System.IO.IOException</exception>
		void ReadData(IObjectDataInput input);

        /// <summary>
        /// Get Server Java Impl class full name
        /// </summary>
        /// <returns>full java class name</returns>
        string GetJavaClassName();
	}
}
