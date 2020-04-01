// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Diagnostics;
using Hazelcast.Client.Spi;
using Hazelcast.IO.Serialization;
using Hazelcast.Logging;

namespace Hazelcast.NearCache
{
    internal class RepairingHandler
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof(RepairingHandler));

        private readonly string _localUuid;
        private readonly int _maxToleratedMissCount;
        private readonly MetaDataContainer[] _metaDataContainers;

        private readonly NearCache _nearCache;
        private readonly int _partitionCount;

        private readonly IClientPartitionService _partitionService;

        public RepairingHandler(string localUuid, NearCache nearCache, IClientPartitionService partitionService)
        {
            _localUuid = localUuid;
            _nearCache = nearCache;
            _partitionService = partitionService;
            _partitionCount = partitionService.GetPartitionCount();
            _metaDataContainers = CreateMetadataContainers(_partitionCount);
            _maxToleratedMissCount = NearCacheManager.GetMaxToleratedMissCount();
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
                    metaData.ResetAllSequences();
                    if (Logger.IsFinestEnabled())
                    {
                        Logger.Finest(string.Format(
                            "Invalid UUID, lost remote partition data unexpectedly:[name={0},partition={1},prevUuid={2},newUuid={3}]",
                            _nearCache.Name, partition, prevUuid, newUuid));
                    }
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
                if (metaData.CompareAndExcangeSquence(currentSequence, nextSequence))
                {
                    var sequenceDiff = nextSequence - currentSequence;
                    if (viaAntiEntropy || sequenceDiff > 1L)
                    {
                        // we have found at least one missing sequence between current and next sequences. if miss is detected by
                        // anti-entropy, number of missed sequences will be `miss = next - current`, otherwise it means miss is
                        // detected by observing received invalidation event sequence numbers and number of missed sequences will be
                        // `miss = next - current - 1`.
                        var missCount = viaAntiEntropy ? sequenceDiff : sequenceDiff - 1;
                        var totalMissCount = metaData.AddAndGetMissedSequenceCount(missCount);

                        if (Logger.IsFinestEnabled())
                        {
                            Logger.Finest(string.Format(
                                "Invalid sequence:[map={0},partition={1},currentSequence={2},nextSequence={3},totalMissCount={4}]",
                                _nearCache.Name, partition, currentSequence, nextSequence, totalMissCount));
                        }
                    }
                    break;
                }
            }
        }

        public MetaDataContainer GetMetaDataContainer(int partition)
        {
            return _metaDataContainers[partition];
        }

        //Handles a single invalidation
        public void Handle(IData key, string sourceUuid, Guid partitionGuid, long sequence)
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
        public void Handle(IEnumerable<IData> keys, IEnumerable<string> sourceUuids, IEnumerable<Guid> partitionUuids,
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

        private static MetaDataContainer[] CreateMetadataContainers(int partitionCount)
        {
            var metaData = new MetaDataContainer[partitionCount];
            for (var partition = 0; partition < partitionCount; partition++)
            {
                metaData[partition] = new MetaDataContainer();
            }
            return metaData;
        }

        private int GetPartitionIdOrDefault(IData key)
        {
            // `name` is used to determine partition ID of map-wide events like clear()
            // since key is `null`, we are using `name` to find the partition ID
            return key == null
                ? _partitionService.GetPartitionId(_nearCache.Name)
                : _partitionService.GetPartitionId(key);
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
                    if (Logger.IsFinestEnabled())
                    {
                        Logger.Finest(string.Format(
                            "Above tolerated miss count:[map={0}, missCount={1}, maxToleratedMissCount={2}]",
                            _nearCache.Name, missCount, _maxToleratedMissCount));
                    }
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
                    metaDataContainer.AddAndGetMissedSequenceCount(-missCount);
                    metaDataContainer.UpdateLastKnownStaleSequence();
                }
            }
        }
    }
}