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

using System.Threading;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.Remote;
using NUnit.Framework;

namespace Hazelcast.Client.Test.Config
{
    [TestFixture]
    public class ClientListenerConfigTest : HazelcastTestSupport
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
        public void TestConfigureLifecycleListener_className()
        {
            StartMember(_remoteController, _cluster);

            var client = _clientFactory.CreateClient(clientConfig =>
            {
                clientConfig.GetGroupConfig().SetName(_cluster.Id).SetPassword(_cluster.Id);
                clientConfig.AddListenerConfig(new ListenerConfig(typeof(SimpleLifecycleListener).AssemblyQualifiedName));
            });

            Assert.True(client.GetLifecycleService().IsRunning());
            Assert.True(SimpleLifecycleListener.reset.Wait(1000));
        }

        [Test]
        public void TestConfigureMembershipListener_className()
        {
            StartMember(_remoteController, _cluster);
            var client = _clientFactory.CreateClient(clientConfig =>
            {
                clientConfig.GetNetworkConfig().SetConnectionAttemptLimit(1000);
                clientConfig.GetGroupConfig().SetName(_cluster.Id).SetPassword(_cluster.Id);
                clientConfig.AddListenerConfig(new ListenerConfig(typeof(SimpleMembershipListener).AssemblyQualifiedName));
            });
            StartMember(_remoteController, _cluster);

            Assert.True(client.GetLifecycleService().IsRunning());
            Assert.True(SimpleMembershipListener.reset.Wait(1000));
        }
    }

    internal class SimpleLifecycleListener : ILifecycleListener
    {
        internal static readonly ManualResetEventSlim reset = new ManualResetEventSlim();

        public void StateChanged(LifecycleEvent lifecycleEvent)
        {
            if (lifecycleEvent.GetState() == LifecycleEvent.LifecycleState.ClientConnected)
            {
                reset.Set();
            }
        }
    }
    
    internal class SimpleMembershipListener : IMembershipListener
    {
        internal static readonly ManualResetEventSlim reset = new ManualResetEventSlim();

        public void MemberAdded(MembershipEvent membershipEvent)
        {
            reset.Set();
        }

        public void MemberAttributeChanged(MemberAttributeEvent memberAttributeEvent)
        {
            reset.Set();
        }

        public void MemberRemoved(MembershipEvent membershipEvent)
        {
            reset.Set();
        }
    }
}