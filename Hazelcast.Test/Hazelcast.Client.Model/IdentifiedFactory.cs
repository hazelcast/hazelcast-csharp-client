using Hazelcast.Client.Model;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Test
{
    internal class IdentifiedFactory : IDataSerializableFactory
    {
        internal const int FactoryId = 66;

        public IIdentifiedDataSerializable Create(int typeId)
        {
            if (typeId == IdentifiedEntryProcessor.ClassId)
            {
                return new IdentifiedEntryProcessor();
            }
            if (typeId == CustomComparator.ClassId)
            {
                return new CustomComparator();
            }
            return null;
        }
    }
}