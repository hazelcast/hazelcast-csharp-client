// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Client;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Client.Spi;
using Hazelcast.Logging;
using Hazelcast.Net.Ext;
using Hazelcast.Util;

namespace Hazelcast.NearCache
{
    internal class NearCacheManager
    {
        private const string ReconciliationIntervalSecondsProperty =
            "hazelcast.invalidation.reconciliation.interval.seconds";

        private const string MinReconciliationIntervalSecondsProperty =
            "hazelcast.invalidation.min.reconciliation.interval.seconds";

        private const string MaxToleratedMissCountProperty = "hazelcast.invalidation.max.tolerated.miss.count";

        private const int ReconciliationIntervalSecondsDefault = 60;
        private const int MinReconciliationIntervalSecondsDefault = 30;
        private const int MaxToleratedMissCountDefault = 10;
        private const int AsyncResultWaitTimeoutMillis = 1 * 60 * 1000;

        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof(NearCacheManager));

        private readonly ConcurrentDictionary<string, BaseNearCache> _caches =
            new ConcurrentDictionary<string, BaseNearCache>();

        private readonly HazelcastClient _client;
        private readonly AtomicLong _lastAntiEntropyRunMillis = new AtomicLong(0);

        private readonly long _reconciliationIntervalMillis;
        private readonly Task _repairingTask;
        private readonly AtomicBoolean _running = new AtomicBoolean(false);

        public NearCacheManager(HazelcastClient client)
        {
            _client = client;
            _reconciliationIntervalMillis = GetReconciliationIntervalSeconds() * 1000;
            _repairingTask = new Task(Repair, TaskCreationOptions.LongRunning);
        }

        public void DestroyNearCache(string name)
        {
            BaseNearCache nearCache;
            if (_caches.TryRemove(name, out nearCache))
            {
                nearCache.Destroy();
            }
        }

        public ICollection<BaseNearCache> GetAllNearCaches()
        {
            return _caches.Values;
        }

        public BaseNearCache GetOrCreateNearCache(string mapName)
        {
            var nearCacheConfig = _client.GetClientConfig().GetNearCacheConfig(mapName);
            return nearCacheConfig == null
                ? null
                : _caches.GetOrAdd(mapName, newMapName =>
                {
                    BaseNearCache nearCache;
                    if (SupportsRepairableNearCache())
                    {
                        nearCache = new NearCache(newMapName, _client, nearCacheConfig);
                    }
                    else
                    {
                        nearCache = new NearCachePre38(newMapName, _client, nearCacheConfig);
                    }
                    InitNearCache(nearCache);
                    return nearCache;
                });
        }

        public void Shutdown()
        {
            if (_running.CompareAndSet(true, false))
            {
                _repairingTask.Wait(TimeSpan.FromSeconds(120));
            }
            DestroyAllNearCache();
        }

        internal static int GetMaxToleratedMissCount()
        {
            var maxToleratedMissCount =
                EnvironmentUtil.ReadInt(MaxToleratedMissCountProperty) ?? MaxToleratedMissCountDefault;
            return ValidationUtil.CheckNotNegative(maxToleratedMissCount,
                string.Format("max-tolerated-miss-count cannot be < 0 but found {0}", maxToleratedMissCount));
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
                RepairGuids(responseParameter.partitionUuidList);
                RepairSequences(responseParameter.namePartitionSequenceList);
            });
        }

        private void FetchMetadataInternal(IList<string> names,
            Action<MapFetchNearCacheInvalidationMetadataCodec.ResponseParameters> process)
        {
            var dataMembers = _client.GetClientClusterService().GetMemberList().Where(member => !member.IsLiteMember);
            foreach (var member in dataMembers)
            {
                var address = member.GetAddress();
                var request = MapFetchNearCacheInvalidationMetadataCodec.EncodeRequest(names, address);
                try
                {
                    var future = _client.GetInvocationService().InvokeOnTarget(request, address);
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
                }
                catch (Exception e)
                {
                    Logger.Warning(string.Format("Cant fetch invalidation meta-data from address:{0} [{1}]", address,
                        e.Message));
                }
            }
        }

        // Marks relevant data as stale if missed invalidation event count is above the max tolerated miss count.
        private void FixSequenceGaps()
        {
            foreach (var baseNearCach in _caches.Values)
            {
                var nc = baseNearCach as NearCache;
                if (nc != null)
                {
                    var ncRepairingHandler = nc.RepairingHandler;
                    if (ncRepairingHandler != null)
                    {
                        ncRepairingHandler.FixSequenceGap();
                    }
                }
            }
        }

        private static int GetReconciliationIntervalSeconds()
        {
            var reconciliationIntervalSeconds = EnvironmentUtil.ReadInt(ReconciliationIntervalSecondsProperty) ??
                                                ReconciliationIntervalSecondsDefault;
            var minReconciliationIntervalSeconds = EnvironmentUtil.ReadInt(MinReconciliationIntervalSecondsProperty) ??
                                                   MinReconciliationIntervalSecondsDefault;
            if (reconciliationIntervalSeconds < 0 || reconciliationIntervalSeconds > 0 &&
                reconciliationIntervalSeconds < minReconciliationIntervalSeconds)
            {
                var msg = string.Format(
                    "Reconciliation interval can be at least {0} seconds if it is not zero, but {1} was configured." +
                    " Note: Configuring a value of zero seconds disables the reconciliation task.",
                    MinReconciliationIntervalSecondsDefault, reconciliationIntervalSeconds);
                throw new ArgumentException(msg);
            }
            return reconciliationIntervalSeconds;
        }

        private void InitNearCache(BaseNearCache baseNearCache)
        {
            try
            {
                baseNearCache.Init();
                var nearCache = baseNearCache as NearCache;
                if (nearCache == null) return;

                var repairingHandler = nearCache.RepairingHandler;
                if (repairingHandler == null) return;

                var names = new List<string> {nearCache.Name};
                FetchMetadataInternal(names, responseParameter =>
                {
                    repairingHandler.InitGuids(responseParameter.partitionUuidList);
                    repairingHandler.InitSequences(responseParameter.namePartitionSequenceList);
                });

                //start repairing task if not started
                if (_running.CompareAndSet(false, true))
                {
                    _repairingTask.Start();
                    _lastAntiEntropyRunMillis.Set(Clock.CurrentTimeMillis());
                }
            }
            catch (Exception e)
            {
                Logger.Warning(e);
            }
        }

        //Repairing task method
        private void Repair()
        {
            while (_running.Get())
            {
                try
                {
                    FixSequenceGaps();
                    RunAntiEntropyIfNeeded();
                }
                catch (Exception e)
                {
                    if (Logger.IsFinestEnabled())
                    {
                        Logger.Finest("Reparing task failed", e);
                    }
                }
                finally
                {
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
                    if (nc != null)
                    {
                        var ncRepairingHandler = nc.RepairingHandler;
                        if (ncRepairingHandler != null)
                        {
                            ncRepairingHandler.CheckOrRepairGuid(pair.Key, pair.Value);
                        }
                    }
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
                    BaseNearCache cache;
                    if (_caches.TryGetValue(pair.Key, out cache))
                    {
                        var nc = cache as NearCache;
                        if (nc != null)
                        {
                            var ncRepairingHandler = nc.RepairingHandler;
                            if (ncRepairingHandler != null)
                            {
                                ncRepairingHandler.CheckOrRepairSequence(subPair.Key, subPair.Value, true);
                            }
                        }
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
            var sinceLastRun = Clock.CurrentTimeMillis() - _lastAntiEntropyRunMillis.Get();
            if (sinceLastRun >= _reconciliationIntervalMillis)
            {
                FetchMetadata();
                _lastAntiEntropyRunMillis.Set(Clock.CurrentTimeMillis());
            }
        }

        private bool SupportsRepairableNearCache()
        {
            var serverVersion = ((ClientClusterService) _client.GetClientClusterService()).ServerVersion;
            return serverVersion >= VersionUtil.Version38;
        }
    }
}