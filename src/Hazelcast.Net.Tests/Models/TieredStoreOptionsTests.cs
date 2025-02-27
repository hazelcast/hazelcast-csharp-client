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
namespace Hazelcast.Tests.Models
{
using NUnit.Framework;
using Hazelcast.Models;
using Hazelcast.Core;
using Hazelcast.Serialization;

[TestFixture]
public class TieredStoreOptionsTests
{
    [Test]
    public void DefaultConstructor_SetsPropertiesToDefault()
    {
        var options = new TieredStoreOptions();

        Assert.IsFalse(options.Enabled);
        Assert.IsNotNull(options.MemoryTier);
        Assert.IsNotNull(options.DiskTier);
    }

    [Test]
    public void Constructor_WithTieredStoreOptionsArgument_CopiesProperties()
    {
        var originalOptions = new TieredStoreOptions
        {
            Enabled = true,
            MemoryTier = new MemoryTierOptions(),
            DiskTier = new DiskTierOptions()
        };
        var copiedOptions = new TieredStoreOptions(originalOptions);

        Assert.AreEqual(originalOptions.Enabled, copiedOptions.Enabled);
        Assert.AreEqual(originalOptions.MemoryTier.Capacity, copiedOptions.MemoryTier.Capacity);
        Assert.AreEqual(originalOptions.DiskTier.DeviceName, copiedOptions.DiskTier.DeviceName);
        Assert.AreEqual(originalOptions.DiskTier.Enabled, copiedOptions.DiskTier.Enabled);
    }

    [Test]
    public void ToString_ReturnsExpectedFormat()
    {
        var options = new TieredStoreOptions
        {
            Enabled = true,
            MemoryTier = new MemoryTierOptions(),
            DiskTier = new DiskTierOptions()
        };

        var result = options.ToString();

        StringAssert.Contains("enabled=True", result);
        StringAssert.Contains("memoryTierConfig=", result);
        StringAssert.Contains("diskTierConfig=", result);
    }
}
}
