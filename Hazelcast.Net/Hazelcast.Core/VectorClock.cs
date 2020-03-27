/*
 * Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;

namespace Hazelcast.Core
{
    internal class VectorClock
    {
        private readonly Dictionary<Guid, long> _timeStampDictionary = new Dictionary<Guid, long>();

        public VectorClock()
        {
        }

        public VectorClock(IList<KeyValuePair<Guid, long>> timeStampList)
        {
            foreach (var pair in timeStampList)
            {
                _timeStampDictionary.Add(pair.Key, pair.Value);
            }
        }

        internal bool IsAfter(VectorClock other)
        {
            var anyTimestampGreater = false;
            foreach (var otherEntry in other._timeStampDictionary)
            {
                var replicaId = otherEntry.Key;
                var otherReplicaTimestamp = otherEntry.Value;
                if (!_timeStampDictionary.TryGetValue(replicaId, out var localReplicaTimestamp) || localReplicaTimestamp < otherReplicaTimestamp)
                {
                    return false;
                }
                if (localReplicaTimestamp > otherReplicaTimestamp)
                {
                    anyTimestampGreater = true;
                }
            }
            // there is at least one local timestamp greater or local vector clock has additional timestamps
            return anyTimestampGreater || other._timeStampDictionary.Count < _timeStampDictionary.Count;
        }

        // Returns a set of replica logical timestamps for this vector clock.
        public ICollection<KeyValuePair<Guid, long>> EntrySet()
        {
            return _timeStampDictionary;
        }
    }
}
