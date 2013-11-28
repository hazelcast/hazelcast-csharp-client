namespace Hazelcast.IO.Serialization
{
    public interface IPortableAdapter<T> : IPortable
    {
        object ToObject();
    }
}