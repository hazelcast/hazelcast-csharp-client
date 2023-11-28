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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Messaging.FrameFields;
using Hazelcast.Models;
using Hazelcast.NearCaching;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Protocol.Models;
using Hazelcast.Testing;
using Hazelcast.Testing.Remote;
using NUnit.Framework;

namespace Hazelcast.Tests.Configuration.Dynamic;

[TestFixture]
public class DynamicConfigureMapTests : RemoteTestBase
{
    [Test]
    public async Task ConfigureMap()
    {
        // create remote client and cluster
        var rcClient = await ConnectToRemoteControllerAsync().CfAwait();
        var rcCluster = await rcClient.CreateClusterAsync(Hazelcast.Testing.Remote.Resources.hazelcast).CfAwait();
        var rcMember = await rcClient.StartMemberAsync(rcCluster);

        var options = CreateHazelcastOptions();
        options.ClusterName = rcCluster.Id;
        await using var client = await HazelcastClientFactory.StartNewClientAsync(options).CfAwait();

        await client.DynamicOptions.ConfigureMapAsync("map-name", mapOptions =>
            {
                // everything below is just values

                mapOptions.Name = "map-name"; // there is no default for that one
                mapOptions.SplitBrainProtectionName = MapOptions.Defaults.SplitBrainProtectionName;

                mapOptions.BackupCount = MapOptions.Defaults.BackupCount;
                mapOptions.AsyncBackupCount = MapOptions.Defaults.AsyncBackupCount;
                mapOptions.TimeToLiveSeconds = MapOptions.Defaults.TtlSeconds;
                mapOptions.MaxIdleSeconds = MapOptions.Defaults.MaxIdleSeconds;
                mapOptions.StatisticsEnabled = MapOptions.Defaults.StatisticsEnabled;
                mapOptions.PerEntryStatsEnabled = MapOptions.Defaults.EntryStatsEnabled;
                mapOptions.ReadBackupData = MapOptions.Defaults.ReadBackupData;

                mapOptions.CacheDeserializedValues = MapOptions.Defaults.CachedDeserializedValues;
                mapOptions.InMemoryFormat = MapOptions.Defaults.InMemoryFormat;
                mapOptions.MetadataPolicy = MapOptions.Defaults.MetadataPolicy;

                // everything below is pre-existing objects

                mapOptions.MapStore.Enabled = MapStoreOptions.Defaults.Enabled;
                mapOptions.MapStore.Offload = MapStoreOptions.Defaults.Offload;
                mapOptions.MapStore.InitialLoadMode = MapStoreOptions.Defaults.InitialLoadMode;
                mapOptions.MapStore.IsWriteCoalescing = MapStoreOptions.Defaults.WriteCoalescing;
                mapOptions.MapStore.ClassName = "className"; // cannot be default 'null' value
                mapOptions.MapStore.FactoryClassName = "factoryClassName"; // cannot be default 'null' value

                mapOptions.MergePolicy.BatchSize = MergePolicyOptions.Defaults.BatchSize;
                mapOptions.MergePolicy.Policy = MergePolicyOptions.Defaults.MergePolicy;

                mapOptions.HotRestart.Enabled = HotRestartOptions.Defaults.Enabled;
                mapOptions.HotRestart.Fsync = HotRestartOptions.Defaults.Fsync;

                mapOptions.DataPersistence.Enabled = DataPersistenceOptions.Defaults.Enabled;
                mapOptions.DataPersistence.Fsync = DataPersistenceOptions.Defaults.Fsync;

                //mapOptions.MerkleTree.Enabled = ; // default is unset
                mapOptions.MerkleTree.Depth = MerkleTreeOptions.Defaults.Depth;

                mapOptions.EventJournal.Enabled = EventJournalOptions.Defaults.Enabled;
                mapOptions.EventJournal.Capacity = EventJournalOptions.Defaults.Capacity;
                mapOptions.EventJournal.TimeToLiveSeconds = 123;  // cannot be default 'zero' value

                mapOptions.Eviction.EvictionPolicy = EvictionOptions.Defaults.EvictionPolicy;
                mapOptions.Eviction.MaxSizePolicy = EvictionOptions.Defaults.MaxSizePolicy;
                mapOptions.Eviction.Size = EvictionOptions.Defaults.Size;
                mapOptions.Eviction.ComparatorClassName = "comparatorClassName"; // cannot be default 'null' value

                mapOptions.TieredStore.Enabled = TieredStoreOptions.Defaults.Enabled;
                mapOptions.TieredStore.DiskTier.Enabled = DiskTierOptions.Defaults.Enabled;
                mapOptions.TieredStore.DiskTier.DeviceName = DiskTierOptions.Defaults.DeviceName;
                mapOptions.TieredStore.MemoryTier.Capacity = MemoryTierOptions.Defaults.Capacity;

                // everything below is initially null
                // collections are auto-initialized

                mapOptions.PartitioningAttributes.Add(new PartitioningAttributeOptions("name"));

                mapOptions.NearCache = new NearCacheOptions
                {
                    Name = "name",
                    InMemoryFormat = InMemoryFormat.Native,
                    CacheLocalEntries = true,
                    InvalidateOnChange = true,
                    SerializeKeys = true,
                    LocalUpdatePolicy = NearCacheOptions.UpdatePolicy.CacheOnUpdate,
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

                mapOptions.WanReplicationRef = new WanReplicationRef
                {
                    Name = "name",
                    MergePolicyClassName = "className",
                    RepublishingEnabled = true,
                    Filters = // initially not null
                    {
                        "filter"
                    }
                };

                mapOptions.EntryListeners.Add(new EntryListenerOptions("className", true, true));

                mapOptions.PartitionLostListeners.Add(new MapPartitionLostListenerOptions("className"));

                mapOptions.Indexes.Add(new IndexOptions
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

                mapOptions.Attributes.Add(new AttributeOptions("name", "extractorClassName"));

                mapOptions.QueryCaches.Add(new QueryCacheOptions("name")
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

                mapOptions.PartitioningStrategy = new PartitioningStrategyOptions("partitioningStrategyClass");
            }
        );

        await client.DisposeAsync();
        await rcClient.StopMemberAsync(rcCluster, rcMember);
        await rcClient.ShutdownClusterAsync(rcCluster);
        await rcClient.ExitAsync();
    }

    [Test]
    public async Task Remote()
    {
        // create remote client and cluster
        var rcClient = await ConnectToRemoteControllerAsync().CfAwait();
        var rcCluster = await rcClient.CreateClusterAsync(Hazelcast.Testing.Remote.Resources.hazelcast).CfAwait();
        var rcMember = await rcClient.StartMemberAsync(rcCluster);

        await using var dispose = new DisposeAsyncAction(async () =>
        {
            await rcClient.StopMemberAsync(rcCluster, rcMember);
            await rcClient.ShutdownClusterAsync(rcCluster);
            await rcClient.ExitAsync();
        });

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
    mapConfig.getPartitioningAttributeConfigs())

var ArrayOfBytes = Java.type(""byte[]"")
var bytes = new ArrayOfBytes(message.getBufferLength())
var frame = message.getStartFrame()
var ix = 0;
while (frame != null)
{
    //writeIntL(frame.content.length + 6)
    var len = frame.content.length + 6
    bytes[ix++] = len & 0xff
    bytes[ix++] = (len >>> 8) & 0xff
    bytes[ix++] = (len >>> 16) & 0xff
    bytes[ix++] = (len >>> 24) & 0xff
    var flags = frame.flags
    if (frame == message.getEndFrame()) flags |= 8192 // IS_FINAL_FLAG
    //writeShortL(flags)
    bytes[ix++] = flags & 0xff
    bytes[ix++] = (flags >>> 8) & 0xff
    //writeBytes(frame.content)
    for (var i = 0; i < frame.content.length; i++) bytes[ix++] = frame.content[i]
    frame = frame.next
}

/*
var bytes = new ArrayOfBytes(17)
var UUID = Java.type(""java.util.UUID"")
var uuid = UUID.randomUUID()
var FixedSizeTypesCodec = Java.type(""com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec"")
FixedSizeTypesCodec.encodeUUID(bytes, 0, uuid)
*/

function h2s(b) {
    if (b < 0) b = 0xFF + b + 1
    var s = b.toString(16)
    if (s.length < 2) s = ""0"" + s
    return s
}

var s = """"
for (var i = 0; i < bytes.length; i++) s += h2s(bytes[i])
result = """" + s
"
            ;

        var response = await rcClient.ExecuteOnControllerAsync(rcCluster.Id, script, Lang.JAVASCRIPT);
        Assert.That(response.Success, $"message: {response.Message}");
        Assert.That(response.Result, Is.Not.Null);
        var s = Encoding.ASCII.GetString(response.Result);
        var javaBytesLength = s.Length / 2;
        var javaBytes = new byte[javaBytesLength];
        for (var i = 0; i < javaBytesLength; i++)
            javaBytes[i] = byte.Parse(s.Substring(i * 2, 2), NumberStyles.HexNumber);

        static void WriteMessage(byte[] bytes)
        {
            var position = 0;
            var f = 0;
            while (position < bytes.Length)
            {
                var frameLength = bytes.ReadIntL(position);
                position += BytesExtensions.SizeOfInt;
                var frameFlags = bytes.ReadShortL(position);
                position += BytesExtensions.SizeOfShort;
                Console.Write($"FRAME {f++}: {frameFlags:X4} {frameLength:X8} ");
                for (var i = 0; i < frameLength-6; i++, position++)
                    Console.Write($"{bytes[position]:X2} ");
                Console.WriteLine();
            }
        }

        static bool CompareMessages(byte[] bytes0, byte[] bytes1)
        {
            if (bytes0.Length != bytes1.Length)
            {
                Console.WriteLine($"LENGHTS: {bytes0.Length} != {bytes1.Length}");
                return false;
            }

            var position = 0;
            var f = 0;
            while (position < bytes0.Length && position < bytes1.Length)
            {
                var frameLength0 = bytes0.ReadIntL(position) - 6;
                var frameLength1 = bytes1.ReadIntL(position) - 6;
                position += BytesExtensions.SizeOfInt;

                if (frameLength0 != frameLength1)
                {
                    Console.WriteLine($"FRAME {f:000}: len {frameLength0} != {frameLength1}");
                    for (var k = 0; k < 64; k++)
                        Console.Write($"{bytes0[position + k]:X2} ");
                    Console.WriteLine();
                    for (var k = 0; k < 64; k++)
                        Console.Write($"{bytes1[position + k]:X2} ");
                    Console.WriteLine();
                    return false;
                }

                var frameFlags0 = bytes0.ReadShortL(position);
                var frameFlags1 = bytes0.ReadShortL(position);
                position += BytesExtensions.SizeOfShort;

                if (frameFlags0 != frameFlags1)
                {
                    Console.WriteLine($"FRAME {f:000}: flags {frameFlags1} != {frameFlags0}");
                    return false;
                }

                Console.Write($"FRAME {f++:000}: {frameFlags0:X4} {frameLength0:00000000} ");
                var i = 0;
                while (i < frameLength0)
                {
                    if (bytes0[position + i] != bytes1[position + i]) break;
                    i++;
                }
                if (i == frameLength0) Console.WriteLine("OK");
                else
                {
                    Console.WriteLine($"ERR at byte {i}");
                    Console.Write(" -  ");
                    for (var j = 0; j < frameLength0; j++) Console.Write($"{bytes0[position + j]:X2} ");
                    Console.WriteLine();
                    Console.Write(" -  ");
                    for (var j = 0; j < frameLength1; j++) Console.Write($"{bytes1[position + j]:X2} ");
                    Console.WriteLine();
                    Console.Write("    ");
                    for (var j = 0; j < i; j++) Console.Write($"   ");
                    Console.WriteLine("^^");
                    return false;
                }

                position += frameLength0;
            }

            return true;
        }

        // TODO: recreate the message, ensure we can parse?
        //var r = DynamicConfigAddMapConfigServerCodec.DecodeRequest();

        var mapOptions = new MapOptions("map-name");
        var message = DynamicConfigAddMapConfigCodec.EncodeRequest(
            mapOptions.Name,
            mapOptions.BackupCount, mapOptions.AsyncBackupCount,
            mapOptions.TimeToLiveSeconds, mapOptions.MaxIdleSeconds,
            EvictionConfigHolder.Of(mapOptions.Eviction),
            mapOptions.ReadBackupData,
            mapOptions.CacheDeserializedValues.ToJavaString(),
            mapOptions.MergePolicy.Policy, mapOptions.MergePolicy.BatchSize,
            mapOptions.InMemoryFormat.ToJavaString(),
            null, //MapListenerConfigs(mapOptions.EntryListeners),
            null, //MapListenerConfigs(mapOptions.PartitionLostListeners),
            mapOptions.StatisticsEnabled,
            mapOptions.SplitBrainProtectionName,
            MapStoreConfigHolder.Of(mapOptions.MapStore),
            NearCacheConfigHolder.Of(mapOptions.NearCache),
            mapOptions.WanReplicationRef,
            mapOptions.Indexes,
            mapOptions.Attributes,
            null, //MapQueryCacheConfigs(mapOptions.QueryCaches),
            null,/*mapOptions.PartitioningStrategy?.PartitioningStrategyClass*/
            null,
            mapOptions.HotRestart, mapOptions.EventJournal, mapOptions.MerkleTree,
            (int)mapOptions.MetadataPolicy, mapOptions.PerEntryStatsEnabled,
            mapOptions.DataPersistence, mapOptions.TieredStore, mapOptions.PartitioningAttributes);

        var l = 0;
        var f = message.FirstFrame;
        while (f != null)
        {
            l += f.Bytes.Length + 6;
            f = f.Next;
        }

        var dotnetBytesLength = 0;
        for (var frame = message.FirstFrame; frame != null; frame = frame.Next)
            dotnetBytesLength += frame.Length;

        var dotnetBytes = new byte[dotnetBytesLength];
        var ix = 0;
        for (var frame = message.FirstFrame; frame != null; frame = frame.Next)
        {
            //writeIntL(frame.content.length + 6)
            var len = frame.Length;
            dotnetBytes.WriteIntL(ix, len);
            ix += BytesExtensions.SizeOfInt;
            var flags = frame.Flags;
            if (frame == message.LastFrame) flags |= FrameFlags.Final;
            //writeShortL(flags)
            dotnetBytes.WriteShortL(ix, (short) flags);
            ix += BytesExtensions.SizeOfShort;
            //writeBytes(frame.content)
            for (var i = 0; i < frame.Bytes.Length; i++) dotnetBytes[ix++] = frame.Bytes[i];
        }

        Assert.That(CompareMessages(javaBytes, dotnetBytes), "Messages don't compare.");
    }
}