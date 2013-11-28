namespace Hazelcast.IO.Serialization
{
    public interface IPortableFactory
    {
        // TODO: return null if type id isn't known by factory
        IPortable Create(int classId);
    }
}