using Hazelcast.Core;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{

    internal interface IPortableContext
    {
        int GetVersion();

        int GetClassVersion(int factoryId, int classId);

        void SetClassVersion(int factoryId, int classId, int version);

        IClassDefinition LookupClassDefinition(int factoryId, int classId, int version);

        IClassDefinition LookupClassDefinition(IData data);

        bool HasClassDefinition(IData data);

        IClassDefinition[] GetClassDefinitions(IData data);

        /// <exception cref="System.IO.IOException"></exception>
        IClassDefinition CreateClassDefinition(int factoryId, byte[] binary);

        IClassDefinition RegisterClassDefinition(IClassDefinition cd);

        /// <exception cref="System.IO.IOException"></exception>
        IClassDefinition LookupOrRegisterClassDefinition(IPortable portable);

        IFieldDefinition GetFieldDefinition(IClassDefinition cd, string name);

        IManagedContext GetManagedContext();

        ByteOrder GetByteOrder();
    }
}