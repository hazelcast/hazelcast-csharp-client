// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
    internal class ClientClusterServiceTest : HazelcastTestSupport
    {
        private IHazelcastInstance _client;
        private Cluster _cluster;
        private RemoteController.Client _remoteController;

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

        protected override void ConfigureClient(ClientConfig config)
        {
            base.ConfigureClient(config);
            config.AddListenerConfig(new ListenerConfig(_initialMembershipListener));
        }

        protected override void ConfigureGroup(ClientConfig config)
        {
            config.GetGroupConfig().SetName(_cluster.Id).SetPassword(_cluster.Id);
        }

        [Test]
        public void MemberAddedEvent()
        {
            var reset = new ManualResetEventSlim();

            MembershipEvent memberAddedEvent = null;
            _client.GetCluster().AddMembershipListener(new MembershipListener
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
            _client.GetCluster().AddMembershipListener(new MembershipListener
            {
                OnMemberAdded = memberAddedEvent => { reset.Set(); }
            });
            var member = StartMember(_remoteController, _cluster);
            Assert.IsTrue(reset.Wait(30 * 1000));
            reset.Reset();

            MembershipEvent memberRemovedEvent = null;
            _client.GetCluster().AddMembershipListener(new MembershipListener
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
        public void InitialMembershipService()
        {
            var listener = new InitialMembershipListener();
            _client.GetCluster().AddMembershipListener(listener);

            var members = listener._membershipEvent.GetMembers();
            Assert.AreEqual(1, members.Count);

            var member = members.FirstOrDefault();
            Assert.IsNotNull(member);
            Assert.IsNotNull(listener._membershipEvent.GetCluster());
            Assert.IsNotNull(listener._membershipEvent.ToString());
            Assert.AreEqual(1, _initialMembershipListener.counter);
        }
        
        [Test]
        public void InitialMembershipWithConfig()
        {
            var members = _initialMembershipListener._membershipEvent.GetMembers();
            Assert.AreEqual(1, members.Count);

            var member = members.FirstOrDefault();
            Assert.IsNotNull(member);
            Assert.IsNotNull(_initialMembershipListener._membershipEvent.GetCluster());
            Assert.IsNotNull(_initialMembershipListener._membershipEvent.ToString());
            Assert.AreEqual(1, _initialMembershipListener.counter);
        }

        private class InitialMembershipListener : IInitialMembershipListener
        {
            public InitialMembershipEvent _membershipEvent;
            public long counter = 0;

            public void MemberAdded(MembershipEvent membershipEvent)
            {
            }

            public void MemberRemoved(MembershipEvent membershipEvent)
            {
            }

            public void MemberAttributeChanged(MemberAttributeEvent memberAttributeEvent)
            {
            }

            public void Init(InitialMembershipEvent membershipEvent)
            {
                _membershipEvent = membershipEvent;
                Interlocked.Increment(ref counter);
            }
        }
    }
}