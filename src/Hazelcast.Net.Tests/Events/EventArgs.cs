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

using System;
using System.Collections.Generic;
using Hazelcast.Data;
using Hazelcast.Events;
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
        public void ClientLifeCycleEventArgs()
        {
            var args = new ClientLifecycleEventArgs(ClientLifecycleState.Shutdown);

            Assert.That(args.State, Is.EqualTo(ClientLifecycleState.Shutdown));
        }

        [Test]
        public void MemberLifecycleEventArgs()
        {
            var memberInfo = new MemberInfo(Guid.NewGuid(), NetworkAddress.Parse("127.0.0.1:88"), new MemberVersion(1, 1, 1), false, new Dictionary<string, string>());
            var args = new MemberLifecycleEventArgs(memberInfo);

            Assert.That(args.Member, Is.SameAs(memberInfo));
        }

        [Test]
        public void DistributedObjectLifecycleEventArgs()
        {
            var memberId = Guid.NewGuid();
            var args = new DistributedObjectLifecycleEventArgs("serviceName", "name", memberId);

            Assert.That(args.Name, Is.EqualTo("name"));
            Assert.That(args.ServiceName, Is.EqualTo("serviceName"));
            Assert.That(args.SourceMemberId, Is.EqualTo(memberId));
        }
    }
}
