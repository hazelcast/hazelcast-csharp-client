// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Models;
using Hazelcast.NearCaching;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Protocol.Models;
using Hazelcast.Testing.Conditions;
using NUnit.Framework;
namespace Hazelcast.Tests.Configuration.Dynamic;

[TestFixture]
public class DynamicConfigureMapTests : DynamicConfigureTestBase
{
    [Test]
    public async Task CanConfigureEverything()
    {
        var options = CreateHazelcastOptions();
        options.ClusterName = RcCluster.Id;
        await using var client = await HazelcastClientFactory.StartNewClientAsync(options).CfAwait();

        await client.DynamicOptions.ConfigureMapAsync("map-name", options =>
        {
            // everything below is just values

            options.Name = "map-name"; // there is no default for that one
            options.SplitBrainProtectionName = MapOptions.Defaults.SplitBrainProtectionName;

            options.BackupCount = MapOptions.Defaults.BackupCount;
            options.AsyncBackupCount = MapOptions.Defaults.AsyncBackupCount;
            options.TimeToLiveSeconds = MapOptions.Defaults.TtlSeconds;
            options.MaxIdleSeconds = MapOptions.Defaults.MaxIdleSeconds;
            options.StatisticsEnabled = MapOptions.Defaults.StatisticsEnabled;
            options.PerEntryStatsEnabled = MapOptions.Defaults.EntryStatsEnabled;
            options.ReadBackupData = MapOptions.Defaults.ReadBackupData;

            options.CacheDeserializedValues = MapOptions.Defaults.CachedDeserializedValues;
            options.InMemoryFormat = MapOptions.Defaults.InMemoryFormat;
            options.MetadataPolicy = MapOptions.Defaults.MetadataPolicy;

            // everything below is pre-existing objects

            options.MapStore.Enabled = MapStoreOptions.Defaults.Enabled;
            options.MapStore.Offload = MapStoreOptions.Defaults.Offload;
            options.MapStore.InitialLoadMode = MapStoreOptions.Defaults.InitialLoadMode;
            options.MapStore.WriteCoalescing = MapStoreOptions.Defaults.WriteCoalescing;
            options.MapStore.ClassName = "className"; // cannot be default 'null' value
            options.MapStore.FactoryClassName = "factoryClassName"; // cannot be default 'null' value

            options.MergePolicy.BatchSize = MergePolicyOptions.Defaults.BatchSize;
            options.MergePolicy.Policy = MergePolicyOptions.Defaults.MergePolicy;

            options.HotRestart.Enabled = HotRestartOptions.Defaults.Enabled;
            options.HotRestart.Fsync = HotRestartOptions.Defaults.Fsync;

            options.DataPersistence.Enabled = DataPersistenceOptions.Defaults.Enabled;
            options.DataPersistence.Fsync = DataPersistenceOptions.Defaults.Fsync;

            //mapOptions.MerkleTree.Enabled = ; // default is unset
            options.MerkleTree.Depth = MerkleTreeOptions.Defaults.Depth;

            options.EventJournal.Enabled = EventJournalOptions.Defaults.Enabled;
            options.EventJournal.Capacity = EventJournalOptions.Defaults.Capacity;
            options.EventJournal.TimeToLiveSeconds = 123;  // cannot be default 'zero' value

            options.Eviction.EvictionPolicy = EvictionOptions.Defaults.EvictionPolicy;
            options.Eviction.MaxSizePolicy = EvictionOptions.Defaults.MaxSizePolicy;
            options.Eviction.Size = EvictionOptions.Defaults.Size;
            options.Eviction.ComparatorClassName = "comparatorClassName"; // cannot be default 'null' value

            options.TieredStore.Enabled = TieredStoreOptions.Defaults.Enabled;
            options.TieredStore.DiskTier.Enabled = DiskTierOptions.Defaults.Enabled;
            options.TieredStore.DiskTier.DeviceName = DiskTierOptions.Defaults.DeviceName;
            options.TieredStore.MemoryTier.Capacity = MemoryTierOptions.Defaults.Capacity;

            // everything below is initially null
            // collections are auto-initialized

            options.PartitioningAttributes.Add(new PartitioningAttributeOptions("name"));

            options.NearCache = new NearCacheOptions
            {
                Name = "name",
                InMemoryFormat = InMemoryFormat.Native,
                CacheLocalEntries = true,
                InvalidateOnChange = true,
                SerializeKeys = true,
                LocalUpdatePolicy = UpdatePolicy.CacheOnUpdate,
                MaxIdleSeconds = 120,
                TimeToLiveSeconds = 120,
                Eviction = // initially not null
                {
                    ComparatorClassName = "comparatorClassName",
                    EvictionPolicy = EvictionPolicy.Lfu,
                    MaxSizePolicy = MaxSizePolicy.EntryCount,
                    Size = 120
                },
                Preloader = // initially not null
                {
                    Enabled = true,
                    Directory = "directory",
                    StoreInitialDelaySeconds = 100,
                    StoreIntervalSeconds = 100
                }
            };

            options.WanReplicationRef = new WanReplicationRef
            {
                Name = "name",
                MergePolicyClassName = "className",
                RepublishingEnabled = true,
                Filters = // initially not null
                {
                    "filter"
                }
            };

            options.EntryListeners.Add(new EntryListenerOptions("className", true, true));

            options.PartitionLostListeners.Add(new MapPartitionLostListenerOptions("className"));

            options.Indexes.Add(new IndexOptions
            {
                Name = "name",
                Type = IndexType.Hashed,
                Attributes = // initially not null
                {
                    "attribute1", 
                    "attribute2"
                },
                BitmapIndex = // initially not null
                {
                    UniqueKey = "uniqueKey", 
                    UniqueKeyTransformation = UniqueKeyTransformation.Raw
                },
                BTreeIndex = // initially not null
                {
                    PageSize = Capacity.Of(120, MemoryUnit.KiloBytes), 
                    MemoryTier = // initially not null
                    {
                        Capacity = Capacity.Of(120, MemoryUnit.KiloBytes)
                    }
                }
            });

            options.Attributes.Add(new AttributeOptions("name", "extractorClassName"));

            options.QueryCaches.Add(new QueryCacheOptions("name")
            {
                Name = "name",
                BatchSize = 120,
                BufferSize = 120,
                DelaySeconds = 120,
                EntryListeners = // initially not null
                {
                    new()
                    {
                        ClassName = "className",
                        IncludeValue = true,
                        Local = true
                    }
                },
                Eviction = // initially not null
                {
                    ComparatorClassName = "comparatorClassName",
                    EvictionPolicy = EvictionPolicy.Lfu,
                    MaxSizePolicy = MaxSizePolicy.EntryCount,
                    Size = 120
                },
                Predicate = // initially not null
                {
                    ClassName = "className",
                    Sql = "SQL"
                },
                Indexes = // initially not null
                {
                    new()
                    {
                        Name = "name", 
                        Attributes = new List<string> {"attribute1", "attribute2"},
                        BitmapIndex = // initially not null
                        {
                            UniqueKey = "uniqueKey",
                            UniqueKeyTransformation = UniqueKeyTransformation.Raw
                        },
                        BTreeIndex = // initially not null
                        {
                            PageSize = Capacity.Of(120, MemoryUnit.KiloBytes),
                            MemoryTier = // initially not null
                            {
                                Capacity = Capacity.Of(120, MemoryUnit.KiloBytes)
                            }
                        },
                        Type = IndexType.Hashed
                    }
                },
                InMemoryFormat = InMemoryFormat.Binary,
                Coalesce = true,
                IncludeValue = true,
            });

            options.PartitioningStrategy = new PartitioningStrategyOptions("partitioningStrategyClass");
        });
    }

    [Test]
    [ServerCondition("5.4")]
    public async Task DefaultOptionsEncodeToSameMessageAsJava()
    {
        const string script = @"

var serializationService = instance_0.serializationService

// alas... it wants a 'client instance'... so we're not going to 'adapt' the
// inner collections because it would be a real pain to re-code it all here
//var ClientDynamicClusterConfig = Java.type(""com.hazelcast.client.impl.clientside.ClientDynamicClusterConfig"")
//var clientDynamicClusterConfig = new ClientDynamicClusterConfig(instance_0.getOriginal())

var DynamicConfigAddMapConfigCodec = Java.type(""com.hazelcast.client.impl.protocol.codec.DynamicConfigAddMapConfigCodec"")
var EvictionConfigHolder = Java.type(""com.hazelcast.client.impl.protocol.task.dynamicconfig.EvictionConfigHolder"")
var MapStoreConfigHolder = Java.type(""com.hazelcast.client.impl.protocol.task.dynamicconfig.MapStoreConfigHolder"")
var NearCacheConfigHolder = Java.type(""com.hazelcast.client.impl.protocol.task.dynamicconfig.NearCacheConfigHolder"")

var MapConfig = Java.type(""com.hazelcast.config.MapConfig"")
var mapConfig = new MapConfig(""map-name"")

var message = DynamicConfigAddMapConfigCodec.encodeRequest(
    mapConfig.getName(),
    mapConfig.getBackupCount(), mapConfig.getAsyncBackupCount(), mapConfig.getTimeToLiveSeconds(),
    mapConfig.getMaxIdleSeconds(), EvictionConfigHolder.of(mapConfig.getEvictionConfig(), serializationService),
    mapConfig.isReadBackupData(), mapConfig.getCacheDeserializedValues().name(),
    mapConfig.getMergePolicyConfig().getPolicy(), mapConfig.getMergePolicyConfig().getBatchSize(),
    mapConfig.getInMemoryFormat().name(), 
    null/*listenerConfigs*/, 
    null/*partitionLostListenerConfigs*/,
    mapConfig.isStatisticsEnabled(), mapConfig.getSplitBrainProtectionName(),
    MapStoreConfigHolder.of(mapConfig.getMapStoreConfig(), serializationService),
    NearCacheConfigHolder.of(mapConfig.getNearCacheConfig(), serializationService),
    mapConfig.getWanReplicationRef(), mapConfig.getIndexConfigs(), mapConfig.getAttributeConfigs(),
    null/*queryCacheConfigHolders*/, null/*partitioningStrategyClassName*/, null/*partitioningStrategy*/, mapConfig.getHotRestartConfig(),
    mapConfig.getEventJournalConfig(), mapConfig.getMerkleTreeConfig(), mapConfig.getMetadataPolicy().getId(),
    mapConfig.isPerEntryStatsEnabled(), mapConfig.getDataPersistenceConfig(), mapConfig.getTieredStoreConfig(),
    mapConfig.getPartitioningAttributeConfigs(),
    null /*namespace*/)

" + ResultIsJavaMessageBytes;

        var javaBytes = await ScriptToBytes(script);

        var options = new MapOptions("map-name");
        var message = DynamicConfigAddMapConfigCodec.EncodeRequest(
            options.Name,
            options.BackupCount, options.AsyncBackupCount,
            options.TimeToLiveSeconds, options.MaxIdleSeconds,
            EvictionConfigHolder.Of(options.Eviction),
            options.ReadBackupData,
            options.CacheDeserializedValues.ToJavaString(),
            options.MergePolicy.Policy, options.MergePolicy.BatchSize,
            options.InMemoryFormat.ToJavaString(),
            null, //MapListenerConfigs(mapOptions.EntryListeners),
            null, //MapListenerConfigs(mapOptions.PartitionLostListeners),
            options.StatisticsEnabled,
            options.SplitBrainProtectionName,
            MapStoreConfigHolder.Of(options.MapStore),
            NearCacheConfigHolder.Of(options.NearCache),
            options.WanReplicationRef,
            options.Indexes,
            options.Attributes,
            null, //MapQueryCacheConfigs(mapOptions.QueryCaches),
            null,/*mapOptions.PartitioningStrategy?.PartitioningStrategyClass*/
            null,
            options.HotRestart, options.EventJournal, options.MerkleTree,
            (int)options.MetadataPolicy, options.PerEntryStatsEnabled,
            options.DataPersistence, options.TieredStore, options.PartitioningAttributes,
            null /*namespace*/);

        var dotnetBytes = MessageToBytes(message);

        AssertMessagesAreIdentical(javaBytes, dotnetBytes);
    }
}