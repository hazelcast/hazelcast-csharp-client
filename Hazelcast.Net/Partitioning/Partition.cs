using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Hazelcast.Clustering;
using Hazelcast.Logging;
using Hazelcast.Serialization;

namespace Hazelcast.Partitioning
{
    class Partition
    {
    }

    // the cluster needs a partitioner (and can expose it?) to handle partitioning
    // a 'cluster' is per-hzclient and handles all the connections etc
    // but could there be a shared 'cluster' representation? singleton partitioner?

    public class Partitioner
    {
        // fixme this is going to introduce a mutual dependency with serialization?
        private readonly ISerializationService _serializationService;
        private readonly bool _isSmartRouting;
        private PartitionTable _partitions; // FIXME lock/threading/etc

        /// <summary>
        /// Initializes a new instance of the <see cref="Partitioner"/> class.
        /// </summary>
        /// <param name="serializationService">The serialization service.</param>
        /// <param name="isSmartRouting">Whether the cluster operates with smart routing.</param>
        public Partitioner(ISerializationService serializationService, bool isSmartRouting)
        {
            _serializationService = serializationService;// ?? throw new ArgumentNullException(nameof(serializationService));
            _isSmartRouting = isSmartRouting;
        }

        /// <summary>
        /// Gets the unique identifier of the member owning a partition.
        /// </summary>
        /// <param name="partitionId">The identifier of the partition.</param>
        /// <returns>The unique identifier of the member owning the partition, or an empty Guid if the partition has no owner.</returns>
        public Guid GetPartitionOwner(int partitionId)
        {
            return _partitions?.MapPartitionId(partitionId) ?? default;
        }

        /// <summary>
        /// Gets the partition identifier for an <see cref="IData"/> instance.
        /// </summary>
        /// <param name="data">The <see cref="IData"/> instance.</param>
        /// <returns>The partition identifier for the specified <see cref="IData"/> instance.</returns>
        public int GetPartitionId(IData data)
        {
            if (_partitions == null) return 0;
            var partitionCount = _partitions.Count;
            var hash = data.PartitionHash;
            return hash == int.MinValue
                ? 0
                : Math.Abs(hash) % partitionCount;
        }

        /// <summary>
        /// Gets the partition identifier for an object.
        /// </summary>
        /// <param name="o">The object.</param>
        /// <returns>The partition identifier for the specified object.</returns>
        public int GetPartitionId(object o) // fixme only reason for the ISerializationService dependency
        {
            return _partitions == null
                ? 0
                : GetPartitionId(_serializationService.ToData(o));
        }

        // fixme
        public void HandlePartitionViewEvent(Guid originClientId, int version, Dictionary<int, Guid> partitionsMap)
        {
            // fixme locking
            if (_partitions == null || _partitions.IsSupersededBy(originClientId, version, partitionsMap))
                _partitions = new PartitionTable(originClientId, version, partitionsMap);

            XConsole.WriteLine(this, "PARTITIONS\n" + string.Join("\n", partitionsMap.Select(kvp => $"{kvp.Key}:{kvp.Value}")));
        }

        /// <summary>
        /// Represents a cluster partition table.
        /// </summary>
        private class PartitionTable
        {
            private readonly Dictionary<int, Guid> _partitionsMap;

            /// <summary>
            /// Initializes a new instance of the <see cref="PartitionTable"/> class.
            /// </summary>
            /// <param name="originClientId">The unique identifier of the client that originated the table.</param>
            /// <param name="version">The version of the partition table.</param>
            /// <param name="partitionsMap">A map of partition identifiers to cluster member identifiers.</param>
            public PartitionTable(Guid originClientId, int version, Dictionary<int, Guid> partitionsMap)
            {
                OriginClientId = originClientId;
                Version = version;
                _partitionsMap = partitionsMap;
            }

            /// <summary>
            /// Gets the identifier of the client that originated the table.
            /// </summary>
            public Guid OriginClientId { get; }

            /// <summary>
            /// Gets the version of the partition table.
            /// </summary>
            public int Version { get; }

            /// <summary>
            /// Gets the number of partitions in the table.
            /// </summary>
            public int Count => _partitionsMap.Count;

            /// <summary>
            /// Maps a partition to the corresponding cluster member.
            /// </summary>
            /// <param name="partitionId">The identifier of the partition.</param>
            /// <returns>The identifier of the cluster member corresponding to the partition, or an
            /// empty Guid if no cluster member corresponds to the partition.</returns>
            public Guid MapPartitionId(int partitionId)
                => _partitionsMap.TryGetValue(partitionId, out var memberId) ? memberId : default;

            /// <summary>
            /// Determines whether this partition table is superseded by a new table.
            /// </summary>
            /// <param name="originClientId">The identifier of the client that originated the new partition table.</param>
            /// <param name="version">The version of the new partition table.</param>
            /// <param name="partitionsMap">A new map of partition identifiers to cluster member identifiers.</param>
            /// <returns></returns>
            public bool IsSupersededBy(Guid originClientId, int version, Dictionary<int, Guid> partitionsMap)
                => partitionsMap.Count > 0 && (originClientId != OriginClientId || version > Version);
        }
    }

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
