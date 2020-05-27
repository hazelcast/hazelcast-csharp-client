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

using System.Threading;
using Hazelcast.Core;

namespace Hazelcast.NearCaching
{
    internal class NearCacheStatistics
    {
        private long _evictions;
        private long _expirations;
        private long _hits;
        private long _misses;
        private long _ownedEntryCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="NearCacheStatistics"/> class.
        /// </summary>
        public NearCacheStatistics()
        {
            CreationTime = Clock.Milliseconds;
        }

        /// <summary>
        /// Gets the time when the cache was created.
        /// </summary>
        public long CreationTime { get; }

        /// <summary>
        /// Gets the number of hits.
        /// </summary>
        public long Hits => Interlocked.Read(ref _hits);

        /// <summary>
        /// Gets the number of misses.
        /// </summary>
        public long Misses => Interlocked.Read(ref _misses);

        /// <summary>
        /// Gets the number of evictions.
        /// </summary>
        public long Evictions => Interlocked.Read(ref _evictions);

        /// <summary>
        /// Gets the number of expirations.
        /// </summary>
        public long Expirations => Interlocked.Read(ref _expirations);

        /// <summary>
        /// Gets the number of entries.
        /// </summary>
        public long EntryCount
        {
            get => Interlocked.Read(ref _ownedEntryCount);
            set => _ownedEntryCount = value;
        }

        /// <summary>
        /// Decrements the number of entries.
        /// </summary>
        public void DecrementEntryCount()
        {
            Interlocked.Decrement(ref _ownedEntryCount);
        }

        /// <summary>
        /// Increments the number of entries.
        /// </summary>
        public void IncrementEntryCount()
        {
            Interlocked.Increment(ref _ownedEntryCount);
        }

        /// <summary>
        /// Increments the number of evictions.
        /// </summary>
        public void IncrementEviction()
        {
            Interlocked.Increment(ref _evictions);
        }

        /// <summary>
        /// Increments the number of expirations.
        /// </summary>
        public void IncrementExpiration()
        {
            Interlocked.Increment(ref _expirations);
        }

        /// <summary>
        /// Increment the number of hits.
        /// </summary>
        public void IncrementHit()
        {
            Interlocked.Increment(ref _hits);
        }

        /// <summary>
        /// Increment the number of misses.
        /// </summary>
        public void IncrementMiss()
        {
            Interlocked.Increment(ref _misses);
        }
    }
}