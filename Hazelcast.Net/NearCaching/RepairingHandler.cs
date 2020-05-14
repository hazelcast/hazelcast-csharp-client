using System;
using System.Collections.Generic;
using System.Diagnostics;
using Hazelcast.Core;
using Hazelcast.Logging;
using Hazelcast.Partitioning;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.NearCaching
{
    internal class RepairingHandler
    {
        private readonly ILogger _logger;

        private readonly Guid _localUuid;
        private readonly int _maxToleratedMissCount;
        private readonly MetaData[] _metaDataContainers;

        private readonly NearCache _nearCache;
        private readonly int _partitionCount;

        private readonly ISerializationService _serializationService; // FIXME initialize!
        private readonly Partitioner _partitioner;

        public RepairingHandler(Guid localUuid, NearCache nearCache, Partitioner partitioner, ILoggerFactory loggerFactory)
        {
            _localUuid = localUuid;
            _nearCache = nearCache;
            _partitioner = partitioner;
            _partitionCount = partitioner.Count;
            _metaDataContainers = CreateMetadataContainers(_partitionCount);
            _maxToleratedMissCount = NearCacheManager.GetMaxToleratedMissCount();
            _logger = loggerFactory.CreateLogger<RepairingHandler>();
        }

        // multiple threads can concurrently call this method: one is anti-entropy, other one is event service thread
        public void CheckOrRepairGuid(int partition, Guid newUuid)
        {
            Debug.Assert(newUuid != Guid.Empty);
            var metaData = GetMetaDataContainer(partition);
            while (true)
            {
                var prevUuid = metaData.Guid;
                if (prevUuid.Equals(newUuid))
                {
                    break;
                }
                if (metaData.TrySetGuid(newUuid))
                {
                    metaData.ResetSequences();
                    _logger.LogDebug($"Invalid UUID, lost remote partition data unexpectedly:[name={_nearCache.Name},partition={partition},prevUuid={prevUuid},newUuid={newUuid}]");
                    break;
                }
            }
        }

        // Checks nextSequence against current one. And updates current sequence if next one is bigger.
        // multiple threads can concurrently call this method: one is anti-entropy, other one is event service thread
        public void CheckOrRepairSequence(int partition, long nextSequence, bool viaAntiEntropy)
        {
            Debug.Assert(nextSequence > 0);

            var metaData = GetMetaDataContainer(partition);
            while (true)
            {
                var currentSequence = metaData.Sequence;
                if (currentSequence >= nextSequence)
                {
                    break;
                }
                if (metaData.UpdateSequence(currentSequence, nextSequence))
                {
                    var sequenceDiff = nextSequence - currentSequence;
                    if (viaAntiEntropy || sequenceDiff > 1L)
                    {
                        // we have found at least one missing sequence between current and next sequences. if miss is detected by
                        // anti-entropy, number of missed sequences will be `miss = next - current`, otherwise it means miss is
                        // detected by observing received invalidation event sequence numbers and number of missed sequences will be
                        // `miss = next - current - 1`.
                        var missCount = viaAntiEntropy ? sequenceDiff : sequenceDiff - 1;
                        var totalMissCount = metaData.AddMissedSequences(missCount);

                        _logger.LogDebug($"Invalid sequence (map={_nearCache.Name}, partition={partition}, " +
                                        $"currentSeq={currentSequence}, nextSeq={nextSequence}, totalMiss={totalMissCount})");
                    }
                    break;
                }
            }
        }

        public MetaData GetMetaDataContainer(int partition)
        {
            return _metaDataContainers[partition];
        }

        //Handles a single invalidation
        public void Handle(IData key, Guid sourceUuid, Guid partitionGuid, long sequence)
        {
            // apply invalidation if it's not originated by local member/client (because local
            // Near Caches are invalidated immediately there is no need to invalidate them twice)
            if (!_localUuid.Equals(sourceUuid))
            {
                // sourceUuid is allowed to be `null`
                if (key == null)
                {
                    _nearCache.Clear();
                }
                else
                {
                    _nearCache.Remove(key);
                }
            }

            var partitionId = GetPartitionIdOrDefault(key);
            CheckOrRepairGuid(partitionId, partitionGuid);
            CheckOrRepairSequence(partitionId, sequence, false);
        }

        /**
         * Handles batch invalidations
         */
        public void Handle(IEnumerable<IData> keys, IEnumerable<Guid> sourceUuids, IEnumerable<Guid> partitionUuids,
            IEnumerable<long> sequences)
        {
            var keyIterator = keys.GetEnumerator();
            var sequenceIterator = sequences.GetEnumerator();
            var partitionUuidIterator = partitionUuids.GetEnumerator();
            var sourceUuidsIterator = sourceUuids.GetEnumerator();
            try
            {
                while (keyIterator.MoveNext() && sourceUuidsIterator.MoveNext() && partitionUuidIterator.MoveNext() &&
                       sequenceIterator.MoveNext())
                {
                    Handle(keyIterator.Current, sourceUuidsIterator.Current, partitionUuidIterator.Current,
                        sequenceIterator.Current);
                }
            }
            finally
            {
                keyIterator.Dispose();
                sequenceIterator.Dispose();
                partitionUuidIterator.Dispose();
                sourceUuidsIterator.Dispose();
            }
        }

        public void InitGuids(IList<KeyValuePair<int, Guid>> partitionUuidList)
        {
            foreach (var pair in partitionUuidList)
            {
                var partitionId = pair.Key;
                var partitionUuid = pair.Value;
                var metaData = GetMetaDataContainer(partitionId);
                metaData.Guid = partitionUuid;
            }
        }

        public void InitSequences(IList<KeyValuePair<string, IList<KeyValuePair<int, long>>>> namePartitionSequenceList)
        {
            foreach (var pair in namePartitionSequenceList)
            {
                foreach (var seqPair in pair.Value)
                {
                    var partitionId = seqPair.Key;
                    var partitionSequence = seqPair.Value;
                    var metaData = GetMetaDataContainer(partitionId);
                    metaData.Sequence = partitionSequence;
                }
            }
        }

        public override string ToString()
        {
            return string.Format("RepairingHandler{{name='{0}', localUuid='{1}'}}", _nearCache.Name, _localUuid);
        }

        internal void FixSequenceGap()
        {
            if (IsAboveMaxToleratedMissCount())
            {
                UpdateLastKnownStaleSequences();
            }
        }

        private static MetaData[] CreateMetadataContainers(int partitionCount)
        {
            var metaData = new MetaData[partitionCount];
            for (var partition = 0; partition < partitionCount; partition++)
            {
                metaData[partition] = new MetaData();
            }
            return metaData;
        }

        private int GetPartitionIdOrDefault(IData key)
        {
            // `name` is used to determine partition ID of map-wide events like clear()
            // since key is `null`, we are using `name` to find the partition ID
            if (key == null) key = _serializationService.ToData(_nearCache.Name);
            return _partitioner.GetPartitionId(key);
        }

        // Calculates number of missed invalidations and checks if repair is needed for the supplied handler.
        // Every handler represents a single Near Cache.
        private bool IsAboveMaxToleratedMissCount()
        {
            int partition = 0;
            long missCount = 0;
            do
            {
                var metaData = GetMetaDataContainer(partition);
                missCount += metaData.MissedSequenceCount;

                if (missCount > _maxToleratedMissCount)
                {
                    _logger.LogDebug($"Exceeded tolerated miss count (map={_nearCache.Name}, miss={missCount}, max={_maxToleratedMissCount}).");
                    return true;
                }
            } while (++partition < _partitionCount);
            return false;
        }

        private void UpdateLastKnownStaleSequences()
        {
            foreach (var metaDataContainer in _metaDataContainers)
            {
                var missCount = metaDataContainer.MissedSequenceCount;
                if (metaDataContainer.MissedSequenceCount != 0)
                {
                    metaDataContainer.AddMissedSequences(-missCount);
                    metaDataContainer.UpdateStaleSequence();
                }
            }
        }
    }
}
