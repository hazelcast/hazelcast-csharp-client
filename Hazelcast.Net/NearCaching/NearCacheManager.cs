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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Configuration;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.NearCaching
{
    // NOTES
    // each 'client' has a NearCache manager
    // this is 'above' the cluster, not part of it, but it uses the cluster
    // = can have a ref to the cluster?

    internal class NearCacheManager : IAsyncDisposable
    {
        private readonly ILogger _logger;

        private readonly ConcurrentAsyncDictionary<string, NearCacheBase> _caches =
            new ConcurrentAsyncDictionary<string, NearCacheBase>();

        private readonly Cluster _cluster;
        private readonly ISerializationService _serializationService;
        private readonly ILoggerFactory _loggerFactory;
        private readonly long _reconciliationIntervalMillis;
        private readonly NearCacheOptions _options;

        private Task _repairing;
        private CancellationTokenSource _repairingCancellation;

        private long _lastAntiEntropyRunMillis;
        private volatile int _running;

        public NearCacheManager(Cluster cluster, ISerializationService serializationService, ILoggerFactory loggerFactory, NearCacheOptions options)
        {
            _cluster = cluster;
            _serializationService = serializationService;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<NearCacheManager>();
            _options = options;

            _reconciliationIntervalMillis = GetReconciliationIntervalSeconds() * 1000;
        }

        public async ValueTask<NearCacheBase> GetOrCreateNearCacheAsync(string mapName, CancellationToken cancellationToken)
        {
            var nearCacheConfig = _options.GetConfig(mapName);
            if (nearCacheConfig == null) return null;

            return await _caches.GetOrAddAsync(mapName, async name => 
            {
                var nearCache = new NearCache(name, _cluster, _serializationService, _loggerFactory, nearCacheConfig, GetMaxToleratedMissCount());
                await InitNearCache(nearCache, cancellationToken).CAF();
                return nearCache;
            }).CAF();
        }

        private int GetMaxToleratedMissCount()
        {
            var value = _options.MaxToleratedMissCount;
            if (value < 0)
                throw new ConfigurationException($"Option 'MaxToleratedMissCount' cannot be < 0.");
            return value;
        }

        private async ValueTask FetchMetadataAsync(CancellationToken cancellationToken)
        {
            var names = new List<string>();
            
            await foreach (var (key, value) in _caches)
                names.Add(value.Name);

            if (names.Count == 0)
                return;

            await FetchMetadataAsyncInternal(names, async responseParameter =>
            {
                await RepairGuids(responseParameter.PartitionUuidList).CAF();
                await RepairSequences(responseParameter.NamePartitionSequenceList).CAF();
            }, cancellationToken).CAF();
        }

        private async ValueTask FetchMetadataAsyncInternal(IList<string> names, Func<MapFetchNearCacheInvalidationMetadataCodec.ResponseParameters, ValueTask> process, CancellationToken cancellationToken)
        {
            var dataMembers = _cluster.LiteMembers;
            foreach (var member in dataMembers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var requestMessage = MapFetchNearCacheInvalidationMetadataCodec.EncodeRequest(names, member.Id);
                    var responseMessage = await _cluster.SendToMemberAsync(requestMessage, member.Id, cancellationToken).CAF();
                    var responseParameter = MapFetchNearCacheInvalidationMetadataCodec.DecodeResponse(responseMessage);

                    await process(responseParameter).CAF();
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, $"Cant fetch invalidation meta-data from address:{member.Address}.");
                }
            }
        }

        // Marks relevant data as stale if missed invalidation event count is above the max tolerated miss count.
        private async ValueTask FixSequenceGaps()
        {
            await foreach (var (_, cache) in _caches)
            {
                var nc = cache as NearCache;
                var ncRepairingHandler = nc?.RepairingHandler;
                ncRepairingHandler?.FixSequenceGap();
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

        private async ValueTask InitNearCache(NearCacheBase baseNearCache, CancellationToken cancellationToken)
        {
            try
            {
                baseNearCache.Init();
                var nearCache = baseNearCache as NearCache;

                var repairingHandler = nearCache?.RepairingHandler;
                if (repairingHandler == null) return;

                var names = new List<string> { nearCache.Name };
                await FetchMetadataAsyncInternal(names, responseParameter =>
                {
                    repairingHandler.InitGuids(responseParameter.PartitionUuidList);
                    repairingHandler.InitSequences(responseParameter.NamePartitionSequenceList);
                    return default;
                }, cancellationToken).CAF();

                //start repairing task if not started
                if (Interlocked.CompareExchange(ref _running, 1, 0) == 0)
                {
                    _repairingCancellation = new CancellationTokenSource();
                    _repairing = Task.Run(() => Repair(_repairingCancellation.Token));

                    Interlocked.Exchange(ref _lastAntiEntropyRunMillis, Clock.Milliseconds);
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed to initialize.");
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
                    await FixSequenceGaps().CAF();
                    await RunAntiEntropyIfNeededAsync(cancellationToken).CAF();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Caught an exception while repairing NearCache.");
                }

                if (cancellationToken.IsCancellationRequested)
                    return;

                // TODO: this should be a constant
                await Task.Delay(1000, cancellationToken).CAF();
            }
        }

        private async ValueTask RepairGuids(IList<KeyValuePair<int, Guid>> guids)
        {
            foreach (var pair in guids)
            {
                await foreach (var (_, cache) in _caches)
                {
                    var nc = cache as NearCache;
                    var ncRepairingHandler = nc?.RepairingHandler;
                    ncRepairingHandler?.CheckOrRepairGuid(pair.Key, pair.Value);
                }
            }
        }

        private async ValueTask RepairSequences(IList<KeyValuePair<string, IList<KeyValuePair<int, long>>>> namePartitionSequenceList)
        {
            foreach (var pair in namePartitionSequenceList)
            {
                foreach (var subPair in pair.Value)
                {
                    var (hasCache, cache) = await _caches.TryGetValue(pair.Key).CAF();
                    if (hasCache)
                    {
                        var nc = cache as NearCache;
                        var ncRepairingHandler = nc?.RepairingHandler;
                        ncRepairingHandler?.CheckOrRepairSequence(subPair.Key, subPair.Value, true);
                    }
                }
            }
        }

        // Periodically sends generic operations to cluster members to get latest invalidation metadata.
        private async ValueTask RunAntiEntropyIfNeededAsync(CancellationToken cancellationToken)
        {
            if (_reconciliationIntervalMillis == 0)
                return;

            // if this thread-safe?
            var lastAntiEntropyRunMillis = Interlocked.Read(ref _lastAntiEntropyRunMillis);
            var sinceLastRun = Clock.Milliseconds - lastAntiEntropyRunMillis;
            if (sinceLastRun >= _reconciliationIntervalMillis)
            {
                await FetchMetadataAsync(cancellationToken).CAF();
                Interlocked.Exchange(ref _lastAntiEntropyRunMillis, Clock.Milliseconds);
            }
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
                    await _repairing.CAF();
                }
                catch (OperationCanceledException) { /* expected */ }

                _repairingCancellation.Dispose();
            }

            await foreach (var (key, value) in _caches)
            {
                _caches.TryRemove(key); // ok with concurrent dictionary
                await value.DestroyAsync().CAF();
            }
        }
    }
}
