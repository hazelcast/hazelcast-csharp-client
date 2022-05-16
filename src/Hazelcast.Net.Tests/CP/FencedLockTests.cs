// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
        private IDisposable HConsoleForTest()

            => HConsole.Capture(options => options
                .Configure().SetMinLevel()
                .Configure(this).SetPrefix("TEST")
                .Configure(this).SetMaxLevel()
                .Configure<FencedLock>().SetPrefix("TEST")
                .Configure<FencedLock>().SetMaxLevel());

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
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            await _lock.LockAsync();
            await _lock.LockAsync();
            await _lock.UnlockAsync();
            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestReentrantTryLock()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            Assert.IsTrue(await _lock.TryLockAsync());
            Assert.IsTrue(await _lock.TryLockAsync());
            await _lock.UnlockAsync();
            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestReentrantTryLockTimeout()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            Assert.True(await _lock.TryLockAsync(TimeSpan.FromSeconds(1)));
            Assert.True(await _lock.TryLockAsync(TimeSpan.FromSeconds(1)));
            await _lock.UnlockAsync();
            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestReentrantLockAndGetFence()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);
            var fence1 = await _lock.LockAndGetFenceAsync();
            var fence2 = await _lock.LockAndGetFenceAsync();

            Assert.AreNotEqual(FencedLock.InvalidFence, fence1);
            Assert.AreNotEqual(FencedLock.InvalidFence, fence2);
            Assert.AreEqual(fence1, fence2);

            var fence3 = await _lock.GetFenceAsync();
            Assert.AreEqual(fence1, fence3);

            await AssertIsLockedAsync(_lock, 2, fence3);

            await _lock.UnlockAsync();
            await _lock.UnlockAsync();

        }

        [Test]
        public async Task TestReentrantTryLockAndGetFence()
        {
            var lockName = CreateUniqueName() + "@group1";
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
            var lockName = CreateUniqueName() + "@group1";
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
        public async Task TestTryLockWhileLockedByAnotherContext()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var otherContext = new OtherContextLock(_lock);
            await otherContext.LockAsync();

            try
            {
                Assert.False(await _lock.TryLockAsync()); // cannot take the lock
            }
            finally
            {
                await otherContext.UnlockAsync();
            }
        }

        [Test]
        public async Task TestTryLockTimeoutWhileLockedByAnotherContext()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var otherContext = new OtherContextLock(_lock);
            await otherContext.LockAsync();

            try
            {
                Assert.False(await _lock.TryLockAsync(TimeSpan.FromSeconds(1))); // cannot take the lock
            }
            finally
            {
                await otherContext.UnlockAsync();
            }
        }

        [Test]
        public async Task TestReentrantLockFails()
        {
            const string lockName = "non-reentrant-lock@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            await _lock.LockAsync();
            await AssertEx.ThrowsAsync<LockAcquireLimitReachedException>(async () => await _lock.LockAsync());

            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestReentrantTryLockFails()
        {
            const string lockName = "non-reentrant-lock@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            await _lock.LockAsync();

            Assert.False(await _lock.TryLockAsync());
            Assert.True(await _lock.IsLockedByCurrentContextAsync());
            Assert.That(await _lock.GetLockCountAsync(), Is.EqualTo(1));
            Assert.AreNotEqual(FencedLock.InvalidFence, await _lock.GetFenceAsync());

            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestReentrantTryLockAndGetFenceFails()
        {
            const string lockName = "non-reentrant-lock@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            await _lock.LockAsync();

            var fence1 = await _lock.GetFenceAsync();
            var fence2 = await _lock.TryLockAndGetFenceAsync();

            Assert.AreNotEqual(FencedLock.InvalidFence, fence1);
            Assert.AreEqual(FencedLock.InvalidFence, fence2);
            Assert.True(await _lock.IsLockedByCurrentContextAsync());
            Assert.That(await _lock.GetLockCountAsync(), Is.EqualTo(1));
            Assert.AreEqual(fence1, await _lock.GetFenceAsync());

            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestReentrantTryLockAndGetFenceTimeoutFails()
        {
            const string lockName = "non-reentrant-lock@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            await _lock.LockAsync();

            var fence1 = await _lock.GetFenceAsync();
            var fence2 = await _lock.TryLockAndGetFenceAsync(TimeSpan.FromSeconds(1));

            Assert.AreNotEqual(FencedLock.InvalidFence, fence1);
            Assert.AreEqual(FencedLock.InvalidFence, fence2);
            Assert.True(await _lock.IsLockedByCurrentContextAsync());
            Assert.That(await _lock.GetLockCountAsync(), Is.EqualTo(1));
            Assert.AreEqual(fence1, await _lock.GetFenceAsync());

            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestReentrantLockAfterLockIsReleasedByAnotherEndpoint()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            // same as Java test except that *other* locks and releases and *we*
            // test the re-entrance whereas Java does it the other way round, but
            // the result is the same - testing the very same thing

            var otherContext = new OtherContextLock(_lock);
            await otherContext.LockAsync();

            async Task Reenter()
            {
                await _lock.LockAsync();
                await _lock.LockAsync();
                await _lock.UnlockAsync();
                await _lock.UnlockAsync();
            }

            var reentering = Reenter();
            await otherContext.UnlockAsync();

            var task = await Task.WhenAny(reentering, Task.Delay(TimeSpan.FromSeconds(10)));
            Assert.That(task, Is.EqualTo(reentering));
        }

        [Test]
        public async Task TestAutoReleaseOnClientDispose()
        {
            var lockName = CreateUniqueName() + "@group1";
            var client = await CreateAndStartClientAsync().CfAwait(); // use our own client since we are going to dispose it
            _lock = await client.CPSubsystem.GetLockAsync(lockName);

            await _lock.LockAsync();
            await client.DisposeAsync();

            var script = $"result = instance_0.getCPSubsystem().getLock(\"{lockName}\").isLocked() ? \"1\" : \"0\";";

            await AssertEx.SucceedsEventually(async () =>
            {
                var result = await RcClient.ExecuteOnControllerAsync(RcCluster.Id, script, Hazelcast.Testing.Remote.Lang.JAVASCRIPT);
                Assert.That(int.Parse(Encoding.UTF8.GetString(result.Result)), Is.EqualTo(0));
            }, 10_000, 1000);
        }

        [Test]
        public async Task TestLock()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            await _lock.LockAsync();
            await AssertIsLockedAsync(_lock);
            await _lock.UnlockAsync();
        }

        private static async Task AssertIsLockedAsync(IFencedLock fencedLock, int count = 1, long? fence = default)
        {
            fence ??= await fencedLock.GetFenceAsync();
            Assert.AreNotEqual(FencedLock.InvalidFence, fence);

            Assert.True(await fencedLock.IsLockedAsync());
            Assert.That(await fencedLock.GetLockCountAsync(), Is.EqualTo(count));
            Assert.True(await fencedLock.IsLockedByCurrentContextAsync());
        }

        [Test]
        public async Task TestLockAndGetFence()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);
            var fence = await _lock.LockAndGetFenceAsync();
            Assert.AreNotEqual(FencedLock.InvalidFence, fence);
            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestTryLockAndGetFence()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);
            var fence = await _lock.TryLockAndGetFenceAsync();
            Assert.AreNotEqual(FencedLock.InvalidFence, fence);
            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestTryLockAndGetFenceTimeout()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);
            var fence = await _lock.TryLockAndGetFenceAsync(TimeSpan.FromSeconds(1));
            Assert.AreNotEqual(FencedLock.InvalidFence, fence);
            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestTryLock()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);
            var locked = await _lock.TryLockAsync();
            Assert.True(locked);
            await AssertIsLockedAsync(_lock);
            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestTryLockTimeout()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);
            var locked = await _lock.TryLockAsync(TimeSpan.FromSeconds(1));
            Assert.True(locked);
            await AssertIsLockedAsync(_lock);
            await _lock.UnlockAsync();
        }

        [Test]
        public async Task TestLockWhenLockedByAnotherContext()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            await _lock.LockAsync();

            var otherContext = new OtherContextLock(_lock);
            var otherContextLocking = otherContext.LockAsync();

            await Task.Delay(TimeSpan.FromSeconds(5));
            Assert.That(otherContextLocking.IsCompleted, Is.False);
            await AssertIsLockedAsync(_lock);

            Assert.That(otherContextLocking.IsCompleted, Is.False);
            await _lock.UnlockAsync();
            await otherContextLocking;
        }

        [Test]
        public async Task TestUnlockWhenFree()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            await AssertEx.ThrowsAsync<SynchronizationLockException>(async () => await _lock.UnlockAsync());
        }

        [Test]
        public async Task TestGetFenceWhenFree()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            await AssertEx.ThrowsAsync<SynchronizationLockException>(async () => await _lock.GetFenceAsync());
        }

        [Test]
        public async Task TestLockedFalseWhenFree()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            Assert.False(await _lock.IsLockedAsync());
        }

        [Test]
        public async Task TestIsLockedByCurrentThreadFalseWhenFree()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            Assert.False(await _lock.IsLockedByCurrentContextAsync());
        }

        [Test]
        public async Task TestLockCountZeroWhenFree()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            await _lock.LockAsync();
            await _lock.UnlockAsync();

            Assert.False(await _lock.IsLockedAsync());
            Assert.That(await _lock.GetLockCountAsync(), Is.EqualTo(0));

            await AssertEx.ThrowsAsync<SynchronizationLockException>(async () => await _lock.GetFenceAsync());
        }

        [Test]
        public async Task TestLockUnlockThenLockOnOtherContext()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var fence = await _lock.LockAndGetFenceAsync();
            await AssertIsLockedAsync(_lock, 1, fence);
            await _lock.UnlockAsync();

            var otherContext = new OtherContextLock(_lock);
            var otherFence = await otherContext.TryLockAndGetFenceAsync();

            Assert.Greater(otherFence, fence);
            await AssertEx.ThrowsAsync<SynchronizationLockException>(async () => await _lock.GetFenceAsync());
        }

        [Test]
        public async Task TestUnlockWhenPendingLockOnOtherContext()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var fence = await _lock.LockAndGetFenceAsync();

            var otherContext = new OtherContextLock(_lock);
            var otherFenceTask = otherContext.TryLockAndGetFenceAsync(TimeSpan.FromSeconds(60));

            await _lock.UnlockAsync();

            var otherFence = await otherFenceTask;

            Assert.Greater(otherFence, fence);

            Assert.True(await _lock.IsLockedAsync());
            Assert.False(await _lock.IsLockedByCurrentContextAsync());
            Assert.That(await _lock.GetLockCountAsync(), Is.EqualTo(1));

            await AssertEx.ThrowsAsync<SynchronizationLockException>(async () => await _lock.GetFenceAsync());

            await otherContext.UnlockAsync();
        }

        [Test]
        public async Task TestUnlockWhenLockedOnOtherContext()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var otherContext = new OtherContextLock(_lock);
            await otherContext.LockAsync();

            Assert.ThrowsAsync<RemoteException>(async () => await _lock.UnlockAsync());
            Assert.True(await _lock.IsLockedAsync());
            Assert.False(await _lock.IsLockedByCurrentContextAsync());
            Assert.That(await _lock.GetLockCountAsync(), Is.EqualTo(1));
        }

        [Test]
        public async Task TestTryLockTimeoutWhenLockedOnOtherContext()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var otherContext = new OtherContextLock(_lock);
            await otherContext.LockAsync();

            var fence = await _lock.TryLockAndGetFenceAsync(TimeSpan.FromMilliseconds(100));

            Assert.AreEqual(FencedLock.InvalidFence, fence);
            Assert.True(await _lock.IsLockedAsync());
            Assert.False(await _lock.IsLockedByCurrentContextAsync());
            Assert.That(await _lock.GetLockCountAsync(), Is.EqualTo(1));
        }

        [Test]
        public async Task TestTryLockLongTimeoutWhenLockedOnOtherContext()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var otherContext = new OtherContextLock(_lock);
            await otherContext.LockAsync();

            // "1500ms is upper bound of lock service"
            // this is a direct copy of a Java test which comes without comments,
            // and I have no idea what is the purpose of the test (note that 1500ms
            // is not really a "max timeout" and a timeout of 20s is accepted as
            // well...)
            var fence = await _lock.TryLockAndGetFenceAsync(TimeSpan.FromMilliseconds(1500 + 1));

            Assert.AreEqual(FencedLock.InvalidFence, fence);
            Assert.True(await _lock.IsLockedAsync());
            Assert.False(await _lock.IsLockedByCurrentContextAsync());
            Assert.That(await _lock.GetLockCountAsync(), Is.EqualTo(1));
        }

        [Test]
        public async Task TestReentrantLockFailsWhenSessionClosed()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var fence = await _lock.LockAndGetFenceAsync();
            await AssertIsLockedAsync(_lock, 1, fence);

            var sessionService = Client.CPSubsystem.GetSessionManager();
            var sessionId = sessionService.GetGroupSessionId(_lock.GroupId);
            Assert.AreNotEqual(sessionId, CPSessionManager.NoSessionId);

            // here Java invokes closeSession which directly send a "close session" to the server
            // but does NOT invalidate the session in the session service at all, so after a while
            // the heartbeat thing will kick and invalidate the session and the session ID will
            // change accordingly - we do the same
            await sessionService.RequestCloseSessionAsync((CPGroupId) _lock.GroupId, sessionId);

            // heartbeat delays is about 5s so give it plenty of time here
            await AssertEx.SucceedsEventually(() =>
            {
                var newSessionId = sessionService.GetGroupSessionId(_lock.GroupId);
                Assert.AreEqual(newSessionId, CPSessionManager.NoSessionId);
            }, 20_000, 500);

            await AssertEx.ThrowsAsync<LockOwnershipLostException>(async () => await _lock.LockAsync());
        }

        [Test]
        public async Task TestReentrantTryLockFailsWhenSessionClosed()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var fence = await _lock.LockAndGetFenceAsync();
            await AssertIsLockedAsync(_lock, 1, fence);

            var sessionService = Client.CPSubsystem.GetSessionManager();
            var sessionId = sessionService.GetGroupSessionId(_lock.GroupId);
            Assert.AreNotEqual(sessionId, CPSessionManager.NoSessionId);

            // same as test above
            await sessionService.RequestCloseSessionAsync((CPGroupId)_lock.GroupId, sessionId);

            // heartbeat delays is about 5s so give it plenty of time here
            await AssertEx.SucceedsEventually(() =>
            {
                var newSessionId = sessionService.GetGroupSessionId(_lock.GroupId);
                Assert.AreEqual(newSessionId, CPSessionManager.NoSessionId);
            }, 20_000, 500);

            await AssertEx.ThrowsAsync<LockOwnershipLostException>(async () => await _lock.TryLockAsync());
        }

        [Test]
        public async Task TestReentrantTryLockTimeoutFailsWhenSessionClosed()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var fence = await _lock.LockAndGetFenceAsync();
            await AssertIsLockedAsync(_lock, 1, fence);

            var sessionService = Client.CPSubsystem.GetSessionManager();
            var sessionId = sessionService.GetGroupSessionId(_lock.GroupId);
            Assert.AreNotEqual(sessionId, CPSessionManager.NoSessionId);

            // same as test above
            await sessionService.RequestCloseSessionAsync((CPGroupId)_lock.GroupId, sessionId);

            // heartbeat delays is about 5s so give it plenty of time here
            await AssertEx.SucceedsEventually(() =>
            {
                var newSessionId = sessionService.GetGroupSessionId(_lock.GroupId);
                Assert.AreEqual(newSessionId, CPSessionManager.NoSessionId);
            }, 20_000, 500);

            await AssertEx.ThrowsAsync<LockOwnershipLostException>(async () => await _lock.TryLockAsync(TimeSpan.FromSeconds(1)));
            Assert.False(await _lock.IsLockedAsync());
        }

        [Test]
        public async Task TestUnlockLockFailsWhenSessionClosed()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var fence = await _lock.LockAndGetFenceAsync();
            await AssertIsLockedAsync(_lock, 1, fence);

            var sessionService = Client.CPSubsystem.GetSessionManager();
            var sessionId = sessionService.GetGroupSessionId(_lock.GroupId);
            Assert.AreNotEqual(sessionId, CPSessionManager.NoSessionId);

            // same as test above
            await sessionService.RequestCloseSessionAsync((CPGroupId)_lock.GroupId, sessionId);

            // heartbeat delays is about 5s so give it plenty of time here
            await AssertEx.SucceedsEventually(() =>
            {
                var newSessionId = sessionService.GetGroupSessionId(_lock.GroupId);
                Assert.AreEqual(newSessionId, CPSessionManager.NoSessionId);
            }, 20_000, 500);

            await AssertEx.ThrowsAsync<LockOwnershipLostException>(async () => await _lock.UnlockAsync());
            Assert.False(await _lock.IsLockedByCurrentContextAsync());
            Assert.False(await _lock.IsLockedAsync());
        }

        [Test]
        public async Task TestUnlocLockFailsWhenSessionCreated()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var fence = await _lock.LockAndGetFenceAsync();
            await AssertIsLockedAsync(_lock, 1, fence);

            var sessionService = Client.CPSubsystem.GetSessionManager();
            var sessionId = sessionService.GetGroupSessionId(_lock.GroupId);
            await sessionService.CloseGroupSessionAsync(_lock.GroupId);

            var newSessionId = sessionService.GetGroupSessionId(_lock.GroupId);
            Assert.AreNotEqual(newSessionId, sessionId);

            var otherContext = new OtherContextLock(_lock);
            await otherContext.LockAsync();

            await AssertEx.ThrowsAsync<LockOwnershipLostException>(async () => await _lock.UnlockAsync());
            Assert.False(await _lock.IsLockedByCurrentContextAsync());

        }

        [Test]
        public async Task TestGetFenceFailsWhenSessionCreated()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var fence = await _lock.LockAndGetFenceAsync();
            await AssertIsLockedAsync(_lock, 1, fence);

            var sessionService = Client.CPSubsystem.GetSessionManager();
            var sessionId = sessionService.GetGroupSessionId(_lock.GroupId);
            await sessionService.CloseGroupSessionAsync(_lock.GroupId);

            var newSessionId = sessionService.GetGroupSessionId(_lock.GroupId);
            Assert.AreNotEqual(newSessionId, sessionId);

            var otherContext = new OtherContextLock(_lock);
            await otherContext.LockAsync();

            Assert.ThrowsAsync<LockOwnershipLostException>(async () => await _lock.GetFenceAsync());
            Assert.False(await _lock.IsLockedByCurrentContextAsync());
        }

        [Test]
        public async Task TestFailTryLockNotAcquireSession()
        {
            var lockName = CreateUniqueName() + "@group1";

            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var otherContext = new OtherContextLock(_lock);
            await otherContext.LockAsync();

            var sessionService = Client.CPSubsystem.GetSessionManager();
            var sessionId = sessionService.GetGroupSessionId(_lock.GroupId);

            Assert.That(sessionService.GetAcquiredSessionCount((CPGroupId)_lock.GroupId, sessionId), Is.EqualTo(1));

            var fence = await _lock.TryLockAndGetFenceAsync();

            Assert.AreEqual(FencedLock.InvalidFence, fence);
            Assert.False(await _lock.IsLockedByCurrentContextAsync());

            Assert.That(sessionService.GetAcquiredSessionCount((CPGroupId)_lock.GroupId, sessionId), Is.EqualTo(1));
        }

        [Test]
        public async Task TestDestroy()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            await _lock.LockAsync();
            await _lock.DestroyAsync();

            await AssertEx.ThrowsAsync<RemoteException>(async () => await _lock.LockAsync());
        }

        private class OtherContextLock
        {
            private readonly IFencedLock _fencedLock;
            private readonly TaskCompletionSource<long> _locked = new TaskCompletionSource<long>();
            private readonly TaskCompletionSource<long> _unlock = new TaskCompletionSource<long>();
            private Task _locking;

            public OtherContextLock(IFencedLock fencedLock)
            {
                _fencedLock = fencedLock;
            }

            private async Task LockingAsync(int mode, TimeSpan timeout = default)
            {
                AsyncContext.RequireNew();
                switch (mode)
                {
                    case 1:
                        await _fencedLock.LockAsync().CfAwait();
                        _locked.TrySetResult(-1);
                        break;
                    case 2:
                        _locked.TrySetResult(await _fencedLock.TryLockAndGetFenceAsync().CfAwait());
                        break;
                    case 3:
                        _locked.TrySetResult(await _fencedLock.TryLockAndGetFenceAsync(timeout).CfAwait());
                        break;
                }

                await _unlock.Task.CfAwait();
                await _fencedLock.UnlockAsync().CfAwait();
            }

            public Task LockAsync()
            {
                if (_locking != null) throw new InvalidOperationException();
                _locking = LockingAsync(1);
                return _locked.Task;
            }

            public Task<long> TryLockAndGetFenceAsync()
            {
                if (_locking != null) throw new InvalidOperationException();
                _locking = LockingAsync(2);
                return _locked.Task;
            }

            public Task<long> TryLockAndGetFenceAsync(TimeSpan timeout)
            {
                if (_locking != null) throw new InvalidOperationException();
                _locking = LockingAsync(3, timeout);
                return _locked.Task;
            }

            public Task UnlockAsync()
            {
                _unlock.TrySetResult(0);
                return _locking;
            }
        }
    }
}
