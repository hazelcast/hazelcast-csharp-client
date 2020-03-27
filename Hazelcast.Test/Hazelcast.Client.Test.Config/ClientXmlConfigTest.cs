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

using System.IO;
using System.Linq;
using Hazelcast.Config;
using Hazelcast.Net.Ext;
using Hazelcast.Security;
using Hazelcast.Test;
using NUnit.Framework;

namespace Hazelcast.Client.Test.Config
{
    [TestFixture]
    public class ClientXmlConfigTest
    {
        [SetUp]
        public void ReadConfig()
        {
            _clientConfig = XmlClientConfigBuilder.Build(new StringReader(Resources.HazelcastConfigFull));
        }

        private Configuration _clientConfig;

        [Test]
        public void TestClusterName()
        {
            Assert.AreEqual("dev", _clientConfig.ClusterName);
        }

        [Test]
        public void TestListenerConfig()
        {
            var listenerConfigs = _clientConfig.ListenerConfigs;

            Assert.That(listenerConfigs, Has.Count.EqualTo(2));
            Assert.That(listenerConfigs[0].TypeName, Is.EqualTo("Hazelcast.Examples.MembershipListener"));
            Assert.That(listenerConfigs[1].TypeName, Is.EqualTo("Hazelcast.Examples.MigrationListener"));
        }

        [Test]
        public void TestNearCacheConfig()
        {
            var nearCacheConfig = _clientConfig.GetNearCacheConfig("asd");
            Assert.That(nearCacheConfig, Is.Not.Null);

            Assert.That(nearCacheConfig.Name, Is.EqualTo("asd"));
            Assert.That(nearCacheConfig.MaxSize, Is.EqualTo(2000));
            Assert.That(nearCacheConfig.TimeToLiveSeconds, Is.EqualTo(100));
            Assert.That(nearCacheConfig.MaxIdleSeconds, Is.EqualTo(100));
            Assert.That(nearCacheConfig.EvictionPolicy, Is.EqualTo(EvictionPolicy.Lfu));
            Assert.That(nearCacheConfig.InvalidateOnChange, Is.True);
            Assert.That(nearCacheConfig.InMemoryFormat, Is.EqualTo(InMemoryFormat.Object));
        }

        [Test]
        public void TestNetworkConfig()
        {
            Assert.That(_clientConfig.NetworkConfig.Addresses, Contains.Item("127.0.0.1"));
            Assert.That(_clientConfig.NetworkConfig.Addresses, Contains.Item("127.0.0.2"));
            Assert.That(_clientConfig.NetworkConfig.SmartRouting, Is.True);
            Assert.That(_clientConfig.NetworkConfig.RedoOperation, Is.True);
            Assert.That(_clientConfig.NetworkConfig.ConnectionTimeout, Is.EqualTo(60000));

            //TODO remove socket interceptor
            // var socketInterceptorConfig = _clientConfig.NetworkConfig.SocketInterceptorConfig;
            // Assert.That(socketInterceptorConfig, Is.Not.Null);
            // Assert.That(socketInterceptorConfig.IsEnabled(), Is.True);
            // Assert.That(socketInterceptorConfig.GetClassName(), Is.EqualTo("Hazelcast.Examples.MySocketInterceptor"));
            // Assert.That(socketInterceptorConfig.GetProperty("foo"), Is.EqualTo("bar"));

            var socketOptions = _clientConfig.NetworkConfig.SocketOptions;

            Assert.That(socketOptions, Is.Not.Null);
            Assert.That(socketOptions.LingerSeconds, Is.EqualTo(3));
            Assert.That(socketOptions.BufferSize, Is.EqualTo(128));
            Assert.That(socketOptions.KeepAlive, Is.True);
            Assert.That(socketOptions.TcpNoDelay, Is.False);
            Assert.That(socketOptions.ReuseAddress, Is.True);
        }

        
        [Test]
        public void TestCloudConfig() 
        {
            var cloudConfig = _clientConfig.NetworkConfig.HazelcastCloudConfig;
            Assert.That(cloudConfig, Is.Not.Null);
            Assert.That(cloudConfig.Enabled, Is.False);
            Assert.That(cloudConfig.DiscoveryToken, Is.EqualTo("EXAMPLE_TOKEN"));
        }

        // [Test]
        // public void TestProxyFactoryConfig()
        // {
        //     var proxyFactoryConfigs = _clientConfig.GetProxyFactoryConfigs();
        //     Assert.That(proxyFactoryConfigs, Has.Count.EqualTo(3));
        //
        //     Assert.That(proxyFactoryConfigs[0].GetClassName(), Is.EqualTo("Hazelcast.Examples.ProxyXYZ1"));
        //     Assert.That(proxyFactoryConfigs[1].GetClassName(), Is.EqualTo("Hazelcast.Examples.ProxyXYZ2"));
        //     Assert.That(proxyFactoryConfigs[2].GetClassName(), Is.EqualTo("Hazelcast.Examples.ProxyXYZ3"));
        //
        //     Assert.That(proxyFactoryConfigs[0].GetService(), Is.EqualTo("sampleService1"));
        //     Assert.That(proxyFactoryConfigs[1].GetService(), Is.EqualTo("sampleService2"));
        //     Assert.That(proxyFactoryConfigs[2].GetService(), Is.EqualTo("sampleService3"));
        // }
        //
        // [Test]
        // public void TestSecurity()
        // {
        //     var credentialsClassName = _clientConfig.GetSecurityConfig().GetCredentialsClassName();
        //     var factoryClassName = _clientConfig.GetSecurityConfig().GetCredentialsFactoryConfig().GetClassName();
        //     var properties = _clientConfig.GetSecurityConfig().GetCredentialsFactoryConfig().GetProperties();
        //     
        //     Assert.That(credentialsClassName, Is.EqualTo("Hazelcast.Security.UsernamePasswordCredentials"));
        //     Assert.That(factoryClassName, Is.EqualTo("Hazelcast.Security.DefaultCredentialsFactory"));
        //
        //     Assert.That(properties.Count, Is.EqualTo(2));
        //     Assert.That(properties["username"], Is.EqualTo("dev-user"));
        //     Assert.That(properties["password"], Is.EqualTo("pass123"));
        // }

        [Test]
        public void TestSerializationConfig()
        {
            var serializationConfig = _clientConfig.SerializationConfig;
            Assert.That(serializationConfig.PortableVersion, Is.EqualTo(3));

            var dsClasses = serializationConfig.DataSerializableFactoryClasses;
            Assert.That(dsClasses, Has.Count.EqualTo(1));
            Assert.That(dsClasses[1], Is.EqualTo("Hazelcast.Examples.DataSerializableFactory"));

            var pfClasses = serializationConfig.PortableFactoryClasses;
            Assert.That(pfClasses, Has.Count.EqualTo(1));
            Assert.That(pfClasses[1], Is.EqualTo("Hazelcast.Examples.PortableFactory"));

            var serializerConfigs = serializationConfig.SerializerConfigs;
            Assert.That(serializerConfigs, Has.Count.EqualTo(1));
            var serializerConfig = serializerConfigs.First();

            Assert.AreEqual("Hazelcast.Examples.DummyType", serializerConfig.GetTypeClassName());
            Assert.AreEqual("Hazelcast.Examples.SerializerFactory", serializerConfig.GetClassName());

            var globalSerializerConfig = serializationConfig.GlobalSerializerConfig;
            Assert.AreEqual("Hazelcast.Examples.GlobalSerializerFactory", globalSerializerConfig.TypeName);

            Assert.AreEqual(ByteOrder.BigEndian, serializationConfig.ByteOrder);
            Assert.AreEqual(true, serializationConfig.CheckClassDefErrors);
            Assert.AreEqual(true, serializationConfig.UseNativeByteOrder);
        }
    }
}