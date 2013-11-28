using Hazelcast.Core;

namespace Hazelcast.IO.Serialization
{
    public interface ISerializationContext
    {
        int GetVersion();

        IClassDefinition Lookup(int factoryId, int classId);

        IClassDefinition Lookup(int factoryId, int classId, int version);

        /// <exception cref="System.IO.IOException"></exception>
        IClassDefinition CreateClassDefinition(int factoryId, byte[] binary);

        IClassDefinition RegisterClassDefinition(IClassDefinition cd);

        /// <exception cref="System.IO.IOException"></exception>
        IClassDefinition LookupOrRegisterClassDefinition(IPortable portable);

        IManagedContext GetManagedContext();
    }
}