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

            Assert.Throws<InvalidOperationException>(() => lb.NotifyMembers(new[] { Guid.NewGuid() }));

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
            Assert.Throws<InvalidOperationException>(() => lb.GetMember());

            lb.NotifyMembers(memberIds);
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

            lb.NotifyMembers(memberIds);
            Assert.That(lb.Count, Is.EqualTo(2));

            for (var i = 0; i < 10; i++)
            {
                var memberId = lb.GetMember();
                Assert.That(memberIds, Does.Contain(memberId));
            }

            lb.NotifyMembers(new Guid[0]);
            Assert.That(lb.Count, Is.EqualTo(0));
            Assert.Throws<InvalidOperationException>(() => lb.GetMember());
        }

        [Test]
        public void RoundRobin()
        {
            var memberIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var lb = new RoundRobinLoadBalancer();

            Assert.That(lb.Count, Is.EqualTo(0));
            Assert.Throws<InvalidOperationException>(() => lb.GetMember());

            lb.NotifyMembers(memberIds);
            Assert.That(lb.Count, Is.EqualTo(3));

            Assert.That(lb.GetMember(), Is.EqualTo(memberIds[1]));
            Assert.That(lb.GetMember(), Is.EqualTo(memberIds[2]));
            Assert.That(lb.GetMember(), Is.EqualTo(memberIds[0]));
            Assert.That(lb.GetMember(), Is.EqualTo(memberIds[1]));
            Assert.That(lb.GetMember(), Is.EqualTo(memberIds[2]));
            Assert.That(lb.GetMember(), Is.EqualTo(memberIds[0]));

            memberIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

            lb.NotifyMembers(memberIds);
            Assert.That(lb.Count, Is.EqualTo(2));

            Assert.That(lb.GetMember(), Is.EqualTo(memberIds[1]));
            Assert.That(lb.GetMember(), Is.EqualTo(memberIds[0]));
            Assert.That(lb.GetMember(), Is.EqualTo(memberIds[1]));
            Assert.That(lb.GetMember(), Is.EqualTo(memberIds[0]));

            lb.NotifyMembers(new Guid[0]);
            Assert.That(lb.Count, Is.EqualTo(0));
            Assert.Throws<InvalidOperationException>(() => lb.GetMember());
        }

        [Test]
        public void ArgumentExceptions()
        {
            Assert.Throws<ArgumentNullException>(() => new RoundRobinLoadBalancer().NotifyMembers(null));
        }
    }
}
