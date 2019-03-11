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

using System;
using System.Threading;
using Hazelcast.Net.Ext;

namespace Hazelcast.NearCache
{
    internal class MetaDataContainer
    {
        private readonly object _guidMutex = new object();

        // Number of missed sequence count
        private readonly AtomicLong _missedSequenceCount = new AtomicLong();

        // Sequence number of last received invalidation event
        private readonly AtomicLong _sequence = new AtomicLong();

        // Holds the biggest sequence number that is lost, lower sequences from this sequence are accepted as stale
        private readonly AtomicLong _staleSequence = new AtomicLong();

        // UUID of the source partition that generates invalidation events
        private Guid _guid;

        public MetaDataContainer()
        {
        }

        public Guid Guid
        {
            get { return _guid; }
            set { _guid = value; }
        }

        public long Sequence
        {
            get { return _sequence.Get(); }
            set { _sequence.Set(value); }
        }

        public long StaleSequence
        {
            get { return _staleSequence.Get(); }
            private set { _staleSequence.Set(value); }
        }

        public long MissedSequenceCount
        {
            get { return _missedSequenceCount.Get(); }
        }

        public long AddAndGetMissedSequenceCount(long missCount)
        {
            return _missedSequenceCount.AddAndGet(missCount);
        }

        public bool CompareAndExcangeSquence(long currentSequence, long nextSequence)
        {
            return _sequence.CompareAndSet(currentSequence, nextSequence);
        }

        private bool CompareAndExcangeStaleSequence(long lastKnownStaleSequence, long lastReceivedSequence)
        {
            return _staleSequence.CompareAndSet(lastKnownStaleSequence, lastReceivedSequence);
        }

        public void ResetAllSequences()
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

        public bool TrySetGuid(Guid guid)
        {
            if (Monitor.TryEnter(_guidMutex))
            {
                try
                {
                    _guid = guid;
                    return true;
                }
                finally
                {
                    Monitor.Exit(_guidMutex);
                }
            }
            return false;
        }

        public void UpdateLastKnownStaleSequence()
        {
            long lastReceivedSequence;
            long lastKnownStaleSequence;
            do
            {
                lastReceivedSequence = _sequence.Get();
                lastKnownStaleSequence = _staleSequence.Get();

                if (lastKnownStaleSequence >= lastReceivedSequence)
                {
                    break;
                }
            } while (!CompareAndExcangeStaleSequence(lastKnownStaleSequence, lastReceivedSequence));
        }
    }
}