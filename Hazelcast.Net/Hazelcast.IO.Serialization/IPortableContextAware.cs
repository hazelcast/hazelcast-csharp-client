namespace Hazelcast.IO.Serialization
{
    internal interface IPortableContextAware
    {
        IPortableContext GetPortableContext();
    }
}