using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Serialization.Hook
{
	
	public sealed class AtomicLongDataSerializerHook : DataSerializerHook
	{
		public static readonly int FId = FactoryIdHelper.GetFactoryId(FactoryIdHelper.AtomicLongDsFactory, -17);

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
