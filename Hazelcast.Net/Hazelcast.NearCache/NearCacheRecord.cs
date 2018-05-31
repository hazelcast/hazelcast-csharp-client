// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;
using Hazelcast.Util;

namespace Hazelcast.NearCache
{
    internal class NearCacheRecord
    {
        private readonly long _creationTime;
        private readonly long _expirationTime;

        // Sequence number of last received invalidation event
        private readonly AtomicLong _sequence = new AtomicLong();
        internal readonly AtomicInteger Hit;

        internal readonly IData Key;
        internal readonly object Value;
        private long _lastAccessTime;

        private volatile int _partitionId;

        internal NearCacheRecord(IData key, object value, long creationTime, long expirationTime)
        {
            Key = key;
            Value = value;
            _lastAccessTime = creationTime;
            _creationTime = creationTime;
            _expirationTime = expirationTime;
            Hit = new AtomicInteger(0);
        }

        public int PartitionId
        {
            get { return _partitionId; }
            set { _partitionId = value; }
        }

        public long Sequence
        {
            get { return _sequence.Get(); }
            set { _sequence.Set(value); }
        }

        public Guid Guid { get; set; }

        internal long LastAccessTime
        {
            get { return Interlocked.Read(ref _lastAccessTime); }
        }

        internal void Access()
        {
            Hit.IncrementAndGet();
            Interlocked.Exchange(ref _lastAccessTime, Clock.CurrentTimeMillis());
        }

        internal bool IsExpiredAt(long now)
        {
            return _expirationTime > BaseNearCache.TimeNotSet && _expirationTime <= now;
        }

        internal bool IsIdleAt(long maxIdleMillis, long now)
        {
            if (maxIdleMillis <= 0) return false;
            return LastAccessTime > BaseNearCache.TimeNotSet
                ? LastAccessTime + maxIdleMillis < now
                : _creationTime + maxIdleMillis < now;
        }
    }
}