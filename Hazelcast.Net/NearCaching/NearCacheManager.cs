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
using Hazelcast.Exceptions;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.NearCaching
{
    // NOTES
    // each 'client' has a NearCache manager
    // this is 'above' the cluster, not part of it, but it uses the cluster
    // = can have a ref to the cluster?

    internal class NearCacheManager
    {
        //private const int ReconciliationIntervalSecondsDefault = 60;
        //private const int MinReconciliationIntervalSecondsDefault = 30;
        //private const int MaxToleratedMissCountDefault = 10;
        //private const int AsyncResultWaitTimeoutMillis = 1 * 60 * 1000;

        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<string, NearCacheBase> _caches =
            new ConcurrentDictionary<string, NearCacheBase>();

        private readonly Cluster _cluster;
        private readonly ISerializationService _serializationService;
        private readonly ILoggerFactory _loggerFactory;
        private readonly long _reconciliationIntervalMillis;
        private readonly Task _repairingTask;
        private readonly NearCacheOptions _options;

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
            _repairingTask = new Task(Repair, TaskCreationOptions.LongRunning);
        }

        public void DestroyNearCache(string name)
        {
            if (_caches.TryRemove(name, out var nearCache))
            {
                nearCache.Destroy();
            }
        }

        public ICollection<NearCacheBase> GetAllNearCaches()
        {
            return _caches.Values;
        }

        public NearCacheBase GetOrCreateNearCache(string mapName)
        {
            var nearCacheConfig = _options.GetConfig(mapName);
            return nearCacheConfig == null
                ? null
                : _caches.GetOrAdd(mapName, newMapName =>
                {
                    var nearCache = new NearCache(newMapName, _cluster, _serializationService, _loggerFactory, nearCacheConfig, GetMaxToleratedMissCount());
                    InitNearCache(nearCache);
                    return nearCache;
                });
        }

        public void Shutdown()
        {
            if (Interlocked.CompareExchange(ref _running, 0, 1) == 1)
            {
                _repairingTask.Wait(TimeSpan.FromSeconds(120));
            }
            DestroyAllNearCache();
        }

        private int GetMaxToleratedMissCount()
        {
            var value = _options.MaxToleratedMissCount;
            if (value < 0)
                throw new ConfigurationException($"Option 'MaxToleratedMissCount' cannot be < 0.");
            return value;
        }

        private void DestroyAllNearCache()
        {
            foreach (var entry in _caches)
            {
                DestroyNearCache(entry.Key);
            }
        }


        private void FetchMetadata()
        {
            var names = _caches.Values.OfType<NearCache>().Select(cache => cache.Name).ToList();
            if (names.Count == 0)
            {
                return;
            }

            FetchMetadataInternal(names, responseParameter =>
            {
                RepairGuids(responseParameter.PartitionUuidList);
                RepairSequences(responseParameter.NamePartitionSequenceList);
            });
        }

        private void FetchMetadataInternal(IList<string> names, Action<MapFetchNearCacheInvalidationMetadataCodec.ResponseParameters> process)
        {
            var dataMembers = _cluster.LiteMembers;
            foreach (var member in dataMembers)
            {
                var request = MapFetchNearCacheInvalidationMetadataCodec.EncodeRequest(names, member.Id);
                try
                {
                    var responseMessage = _cluster.SendToMemberAsync(request, member.Id, /*AsyncResultWaitTimeoutMillis, */ CancellationToken.None).Result; // FIXME ASYNC oops!
                    var responseParameter = MapFetchNearCacheInvalidationMetadataCodec.DecodeResponse(responseMessage);
                    process(responseParameter);
                    /*
                    var future = _cluster.InvocationService.InvokeOnTarget(request, member.Uuid);
                    var task = future.ToTask();

                    task.ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            // ReSharper disable once PossibleNullReferenceException
                            throw t.Exception.Flatten().InnerExceptions.First();
                        }
                        var responseMessage = ThreadUtil.GetResult(t, AsyncResultWaitTimeoutMillis);
                        var responseParameter =
                            MapFetchNearCacheInvalidationMetadataCodec.DecodeResponse(responseMessage);
                        process(responseParameter);
                    }).IgnoreExceptions();
                    */
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, $"Cant fetch invalidation meta-data from address:{member.Address}.");
                }
            }
        }

        // Marks relevant data as stale if missed invalidation event count is above the max tolerated miss count.
        private void FixSequenceGaps()
        {
            foreach (var baseNearCach in _caches.Values)
            {
                var nc = baseNearCach as NearCache;
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

        private void InitNearCache(NearCacheBase baseNearCache)
        {
            try
            {
                baseNearCache.Init();
                var nearCache = baseNearCache as NearCache;

                var repairingHandler = nearCache?.RepairingHandler;
                if (repairingHandler == null) return;

                var names = new List<string> { nearCache.Name };
                FetchMetadataInternal(names, responseParameter =>
                {
                    repairingHandler.InitGuids(responseParameter.PartitionUuidList);
                    repairingHandler.InitSequences(responseParameter.NamePartitionSequenceList);
                });

                //start repairing task if not started
                if (Interlocked.CompareExchange(ref _running, 1, 0) == 0)
                {
                    _repairingTask.Start();
                    Interlocked.Exchange(ref _lastAntiEntropyRunMillis, Clock.Milliseconds);
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed to initialize.");
            }
        }

        //Repairing task method
        private void Repair()
        {
            while (_running == 1)
            {
                try
                {
                    FixSequenceGaps();
                    RunAntiEntropyIfNeeded();
                }
                catch (Exception e)
                {
                    _logger.LogDebug(e, "Failed to repair.");
                }
                finally
                {
                    // FIXME no!
                    Thread.Sleep(1000);
                }
            }
        }

        private void RepairGuids(IList<KeyValuePair<int, Guid>> guids)
        {
            foreach (var pair in guids)
            {
                foreach (var cache in _caches.Values)
                {
                    var nc = cache as NearCache;
                    var ncRepairingHandler = nc?.RepairingHandler;
                    ncRepairingHandler?.CheckOrRepairGuid(pair.Key, pair.Value);
                }
            }
        }

        private void RepairSequences(
            IList<KeyValuePair<string, IList<KeyValuePair<int, long>>>> namePartitionSequenceList)
        {
            foreach (var pair in namePartitionSequenceList)
            {
                foreach (var subPair in pair.Value)
                {
                    if (_caches.TryGetValue(pair.Key, out var cache))
                    {
                        var nc = cache as NearCache;
                        var ncRepairingHandler = nc?.RepairingHandler;
                        ncRepairingHandler?.CheckOrRepairSequence(subPair.Key, subPair.Value, true);
                    }
                }
            }
        }

        // Periodically sends generic operations to cluster members to get latest invalidation metadata.
        private void RunAntiEntropyIfNeeded()
        {
            if (_reconciliationIntervalMillis == 0)
            {
                return;
            }

            // if this thread-safe?
            var lastAntiEntropyRunMillis = Interlocked.Read(ref _lastAntiEntropyRunMillis);
            var sinceLastRun = Clock.Milliseconds - lastAntiEntropyRunMillis;
            if (sinceLastRun >= _reconciliationIntervalMillis)
            {
                FetchMetadata();
                Interlocked.Exchange(ref _lastAntiEntropyRunMillis, Clock.Milliseconds);
            }
        }
    }
}
