// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Config;
using NUnit.Framework;

namespace Hazelcast.Client.Test.Config
{
    [TestFixture]
    public class MatchingPointConfigPatternMatcherTest
    {
        public virtual void TestMapConfigWildcardMultipleAmbiguousConfigs()
        {
            var nearCacheConfig1 = new NearCacheConfig().SetName("com.hazelcast*");
            var nearCacheConfig2 = new NearCacheConfig().SetName("*com.hazelcast");
            var config = new ClientConfig();
            config.SetConfigPatternMatcher(new MatchingPointConfigPatternMatcher());
            config.AddNearCacheConfig(nearCacheConfig1);
            config.AddNearCacheConfig(nearCacheConfig2);
            config.GetNearCacheConfig("com.hazelcast");
        }

        [Test]
        public void TestDuplicateConfig()
        {
            Assert.Throws<ConfigurationException>(() =>
            {
                var nearCacheConfig1 = new NearCacheConfig().SetName("com.hazelcast.*ap");
                var nearCacheConfig2 = new NearCacheConfig().SetName("com.hazelcast*map");
                var config = new ClientConfig();
                config.SetConfigPatternMatcher(new MatchingPointConfigPatternMatcher());
                config.AddNearCacheConfig(nearCacheConfig1);
                config.AddNearCacheConfig(nearCacheConfig2);

                config.GetNearCacheConfig("com.hazelcast.map");
            });
        }

        [Test]
        public virtual void TestNearCacheConfigWildcard1()
        {
            var nearCacheConfig = new NearCacheConfig().SetName("*hazelcast.test.myNearCache");
            var config = new ClientConfig();
            config.SetConfigPatternMatcher(new MatchingPointConfigPatternMatcher());
            config.AddNearCacheConfig(nearCacheConfig);
            Assert.AreEqual(nearCacheConfig, config.GetNearCacheConfig("com.hazelcast.test.myNearCache"));
        }

        [Test]
        public virtual void TestNearCacheConfigWildcard2()
        {
            var nearCacheConfig = new NearCacheConfig().SetName("com.hazelcast.*.myNearCache");
            var config = new ClientConfig();
            config.SetConfigPatternMatcher(new MatchingPointConfigPatternMatcher());
            config.AddNearCacheConfig(nearCacheConfig);
            Assert.AreEqual(nearCacheConfig, config.GetNearCacheConfig("com.hazelcast.test.myNearCache"));
        }

        [Test]
        public virtual void TestNearCacheConfigWildcard3()
        {
            var nearCacheConfig = new NearCacheConfig().SetName("com.hazelcast.test.*");
            var config = new ClientConfig();
            config.SetConfigPatternMatcher(new MatchingPointConfigPatternMatcher());
            config.AddNearCacheConfig(nearCacheConfig);
            Assert.AreEqual(nearCacheConfig, config.GetNearCacheConfig("com.hazelcast.test.myNearCache"));
        }

        [Test]
        public virtual void TestNearCacheConfigWildcardMatchingPointEndsWith()
        {
            var nearCacheConfig1 = new NearCacheConfig().SetName("*.sub");
            var nearCacheConfig2 = new NearCacheConfig().SetName("*.test.sub");
            var nearCacheConfig3 = new NearCacheConfig().SetName("*.hazelcast.test.sub");
            var config = new ClientConfig();
            config.SetConfigPatternMatcher(new MatchingPointConfigPatternMatcher());
            config.AddNearCacheConfig(nearCacheConfig1);
            config.AddNearCacheConfig(nearCacheConfig2);
            config.AddNearCacheConfig(nearCacheConfig3);
            // we should not match any of the configs (endsWith)
            Assert.AreEqual(null, config.GetNearCacheConfig("com.hazelFast.Fast.sub.myNearCache"));
            Assert.AreEqual(null, config.GetNearCacheConfig("hazelFast.test.sub.myNearCache"));
            Assert.AreEqual(null, config.GetNearCacheConfig("test.sub.myNearCache"));
        }

        [Test]
        public virtual void TestNearCacheConfigWildcardMatchingPointStartsWith()
        {
            var nearCacheConfig1 = new NearCacheConfig().SetName("hazelcast.*");
            var nearCacheConfig2 = new NearCacheConfig().SetName("hazelcast.test.*");
            var nearCacheConfig3 = new NearCacheConfig().SetName("hazelcast.test.sub.*");
            var config = new ClientConfig();
            config.SetConfigPatternMatcher(new MatchingPointConfigPatternMatcher());
            config.AddNearCacheConfig(nearCacheConfig1);
            config.AddNearCacheConfig(nearCacheConfig2);
            config.AddNearCacheConfig(nearCacheConfig3);
            // we should not match any of the configs (startsWith)
            Assert.AreEqual(null, config.GetNearCacheConfig("com.hazelcast.myNearCache"));
            Assert.AreEqual(null, config.GetNearCacheConfig("com.hazelcast.test.myNearCache"));
            Assert.AreEqual(null, config.GetNearCacheConfig("com.hazelcast.test.sub.myNearCache"));
        }

        [Test]
        public virtual void TestNearCacheConfigWildcardMultipleConfigs()
        {
            var nearCacheConfig1 = new NearCacheConfig().SetName("com.hazelcast.*");
            var nearCacheConfig2 = new NearCacheConfig().SetName("com.hazelcast.test.*");
            var nearCacheConfig3 = new NearCacheConfig().SetName("com.hazelcast.test.sub.*");
            var config = new ClientConfig();
            config.SetConfigPatternMatcher(new MatchingPointConfigPatternMatcher());
            config.AddNearCacheConfig(nearCacheConfig1);
            config.AddNearCacheConfig(nearCacheConfig2);
            config.AddNearCacheConfig(nearCacheConfig3);
            // we should get the best matching result
            Assert.AreEqual(nearCacheConfig1, config.GetNearCacheConfig("com.hazelcast.myNearCache"));
            Assert.AreEqual(nearCacheConfig2, config.GetNearCacheConfig("com.hazelcast.test.myNearCache"));
            Assert.AreEqual(nearCacheConfig3, config.GetNearCacheConfig("com.hazelcast.test.sub.myNearCache"));
        }

        [Test]
        public virtual void TestNearCacheConfigWildcardOnly()
        {
            var nearCacheConfig = new NearCacheConfig().SetName("*");
            var config = new ClientConfig();
            config.SetConfigPatternMatcher(new MatchingPointConfigPatternMatcher());
            config.AddNearCacheConfig(nearCacheConfig);
            Assert.AreEqual(nearCacheConfig, config.GetNearCacheConfig("com.hazelcast.myNearCache"));
        }

        [Test]
        public virtual void TestNearCacheConfigWildcardOnlyMultipleConfigs()
        {
            var nearCacheConfig1 = new NearCacheConfig().SetName("*");
            var nearCacheConfig2 = new NearCacheConfig().SetName("com.hazelcast.*");
            var config = new ClientConfig();
            config.SetConfigPatternMatcher(new MatchingPointConfigPatternMatcher());
            config.AddNearCacheConfig(nearCacheConfig1);
            config.AddNearCacheConfig(nearCacheConfig2);
            // we should get the best matching result
            Assert.AreEqual(nearCacheConfig2, config.GetNearCacheConfig("com.hazelcast.myNearCache"));
        }

        [Test]
        public virtual void TestNearCacheConfigWithoutWildcard()
        {
            var nearCacheConfig = new NearCacheConfig().SetName("someNearCache");
            var config = new ClientConfig();
            config.SetConfigPatternMatcher(new MatchingPointConfigPatternMatcher());
            config.AddNearCacheConfig(nearCacheConfig);
            Assert.AreEqual(nearCacheConfig, config.GetNearCacheConfig("someNearCache"));
            // non-matching name
            Assert.AreNotEqual(nearCacheConfig, config.GetNearCacheConfig("doesNotExist"));
            // non-matching case
            Assert.AreNotEqual(nearCacheConfig, config.GetNearCacheConfig("SomeNearCache"));
        }
    }
}