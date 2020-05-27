using System.Threading;
using System.Threading.Tasks;
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
    }
}
