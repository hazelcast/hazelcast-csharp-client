namespace Hazelcast.IO.Serialization
{
    public interface DataSerializerHook
    {
        int GetFactoryId();

        IDataSerializableFactory CreateFactory();
    }
}