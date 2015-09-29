using Hazelcast.Config;
using NUnit.Framework;

namespace Hazelcast.Client.Test.Config
{
    [TestFixture]
    public class MatchingPointConfigPatternMatcherTest
    {
        public virtual void TestMapConfigWildcardMultipleAmbiguousConfigs()
        {
            NearCacheConfig nearCacheConfig1 = new NearCacheConfig().SetName("com.hazelcast*");
            NearCacheConfig nearCacheConfig2 = new NearCacheConfig().SetName("*com.hazelcast");
            ClientConfig config = new ClientConfig();
            config.SetConfigPatternMatcher(new MatchingPointConfigPatternMatcher());
            config.AddNearCacheConfig(nearCacheConfig1);
            config.AddNearCacheConfig(nearCacheConfig2);
            config.GetNearCacheConfig("com.hazelcast");
        }

        [Test]
        public virtual void TestNearCacheConfigWildcard1()
        {
            NearCacheConfig nearCacheConfig = new NearCacheConfig().SetName("*hazelcast.test.myNearCache");
            ClientConfig config = new ClientConfig();
            config.SetConfigPatternMatcher(new MatchingPointConfigPatternMatcher());
            config.AddNearCacheConfig(nearCacheConfig);
            Assert.AreEqual(nearCacheConfig, config.GetNearCacheConfig("com.hazelcast.test.myNearCache"));
        }

        [Test]
        public virtual void TestNearCacheConfigWildcard2()
        {
            NearCacheConfig nearCacheConfig = new NearCacheConfig().SetName("com.hazelcast.*.myNearCache");
            ClientConfig config = new ClientConfig();
            config.SetConfigPatternMatcher(new MatchingPointConfigPatternMatcher());
            config.AddNearCacheConfig(nearCacheConfig);
            Assert.AreEqual(nearCacheConfig, config.GetNearCacheConfig("com.hazelcast.test.myNearCache"));
        }

        [Test]
        public virtual void TestNearCacheConfigWildcard3()
        {
            NearCacheConfig nearCacheConfig = new NearCacheConfig().SetName("com.hazelcast.test.*");
            ClientConfig config = new ClientConfig();
            config.SetConfigPatternMatcher(new MatchingPointConfigPatternMatcher());
            config.AddNearCacheConfig(nearCacheConfig);
            Assert.AreEqual(nearCacheConfig, config.GetNearCacheConfig("com.hazelcast.test.myNearCache"));
        }

        [Test]
        public virtual void TestNearCacheConfigWildcardMatchingPointEndsWith()
        {
            NearCacheConfig nearCacheConfig1 = new NearCacheConfig().SetName("*.sub");
            NearCacheConfig nearCacheConfig2 = new NearCacheConfig().SetName("*.test.sub");
            NearCacheConfig nearCacheConfig3 = new NearCacheConfig().SetName("*.hazelcast.test.sub");
            ClientConfig config = new ClientConfig();
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
            NearCacheConfig nearCacheConfig1 = new NearCacheConfig().SetName("hazelcast.*");
            NearCacheConfig nearCacheConfig2 = new NearCacheConfig().SetName("hazelcast.test.*");
            NearCacheConfig nearCacheConfig3 = new NearCacheConfig().SetName("hazelcast.test.sub.*");
            ClientConfig config = new ClientConfig();
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
            NearCacheConfig nearCacheConfig1 = new NearCacheConfig().SetName("com.hazelcast.*");
            NearCacheConfig nearCacheConfig2 = new NearCacheConfig().SetName("com.hazelcast.test.*");
            NearCacheConfig nearCacheConfig3 = new NearCacheConfig().SetName("com.hazelcast.test.sub.*");
            ClientConfig config = new ClientConfig();
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
            NearCacheConfig nearCacheConfig = new NearCacheConfig().SetName("*");
            ClientConfig config = new ClientConfig();
            config.SetConfigPatternMatcher(new MatchingPointConfigPatternMatcher());
            config.AddNearCacheConfig(nearCacheConfig);
            Assert.AreEqual(nearCacheConfig, config.GetNearCacheConfig("com.hazelcast.myNearCache"));
        }

        [Test]
        public virtual void TestNearCacheConfigWildcardOnlyMultipleConfigs()
        {
            NearCacheConfig nearCacheConfig1 = new NearCacheConfig().SetName("*");
            NearCacheConfig nearCacheConfig2 = new NearCacheConfig().SetName("com.hazelcast.*");
            ClientConfig config = new ClientConfig();
            config.SetConfigPatternMatcher(new MatchingPointConfigPatternMatcher());
            config.AddNearCacheConfig(nearCacheConfig1);
            config.AddNearCacheConfig(nearCacheConfig2);
            // we should get the best matching result
            Assert.AreEqual(nearCacheConfig2, config.GetNearCacheConfig("com.hazelcast.myNearCache"));
        }

        [Test]
        public virtual void TestNearCacheConfigWithoutWildcard()
        {
            NearCacheConfig nearCacheConfig = new NearCacheConfig().SetName("someNearCache");
            ClientConfig config = new ClientConfig();
            config.SetConfigPatternMatcher(new MatchingPointConfigPatternMatcher());
            config.AddNearCacheConfig(nearCacheConfig);
            Assert.AreEqual(nearCacheConfig, config.GetNearCacheConfig("someNearCache"));
            // non-matching name
            Assert.AreNotEqual(nearCacheConfig, config.GetNearCacheConfig("doesNotExist"));
            // non-matching case
            Assert.AreNotEqual(nearCacheConfig, config.GetNearCacheConfig("SomeNearCache"));
        }

        [Test, ExpectedException(typeof(ConfigurationException))]
        public virtual void TestDuplicateConfig()
        {
            NearCacheConfig nearCacheConfig1 = new NearCacheConfig().SetName("com.hazelcast.*ap");
            NearCacheConfig nearCacheConfig2 = new NearCacheConfig().SetName("com.hazelcast*map");
            ClientConfig config = new ClientConfig();
            config.SetConfigPatternMatcher(new MatchingPointConfigPatternMatcher());
            config.AddNearCacheConfig(nearCacheConfig1);
            config.AddNearCacheConfig(nearCacheConfig2);

            config.GetNearCacheConfig("com.hazelcast.map");
        }
    }
}