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
using Hazelcast.Core;
using NUnit.Framework;
using Hazelcast.Models;
using Hazelcast.Serialization;
using NSubstitute;

namespace Hazelcast.Tests.Models
{
    [TestFixture]
    public class MergePolicyOptionsTests
    {
        [Test]
        public void Constructor_WithValidParameters_InitializesPropertiesCorrectly()
        {
            var policy = "TestPolicy";
            var batchSize = 200;

            var mergePolicyOptions = new MergePolicyOptions(policy, batchSize);

            Assert.AreEqual(policy, mergePolicyOptions.Policy);
            Assert.AreEqual(batchSize, mergePolicyOptions.BatchSize);
        }

        [Test]
        public void WriteData_ReadData_WritesAndReadsDataCorrectly()
        {
            var policy = "TestPolicy";
            var batchSize = 200;
            var mergePolicyOptions = new MergePolicyOptions(policy, batchSize);

            var output = new ObjectDataOutput(1024, null, Endianness.LittleEndian);
            mergePolicyOptions.WriteData(output);

            var input = new ObjectDataInput(output.Buffer, null, Endianness.LittleEndian);
            var readMergePolicyOptions = new MergePolicyOptions();
            readMergePolicyOptions.ReadData(input);

            Assert.AreEqual(mergePolicyOptions.Policy, readMergePolicyOptions.Policy);
            Assert.AreEqual(mergePolicyOptions.BatchSize, readMergePolicyOptions.BatchSize);
        }

        [Test]
        public void ToString_ReturnsCorrectFormat()
        {
            var policy = "TestPolicy";
            var batchSize = 200;
            var mergePolicyOptions = new MergePolicyOptions(policy, batchSize);

            var expectedString = $"MergePolicyConfig{{policy='{policy}', batchSize={batchSize}}}";

            Assert.AreEqual(expectedString, mergePolicyOptions.ToString());
        }
    }
}