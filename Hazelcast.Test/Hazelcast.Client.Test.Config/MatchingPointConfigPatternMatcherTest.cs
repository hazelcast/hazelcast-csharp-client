// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
        public void TestMapConfigWildcardMultipleAmbiguousConfigs()
        {
            const string pattern1 = "com.hazelcast*";
            const string pattern2 = "com.hazelcast*";
            var config = new Configuration {ConfigPatternMatcher = new MatchingPointConfigPatternMatcher()};
            config.ConfigureNearCache(pattern1, ncCfg => { });
            config.ConfigureNearCache(pattern2, ncCfg => { });

            config.GetNearCacheConfig("com.hazelcast");
        }

        [Test]
        public void TestDuplicateConfig()
        {
            Assert.Throws<InvalidConfigurationException>(() =>
            {
                const string pattern1 = "com.hazelcast.*ap";
                const string pattern2 = "com.hazelcast*map";
                var config = new Configuration {ConfigPatternMatcher = new MatchingPointConfigPatternMatcher()};
                config.ConfigureNearCache(pattern1, ncCfg => { });
                config.ConfigureNearCache(pattern2, ncCfg => { });

                config.GetNearCacheConfig("com.hazelcast.map");
            });
        }

        [Test]
        public void TestNearCacheConfigWildcard(
            [Values("*hazelcast.test.myNearCache", "com.hazelcast.*.myNearCache", "com.hazelcast.test.*")]
            string pattern1)
        {
            var nearCacheConfig = new NearCacheConfig {Name = pattern1};

            var config = new Configuration {ConfigPatternMatcher = new MatchingPointConfigPatternMatcher()};
            config.ConfigureNearCache(pattern1, ncCfg => { });
            Assert.IsTrue(Equals(nearCacheConfig, config.GetNearCacheConfig("com.hazelcast.test.myNearCache")));
        }

        [Test]
        public void TestNearCacheConfigWildcardMatchingPointEndsWith()
        {
            var config = new Configuration {ConfigPatternMatcher = new MatchingPointConfigPatternMatcher()};
            config.ConfigureNearCache("*.sub", ncCfg => { });
            config.ConfigureNearCache("*.test.sub", ncCfg => { });
            config.ConfigureNearCache("*.hazelcast.test.sub", ncCfg => { });
            // we should not match any of the configs (endsWith)
            Assert.AreEqual(null, config.GetNearCacheConfig("com.hazelFast.Fast.sub.myNearCache"));
            Assert.AreEqual(null, config.GetNearCacheConfig("hazelFast.test.sub.myNearCache"));
            Assert.AreEqual(null, config.GetNearCacheConfig("test.sub.myNearCache"));
        }

        [Test]
        public void TestNearCacheConfigWildcardMatchingPointStartsWith()
        {
            var config = new Configuration {ConfigPatternMatcher = new MatchingPointConfigPatternMatcher()};
            config.ConfigureNearCache("hazelcast.*", ncCfg => { });
            config.ConfigureNearCache("hazelcast.test.*", ncCfg => { });
            config.ConfigureNearCache("hazelcast.test.sub.*", ncCfg => { });
            // we should not match any of the configs (startsWith)
            Assert.AreEqual(null, config.GetNearCacheConfig("com.hazelcast.myNearCache"));
            Assert.AreEqual(null, config.GetNearCacheConfig("com.hazelcast.test.myNearCache"));
            Assert.AreEqual(null, config.GetNearCacheConfig("com.hazelcast.test.sub.myNearCache"));
        }

        [Test]
        public void TestNearCacheConfigWildcardMultipleConfigs()
        {
            var nearCacheConfig1 = new NearCacheConfig {Name = "com.hazelcast.*"};
            var nearCacheConfig2 = new NearCacheConfig {Name = "com.hazelcast.test.*"};
            var nearCacheConfig3 = new NearCacheConfig {Name = "com.hazelcast.test.sub.*"};
            var config = new Configuration {ConfigPatternMatcher = new MatchingPointConfigPatternMatcher()};
            config.NearCacheConfigs.Add(nearCacheConfig1.Name, nearCacheConfig1);
            config.NearCacheConfigs.Add(nearCacheConfig2.Name, nearCacheConfig2);
            config.NearCacheConfigs.Add(nearCacheConfig3.Name, nearCacheConfig3);
            // we should get the best matching result
            Assert.AreEqual(nearCacheConfig1, config.GetNearCacheConfig("com.hazelcast.myNearCache"));
            Assert.AreEqual(nearCacheConfig2, config.GetNearCacheConfig("com.hazelcast.test.myNearCache"));
            Assert.AreEqual(nearCacheConfig3, config.GetNearCacheConfig("com.hazelcast.test.sub.myNearCache"));
        }

        [Test]
        public void TestNearCacheConfigWildcardOnly()
        {
            var nearCacheConfig = new NearCacheConfig {Name = "*"};
            var config = new Configuration {ConfigPatternMatcher = new MatchingPointConfigPatternMatcher()};
            config.NearCacheConfigs.Add(nearCacheConfig.Name, nearCacheConfig);
            Assert.AreEqual(nearCacheConfig, config.GetNearCacheConfig("com.hazelcast.myNearCache"));
        }

        [Test]
        public void TestNearCacheConfigWildcardOnlyMultipleConfigs()
        {
            var nearCacheConfig1 = new NearCacheConfig {Name = "*"};
            var nearCacheConfig2 = new NearCacheConfig {Name = "com.hazelcast.*"};
            var config = new Configuration {ConfigPatternMatcher = new MatchingPointConfigPatternMatcher()};
            config.NearCacheConfigs.Add(nearCacheConfig1.Name, nearCacheConfig1);
            config.NearCacheConfigs.Add(nearCacheConfig2.Name, nearCacheConfig2);
            // we should get the best matching result
            Assert.AreEqual(nearCacheConfig2, config.GetNearCacheConfig("com.hazelcast.myNearCache"));
        }

        [Test]
        public void TestNearCacheConfigWithoutWildcard()
        {
            var nearCacheConfig = new NearCacheConfig {Name = "someNearCache"};
            var config = new Configuration {ConfigPatternMatcher = new MatchingPointConfigPatternMatcher()};

            config.NearCacheConfigs.Add(nearCacheConfig.Name, nearCacheConfig);
            Assert.AreEqual(nearCacheConfig, config.GetNearCacheConfig("someNearCache"));
            // non-matching name
            Assert.AreNotEqual(nearCacheConfig, config.GetNearCacheConfig("doesNotExist"));
            // non-matching case
            Assert.AreNotEqual(nearCacheConfig, config.GetNearCacheConfig("SomeNearCache"));
        }
        
        private static bool Equals(NearCacheConfig first, NearCacheConfig other)
        {
            return first.EvictionPolicy == other.EvictionPolicy && first.InMemoryFormat == other.InMemoryFormat &&
                   first.MaxIdleSeconds == other.MaxIdleSeconds && first.MaxSize == other.MaxSize && first.Name == other.Name &&
                   first.TimeToLiveSeconds == other.TimeToLiveSeconds && first.InvalidateOnChange == other.InvalidateOnChange &&
                   first.SerializeKeys == other.SerializeKeys;
        }

    }
}