/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Hazelcast.Core;
using Hazelcast.IO;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    class ClientClusterServiceTest : HazelcastBaseTest
    {
        [Test]
        public void TestInitialMembershipService()
        {
            var listener = new InitialMembershipListener();
            Client.GetCluster().AddMembershipListener(listener);

            var members = listener._membershipEvent.GetMembers();
            Assert.AreEqual(1, members.Count);

            var member = members.FirstOrDefault();
            Assert.IsNotNull(member);
            Assert.IsNotNull(listener._membershipEvent.GetCluster());
            Assert.IsNotNull(listener._membershipEvent.ToString());
        }

        [Test]
        public void MemberAddedEvent()
        {
            var reset = new ManualResetEventSlim();

            MembershipEvent memberAddedEvent = null;
            Client.GetCluster().AddMembershipListener(new MembershipListener()
            {
                OnMemberAdded = memberAdded =>
                {
                    memberAddedEvent = memberAdded;
                    reset.Set();
                }
            });
            var node = Cluster.AddNode();
            try
            {
                Assert.IsTrue(reset.Wait(30*1000));
                Assert.IsInstanceOf<ICluster>(memberAddedEvent.Source);
                Assert.IsInstanceOf<ICluster>(memberAddedEvent.GetCluster());
                Assert.AreEqual(MembershipEvent.MemberAdded, memberAddedEvent.GetEventType());
                Assert.IsNotNull(memberAddedEvent.GetMember());
                Assert.AreEqual(2, memberAddedEvent.GetMembers().Count);

            }
            finally
            {
                Cluster.RemoveNode(node);
            }
        }

        [Test]
        public void MemberRemovedEvent()
        {
            var node = Cluster.AddNode();
            Client.GetCluster().AddMembershipListener(new MembershipListener()
            {
                OnMemberAdded = memberAddedEvent =>
                {
                    Cluster.RemoveNode(node);
                }
            });
            
            var reset = new ManualResetEventSlim();

            MembershipEvent memberRemovedEvent = null;
            Client.GetCluster().AddMembershipListener(new MembershipListener
            {
                OnMemberRemoved = memberRemoved =>
                {
                    memberRemovedEvent = memberRemoved;
                    reset.Set();
                }
            });
            Assert.IsTrue(reset.Wait(30 * 1000));
            Assert.IsInstanceOf<ICluster>(memberRemovedEvent.Source);
            Assert.IsInstanceOf<ICluster>(memberRemovedEvent.GetCluster());
            Assert.AreEqual(MembershipEvent.MemberRemoved, memberRemovedEvent.GetEventType());
            Assert.IsNotNull(memberRemovedEvent.GetMember());
            Assert.AreEqual(1, memberRemovedEvent.GetMembers().Count);
        }

        private class InitialMembershipListener : IInitialMembershipListener
        {
            public InitialMembershipEvent _membershipEvent;

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
            }
        }
    }
}
