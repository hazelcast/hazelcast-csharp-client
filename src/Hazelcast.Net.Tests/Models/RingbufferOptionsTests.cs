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
public class RingbufferOptionsTests
{
    [Test]
    public void DefaultConstructor_SetsPropertiesToDefault()
    {
        var options = new RingbufferOptions();

        Assert.AreEqual(RingbufferOptions.Defaults.Capacity, options.Capacity);
        Assert.AreEqual(RingbufferOptions.Defaults.SyncBackupCount, options.BackupCount);
        Assert.AreEqual(RingbufferOptions.Defaults.AsyncBackupCount, options.AsyncBackupCount);
        Assert.AreEqual(RingbufferOptions.Defaults.TtlSeconds, options.TimeToLiveSeconds);
        Assert.AreEqual(RingbufferOptions.Defaults.InMemoryFormat, options.InMemoryFormat);
    }

    [Test]
    public void Constructor_WithName_SetsName()
    {
        var options = new RingbufferOptions("TestName");

        Assert.AreEqual("TestName", options.Name);
    }

    [Test]
    public void Constructor_WithRingbufferOptionsArgument_CopiesProperties()
    {
        var originalOptions = new RingbufferOptions("TestName");
        var copiedOptions = new RingbufferOptions(originalOptions);

        Assert.AreEqual(originalOptions.Name, copiedOptions.Name);
        Assert.AreEqual(originalOptions.Capacity, copiedOptions.Capacity);
        Assert.AreEqual(originalOptions.BackupCount, copiedOptions.BackupCount);
        Assert.AreEqual(originalOptions.AsyncBackupCount, copiedOptions.AsyncBackupCount);
        Assert.AreEqual(originalOptions.TimeToLiveSeconds, copiedOptions.TimeToLiveSeconds);
        Assert.AreEqual(originalOptions.InMemoryFormat, copiedOptions.InMemoryFormat);
    }

    [Test]
    public void ToString_ReturnsExpectedFormat()
    {
        var options = new RingbufferOptions("TestName");

        var result = options.ToString();

        StringAssert.Contains("name='TestName'", result);
        StringAssert.Contains("capacity=", result);
        StringAssert.Contains("backupCount=", result);
        StringAssert.Contains("asyncBackupCount=", result);
        StringAssert.Contains("timeToLiveSeconds=", result);
        StringAssert.Contains("inMemoryFormat=", result);
    }
}
}
