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
using Hazelcast.Core;
using Hazelcast.Tests.Serialization.Compact;
using NSubstitute;

namespace Hazelcast.Tests.Models
{
    [TestFixture]
    public class RingbufferStoreOptionsTests
    {
        [Test]
        public void Constructor_WithValidParameters_InitializesPropertiesCorrectly()
        {
            var enabled = true;
            var className = "TestClassName";
            var factoryClassName = "TestFactoryClassName";
            var properties = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } };

            var ringbufferStoreOptions = new RingbufferStoreOptions
            {
                Enabled = enabled,
                ClassName = className,
                FactoryClassName = factoryClassName,
                Properties = properties
            };

            Assert.AreEqual(enabled, ringbufferStoreOptions.Enabled);
            Assert.AreEqual(className, ringbufferStoreOptions.ClassName);
            Assert.AreEqual(factoryClassName, ringbufferStoreOptions.FactoryClassName);
            Assert.AreEqual(properties, ringbufferStoreOptions.Properties);
        }

        [Test]
        public void WriteData_ReadData_WritesAndReadsDataCorrectly()
        {
            var enabled = true;
            var className = "TestClassName";
            var factoryClassName = "TestFactoryClassName";
            var properties = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } };

            var ringbufferStoreOptions = new RingbufferStoreOptions
            {
                Enabled = enabled,
                ClassName = className,
                FactoryClassName = factoryClassName,
                Properties = properties
            };
            
            var orw = Substitute.For<IReadWriteObjectsFromIObjectDataInputOutput>();
            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            ringbufferStoreOptions.WriteData(output);

            var input = new ObjectDataInput(output.Buffer, orw, Endianness.LittleEndian);
            var readRingbufferStoreOptions = new RingbufferStoreOptions();
            readRingbufferStoreOptions.ReadData(input);

            Assert.AreEqual(ringbufferStoreOptions.Enabled, readRingbufferStoreOptions.Enabled);
            Assert.AreEqual(ringbufferStoreOptions.ClassName, readRingbufferStoreOptions.ClassName);
            Assert.AreEqual(ringbufferStoreOptions.FactoryClassName, readRingbufferStoreOptions.FactoryClassName);
        }

        [Test]
        public void ToString_ReturnsCorrectFormat()
        {
            var enabled = true;
            var className = "TestClassName";
            var factoryClassName = "TestFactoryClassName";
            var properties = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } };

            var ringbufferStoreOptions = new RingbufferStoreOptions
            {
                Enabled = enabled,
                ClassName = className,
                FactoryClassName = factoryClassName,
                Properties = properties
            };

            var expectedString = $"RingbufferStoreConfig{{enabled={enabled}, className='{className}', factoryClassName='{factoryClassName}', properties={properties}}}";

            Assert.AreEqual(expectedString, ringbufferStoreOptions.ToString());
        }
    }
}