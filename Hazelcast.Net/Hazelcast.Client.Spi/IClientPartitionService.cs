using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Spi
{
    public interface IClientPartitionService
    {
        Address GetPartitionOwner(int partitionId);

        //int GetPartitionId(Data key);

        int GetPartitionId(object key);

        int GetPartitionCount();

        //IPartition GetPartition(int partitionId);
    }
}