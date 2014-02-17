using Hazelcast.IO.Serialization;

namespace Hazelcast.Serialization.Hook
{
    internal sealed class AtomicLongDataSerializerHook : DataSerializerHook
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