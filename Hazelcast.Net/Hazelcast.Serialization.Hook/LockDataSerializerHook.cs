using Hazelcast.IO.Serialization;

namespace Hazelcast.Serialization.Hook
{
    internal sealed class LockDataSerializerHook : DataSerializerHook
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