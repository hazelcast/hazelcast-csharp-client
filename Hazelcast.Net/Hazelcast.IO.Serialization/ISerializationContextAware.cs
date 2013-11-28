namespace Hazelcast.IO.Serialization
{
    internal interface ISerializationContextAware
    {
        ISerializationContext GetSerializationContext();
    }
}