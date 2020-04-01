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

using System.Collections.Generic;
using TimeStampIList = System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<string, long>>;

namespace Hazelcast.Core
{
    internal class VectorClock
    {
        private readonly Dictionary<string, long> _timeStampDictionary = new Dictionary<string, long>();
        private readonly TimeStampIList _timeStampList = new List<KeyValuePair<string, long>>();

        public VectorClock()
        {
        }

        public VectorClock(TimeStampIList timeStampList)
        {
            _timeStampList = timeStampList;
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
                long localReplicaTimestamp;
                if (!_timeStampDictionary.TryGetValue(replicaId, out localReplicaTimestamp) || localReplicaTimestamp < otherReplicaTimestamp)
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
        public IList<KeyValuePair<string, long>> EntrySet()
        {
            return _timeStampList;
        }
    }
}
