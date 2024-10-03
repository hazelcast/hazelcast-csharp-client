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
using System;
using Hazelcast.Core;
using Hazelcast.Models;
using Hazelcast.NearCaching;
using Hazelcast.Serialization;
using Hazelcast.Tests.Serialization.Compact;
using NSubstitute;
using NUnit.Framework;
namespace Hazelcast.Tests.Models
{
    public class TestOptionModelsTests
    {
        [Test]
        public void TestTimedExpiryPolicyFactoryOptions()
        {
            // Arrange
            var expiryPolicyType = ExpiryPolicyType.Accessed;
            var durationConfig = new DurationOptions(1, TimeUnit.Seconds);
            var options = new TimedExpiryPolicyFactoryOptions(expiryPolicyType, durationConfig);

            // Act
            var actualExpiryPolicyType = options.ExpiryPolicyType;
            var actualDurationConfig = options.DurationConfig;

            // Assert
            Assert.AreEqual(expiryPolicyType, actualExpiryPolicyType);
            Assert.AreEqual(durationConfig.TimeUnit, actualDurationConfig.TimeUnit);
            Assert.AreEqual(durationConfig.DurationAmount, actualDurationConfig.DurationAmount);
        }

        [TestFixture]
        public class MemoryUnitExtensionsTests
        {
            [Test]
            public void TestToBytes()
            {
                // Arrange
                var valueInKiloBytes = 5;
                var expectedValueInBytes = 5 * 1000;

                // Act
                var actualValueInBytes = MemoryUnitExtensions.Convert(MemoryUnit.Bytes, MemoryUnit.KiloBytes, valueInKiloBytes);

                // Assert
                Assert.AreEqual(expectedValueInBytes, actualValueInBytes);
            }
        }
        
        [Test]
        public void Abbrev_ShouldReturnCorrectAbbreviation()
        {
            Assert.AreEqual("B", MemoryUnit.Bytes.Abbrev());
            Assert.AreEqual("KB", MemoryUnit.KiloBytes.Abbrev());
            Assert.AreEqual("MB", MemoryUnit.MegaBytes.Abbrev());
            Assert.AreEqual("GB", MemoryUnit.GigaBytes.Abbrev());
        }

        [Test]
        public void Abbrev_ShouldThrowExceptionForInvalidMemoryUnit()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ((MemoryUnit)999).Abbrev());
        }

        [Test]
        public void TestCacheSimpleEntryListenerOptions()
        {
            // Arrange
            var cacheEntryListenerFactory = "TestFactory";
            var cacheEntryEventFilterFactory = "TestFilterFactory";
            var oldValueRequired = true;
            var synchronous = true;

            var options = new CacheSimpleEntryListenerOptions
            {
                CacheEntryListenerFactory = cacheEntryListenerFactory,
                CacheEntryEventFilterFactory = cacheEntryEventFilterFactory,
                OldValueRequired = oldValueRequired,
                Synchronous = synchronous
            };

            // Act
            var actualCacheEntryListenerFactory = options.CacheEntryListenerFactory;
            var actualCacheEntryEventFilterFactory = options.CacheEntryEventFilterFactory;
            var actualOldValueRequired = options.OldValueRequired;
            var actualSynchronous = options.Synchronous;

            // Assert
            Assert.AreEqual(cacheEntryListenerFactory, actualCacheEntryListenerFactory);
            Assert.AreEqual(cacheEntryEventFilterFactory, actualCacheEntryEventFilterFactory);
            Assert.AreEqual(oldValueRequired, actualOldValueRequired);
            Assert.AreEqual(synchronous, actualSynchronous);
        }

        [Test]
        public void TestHotRestartOptions()
        {
            // Arrange
            var enabled = true;
            var fsync = true;

            var options = new HotRestartOptions
            {
                Enabled = enabled,
                Fsync = fsync
            };

            // Act
            var actualEnabled = options.Enabled;
            var actualFsync = options.Fsync;

            // Assert
            Assert.AreEqual(enabled, actualEnabled);
            Assert.AreEqual(fsync, actualFsync);
        }

        [Test]
        public void TestPartitioningStrategyOptions()
        {
            // Arrange
            var partitioningStrategyClass = "TestStrategyClass";

            var options = new PartitioningStrategyOptions
            {
                PartitioningStrategyClass = partitioningStrategyClass
            };

            // Act
            var actualPartitioningStrategyClass = options.PartitioningStrategyClass;

            // Assert
            Assert.AreEqual(partitioningStrategyClass, actualPartitioningStrategyClass);
        }

        [Test]
        public void TestDataPersistenceOptions()
        {
            // Arrange
            var enabled = true;
            var fsync = true;

            var options = new DataPersistenceOptions
            {
                Enabled = enabled,
                Fsync = fsync
            };

            // Act
            var actualEnabled = options.Enabled;
            var actualFsync = options.Fsync;

            // Assert
            Assert.AreEqual(enabled, actualEnabled);
            Assert.AreEqual(fsync, actualFsync);
        }

        [Test]
        public void TestReadDataAndWriteData()
        {

            var eviction = new EvictionOptions { Size = 100, EvictionPolicy = EvictionPolicy.Lru };
            var preloader = new NearCachePreloaderOptions { Enabled = true, Directory = "TestDirectory", StoreInitialDelaySeconds = 5, StoreIntervalSeconds = 10 };
            // Arrange
            var originalOptions = new NearCacheOptions
            {
                Name = "Test",
                InvalidateOnChange = true,
                TimeToLiveSeconds = 60,
                MaxIdleSeconds = 30,
                InMemoryFormat = InMemoryFormat.Binary,
                LocalUpdatePolicy = UpdatePolicy.Invalidate,
                Eviction = eviction,
                Preloader = preloader
            };

            var orw = Substitute.For<IReadWriteObjectsFromIObjectDataInputOutput>();
            orw.Read<EvictionOptions>(Arg.Any<IObjectDataInput>()).Returns(eviction);
            orw.Read<NearCachePreloaderOptions>(Arg.Any<IObjectDataInput>()).Returns(preloader);

            var output = new ObjectDataOutput(1000, orw, Endianness.LittleEndian);
            var input = new ObjectDataInput(output.Buffer, orw, Endianness.LittleEndian);

            // Act
            originalOptions.WriteData(output);
            var newOptions = new NearCacheOptions();
            newOptions.ReadData(input);

            // Assert
            Assert.AreEqual(originalOptions.Name, newOptions.Name);
            Assert.AreEqual(originalOptions.InvalidateOnChange, newOptions.InvalidateOnChange);
            Assert.AreEqual(originalOptions.TimeToLiveSeconds, newOptions.TimeToLiveSeconds);
            Assert.AreEqual(originalOptions.MaxIdleSeconds, newOptions.MaxIdleSeconds);
            Assert.AreEqual(originalOptions.InMemoryFormat, newOptions.InMemoryFormat);
            Assert.AreEqual(originalOptions.LocalUpdatePolicy, newOptions.LocalUpdatePolicy);
            Assert.AreEqual(originalOptions.Eviction.Size, newOptions.Eviction.Size);
            Assert.AreEqual(originalOptions.Eviction.EvictionPolicy, newOptions.Eviction.EvictionPolicy);
            Assert.AreEqual(originalOptions.Preloader.Enabled, newOptions.Preloader.Enabled);
            Assert.AreEqual(originalOptions.Preloader.Directory, newOptions.Preloader.Directory);
            Assert.AreEqual(originalOptions.Preloader.StoreInitialDelaySeconds, newOptions.Preloader.StoreInitialDelaySeconds);
            Assert.AreEqual(originalOptions.Preloader.StoreIntervalSeconds, newOptions.Preloader.StoreIntervalSeconds);
        }
        
        [Test]
        public void ReadDataAndWriteData_ShouldPreserveState()
        {
            // Arrange
            var originalOptions = new NearCachePreloaderOptions
            {
                Enabled = true,
                Directory = "TestDirectory",
                StoreInitialDelaySeconds = 5,
                StoreIntervalSeconds = 10
            };

            var orw = Substitute.For<IReadWriteObjectsFromIObjectDataInputOutput>();
            var output = new ObjectDataOutput(1000, orw, Endianness.LittleEndian);
            var input = new ObjectDataInput(output.Buffer, orw, Endianness.LittleEndian);

            // Act
            originalOptions.WriteData(output);
            var newOptions = new NearCachePreloaderOptions();
            newOptions.ReadData(input);

            // Assert
            Assert.AreEqual(originalOptions.Enabled, newOptions.Enabled);
            Assert.AreEqual(originalOptions.Directory, newOptions.Directory);
            Assert.AreEqual(originalOptions.StoreInitialDelaySeconds, newOptions.StoreInitialDelaySeconds);
            Assert.AreEqual(originalOptions.StoreIntervalSeconds, newOptions.StoreIntervalSeconds);
        }
    }
}
