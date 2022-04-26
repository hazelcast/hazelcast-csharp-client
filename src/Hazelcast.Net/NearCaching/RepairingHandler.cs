// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using Hazelcast.Core;
using Hazelcast.Partitioning;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.NearCaching
{
    /// <summary>
    /// Represents a Near Cache repairing handler.
    /// </summary>
    internal class RepairingHandler
    {
        private readonly ILogger _logger;
        private readonly Guid _clusterClientId;
        private readonly int _maxToleratedMissCount;
        private readonly MetaData[] _metadataTable;
        private readonly NearCache _nearCache;
        private readonly int _partitionCount;
        private readonly SerializationService _serializationService;
        private readonly Partitioner _partitioner;

        /// <summary>
        /// Initializes a new instance of the <see cref="RepairingHandler"/> class.
        /// </summary>
        /// <param name="clusterClientId">The unique identifier of the cluster, as assigned by the client.</param>
        /// <param name="nearCache">The near cache instance.</param>
        /// <param name="maxToleratedMissCount">The max tolerated miss count.</param>
        /// <param name="partitioner">The partitioner.</param>
        /// <param name="serializationService">The serialization service.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public RepairingHandler(Guid clusterClientId, NearCache nearCache, int maxToleratedMissCount, Partitioner partitioner, SerializationService serializationService, ILoggerFactory loggerFactory)
        {
            _clusterClientId = clusterClientId;
            _nearCache = nearCache;
            _partitioner = partitioner;
            _partitionCount = partitioner.Count;
            _metadataTable = CreateMetadataTable(_partitionCount);
            _maxToleratedMissCount = maxToleratedMissCount;
            _serializationService = serializationService;
            _logger = loggerFactory.CreateLogger<RepairingHandler>();
        }

        // multiple threads can concurrently call this method: one is anti-entropy, other one is event service thread
        // TODO: understand and document what those GUIDs are?!
        /// <summary>
        /// Updates the ???
        /// </summary>
        /// <param name="partitionId">The partition identifier.</param>
        /// <param name="newUuid">???</param>
        public void UpdateUuid(int partitionId, Guid newUuid)
        {
            if (newUuid == default) throw new ArgumentOutOfRangeException(nameof(newUuid));

            var metadata = GetMetadata(partitionId);

            while (true)
            {
                var currentUuid = metadata.Guid;

                // ignore if not changed
                if (currentUuid.Equals(newUuid))
                    break;

                // try to update the ???, loop if current ??? is locked?
                // assuming that eventually, the new ??? will be accepted
                if (!metadata.TrySetGuid(newUuid))
                    continue;

                // reset and report
                metadata.ResetSequences();
                _logger.IfDebug()?.LogDebug("Invalid UUID, lost remote partition data unexpectedly (map={NearCacheName}, partition={PartitionId}, current={CurrentUuid}, new={NewUuid})", _nearCache.Name, partitionId, currentUuid, newUuid);

                // we're done
                break;
            }
        }

        /// <summary>
        /// Updates the sequence of a partition.
        /// </summary>
        /// <param name="partitionId">The partition identifier.</param>
        /// <param name="newSequence">The new sequence value.</param>
        /// <param name="viaAntiEntropy">Whether the method is invoked by the anti-entropy task.</param>
        public void UpdateSequence(int partitionId, long newSequence, bool viaAntiEntropy)
        {
            if (newSequence < 0) throw new ArgumentOutOfRangeException(nameof(newSequence));

            var metadata = GetMetadata(partitionId);

            while (true)
            {
                var currentSequence = metadata.Sequence;

                // ignore an obsolete new sequence
                if (currentSequence >= newSequence)
                    break;

                // try to update the sequence, loop if current sequence has changed in the meantime
                // assuming that, eventually, the new sequence will either be accepted, or obsolete
                if (!metadata.UpdateSequence(currentSequence, newSequence))
                    continue;

                // sequence has been updated - handle the change
                var sequenceDelta = newSequence - currentSequence;
                if (viaAntiEntropy || sequenceDelta > 1L)
                {
                    // we have found at least one missing sequence between current and new sequences. if miss is detected by
                    // anti-entropy, number of missed sequences will be 'miss = new - current', otherwise it means miss is
                    // detected by observing received invalidation event sequence numbers and number of missed sequences will be
                    // 'miss =  new - current - 1'.
                    var missCount = viaAntiEntropy ? sequenceDelta : sequenceDelta - 1;
                    var totalMissCount = metadata.AddMissedSequences(missCount);

                    // report
                    _logger.IfDebug()?.LogDebug("Invalid sequence (map={NearCacheName}, partition={PartitionId}, current={CurrentSequence}, new={NewSequence}, totalMiss={TotalMissCount})", _nearCache.Name, partitionId, currentSequence, newSequence, totalMissCount);
                }

                // we're done
                break;
            }
        }

        #region Meta data

        /// <summary>
        /// Populates a meta data table.
        /// </summary>
        /// <param name="partitionCount">The number of partitions.</param>
        /// <returns>A meta data table.</returns>
        private static MetaData[] CreateMetadataTable(int partitionCount)
        {
            var metaData = new MetaData[partitionCount];
            for (var partitionId = 0; partitionId < partitionCount; partitionId++)
                metaData[partitionId] = new MetaData();
            return metaData;
        }

        /// <summary>
        /// Gets meta data for a partition.
        /// </summary>
        /// <param name="partitionId">The partition identifier.</param>
        /// <returns>Meta data for the specified partition.</returns>
        public MetaData GetMetadata(int partitionId)
            => _metadataTable[partitionId];

        #endregion

        /// <summary>
        /// Handles an invalidation.
        /// </summary>
        /// <param name="key">The invalidated key.</param>
        /// <param name="sourceClusterClientId">The identifier of the cluster client originating the event.</param>
        /// <param name="partitionGuid">???</param>
        /// <param name="sequence">The sequence.</param>
        public void Handle(IData key, Guid sourceClusterClientId, Guid partitionGuid, long sequence)
        {
            // apply invalidation if it's not originated by the cluster client (Hazelcast client)
            // running this code, because local Near Caches are invalidated immediately.
            if (!_clusterClientId.Equals(sourceClusterClientId))
            {
                // sourceClusterClientId is allowed to be null, meaning: all
                if (key == null)
                    _nearCache.Clear();
                else
                    _nearCache.Remove(key);
            }

            var partitionId = GetPartitionIdOrDefault(key);
            UpdateUuid(partitionId, partitionGuid);
            UpdateSequence(partitionId, sequence, false);
        }

        /// <summary>
        /// Handles an invalidation.
        /// </summary>
        /// <param name="keys">The invalidated keys.</param>
        /// <param name="sourceClusterClientIds">The identifiers of the cluster client originating the event.</param>
        /// <param name="partitionUuids"></param>
        /// <param name="sequences">The sequences.</param>
        public void Handle(IEnumerable<IData> keys, IEnumerable<Guid> sourceClusterClientIds, IEnumerable<Guid> partitionUuids,
            IEnumerable<long> sequences)
        {
            foreach (var (key, sourceClusterClientId, partitionUuid, sequence) in (keys, sourceClusterClientIds, partitionUuids, sequences).Combine())
                Handle(key, sourceClusterClientId, partitionUuid, sequence);
        }

        /// <summary>
        /// Initializes the ????
        /// </summary>
        /// <param name="partitionUuidList"></param>
        public void InitializeGuids(IList<KeyValuePair<int, Guid>> partitionUuidList)
        {
            // received from server:
            // (partition id -> partition ???), (...), ...

            foreach (var (partitionId, partitionUuid) in partitionUuidList)
            {
                var metadata = GetMetadata(partitionId);
                metadata.Guid = partitionUuid;
            }
        }

        /// <summary>
        /// Initializes the partition sequences.
        /// </summary>
        /// <param name="partitionSequencesTable">The partition sequences table.</param>
        public void InitializeSequences(IList<KeyValuePair<string, IList<KeyValuePair<int, long>>>> partitionSequencesTable)
        {
            // received from server:
            // cache name -> ( partition id -> partition sequence ), (...), ...

            foreach (var (_, partitionSequences) in partitionSequencesTable)
            foreach (var (partitionId, partitionSequence) in partitionSequences)
            {
                var metadata = GetMetadata(partitionId);
                metadata.Sequence = partitionSequence;
            }
        }

        public override string ToString()
        {
            return $"RepairingHandler{{name='{_nearCache.Name}', localUuid='{_clusterClientId}'}}";
        }

        internal void FixSequenceGap()
        {
            if (IsAboveMaxToleratedMissCount())
            {
                UpdateLastKnownStaleSequences();
            }
        }

        private int GetPartitionIdOrDefault(IData key)
        {
            // `name` is used to determine partition ID of map-wide events like clear()
            // since key is `null`, we are using `name` to find the partition ID
            if (key == null) key = _serializationService.ToData(_nearCache.Name);
            return _partitioner.GetPartitionId(key.PartitionHash);
        }

        // Calculates number of missed invalidations and checks if repair is needed for the supplied handler.
        // Every handler represents a single Near Cache.
        private bool IsAboveMaxToleratedMissCount()
        {
            int partition = 0;
            long missCount = 0;
            do
            {
                var metaData = GetMetadata(partition);
                missCount += metaData.MissedSequenceCount;

                if (missCount > _maxToleratedMissCount)
                {
                    _logger.IfDebug()?.LogDebug("Exceeded tolerated miss count (map={NearCacheName}, miss={MissCount}, max={MaxToleratedMissCount}).", _nearCache.Name, missCount, _maxToleratedMissCount);
                    return true;
                }
            } while (++partition < _partitionCount);
            return false;
        }

        private void UpdateLastKnownStaleSequences()
        {
            foreach (var metaDataContainer in _metadataTable)
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
