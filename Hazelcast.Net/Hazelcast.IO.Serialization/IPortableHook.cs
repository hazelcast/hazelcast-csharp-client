using System.Collections.Generic;

namespace Hazelcast.IO.Serialization
{
    public interface IPortableHook
    {
        int GetFactoryId();

        IPortableFactory CreateFactory();

        ICollection<IClassDefinition> GetBuiltinDefinitions();
    }
}