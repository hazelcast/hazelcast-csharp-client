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
    // NOTES ABOUT ATOMICITY
    //
    // Partition I, Section 12.6.6 of the CLI spec states: "A conforming CLI shall guarantee
    // that read and write access to properly aligned memory locations no larger than the native
    // word size is atomic when all the write accesses to a location are the same size."
    //
    // And, C# specs section 5.5 states: "Reads and writes of the following data types are
    // atomic: bool, char, byte, sbyte, short, ushort, uint, int, float, and reference types.
    // In addition, reads and writes of enum types with an underlying type in the previous
    // list are also atomic. Reads and writes of other types, including long, ulong, double,
    // and decimal, as well as user-defined types, are not guaranteed to be atomic."
    //
    // However - atomic does not imply thread-safety due to the processor reordering reads and
    // writes. The variables should be marked volatile, or accessed through Interlocked.
    //
    // References
    // https://stackoverflow.com/questions/9666/is-accessing-a-variable-in-c-sharp-an-atomic-operation
    // https://stackoverflow.com/questions/2433772/are-primitive-data-types-in-c-sharp-atomic-thread-safe
    //
    // So,
    // Anything safe as per section 5.5 quoted above has to be volatile
    // Anything not safe has to be Interlocked.

    internal class NearCacheRecord
    {
        private readonly long _creationTime;
        private readonly long _expirationTime;

        private long _sequence; // sequence number of last received invalidation event
        private long _hits;

        private long _lastHitTime;
        private volatile int _partitionId;

        /// <summary>
        /// Initializes a new instance of the <see cref="NearCacheRecord"/> class.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="creationTime"></param>
        /// <param name="expirationTime"></param>
        internal NearCacheRecord(IData key, object value, long creationTime, long expirationTime)
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
        /// Gets the partition identifier.
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
        /// Determines whether the record has expired as a time.
        /// </summary>
        /// <param name="time">A time.</param>
        /// <returns>true if the record has expired at the specified time; otherwise false.</returns>
        internal bool IsExpiredAt(long time)
        {
            return _expirationTime > NearCacheBase.TimeNotSet && _expirationTime <= time;
        }

        /// <summary>
        /// Determines whether the record ... fixme?
        /// </summary>
        /// <param name="maxIdleMilliseconds"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        internal bool IsIdleAt(long maxIdleMilliseconds, long now)
        {
            if (maxIdleMilliseconds <= 0) return false;

            var lastAccessTime = Interlocked.Read(ref _lastHitTime);

            return lastAccessTime > NearCacheBase.TimeNotSet
                ? lastAccessTime + maxIdleMilliseconds < now
                : _creationTime + maxIdleMilliseconds < now;
        }
    }
}