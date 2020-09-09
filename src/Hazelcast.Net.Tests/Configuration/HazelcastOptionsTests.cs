﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

using System;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Clustering.LoadBalancing;
using Hazelcast.Configuration.Binding;
using Hazelcast.Core;
using Hazelcast.NearCaching;
using Hazelcast.Networking;
using Hazelcast.Security;
using Hazelcast.Serialization;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Hazelcast.Tests.Configuration
{
    [TestFixture]
    public class HazelcastOptionsTests
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
        public async Task HazelcastOptionsRoot()
        {
            var options = ReadResource(Resources.HazelcastOptions);

            Assert.AreEqual("cluster", options.ClusterName);
            Assert.AreEqual("client", options.ClientName);
            Assert.AreEqual(2, options.Labels.Count);

            Assert.IsTrue(options.Labels.Contains("label_1"));
            Assert.IsTrue(options.Labels.Contains("label_2"));

            Assert.AreEqual(1, options.Subscribers.Count);
            var subscriber = options.Subscribers[0];
            Assert.IsInstanceOf<HazelcastClientEventSubscriber>(subscriber);

            TestSubscriber.Ctored = false;
            await subscriber.SubscribeAsync(null, CancellationToken.None);
            Assert.IsTrue(TestSubscriber.Ctored);
        }

        [Test]
        public void LoggingOptionsSection()
        {
            _ = ReadResource(Resources.HazelcastOptions).Logging;

            // nothing is configured here
        }

        [Test]
        public void CoreOptionsSection()
        {
            var options = ReadResource(Resources.HazelcastOptions).Core;

            Assert.AreEqual(1000, options.Clock.OffsetMilliseconds);
        }

        [Test]
        public void MessagingOptionsSection()
        {
            var options = ReadResource(Resources.HazelcastOptions).Messaging;

            Assert.AreEqual(1000, options.MaxFastInvocationCount);
            Assert.AreEqual(1001, options.MinRetryDelayMilliseconds);
            Assert.AreEqual(1003, options.DefaultOperationTimeoutMilliseconds);
        }

        [Test]
        public void HeartbeatOptionsSection()
        {
            var options = ReadResource(Resources.HazelcastOptions).Heartbeat;

            Assert.AreEqual(1000, options.PeriodMilliseconds);
            Assert.AreEqual(1001, options.TimeoutMilliseconds);
            Assert.AreEqual(1002, options.PingTimeoutMilliseconds);
        }

        [Test]
        public void NetworkingOptionsSection()
        {
            var options = ReadResource(Resources.HazelcastOptions).Networking;

            Assert.AreEqual(2, options.Addresses.Count);
            Assert.IsTrue(options.Addresses.Contains("localhost"));
            Assert.IsTrue(options.Addresses.Contains("otherhost"));
            Assert.IsFalse(options.ShuffleAddresses);
            Assert.IsFalse(options.SmartRouting);
            Assert.IsFalse(options.RetryOnTargetDisconnected);
            Assert.AreEqual(1000, options.ConnectionTimeoutMilliseconds);
            Assert.AreEqual(1001, options.WaitForClientMilliseconds);
            Assert.AreEqual(ReconnectMode.DoNotReconnect, options.ReconnectMode);
            Assert.IsFalse(options.ShuffleAddresses);

            var sslOptions = options.Ssl;
            Assert.IsTrue(sslOptions.Enabled);
            Assert.IsFalse(sslOptions.ValidateCertificateChain);
            Assert.IsTrue(sslOptions.ValidateCertificateName);
            Assert.IsTrue(sslOptions.CheckCertificateRevocation);
            Assert.AreEqual("cert", sslOptions.CertificateName);
            Assert.AreEqual("path", sslOptions.CertificatePath);
            Assert.AreEqual("password", sslOptions.CertificatePassword);
            Assert.AreEqual(SslProtocols.Tls11, sslOptions.Protocol);
            Console.WriteLine(sslOptions.ToString());

            var cloudOptions = options.Cloud;
            Assert.IsTrue(cloudOptions.Enabled);
            Assert.AreEqual("token", cloudOptions.DiscoveryToken);
            Assert.AreEqual(new Uri("http://cloud"), cloudOptions.UrlBase);

            var socketOptions = options.Socket;
            Assert.AreEqual(1000, socketOptions.BufferSizeKiB);
            Assert.IsFalse(socketOptions.KeepAlive);
            Assert.AreEqual(1001, socketOptions.LingerSeconds);
            Assert.IsTrue(socketOptions.TcpNoDelay);

            var interceptorOptions = options.SocketInterception;
            Assert.IsTrue(interceptorOptions.Enabled);
            Assert.IsInstanceOf<TestSocketInterceptor>(interceptorOptions.Interceptor.Service);
            Console.WriteLine(interceptorOptions.ToString());

            var retryOptions = options.ConnectionRetry;
            Assert.AreEqual(1000, retryOptions.InitialBackoffMilliseconds);
            Assert.AreEqual(1001, retryOptions.MaxBackoffMilliseconds);
            Assert.AreEqual(1002, retryOptions.Multiplier);
            Assert.AreEqual(1003, retryOptions.ClusterConnectionTimeoutMilliseconds);
            Assert.AreEqual(1004, retryOptions.Jitter);
        }

        [Test]
        public void AuthenticationOptionsFile()
        {
            var options = ReadResource(Resources.HazelcastOptions).Authentication;

            var authenticator = options.Authenticator.Service;
            Assert.IsInstanceOf<Authenticator>(authenticator);

            var credentialsFactory = options.CredentialsFactory.Service;
            Assert.IsInstanceOf<TestCredentialsFactory>(credentialsFactory);

            var testCredentialsFactory = (TestCredentialsFactory) credentialsFactory;
            Assert.AreEqual("arg", testCredentialsFactory.Arg1);
            Assert.AreEqual(1000, testCredentialsFactory.Arg2);
        }

        [Test]
        public void LoadBalancingOptionsFile()
        {
            var options = ReadResource(Resources.HazelcastOptions).LoadBalancing;

            var loadBalancer = options.LoadBalancer.Service;
            Assert.IsInstanceOf<RandomLoadBalancer>(loadBalancer);
        }

        public class TestSubscriber : IHazelcastClientEventSubscriber
        {
            public static bool Ctored { get; set; }

            public Task SubscribeAsync(IHazelcastClient client, CancellationToken cancellationToken)
            {
                Ctored = true;
                return Task.CompletedTask;
            }
        }

        public class TestSocketInterceptor : ISocketInterceptor
        {
            public TestSocketInterceptor()
            { }
        }

        public class TestCredentialsFactory : ICredentialsFactory
        {
            public TestCredentialsFactory(string arg1, int arg2)
            {
                Arg1 = arg1;
                Arg2 = arg2;
            }

            public string Arg1 { get; }

            public int Arg2 { get; }

            public ICredentials NewCredentials()
            {
                throw new NotSupportedException();
            }

            public void Dispose()
            { }
        }

        [Test]
        public void SerializationOptionsFile()
        {
            var options = ReadResource(Resources.HazelcastOptions).Serialization;

            Assert.AreEqual(Endianness.LittleEndian, options.Endianness);
            Assert.AreEqual(1000, options.PortableVersion);
            Assert.IsFalse(options.ValidateClassDefinitions);

            Assert.AreEqual(1, options.PortableFactories.Count);
            var portableFactoryOptions = options.PortableFactories.First();
            Assert.AreEqual(1001, portableFactoryOptions.Id);
            Assert.IsInstanceOf<TestPortableFactory>(portableFactoryOptions.Service);

            Assert.AreEqual(1, options.DataSerializableFactories.Count);
            var dataSerializableFactoryOptions = options.DataSerializableFactories.First();
            Assert.AreEqual(1002, dataSerializableFactoryOptions.Id);
            Assert.IsInstanceOf<TestDataSerializableFactory>(dataSerializableFactoryOptions.Service);

            Assert.IsNotNull(options.DefaultSerializer);
            Assert.IsTrue(options.DefaultSerializer.OverrideClr);
            Assert.IsInstanceOf<TestDefaultSerializer>(options.DefaultSerializer.Service);

            Assert.AreEqual(1, options.Serializers.Count);
            var serializerOptions = options.Serializers.First();
            Assert.AreEqual(typeof(HazelcastClient), serializerOptions.SerializedType);
            Assert.IsInstanceOf<TestSerializer>(serializerOptions.Service);
        }

        [Test]
        public void NearCacheOptionsFile()
        {
            var options = ReadResource(Resources.HazelcastOptions).NearCache;

            Assert.AreEqual(2, options.Configurations.Count);

            Assert.IsTrue(options.Configurations.TryGetValue("default", out var defaultNearCache));
            Assert.AreEqual(EvictionPolicy.Lru, defaultNearCache.EvictionPolicy);
            Assert.AreEqual(InMemoryFormat.Binary, defaultNearCache.InMemoryFormat);
            Assert.AreEqual(1000, defaultNearCache.MaxIdleSeconds);
            Assert.AreEqual(1001, defaultNearCache.MaxSize);
            Assert.AreEqual(1002, defaultNearCache.TimeToLiveSeconds);
            Assert.IsTrue(defaultNearCache.InvalidateOnChange);

            Assert.IsTrue(options.Configurations.TryGetValue("other", out var otherNearCache));
            Assert.AreEqual(EvictionPolicy.Lfu, otherNearCache.EvictionPolicy);
            Assert.AreEqual(InMemoryFormat.Object, otherNearCache.InMemoryFormat);
            Assert.AreEqual(2000, otherNearCache.MaxIdleSeconds);
            Assert.AreEqual(2001, otherNearCache.MaxSize);
            Assert.AreEqual(2002, otherNearCache.TimeToLiveSeconds);
            Assert.IsFalse(otherNearCache.InvalidateOnChange);

            // TODO: whatever keys?
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

            public int TypeId => throw new NotSupportedException();
        }

        public class TestSerializer : ISerializer
        {
            public void Destroy()
            {
                throw new NotSupportedException();
            }

            public int TypeId => throw new NotSupportedException();
        }
    }
}
