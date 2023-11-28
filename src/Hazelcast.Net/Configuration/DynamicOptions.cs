// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Models;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Protocol.Models;

namespace Hazelcast.Configuration;

/// <summary>
/// Represents a service that can dynamically configure cluster-side objects.
/// </summary>
public class DynamicOptions
{
    private readonly HazelcastClient _client;

    internal DynamicOptions(HazelcastClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Dynamically configures a map.
    /// </summary>
    /// <param name="mapOptions">The map configuration.</param>
    public async Task ConfigureMapAsync(MapOptions mapOptions)
    {
        mapOptions.MergeConfigs();

        var requestMessage = DynamicConfigAddMapConfigCodec.EncodeRequest(
            mapOptions.Name,
            mapOptions.BackupCount, mapOptions.AsyncBackupCount,
            mapOptions.TimeToLiveSeconds, mapOptions.MaxIdleSeconds,
            EvictionConfigHolder.Of(mapOptions.Eviction),
            mapOptions.ReadBackupData,
            mapOptions.CacheDeserializedValues.ToJavaString(),
            mapOptions.MergePolicy.Policy, mapOptions.MergePolicy.BatchSize,
            mapOptions.InMemoryFormat.ToJavaString(),
            MapListenerConfigs(mapOptions.EntryListeners),
            MapListenerConfigs(mapOptions.PartitionLostListeners),
            mapOptions.StatisticsEnabled,
            mapOptions.SplitBrainProtectionName,
            MapStoreConfigHolder.Of(mapOptions.MapStore),
            NearCacheConfigHolder.Of(mapOptions.NearCache),
            mapOptions.WanReplicationRef,
            mapOptions.Indexes,
            mapOptions.Attributes,
            MapQueryCacheConfigs(mapOptions.QueryCaches),
            mapOptions.PartitioningStrategy?.PartitioningStrategyClass,
            null,
            mapOptions.HotRestart, mapOptions.EventJournal, mapOptions.MerkleTree,
            (int)mapOptions.MetadataPolicy, mapOptions.PerEntryStatsEnabled,
            mapOptions.DataPersistence, mapOptions.TieredStore, mapOptions.PartitioningAttributes);
        var responseMessage = await _client.Cluster.Messaging.SendAsync(requestMessage);
        var response = DynamicConfigAddMapConfigCodec.DecodeResponse(responseMessage);
    }

    public async Task ConfigureMapAsync(string name, Action<MapOptions> configure)
    {
        var mapConfig = new MapOptions(name);
        configure(mapConfig);
        await ConfigureMapAsync(mapConfig);
    }

    // FIXME: missing plenty!

    private List<QueryCacheConfigHolder> MapQueryCacheConfigs(List<QueryCacheOptions> queryCacheConfigs)
    {
        if (queryCacheConfigs == null || queryCacheConfigs.Count == 0) return null;
        var mapped = new List<QueryCacheConfigHolder>(queryCacheConfigs.Count);
        foreach (var queryCacheConfig in queryCacheConfigs)
            mapped.Add(QueryCacheConfigHolder.Of(queryCacheConfig, _client.SerializationService));
        return mapped;
    }

    private List<ListenerConfigHolder> MapListenerConfigs<T>(List<T> listenerConfigs)
        where T : ListenerOptions
    {
        if (listenerConfigs == null || listenerConfigs.Count == 0) return null;
        var mapped = new List<ListenerConfigHolder>(listenerConfigs.Count);
        foreach (var listenerConfig in listenerConfigs)
            mapped.Add(ListenerConfigHolder.Of(listenerConfig));
        return mapped;
    }
}