using System;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Clustering.LoadBalancing;
using Hazelcast.Core;
using Hazelcast.NearCaching;
using Hazelcast.Networking;
using Hazelcast.Security;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Hazelcast.Configuration.Binding;
using Hazelcast.Serialization;

namespace Hazelcast.Tests
{
    [TestFixture]
    public class ConfigurationTests
    {
        [Test]
        public void EmptyOptionsFile()
        {
            var json = Resources.Empty;

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            var builder = new ConfigurationBuilder();
            builder.AddJsonStream(stream);
            var configuration = builder.Build();

            var options = new HazelcastOptions();
            configuration.HzBind(HazelcastOptions.Hazelcast, options);

            Assert.AreEqual("dev", options.ClusterName);
        }

        [Test]
        public void EmptyOptionsFileWithComments()
        {
            var json = Resources.EmptyWithComments;

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            var builder = new ConfigurationBuilder();
            builder.AddJsonStream(stream);
            var configuration = builder.Build();

            var options = new HazelcastOptions();
            configuration.HzBind(HazelcastOptions.Hazelcast, options);

            Assert.AreEqual("dev", options.ClusterName);
        }

        private static HazelcastOptions ReadResource(string json)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            var builder = new ConfigurationBuilder();
            builder.AddJsonStream(stream);
            var configuration = builder.Build();

            var options = new HazelcastOptions();
            configuration.HzBind(HazelcastOptions.Hazelcast, options);

            return options;
        }

        [Test]
        public void HazelcastOptionsFile()
        {
            var options = ReadResource(Resources.HazelcastOptions);

            Assert.AreEqual("testClusterName", options.ClusterName);
            Assert.AreEqual("testClientName", options.ClientName);
            Assert.AreEqual(2, options.Properties.Count);
            Assert.IsTrue(options.Properties.ContainsKey("aKey"));
            Assert.IsTrue(options.Properties["aKey"] == "aValue");
            Assert.IsTrue(options.Properties.ContainsKey("anotherKey"));
            Assert.IsTrue(options.Properties["anotherKey"] == "anotherValue");
            Assert.AreEqual(2, options.Labels.Count);
            Assert.IsTrue(options.Labels.Contains("label1"));
            Assert.IsTrue(options.Labels.Contains("label2"));
            Assert.IsTrue(options.AsyncStart);
        }

        [Test]
        public async Task ClusterOptionsFile()
        {
            var options = ReadResource(Resources.ClusterOptions) as IClusterOptions;

            Assert.AreEqual(1, options.Subscribers.Count);
            var subscriber = options.Subscribers[0];
            Assert.IsInstanceOf<ClusterEventSubscriber>(subscriber);

            TestSubscriber.Ctored = false;
            await subscriber.SubscribeAsync(null, CancellationToken.None);
            Assert.IsTrue(TestSubscriber.Ctored);
        }

        public class TestSubscriber : IClusterEventSubscriber
        {
            public static bool Ctored { get; set; }

            public Task SubscribeAsync(Cluster cluster, CancellationToken cancellationToken)
            {
                Ctored = true;
                return Task.CompletedTask;
            }
        }

        [Test]
        public void LoggingOptionsFile()
        {
            var options = ReadResource(Resources.LoggingOptions).Logging;

            // TODO we should be able to configure logging?
        }

        [Test]
        public void NetworkingOptionsFile()
        {
            var options = ReadResource(Resources.NetworkingOptions).Network;

            Assert.AreEqual(2, options.Addresses.Count);
            Assert.IsTrue(options.Addresses.Contains("localhost"));
            Assert.IsTrue(options.Addresses.Contains("otherhost"));
            Assert.IsFalse(options.SmartRouting);
            Assert.IsFalse(options.RedoOperation);
            Assert.AreEqual(666, options.ConnectionTimeoutMilliseconds);
            Assert.AreEqual(ReconnectMode.DoNotReconnect, options.ReconnectMode);
            Assert.IsFalse(options.ShuffleAddresses);


            var sslOptions = options.Ssl;
            Assert.IsTrue(sslOptions.IsEnabled);
            Assert.IsFalse(sslOptions.ValidateCertificateChain);
            Assert.IsTrue(sslOptions.ValidateCertificateName);
            Assert.IsTrue(sslOptions.CheckCertificateRevocation);
            Assert.AreEqual("testCertificateName", sslOptions.CertificateName);
            Assert.AreEqual("testCertificatePath", sslOptions.CertificatePath);
            Assert.AreEqual("testCertificatePassword", sslOptions.CertificatePassword);
            Assert.AreEqual(SslProtocols.Tls11,sslOptions.SslProtocol);

            var cloudOptions = options.Cloud;
            Assert.IsTrue(cloudOptions.IsEnabled);
            Assert.AreEqual("testToken", cloudOptions.DiscoveryToken);
            Assert.AreEqual("testUrl", cloudOptions.UrlBase);

            var socketOptions = options.Socket;
            Assert.AreEqual(666, socketOptions.BufferSize);
            Assert.IsFalse(socketOptions.KeepAlive);
            Assert.AreEqual(667, socketOptions.LingerSeconds);
            Assert.IsFalse(socketOptions.ReuseAddress);
            Assert.IsTrue(socketOptions.TcpNoDelay);

            var interceptorOptions = options.SocketInterceptor;
            Assert.IsTrue(interceptorOptions.IsEnabled);
            Assert.IsInstanceOf<TestSocketInterceptor>(interceptorOptions.SocketInterceptor.Create());

            var retryOptions = options.ConnectionRetry;
            Assert.AreEqual(666, retryOptions.InitialBackoffMilliseconds);
            Assert.AreEqual(667, retryOptions.MaxBackoffMilliseconds);
            Assert.AreEqual(668, retryOptions.Multiplier);
            Assert.AreEqual(669, retryOptions.ClusterConnectionTimeoutMilliseconds);
            Assert.AreEqual(42, retryOptions.Jitter);
        }

        public class TestSocketInterceptor : ISocketInterceptor
        {
            public TestSocketInterceptor(SocketInterceptorOptions options)
            { }
        }

        [Test]
        public void AuthenticationOptionsFile()
        {
            var options = ReadResource(Resources.AuthenticationOptions).Authentication;

            Assert.AreEqual("Hazelcast.Clustering.Authenticator, Hazelcast.Net", options.AuthenticatorType);
            Assert.AreEqual(2, options.AuthenticatorArgs.Count);
            Assert.IsTrue(options.AuthenticatorArgs.TryGetValue("arg3", out var arg3) && arg3 is string s3 && s3 == "value3");
            Assert.IsTrue(options.AuthenticatorArgs.TryGetValue("arg4", out var arg4) && arg4 is string s4 && s4 == "value4");

            var authenticator = options.Authenticator.Create();
            Assert.IsInstanceOf<Authenticator>(authenticator);
        }

        [Test]
        public void SecurityOptionsFile()
        {
            var options = ReadResource(Resources.SecurityOptions).Security;

            Assert.AreEqual("Hazelcast.Security.DefaultCredentialsFactory, Hazelcast.Net", options.CredentialsFactoryType);
            Assert.AreEqual(2, options.CredentialsFactoryArgs.Count);
            Assert.IsTrue(options.CredentialsFactoryArgs.TryGetValue("arg1", out var arg1) && arg1 is string s1 && s1 == "value1");
            Assert.IsTrue(options.CredentialsFactoryArgs.TryGetValue("arg2", out var arg2) && arg2 is string s2 && s2 == "value2");

            var credentialsFactory = options.CredentialsFactory.Create();
            Assert.IsInstanceOf<DefaultCredentialsFactory>(credentialsFactory);
        }

        [Test]
        public void LoadBalancingOptionsFile()
        {
            var options = ReadResource(Resources.LoadBalancingOptions).LoadBalancer;

            Assert.AreEqual("Hazelcast.Clustering.LoadBalancing.RandomLoadBalancer, Hazelcast.Net", options.LoadBalancerType);
            Assert.AreEqual(2, options.LoadBalancerArgs.Count);
            Assert.IsTrue(options.LoadBalancerArgs.TryGetValue("arg1", out var arg1) && arg1 == "value1");
            Assert.IsTrue(options.LoadBalancerArgs.TryGetValue("arg2", out var arg2) && arg2 == "value2");

            var loadBalancer = options.LoadBalancer.Create();
            Assert.IsInstanceOf<RandomLoadBalancer>(loadBalancer);
        }

        [Test]
        public void SerializationOptionsFile()
        {
            var options = ReadResource(Resources.SerializationOptions).Serialization;

            Assert.AreEqual(Endianness.LittleEndian, options.Endianness);
            Assert.AreEqual(42, options.PortableVersion);
            Assert.IsFalse(options.CheckClassDefinitionErrors);

            Assert.AreEqual(1, options.PortableFactories.Count);
            var portableFactoryOptions = options.PortableFactories.First();
            Assert.AreEqual(666, portableFactoryOptions.Id);
            Assert.IsInstanceOf<TestPortableFactory>(portableFactoryOptions.Create());

            Assert.AreEqual(1, options.DataSerializableFactories.Count);
            var dataSerializableFactoryOptions = options.DataSerializableFactories.First();
            Assert.AreEqual(667, dataSerializableFactoryOptions.Id);
            Assert.IsInstanceOf<TestDataSerializableFactory>(dataSerializableFactoryOptions.Create());

            Assert.IsNotNull(options.DefaultSerializer);
            Assert.IsTrue(options.DefaultSerializer.OverrideClr);
            Assert.IsInstanceOf<TestDefaultSerializer>(options.DefaultSerializer.Create());

            Assert.AreEqual(1, options.Serializers.Count);
            var serializerOptions = options.Serializers.First();
            Assert.AreEqual("SomeClassName", serializerOptions.SerializedType);
            Assert.IsInstanceOf<TestSerializer>(serializerOptions.Create());
        }

        public class TestPortableFactory : IPortableFactory
        {
            public IPortable Create(int classId)
            {
                throw new NotSupportedException();
            }
        }

        public class TestDataSerializableFactory : IDataSerializableFactory
        {
            public IIdentifiedDataSerializable Create(int typeId)
            {
                throw new NotSupportedException();
            }
        }

        public class TestDefaultSerializer : ISerializer
        {
            public void Destroy()
            {
                throw new NotSupportedException();
            }

            public int GetTypeId()
            {
                throw new NotSupportedException();
            }
        }

        public class TestSerializer : ISerializer
        {
            public void Destroy()
            {
                throw new NotSupportedException();
            }

            public int GetTypeId()
            {
                throw new NotSupportedException();
            }
        }

        [Test]
        public void NearCacheOptionsFile()
        {
            var options = ReadResource(Resources.NearCacheOptions).NearCache;

            Assert.AreEqual(2, options.Count);

            Assert.IsTrue(options.TryGetValue("default", out var defaultNearCache));
            Assert.AreEqual("defaultName", defaultNearCache.Name);
            Assert.AreEqual(EvictionPolicy.Lru, defaultNearCache.EvictionPolicy);
            Assert.AreEqual(InMemoryFormat.Binary, defaultNearCache.InMemoryFormat);
            Assert.AreEqual(666, defaultNearCache.MaxIdleSeconds);
            Assert.AreEqual(667, defaultNearCache.MaxSize);
            Assert.AreEqual(668, defaultNearCache.TimeToLiveSeconds);
            Assert.IsTrue(defaultNearCache.InvalidateOnChange);

            Assert.IsTrue(options.TryGetValue("other", out var otherNearCache));
            Assert.AreEqual("otherName", otherNearCache.Name);
            Assert.AreEqual(EvictionPolicy.Lfu, otherNearCache.EvictionPolicy);
            Assert.AreEqual(InMemoryFormat.Object, otherNearCache.InMemoryFormat);
            Assert.AreEqual(166, otherNearCache.MaxIdleSeconds);
            Assert.AreEqual(167, otherNearCache.MaxSize);
            Assert.AreEqual(168, otherNearCache.TimeToLiveSeconds);
            Assert.IsFalse(otherNearCache.InvalidateOnChange);

            // TODO: whatever keys?
        }
    }
}

