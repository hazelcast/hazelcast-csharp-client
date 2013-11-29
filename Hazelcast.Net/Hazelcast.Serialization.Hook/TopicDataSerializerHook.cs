using Hazelcast.IO.Serialization;

namespace Hazelcast.Serialization.Hook
{
    public sealed class TopicDataSerializerHook : DataSerializerHook
    {
        public static readonly int FId = FactoryIdHelper.GetFactoryId(FactoryIdHelper.TopicDsFactory, -18);

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