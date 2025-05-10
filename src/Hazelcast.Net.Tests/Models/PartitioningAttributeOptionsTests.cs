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
using Hazelcast.Core;
using NUnit.Framework;
using Hazelcast.Models;
using Hazelcast.Serialization;
using NSubstitute;

namespace Hazelcast.Tests.Models
{
    public class PartitioningAttributeOptionsTests
    {
        [Test]
        public void Constructor_WithValidParameter_InitializesPropertiesCorrectly()
        {
            var attributeName = "TestAttribute";

            var partitioningAttributeOptions = new PartitioningAttributeOptions(attributeName);

            Assert.AreEqual(attributeName, partitioningAttributeOptions.AttributeName);
        }

        [Test]
        public void WriteData_ReadData_WritesAndReadsDataCorrectly()
        {
            var attributeName = "TestAttribute";
            var partitioningAttributeOptions = new PartitioningAttributeOptions(attributeName);

            var output = new ObjectDataOutput(1024, null, Endianness.LittleEndian);
            partitioningAttributeOptions.WriteData(output);

            var input = new ObjectDataInput(output.Buffer, null, Endianness.LittleEndian);
            var readPartitioningAttributeOptions = new PartitioningAttributeOptions();
            readPartitioningAttributeOptions.ReadData(input);

            Assert.AreEqual(partitioningAttributeOptions.AttributeName, readPartitioningAttributeOptions.AttributeName);
        }

        [Test]
        public void ToString_ReturnsCorrectFormat()
        {
            var attributeName = "TestAttribute";
            var partitioningAttributeOptions = new PartitioningAttributeOptions(attributeName);

            var expectedString = $"PartitioningAttributeConfig{{attributeName='{attributeName}'}}";

            Assert.AreEqual(expectedString, partitioningAttributeOptions.ToString());
        }
    }
}