using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.DotNet
{
    [TestFixture]
    public class SemaphoreTests
    {
        [Test]
        public async Task TakeWithoutWaiting()
        {
            var semaphore = new SemaphoreSlim(1);

            await semaphore.WaitAsync().CAF();

            // zero means we could not enter, if we tried
            Assert.AreEqual(0, semaphore.CurrentCount);

            // better: either take it immediately, or fail
            var taken = await semaphore.WaitAsync(0).CAF();

            Assert.IsFalse(taken);
        }

        [Test]
        [Timeout(10_000)]
        public async Task AcquireSuccess()
        {
            var semaphore = new SemaphoreSlim(1);

            var acquired = await semaphore.AcquireAsync().CAF();
            Assert.IsTrue(acquired.Acquired);
            acquired.Dispose();

            acquired = await semaphore.AcquireAsync().CAF();
            Assert.IsTrue(acquired.Acquired);
            acquired.Dispose();
        }

        [Test]
        public void AcquireFail()
        {
            var semaphore = new SemaphoreSlim(0);
            var cancellation = new CancellationTokenSource(100);
            Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                using var acquired = await semaphore.AcquireAsync(cancellation.Token).CAF();
            });
        }

        [Test]
        [Timeout(10_000)]
        public async Task TryAcquireSuccess()
        {
            var semaphore = new SemaphoreSlim(1);

            var acquired = await semaphore.TryAcquireAsync().CAF();
            Assert.IsTrue(acquired.Acquired);
            acquired.Dispose();

            acquired = await semaphore.TryAcquireAsync().CAF();
            Assert.IsTrue(acquired.Acquired);
            acquired.Dispose();
        }

        [Test]
        public async Task TryAcquireFail()
        {
            var semaphore = new SemaphoreSlim(0);
            using var acquired = await semaphore.TryAcquireAsync().CAF();
            Assert.IsFalse(acquired.Acquired);
        }
    }
}
