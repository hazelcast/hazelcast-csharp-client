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

    internal class NearCacheManager
    {
        private const int ReconciliationIntervalSecondsDefault = 60;
        private const int MinReconciliationIntervalSecondsDefault = 30;
        private const int MaxToleratedMissCountDefault = 10;
        private const int AsyncResultWaitTimeoutMillis = 1 * 60 * 1000;

        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<string, NearCacheBase> _caches =
            new ConcurrentDictionary<string, NearCacheBase>();

        private readonly Cluster _cluster;
        private readonly ISerializationService _serializationService;
        private readonly ILoggerFactory _loggerFactory;
        private readonly long _reconciliationIntervalMillis;
        private readonly Task _repairingTask;
        private readonly NearCacheConfigurations _configurations;

        private long _lastAntiEntropyRunMillis;
        private volatile int _running;

        public NearCacheManager(Cluster cluster, ISerializationService serializationService, ILoggerFactory loggerFactory, NearCacheConfigurations configurations)
        {
            _cluster = cluster;
            _serializationService = serializationService;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<NearCacheManager>();
            _configurations = configurations;

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
            var nearCacheConfig = _configurations.GetConfig(mapName);
            return nearCacheConfig == null
                ? null
                : _caches.GetOrAdd(mapName, newMapName =>
                {
                    var nearCache = new NearCache(newMapName, _cluster, _serializationService, _loggerFactory, nearCacheConfig);
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

        internal static int GetMaxToleratedMissCount()
        {
            var value = HazelcastEnvironment.NearCache.MaxToleratedMissCount ?? MaxToleratedMissCountDefault;
            if (value < 0)
                throw new Exception($"Environment variable {HazelcastEnvironment.NearCache.MaxToleratedMissCountName} cannot be < 0."); // FIXME onfiguration exception
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
                    var responseMessage = _cluster.SendToMemberAsync(request, member.Id, AsyncResultWaitTimeoutMillis).Result; // FIXME ASYNC!
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

        private static int GetReconciliationIntervalSeconds()
        {
            var reconciliationIntervalSeconds = HazelcastEnvironment.NearCache.ReconciliationIntervalSeconds ?? ReconciliationIntervalSecondsDefault;
            if (reconciliationIntervalSeconds < 0)
                throw new Exception($"Environment variable {HazelcastEnvironment.NearCache.ReconciliationIntervalSecondsName} cannot be < 0."); // FIXME onfiguration exception

            var minReconciliationIntervalSeconds = HazelcastEnvironment.NearCache.MinReconciliationIntervalSeconds ?? MinReconciliationIntervalSecondsDefault;
            if (minReconciliationIntervalSeconds < 0)
                throw new Exception($"Environment variable {HazelcastEnvironment.NearCache.MinReconciliationIntervalSecondsName} cannot be < 0."); // FIXME onfiguration exception

            if (reconciliationIntervalSeconds > 0 && reconciliationIntervalSeconds < minReconciliationIntervalSeconds)
                throw new Exception($"When it is > 0, environment variable {HazelcastEnvironment.NearCache.ReconciliationIntervalSecondsName} " +
                                    $"must be >= environment variable {HazelcastEnvironment.NearCache.MinReconciliationIntervalSecondsName}."); // FIXME onfiguration exception

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
