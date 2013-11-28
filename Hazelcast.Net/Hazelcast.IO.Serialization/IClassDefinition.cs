using System.Collections.Generic;

namespace Hazelcast.IO.Serialization
{
    public interface IClassDefinition : IDataSerializable
    {
        int GetFactoryId();

        IFieldDefinition Get(string name);

        IFieldDefinition Get(int fieldIndex);

        bool HasField(string fieldName);

        ICollection<string> GetFieldNames();

        FieldType GetFieldType(string fieldName);

        int GetFieldClassId(string fieldName);

        int GetFieldCount();

        int GetClassId();

        int GetVersion();
    }
}