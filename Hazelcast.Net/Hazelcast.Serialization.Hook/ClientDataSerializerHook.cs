using Hazelcast.Client;
using Hazelcast.IO.Serialization;


namespace Hazelcast.Serialization.Hook
{
	
	public sealed class ClientDataSerializerHook : DataSerializerHook
	{
		public static readonly int Id = FactoryIdHelper.GetFactoryId(FactoryIdHelper.ClientDsFactory, -3);

		public int GetFactoryId()
		{
			return Id;
		}

		public IDataSerializableFactory CreateFactory()
		{
			return null;
		}
	}
}
