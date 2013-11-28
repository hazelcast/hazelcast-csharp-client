namespace Hazelcast.IO.Serialization
{
    public interface IIdentifiedDataSerializable : IDataSerializable
    {
        int GetFactoryId();

        int GetId();
    }
}