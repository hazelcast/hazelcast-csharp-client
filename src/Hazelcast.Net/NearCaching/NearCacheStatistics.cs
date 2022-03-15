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
using System.Text;
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
            private const string NearCacheAttributePrefix = "nc";
            private const string NearCacheDescriptorPrefix = "nearcache";
            private const string NearCacheDescriptorDiscriminator = "name";
            private readonly string _nearCacheName;

            private MetricDescriptor<T> CreateDescriptor<T>(string metricName, MetricUnit unit)
                => MetricDescriptor.Create<T>(NearCacheDescriptorPrefix, metricName, unit)
                    .WithAttributePrefixes(NearCacheAttributePrefix, _nearCacheName) // override as 'nc.<name>.metric=value' for attributes
                    .WithDiscriminator(NearCacheDescriptorDiscriminator, _nearCacheName);

            public MetricDescriptors(string nearCacheName)
            {
                _nearCacheName = nearCacheName.TrimStart('/');

                CreationTime = CreateDescriptor<long>("creationTime", MetricUnit.Milliseconds);
                Evictions = CreateDescriptor<long>("evictions", MetricUnit.Count);
                Hits = CreateDescriptor<long>("hits", MetricUnit.Count);
                Misses = CreateDescriptor<long>("misses", MetricUnit.Count);
                EntryCount = CreateDescriptor<long>("ownedEntryCount", MetricUnit.Count);
                Expirations = CreateDescriptor<long>("expirations", MetricUnit.Count);
                Invalidations = CreateDescriptor<long>("invalidations", MetricUnit.Count);
                InvalidationRequests = CreateDescriptor<long>("invalidationRequests", MetricUnit.Count);
                OwnedEntryMemoryCost = CreateDescriptor<long>("ownedEntryMemoryCost", MetricUnit.Bytes);

                // these exist in Java but not in Python?
                LastPersistenceDuration = CreateDescriptor<long>("lastPersistenceDuration", MetricUnit.Milliseconds);
                LastPersistenceKeyCount = CreateDescriptor<long>("lastPersistenceKeyCount", MetricUnit.Count);
                LastPersistenceTime = CreateDescriptor<long>("lastPersistenceTime", MetricUnit.Milliseconds);
                LastPersistenceWrittenBytes = CreateDescriptor<long>("lastPersistenceWrittenBytes", MetricUnit.Bytes);
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

            // TODO consider enabling these?
            /*
            yield return _metricDescriptors.LastPersistenceDuration.WithoutValue();
            yield return _metricDescriptors.LastPersistenceKeyCount.WithoutValue();
            yield return _metricDescriptors.LastPersistenceTime.WithoutValue();
            yield return _metricDescriptors.LastPersistenceWrittenBytes.WithoutValue();
            */
        }
    }
}
