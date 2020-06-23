namespace Hazelcast.Serialization
{
    internal interface ISerializerDefinitions
    {
        void AddSerializers(SerializationService service);
    }
}
