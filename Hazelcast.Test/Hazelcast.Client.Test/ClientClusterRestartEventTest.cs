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

using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientClusterRestartEventTest : MultiMemberBaseNoSetupTest
    {
        [TearDown]
        public void Teardown()
        {
            ShutdownRemoteController();
        }

        [Test]
        public void TestSingleMember()
        {
            string oldMemberUUID = null;
            SetupCluster(() =>
            {
                oldMemberUUID = StartNewMember();
            });

            var memberAdded = new CountdownEvent(1);
            var memberRemoved = new CountdownEvent(1);

            string addedMemberReferenceUUID = null;
            string removedMemberReferenceUUID = null;

            Client.GetCluster().AddMembershipListener(new MembershipListener
            {
                OnMemberAdded = membershipEvent =>
                {
                    Interlocked.Exchange(ref addedMemberReferenceUUID, membershipEvent.GetMember().GetUuid());
                    memberAdded.Signal();
                },
                OnMemberRemoved = membershipEvent =>
                {
                    Interlocked.Exchange(ref removedMemberReferenceUUID, membershipEvent.GetMember().GetUuid());
                    memberRemoved.Signal();
                }
            });

            ShutdownMember(oldMemberUUID);
            var newMemberUUID = StartNewMember();

            TestSupport.AssertOpenEventually(memberRemoved);
            Assert.AreEqual(oldMemberUUID, removedMemberReferenceUUID);

            TestSupport.AssertOpenEventually(memberAdded);
            Assert.AreEqual(newMemberUUID, addedMemberReferenceUUID);

            var members = Client.GetCluster().GetMembers();
            Assert.True(members.Any(member => member.GetUuid() == newMemberUUID));
            Assert.AreEqual(1, members.Count);
        }

        [Test]
        public void TestMultiMember()
        {
            string oldMemberUUID0 = null;
            string oldMemberUUID1 = null;
            SetupCluster(() =>
            {
                oldMemberUUID0 = StartNewMember();
                oldMemberUUID1 = StartNewMember();
            });

            var memberAdded = new CountdownEvent(2);
            var memberRemoved = new CountdownEvent(2);

            var addedMemberUUIDs = new ConcurrentBag<string>();
            var removedMemberUUIDs = new ConcurrentBag<string>();

            Client.GetCluster().AddMembershipListener(new MembershipListener
            {
                OnMemberAdded = membershipEvent =>
                {
                    addedMemberUUIDs.Add(membershipEvent.GetMember().GetUuid());
                    memberAdded.Signal();
                },
                OnMemberRemoved = membershipEvent =>
                {
                    removedMemberUUIDs.Add(membershipEvent.GetMember().GetUuid());
                    memberRemoved.Signal();
                }
            });
            while (!ShutdownCluster())
            {
                Thread.Sleep(1000);
            }
            var newMemberUUID0 = StartNewMember();
            var newMemberUUID1 = StartNewMember();

            TestSupport.AssertOpenEventually(memberRemoved);
            Assert.AreEqual(2, removedMemberUUIDs.Count);
            Assert.Contains(oldMemberUUID0, removedMemberUUIDs);
            Assert.Contains(oldMemberUUID1, removedMemberUUIDs);

            TestSupport.AssertOpenEventually(memberAdded);
            Assert.AreEqual(2, addedMemberUUIDs.Count);
            Assert.Contains(newMemberUUID0, addedMemberUUIDs);
            Assert.Contains(newMemberUUID1, addedMemberUUIDs);

            var members = Client.GetCluster().GetMembers();
            Assert.AreEqual(2, members.Count);
            Assert.True(members.Any(member => member.GetUuid() == newMemberUUID0));
            Assert.True(members.Any(member => member.GetUuid() == newMemberUUID1));
        }
    }
}