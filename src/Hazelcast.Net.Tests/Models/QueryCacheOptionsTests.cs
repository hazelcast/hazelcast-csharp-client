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
using Hazelcast.Configuration;

[TestFixture]
public class QueryCacheOptionsTests
{
    [Test]
    public void DefaultConstructor_SetsPropertiesToDefault()
    {
        var options = new QueryCacheOptions();

        Assert.AreEqual(QueryCacheOptions.Defaults.BatchSize, options.BatchSize);
        Assert.AreEqual(QueryCacheOptions.Defaults.BufferSize, options.BufferSize);
        Assert.AreEqual(QueryCacheOptions.Defaults.DelaySeconds, options.DelaySeconds);
        Assert.AreEqual(QueryCacheOptions.Defaults.IncludeValue, options.IncludeValue);
        Assert.AreEqual(QueryCacheOptions.Defaults.Populate, options.Populate);
        Assert.AreEqual(QueryCacheOptions.Defaults.Coalesce, options.Coalesce);
        Assert.AreEqual(QueryCacheOptions.Defaults.SerializeKeys, options.SerializeKeys);
        Assert.AreEqual(QueryCacheOptions.Defaults.InMemoryFormat, options.InMemoryFormat);
    }

    [Test]
    public void Constructor_WithName_SetsName()
    {
        var options = new QueryCacheOptions("TestName");

        Assert.AreEqual("TestName", options.Name);
    }

    [Test]
    public void Constructor_WithQueryCacheOptionsArgument_CopiesProperties()
    {
        var originalOptions = new QueryCacheOptions("TestName");
        var copiedOptions = new QueryCacheOptions(originalOptions);

        Assert.AreEqual(originalOptions.Name, copiedOptions.Name);
        Assert.AreEqual(originalOptions.BatchSize, copiedOptions.BatchSize);
        Assert.AreEqual(originalOptions.BufferSize, copiedOptions.BufferSize);
        Assert.AreEqual(originalOptions.DelaySeconds, copiedOptions.DelaySeconds);
        Assert.AreEqual(originalOptions.IncludeValue, copiedOptions.IncludeValue);
        Assert.AreEqual(originalOptions.Populate, copiedOptions.Populate);
        Assert.AreEqual(originalOptions.Coalesce, copiedOptions.Coalesce);
        Assert.AreEqual(originalOptions.SerializeKeys, copiedOptions.SerializeKeys);
        Assert.AreEqual(originalOptions.InMemoryFormat, copiedOptions.InMemoryFormat);
    }

    [Test]
    public void ToString_ReturnsExpectedFormat()
    {
        var options = new QueryCacheOptions("TestName");

        var result = options.ToString();

        StringAssert.Contains("batchSize=", result);
        StringAssert.Contains("bufferSize=", result);
        StringAssert.Contains("delaySeconds=", result);
        StringAssert.Contains("includeValue=", result);
        StringAssert.Contains("populate=", result);
        StringAssert.Contains("coalesce=", result);
        StringAssert.Contains("serializeKeys=", result);
        StringAssert.Contains("inMemoryFormat=", result);
        StringAssert.Contains("name='TestName'", result);
    }
}
}
