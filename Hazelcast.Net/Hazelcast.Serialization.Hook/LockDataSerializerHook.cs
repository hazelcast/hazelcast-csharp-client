using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Serialization.Hook
{
	
	public sealed class LockDataSerializerHook : DataSerializerHook
	{
		public static readonly int FId = FactoryIdHelper.GetFactoryId(FactoryIdHelper.LockDsFactory, -15);

		public int GetFactoryId()
		{
			return FId;
		}

		public IDataSerializableFactory CreateFactory()
		{
			return null;
		}
	}
}
