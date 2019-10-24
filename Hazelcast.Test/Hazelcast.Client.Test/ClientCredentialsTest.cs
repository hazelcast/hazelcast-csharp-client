﻿// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
using Hazelcast.Config;
using Hazelcast.Remote;
using Hazelcast.Security;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientCredentialsTest : HazelcastTestSupport
    {
        private RemoteController.Client _remoteController;
        private Cluster _cluster;
        private readonly HazelcastClientFactory _clientFactory = new HazelcastClientFactory();

        [SetUp]
        public void Setup()
        {
            _remoteController = CreateRemoteController();
            _cluster = CreateCluster(_remoteController);
        }

        [TearDown]
        public void TearDown()
        {
            HazelcastClient.ShutdownAll();
            StopCluster(_remoteController, _cluster);
            StopRemoteController(_remoteController);
        }

        [Test]
        public void CredentialConfig_Credentials_Factory()
        {
            Assert.Catch<ConfigurationException>(() =>
            {
                _clientFactory.CreateClient(clientConfig =>
                {
                    clientConfig.GetSecurityConfig().SetCredentials(new UsernamePasswordCredentials(_cluster.Id, _cluster.Id));
                    clientConfig.GetSecurityConfig().GetCredentialsFactoryConfig()
                        .SetImplementation(new UsernamePasswordCredentialsFactory());
                });
            });
        }

        [Test]
        public void CredentialConfig_CredentialsClass_Factory()
        {
            Assert.Catch<ConfigurationException>(() =>
            {
                _clientFactory.CreateClient(clientConfig =>
                {
                    clientConfig.GetSecurityConfig().SetCredentialsClassName(typeof(UsernamePasswordCredentials).FullName);
                    clientConfig.GetSecurityConfig().GetCredentialsFactoryConfig()
                        .SetImplementation(new UsernamePasswordCredentialsFactory());
                });
            });
        }

        [Test]
        public void CredentialConfig_CredentialsClass_FactoryClass()
        {
            Assert.Catch<ConfigurationException>(() =>
            {
                _clientFactory.CreateClient(clientConfig =>
                {
                    clientConfig.GetSecurityConfig().SetCredentialsClassName(typeof(UsernamePasswordCredentials).FullName);
                    clientConfig.GetSecurityConfig().GetCredentialsFactoryConfig()
                        .SetClassName(typeof(UsernamePasswordCredentialsFactory).FullName);
                });
            });
        }

        [Test]
        public void CredentialConfig_Credentials_FactoryClass()
        {
            Assert.Catch<ConfigurationException>(() =>
            {
                _clientFactory.CreateClient(clientConfig =>
                {
                    clientConfig.GetSecurityConfig().SetCredentials(new UsernamePasswordCredentials(_cluster.Id, _cluster.Id));
                    clientConfig.GetSecurityConfig().GetCredentialsFactoryConfig()
                        .SetClassName(typeof(UsernamePasswordCredentialsFactory).FullName);
                });
            });
        }

        [Test]
        public void CredentialConfig_Credentials()
        {
            StartMember(_remoteController, _cluster);

            var client = _clientFactory.CreateClient(clientConfig =>
            {
                clientConfig.GetSecurityConfig().SetCredentials(new UsernamePasswordCredentials(_cluster.Id, _cluster.Id));
            });

            Assert.True(client.GetLifecycleService().IsRunning());
        }

        [Test]
        public void CredentialConfig_CredentialsClass()
        {
            StartMember(_remoteController, _cluster);
           
            var exception = Assert.Catch<InvalidOperationException>(() =>
            {
                _clientFactory.CreateClient(clientConfig =>
                {
                    clientConfig.GetSecurityConfig().SetCredentialsClassName(typeof(DummyCredentials).AssemblyQualifiedName);

                });
            });
            var innerExceptions = ((AggregateException) exception.InnerException).InnerExceptions;
            Assert.True(innerExceptions.Count > 0);
        }

        [Test]
        public void CredentialConfig_CredentialsClass_empty()
        {
            StartMember(_remoteController, _cluster);
           
            var exception = Assert.Catch<InvalidOperationException>(() =>
            {
                _clientFactory.CreateClient(clientConfig =>
                {
                    clientConfig.GetSecurityConfig().SetCredentialsClassName(typeof(UsernamePasswordCredentials).AssemblyQualifiedName);

                });
            });
            var innerExceptions = ((AggregateException) exception.InnerException).InnerExceptions;
            Assert.True(innerExceptions.Count > 0);
        }

        [Test]
        public void CredentialConfig_CredentialsFactory()
        {
            StartMember(_remoteController, _cluster);

            var client = _clientFactory.CreateClient(clientConfig =>
            {
                clientConfig.GetSecurityConfig().GetCredentialsFactoryConfig()
                    .SetImplementation(new UsernamePasswordCredentialsFactory())
                    .SetProperty("username", _cluster.Id)
                    .SetProperty("password", _cluster.Id);
            });

            Assert.True(client.GetLifecycleService().IsRunning());
        }

        [Test]
        public void CredentialConfig_CredentialsFactory_with_defaults()
        {
            StartMember(_remoteController, _cluster);

            var client = _clientFactory.CreateClient(clientConfig =>
            {
                clientConfig.GetGroupConfig().SetName(_cluster.Id).SetPassword(_cluster.Id);
                clientConfig.GetSecurityConfig().GetCredentialsFactoryConfig()
                    .SetImplementation(new UsernamePasswordCredentialsFactory());
            });

            Assert.True(client.GetLifecycleService().IsRunning());
        }

        [Test]
        public void CredentialConfig_CredentialsFactoryClass()
        {
            StartMember(_remoteController, _cluster);

            var client = _clientFactory.CreateClient(clientConfig =>
            {
                clientConfig.GetSecurityConfig().GetCredentialsFactoryConfig()
                    .SetClassName(typeof(UsernamePasswordCredentialsFactory).FullName)
                    .SetProperty("username", _cluster.Id)
                    .SetProperty("password", _cluster.Id);
            });

            Assert.True(client.GetLifecycleService().IsRunning());
        }

        [Test]
        public void CredentialConfig_CustomCredentialsFactory()
        {
            StartMember(_remoteController, _cluster);

            var factory = new DummyCredentialFactory();
            var client = _clientFactory.CreateClient(clientConfig =>
            {
                clientConfig.GetSecurityConfig().GetCredentialsFactoryConfig()
                    .SetImplementation(factory)
                    .SetProperty("username", _cluster.Id)
                    .SetProperty("password", _cluster.Id);
            });
            Assert.NotNull(factory.GroupConfig);
            Assert.NotNull(factory.Properties);
            Assert.True(client.GetLifecycleService().IsRunning());
            
            client.Shutdown();
            
            Assert.Null(factory.GroupConfig);
            Assert.Null(factory.Properties);
        }
    }

    [Serializable]
    internal class DummyCredentials : ICredentials
    {
        public string GetEndpoint() => null;

        public string GetPrincipal() => null;

        public void SetEndpoint(string endpoint)
        {
        }
    }

    internal class DummyCredentialFactory : ICredentialsFactory
    {
        internal GroupConfig GroupConfig;
        internal IDictionary<string, string> Properties;

        public void Configure(GroupConfig groupConfig, IDictionary<string, string> properties)
        {
            GroupConfig = groupConfig;
            Properties = properties;
        }

        public ICredentials NewCredentials()
        {
            if (!Properties.TryGetValue("username", out var username))
            {
                username = GroupConfig.GetName();
            }
            if (!Properties.TryGetValue("password", out var password))
            {
                password = GroupConfig.GetPassword();
            }
            return new UsernamePasswordCredentials(username, password);
        }

        public void Destroy()
        {
            GroupConfig = null;
            Properties = null;
        }
    }
}