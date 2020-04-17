using System.Collections.Concurrent;
using System.ComponentModel;
using System.Text;
using Hazelcast.Clustering;

namespace Hazelcast.Partitioning
{
    class Partition
    {
    }

    // the cluster needs a partitioner (and can expose it?) to handle partitioning
    // a 'cluster' is per-hzclient and handles all the connections etc
    // but could there be a shared 'cluster' representation? singleton partitioner?

    // there is one partition service per 'client'
    // uses 'isSmart' for isSmartRouting and todo: what is this?

    // an object stored in hz has a partitioning key
    // this key is serialized to IData
    // this serialized value is hashed
    // and % with the partition count -> partition id
    //
    // the partitioning key is determined by a partitioning strategy
    // which does hz full object -> hz key object
    //
    // IData has PartitionHash property meaning it can provide its own hash
    // otherwise, the hash is obtained by native GetHashCode()
    //
    // todo: what happens in serializer service?
    // this is where partitioning strategies seems to be maintained?
    //
    // the partitioner has a table of partitions
    //   partition id (int) -> cluster member id (guid)
    //   'state version' = version of the table
    //   'connection' = the connection that reported the table
    //
    // event: PartitionViewEvent
    //  if different connection or greater state version or ...
    //  must apply = replaces the partition table
    // todo: subscription?
    //
    // get partition owner: use partitions table to do partition id (int) -> cluster member id (guid)
    // get partition id: use strategies and stuff -> partition id (int)

    // events
    // partition lost
    // - lost backup count (0 = owner, 1 = first backup, 2 = second...)
    // - true if all replicas of a partition are lost = ?
    // - source / address of the node that dispatches the event
    // exists but is not actually used
}
