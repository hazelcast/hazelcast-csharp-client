﻿// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.CP;
using Hazelcast.Exceptions;
using Hazelcast.Protocol;
using Hazelcast.Testing;
using Hazelcast.Tests.CP._;
using NUnit.Framework;

namespace Hazelcast.Tests.CP
{
    namespace _
    {
        internal static class CPSubsystemExtensions
        {
            public static CPSessionManager GetSessionManager(this ICPSubsystem cp)
                => ((CPSubsystem) cp)._cpSubsystemSession;

            public static async Task CloseGroupSessionAsync(this CPSessionManager sm, ICPGroupId groupId)
            {
                await sm.CloseSessionAsync((CPGroupId) groupId, sm.GetSessionId((CPGroupId) groupId));
            }

            public static void InvalidateGroupSession(this CPSessionManager sm, ICPGroupId groupId)
                => sm.InvalidateSession((CPGroupId) groupId, sm.GetSessionId((CPGroupId) groupId));

            public static long GetGroupSessionId(this CPSessionManager sm, ICPGroupId groupId)
                => sm.GetSessionId((CPGroupId) groupId);
        }
    }
    
    [Timeout(30_000)]
    internal class FencedLockTests : MultiMembersRemoteTestBase
    {
        private IFencedLock _lock;

        protected override string RcClusterConfiguration => TestFiles.ReadAllText(this, "Cluster/cp.xml");

        [OneTimeSetUp]
        public async Task TestOneTimeSetUp()
        {
            // CP-subsystem wants at least 3 members
            for (var i = 0; i < 3; i++) await AddMember().CfAwait();
            Client = await CreateAndStartClientAsync().CfAwait();
        }

        [OneTimeTearDown]
        public async Task TestOneTimeTearDown()
        {
            if (Client == null) return;

            await Client.DisposeAsync();
            Client = null;
        }

        public IHazelcastClient Client { get; private set; }

        [TearDown]
        public async Task TearDown()
        {
            // tear down the CP session after each test, so each test runs its own session
            await Client.CPSubsystem.GetSessionManager().CloseGroupSessionAsync(_lock.GroupId);
        }

        [Test]
        public async Task TestReentrantLock()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            await _lock.LockAsync();
            await _lock.LockAsync();
            await _lock.UnlockAsync();
            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestReentrantTryLock()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

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
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

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
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);
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
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);
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
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);
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

            _lock = await Client.CPSubsystem.GetLockAsync(lockName);
            bool locked = await _lock.TryLockAsync();//cannot take the lock
            Assert.False(locked);
        }

        [Test]
        public async Task TestTryLockTimeoutWhileLockedByAnotherEndpoint()
        {
            string lockName = CreateUniqueName() + "@group1";

            await DoLockOnAnotherContextAsync(lockName);

            _lock = await Client.CPSubsystem.GetLockAsync(lockName);
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
                    var __lock = await Client.CPSubsystem.GetLockAsync(lockName);
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
                var __lock = await Client.CPSubsystem.GetLockAsync(lockName);
                return await __lock.TryLockAndGetFenceAsync();

            }).ConfigureAwait(false);
        }

        private async Task<long> DoLockOnAnotherContextAsync(string lockName, TimeSpan timeout)
        {
            //lock by another context
            return await Task.Run(async () =>
            {
                AsyncContext.RequireNew();
                var __lock = await Client.CPSubsystem.GetLockAsync(lockName);
                return await __lock.TryLockAndGetFenceAsync(timeout);

            }).ConfigureAwait(false);
        }

        [Test]
        public async Task TestReentrantLockFails()
        {
            string lockName = "non-reentrant-lock@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);
            await _lock.LockAsync();
            Assert.ThrowsAsync<LockAcquireLimitReachedException>(async () => await _lock.LockAsync());
            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestReentrantTryLockFails()
        {
            string lockName = "non-reentrant-lock@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);
            await _lock.LockAsync();

            bool locked = await _lock.TryLockAsync();
            Assert.False(locked);
            Assert.True(await _lock.IsLockedByCurrentContext());
            Assert.That(await _lock.GetLockCountAsync(), Is.EqualTo(1));
            Assert.AreNotEqual(FencedLock.InvalidFence, await _lock.GetFenceAsync());
            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestReentrantTryLockAndGetFenceFails()
        {
            string lockName = "non-reentrant-lock@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);
            await _lock.LockAsync();
            long fence1 = await _lock.GetFenceAsync();
            long fence2 = await _lock.TryLockAndGetFenceAsync();

            Assert.AreEqual(FencedLock.InvalidFence, fence2);
            Assert.True(await _lock.IsLockedByCurrentContext());
            Assert.That(await _lock.GetLockCountAsync(), Is.EqualTo(1));
            Assert.AreEqual(fence1, await _lock.GetFenceAsync());

            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestReentrantTryLockAndGetFenceTimeoutFails()
        {
            string lockName = "non-reentrant-lock@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);
            await _lock.LockAsync();

            long fence1 = await _lock.GetFenceAsync();

            long fence2 = await _lock.TryLockAndGetFenceAsync(TimeSpan.FromSeconds(1));
            Assert.AreEqual(FencedLock.InvalidFence, fence2);
            Assert.True(await _lock.IsLockedByCurrentContext());
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
                var __lock = await Client.CPSubsystem.GetLockAsync(lockName);
                await __lock.LockAsync();
                await __lock.LockAsync();
                await __lock.UnlockAsync();
                await __lock.UnlockAsync();

            }).ConfigureAwait(false);

            _lock = await Client.CPSubsystem.GetLockAsync(lockName);
            await _lock.LockAsync();//cannot take the lock
            long fence = await _lock.GetFenceAsync();
            Assert.AreNotEqual(FencedLock.InvalidFence, fence);
            Assert.True(await _lock.IsLockedByCurrentContext());
        }

        //[Test]
        public async Task TestAutoReleaseOnClientDispose()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

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
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);
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
            Assert.True(await _lock.IsLockedByCurrentContext());
        }

        private async Task AssertFencedLockNotValidAsync(IFencedLock _lock, int count = 1, long fence = -999)
        {
            if (fence != -999)
                fence = await _lock.GetFenceAsync();

            Assert.AreNotEqual(FencedLock.InvalidFence, fence);
            Assert.That(await _lock.GetLockCountAsync(), Is.EqualTo(count));
            Assert.False(await _lock.IsLockedByCurrentContext());
        }

        [Test]
        public async Task TestLockAndGetFence()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);
            long fence = await _lock.LockAndGetFenceAsync();
            Assert.AreNotEqual(FencedLock.InvalidFence, fence);
            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestTryLockAndGetFence()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);
            long fence = await _lock.TryLockAndGetFenceAsync();
            Assert.AreNotEqual(FencedLock.InvalidFence, fence);
            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestTryLockAndGetFenceTimeout()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);
            long fence = await _lock.TryLockAndGetFenceAsync(TimeSpan.FromSeconds(1));
            Assert.AreNotEqual(FencedLock.InvalidFence, fence);
            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestTryLock()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);
            bool locked = await _lock.TryLockAsync();
            Assert.True(locked);
            await AssertFencedLockValidAsync(_lock);
            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestTryLockTimeout()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);
            bool locked = await _lock.TryLockAsync(TimeSpan.FromSeconds(1));
            Assert.True(locked);
            await AssertFencedLockValidAsync(_lock);
            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestLockWhenLockedByAnotherEndpoint()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

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
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            Assert.ThrowsAsync<SynchronizationLockException>(async () => await _lock.UnlockAsync());
        }

        [Test]
        public async Task TestGetFenceWhenFree()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            Assert.ThrowsAsync<SynchronizationLockException>(async () => await _lock.GetFenceAsync());
        }

        [Test]
        public async Task TestLockedFalseWhenFree()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            Assert.False(await _lock.IsLockedAsync());
        }

        [Test]
        public async Task TestIsLockedByCurrentThreadFalseWhenFree()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            Assert.False(await _lock.IsLockedByCurrentContext());
        }

        [Test]
        public async Task TestLockCountZeroWhenFree()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

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
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

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
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            long fence = await _lock.LockAndGetFenceAsync();

            var newFenceTask = DoLockOnAnotherContextAsync(lockName, TimeSpan.FromSeconds(60));

            await _lock.UnlockAsync();

            long newFence = await newFenceTask;

            Assert.Greater(newFence, fence);

            Assert.True(await _lock.IsLockedAsync());
            Assert.False(await _lock.IsLockedByCurrentContext());
            Assert.That(await _lock.GetLockCountAsync(), Is.EqualTo(1));

            Assert.ThrowsAsync<SynchronizationLockException>(async () => await _lock.GetFenceAsync());
        }

        [Test]
        public async Task TestUnlockWhenLockedOnOtherEndpoint()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            await DoLockOnAnotherContextAsync(lockName);

            Assert.ThrowsAsync<RemoteException>(async () => await _lock.UnlockAsync());
            Assert.True(await _lock.IsLockedAsync());
            Assert.False(await _lock.IsLockedByCurrentContext());
            Assert.That(await _lock.GetLockCountAsync(), Is.EqualTo(1));
        }


        [Test]
        public async Task TestTryLockTimeoutWhenLockedOnOtherEndpoint()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            await DoLockOnAnotherContextAsync(lockName);

            long fence = await _lock.TryLockAndGetFenceAsync(TimeSpan.FromMilliseconds(100));

            Assert.AreEqual(FencedLock.InvalidFence, fence);
            Assert.True(await _lock.IsLockedAsync());
            Assert.False(await _lock.IsLockedByCurrentContext());
            Assert.That(await _lock.GetLockCountAsync(), Is.EqualTo(1));
        }

        [Test]
        public async Task TestTryLockLongTimeoutWhenLockedOnOtherEndpoint()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            await DoLockOnAnotherContextAsync(lockName);

            //1500 ms is upper bound of lock service
            long fence = await _lock.TryLockAndGetFenceAsync(TimeSpan.FromMilliseconds(1500 + 10));

            Assert.AreEqual(FencedLock.InvalidFence, fence);
            Assert.True(await _lock.IsLockedAsync());
            Assert.False(await _lock.IsLockedByCurrentContext());
            Assert.That(await _lock.GetLockCountAsync(), Is.EqualTo(1));
        }

        [Test]
        public async Task TestReentrantLockFailsWhenSessionClosed()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            long fence = await _lock.LockAndGetFenceAsync();

            var sessionService = Client.CPSubsystem.GetSessionManager();
            var sessionId = sessionService.GetGroupSessionId(_lock.GroupId);
            sessionService.InvalidateGroupSession(_lock.GroupId);

            await Task.Delay(6_000);

            var newSessionId = sessionService.GetGroupSessionId(_lock.GroupId);

            Assert.AreNotEqual(newSessionId, sessionId);

            Assert.ThrowsAsync<LockOwnershipLostException>(async () => await _lock.LockAsync());
        }

        [Test]
        public async Task TestReentrantTryLockFailsWhenSessionClosed()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            long fence = await _lock.LockAndGetFenceAsync();

            await AssertFencedLockValidAsync(_lock, 1, fence);

            var sessionService = Client.CPSubsystem.GetSessionManager();
            var sessionId = sessionService.GetGroupSessionId(_lock.GroupId);
            sessionService.InvalidateGroupSession(_lock.GroupId);

            await Task.Delay(6_000);

            var newSessionId = sessionService.GetGroupSessionId(_lock.GroupId);

            Assert.AreNotEqual(newSessionId, sessionId);

            Assert.ThrowsAsync<LockOwnershipLostException>(async () => await _lock.TryLockAsync());
        }

        [Test]
        public async Task TestReentrantTryLockTimeoutFailsWhenSessionClosed()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            long fence = await _lock.LockAndGetFenceAsync();

            await AssertFencedLockValidAsync(_lock, 1, fence);

            var sessionService = Client.CPSubsystem.GetSessionManager();
            var sessionId = sessionService.GetGroupSessionId(_lock.GroupId);
            await sessionService.CloseGroupSessionAsync(_lock.GroupId);

            var newSessionId = sessionService.GetGroupSessionId(_lock.GroupId);

            Assert.AreNotEqual(newSessionId, sessionId);
            Assert.ThrowsAsync<LockOwnershipLostException>(async () => await _lock.TryLockAsync(TimeSpan.FromSeconds(1)));
            Assert.False(await _lock.IsLockedAsync());
        }

        [Test]
        public async Task TestUnlocLockFailsWhenSessionClosed()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            long fence = await _lock.LockAndGetFenceAsync();

            await AssertFencedLockValidAsync(_lock, 1, fence);

            var sessionService = Client.CPSubsystem.GetSessionManager();
            var sessionId = sessionService.GetGroupSessionId(_lock.GroupId);
            await sessionService.CloseGroupSessionAsync(_lock.GroupId);

            var newSessionId = sessionService.GetGroupSessionId(_lock.GroupId);

            Assert.AreNotEqual(newSessionId, sessionId);
            Assert.ThrowsAsync<LockOwnershipLostException>(async () => await _lock.UnlockAsync());
            Assert.False(await _lock.IsLockedByCurrentContext());
            Assert.False(await _lock.IsLockedAsync());
        }

        [Test]
        public async Task TestUnlocLockFailsWhenSessionCreated()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            long fence = await _lock.LockAndGetFenceAsync();

            await AssertFencedLockValidAsync(_lock, 1, fence);

            var sessionService = Client.CPSubsystem.GetSessionManager();
            var sessionId = sessionService.GetGroupSessionId(_lock.GroupId);
            await sessionService.CloseGroupSessionAsync(_lock.GroupId);

            var newSessionId = sessionService.GetGroupSessionId(_lock.GroupId);

            await DoLockOnAnotherContextAsync(lockName);

            Assert.AreNotEqual(newSessionId, sessionId);
            Assert.ThrowsAsync<LockOwnershipLostException>(async () => await _lock.UnlockAsync());
            Assert.False(await _lock.IsLockedByCurrentContext());

        }

        [Test]
        public async Task TestGetFenceFailsWhenSessionCreated()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            long fence = await _lock.LockAndGetFenceAsync();

            await AssertFencedLockValidAsync(_lock, 1, fence);

            var sessionService = Client.CPSubsystem.GetSessionManager();
            var sessionId = sessionService.GetGroupSessionId(_lock.GroupId);
            await sessionService.CloseGroupSessionAsync(_lock.GroupId);

            var newSessionId = sessionService.GetGroupSessionId(_lock.GroupId);

            await DoLockOnAnotherContextAsync(lockName);

            Assert.AreNotEqual(newSessionId, sessionId);
            Assert.ThrowsAsync<LockOwnershipLostException>(async () => await _lock.GetFenceAsync());
            Assert.False(await _lock.IsLockedByCurrentContext());

        }

        [Test]
        public async Task TestFailTryLockNotAcquireSession()
        {
            string lockName = CreateUniqueName() + "@group1";

            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            await Task.Run(async () =>
            {
                AsyncContext.RequireNew();
                await _lock.LockAsync();

            }).ConfigureAwait(false);

            var sessionService = Client.CPSubsystem.GetSessionManager();
            var sessionId = sessionService.GetGroupSessionId(_lock.GroupId);

            Assert.That(sessionService.GetAcquiredSessionCount((CPGroupId)_lock.GroupId, sessionId), Is.EqualTo(1));

            long fence = await _lock.TryLockAndGetFenceAsync();

            Assert.AreEqual(FencedLock.InvalidFence, fence);
            Assert.False(await _lock.IsLockedByCurrentContext());

            Assert.That(sessionService.GetAcquiredSessionCount((CPGroupId)_lock.GroupId, sessionId), Is.EqualTo(1));
        }

        [Test]
        public async Task TestDestroy()
        {
            string lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            await _lock.LockAsync();

            await _lock.DestroyAsync();

            Assert.ThrowsAsync<RemoteException>(async () => await _lock.LockAsync());
        }
    }
}
