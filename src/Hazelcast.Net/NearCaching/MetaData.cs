// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.NearCaching
{
    internal class MetaData
    {
        private readonly object _guidMutex = new object();

        // Number of missed sequence count
        private long _missedSequenceCount;

        // sequence number of the last received invalidation event
        private long _sequence;

        // Holds the biggest sequence number that is lost, lower sequences from this sequence are accepted as stale
        private long _staleSequence;

        // UUID of the source partition that generates invalidation events

        public Guid Guid { get; set; }

        public long Sequence
        {
            get => Interlocked.Read(ref _sequence);
            set => Interlocked.Exchange(ref _sequence, value);
        }

        public long StaleSequence
        {
            get => Interlocked.Read(ref _staleSequence);
            set => Interlocked.Exchange(ref _staleSequence, value);
        }

        public long MissedSequenceCount => Interlocked.Read(ref _missedSequenceCount);

        public long AddMissedSequences(long count)
            => Interlocked.Add(ref _missedSequenceCount, count);

        public bool UpdateSequence(long expectedValue, long newValue)
            => Interlocked.CompareExchange(ref _sequence, newValue, expectedValue) == expectedValue;

        private bool UpdateStaleSequence(long expectedValue, long newValue)
            => Interlocked.CompareExchange(ref _staleSequence, newValue, expectedValue) == expectedValue;

        public void ResetSequences()
        {
            ResetSequence();
            ResetStaleSequence();
        }

        public void ResetSequence()
        {
            Sequence = 0;
        }

        public void ResetStaleSequence()
        {
            StaleSequence = 0;
        }

        // TODO: it is unclear why we need this method
        // only place where the mutex is used and it is used only
        // once and it is unclear what decision is made based on
        // the returned value
        public bool TrySetGuid(Guid guid)
        {
            if (Monitor.TryEnter(_guidMutex))
            {
                try
                {
                    Guid = guid;
                    return true;
                }
                finally
                {
                    Monitor.Exit(_guidMutex);
                }
            }
            return false;
        }

        public void UpdateStaleSequence()
        {
            long sequence;
            long staleSequence;
            do
            {
                sequence = Interlocked.Read(ref _sequence);
                staleSequence = Interlocked.Read(ref _staleSequence);

                if (staleSequence >= sequence) break;

            } while (!UpdateStaleSequence(staleSequence, sequence));
        }
    }
}
