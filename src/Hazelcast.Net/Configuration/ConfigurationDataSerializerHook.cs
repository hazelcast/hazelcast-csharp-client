// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Models;
using Hazelcast.NearCaching;
using Hazelcast.Serialization;

namespace Hazelcast.Configuration;

internal class ConfigurationDataSerializerHook : IDataSerializerHook
{
    public const int WanReplicationConfig = 0;
    public const int WanCustomPublisherConfig = 1;
    public const int WanBatchPublisherConfig = 2;
    public const int WanConsumerConfig = 3;
    public const int NearCacheConfig = 4;
    public const int NearCachePreloaderConfig = 5;
    public const int AddDynamicConfigOp = 6;
    public const int DynamicConfigPreJoinOp = 7;
    public const int MultimapConfig = 8;
    public const int ListenerConfig = 9;
    public const int EntryListenerConfig = 10;
    public const int MapConfig = 11;
    public const int RandomEvictionPolicy = 12;
    public const int LfuEvictionPolicy = 13;
    public const int LruEvictionPolicy = 14;
    public const int MapStoreConfig = 15;
    public const int MapPartitionLostListenerConfig = 16;
    public const int IndexConfig = 17;
    public const int MapAttributeConfig = 18;
    public const int QueryCacheConfig = 19;
    public const int PredicateConfig = 20;
    public const int PartitionStrategyConfig = 21;
    public const int HotRestartConfig = 22;
    public const int TopicConfig = 23;
    public const int ReliableTopicConfig = 24;
    public const int ItemListenerConfig = 25;
    public const int QueueStoreConfig = 26;
    public const int QueueConfig = 27;
    public const int ListConfig = 28;
    public const int SetConfig = 29;
    public const int ExecutorConfig = 30;
    public const int DurableExecutorConfig = 31;
    public const int ScheduledExecutorConfig = 32;
    public const int ReplicatedMapConfig = 33;
    public const int RingbufferConfig = 34;
    public const int RingbufferStoreConfig = 35;
    public const int CardinalityEstimatorConfig = 36;
    public const int SimpleCacheConfig = 37;
    public const int SimpleCacheConfigExpiryPolicyFactoryConfig = 38;
    public const int SimpleCacheConfigTimedExpiryPolicyFactoryConfig = 39;
    public const int SimpleCacheConfigDurationConfig = 40;
    public const int SplitBrainProtectionConfig = 41;
    public const int MapListenerToEntryListenerAdapter = 42;
    public const int EventJournalConfig = 43;
    public const int SplitBrainProtectionListenerConfig = 44;
    public const int CachePartitionLostListenerConfig = 45;
    public const int SimpleCacheEntryListenerConfig = 46;
    public const int FlakeIdGeneratorConfig = 47;
    public const int MergePolicyConfig = 48;
    public const int PnCounterConfig = 49;
    public const int MerkleTreeConfig = 50;
    public const int WanSyncConfig = 51;
    public const int KubernetesConfig = 52;
    public const int EurekaConfig = 53;
    public const int GcpConfig = 54;
    public const int AzureConfig = 55;
    public const int AwsConfig = 56;
    public const int DiscoveryConfig = 57;
    public const int DiscoveryStrategyConfig = 58;
    public const int WanReplicationRef = 59;
    public const int EvictionConfig = 60;
    public const int PermissionConfig = 61;
    public const int BitmapIndexOptions = 62;
    public const int DataPersistenceConfig = 63;
    public const int TieredStoreConfig = 64;
    public const int MemoryTierConfig = 65;
    public const int DiskTierConfig = 66;
    public const int BtreeIndexConfig = 67;
    public const int DataConnectionConfig = 68;
    public const int PartitionAttributeConfig = 69;

    private const int Len = PartitionAttributeConfig + 1;

    // for reasons (?) Java can read this from system properties. we don't.
    public const int FactoryIdConst = -31;

    public int FactoryId => FactoryIdConst;

    public IDataSerializableFactory CreateFactory()
    {
        var constructors = new Func<IIdentifiedDataSerializable>[Len];

        //constructors[WanReplicationConfig] = () => new WanReplicationConfig();
        //constructors[WanConsumerConfig] = () => new WanConsumerConfig();
        //constructors[WanCustomPublisherConfig] = () => new WanCustomPublisherConfig();
        //constructors[WanBatchPublisherConfig] = () => new WanBatchPublisherConfig();
        constructors[NearCacheConfig] = () => new NearCacheOptions();
        constructors[NearCachePreloaderConfig] = () => new NearCachePreloaderOptions();
        //constructors[AddDynamicConfigOp] = () => new AddDynamicConfigOperation();
        //constructors[DynamicConfigPreJoinOp] = () => new DynamicConfigPreJoinOperation();
        //constructors[MultimapConfig] = () => new MultiMapConfig();
        constructors[ListenerConfig] = () => new ListenerOptions();
        constructors[EntryListenerConfig] = () => new EntryListenerOptions();
        constructors[MapConfig] = () => new MapOptions();
        constructors[MapStoreConfig] = () => new MapStoreOptions();
        constructors[MapPartitionLostListenerConfig] = () => new MapPartitionLostListenerOptions();
        constructors[IndexConfig] = () => new IndexOptions();
        constructors[MapAttributeConfig] = () => new AttributeOptions();
        constructors[QueryCacheConfig] = () => new QueryCacheOptions();
        constructors[PredicateConfig] = () => new PredicateOptions();
        constructors[PartitionStrategyConfig] = () => new PartitioningStrategyOptions();
        constructors[HotRestartConfig] = () => new HotRestartOptions();
        //constructors[TopicConfig] = () => new TopicConfig();
        //constructors[ReliableTopicConfig] = () => new ReliableTopicConfig();
        //constructors[ItemListenerConfig] = () => new ItemListenerConfig();
        //constructors[QueueStoreConfig] = () => new QueueStoreConfig();
        //constructors[QueueConfig] = () => new QueueConfig();
        //constructors[ListConfig] = () => new ListConfig();
        //constructors[SetConfig] = () => new SetConfig();
        //constructors[ExecutorConfig] = () => new ExecutorConfig();
        //constructors[DurableExecutorConfig] = () => new DurableExecutorConfig();
        //constructors[ScheduledExecutorConfig] = () => new ScheduledExecutorConfig();
        //constructors[ReplicatedMapConfig] = () => new ReplicatedMapConfig();
        //constructors[RingbufferConfig] = () => new RingbufferConfig();
        //constructors[RingbufferStoreConfig] = () => new RingbufferStoreConfig();
        //constructors[CardinalityEstimatorConfig] = () => new CardinalityEstimatorConfig();
        //constructors[SimpleCacheConfig] = () => new CacheSimpleConfig();
        //constructors[SimpleCacheConfigExpiryPolicyFactoryConfig] = () => new CacheSimpleConfig.ExpiryPolicyFactoryConfig();
        //constructors[SimpleCacheConfigTimedExpiryPolicyFactoryConfig] = () => new CacheSimpleConfig.ExpiryPolicyFactoryConfig.TimedExpiryPolicyFactoryConfig();
        //constructors[SimpleCacheConfigDurationConfig] = () => new CacheSimpleConfig.ExpiryPolicyFactoryConfig.DurationConfig();
        //constructors[SplitBrainProtectionConfig] = () => new SplitBrainProtectionConfig();
        constructors[EventJournalConfig] = () => new EventJournalOptions();
        //constructors[SplitBrainProtectionListenerConfig] = () => new SplitBrainProtectionListenerConfig();
        //constructors[CachePartitionLostListenerConfig] = () => new CachePartitionLostListenerConfig();
        constructors[SimpleCacheEntryListenerConfig] = () => new CacheSimpleEntryListenerOptions();
        //constructors[FlakeIdGeneratorConfig] = () => new FlakeIdGeneratorConfig();
        constructors[MergePolicyConfig] = () => new MergePolicyOptions();
        //constructors[PnCounterConfig] = () => new PNCounterConfig();
        constructors[MerkleTreeConfig] = () => new MerkleTreeOptions();
        //constructors[WanSyncConfig] = () => new WanSyncConfig();
        //constructors[KubernetesConfig] = () => new KubernetesConfig();
        //constructors[EurekaConfig] = () => new EurekaConfig();
        //constructors[GcpConfig] = () => new GcpConfig();
        //constructors[AzureConfig] = () => new AzureConfig();
        //constructors[AwsConfig] = () => new AwsConfig();
        //constructors[DiscoveryConfig] = () => new DiscoveryConfig();
        //constructors[DiscoveryStrategyConfig] = () => new DiscoveryStrategyConfig();
        constructors[WanReplicationRef] = () => new WanReplicationRef();
        constructors[EvictionConfig] = () => new EvictionOptions();
        //constructors[PermissionConfig] = () => new PermissionConfig();
        constructors[BitmapIndexOptions] = () => new BitmapIndexOptions();
        constructors[DataPersistenceConfig] = () => new DataPersistenceOptions();
        constructors[TieredStoreConfig] = () => new TieredStoreOptions();
        constructors[MemoryTierConfig] = () => new MemoryTierOptions();
        constructors[DiskTierConfig] = () => new DiskTierOptions();
        constructors[BtreeIndexConfig] = () => new BTreeIndexOptions();
        //constructors[DataConnectionConfig] = () => new DataConnectionConfig();
        constructors[PartitionAttributeConfig] = () => new PartitioningAttributeOptions();

        return new ArrayDataSerializableFactory(constructors);
    }
}
