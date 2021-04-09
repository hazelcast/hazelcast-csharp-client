// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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

using System.Collections.Generic;
using System.Threading;
using Hazelcast.Core;
using Hazelcast.Metrics;

namespace Hazelcast.NearCaching
{
    internal class NearCacheStatistics : IMetricSource
    {
        private readonly MetricDescriptors _metricDescriptors;

        private long _evictions;
        private long _expirations;
        private long _hits;
        private long _misses;
        private long _staleReads;
        private long _ownedEntryCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="NearCacheStatistics"/> class.
        /// </summary>
        /// <param name="name">The name of the cache.</param>
        public NearCacheStatistics(string name)
        {
            CreationTime = Clock.Milliseconds;
            _metricDescriptors = new MetricDescriptors(name);
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
        /// Gets the number of stale reads.
        /// </summary>
        public long StaleReads => Interlocked.Read(ref _staleReads);

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
        /// Notifies of an added entry.
        /// </summary>
        public void NotifyEntryRemoved()
        {
            Interlocked.Decrement(ref _ownedEntryCount);
        }

        /// <summary>
        /// Notifies of a removed entry.
        /// </summary>
        public void NotifyEntryAdded()
        {
            Interlocked.Increment(ref _ownedEntryCount);
        }

        /// <summary>
        /// Resets the number of entries.
        /// </summary>
        public void ResetEntryCount()
        {
            Interlocked.Exchange(ref _ownedEntryCount, 0);
        }

        /// <summary>
        /// Notifies of an eviction.
        /// </summary>
        public void NotifyEviction()
        {
            Interlocked.Increment(ref _evictions);
        }

        /// <summary>
        /// Notifies of an expiration.
        /// </summary>
        public void NotifyExpiration()
        {
            Interlocked.Increment(ref _expirations);
        }

        /// <summary>
        /// Notifies of a hit.
        /// </summary>
        public void NotifyHit()
        {
            Interlocked.Increment(ref _hits);
        }

        /// <summary>
        /// Notifies of a miss.
        /// </summary>
        public void NotifyMiss()
        {
            Interlocked.Increment(ref _misses);
        }

        /// <summary>
        /// Notifies of a stale entry.
        /// </summary>
        public void NotifyStaleRead()
        {
            Interlocked.Increment(ref _staleReads);
        }


        private class MetricDescriptors
        {
            public MetricDescriptors(string name)
            {
                var prefix = "nc" + EnumerableOfMetricsExtensions.NameSeparator + name.TrimStart('/');

                CreationTime = new MetricDescriptor<long>(prefix, "creationTime");
                Evictions = new MetricDescriptor<long>(prefix, "evictions");
                Hits = new MetricDescriptor<long>(prefix, "hits");
                Misses = new MetricDescriptor<long>(prefix, "misses");
                EntryCount = new MetricDescriptor<long>(prefix, "ownedEntryCount");
                Expirations = new MetricDescriptor<long>(prefix, "expirations");
                Invalidations = new MetricDescriptor<long>(prefix, "invalidations");
                InvalidationRequests = new MetricDescriptor<long>(prefix, "invalidationRequests");
                OwnedEntryMemoryCost = new MetricDescriptor<long>(prefix, "ownedEntryMemoryCost");

                LastPersistenceDuration = new MetricDescriptor<long>(prefix, "lastPersistenceDuration");
                LastPersistenceKeyCount = new MetricDescriptor<long>(prefix, "lastPersistenceKeyCount");
                LastPersistenceTime = new MetricDescriptor<long>(prefix, "lastPersistenceTime");
                LastPersistenceWrittenBytes = new MetricDescriptor<long>(prefix, "lastPersistenceWrittenBytes");
            }

            public readonly MetricDescriptor<long> CreationTime;
            public readonly MetricDescriptor<long> Evictions;
            public readonly MetricDescriptor<long> Hits;
            public readonly MetricDescriptor<long> Misses;
            public readonly MetricDescriptor<long> EntryCount;
            public readonly MetricDescriptor<long> Expirations;
            public readonly MetricDescriptor<long> Invalidations;
            public readonly MetricDescriptor<long> InvalidationRequests;
            public readonly MetricDescriptor<long> OwnedEntryMemoryCost;

            public readonly MetricDescriptor<long> LastPersistenceDuration;
            public readonly MetricDescriptor<long> LastPersistenceKeyCount;
            public readonly MetricDescriptor<long> LastPersistenceTime;
            public readonly MetricDescriptor<long> LastPersistenceWrittenBytes;
        }

        public IEnumerable<Metric> PublishMetrics()
        {
            // these are the stats currently sent by the Java v4 client

            yield return _metricDescriptors.CreationTime.WithValue(CreationTime);
            yield return _metricDescriptors.Evictions.WithValue(Evictions);
            yield return _metricDescriptors.Hits.WithValue(Hits);
            yield return _metricDescriptors.Misses.WithValue(Misses);
            yield return _metricDescriptors.EntryCount.WithValue(EntryCount);
            yield return _metricDescriptors.Expirations.WithValue(Expirations);

            yield return _metricDescriptors.Invalidations.WithoutValue();
            yield return _metricDescriptors.InvalidationRequests.WithoutValue();
            yield return _metricDescriptors.OwnedEntryMemoryCost.WithoutValue();

            yield return _metricDescriptors.LastPersistenceDuration.WithoutValue();
            yield return _metricDescriptors.LastPersistenceKeyCount.WithoutValue();
            yield return _metricDescriptors.LastPersistenceTime.WithoutValue();
            yield return _metricDescriptors.LastPersistenceWrittenBytes.WithoutValue();

            // TODO: "lastPersistenceFailure") if ... ?
        }
    }
}
