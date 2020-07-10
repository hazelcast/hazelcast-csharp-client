using System;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class SemaphoreAcquisitionTests
    {
        [Test]
        public async Task AcquireWithTaskCompleted()
        {
            var s = new SemaphoreSlim(1);

            var task = Task.Run(async () => await s.WaitAsync());
            await task;

            var a = SemaphoreAcquisition.Create(task, s);

            Assert.That(a.Acquired, Is.True);
            Assert.That((bool) a, Is.True);
            Assert.That(s.Wait(0), Is.False);

            a.Dispose();

            Assert.That(s.Wait(0), Is.True);
            s.Release();

            s.Dispose();
        }

        [Test]
        public async Task AcquireWithTaskNotCompleted()
        {
            var s = new SemaphoreSlim(0);

            var task = Task.Run(async () => await s.WaitAsync());

            var a = SemaphoreAcquisition.Create(task, s);

            Assert.That(a.Acquired, Is.False);
            Assert.That((bool)a, Is.False);

            a.Dispose();

            Assert.That(s.Wait(0), Is.False);

            s.Release();
            await task;

            s.Dispose();
        }

        [Test]
        public async Task AcquireWithTaskOfBoolTrue()
        {
            var s = new SemaphoreSlim(1);

            var task = Task.Run(async () => await s.WaitAsync(0));
            await task;

            var a = SemaphoreAcquisition.Create(task, s);

            Assert.That(a.Acquired, Is.True);
            Assert.That((bool) a, Is.True);
            Assert.That(s.Wait(0), Is.False);

            a.Dispose();

            Assert.That(s.Wait(0), Is.True);
            s.Release();

            s.Dispose();
        }

        [Test]
        public async Task AcquireWithTaskOfBoolFalse()
        {
            var s = new SemaphoreSlim(0);

            var task = Task.Run(async () => await s.WaitAsync(0));
            await task;

            var a = SemaphoreAcquisition.Create(task, s);

            Assert.That(a.Acquired, Is.False);
            Assert.That((bool) a, Is.False);
            Assert.That(s.Wait(0), Is.False);

            a.Dispose();

            Assert.That(s.Wait(0), Is.False);

            s.Dispose();
        }

        [Test]
        public async Task AcquireAsyncAndSucceed()
        {
            var s = new SemaphoreSlim(1);

            using (var a = await s.AcquireAsync())
            {
                Assert.That(a.Acquired, Is.True);
                Assert.That((bool) a, Is.True);
                Assert.That(s.CurrentCount, Is.EqualTo(0));
            }

            Assert.That(s.CurrentCount, Is.EqualTo(1));

            using (var a = await s.AcquireAsync(default))
            {
                Assert.That(a.Acquired, Is.True);
                Assert.That((bool)a, Is.True);
                Assert.That(s.CurrentCount, Is.EqualTo(0));
            }

            Assert.That(s.CurrentCount, Is.EqualTo(1));
        }

        [Test]
        public void AcquireAsyncAndFail()
        {
            var s = new SemaphoreSlim(0);

            var cancellation = new CancellationTokenSource(100);

            Assert.ThrowsAsync<TaskCanceledException>(async () => await s.AcquireAsync(cancellation.Token));
        }

        [Test]
        public async Task TryAcquireAsyncAndSucceed()
        {
            var s = new SemaphoreSlim(1);

            using (var a = await s.TryAcquireAsync())
            {
                Assert.That(a.Acquired, Is.True);
                Assert.That((bool)a, Is.True);
                Assert.That(s.CurrentCount, Is.EqualTo(0));
            }

            Assert.That(s.CurrentCount, Is.EqualTo(1));
        }

        [Test]
        public async Task TryAcquireAsyncAndFail()
        {
            var s = new SemaphoreSlim(0);

            using (var a = await s.TryAcquireAsync())
            {
                Assert.That(a.Acquired, Is.False);
                Assert.That((bool) a, Is.False);
                Assert.That(s.CurrentCount, Is.EqualTo(0));
            }

            Assert.That(s.CurrentCount, Is.EqualTo(0));
        }

        [Test]
        public void Arguments()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () => await SemaphoreExtensions.AcquireAsync(null));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await SemaphoreExtensions.AcquireAsync(null, CancellationToken.None));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await SemaphoreExtensions.TryAcquireAsync(null));
        }
    }
}
