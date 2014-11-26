namespace Hazelcast.IO.Serialization
{
	internal interface ISerializerAdapter
	{
		/// <exception cref="System.IO.IOException"></exception>
		void Write(IObjectDataOutput output, object obj);

		/// <exception cref="System.IO.IOException"></exception>
		object Read(IObjectDataInput @in);

		/// <exception cref="System.IO.IOException"></exception>
		IData ToData(object obj, int partitionHash);

		/// <exception cref="System.IO.IOException"></exception>
		object ToObject(IData data);

		int GetTypeId();

		void Destroy();

		ISerializer GetImpl();
	}
}