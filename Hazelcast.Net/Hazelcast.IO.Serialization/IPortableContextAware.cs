namespace Hazelcast.IO.Serialization
{
    internal interface IPortableContextAware
    {
        IPortableContext GetSerializationContext();
    }
}