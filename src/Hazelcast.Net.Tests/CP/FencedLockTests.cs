using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.CP;
using Hazelcast.Exceptions;
using Hazelcast.Protocol;
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
        public async Task TestReentrantLock()
        {
            string lockName = "lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            await fencedLock.LockAsync();
            await fencedLock.LockAsync();
            await fencedLock.UnlockAsync();
            await fencedLock.UnlockAsync();
        }

        [Test]
        public async Task TestReentrantTryLock()
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
        public async Task TestReentrantTryLockTimeout()
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
        public async Task TestReentrantLockAndGetFence()
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

            await AssertFencedLockValidAsync(fencedLock, 2, fence3);

            await fencedLock.UnlockAsync();
            await fencedLock.UnlockAsync();

        }

        [Test]
        public async Task TestReentrantTryLockAndGetFence()
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
        public async Task TestReentrantTryLockAndGetFenceTimeout()
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

            await DoLockOnAnotherContextAsync(lockName);

            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
            bool locked = await fencedLock.TryLockAsync();//cannot take the lock
            Assert.False(locked);
        }

        [Test]
        public async Task TestTryLockTimeoutWhileLockedByAnotherEndpoint()
        {
            string lockName = "lock@group1";

            await DoLockOnAnotherContextAsync(lockName);

            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
            bool locked = await fencedLock.TryLockAsync(TimeSpan.FromSeconds(1));//cannot take the lock
            Assert.False(locked);
        }

        private async Task<long> DoLockOnAnotherContextAsync(string lockName)
        {
            //lock by another context
            return await Task.Run(async () =>
             {
                 AsyncContext.RequireNew();
                 var _fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
                 return await _fencedLock.TryLockAndGetFenceAsync();

             }).ConfigureAwait(false);
        }

        private async Task<long> DoLockOnAnotherContextAsync(string lockName, TimeSpan timeout)
        {
            //lock by another context
            return await Task.Run(async () =>
            {
                AsyncContext.RequireNew();
                var _fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
                return await _fencedLock.TryLockAndGetFenceAsync(timeout);

            }).ConfigureAwait(false);
        }

        [Test]
        public async Task TestReentrantLockFails()
        {
            string lockName = "non-reentrant-lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
            await fencedLock.LockAsync();
            Assert.ThrowsAsync<LockAcquireLimitReachedException>(async () => await fencedLock.LockAsync());
            await fencedLock.UnlockAsync();
        }

        [Test]
        public async Task TestReentrantTryLockFails()
        {
            string lockName = "non-reentrant-lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
            await fencedLock.LockAsync();

            bool locked = await fencedLock.TryLockAsync();
            Assert.False(locked);
            Assert.True(await fencedLock.IsLockedByCurrentThreadAsync());
            Assert.That(await fencedLock.GetLockCountAsync(), Is.EqualTo(1));
            Assert.AreNotEqual(FencedLock.InvalidFence, await fencedLock.GetFenceAsync());
            await fencedLock.UnlockAsync();
        }

        [Test]
        public async Task TestReentrantTryLockAndGetFenceFails()
        {
            string lockName = "non-reentrant-lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
            await fencedLock.LockAsync();
            long fence1 = await fencedLock.GetFenceAsync();
            long fence2 = await fencedLock.TryLockAndGetFenceAsync();

            Assert.AreEqual(FencedLock.InvalidFence, fence2);
            Assert.True(await fencedLock.IsLockedByCurrentThreadAsync());
            Assert.That(await fencedLock.GetLockCountAsync(), Is.EqualTo(1));
            Assert.AreEqual(fence1, await fencedLock.GetFenceAsync());

            await fencedLock.UnlockAsync();
        }

        [Test]
        public async Task TestReentrantTryLockAndGetFenceTimeoutFails()
        {
            string lockName = "non-reentrant-lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
            await fencedLock.LockAsync();

            long fence1 = await fencedLock.GetFenceAsync();

            long fence2 = await fencedLock.TryLockAndGetFenceAsync(TimeSpan.FromSeconds(1));
            Assert.AreEqual(FencedLock.InvalidFence, fence2);
            Assert.True(await fencedLock.IsLockedByCurrentThreadAsync());
            Assert.That(await fencedLock.GetLockCountAsync(), Is.EqualTo(1));
            Assert.AreEqual(fence1, await fencedLock.GetFenceAsync());

            await fencedLock.UnlockAsync();
        }

        [Test]
        public async Task TestReentrantLockAfterLockIsReleasedByAnotherEndpoint()
        {
            string lockName = "lock@group1";
            //lock by another context
            await Task.Run(async () =>
            {
                AsyncContext.RequireNew();
                var _fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
                await _fencedLock.LockAsync();
                await _fencedLock.LockAsync();
                await _fencedLock.UnlockAsync();
                await _fencedLock.UnlockAsync();

            }).ConfigureAwait(false);

            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
            await fencedLock.LockAsync();//cannot take the lock
            long fence = await fencedLock.GetFenceAsync();
            Assert.AreNotEqual(FencedLock.InvalidFence, fence);
            Assert.True(await fencedLock.IsLockedByCurrentThreadAsync());
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

        [Test]
        public async Task TestLock()
        {
            string lockName = "lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
            await fencedLock.LockAsync();

            await AssertFencedLockValidAsync(fencedLock);

            await fencedLock.UnlockAsync();
        }

        private async Task AssertFencedLockValidAsync(IFencedLock fencedLock, int count = 1, long fence = -999)
        {
            if (fence != -999)
                fence = await fencedLock.GetFenceAsync();

            Assert.AreNotEqual(FencedLock.InvalidFence, fence);
            Assert.True(await fencedLock.IsLockedAsync());
            Assert.That(await fencedLock.GetLockCountAsync(), Is.EqualTo(count));
            Assert.True(await fencedLock.IsLockedByCurrentThreadAsync());
        }

        private async Task AssertFencedLockNotValidAsync(IFencedLock fencedLock, int count = 1, long fence = -999)
        {
            if (fence != -999)
                fence = await fencedLock.GetFenceAsync();

            Assert.AreNotEqual(FencedLock.InvalidFence, fence);
            Assert.That(await fencedLock.GetLockCountAsync(), Is.EqualTo(count));
            Assert.False(await fencedLock.IsLockedByCurrentThreadAsync());
        }

        [Test]
        public async Task TestLockAndGetFence()
        {
            string lockName = "lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
            long fence = await fencedLock.LockAndGetFenceAsync();
            Assert.AreNotEqual(FencedLock.InvalidFence, fence);
            await fencedLock.UnlockAsync();
        }

        [Test]
        public async Task TestTryLockAndGetFence()
        {
            string lockName = "lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
            long fence = await fencedLock.TryLockAndGetFenceAsync();
            Assert.AreNotEqual(FencedLock.InvalidFence, fence);
            await fencedLock.UnlockAsync();
        }

        [Test]
        public async Task TestTryLockAndGetFenceTimeout()
        {
            string lockName = "lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
            long fence = await fencedLock.TryLockAndGetFenceAsync(TimeSpan.FromSeconds(1));
            Assert.AreNotEqual(FencedLock.InvalidFence, fence);
            await fencedLock.UnlockAsync();
        }

        [Test]
        public async Task TestTryLock()
        {
            string lockName = "lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
            bool locked = await fencedLock.TryLockAsync();
            Assert.True(locked);
            await AssertFencedLockValidAsync(fencedLock);
            await fencedLock.UnlockAsync();
        }

        [Test]
        public async Task TestTryLockTimeout()
        {
            string lockName = "lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
            bool locked = await fencedLock.TryLockAsync(TimeSpan.FromSeconds(1));
            Assert.True(locked);
            await AssertFencedLockValidAsync(fencedLock);
            await fencedLock.UnlockAsync();
        }

        [Test]
        public async Task TestLockWhenLockedByAnotherEndpoint()
        {
            string lockName = "lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            await fencedLock.LockAsync();

            var anotherLock = DoLockOnAnotherContextAsync(lockName);

            var completedFirst = await Task.WhenAny(anotherLock, Task.Delay(TimeSpan.FromSeconds(5)));

            if (completedFirst != anotherLock)
                await AssertFencedLockValidAsync(fencedLock);
            else
                Assert.Fail("Other endpoint took the lock");
        }

        [Test]
        public async Task TestUnlockWhenFree()
        {
            string lockName = "lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            Assert.ThrowsAsync<SynchronizationLockException>(async () => await fencedLock.UnlockAsync());
        }

        [Test]
        public async Task TestGetFenceWhenFree()
        {
            string lockName = "lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            Assert.ThrowsAsync<SynchronizationLockException>(async () => await fencedLock.GetFenceAsync());
        }

        [Test]
        public async Task TestLockedFalseWhenFree()
        {
            string lockName = "lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            Assert.False(await fencedLock.IsLockedAsync());
        }

        [Test]
        public async Task TestIsLockedByCurrentThreadFalseWhenFree()
        {
            string lockName = "lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            Assert.False(await fencedLock.IsLockedByCurrentThreadAsync());
        }

        [Test]
        public async Task TestLockCountZeroWhenFree()
        {
            string lockName = "lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            await fencedLock.LockAsync();
            await fencedLock.UnlockAsync();

            Assert.False(await fencedLock.IsLockedAsync());
            Assert.That(await fencedLock.GetLockCountAsync(), Is.EqualTo(0));

            Assert.ThrowsAsync<SynchronizationLockException>(async () => await fencedLock.GetFenceAsync());
        }

        [Test]
        public async Task TestLockUnlockThenLockOnOtherEndPoint()
        {
            string lockName = "lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            long fence = await fencedLock.LockAndGetFenceAsync();
            await AssertFencedLockValidAsync(fencedLock, 1, fence);

            await fencedLock.UnlockAsync();

            long newFence = await DoLockOnAnotherContextAsync(lockName);

            Assert.Greater(newFence, fence);

            Assert.ThrowsAsync<SynchronizationLockException>(async () => await fencedLock.GetFenceAsync());
        }

        [Test]
        public async Task TestUnlockWhenPendingLockOnOtherEndpoint()
        {
            string lockName = "lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            long fence = await fencedLock.LockAndGetFenceAsync();

            var newFenceTask = DoLockOnAnotherContextAsync(lockName, TimeSpan.FromSeconds(60));

            await fencedLock.UnlockAsync();

            long newFence = await newFenceTask;

            Assert.Greater(newFence, fence);

            Assert.True(await fencedLock.IsLockedAsync());
            Assert.False(await fencedLock.IsLockedByCurrentThreadAsync());
            Assert.That(await fencedLock.GetLockCountAsync(), Is.EqualTo(1));

            Assert.ThrowsAsync<SynchronizationLockException>(async () => await fencedLock.GetFenceAsync());
        }

        [Test]
        public async Task TestUnlockWhenLockedOnOtherEndpoint()
        {
            string lockName = "lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            await DoLockOnAnotherContextAsync(lockName);

            Assert.ThrowsAsync<SynchronizationLockException>(async () => await fencedLock.UnlockAsync());
            Assert.True(await fencedLock.IsLockedAsync());
            Assert.False(await fencedLock.IsLockedByCurrentThreadAsync());
            Assert.That(await fencedLock.GetLockCountAsync(), Is.EqualTo(1));
        }


        [Test]
        public async Task TestTryLockTimeoutWhenLockedOnOtherEndpoint()
        {
            string lockName = "lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            await DoLockOnAnotherContextAsync(lockName);

            long fence = await fencedLock.TryLockAndGetFenceAsync(TimeSpan.FromMilliseconds(100));

            Assert.AreEqual(FencedLock.InvalidFence, fence);
            Assert.True(await fencedLock.IsLockedAsync());
            Assert.False(await fencedLock.IsLockedByCurrentThreadAsync());
            Assert.That(await fencedLock.GetLockCountAsync(), Is.EqualTo(1));
        }

        [Test]
        public async Task TestTryLockLongTimeoutWhenLockedOnOtherEndpoint()
        {
            string lockName = "lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            await DoLockOnAnotherContextAsync(lockName);

            //1500 ms is upper bound of lock service
            long fence = await fencedLock.TryLockAndGetFenceAsync(TimeSpan.FromMilliseconds(1500 + 10));

            Assert.AreEqual(FencedLock.InvalidFence, fence);
            Assert.True(await fencedLock.IsLockedAsync());
            Assert.False(await fencedLock.IsLockedByCurrentThreadAsync());
            Assert.That(await fencedLock.GetLockCountAsync(), Is.EqualTo(1));
        }

        [Test]
        public async Task TestReentrantLockFailsWhenSessionClosed()
        {
            string lockName = "lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            long fence = await fencedLock.LockAndGetFenceAsync();

            var sessionService = ((CPSubsystem)Client.CPSubsystem)._cpSubsystemSession;
            var sessionId = sessionService.GetSessionId((CPGroupId)fencedLock.CPGroupId);
            sessionService.InvalidateSession((CPGroupId)fencedLock.GroupId, sessionId);

            await Task.Delay(6_000);

            var newSessionId = sessionService.GetSessionId((CPGroupId)fencedLock.CPGroupId);

            Assert.AreNotEqual(newSessionId, sessionId);

            Assert.ThrowsAsync<LockOwnershipLostException>(async () => await fencedLock.LockAsync());
        }

        [Test]
        public async Task TestReentrantTryLockFailsWhenSessionClosed()
        {
            string lockName = "lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            long fence = await fencedLock.LockAndGetFenceAsync();

            await AssertFencedLockValidAsync(fencedLock, 1, fence);

            var sessionService = ((CPSubsystem)Client.CPSubsystem)._cpSubsystemSession;
            var sessionId = sessionService.GetSessionId((CPGroupId)fencedLock.CPGroupId);
            sessionService.InvalidateSession((CPGroupId)fencedLock.GroupId, sessionId);

            await Task.Delay(6_000);

            var newSessionId = sessionService.GetSessionId((CPGroupId)fencedLock.CPGroupId);

            Assert.AreNotEqual(newSessionId, sessionId);

            Assert.ThrowsAsync<LockOwnershipLostException>(async () => await fencedLock.TryLockAsync());
        }

        [Test]
        public async Task TestReentrantTryLockTimeoutFailsWhenSessionClosed()
        {
            string lockName = "lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            long fence = await fencedLock.LockAndGetFenceAsync();

            await AssertFencedLockValidAsync(fencedLock, 1, fence);

            var sessionService = ((CPSubsystem)Client.CPSubsystem)._cpSubsystemSession;
            var sessionId = sessionService.GetSessionId((CPGroupId)fencedLock.CPGroupId);
            await sessionService.CloseSessionAsync((CPGroupId)fencedLock.CPGroupId, sessionId);

            var newSessionId = sessionService.GetSessionId((CPGroupId)fencedLock.CPGroupId);

            Assert.AreNotEqual(newSessionId, sessionId);
            Assert.ThrowsAsync<LockOwnershipLostException>(async () => await fencedLock.TryLockAsync(TimeSpan.FromSeconds(1)));
            Assert.False(await fencedLock.IsLockedAsync());
        }

        [Test]
        public async Task TestUnlocLockFailsWhenSessionClosed()
        {
            string lockName = "lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            long fence = await fencedLock.LockAndGetFenceAsync();

            await AssertFencedLockValidAsync(fencedLock, 1, fence);

            var sessionService = ((CPSubsystem)Client.CPSubsystem)._cpSubsystemSession;
            var sessionId = sessionService.GetSessionId((CPGroupId)fencedLock.CPGroupId);
            await sessionService.CloseSessionAsync((CPGroupId)fencedLock.CPGroupId, sessionId);

            var newSessionId = sessionService.GetSessionId((CPGroupId)fencedLock.CPGroupId);

            Assert.AreNotEqual(newSessionId, sessionId);
            Assert.ThrowsAsync<LockOwnershipLostException>(async () => await fencedLock.UnlockAsync());
            Assert.False(await fencedLock.IsLockedByCurrentThreadAsync());
            Assert.False(await fencedLock.IsLockedAsync());
        }

        [Test]
        public async Task TestUnlocLockFailsWhenSessionCreated()
        {
            string lockName = "lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            long fence = await fencedLock.LockAndGetFenceAsync();

            await AssertFencedLockValidAsync(fencedLock, 1, fence);

            var sessionService = ((CPSubsystem)Client.CPSubsystem)._cpSubsystemSession;
            var sessionId = sessionService.GetSessionId((CPGroupId)fencedLock.CPGroupId);
            await sessionService.CloseSessionAsync((CPGroupId)fencedLock.CPGroupId, sessionId);

            var newSessionId = sessionService.GetSessionId((CPGroupId)fencedLock.CPGroupId);

            await DoLockOnAnotherContextAsync(lockName);

            Assert.AreNotEqual(newSessionId, sessionId);
            Assert.ThrowsAsync<LockOwnershipLostException>(async () => await fencedLock.UnlockAsync());
            Assert.False(await fencedLock.IsLockedByCurrentThreadAsync());

        }

        [Test]
        public async Task TestGetFenceFailsWhenSessionCreated()
        {
            string lockName = "lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            long fence = await fencedLock.LockAndGetFenceAsync();

            await AssertFencedLockValidAsync(fencedLock, 1, fence);

            var sessionService = ((CPSubsystem)Client.CPSubsystem)._cpSubsystemSession;
            var sessionId = sessionService.GetSessionId((CPGroupId)fencedLock.CPGroupId);
            await sessionService.CloseSessionAsync((CPGroupId)fencedLock.CPGroupId, sessionId);

            var newSessionId = sessionService.GetSessionId((CPGroupId)fencedLock.CPGroupId);

            await DoLockOnAnotherContextAsync(lockName);

            Assert.AreNotEqual(newSessionId, sessionId);
            Assert.ThrowsAsync<LockOwnershipLostException>(async () => await fencedLock.GetFenceAsync());
            Assert.False(await fencedLock.IsLockedByCurrentThreadAsync());

        }

        [Test]
        public async Task TestFailTryLockNotAcquireSession()
        {
            string lockName = "lock@group1";

            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            await Task.Run(async () =>
            {
                AsyncContext.RequireNew();
                await fencedLock.LockAsync();

            }).ConfigureAwait(false);

            var sessionService = ((CPSubsystem)Client.CPSubsystem)._cpSubsystemSession;
            var sessionId = sessionService.GetSessionId((CPGroupId)fencedLock.CPGroupId);

            Assert.That(sessionService.GetAcquiredSessionCount((CPGroupId)fencedLock.CPGroupId, sessionId), Is.EqualTo(1));

            long fence = await fencedLock.TryLockAndGetFenceAsync();

            Assert.AreEqual(FencedLock.InvalidFence, fence);
            Assert.False(await fencedLock.IsLockedByCurrentThreadAsync());

            Assert.That(sessionService.GetAcquiredSessionCount((CPGroupId)fencedLock.CPGroupId, sessionId), Is.EqualTo(1));
        }

        [Test]
        public async Task TestDestroy()
        {
            string lockName = "lock@group1";
            var fencedLock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            await fencedLock.LockAsync();

            await fencedLock.DestroyAsync();

            Assert.ThrowsAsync<DistributedObjectDestroyedException>(async () => await fencedLock.LockAsync());
        }
    }
}
