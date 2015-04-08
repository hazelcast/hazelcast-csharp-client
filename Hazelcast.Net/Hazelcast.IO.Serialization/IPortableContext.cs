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

        /// <exception cref="System.IO.IOException" />
        IClassDefinition LookupClassDefinition(IData data);

        IClassDefinition RegisterClassDefinition(IClassDefinition cd);

        /// <exception cref="System.IO.IOException" />
        IClassDefinition LookupOrRegisterClassDefinition(IPortable portable);

        IFieldDefinition GetFieldDefinition(IClassDefinition cd, string name);
        IManagedContext GetManagedContext();
        ByteOrder GetByteOrder();
    }
}