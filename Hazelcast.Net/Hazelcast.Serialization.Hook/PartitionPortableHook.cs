using System.Collections.Generic;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Serialization.Hook
{
    public sealed class PartitionPortableHook : IPortableHook
    {
        public static readonly int FId = FactoryIdHelper.GetFactoryId(FactoryIdHelper.PartitionPortableFactory, -2);

        public int GetFactoryId()
        {
            return FId;
        }

        public IPortableFactory CreateFactory()
        {
            return null;
        }

        public ICollection<IClassDefinition> GetBuiltinDefinitions()
        {
            return null;
        }
    }
}