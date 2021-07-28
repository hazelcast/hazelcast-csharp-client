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
using System.Linq;
using Hazelcast.Clustering.LoadBalancing;
using Hazelcast.Models;
using Hazelcast.Networking;
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

            // default state
            {
                Assert.That(lb.Count, Is.EqualTo(1));
                for (var i = 0; i < 4; i++)
                    Assert.That(lb.GetMember(onlyDataMember: i % 2 == 0), Is.EqualTo(memberId));
            }

            // after set members
            foreach (var members in new[]
            {
                new[] { NewMemberInfo(true) },
                new[] { NewMemberInfo(false) },
                new[] { NewMemberInfo(true), NewMemberInfo(false) }
            })
            {
                lb.SetMembers(members);
                Assert.That(lb.Count, Is.EqualTo(1));
                for (var i = 0; i < 4; i++)
                    Assert.That(lb.GetMember(), Is.EqualTo(memberId));
            }

            // set via metadata
            {
                lb = new StaticLoadBalancer(new Dictionary<string, string>
                {
                    { "memberId", memberId.ToString() }
                });
                Assert.That(lb.Count, Is.EqualTo(1));
                Assert.That(lb.GetMember(), Is.EqualTo(memberId));
            }
        }

        [Test]
        public void Random()
        {
            var lb = new RandomLoadBalancer();

            // default state
            {
                Assert.That(lb.Count, Is.EqualTo(0));
                Assert.That(lb.GetMember(), Is.EqualTo(Guid.Empty));
            }

            // has only lite members
            {
                var members = new[] { NewMemberInfo(false), NewMemberInfo(false) };

                lb.SetMembers(members);
                Assert.That(lb.Count, Is.EqualTo(members.Length));

                Assert.That(lb.GetMember(true), Is.EqualTo(Guid.Empty));
            }

            // returns different values
            foreach (var members in new[]
            {
                new[] { NewMemberInfo(true), NewMemberInfo(true), NewMemberInfo(true) },
                new[] { NewMemberInfo(true), NewMemberInfo(false), NewMemberInfo(true) },
                new[] { NewMemberInfo(false), NewMemberInfo(false), NewMemberInfo(false) },
            })
            {
                lb.SetMembers(members);
                Assert.That(lb.Count, Is.EqualTo(3));

                var seen = Guid.Empty;
                var seenDifferent = false;
                for (var i = 0; i < 10; i++)
                {
                    var memberId = lb.GetMember();
                    Assert.That(members.Select(m => m.Id), Does.Contain(memberId));

                    if (seen == Guid.Empty) seen = memberId;
                    else if (seen != memberId) seenDifferent = true;
                }

                Assert.That(seenDifferent, Is.True);
            }

            // returns values within set
            {
                var members = new[] { NewMemberInfo(false), NewMemberInfo(true) };

                lb.SetMembers(members);
                Assert.That(lb.Count, Is.EqualTo(2));

                for (var i = 0; i < 10; i++)
                {
                    var memberId = lb.GetMember(onlyDataMember: i % 2 == 0);
                    Assert.That(members.Select(m => m.Id), Does.Contain(memberId));
                }
            }

            // set empty
            {
                lb.SetMembers(Array.Empty<MemberInfo>());
                Assert.That(lb.Count, Is.EqualTo(0));
                Assert.That(lb.GetMember(onlyDataMember: false), Is.EqualTo(Guid.Empty));
                Assert.That(lb.GetMember(onlyDataMember: true), Is.EqualTo(Guid.Empty));
            }
        }

        [Test]
        public void RoundRobin()
        {
            var lb = new RoundRobinLoadBalancer();

            // default state
            {
                Assert.That(lb.Count, Is.EqualTo(0));
                Assert.That(lb.GetMember(), Is.EqualTo(Guid.Empty));
            }

            // has only lite members
            {
                var members = new[] { NewMemberInfo(false), NewMemberInfo(false) };

                lb.SetMembers(members);
                Assert.That(lb.Count, Is.EqualTo(members.Length));

                Assert.That(lb.GetMember(true), Is.EqualTo(Guid.Empty));
            }

            // all members have same type
            foreach (var members in new[]
            {
                new[] { NewMemberInfo(true), NewMemberInfo(true), NewMemberInfo(true) },
                new[] { NewMemberInfo(false), NewMemberInfo(false), NewMemberInfo(false) }
            })
            {
                lb.SetMembers(members);
                Assert.That(lb.Count, Is.EqualTo(members.Length));

                for (var index = 1; index <= members.Length * 2; index++)
                    Assert.That(lb.GetMember(), Is.EqualTo(members[index % members.Length].Id));
            }

            // set empty
            {
                lb.SetMembers(Array.Empty<MemberInfo>());
                Assert.That(lb.Count, Is.EqualTo(0));
                Assert.That(lb.GetMember(onlyDataMember: false), Is.EqualTo(Guid.Empty));
                Assert.That(lb.GetMember(onlyDataMember: true), Is.EqualTo(Guid.Empty));
            }
        }

        [Test]
        public void ArgumentExceptions()
        {
            Assert.Throws<ArgumentNullException>(() => new RoundRobinLoadBalancer().SetMembers(null));
            Assert.Throws<ArgumentNullException>(() => new RandomLoadBalancer().SetMembers(null));
        }

        private MemberInfo NewMemberInfo(bool isDataMember) => new MemberInfo(id: Guid.NewGuid(),
            NetworkAddress.Parse("localhost"), new MemberVersion(1, 0, 0),
            isLiteMember: !isDataMember, attributes: new Dictionary<string, string>());
    }
}