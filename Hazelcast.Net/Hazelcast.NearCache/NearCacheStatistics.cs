// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

using System.Threading;
using Hazelcast.Util;

namespace Hazelcast.NearCache
{
    internal class NearCacheStatistics
    {
        private readonly long _creationTime;
        private long _evictions;
        private long _expirations;
        private long _hits;
        private long _misses;
        private long _ownedEntryCount;

        public NearCacheStatistics()
        {
            _creationTime = Clock.CurrentTimeMillis();
        }

        public long CreationTime => _creationTime;

        public long Hits => Interlocked.Read(ref _hits);

        public long Misses => Interlocked.Read(ref _misses);

        public long Evictions => Interlocked.Read(ref _evictions);

        public long Expirations => Interlocked.Read(ref _expirations);

        public long OwnedEntryCount
        {
            get => Interlocked.Read(ref _ownedEntryCount);
            set => _ownedEntryCount = value;
        }

        public void DecrementOwnedEntryCount()
        {
            Interlocked.Decrement(ref _ownedEntryCount);
        }

        public void IncrementEviction()
        {
            Interlocked.Increment(ref _evictions);
        }

        public void IncrementExpiration()
        {
            Interlocked.Increment(ref _expirations);
        }

        public void IncrementHit()
        {
            Interlocked.Increment(ref _hits);
        }

        public void IncrementMiss()
        {
            Interlocked.Increment(ref _misses);
        }

        public void IncrementOwnedEntryCount()
        {
            Interlocked.Increment(ref _ownedEntryCount);
        }
    }
}