namespace Hazelcast.IO.Serialization
{
    public interface IFieldDefinition : IDataSerializable
    {
        FieldType GetFieldType();

        string GetName();

        int GetIndex();

        int GetClassId();

        int GetFactoryId();
    }
}