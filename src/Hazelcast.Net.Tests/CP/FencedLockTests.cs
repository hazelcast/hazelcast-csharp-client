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
    [Timeout(30_000)]
    internal class FencedLockTests : SingleMemberClientRemoteTestBase
    {
        private IFencedLock _lock;

        [TearDown]
        public async Task TearDown()
        {
            var sessionService = ((CPSubsystem)Client.CPSubsystem)._cpSubsystemSession;
            var sessionId = sessionService.GetSessionId((CPGroupId)_lock.CPGroupId);
            await sessionService.CloseSessionAsync((CPGroupId)_lock.CPGroupId, sessionId);
        }

        [Test]
        public async Task TestReentrantLock()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            await _lock.LockAsync();
            await _lock.LockAsync();
            await _lock.UnlockAsync();
            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestReentrantTryLock()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            bool locked = await _lock.TryLockAsync();
            Assert.IsTrue(locked);
            locked = await _lock.TryLockAsync();
            Assert.IsTrue(locked);
            await _lock.UnlockAsync();
            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestReentrantTryLockTimeout()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            bool lock1 = await _lock.TryLockAsync(TimeSpan.FromSeconds(1));
            bool lock2 = await _lock.TryLockAsync(TimeSpan.FromSeconds(1));
            Assert.True(lock1);
            Assert.True(lock2);
            await _lock.UnlockAsync();
            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestReentrantLockAndGetFence()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
            var fence1 = await _lock.LockAndGetFenceAsync();
            var fence2 = await _lock.LockAndGetFenceAsync();

            Assert.AreNotEqual(FencedLock.InvalidFence, fence1);
            Assert.AreNotEqual(FencedLock.InvalidFence, fence2);
            Assert.AreEqual(fence1, fence2);
            var fence3 = await _lock.GetFenceAsync();
            Assert.AreEqual(fence1, fence3);

            await AssertFencedLockValidAsync(_lock, 2, fence3);

            await _lock.UnlockAsync();
            await _lock.UnlockAsync();

        }

        [Test]
        public async Task TestReentrantTryLockAndGetFence()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
            var fence1 = await _lock.TryLockAndGetFenceAsync();
            var fence2 = await _lock.TryLockAndGetFenceAsync();

            Assert.AreNotEqual(FencedLock.InvalidFence, fence1);
            Assert.AreNotEqual(FencedLock.InvalidFence, fence2);
            Assert.AreEqual(fence1, fence2);

            var fence3 = await _lock.GetFenceAsync();
            Assert.AreEqual(fence1, fence3);

            await _lock.UnlockAsync();
            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestReentrantTryLockAndGetFenceTimeout()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
            var fence1 = await _lock.TryLockAndGetFenceAsync(TimeSpan.FromSeconds(1));
            var fence2 = await _lock.TryLockAndGetFenceAsync(TimeSpan.FromSeconds(1));

            Assert.AreNotEqual(FencedLock.InvalidFence, fence1);
            Assert.AreNotEqual(FencedLock.InvalidFence, fence2);
            Assert.AreEqual(fence1, fence2);

            var fence3 = await _lock.GetFenceAsync();
            Assert.AreEqual(fence1, fence3);

            await _lock.UnlockAsync();
            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestTryLockWhileLockedByAnotherEndpoint()
        {
            string lockName = CreateUniqueName() + "@group1";

            await DoLockOnAnotherContextAsync(lockName);

            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
            bool locked = await _lock.TryLockAsync();//cannot take the lock
            Assert.False(locked);
        }

        [Test]
        public async Task TestTryLockTimeoutWhileLockedByAnotherEndpoint()
        {
            string lockName = CreateUniqueName() + "@group1";

            await DoLockOnAnotherContextAsync(lockName);

            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
            bool locked = await _lock.TryLockAsync(TimeSpan.FromSeconds(1));//cannot take the lock
            Assert.False(locked);
        }

        private async Task DoLockOnAnotherContextAsync(string lockName)
        {
            //lock by another context
            await Task.Run(async () =>
            {
                try
                {
                    AsyncContext.RequireNew();
                    var __lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
                    await __lock.LockAsync();
                }
                catch (Exception )
                {                    
                }

            }).ConfigureAwait(false);
        }

        private async Task<long> DoTryLockAndGetFenceOnAnotherContextAsync(string lockName)
        {
            //lock by another context
            return await Task.Run(async () =>
            {
                AsyncContext.RequireNew();
                var __lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
                return await __lock.TryLockAndGetFenceAsync();

            }).ConfigureAwait(false);
        }

        private async Task<long> DoLockOnAnotherContextAsync(string lockName, TimeSpan timeout)
        {
            //lock by another context
            return await Task.Run(async () =>
            {
                AsyncContext.RequireNew();
                var __lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
                return await __lock.TryLockAndGetFenceAsync(timeout);

            }).ConfigureAwait(false);
        }

        [Test]
        public async Task TestReentrantLockFails()
        {
            string lockName = "non-reentrant-lock@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
            await _lock.LockAsync();
            Assert.ThrowsAsync<LockAcquireLimitReachedException>(async () => await _lock.LockAsync());
            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestReentrantTryLockFails()
        {
            string lockName = "non-reentrant-lock@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
            await _lock.LockAsync();

            bool locked = await _lock.TryLockAsync();
            Assert.False(locked);
            Assert.True(await _lock.IsLockedByCurrentThreadAsync());
            Assert.That(await _lock.GetLockCountAsync(), Is.EqualTo(1));
            Assert.AreNotEqual(FencedLock.InvalidFence, await _lock.GetFenceAsync());
            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestReentrantTryLockAndGetFenceFails()
        {
            string lockName = "non-reentrant-lock@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
            await _lock.LockAsync();
            long fence1 = await _lock.GetFenceAsync();
            long fence2 = await _lock.TryLockAndGetFenceAsync();

            Assert.AreEqual(FencedLock.InvalidFence, fence2);
            Assert.True(await _lock.IsLockedByCurrentThreadAsync());
            Assert.That(await _lock.GetLockCountAsync(), Is.EqualTo(1));
            Assert.AreEqual(fence1, await _lock.GetFenceAsync());

            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestReentrantTryLockAndGetFenceTimeoutFails()
        {
            string lockName = "non-reentrant-lock@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
            await _lock.LockAsync();

            long fence1 = await _lock.GetFenceAsync();

            long fence2 = await _lock.TryLockAndGetFenceAsync(TimeSpan.FromSeconds(1));
            Assert.AreEqual(FencedLock.InvalidFence, fence2);
            Assert.True(await _lock.IsLockedByCurrentThreadAsync());
            Assert.That(await _lock.GetLockCountAsync(), Is.EqualTo(1));
            Assert.AreEqual(fence1, await _lock.GetFenceAsync());

            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestReentrantLockAfterLockIsReleasedByAnotherEndpoint()
        {
            string lockName = CreateUniqueName() + "@group1";
            //lock by another context
            await Task.Run(async () =>
            {
                AsyncContext.RequireNew();
                var __lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
                await __lock.LockAsync();
                await __lock.LockAsync();
                await __lock.UnlockAsync();
                await __lock.UnlockAsync();

            }).ConfigureAwait(false);

            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
            await _lock.LockAsync();//cannot take the lock
            long fence = await _lock.GetFenceAsync();
            Assert.AreNotEqual(FencedLock.InvalidFence, fence);
            Assert.True(await _lock.IsLockedByCurrentThreadAsync());
        }

        //[Test]
        public async Task TestAutoReleaseOnClientDispose()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            await _lock.LockAsync();
            await Client.DisposeAsync();

            string script = $"result = instance_0.getCPSubsystem().getLock(\"{lockName}\").isLocked() ? \"1\" : \"0\";";

            var result = await RcClient.ExecuteOnControllerAsync(RcCluster.Id, script, Hazelcast.Testing.Remote.Lang.JAVASCRIPT);

            Assert.That(int.Parse(Encoding.UTF8.GetString(result.Result)), Is.EqualTo(0));
        }

        [Test]
        public async Task TestLock()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
            await _lock.LockAsync();

            await AssertFencedLockValidAsync(_lock);

            await _lock.UnlockAsync();
        }

        private async Task AssertFencedLockValidAsync(IFencedLock _lock, int count = 1, long fence = -999)
        {
            if (fence != -999)
                fence = await _lock.GetFenceAsync();

            Assert.AreNotEqual(FencedLock.InvalidFence, fence);
            Assert.True(await _lock.IsLockedAsync());
            Assert.That(await _lock.GetLockCountAsync(), Is.EqualTo(count));
            Assert.True(await _lock.IsLockedByCurrentThreadAsync());
        }

        private async Task AssertFencedLockNotValidAsync(IFencedLock _lock, int count = 1, long fence = -999)
        {
            if (fence != -999)
                fence = await _lock.GetFenceAsync();

            Assert.AreNotEqual(FencedLock.InvalidFence, fence);
            Assert.That(await _lock.GetLockCountAsync(), Is.EqualTo(count));
            Assert.False(await _lock.IsLockedByCurrentThreadAsync());
        }

        [Test]
        public async Task TestLockAndGetFence()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
            long fence = await _lock.LockAndGetFenceAsync();
            Assert.AreNotEqual(FencedLock.InvalidFence, fence);
            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestTryLockAndGetFence()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
            long fence = await _lock.TryLockAndGetFenceAsync();
            Assert.AreNotEqual(FencedLock.InvalidFence, fence);
            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestTryLockAndGetFenceTimeout()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
            long fence = await _lock.TryLockAndGetFenceAsync(TimeSpan.FromSeconds(1));
            Assert.AreNotEqual(FencedLock.InvalidFence, fence);
            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestTryLock()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
            bool locked = await _lock.TryLockAsync();
            Assert.True(locked);
            await AssertFencedLockValidAsync(_lock);
            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestTryLockTimeout()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);
            bool locked = await _lock.TryLockAsync(TimeSpan.FromSeconds(1));
            Assert.True(locked);
            await AssertFencedLockValidAsync(_lock);
            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestLockWhenLockedByAnotherEndpoint()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            await _lock.LockAsync();

            var anotherLock = DoLockOnAnotherContextAsync(lockName);

            var completedFirst = await Task.WhenAny(anotherLock, Task.Delay(TimeSpan.FromSeconds(5)));

            if (completedFirst != anotherLock)
                await AssertFencedLockValidAsync(_lock);
            else
                Assert.Fail("Other endpoint took the lock");
        }

        [Test]
        public async Task TestUnlockWhenFree()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            Assert.ThrowsAsync<SynchronizationLockException>(async () => await _lock.UnlockAsync());
        }

        [Test]
        public async Task TestGetFenceWhenFree()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            Assert.ThrowsAsync<SynchronizationLockException>(async () => await _lock.GetFenceAsync());
        }

        [Test]
        public async Task TestLockedFalseWhenFree()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            Assert.False(await _lock.IsLockedAsync());
        }

        [Test]
        public async Task TestIsLockedByCurrentThreadFalseWhenFree()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            Assert.False(await _lock.IsLockedByCurrentThreadAsync());
        }

        [Test]
        public async Task TestLockCountZeroWhenFree()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            await _lock.LockAsync();
            await _lock.UnlockAsync();

            Assert.False(await _lock.IsLockedAsync());
            Assert.That(await _lock.GetLockCountAsync(), Is.EqualTo(0));

            Assert.ThrowsAsync<SynchronizationLockException>(async () => await _lock.GetFenceAsync());
        }

        [Test]
        public async Task TestLockUnlockThenLockOnOtherEndPoint()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            long fence = await _lock.LockAndGetFenceAsync();
            await AssertFencedLockValidAsync(_lock, 1, fence);

            await _lock.UnlockAsync();

            long newFence = await DoTryLockAndGetFenceOnAnotherContextAsync(lockName);

            Assert.Greater(newFence, fence);

            Assert.ThrowsAsync<SynchronizationLockException>(async () => await _lock.GetFenceAsync());
        }

        [Test]
        public async Task TestUnlockWhenPendingLockOnOtherEndpoint()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            long fence = await _lock.LockAndGetFenceAsync();

            var newFenceTask = DoLockOnAnotherContextAsync(lockName, TimeSpan.FromSeconds(60));

            await _lock.UnlockAsync();

            long newFence = await newFenceTask;

            Assert.Greater(newFence, fence);

            Assert.True(await _lock.IsLockedAsync());
            Assert.False(await _lock.IsLockedByCurrentThreadAsync());
            Assert.That(await _lock.GetLockCountAsync(), Is.EqualTo(1));

            Assert.ThrowsAsync<SynchronizationLockException>(async () => await _lock.GetFenceAsync());
        }

        [Test]
        public async Task TestUnlockWhenLockedOnOtherEndpoint()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            await DoLockOnAnotherContextAsync(lockName);

            Assert.ThrowsAsync<SynchronizationLockException>(async () => await _lock.UnlockAsync());
            Assert.True(await _lock.IsLockedAsync());
            Assert.False(await _lock.IsLockedByCurrentThreadAsync());
            Assert.That(await _lock.GetLockCountAsync(), Is.EqualTo(1));
        }


        [Test]
        public async Task TestTryLockTimeoutWhenLockedOnOtherEndpoint()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            await DoLockOnAnotherContextAsync(lockName);

            long fence = await _lock.TryLockAndGetFenceAsync(TimeSpan.FromMilliseconds(100));

            Assert.AreEqual(FencedLock.InvalidFence, fence);
            Assert.True(await _lock.IsLockedAsync());
            Assert.False(await _lock.IsLockedByCurrentThreadAsync());
            Assert.That(await _lock.GetLockCountAsync(), Is.EqualTo(1));
        }

        [Test]
        public async Task TestTryLockLongTimeoutWhenLockedOnOtherEndpoint()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            await DoLockOnAnotherContextAsync(lockName);

            //1500 ms is upper bound of lock service
            long fence = await _lock.TryLockAndGetFenceAsync(TimeSpan.FromMilliseconds(1500 + 10));

            Assert.AreEqual(FencedLock.InvalidFence, fence);
            Assert.True(await _lock.IsLockedAsync());
            Assert.False(await _lock.IsLockedByCurrentThreadAsync());
            Assert.That(await _lock.GetLockCountAsync(), Is.EqualTo(1));
        }

        [Test]
        public async Task TestReentrantLockFailsWhenSessionClosed()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            long fence = await _lock.LockAndGetFenceAsync();

            var sessionService = ((CPSubsystem)Client.CPSubsystem)._cpSubsystemSession;
            var sessionId = sessionService.GetSessionId((CPGroupId)_lock.CPGroupId);
            sessionService.InvalidateSession((CPGroupId)_lock.GroupId, sessionId);

            await Task.Delay(6_000);

            var newSessionId = sessionService.GetSessionId((CPGroupId)_lock.CPGroupId);

            Assert.AreNotEqual(newSessionId, sessionId);

            Assert.ThrowsAsync<LockOwnershipLostException>(async () => await _lock.LockAsync());
        }

        [Test]
        public async Task TestReentrantTryLockFailsWhenSessionClosed()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            long fence = await _lock.LockAndGetFenceAsync();

            await AssertFencedLockValidAsync(_lock, 1, fence);

            var sessionService = ((CPSubsystem)Client.CPSubsystem)._cpSubsystemSession;
            var sessionId = sessionService.GetSessionId((CPGroupId)_lock.CPGroupId);
            sessionService.InvalidateSession((CPGroupId)_lock.GroupId, sessionId);

            await Task.Delay(6_000);

            var newSessionId = sessionService.GetSessionId((CPGroupId)_lock.CPGroupId);

            Assert.AreNotEqual(newSessionId, sessionId);

            Assert.ThrowsAsync<LockOwnershipLostException>(async () => await _lock.TryLockAsync());
        }

        [Test]
        public async Task TestReentrantTryLockTimeoutFailsWhenSessionClosed()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            long fence = await _lock.LockAndGetFenceAsync();

            await AssertFencedLockValidAsync(_lock, 1, fence);

            var sessionService = ((CPSubsystem)Client.CPSubsystem)._cpSubsystemSession;
            var sessionId = sessionService.GetSessionId((CPGroupId)_lock.CPGroupId);
            await sessionService.CloseSessionAsync((CPGroupId)_lock.CPGroupId, sessionId);

            var newSessionId = sessionService.GetSessionId((CPGroupId)_lock.CPGroupId);

            Assert.AreNotEqual(newSessionId, sessionId);
            Assert.ThrowsAsync<LockOwnershipLostException>(async () => await _lock.TryLockAsync(TimeSpan.FromSeconds(1)));
            Assert.False(await _lock.IsLockedAsync());
        }

        [Test]
        public async Task TestUnlocLockFailsWhenSessionClosed()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            long fence = await _lock.LockAndGetFenceAsync();

            await AssertFencedLockValidAsync(_lock, 1, fence);

            var sessionService = ((CPSubsystem)Client.CPSubsystem)._cpSubsystemSession;
            var sessionId = sessionService.GetSessionId((CPGroupId)_lock.CPGroupId);
            await sessionService.CloseSessionAsync((CPGroupId)_lock.CPGroupId, sessionId);

            var newSessionId = sessionService.GetSessionId((CPGroupId)_lock.CPGroupId);

            Assert.AreNotEqual(newSessionId, sessionId);
            Assert.ThrowsAsync<LockOwnershipLostException>(async () => await _lock.UnlockAsync());
            Assert.False(await _lock.IsLockedByCurrentThreadAsync());
            Assert.False(await _lock.IsLockedAsync());
        }

        [Test]
        public async Task TestUnlocLockFailsWhenSessionCreated()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            long fence = await _lock.LockAndGetFenceAsync();

            await AssertFencedLockValidAsync(_lock, 1, fence);

            var sessionService = ((CPSubsystem)Client.CPSubsystem)._cpSubsystemSession;
            var sessionId = sessionService.GetSessionId((CPGroupId)_lock.CPGroupId);
            await sessionService.CloseSessionAsync((CPGroupId)_lock.CPGroupId, sessionId);

            var newSessionId = sessionService.GetSessionId((CPGroupId)_lock.CPGroupId);

            await DoLockOnAnotherContextAsync(lockName);

            Assert.AreNotEqual(newSessionId, sessionId);
            Assert.ThrowsAsync<LockOwnershipLostException>(async () => await _lock.UnlockAsync());
            Assert.False(await _lock.IsLockedByCurrentThreadAsync());

        }

        [Test]
        public async Task TestGetFenceFailsWhenSessionCreated()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            long fence = await _lock.LockAndGetFenceAsync();

            await AssertFencedLockValidAsync(_lock, 1, fence);

            var sessionService = ((CPSubsystem)Client.CPSubsystem)._cpSubsystemSession;
            var sessionId = sessionService.GetSessionId((CPGroupId)_lock.CPGroupId);
            await sessionService.CloseSessionAsync((CPGroupId)_lock.CPGroupId, sessionId);

            var newSessionId = sessionService.GetSessionId((CPGroupId)_lock.CPGroupId);

            await DoLockOnAnotherContextAsync(lockName);

            Assert.AreNotEqual(newSessionId, sessionId);
            Assert.ThrowsAsync<LockOwnershipLostException>(async () => await _lock.GetFenceAsync());
            Assert.False(await _lock.IsLockedByCurrentThreadAsync());

        }

        [Test]
        public async Task TestFailTryLockNotAcquireSession()
        {
            string lockName = CreateUniqueName() + "@group1";

            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            await Task.Run(async () =>
            {
                AsyncContext.RequireNew();
                await _lock.LockAsync();

            }).ConfigureAwait(false);

            var sessionService = ((CPSubsystem)Client.CPSubsystem)._cpSubsystemSession;
            var sessionId = sessionService.GetSessionId((CPGroupId)_lock.CPGroupId);

            Assert.That(sessionService.GetAcquiredSessionCount((CPGroupId)_lock.CPGroupId, sessionId), Is.EqualTo(1));

            long fence = await _lock.TryLockAndGetFenceAsync();

            Assert.AreEqual(FencedLock.InvalidFence, fence);
            Assert.False(await _lock.IsLockedByCurrentThreadAsync());

            Assert.That(sessionService.GetAcquiredSessionCount((CPGroupId)_lock.CPGroupId, sessionId), Is.EqualTo(1));
        }

        [Test]
        public async Task TestDestroy()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetFencedLockAsync(lockName);

            await _lock.LockAsync();

            await _lock.DestroyAsync();

            Assert.ThrowsAsync<DistributedObjectDestroyedException>(async () => await _lock.LockAsync());
        }
    }
}
