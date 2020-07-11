using System.Threading;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class CancellationTokenSourceExtensionsTests
    {
        [Test]
        public void Test1()
        {
            var cancellation1 = new CancellationTokenSource();
            var cancellation2 = new CancellationTokenSource();

            var cancellation = cancellation1.LinkedWith(cancellation2.Token);

            Assert.That(cancellation1.IsCancellationRequested, Is.False);
            Assert.That(cancellation2.IsCancellationRequested, Is.False);

            Assert.That(cancellation.IsCancellationRequested, Is.False);
            cancellation1.Cancel();
            Assert.That(cancellation.IsCancellationRequested, Is.True);

            Assert.That(cancellation1.IsCancellationRequested, Is.True);
            Assert.That(cancellation2.IsCancellationRequested, Is.False);
        }

        [Test]
        public void Test2()
        {
            var cancellation1 = new CancellationTokenSource();
            var cancellation2 = new CancellationTokenSource();

            var cancellation = cancellation1.LinkedWith(cancellation2.Token);

            Assert.That(cancellation1.IsCancellationRequested, Is.False);
            Assert.That(cancellation2.IsCancellationRequested, Is.False);

            Assert.That(cancellation.IsCancellationRequested, Is.False);
            cancellation2.Cancel();
            Assert.That(cancellation.IsCancellationRequested, Is.True);

            Assert.That(cancellation1.IsCancellationRequested, Is.False);
            Assert.That(cancellation2.IsCancellationRequested, Is.True);
        }
    }
}
