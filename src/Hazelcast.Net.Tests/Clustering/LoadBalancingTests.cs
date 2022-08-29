// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Clustering.LoadBalancing;
using NUnit.Framework;

namespace Hazelcast.Tests.Clustering
{
    [TestFixture]
    public class LoadBalancingTests
    {
        [Test]
        public void Static()
        {
            var memberId = Guid.NewGuid();
            var lb = new StaticLoadBalancer(memberId);

            Assert.That(lb.Count, Is.EqualTo(1));
            for (var i = 0; i < 4; i++)
                Assert.That(lb.GetMember(), Is.EqualTo(memberId));

            // no effect
            lb.SetMembers(new[] { Guid.NewGuid() });

            Assert.That(lb.Count, Is.EqualTo(1));
            for (var i = 0; i < 4; i++)
                Assert.That(lb.GetMember(), Is.EqualTo(memberId));

            lb = new StaticLoadBalancer(new Dictionary<string, string>
            {
                { "memberId", memberId.ToString() }
            });

            Assert.That(lb.Count, Is.EqualTo(1));
            Assert.That(lb.GetMember(), Is.EqualTo(memberId));
        }

        [Test]
        public void Random()
        {
            var memberIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var lb = new RandomLoadBalancer();

            Assert.That(lb.Count, Is.EqualTo(0));
            Assert.That(lb.GetMember(), Is.EqualTo(Guid.Empty));

            lb.SetMembers(memberIds);
            Assert.That(lb.Count, Is.EqualTo(3));

            var seen = Guid.Empty;
            var seenDifferent = false;
            for (var i = 0; i < 10; i++)
            {
                var memberId = lb.GetMember();
                Assert.That(memberIds, Does.Contain(memberId));

                if (seen == Guid.Empty) seen = memberId;
                else if (seen != memberId) seenDifferent = true;
            }

            Assert.That(seenDifferent, Is.True);

            memberIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

            lb.SetMembers(memberIds);
            Assert.That(lb.Count, Is.EqualTo(2));

            for (var i = 0; i < 10; i++)
            {
                var memberId = lb.GetMember();
                Assert.That(memberIds, Does.Contain(memberId));
            }

            lb.SetMembers(new Guid[0]);
            Assert.That(lb.Count, Is.EqualTo(0));
            Assert.That(lb.GetMember(), Is.EqualTo(Guid.Empty));
        }

        [Test]
        public void RoundRobin()
        {
            var memberIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var lb = new RoundRobinLoadBalancer();

            Assert.That(lb.Count, Is.EqualTo(0));
            Assert.That(lb.GetMember(), Is.EqualTo(Guid.Empty));

            lb.SetMembers(memberIds);
            Assert.That(lb.Count, Is.EqualTo(3));

            Assert.That(lb.GetMember(), Is.EqualTo(memberIds[1]));
            Assert.That(lb.GetMember(), Is.EqualTo(memberIds[2]));
            Assert.That(lb.GetMember(), Is.EqualTo(memberIds[0]));
            Assert.That(lb.GetMember(), Is.EqualTo(memberIds[1]));
            Assert.That(lb.GetMember(), Is.EqualTo(memberIds[2]));
            Assert.That(lb.GetMember(), Is.EqualTo(memberIds[0]));

            memberIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

            lb.SetMembers(memberIds);
            Assert.That(lb.Count, Is.EqualTo(2));

            Assert.That(lb.GetMember(), Is.EqualTo(memberIds[1]));
            Assert.That(lb.GetMember(), Is.EqualTo(memberIds[0]));
            Assert.That(lb.GetMember(), Is.EqualTo(memberIds[1]));
            Assert.That(lb.GetMember(), Is.EqualTo(memberIds[0]));

            lb.SetMembers(new Guid[0]);
            Assert.That(lb.Count, Is.EqualTo(0));
            Assert.That(lb.GetMember(), Is.EqualTo(Guid.Empty));
        }

        [Test]
        public void ArgumentExceptions()
        {
            Assert.Throws<ArgumentNullException>(() => new RoundRobinLoadBalancer().SetMembers(null));
        }
    }
}
