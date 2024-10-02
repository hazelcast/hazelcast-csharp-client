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
using NUnit.Framework;
using Hazelcast.Models;
using Hazelcast.Serialization;
using System.Collections.Generic;
using Hazelcast.Configuration;
using Hazelcast.Core;
using Hazelcast.Query;
using Hazelcast.Serialization.ConstantSerializers;
using Hazelcast.Tests.Serialization.Compact;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Hazelcast.Tests.Models
{
    [TestFixture]
    public class MapStoreOptionsTests
    {
        [Test]
        public void Constructor_WithValidParameters_InitializesPropertiesCorrectly()
        {
            var enabled = true;
            var className = "TestClassName";
            var factoryClassName = "TestFactoryClassName";
            var writeDelaySeconds = 10;
            var writeBatchSize = 5;
            var properties = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } };
            var initialLoadMode = LoadMode.Eager;
            var writeCoalescing = true;
            var offload = true;

            var mapStoreOptions = new MapStoreOptions
            {
                Enabled = enabled,
                ClassName = className,
                FactoryClassName = factoryClassName,
                WriteDelaySeconds = writeDelaySeconds,
                WriteBatchSize = writeBatchSize,
                Properties = properties,
                InitialLoadMode = initialLoadMode,
                WriteCoalescing = writeCoalescing,
                Offload = offload
            };

            Assert.AreEqual(enabled, mapStoreOptions.Enabled);
            Assert.AreEqual(className, mapStoreOptions.ClassName);
            Assert.AreEqual(factoryClassName, mapStoreOptions.FactoryClassName);
            Assert.AreEqual(writeDelaySeconds, mapStoreOptions.WriteDelaySeconds);
            Assert.AreEqual(writeBatchSize, mapStoreOptions.WriteBatchSize);
            Assert.AreEqual(properties, mapStoreOptions.Properties);
            Assert.AreEqual(initialLoadMode, mapStoreOptions.InitialLoadMode);
            Assert.AreEqual(writeCoalescing, mapStoreOptions.WriteCoalescing);
            Assert.AreEqual(offload, mapStoreOptions.Offload);
        }

        [Test]
        public void WriteData_ReadData_WritesAndReadsDataCorrectly()
        {
            var enabled = true;
            var className = "TestClassName";
            var factoryClassName = "TestFactoryClassName";
            var writeDelaySeconds = 10;
            var writeBatchSize = 5;
            var properties = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } };
            var initialLoadMode = LoadMode.Eager;
            var writeCoalescing = true;
            var offload = true;

            var mapStoreOptions = new MapStoreOptions
            {
                Enabled = enabled,
                ClassName = className,
                FactoryClassName = factoryClassName,
                WriteDelaySeconds = writeDelaySeconds,
                WriteBatchSize = writeBatchSize,
                Properties = properties,
                InitialLoadMode = initialLoadMode,
                WriteCoalescing = writeCoalescing,
                Offload = offload
            };

            var orw = Substitute.For<IReadWriteObjectsFromIObjectDataInputOutput>();
            
            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            mapStoreOptions.WriteData(output);

            var input = new ObjectDataInput(output.Buffer, orw, Endianness.LittleEndian);
            var readMapStoreOptions = new MapStoreOptions();
            readMapStoreOptions.ReadData(input);

            Assert.AreEqual(mapStoreOptions.Enabled, readMapStoreOptions.Enabled);
            Assert.AreEqual(mapStoreOptions.ClassName, readMapStoreOptions.ClassName);
            Assert.AreEqual(mapStoreOptions.FactoryClassName, readMapStoreOptions.FactoryClassName);
            Assert.AreEqual(mapStoreOptions.WriteDelaySeconds, readMapStoreOptions.WriteDelaySeconds);
            Assert.AreEqual(mapStoreOptions.WriteBatchSize, readMapStoreOptions.WriteBatchSize);
            Assert.AreEqual(mapStoreOptions.InitialLoadMode, readMapStoreOptions.InitialLoadMode);
            Assert.AreEqual(mapStoreOptions.WriteCoalescing, readMapStoreOptions.WriteCoalescing);
            Assert.AreEqual(mapStoreOptions.Offload, readMapStoreOptions.Offload);
        }

        [Test]
        public void ToString_ReturnsCorrectFormat()
        {
            var enabled = true;
            var className = "TestClassName";
            var factoryClassName = "TestFactoryClassName";
            var writeDelaySeconds = 10;
            var writeBatchSize = 5;
            var properties = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } };
            var initialLoadMode = LoadMode.Eager;
            var writeCoalescing = true;
            var offload = true;

            var mapStoreOptions = new MapStoreOptions
            {
                Enabled = enabled,
                ClassName = className,
                FactoryClassName = factoryClassName,
                WriteDelaySeconds = writeDelaySeconds,
                WriteBatchSize = writeBatchSize,
                Properties = properties,
                InitialLoadMode = initialLoadMode,
                WriteCoalescing = writeCoalescing,
                Offload = offload
            };

            var expectedString = $"MapStoreConfig{{enabled={enabled}, className='{className}', factoryClassName='{factoryClassName}', writeDelaySeconds={writeDelaySeconds}, writeBatchSize={writeBatchSize}, properties={properties}, initialLoadMode={initialLoadMode}, writeCoalescing={writeCoalescing}, offload={offload}}}";

            Assert.AreEqual(expectedString, mapStoreOptions.ToString());
        }
    }
}