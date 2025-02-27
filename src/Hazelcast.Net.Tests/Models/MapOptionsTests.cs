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
using NUnit.Framework;
using Hazelcast.Models;
using Hazelcast.Serialization;
using System.Collections.Generic;
using Hazelcast.Configuration;
using Hazelcast.Core;
using Hazelcast.NearCaching;
using Hazelcast.Serialization.ConstantSerializers;
using Hazelcast.Tests.Serialization.Compact;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Hazelcast.Tests.Models
{
    [TestFixture]
    public class MapOptionsTests
    {
        [Test]
        public void Constructor_WithValidParameters_InitializesPropertiesCorrectly()
        {
            var name = "TestMap";
            var backupCount = 2;
            var asyncBackupCount = 1;

            var mapOptions = new MapOptions(name)
            {
                BackupCount = backupCount,
                AsyncBackupCount = asyncBackupCount
            };

            Assert.AreEqual(name, mapOptions.Name);
            Assert.AreEqual(backupCount, mapOptions.BackupCount);
            Assert.AreEqual(asyncBackupCount, mapOptions.AsyncBackupCount);
        }
        [Test]
        public void Constructor_WithMapOptionsArgument_CopiesProperties()
        {
            var originalOptions = new MapOptions("TestMap")
            {
                BackupCount = 2,
                AsyncBackupCount = 1,
                TimeToLiveSeconds = 3600,
                MaxIdleSeconds = 1800,
                Eviction = new EvictionOptions(),
                MapStore = new MapStoreOptions(),
                NearCache = new NearCacheOptions(),
                ReadBackupData = true,
                CacheDeserializedValues = CacheDeserializedValues.Never,
                MergePolicy = new MergePolicyOptions(),
                InMemoryFormat = InMemoryFormat.Binary,
                WanReplicationRef = new WanReplicationRef(),
                EntryListeners = new List<EntryListenerOptions>(),
                PartitionLostListeners = new List<MapPartitionLostListenerOptions>(),
                Indexes = new List<IndexOptions>(),
                Attributes = new List<AttributeOptions>(),
                QueryCaches = new List<QueryCacheOptions>(),
                StatisticsEnabled = true,
                PartitioningStrategy = new PartitioningStrategyOptions(),
                SplitBrainProtectionName = "TestProtection",
                HotRestart = new HotRestartOptions(),
                MerkleTree = new MerkleTreeOptions(),
                EventJournal = new EventJournalOptions(),
                MetadataPolicy = MetadataPolicy.CreateOnUpdate,
                PerEntryStatsEnabled = true,
                DataPersistence = new DataPersistenceOptions(),
                TieredStore = new TieredStoreOptions(),
                PartitioningAttributes = new List<PartitioningAttributeOptions>()
            };

            var copiedOptions = new MapOptions(originalOptions);

            Assert.AreEqual(originalOptions.Name, copiedOptions.Name);
            Assert.AreEqual(originalOptions.BackupCount, copiedOptions.BackupCount);
            Assert.AreEqual(originalOptions.AsyncBackupCount, copiedOptions.AsyncBackupCount);
            Assert.AreEqual(originalOptions.TimeToLiveSeconds, copiedOptions.TimeToLiveSeconds);
            Assert.AreEqual(originalOptions.MaxIdleSeconds, copiedOptions.MaxIdleSeconds);
            Assert.AreEqual(originalOptions.Eviction.Size, copiedOptions.Eviction.Size);
            Assert.AreEqual(originalOptions.MapStore.Enabled, copiedOptions.MapStore.Enabled);
            Assert.AreEqual(originalOptions.NearCache.Name, copiedOptions.NearCache.Name);
            Assert.AreEqual(originalOptions.ReadBackupData, copiedOptions.ReadBackupData);
            Assert.AreEqual(originalOptions.CacheDeserializedValues, copiedOptions.CacheDeserializedValues);
            Assert.AreEqual(originalOptions.MergePolicy.Policy, copiedOptions.MergePolicy.Policy);
            Assert.AreEqual(originalOptions.InMemoryFormat, copiedOptions.InMemoryFormat);
            Assert.AreEqual(originalOptions.WanReplicationRef.Name, copiedOptions.WanReplicationRef.Name);
            Assert.AreEqual(originalOptions.EntryListeners.Count, copiedOptions.EntryListeners.Count);
            Assert.AreEqual(originalOptions.PartitionLostListeners.Capacity, copiedOptions.PartitionLostListeners.Count);
            Assert.AreEqual(originalOptions.Indexes.Count, copiedOptions.Indexes.Count);
            Assert.AreEqual(originalOptions.Attributes.Count, copiedOptions.Attributes.Count);
            Assert.AreEqual(originalOptions.QueryCaches.Count, copiedOptions.QueryCaches.Count);
            Assert.AreEqual(originalOptions.StatisticsEnabled, copiedOptions.StatisticsEnabled);
            Assert.AreEqual(originalOptions.SplitBrainProtectionName, copiedOptions.SplitBrainProtectionName);
            Assert.AreEqual(originalOptions.HotRestart.Enabled, copiedOptions.HotRestart.Enabled);
            Assert.AreEqual(originalOptions.MerkleTree.Enabled, copiedOptions.MerkleTree.Enabled);
            Assert.AreEqual(originalOptions.EventJournal.Enabled, copiedOptions.EventJournal.Enabled);
            Assert.AreEqual(originalOptions.MetadataPolicy, copiedOptions.MetadataPolicy);
            Assert.AreEqual(originalOptions.PerEntryStatsEnabled, copiedOptions.PerEntryStatsEnabled);
            Assert.AreEqual(originalOptions.DataPersistence.Enabled, copiedOptions.DataPersistence.Enabled);
            Assert.AreEqual(originalOptions.TieredStore.Enabled, copiedOptions.TieredStore.Enabled);
            Assert.AreEqual(originalOptions.PartitioningAttributes.Count, copiedOptions.PartitioningAttributes.Count);
        }
        [Test]
        public void WriteData_ReadData_WritesAndReadsDataCorrectly()
        {
            var name = "TestMap";
            var backupCount = 2;
            var asyncBackupCount = 1;

            var mapOptions = new MapOptions(name)
            {
                BackupCount = backupCount,
                AsyncBackupCount = asyncBackupCount
            };

            var orw = Substitute.For<IReadWriteObjectsFromIObjectDataInputOutput>();
            orw.Read<HotRestartOptions>(Arg.Any<IObjectDataInput>()).Returns(new HotRestartOptions());
            orw.Read<DataPersistenceOptions>(Arg.Any<IObjectDataInput>()).Returns(new DataPersistenceOptions());
            orw.Read<TieredStoreOptions>(Arg.Any<IObjectDataInput>()).Returns(new TieredStoreOptions());

            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            mapOptions.WriteData(output);

            var input = new ObjectDataInput(output.Buffer, orw, Endianness.LittleEndian);
            var readMapOptions = new MapOptions();
            readMapOptions.ReadData(input);

            Assert.AreEqual(mapOptions.Name, readMapOptions.Name);
            Assert.AreEqual(mapOptions.BackupCount, readMapOptions.BackupCount);
            Assert.AreEqual(mapOptions.AsyncBackupCount, readMapOptions.AsyncBackupCount);
        }

        [Test]
        public void ToString_ReturnsCorrectFormat()
        {
            var name = "TestMap";
            var backupCount = 2;
            var asyncBackupCount = 1;

            var mapOptions = new MapOptions(name)
            {
                BackupCount = backupCount,
                AsyncBackupCount = asyncBackupCount
            };

            var expectedString = $"MapConfig{{name='{name}'";

            Assert.IsTrue(mapOptions.ToString().StartsWith(expectedString));
        }
    }
}
