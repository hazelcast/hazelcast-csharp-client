using System;
using Hazelcast.Config;
using NUnit.Framework;

namespace Hazelcast.Client.Test.Config
{
    [TestFixture]
    public class ClientNearCacheConfigTest
    {
        [Test]
        public virtual void TestSpecificNearCacheConfig_whenAsteriskAtTheBeginning()
        {
            var clientConfig = new ClientConfig();
            var genericNearCacheConfig = new NearCacheConfig();
            genericNearCacheConfig.SetName("*Map");
            clientConfig.AddNearCacheConfig(genericNearCacheConfig);
            var specificNearCacheConfig = new NearCacheConfig();
            specificNearCacheConfig.SetName("*MapStudent");
            clientConfig.AddNearCacheConfig(specificNearCacheConfig);
            var mapFoo = clientConfig.GetNearCacheConfig("fooMap");
            var mapStudentFoo = clientConfig.GetNearCacheConfig("fooMapStudent");
            Assert.AreEqual(genericNearCacheConfig, mapFoo);
            Assert.AreEqual(specificNearCacheConfig, mapStudentFoo);
        }

        [Test]
        public virtual void TestSpecificNearCacheConfig_whenAsteriskAtTheEnd()
        {
            var clientConfig = new ClientConfig();
            var genericNearCacheConfig = new NearCacheConfig();
            genericNearCacheConfig.SetName("map*");
            clientConfig.AddNearCacheConfig(genericNearCacheConfig);
            var specificNearCacheConfig = new NearCacheConfig();
            specificNearCacheConfig.SetName("mapStudent*");
            clientConfig.AddNearCacheConfig(specificNearCacheConfig);
            var mapFoo = clientConfig.GetNearCacheConfig("mapFoo");
            var mapStudentFoo = clientConfig.GetNearCacheConfig("mapStudentFoo");
            Assert.AreEqual(genericNearCacheConfig, mapFoo);
            Assert.AreEqual(specificNearCacheConfig, mapStudentFoo);
        }

        [Test]
        public virtual void TestSpecificNearCacheConfig_whenAsteriskInTheMiddle()
        {
            var clientConfig = new ClientConfig();
            var genericNearCacheConfig = new NearCacheConfig();
            genericNearCacheConfig.SetName("map*Bar");
            clientConfig.AddNearCacheConfig(genericNearCacheConfig);
            var specificNearCacheConfig = new NearCacheConfig();
            specificNearCacheConfig.SetName("mapStudent*Bar");
            clientConfig.AddNearCacheConfig(specificNearCacheConfig);
            var mapFoo = clientConfig.GetNearCacheConfig("mapFooBar");
            var mapStudentFoo = clientConfig.GetNearCacheConfig("mapStudentFooBar");
            Assert.AreEqual(genericNearCacheConfig, mapFoo);
            Assert.AreEqual(specificNearCacheConfig, mapStudentFoo);
        }

        [Test]
        public void TestReadOnlyNearCacheConfig()
        {
            var config = new NearCacheConfig();
            var readOnly = config.GetAsReadOnly();

            var actions = new Action[]
            {
                () => readOnly.SetEvictionPolicy(TestSupport.RandomString()),
                () => readOnly.SetName(TestSupport.RandomString()),
                () => readOnly.SetInMemoryFormat(TestSupport.RandomString()),
                () => readOnly.SetInMemoryFormat(InMemoryFormat.Binary),
                () => readOnly.SetInvalidateOnChange(true),
                () => readOnly.SetMaxIdleSeconds(TestSupport.RandomInt()),
                () => readOnly.SetMaxSize(TestSupport.RandomInt()),
                () => readOnly.SetTimeToLiveSeconds(TestSupport.RandomInt())
            };

            foreach (var action in actions)
            {
                try
                {
                    action();
                    Assert.Fail("The config was not readonly.");
                }
                catch (NotSupportedException)
                {
                }
            }
        }
    }
}