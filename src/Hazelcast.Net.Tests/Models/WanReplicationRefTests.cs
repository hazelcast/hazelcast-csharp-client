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
using System.Collections.Generic;
using Hazelcast.Models;
using NUnit.Framework;
namespace Hazelcast.Tests.Models
{
    public class WanReplicationRefTests
    {
        [Test]
        public void Constructor_WithValidParameters_InitializesPropertiesCorrectly()
        {
            var name = "TestName";
            var mergePolicyClassName = "MergePolicy";
            var filters = new List<string> { "Filter1", "Filter2" };
            var republishingEnabled = true;

            var wanReplicationRef = new WanReplicationRef(name, mergePolicyClassName, filters, republishingEnabled);

            Assert.AreEqual(name, wanReplicationRef.Name);
            Assert.AreEqual(mergePolicyClassName, wanReplicationRef.MergePolicyClassName);
            Assert.AreEqual(filters, wanReplicationRef.Filters);
            Assert.AreEqual(republishingEnabled, wanReplicationRef.RepublishingEnabled);
        }

        [Test]
        public void AddFilter_WithValidFilter_AddsFilterToList()
        {
            var wanReplicationRef = new WanReplicationRef();
            var filter = "TestFilter";

            wanReplicationRef.AddFilter(filter);

            Assert.Contains(filter, wanReplicationRef.Filters);
        }


        [Test]
        public void ToString_ReturnsCorrectFormat()
        {
            var name = "TestName";
            var mergePolicyClassName = "MergePolicy";
            var filters = new List<string> { "Filter1", "Filter2" };
            var republishingEnabled = true;
            var wanReplicationRef = new WanReplicationRef(name, mergePolicyClassName, filters, republishingEnabled);

            var expectedString = $"WanReplicationRef{{name='{name}', mergePolicy='{mergePolicyClassName}', filters='{string.Join(",", filters)}', republishingEnabled='{republishingEnabled}'}}";

            Assert.AreEqual(expectedString, wanReplicationRef.ToString());
        }
    }
}
