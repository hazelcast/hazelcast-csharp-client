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
    [TestFixture]
    public class MerkleTreeOptionsTests
    {
        [Test]
        public void Constructor_WithValidParameters_InitializesPropertiesCorrectly()
        {
            var depth = 5;
            var enabled = true;

            var merkleTreeOptions = new MerkleTreeOptions
            {
                Depth = depth,
                Enabled = enabled
            };

            Assert.AreEqual(depth, merkleTreeOptions.Depth);
            Assert.AreEqual(enabled, merkleTreeOptions.Enabled);
        }

        [Test]
        public void WriteData_ReadData_WritesAndReadsDataCorrectly()
        {
            var depth = 5;
            var enabled = true;

            var merkleTreeOptions = new MerkleTreeOptions
            {
                Depth = depth,
                Enabled = enabled
            };

            var output = new ObjectDataOutput(1024, null, Endianness.LittleEndian, new DefaultBufferPool());
            merkleTreeOptions.WriteData(output);

            var input = new ObjectDataInput(output.Buffer, null, Endianness.LittleEndian);
            var readMerkleTreeOptions = new MerkleTreeOptions();
            readMerkleTreeOptions.ReadData(input);

            Assert.AreEqual(merkleTreeOptions.Depth, readMerkleTreeOptions.Depth);
            Assert.AreEqual(merkleTreeOptions.Enabled, readMerkleTreeOptions.Enabled);
        }

        [Test]
        public void ToString_ReturnsCorrectFormat()
        {
            var depth = 5;
            var enabled = true;

            var merkleTreeOptions = new MerkleTreeOptions
            {
                Depth = depth,
                Enabled = enabled
            };

            var expectedString = $"MerkleTreeConfig{{enabled={enabled}, depth={depth}}}";

            Assert.AreEqual(expectedString, merkleTreeOptions.ToString());
        }
    }
}