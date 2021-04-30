// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Clustering;
using Hazelcast.Events;
using Hazelcast.Models;
using Hazelcast.Networking;
using NUnit.Framework;

namespace Hazelcast.Tests.Events
{
    [TestFixture]
    public class EventArgs
    {
        [Test]
        public void PartitionLostEventArgs()
        {
            var memberInfo = new MemberInfo(Guid.NewGuid(), NetworkAddress.Parse("127.0.0.1:88"), new MemberVersion(1, 1, 1), false, new Dictionary<string, string>());
            var args = new PartitionLostEventArgs(12, 13, true, memberInfo);

            Assert.That(args.PartitionId, Is.EqualTo(12));
            Assert.That(args.LostBackupCount, Is.EqualTo(13));
            Assert.That(args.IsAllReplicasInPartitionLost);
            Assert.That(args.Member, Is.SameAs(memberInfo));
        }

        [Test]
        public void StateChangedEventArgs()
        {
            var args = new StateChangedEventArgs(ClientState.Connected);

            Assert.That(args.State, Is.EqualTo(ClientState.Connected));
        }

        [Test]
        public void MembersUpdatedEventArgs()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var id3 = Guid.NewGuid();
            var mi1 = new MemberInfo(id1, NetworkAddress.Parse("127.0.0.1:88"), new MemberVersion(1, 1, 1), false, new Dictionary<string, string>());
            var mi2 = new MemberInfo(id2, NetworkAddress.Parse("127.0.0.1:88"), new MemberVersion(1, 1, 1), false, new Dictionary<string, string>());
            var mi3 = new MemberInfo(id3, NetworkAddress.Parse("127.0.0.1:88"), new MemberVersion(1, 1, 1), false, new Dictionary<string, string>());

            var args = new MembersUpdatedEventArgs(new [] { mi1 }, new []{ mi2 }, new [] { mi1, mi3 });

            Assert.That(args.AddedMembers.Count, Is.EqualTo(1));
            Assert.That(args.AddedMembers, Does.Contain(mi1));

            Assert.That(args.RemovedMembers.Count, Is.EqualTo(1));
            Assert.That(args.RemovedMembers, Does.Contain(mi2));

            Assert.That(args.Members.Count, Is.EqualTo(2));
            Assert.That(args.Members, Does.Contain(mi1));
            Assert.That(args.Members, Does.Contain(mi3));
        }

        [Test]
        public void DistributedObjectCreatedEventArgs()
        {
            var memberId = Guid.NewGuid();
            var args = new DistributedObjectCreatedEventArgs("serviceName", "name", memberId);

            Assert.That(args.Name, Is.EqualTo("name"));
            Assert.That(args.ServiceName, Is.EqualTo("serviceName"));
            Assert.That(args.SourceMemberId, Is.EqualTo(memberId));
        }

        [Test]
        public void DistributedObjectDestroyedEventArgs()
        {
            var memberId = Guid.NewGuid();
            var args = new DistributedObjectCreatedEventArgs("serviceName", "name", memberId);

            Assert.That(args.Name, Is.EqualTo("name"));
            Assert.That(args.ServiceName, Is.EqualTo("serviceName"));
            Assert.That(args.SourceMemberId, Is.EqualTo(memberId));
        }
    }
}
