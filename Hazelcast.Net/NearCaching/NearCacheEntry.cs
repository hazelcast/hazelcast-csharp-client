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
using System.Threading;
using Hazelcast.Core;
using Hazelcast.Serialization;

namespace Hazelcast.NearCaching
{
    internal class NearCacheEntry
    {
        private readonly long _creationTime;
        private readonly long _expirationTime;

        private long _sequence; // sequence number of last received invalidation event
        private long _hits;

        private long _lastHitTime;
        private volatile int _partitionId;

        /// <summary>
        /// Initializes a new instance of the <see cref="NearCacheEntry"/> class.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="creationTime"></param>
        /// <param name="expirationTime"></param>
        internal NearCacheEntry(IData key, object value, long creationTime, long expirationTime)
        {
            Key = key;
            Value = value;

            _creationTime = creationTime;
            _expirationTime = expirationTime;
            _lastHitTime = creationTime;

            _sequence = _hits = 0;
        }

        /// <summary>
        /// Gets or sets the unique identifier of the record.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets the key of the record.
        /// </summary>
        public IData Key { get; }

        /// <summary>
        /// Gets the value of the record.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Gets the number of time the record has been hit.
        /// </summary>
        public long Hits => Interlocked.Read(ref _hits);

        /// <summary>
        /// Gets the partition identifier corresponding to the key.
        /// </summary>
        public int PartitionId
        {
            get => _partitionId;
            set => _partitionId = value;
        }

        /// <summary>
        /// Gets the sequence value.
        /// </summary>
        public long Sequence
        {
            get => Interlocked.Read(ref _sequence);
            set => Interlocked.Exchange(ref _sequence, value);
        }

        /// <summary>
        /// Gets the time when the record was last hit.
        /// </summary>
        internal long LastHitTime => Interlocked.Read(ref _lastHitTime);

        /// <summary>
        /// Notifies the record that it has been hit.
        /// </summary>
        internal void NotifyHit()
        {
            Interlocked.Increment(ref _hits);
            Interlocked.Exchange(ref _lastHitTime, Clock.Milliseconds);
        }

        /// <summary>
        /// Determines whether the record has expired at a given time.
        /// </summary>
        /// <param name="time">A time.</param>
        /// <returns>true if the record has expired at the specified time; otherwise false.</returns>
        internal bool IsExpiredAt(long time)
        {
            return _expirationTime > Clock.Never && _expirationTime <= time;
        }

        /// <summary>
        /// Determines whether the record is idle at a given time.
        /// </summary>
        /// <param name="idleMilliseconds">The period of time a record can remain un-hit before becoming idle.</param>
        /// <param name="time">A time.</param>
        /// <returns>true if the record is idle at the specified time; otherwise false.</returns>
        /// <remarks>
        /// <para>A record is idle if it has not been hit since <paramref name="idleMilliseconds"/>.</para>
        /// </remarks>
        internal bool IsIdleAt(long idleMilliseconds, long time)
        {
            if (idleMilliseconds <= 0) return false;

            var lastAccessTime = Interlocked.Read(ref _lastHitTime);

            return lastAccessTime > Clock.Never
                ? lastAccessTime + idleMilliseconds < time
                : _creationTime + idleMilliseconds < time;
        }
    }
}