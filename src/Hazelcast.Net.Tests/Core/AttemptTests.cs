using System;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class AttemptTests
    {
        [Test]
        public void Failure()
        {
            var a = Attempt.Failed;
            Assert.That(a.Success, Is.False);

            var a1 = Attempt.Fail(1, new Exception("bang"));
            Assert.That(a1.Success, Is.False);
            Assert.That(a1.Value, Is.EqualTo(1));
            Assert.That(a1.HasException, Is.True);
            Assert.That(a1.Exception?.Message, Is.EqualTo("bang"));

            var a2 = Attempt.Fail<int>(new Exception("bang"));
            Assert.That(a2.Success, Is.False);
            Assert.That(a2.Value, Is.EqualTo(default(int)));
            Assert.That(a2.HasException, Is.True);
            Assert.That(a2.Exception?.Message, Is.EqualTo("bang"));

            var ax = Attempt.Fail<int>();
            Assert.That(ax.Success, Is.False);
            Assert.That(ax.Value, Is.EqualTo(default(int)));
        }

        [Test]
        public void Success()
        {
            var a1 = Attempt.Succeed(1);
            Assert.That(a1.Success, Is.True);
            Assert.That(a1.Value, Is.EqualTo(1));
        }

        [Test]
        public void ValueOr()
        {
            var a1 = Attempt.Succeed(1);
            Assert.That(a1.ValueOr(2), Is.EqualTo(1));

            var a2 = Attempt.Fail(1);
            Assert.That(a2.ValueOr(2), Is.EqualTo(2));
        }

        [Test]
        public void ImplicitConversions()
        {
            var a1 = (Attempt<int>) 1;
            Assert.That((bool) a1, Is.True);

            var (success, value) = a1;
            Assert.That(success, Is.True);
            Assert.That(value, Is.EqualTo(1));

            int i = a1;
            Assert.That(i, Is.EqualTo(1));
        }
    }
}
