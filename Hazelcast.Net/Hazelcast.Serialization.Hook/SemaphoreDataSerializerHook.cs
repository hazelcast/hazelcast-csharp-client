using Hazelcast.IO.Serialization;

namespace Hazelcast.Serialization.Hook
{
    internal class SemaphoreDataSerializerHook : DataSerializerHook
    {
        internal static readonly int FId = FactoryIdHelper.GetFactoryId(FactoryIdHelper.SemaphoreDsFactory, -16);

        public virtual int GetFactoryId()
        {
            return FId;
        }

        public virtual IDataSerializableFactory CreateFactory()
        {
            return null;
        }
    }
}