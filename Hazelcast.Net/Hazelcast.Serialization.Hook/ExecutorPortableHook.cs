using System.Collections.Generic;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Serialization.Hook
{
    public sealed class ExecutorPortableHook : IPortableHook
    {
        public static readonly int FId = FactoryIdHelper.GetFactoryId(FactoryIdHelper.ExecutorPortableFactory, -13);

        public int GetFactoryId()
        {
            return FId;
        }

        public IPortableFactory CreateFactory()
        {
            return new ArrayPortableFactory();
        }

        public ICollection<IClassDefinition> GetBuiltinDefinitions()
        {
            return null;
        }
    }
}