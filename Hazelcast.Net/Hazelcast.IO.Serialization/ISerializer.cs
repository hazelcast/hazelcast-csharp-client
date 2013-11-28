namespace Hazelcast.IO.Serialization
{
    public interface ISerializer
    {
        int GetTypeId();

        void Destroy();
    }
}