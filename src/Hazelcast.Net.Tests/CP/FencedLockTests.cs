using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.CP;
using Hazelcast.Exceptions;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.CP
{
    internal class FencedLockTests : SingleMemberClientRemoteTestBase
    {
        private IDisposable HConsoleForTest()

            => HConsole.Capture(options => options
                .ClearAll()
                .Configure().SetMaxLevel()
                .Configure(this).SetPrefix("TEST")
                .Configure<AsyncContext>().SetMinLevel()
                .Configure<FencedLock>().SetIndent(1).SetLevel(0).SetPrefix("FENCEDLOCK"));

        [Test]
        public async Task TestLock()
        {
            string lockName = "lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            await fencedLock.LockAsync();
            await fencedLock.LockAsync();
            await fencedLock.UnlockAsync();
            await fencedLock.UnlockAsync();
        }

        [Test]
        public async Task TestTryLock()
        {
            string lockName = "lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            bool locked = await fencedLock.TryLockAsync();
            Assert.IsTrue(locked);
            locked = await fencedLock.TryLockAsync();
            Assert.IsTrue(locked);
            await fencedLock.UnlockAsync();
            await fencedLock.UnlockAsync();
        }
        [Test]
        public async Task TestTryLockTimeout()
        {
            string lockName = "lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            bool lock1 = await fencedLock.TryLockAsync(TimeSpan.FromSeconds(1));
            bool lock2 = await fencedLock.TryLockAsync(TimeSpan.FromSeconds(1));
            Assert.True(lock1);
            Assert.True(lock2);
            await fencedLock.UnlockAsync();
            await fencedLock.UnlockAsync();
        }

        [Test]
        public async Task TestLockAndGetFence()
        {
            string lockName = "lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
            var fence1 = await fencedLock.LockAndGetFenceAsync();
            var fence2 = await fencedLock.LockAndGetFenceAsync();

            Assert.AreNotEqual(FencedLock.InvalidFence, fence1);
            Assert.AreNotEqual(FencedLock.InvalidFence, fence2);
            Assert.AreEqual(fence1, fence2);
            var fence3 = await fencedLock.GetFenceAsync();
            Assert.AreEqual(fence1, fence3);

            await fencedLock.UnlockAsync();
            await fencedLock.UnlockAsync();

        }



        [Test]
        public async Task TestTryLockAndGetFence()
        {
            string lockName = "lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
            var fence1 = await fencedLock.TryLockAndGetFenceAsync();
            var fence2 = await fencedLock.TryLockAndGetFenceAsync();

            Assert.AreNotEqual(FencedLock.InvalidFence, fence1);
            Assert.AreNotEqual(FencedLock.InvalidFence, fence2);
            Assert.AreEqual(fence1, fence2);

            var fence3 = await fencedLock.GetFenceAsync();
            Assert.AreEqual(fence1, fence3);

            await fencedLock.UnlockAsync();
            await fencedLock.UnlockAsync();
        }

        [Test]
        public async Task TestTryLockAndGetFenceTimeout()
        {
            string lockName = "lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
            var fence1 = await fencedLock.TryLockAndGetFenceAsync(TimeSpan.FromSeconds(1));
            var fence2 = await fencedLock.TryLockAndGetFenceAsync(TimeSpan.FromSeconds(1));

            Assert.AreNotEqual(FencedLock.InvalidFence, fence1);
            Assert.AreNotEqual(FencedLock.InvalidFence, fence2);
            Assert.AreEqual(fence1, fence2);

            var fence3 = await fencedLock.GetFenceAsync();
            Assert.AreEqual(fence1, fence3);

            await fencedLock.UnlockAsync();
            await fencedLock.UnlockAsync();
        }

        [Test]
        public async Task TestTryLockWhileLockedByAnotherEndpoint()
        {
            string lockName = "lock@group1";
            //lock by another context
            await Task.Run(async () =>
            {
                AsyncContext.RequireNew();
                var _fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
                await _fencedLock.LockAsync();

            }).ConfigureAwait(false);

            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
            bool locked = await fencedLock.TryLockAsync();//cannot take the lock
            Assert.False(locked);
        }

        [Test]
        public async Task TestTryLockTimeoutWhileLockedByAnotherEndpoint()
        {
            string lockName = "lock@group1";
            //lock by another context
            await Task.Run(async () =>
            {
                AsyncContext.RequireNew();
                var _fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
                await _fencedLock.LockAsync();

            }).ConfigureAwait(false);

            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
            bool locked = await fencedLock.TryLockAsync(TimeSpan.FromSeconds(1));//cannot take the lock
            Assert.False(locked);
        }



        [Test]
        public async Task TestAutoReleaseOnClientDispose()
        {
            string lockName = "lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            await fencedLock.LockAsync();
            await Client.DisposeAsync();

            string script = $"result = instance_0.getCPSubsystem().getLock(\"{lockName}\").isLocked() ? \"1\" : \"0\";";

            var result = await RcClient.ExecuteOnControllerAsync(RcCluster.Id, script, Hazelcast.Testing.Remote.Lang.JAVASCRIPT);

            Assert.That(int.Parse(Encoding.UTF8.GetString(result.Result)), Is.EqualTo(0));
        }



    }
}
