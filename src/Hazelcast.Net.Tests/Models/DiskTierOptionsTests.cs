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
    public class DiskTierOptionsTests
    {
        [Test]
        public void Constructor_WithValidParameters_InitializesPropertiesCorrectly()
        {
            var enabled = true;
            var deviceName = "TestDevice";

            var diskTierOptions = new DiskTierOptions { Enabled = enabled, DeviceName = deviceName };

            Assert.AreEqual(enabled, diskTierOptions.Enabled);
            Assert.AreEqual(deviceName, diskTierOptions.DeviceName);
        }

        [Test]
        public void WriteData_ReadData_WritesAndReadsDataCorrectly()
        {
            var enabled = true;
            var deviceName = "TestDevice";
            var diskTierOptions = new DiskTierOptions { Enabled = enabled, DeviceName = deviceName };

            var output = new ObjectDataOutput(1024, null, Endianness.LittleEndian);
            diskTierOptions.WriteData(output);

            var input = new ObjectDataInput(output.Buffer, null, Endianness.LittleEndian);
            var readDiskTierOptions = new DiskTierOptions();
            readDiskTierOptions.ReadData(input);

            Assert.AreEqual(diskTierOptions.Enabled, readDiskTierOptions.Enabled);
            Assert.AreEqual(diskTierOptions.DeviceName, readDiskTierOptions.DeviceName);
        }

        [Test]
        public void ToString_ReturnsCorrectFormat()
        {
            var enabled = true;
            var deviceName = "TestDevice";
            var diskTierOptions = new DiskTierOptions { Enabled = enabled, DeviceName = deviceName };

            var expectedString = $"DiskTierConfig{{enabled={enabled}, deviceName='{deviceName}'}}";

            Assert.AreEqual(expectedString, diskTierOptions.ToString());
        }
    }
}
