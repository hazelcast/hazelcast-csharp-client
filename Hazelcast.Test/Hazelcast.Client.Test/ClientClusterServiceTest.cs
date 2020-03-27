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

using System.Linq;
using System.Threading;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.Remote;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    public class ClientClusterServiceTest : HazelcastTestSupport
    {
        private IHazelcastInstance _client;
        private Cluster _cluster;
        private IRemoteController _remoteController;
        private InitialMembershipListener _initialMembershipListener;

        [SetUp]
        public void Setup()
        {
            _initialMembershipListener = new InitialMembershipListener();
            _remoteController = CreateRemoteController();
            _cluster = CreateCluster(_remoteController);
            StartMember(_remoteController, _cluster);
            _client = CreateClient();
        }

        [TearDown]
        public void TearDown()
        {
            _client.Shutdown();
            _remoteController.shutdownCluster(_cluster.Id);
            StopRemoteController(_remoteController);
        }

        protected override void ConfigureClient(Configuration config)
        {
            base.ConfigureClient(config);
            config.ConfigureListeners(listeners =>
            {
                listeners.Add(new ListenerConfig(_initialMembershipListener));
            });
        }

        protected override void ConfigureGroup(Configuration config)
        {
            config.ClusterName = _cluster.Id;
        }

        [Test]
        public void MemberAddedEvent()
        {
            var reset = new ManualResetEventSlim();

            MembershipEvent memberAddedEvent = null;
            _client.Cluster.AddMembershipListener(new MembershipListener
            {
                OnMemberAdded = memberAdded =>
                {
                    memberAddedEvent = memberAdded;
                    reset.Set();
                }
            });
            StartMember(_remoteController, _cluster);
            Assert.IsTrue(reset.Wait(30*1000));
            Assert.IsInstanceOf<ICluster>(memberAddedEvent.Source);
            Assert.IsInstanceOf<ICluster>(memberAddedEvent.GetCluster());
            Assert.AreEqual(MembershipEvent.MemberAdded, memberAddedEvent.GetEventType());
            Assert.IsNotNull(memberAddedEvent.GetMember());
            Assert.AreEqual(2, memberAddedEvent.GetMembers().Count);
        }

        [Test]
        public void MemberRemovedEvent()
        {
            var reset = new ManualResetEventSlim();
            _client.Cluster.AddMembershipListener(new MembershipListener
            {
                OnMemberAdded = memberAddedEvent => { reset.Set(); }
            });
            var member = StartMember(_remoteController, _cluster);
            Assert.IsTrue(reset.Wait(30 * 1000));
            reset.Reset();

            MembershipEvent memberRemovedEvent = null;
            _client.Cluster.AddMembershipListener(new MembershipListener
            {
                OnMemberRemoved = memberRemoved =>
                {
                    memberRemovedEvent = memberRemoved;
                    reset.Set();
                }
            });
            StopMember(_remoteController, _cluster, member);
            
            Assert.IsTrue(reset.Wait(30*1000));
            Assert.IsInstanceOf<ICluster>(memberRemovedEvent.Source);
            Assert.IsInstanceOf<ICluster>(memberRemovedEvent.GetCluster());
            Assert.AreEqual(MembershipEvent.MemberRemoved, memberRemovedEvent.GetEventType());
            Assert.IsNotNull(memberRemovedEvent.GetMember());
            Assert.AreEqual(1, memberRemovedEvent.GetMembers().Count);
        }

        [Test]
        public void TestInitialMembershipService()
        {
            var listener = new InitialMembershipListener();
            _client.Cluster.AddMembershipListener(listener);

            var members = listener.MembershipEvent.GetMembers();
            Assert.AreEqual(1, members.Count);

            var member = members.FirstOrDefault();
            Assert.IsNotNull(member);
            Assert.IsNotNull(listener.MembershipEvent.GetCluster());
            Assert.IsNotNull(listener.MembershipEvent.ToString());
            Assert.AreEqual(1, _initialMembershipListener.Counter);
        }
        
        [Test]
        public void TestInitialMembershipWithConfig()
        {
            var members = _initialMembershipListener.MembershipEvent.GetMembers();
            Assert.AreEqual(1, members.Count);

            var member = members.FirstOrDefault();
            Assert.IsNotNull(member);
            Assert.IsNotNull(_initialMembershipListener.MembershipEvent.GetCluster());
            Assert.IsNotNull(_initialMembershipListener.MembershipEvent.ToString());
            Assert.AreEqual(1, _initialMembershipListener.Counter);
        }

        private class InitialMembershipListener : IInitialMembershipListener
        {
            public InitialMembershipEvent MembershipEvent;
            public long Counter;

            public void MemberAdded(MembershipEvent membershipEvent)
            {
            }

            public void MemberRemoved(MembershipEvent membershipEvent)
            {
            }
            public void Init(InitialMembershipEvent membershipEvent)
            {
                MembershipEvent = membershipEvent;
                Interlocked.Increment(ref Counter);
            }
        }
    }
}