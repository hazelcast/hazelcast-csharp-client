namespace Hazelcast.IO.Serialization
{
    public interface IDataSerializableFactory
    {
        // TODO: return null if type id isn't known by factory
        IIdentifiedDataSerializable Create(int typeId);
    }
}