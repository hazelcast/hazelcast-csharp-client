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
public class CacheSimpleEntryListenerOptionsTests
{
    [Test]
    public void DefaultConstructor_SetsPropertiesToDefault()
    {
        var options = new CacheSimpleEntryListenerOptions();

        Assert.IsNull(options.CacheEntryListenerFactory);
        Assert.IsNull(options.CacheEntryEventFilterFactory);
        Assert.IsFalse(options.OldValueRequired);
        Assert.IsFalse(options.Synchronous);
    }

    [Test]
    public void Constructor_WithCacheSimpleEntryListenerOptionsArgument_CopiesProperties()
    {
        var originalOptions = new CacheSimpleEntryListenerOptions
        {
            CacheEntryListenerFactory = "TestFactory",
            CacheEntryEventFilterFactory = "TestFilterFactory",
            OldValueRequired = true,
            Synchronous = true
        };
        var copiedOptions = new CacheSimpleEntryListenerOptions(originalOptions);

        Assert.AreEqual(originalOptions.CacheEntryListenerFactory, copiedOptions.CacheEntryListenerFactory);
        Assert.AreEqual(originalOptions.CacheEntryEventFilterFactory, copiedOptions.CacheEntryEventFilterFactory);
        Assert.AreEqual(originalOptions.OldValueRequired, copiedOptions.OldValueRequired);
        Assert.AreEqual(originalOptions.Synchronous, copiedOptions.Synchronous);
    }

    [Test]
    public void ToString_ReturnsExpectedFormat()
    {
        var options = new CacheSimpleEntryListenerOptions
        {
            CacheEntryListenerFactory = "TestFactory",
            CacheEntryEventFilterFactory = "TestFilterFactory",
            OldValueRequired = true,
            Synchronous = true
        };

        var result = options.ToString();

        StringAssert.Contains("cacheEntryListenerFactory='TestFactory'", result);
        StringAssert.Contains("cacheEntryEventFilterFactory='TestFilterFactory'", result);
        StringAssert.Contains("oldValueRequired=True", result);
        StringAssert.Contains("synchronous=True", result);
    }
}
}
