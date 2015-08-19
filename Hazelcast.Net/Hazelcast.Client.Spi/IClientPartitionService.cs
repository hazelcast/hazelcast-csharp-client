using Hazelcast.IO;

namespace Hazelcast.Client.Spi
{
    public interface IClientPartitionService
    {
        Address GetPartitionOwner(int partitionId);
        int GetPartitionId(object key);
        int GetPartitionCount();
    }
}