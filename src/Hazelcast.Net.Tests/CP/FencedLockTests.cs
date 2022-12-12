// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.CP;
using Hazelcast.Exceptions;
using Hazelcast.Protocol;
using Hazelcast.Protocol.Models;
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
            if (_lock != null)
                await Client.CPSubsystem.GetSessionManager().CloseGroupSessionAsync(_lock.GroupId);
        }

        [Test]
        public void TestLockContextResetSequence()
        {
            var c1 = new LockContext();
            Assert.Greater(c1.Id, 0);
            LockContext.ResetSequence();
            var c2 = new LockContext();
            Assert.LessOrEqual(c2.Id, c1.Id);
        }

        [Test]
        public async Task TestReentrantLock()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var lockContext = new LockContext();
            await _lock.LockAsync(lockContext);
            await _lock.LockAsync(lockContext);
            await _lock.UnlockAsync(lockContext);
            await _lock.UnlockAsync(lockContext);
        }

        [Test]
        public async Task TestReentrantTryLock()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var lockContext = new LockContext();
            Assert.IsTrue(await _lock.TryLockAsync(lockContext));
            Assert.IsTrue(await _lock.TryLockAsync(lockContext));
            await _lock.UnlockAsync(lockContext);
            await _lock.UnlockAsync(lockContext);
        }

        [Test]
        public async Task TestReentrantTryLockTimeout()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var lockContext = new LockContext();
            Assert.True(await _lock.TryLockAsync(lockContext, TimeSpan.FromSeconds(1)));
            Assert.True(await _lock.TryLockAsync(lockContext, TimeSpan.FromSeconds(1)));
            await _lock.UnlockAsync(lockContext);
            await _lock.UnlockAsync(lockContext);
        }

        [Test]
        public async Task TestReentrantLockAndGetFence()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var lockContext = new LockContext();
            var fence1 = await _lock.LockAndGetFenceAsync(lockContext);
            var fence2 = await _lock.LockAndGetFenceAsync(lockContext);

            Assert.AreNotEqual(FencedLock.InvalidFence, fence1);
            Assert.AreNotEqual(FencedLock.InvalidFence, fence2);
            Assert.AreEqual(fence1, fence2);

            var fence3 = await _lock.GetFenceAsync(lockContext);
            Assert.AreEqual(fence1, fence3);

            await AssertIsLockedAsync(_lock, lockContext, 2, fence3);

            await _lock.UnlockAsync(lockContext);
            await _lock.UnlockAsync(lockContext);
        }

        [Test]
        public async Task TestReentrantTryLockAndGetFence()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var lockContext = new LockContext();
            var fence1 = await _lock.TryLockAndGetFenceAsync(lockContext);
            var fence2 = await _lock.TryLockAndGetFenceAsync(lockContext);

            Assert.AreNotEqual(FencedLock.InvalidFence, fence1);
            Assert.AreNotEqual(FencedLock.InvalidFence, fence2);
            Assert.AreEqual(fence1, fence2);

            var fence3 = await _lock.GetFenceAsync(lockContext);
            Assert.AreEqual(fence1, fence3);

            await _lock.UnlockAsync(lockContext);
            await _lock.UnlockAsync(lockContext);
        }

        [Test]
        public async Task TestReentrantTryLockAndGetFenceTimeout()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var lockContext = new LockContext();
            var fence1 = await _lock.TryLockAndGetFenceAsync(lockContext, TimeSpan.FromSeconds(1));
            var fence2 = await _lock.TryLockAndGetFenceAsync(lockContext, TimeSpan.FromSeconds(1));

            Assert.AreNotEqual(FencedLock.InvalidFence, fence1);
            Assert.AreNotEqual(FencedLock.InvalidFence, fence2);
            Assert.AreEqual(fence1, fence2);

            var fence3 = await _lock.GetFenceAsync(lockContext);
            Assert.AreEqual(fence1, fence3);

            await _lock.UnlockAsync(lockContext);
            await _lock.UnlockAsync(lockContext);
        }

        [Test]
        public async Task TestTryLockWhileLockedByAnotherContext()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var otherContext = new LockContext();
            await _lock.LockAsync(otherContext);

            var lockContext = new LockContext();

            Assert.False(await _lock.TryLockAsync(lockContext)); // cannot take the lock
        }

        [Test]
        public async Task TestTryLockTimeoutWhileLockedByAnotherContext()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var otherContext = new LockContext();
            await _lock.LockAsync(otherContext);

            var lockContext = new LockContext();

            Assert.False(await _lock.TryLockAsync(lockContext, TimeSpan.FromSeconds(1))); // cannot take the lock
        }

        [Test]
        public async Task TestReentrantLockFails()
        {
            const string lockName = "non-reentrant-lock@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var lockContext = new LockContext();

            await _lock.LockAsync(lockContext);
            await AssertEx.ThrowsAsync<LockAcquireLimitReachedException>(async () => await _lock.LockAsync(lockContext));

            await _lock.UnlockAsync(lockContext);
        }

        [Test]
        public async Task TestReentrantTryLockFails()
        {
            const string lockName = "non-reentrant-lock@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var lockContext = new LockContext();
            await _lock.LockAsync(lockContext);

            Assert.False(await _lock.TryLockAsync(lockContext));
            Assert.True(await _lock.IsLockedByContextAsync(lockContext));
            Assert.That(await _lock.GetLockCountAsync(lockContext), Is.EqualTo(1));
            Assert.AreNotEqual(FencedLock.InvalidFence, await _lock.GetFenceAsync(lockContext));

            await _lock.UnlockAsync(lockContext);
        }

        [Test]
        public async Task TestReentrantTryLockAndGetFenceFails()
        {
            const string lockName = "non-reentrant-lock@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var lockContext = new LockContext();
            await _lock.LockAsync(lockContext);

            var fence1 = await _lock.GetFenceAsync(lockContext);
            var fence2 = await _lock.TryLockAndGetFenceAsync(lockContext);

            Assert.AreNotEqual(FencedLock.InvalidFence, fence1);
            Assert.AreEqual(FencedLock.InvalidFence, fence2);
            Assert.True(await _lock.IsLockedByContextAsync(lockContext));
            Assert.That(await _lock.GetLockCountAsync(lockContext), Is.EqualTo(1));
            Assert.AreEqual(fence1, await _lock.GetFenceAsync(lockContext));

            await _lock.UnlockAsync(lockContext);
        }

        [Test]
        public async Task TestReentrantTryLockAndGetFenceTimeoutFails()
        {
            const string lockName = "non-reentrant-lock@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var lockContext = new LockContext();
            await _lock.LockAsync(lockContext);

            var fence1 = await _lock.GetFenceAsync(lockContext);
            var fence2 = await _lock.TryLockAndGetFenceAsync(lockContext, TimeSpan.FromSeconds(1));

            Assert.AreNotEqual(FencedLock.InvalidFence, fence1);
            Assert.AreEqual(FencedLock.InvalidFence, fence2);
            Assert.True(await _lock.IsLockedByContextAsync(lockContext));
            Assert.That(await _lock.GetLockCountAsync(lockContext), Is.EqualTo(1));
            Assert.AreEqual(fence1, await _lock.GetFenceAsync(lockContext));

            await _lock.UnlockAsync(lockContext);
        }

        [Test]
        public async Task TestReentrantLockAfterLockIsReleasedByAnotherContext()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            // same as Java test except that *other* locks and releases and *we*
            // test the re-entrance whereas Java does it the other way round, but
            // the result is the same - testing the very same thing

            var otherContext = new LockContext();
            await _lock.LockAsync(otherContext);

            var lockContext = new LockContext();

            async Task Reenter()
            {
                await _lock.LockAsync(lockContext);
                await _lock.LockAsync(lockContext);
                await _lock.UnlockAsync(lockContext);
                await _lock.UnlockAsync(lockContext);
            }

            var reentering = Reenter();
            await _lock.UnlockAsync(otherContext);

            var task = await Task.WhenAny(reentering, Task.Delay(TimeSpan.FromSeconds(10)));
            Assert.That(task, Is.EqualTo(reentering));
        }

        [Test]
        [Timeout(180_000)] // this test takes time (see below), override default timeout
        public async Task TestAutoReleaseOnClientDispose()
        {
            var lockName = CreateUniqueName() + "@group1";

            // everything below happens on the very same async context, but the full cluster-wide
            // lock owner is (client, thread) in Java or (client, context) in .NET, so for all
            // it matters the two client are different owners. as per docs: "if you lock using a
            // FencedLock, the critical section that it guards is guaranteed to be executed by
            // only one thread in the entire cluster" - the thread (context) that locked it
            var lockContext = new LockContext();
            
            var client1 = await CreateAndStartClientAsync().CfAwait();
            var lock1 = await client1.CPSubsystem.GetLockAsync(lockName);

            var client2 = await CreateAndStartClientAsync().CfAwait();
            var lock2 = await client2.CPSubsystem.GetLockAsync(lockName);

            Assert.That(lock1, Is.Not.SameAs(lock2)); // two different objects

            var group1 = (CPGroupId)lock1.GroupId;
            Console.WriteLine($"Group1: {group1.Id} {group1.Name}");
            var group2 = (CPGroupId)lock2.GroupId;
            Console.WriteLine($"Group2: {group2.Id} {group2.Name}");
            Assert.That(group1.Id, Is.EqualTo(group2.Id)); // same groups

            var session1 = client1.CPSubsystem.GetSessionManager().GetSessionId(group1);
            Console.WriteLine($"Session1: {session1}");
            var session2 = client2.CPSubsystem.GetSessionManager().GetSessionId(group2);
            Console.WriteLine($"Session2: {session2}");
            Assert.That(session1, Is.EqualTo(-1)); // no session yet
            Assert.That(session1, Is.EqualTo(session2)); // no session yet

            await lock1.LockAsync(lockContext);
            Assert.That(await lock2.TryLockAsync(lockContext), Is.False); // same context, other client = cannot lock

            session1 = client1.CPSubsystem.GetSessionManager().GetSessionId(group1);
            Console.WriteLine($"Session1: {session1}");
            session2 = client2.CPSubsystem.GetSessionManager().GetSessionId(group2);
            Console.WriteLine($"Session2: {session2}");
            Assert.That(session1, Is.Not.EqualTo(-1)); // now we have a session
            Assert.That(session2, Is.Not.EqualTo(-1)); // now we have a session
            Assert.That(session1, Is.Not.EqualTo(session2)); // two different sessions

            // disposing the client disconnects it from the cluster - and then what becomes of the
            // locks it owns depend on the session - as long as the session stays, the locks stay
            // too - the default session TTL is 300s, but for the sake of our tests it is forced
            // to 90s in the cp.xml configuration file - so after 90s the session will expire
            // and the lock will be released

            await client1.DisposeAsync();

            // eventually, client1 session expires and client2 is free to lock
            await AssertEx.SucceedsEventually(async () =>
            {
                Assert.That(await lock2.TryLockAsync(lockContext), Is.True);
            }, 120_000, 10_000); // session TTL is 90s

            await client2.DisposeAsync();
        }

        [Test]
        public async Task TestLock()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var lockContext = new LockContext();
            await _lock.LockAsync(lockContext);
            await AssertIsLockedAsync(_lock, lockContext);
            await _lock.UnlockAsync(lockContext);
        }

        private static async Task AssertIsLockedAsync(IFencedLock fencedLock, LockContext lockContext, int count = 1, long? fence = default)
        {
            fence ??= await fencedLock.GetFenceAsync(lockContext);
            Assert.AreNotEqual(FencedLock.InvalidFence, fence);

            Assert.True(await fencedLock.IsLockedAsync(lockContext));
            Assert.That(await fencedLock.GetLockCountAsync(lockContext), Is.EqualTo(count));
            Assert.True(await fencedLock.IsLockedByContextAsync(lockContext));
        }

        [Test]
        public async Task TestLockAndGetFence()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);
            var lockContext = new LockContext();
            var fence = await _lock.LockAndGetFenceAsync(lockContext);
            Assert.AreNotEqual(FencedLock.InvalidFence, fence);
            await _lock.UnlockAsync(lockContext);
        }

        [Test]
        public async Task TestTryLockAndGetFence()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);
            var lockContext = new LockContext();
            var fence = await _lock.TryLockAndGetFenceAsync(lockContext);
            Assert.AreNotEqual(FencedLock.InvalidFence, fence);
            await _lock.UnlockAsync(lockContext);
        }

        [Test]
        public async Task TestTryLockAndGetFenceTimeout()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);
            var lockContext = new LockContext();
            var fence = await _lock.TryLockAndGetFenceAsync(lockContext, TimeSpan.FromSeconds(1));
            Assert.AreNotEqual(FencedLock.InvalidFence, fence);
            await _lock.UnlockAsync(lockContext);
        }

        [Test]
        public async Task TestTryLock()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);
            var lockContext = new LockContext();
            var locked = await _lock.TryLockAsync(lockContext);
            Assert.True(locked);
            await AssertIsLockedAsync(_lock, lockContext);
            await _lock.UnlockAsync(lockContext);
        }

        [Test]
        public async Task TestTryLockTimeout()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);
            var lockContext = new LockContext();
            var locked = await _lock.TryLockAsync(lockContext, TimeSpan.FromSeconds(1));
            Assert.True(locked);
            await AssertIsLockedAsync(_lock, lockContext);
            await _lock.UnlockAsync(lockContext);
        }

        [Test]
        public async Task TestLockWhenLockedByAnotherContext()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var lockContext = new LockContext();
            await _lock.LockAsync(lockContext);

            var otherContext = new LockContext();
            var otherContextLocking = _lock.LockAsync(otherContext);

            await Task.Delay(TimeSpan.FromSeconds(5));
            Assert.That(otherContextLocking.IsCompleted, Is.False);
            await AssertIsLockedAsync(_lock, lockContext);

            Assert.That(otherContextLocking.IsCompleted, Is.False);
            await _lock.UnlockAsync(lockContext);
            await otherContextLocking;
        }

        [Test]
        public async Task TestUnlockWhenFree1()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var lockContext = new LockContext();

            // we get a SynchronizationLockException because we have no session
            await AssertEx.ThrowsAsync<SynchronizationLockException>(async () => await _lock.UnlockAsync(lockContext));
        }

        [Test]
        public async Task TestUnlockWhenFree2()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var lockContext = new LockContext();

            // this will make sure that we do have a session for the lock's group
            await _lock.LockAsync(lockContext);
            await _lock.UnlockAsync(lockContext);

            // we get a RemoteError with Error IllegalMonitorState because we have a session - but are not locked
            var e = await AssertEx.ThrowsAsync<RemoteException>(async () => await _lock.UnlockAsync(lockContext));
            Assert.That(e.Error, Is.EqualTo(RemoteError.IllegalMonitorState));
        }


        [Test]
        public async Task TestGetFenceWhenFree()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var lockContext = new LockContext();
            await AssertEx.ThrowsAsync<SynchronizationLockException>(async () => await _lock.GetFenceAsync(lockContext));
        }

        [Test]
        public async Task TestLockedFalseWhenFree()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var lockContext = new LockContext();
            Assert.False(await _lock.IsLockedAsync(lockContext));
        }

        [Test]
        public async Task TestIsLockedByContextFalseWhenFree()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var lockContext = new LockContext();
            Assert.False(await _lock.IsLockedByContextAsync(lockContext));
        }

        [Test]
        public async Task TestLockCountZeroWhenFree()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var lockContext = new LockContext();
            await _lock.LockAsync(lockContext);
            await _lock.UnlockAsync(lockContext);

            Assert.False(await _lock.IsLockedAsync(lockContext));
            Assert.That(await _lock.GetLockCountAsync(lockContext), Is.EqualTo(0));

            await AssertEx.ThrowsAsync<SynchronizationLockException>(async () => await _lock.GetFenceAsync(lockContext));
        }

        [Test]
        public async Task TestLockUnlockThenLockOnOtherContext()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var lockContext = new LockContext();
            var fence = await _lock.LockAndGetFenceAsync(lockContext);
            await AssertIsLockedAsync(_lock, lockContext, 1, fence);
            await _lock.UnlockAsync(lockContext);

            var otherContext = new LockContext();
            var otherFence = await _lock.TryLockAndGetFenceAsync(otherContext);

            Assert.Greater(otherFence, fence);
            await AssertEx.ThrowsAsync<SynchronizationLockException>(async () => await _lock.GetFenceAsync(lockContext));
        }

        [Test]
        public async Task TestUnlockWhenPendingLockOnOtherContext()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var lockContext = new LockContext();
            var fence = await _lock.LockAndGetFenceAsync(lockContext);

            var otherContext = new LockContext();
            var otherFenceTask = _lock.TryLockAndGetFenceAsync(otherContext, TimeSpan.FromSeconds(60));

            await _lock.UnlockAsync(lockContext);

            var otherFence = await otherFenceTask;

            Assert.Greater(otherFence, fence);

            Assert.True(await _lock.IsLockedAsync(lockContext));
            Assert.False(await _lock.IsLockedByContextAsync(lockContext));
            Assert.That(await _lock.GetLockCountAsync(lockContext), Is.EqualTo(1));

            await AssertEx.ThrowsAsync<SynchronizationLockException>(async () => await _lock.GetFenceAsync(lockContext));

            await _lock.UnlockAsync(otherContext);
        }

        [Test]
        public async Task TestUnlockWhenLockedOnOtherContext()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var otherContext = new LockContext();
            await _lock.LockAsync(otherContext);

            var lockContext = new LockContext();
            Assert.ThrowsAsync<RemoteException>(async () => await _lock.UnlockAsync(lockContext));
            Assert.True(await _lock.IsLockedAsync(lockContext));
            Assert.False(await _lock.IsLockedByContextAsync(lockContext));
            Assert.That(await _lock.GetLockCountAsync(lockContext), Is.EqualTo(1));
        }

        [Test]
        public async Task TestTryLockTimeoutWhenLockedOnOtherContext()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var otherContext = new LockContext();
            await _lock.LockAsync(otherContext);

            var lockContext = new LockContext();
            var fence = await _lock.TryLockAndGetFenceAsync(lockContext, TimeSpan.FromMilliseconds(100));

            Assert.AreEqual(FencedLock.InvalidFence, fence);
            Assert.True(await _lock.IsLockedAsync(lockContext));
            Assert.False(await _lock.IsLockedByContextAsync(lockContext));
            Assert.That(await _lock.GetLockCountAsync(lockContext), Is.EqualTo(1));
        }

        [Test]
        public async Task TestTryLockLongTimeoutWhenLockedOnOtherContext()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var otherContext = new LockContext();
            await _lock.LockAsync(otherContext);

            // "1500ms is upper bound of lock service"
            // this is a direct copy of a Java test which comes without comments,
            // and I have no idea what is the purpose of the test (note that 1500ms
            // is not really a "max timeout" and a timeout of 20s is accepted as
            // well...)
            var lockContext = new LockContext();
            var fence = await _lock.TryLockAndGetFenceAsync(lockContext, TimeSpan.FromMilliseconds(1500 + 1));

            Assert.AreEqual(FencedLock.InvalidFence, fence);
            Assert.True(await _lock.IsLockedAsync(lockContext));
            Assert.False(await _lock.IsLockedByContextAsync(lockContext));
            Assert.That(await _lock.GetLockCountAsync(lockContext), Is.EqualTo(1));
        }

        [Test]
        public async Task TestReentrantLockFailsWhenSessionClosed()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var lockContext = new LockContext();
            var fence = await _lock.LockAndGetFenceAsync(lockContext);
            await AssertIsLockedAsync(_lock, lockContext, 1, fence);

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

            await AssertEx.ThrowsAsync<LockOwnershipLostException>(async () => await _lock.LockAsync(lockContext));
        }

        [Test]
        public async Task TestReentrantTryLockFailsWhenSessionClosed()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var lockContext = new LockContext();
            var fence = await _lock.LockAndGetFenceAsync(lockContext);
            await AssertIsLockedAsync(_lock, lockContext, 1, fence);

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

            await AssertEx.ThrowsAsync<LockOwnershipLostException>(async () => await _lock.TryLockAsync(lockContext));
        }

        [Test]
        public async Task TestReentrantTryLockTimeoutFailsWhenSessionClosed()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var lockContext = new LockContext();
            var fence = await _lock.LockAndGetFenceAsync(lockContext);
            await AssertIsLockedAsync(_lock, lockContext, 1, fence);

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

            await AssertEx.ThrowsAsync<LockOwnershipLostException>(async () => await _lock.TryLockAsync(lockContext, TimeSpan.FromSeconds(1)));
            Assert.False(await _lock.IsLockedAsync(lockContext));
        }

        [Test]
        public async Task TestUnlockLockFailsWhenSessionClosed()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var lockContext = new LockContext();
            var fence = await _lock.LockAndGetFenceAsync(lockContext);
            await AssertIsLockedAsync(_lock, lockContext, 1, fence);

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

            await AssertEx.ThrowsAsync<LockOwnershipLostException>(async () => await _lock.UnlockAsync(lockContext));
            Assert.False(await _lock.IsLockedByContextAsync(lockContext));
            Assert.False(await _lock.IsLockedAsync(lockContext));
        }

        [Test]
        public async Task TestUnlocLockFailsWhenSessionCreated()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var lockContext = new LockContext();
            var fence = await _lock.LockAndGetFenceAsync(lockContext);
            await AssertIsLockedAsync(_lock, lockContext, 1, fence);

            var sessionService = Client.CPSubsystem.GetSessionManager();
            var sessionId = sessionService.GetGroupSessionId(_lock.GroupId);
            await sessionService.CloseGroupSessionAsync(_lock.GroupId);

            var newSessionId = sessionService.GetGroupSessionId(_lock.GroupId);
            Assert.AreNotEqual(newSessionId, sessionId);

            var otherContext = new LockContext();
            await _lock.LockAsync(otherContext);

            await AssertEx.ThrowsAsync<LockOwnershipLostException>(async () => await _lock.UnlockAsync(lockContext));
            Assert.False(await _lock.IsLockedByContextAsync(lockContext));

        }

        [Test]
        public async Task TestGetFenceFailsWhenSessionCreated()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var lockContext = new LockContext();
            var fence = await _lock.LockAndGetFenceAsync(lockContext);
            await AssertIsLockedAsync(_lock, lockContext, 1, fence);

            var sessionService = Client.CPSubsystem.GetSessionManager();
            var sessionId = sessionService.GetGroupSessionId(_lock.GroupId);
            await sessionService.CloseGroupSessionAsync(_lock.GroupId);

            var newSessionId = sessionService.GetGroupSessionId(_lock.GroupId);
            Assert.AreNotEqual(newSessionId, sessionId);

            var otherContext = new LockContext();
            await _lock.LockAsync(otherContext);

            Assert.ThrowsAsync<LockOwnershipLostException>(async () => await _lock.GetFenceAsync(lockContext));
            Assert.False(await _lock.IsLockedByContextAsync(lockContext));
        }

        [Test]
        public async Task TestFailTryLockNotAcquireSession()
        {
            var lockName = CreateUniqueName() + "@group1";

            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var otherContext = new LockContext();
            await _lock.LockAsync(otherContext);

            var sessionService = Client.CPSubsystem.GetSessionManager();
            var sessionId = sessionService.GetGroupSessionId(_lock.GroupId);

            Assert.That(sessionService.GetAcquiredSessionCount((CPGroupId)_lock.GroupId, sessionId), Is.EqualTo(1));

            var lockContext = new LockContext();
            var fence = await _lock.TryLockAndGetFenceAsync(lockContext);

            Assert.AreEqual(FencedLock.InvalidFence, fence);
            Assert.False(await _lock.IsLockedByContextAsync(lockContext));

            Assert.That(sessionService.GetAcquiredSessionCount((CPGroupId)_lock.GroupId, sessionId), Is.EqualTo(1));
        }

        [Test]
        public async Task TestDestroy()
        {
            var lockName = CreateUniqueName() + "@group1";
            _lock = await Client.CPSubsystem.GetLockAsync(lockName);

            var lockContext = new LockContext();
            await _lock.LockAsync(lockContext);
            await _lock.DestroyAsync();

            await AssertEx.ThrowsAsync<RemoteException>(async () => await _lock.LockAsync(lockContext));
        }
    }
}
