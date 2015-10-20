using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Hazelcast.Config;
using Hazelcast.Net.Ext;
using Hazelcast.Security;
using NUnit.Framework;
using Properties;

namespace Hazelcast.Client.Test.Config
{
    [TestFixture]
    public class ClientXmlConfigTest
    {
        private ClientConfig _clientConfig;

        [SetUp]
        public void ReadConfig()
        {
            _clientConfig = XmlClientConfigBuilder.Build(new StringReader(Resources.hazelcast_config_full));
        }

        [Test]
        public void TestNetworkConfig()
        {
            Assert.That(_clientConfig.GetNetworkConfig().GetAddresses(), Contains.Item("127.0.0.1"));
            Assert.That(_clientConfig.GetNetworkConfig().GetAddresses(), Contains.Item("127.0.0.2"));
            Assert.That(_clientConfig.GetNetworkConfig().IsSmartRouting(), Is.True);
            Assert.That(_clientConfig.GetNetworkConfig().IsRedoOperation(), Is.True);
            Assert.That(_clientConfig.GetNetworkConfig().GetConnectionAttemptLimit(), Is.EqualTo(2));
            Assert.That(_clientConfig.GetNetworkConfig().GetConnectionAttemptPeriod(), Is.EqualTo(3000));
            Assert.That(_clientConfig.GetNetworkConfig().GetConnectionTimeout(), Is.EqualTo(60000));

            var socketInterceptorConfig = _clientConfig.GetNetworkConfig().GetSocketInterceptorConfig();
            Assert.That(socketInterceptorConfig, Is.Not.Null);
            Assert.That(socketInterceptorConfig.IsEnabled(), Is.True);
            Assert.That(socketInterceptorConfig.GetClassName(), Is.EqualTo("com.hazelcast.examples.MySocketInterceptor"));
            Assert.That(socketInterceptorConfig.GetProperty("foo"), Is.EqualTo("bar"));

            var socketOptions = _clientConfig.GetNetworkConfig().GetSocketOptions();

            Assert.That(socketOptions, Is.Not.Null);
            Assert.That(socketOptions.GetTimeout(), Is.EqualTo(-1));
            Assert.That(socketOptions.GetLingerSeconds(), Is.EqualTo(3));
            Assert.That(socketOptions.GetBufferSize(), Is.EqualTo(32));
            Assert.That(socketOptions.IsKeepAlive(), Is.True);
            Assert.That(socketOptions.IsTcpNoDelay(), Is.False);
            Assert.That(socketOptions.IsReuseAddress(), Is.True);
        }

        [Test]
        public void TestGroupConfig()
        {
            Assert.That(_clientConfig.GetGroupConfig(), Is.EqualTo(new GroupConfig("dev", "dev-pass")));
        }

        [Test]
        public void TestSecurity()
        {
            Assert.That(_clientConfig.GetCredentials(), Is.InstanceOf(typeof(UsernamePasswordCredentials)));

            var credentials = (UsernamePasswordCredentials) _clientConfig.GetCredentials();

            Assert.That(credentials.GetUsername(), Is.EqualTo("dev-user"));
            Assert.That(credentials.GetPassword(), Is.EqualTo("pass123"));
        }

        [Test]
        public void TestNearCacheConfig()
        {
            var nearCacheConfig = _clientConfig.GetNearCacheConfig("asd");
            Assert.That(nearCacheConfig, Is.Not.Null);

            Assert.That(nearCacheConfig.GetName(), Is.EqualTo("asd"));
            Assert.That(nearCacheConfig.GetMaxSize(), Is.EqualTo(2000));
            Assert.That(nearCacheConfig.GetTimeToLiveSeconds(), Is.EqualTo(100));
            Assert.That(nearCacheConfig.GetMaxIdleSeconds(), Is.EqualTo(100));
            Assert.That(nearCacheConfig.GetEvictionPolicy(), Is.EqualTo("LFU"));
            Assert.That(nearCacheConfig.IsInvalidateOnChange(), Is.True);
            Assert.That(nearCacheConfig.GetInMemoryFormat(), Is.EqualTo(InMemoryFormat.Object));
        }

        [Test]
        public void TestProxyFactoryConfig()
        {
            var proxyFactoryConfigs = _clientConfig.GetProxyFactoryConfigs();
            Assert.That(proxyFactoryConfigs, Has.Count.EqualTo(3));

            Assert.That(proxyFactoryConfigs[0].GetClassName(), Is.EqualTo("com.hazelcast.examples.ProxyXYZ1"));
            Assert.That(proxyFactoryConfigs[1].GetClassName(), Is.EqualTo("com.hazelcast.examples.ProxyXYZ2"));
            Assert.That(proxyFactoryConfigs[2].GetClassName(), Is.EqualTo("com.hazelcast.examples.ProxyXYZ3"));

            Assert.That(proxyFactoryConfigs[0].GetService(), Is.EqualTo("sampleService1"));
            Assert.That(proxyFactoryConfigs[1].GetService(), Is.EqualTo("sampleService2"));
            Assert.That(proxyFactoryConfigs[2].GetService(), Is.EqualTo("sampleService3"));
        }

        [Test]
        public void TestListenerConfig()
        {
            var listenerConfigs = _clientConfig.GetListenerConfigs();

            Assert.That(listenerConfigs, Has.Count.EqualTo(3));
            Assert.That(listenerConfigs[0].GetClassName(), Is.EqualTo("Hazelcast.Examples.MembershipListener"));
            Assert.That(listenerConfigs[1].GetClassName(), Is.EqualTo("Hazelcast.Examples.InstanceListener"));
            Assert.That(listenerConfigs[2].GetClassName(), Is.EqualTo("Hazelcast.Examples.MigrationListener"));
        }

        [Test]
        public void TestSerializationConfig()
        {
            var serializationConfig = _clientConfig.GetSerializationConfig();
            Assert.That(serializationConfig.GetPortableVersion(), Is.EqualTo(3));

            var dsClasses = serializationConfig.GetDataSerializableFactoryClasses();
            Assert.That(dsClasses, Has.Count.EqualTo(1));
            Assert.That(dsClasses[1], Is.EqualTo("com.hazelcast.examples.DataSerializableFactory"));

            var pfClasses = serializationConfig.GetPortableFactoryClasses();
            Assert.That(pfClasses, Has.Count.EqualTo(1));
            Assert.That(pfClasses[1], Is.EqualTo("com.hazelcast.examples.PortableFactory"));

            var serializerConfigs = serializationConfig.GetSerializerConfigs();
            Assert.That(serializerConfigs, Has.Count.EqualTo(1));
            var serializerConfig = serializerConfigs.First();

            Assert.AreEqual("com.hazelcast.examples.DummyType", serializerConfig.GetTypeClassName());
            Assert.AreEqual("com.hazelcast.examples.SerializerFactory", serializerConfig.GetClassName());

            var globalSerializerConfig = serializationConfig.GetGlobalSerializerConfig();
            Assert.AreEqual("com.hazelcast.examples.GlobalSerializerFactory", globalSerializerConfig.GetClassName());

            Assert.AreEqual(ByteOrder.BigEndian, serializationConfig.GetByteOrder());
            Assert.AreEqual(true, serializationConfig.IsCheckClassDefErrors());
            Assert.AreEqual(true, serializationConfig.IsEnableCompression());
            Assert.AreEqual(true, serializationConfig.IsEnableSharedObject());
            Assert.AreEqual(true, serializationConfig.IsUseNativeByteOrder());

        }

    }
}
