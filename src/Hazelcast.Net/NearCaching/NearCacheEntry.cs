// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
    /// <summary>
    /// Represents an entry in the <see cref="NearCache"/>.
    /// </summary>
    internal class NearCacheEntry
    {
        private readonly long _creationTime;
        private readonly long _expirationTime;

        private long _sequence;
        private long _hits;

        private long _lastHitTime;
        private volatile int _partitionId;

        /// <summary>
        /// Initializes a new instance of the <see cref="NearCacheEntry"/> class.
        /// </summary>
        /// <param name="keyData">The key data.</param>
        /// <param name="valueObject">The value object.</param>
        /// <param name="timeToLive">The time the entry lives, before expiring.</param>
        /// <remarks>
        /// <para>The <paramref name="valueObject"/> can be either <see cref="IData"/> or
        /// TValue, depending on the configured <see cref="InMemoryFormat"/> of the cache.</para>
        /// </remarks>
        internal NearCacheEntry(IData keyData, object valueObject, long timeToLive)
        {
            KeyData = keyData;
            ValueObject = valueObject;
            _creationTime = Clock.Milliseconds;
            _expirationTime = timeToLive > 0 ? _creationTime + timeToLive : Clock.Never;
            _lastHitTime = Clock.Never;
        }

        /// <summary>
        /// Gets or sets the unique identifier of the entry.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets the key data of the entry.
        /// </summary>
        public IData KeyData { get; }

        /// <summary>
        /// Gets the value object of the entry.
        /// </summary>
        public object ValueObject { get; }

        /// <summary>
        /// Gets the number of time the entry has been hit.
        /// </summary>
        public long Hits => Interlocked.Read(ref _hits);

        /// <summary>
        /// Gets or sets the partition identifier corresponding to the entry key.
        /// </summary>
        public int PartitionId
        {
            get => _partitionId;
            set => _partitionId = value;
        }

        /// <summary>
        /// Gets or sets the sequence value of the entry.
        /// </summary>
        public long Sequence
        {
            get => Interlocked.Read(ref _sequence);
            set => Interlocked.Exchange(ref _sequence, value);
        }

        /// <summary>
        /// Gets the time when the entry was last hit.
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
        /// Determines whether the record is expired at a given time.
        /// </summary>
        /// <param name="time">A time.</param>
        /// <returns><c>true</c> if the record is expired at the specified time; otherwise <c>false</c>.</returns>
        internal bool IsExpiredAt(long time)
        {
            return _expirationTime > Clock.Never && _expirationTime <= time;
        }

        /// <summary>
        /// Determines whether the record is idle at a given time.
        /// </summary>
        /// <param name="idleMilliseconds">The period of time a record can remain un-hit before becoming idle.</param>
        /// <param name="time">A time.</param>
        /// <returns><c>true</c> if the record is idle at the specified time; otherwise <c>false</c>.</returns>
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
