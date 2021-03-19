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

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Configuration;
using Hazelcast.Core;
using Hazelcast.Models;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.NearCaching
{
    // NOTES
    // each 'client' has a NearCache manager
    // this is 'above' the cluster, not part of it, but it uses the cluster
    // = can have a ref to the cluster?

    internal class NearCacheManager : IAsyncEnumerable<NearCache>, IAsyncDisposable
    {
        private readonly ILogger _logger;

        private readonly ConcurrentAsyncDictionary<string, NearCache> _caches = new ConcurrentAsyncDictionary<string, NearCache>();

        private readonly Cluster _cluster;
        private readonly SerializationService _serializationService;
        private readonly ILoggerFactory _loggerFactory;
        private readonly long _reconciliationIntervalMillis;
        private readonly CommonNearCacheOptions _options;

        private Task _repairing;
        private CancellationTokenSource _repairingCancellation;

        private long _lastAntiEntropyRunMillis;
        private volatile int _running;

        public NearCacheManager(Cluster cluster, SerializationService serializationService, ILoggerFactory loggerFactory, CommonNearCacheOptions options)
        {
            _cluster = cluster;
            _serializationService = serializationService;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<NearCacheManager>();
            _options = options;

            _reconciliationIntervalMillis = GetReconciliationIntervalSeconds() * 1000;
        }

        public async ValueTask<NearCache<TValue>> GetOrCreateNearCacheAsync<TValue>(string name, NearCacheOptions options, CancellationToken cancellationToken = default)
            => new NearCache<TValue>(await GetOrCreateNearCacheAsync(name, options, cancellationToken).CfAwait());

        public async ValueTask<NearCache> GetOrCreateNearCacheAsync(string name, NearCacheOptions options, CancellationToken cancellationToken = default)
        {
            return await _caches.GetOrAddAsync(name, async (n, token) =>
            {
                var nearCache = new NearCache(n, _cluster, _serializationService, _loggerFactory, options, GetMaxToleratedMissCount());
                await InitializeNearCache(nearCache).CfAwait();
                return nearCache;
            }, cancellationToken).CfAwait();
        }

        private int GetMaxToleratedMissCount()
        {
            var value = _options.MaxToleratedMissCount;
            if (value < 0)
                throw new ConfigurationException("Option 'MaxToleratedMissCount' cannot be < 0.");
            return value;
        }

        /// <summary>
        /// Fetches and processes meta data for all managed caches.
        /// </summary>
        private async ValueTask FetchMetadataAsync()
        {
            var names = new List<string>();

            await foreach (var (_, value) in _caches)
                names.Add(value.Name);

            if (names.Count == 0)
                return;

            await foreach (var (member, metadata) in FetchMetadataAsync(names))
            {
                try
                {
                    await RepairGuids(metadata.PartitionUuidList).CfAwait();
                    await RepairSequences(metadata.NamePartitionSequenceList).CfAwait();
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, $"An exception was thrown while processing invalidation meta data from address {member.Address}.");
                }
            }
        }

        /// <summary>
        /// Fetches metadata for specified caches.
        /// </summary>
        /// <param name="names">The names of the caches.</param>
        private async IAsyncEnumerable<(MemberInfo, MapFetchNearCacheInvalidationMetadataCodec.ResponseParameters)> FetchMetadataAsync(ICollection<string> names)
        {
            foreach (var member in _cluster.Members.GetMembers(true))
            {
                var requestMessage = MapFetchNearCacheInvalidationMetadataCodec.EncodeRequest(names, member.Id);
                var responseMessage = await _cluster.Messaging.SendToMemberAsync(requestMessage, member.Id).CfAwait();
                var response = MapFetchNearCacheInvalidationMetadataCodec.DecodeResponse(responseMessage);

                yield return (member, response);
            }
        }

        // Marks relevant data as stale if missed invalidation event count is above the max tolerated miss count.
        private async ValueTask FixSequenceGaps()
        {
            await foreach (var (_, cache) in _caches)
            {
                cache.RepairingHandler?.FixSequenceGap();
            }
        }

        private int GetReconciliationIntervalSeconds()
        {
            var reconciliationIntervalSeconds =_options.ReconciliationIntervalSeconds;
            if (reconciliationIntervalSeconds < 0)
                throw new ConfigurationException("Option 'ReconciliationIntervalSeconds' cannot be < 0.");

            var minReconciliationIntervalSeconds = _options.MinReconciliationIntervalSeconds;
            if (minReconciliationIntervalSeconds < 0)
                throw new ConfigurationException("Option 'MinReconciliationIntervalSeconds' cannot be < 0.");

            if (reconciliationIntervalSeconds > 0 && reconciliationIntervalSeconds < minReconciliationIntervalSeconds)
                throw new ConfigurationException("When it is > 0, option 'ReconciliationIntervalSeconds' " +
                                    "must be >= option 'MinReconciliationIntervalSeconds'.");

            return reconciliationIntervalSeconds;
        }

        private async ValueTask InitializeNearCache(NearCache nearCache)
        {
            await nearCache.InitializeAsync().CfAwait();

            if (!nearCache.IsInvalidating)
                return;

            var repairingHandler = nearCache.RepairingHandler;
            if (repairingHandler == null) return; // though that should never happen

            // initialize
            await foreach (var (member, metadata) in FetchMetadataAsync(new[] { nearCache.Name }))
            {
                try
                {
                    repairingHandler.InitializeGuids(metadata.PartitionUuidList);
                    repairingHandler.InitializeSequences(metadata.NamePartitionSequenceList);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, $"An exception was thrown while processing invalidation meta data from address {member.Address}.");
                }
            }

            // start repairing task if not started
            if (Interlocked.CompareExchange(ref _running, 1, 0) == 0)
            {
                _repairingCancellation = new CancellationTokenSource();
                _repairing = Repair(_repairingCancellation.Token).AsTask();

                Interlocked.Exchange(ref _lastAntiEntropyRunMillis, Clock.Milliseconds);
            }
        }

        // body of the background repairing task
        // runs until cancelled - and then, it may simply stop, or throw
        private async ValueTask Repair(CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                try
                {
                    await FixSequenceGaps().CfAwait();
                    await RunAntiEntropyIfNeededAsync().CfAwait();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Caught an exception while repairing NearCache.");
                }

                if (cancellationToken.IsCancellationRequested)
                    return;

                // TODO: this should be a constant
                await Task.Delay(1000, cancellationToken).CfAwait();
            }
        }

        private async ValueTask RepairGuids(IList<KeyValuePair<int, Guid>> guids)
        {
            foreach (var (partitionId, newUuid) in guids)
            {
                await foreach (var (_, cache) in _caches)
                {
                    cache?.RepairingHandler?.UpdateUuid(partitionId, newUuid);
                }
            }
        }

        private async ValueTask RepairSequences(IList<KeyValuePair<string, IList<KeyValuePair<int, long>>>> namePartitionSequenceList)
        {
            foreach (var (name, newSequences) in namePartitionSequenceList)
            {
                foreach (var (partitionId, newSequence) in newSequences)
                {
                    var (hasCache, cache) = await _caches.TryGetAsync(name).CfAwait();
                    if (!hasCache) continue;

                    cache.RepairingHandler?.UpdateSequence(partitionId, newSequence, true);
                }
            }
        }

        // Periodically sends generic operations to cluster members to get latest invalidation metadata.
        private async ValueTask RunAntiEntropyIfNeededAsync()
        {
            if (_reconciliationIntervalMillis == 0)
                return;

            // if this thread-safe?
            var lastAntiEntropyRunMillis = Interlocked.Read(ref _lastAntiEntropyRunMillis);
            var sinceLastRun = Clock.Milliseconds - lastAntiEntropyRunMillis;
            if (sinceLastRun >= _reconciliationIntervalMillis)
            {
                await FetchMetadataAsync().CfAwait();
                Interlocked.Exchange(ref _lastAntiEntropyRunMillis, Clock.Milliseconds);
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerator<NearCache> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
        {
            await foreach (var entry in _caches) yield return entry.Value;
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _running, 0, 1) == 0)
                return;

            if (_repairingCancellation != null)
            {
                _repairingCancellation.Cancel();
                try
                {
                    await _repairing.CfAwait();
                }
                catch (OperationCanceledException) { /* expected */ }

                _repairingCancellation.Dispose();
            }

            await foreach (var (name, cache) in _caches)
            {
                _caches.TryRemove(name); // ok with concurrent dictionary
                await cache.DisposeAsync().CfAwait();
            }
        }
    }
}
