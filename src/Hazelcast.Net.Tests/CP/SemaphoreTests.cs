// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.CP;
using Hazelcast.Protocol;
using Hazelcast.Testing;
using Hazelcast.Tests.CP._;
using NUnit.Framework;

namespace Hazelcast.Tests.CP;

[TestFixture]
[Category("enterprise")]
public class SemaphoreTests : MultiMembersRemoteTestBase
{
    // does not configure semaphore-not-* semaphores
    // does configure semaphore-jdk semaphore as session-less (jdk-compatible)
    protected override string RcClusterConfiguration => TestFiles.ReadAllText(this, "Cluster/cp.xml");

    private readonly List<ICPGroupId> _groups = new();
    private IHazelcastClient _client;

    [OneTimeSetUp]
    public async Task TestOneTimeSetUp()
    {
        // CP-subsystem wants at least 3 members
        for (var i = 0; i < 3; i++) await AddMember().CfAwait();
        _client = await CreateAndStartClientAsync().CfAwait();
    }

    [OneTimeTearDown]
    public async Task TestOneTimeTearDown()
    {
        if (_client == null) return;
        await _client.DisposeAsync();
        _client = null;
    }

    [TearDown]
    public async Task TearDown()
    {
        // tear down the CP session after each test, so each test runs its own session
        try
        {
            foreach (var group in _groups)
                await _client.CPSubsystem.GetSessionManager().CloseGroupSessionAsync(group);
        }
        finally
        {
            _groups.Clear();
        }
    }

    private async Task<ISemaphore> GetSemaphore(bool sessionLess = false)
    {
        var name = "semaphore-" + (sessionLess ? "jdk" : "not") + "-" + CreateUniqueName();
        var semaphore = await _client.CPSubsystem.GetSemaphore(name);
        _groups.Add(semaphore.GroupId); // make sure we close the corresponding session when tearing down
        return semaphore;
    }
    
    public async Task GetWithGroup()
    {
        var groupName = CreateUniqueName();
        var objectName = CreateUniqueName() + "@" + groupName;
        await using var semaphore = await _client.CPSubsystem.GetSemaphore(objectName);

        Assert.That(semaphore.GroupId.Name, Is.EqualTo(groupName));

        await semaphore.DestroyAsync();
    }

    [Test]
    public async Task CanGetSemaphore([Values] bool sessionLess)
    {
        await using var semaphore = await GetSemaphore(sessionLess);

        Assert.That((sessionLess && semaphore is SessionLessSemaphore) ||
                    (!sessionLess && semaphore is SessionAwareSemaphore));
    }

    [Test]
    public async Task CanInitialize([Values] bool sessionLess)
    {
        await using var semaphore = await GetSemaphore(sessionLess);

        // semaphore starts with zero permits
        Assert.That(await semaphore.GetAvailablePermitsAsync(), Is.EqualTo(0));

        // can initialize
        await semaphore.InitializeAsync(12);

        // and the number of permits matches
        Assert.That(await semaphore.GetAvailablePermitsAsync(), Is.EqualTo(12));

        // can initialize again, same count, different count...
        await semaphore.InitializeAsync(12);
        await semaphore.InitializeAsync(4);
        await semaphore.InitializeAsync(3);

        // but the number of permits does not change anymore
        Assert.That(await semaphore.GetAvailablePermitsAsync(), Is.EqualTo(12));

        // can happily re-initialize at any time, has no effect
        await semaphore.AcquireAsync(4);
        Assert.That(await semaphore.GetAvailablePermitsAsync(), Is.EqualTo(8));
        await semaphore.InitializeAsync(4);
        Assert.That(await semaphore.GetAvailablePermitsAsync(), Is.EqualTo(8));
    }

    [Test]
    public async Task CanAcquireAndReleasePermits([Values] bool sessionLess)
    {
        await using var semaphore = await GetSemaphore(sessionLess);

        // semaphore starts with zero permits, initialize with 12
        Assert.That(await semaphore.GetAvailablePermitsAsync(), Is.EqualTo(0));
        await semaphore.InitializeAsync(12);

        // can acquire a permit
        Assert.That(await semaphore.TryAcquireAsync(1, 1_000));

        // can acquire more permits
        Assert.That(await semaphore.TryAcquireAsync(11, 1_000));

        // no more available permits
        Assert.That(await semaphore.GetAvailablePermitsAsync(), Is.EqualTo(0));

        // thus, cannot acquire more permits
        Assert.That(await semaphore.TryAcquireAsync(1, 1_000), Is.False);

        // can release a permit
        await semaphore.ReleaseAsync(1);

        // can release more permits
        await semaphore.ReleaseAsync(2);

        // permits are available again
        Assert.That(await semaphore.GetAvailablePermitsAsync(), Is.EqualTo(3));
    }

    [Test]
    public async Task CanIncreaseAndReducePermits([Values] bool sessionLess)
    {
        await using var semaphore = await GetSemaphore(sessionLess);

        // semaphore starts with zero permits, initialize with 12
        Assert.That(await semaphore.GetAvailablePermitsAsync(), Is.EqualTo(0));
        await semaphore.InitializeAsync(12);
        Assert.That(await semaphore.GetAvailablePermitsAsync(), Is.EqualTo(12));

        /*
         * Reduces the number of available permits by the indicated amount. This
         * method differs from {@code acquire} as it does not block until permits
         * become available. Similarly, if the caller has acquired some permits,
         * they are not released with this call.
         */

        await semaphore.AcquireAsync(8);
        await semaphore.ReducePermitsAsync(8);
        Assert.That(await semaphore.GetAvailablePermitsAsync(), Is.EqualTo(-4)); // ¯\_(ツ)_/¯

        await semaphore.ReleaseAsync(8);
        Assert.That(await semaphore.GetAvailablePermitsAsync(), Is.EqualTo(4));

        if (sessionLess)
        {
            // could release pretty much anything, so don't test
        }
        else
        {
            // cannot release 'reduced' permits (they are not acquired)
            await AssertEx.ThrowsAsync<RemoteException>(async () => await semaphore.ReleaseAsync(8));
        }

        // can add permits
        await semaphore.AcquireAsync(4);
        Assert.That(await semaphore.TryAcquireAsync(4), Is.False);
        await semaphore.IncreasePermitsAsync(4); // total 8 permits
        Assert.That(await semaphore.TryAcquireAsync(4));

        // increasing permits unlocks waiters
        Task<bool> acquiring; // acquire - in a different async flow
        using (AsyncContext.New()) acquiring = semaphore.TryAcquireAsync(1, 8_000);
        await Task.Delay(200);
        Assert.That(acquiring.IsCompleted, Is.False);
        await semaphore.IncreasePermitsAsync(1); // total 9 permits

        // note: it's important to acquire and increase permits in different async flows,
        // as we are not supposed to perform concurrent operations on the semaphore from
        // the same "thread" - and we would get either acquiring returning false on some
        // occasions, or throwing about the acquisition operation being cancelled because
        // of another operation.
        Assert.That(await acquiring);
    }

    [Test]
    public async Task TroubleshootThreads()
    {
        await using var semaphore = await GetSemaphore();
    }

    [Test]
    public async Task CanDrainPermits([Values] bool sessionLess)
    {
        await using var semaphore = await GetSemaphore(sessionLess);

        // semaphore starts with zero permits, initialize with 12
        Assert.That(await semaphore.GetAvailablePermitsAsync(), Is.EqualTo(0));
        await semaphore.InitializeAsync(12);
        Assert.That(await semaphore.GetAvailablePermitsAsync(), Is.EqualTo(12));

        // acquire
        await semaphore.AcquireAsync(2);
        Assert.That(await semaphore.GetAvailablePermitsAsync(), Is.EqualTo(10));

        // drain = acquire all available permits
        Assert.That(await semaphore.DrainPermitsAsync(), Is.EqualTo(10));
        Assert.That(await semaphore.GetAvailablePermitsAsync(), Is.EqualTo(0));

        // release
        await semaphore.ReleaseAsync(12);
    }

    [Test]
    public async Task CannotAcquireNonAvailablePermits([Values] bool sessionLess)
    {
        await using var semaphore = await GetSemaphore(sessionLess);

        // semaphore starts with zero permits
        Assert.That(await semaphore.GetAvailablePermitsAsync(), Is.EqualTo(0));

        // and thus, no permits can be acquired
        Assert.That(await semaphore.TryAcquireAsync(1, 1_000), Is.False);
    }

    [Test]
    public async Task CannotReleaseNonAcquiredPermits([Values] bool sessionLess)
    {
        await using var semaphore = await GetSemaphore(sessionLess);

        // semaphore starts with zero permits, initialize with 12
        Assert.That(await semaphore.GetAvailablePermitsAsync(), Is.EqualTo(0));
        await semaphore.InitializeAsync(12);

        if (sessionLess)
        {
            // in jdk-compatible more, can happily release non-acquired permits
            await semaphore.ReleaseAsync();
            await semaphore.ReleaseAsync(3);
            await semaphore.AcquireAsync(4);
            await semaphore.ReleaseAsync(37);
            Assert.That(await semaphore.GetAvailablePermitsAsync(), Is.EqualTo(12 + 1 + 3 - 4 + 37));
        }
        else
        {
            // cannot release permits
            await AssertEx.ThrowsAsync<Exception>(async () => await semaphore.ReleaseAsync());

            // can release acquired permits, but not more
            await semaphore.AcquireAsync(4);
            await semaphore.ReleaseAsync(3);
            await AssertEx.ThrowsAsync<Exception>(async () => await semaphore.ReleaseAsync(3));
        }
    }

    [Test]
    public async Task CanAcquireAtParallel([Values] bool sessionLess)
    {
        await using var semaphore = await GetSemaphore(sessionLess);

        Assert.That(await semaphore.GetAvailablePermitsAsync(), Is.EqualTo(0));
        await semaphore.InitializeAsync(2);
        Assert.That(await semaphore.GetAvailablePermitsAsync(), Is.EqualTo(2));

        var workerCount = 5;
        var workDone = 0;
        var smName = semaphore.Name;

        async Task DoSomeWork()
        {
            // Each run will bw in different thread. So, new context is a must.
            AsyncContext.New();

            var sm = await _client.CPSubsystem.GetSemaphore(smName);

            var acquired = await sm.TryAcquireAsync(1, 5_00);

            if (acquired)
            {
                Interlocked.Increment(ref workDone);
                // Do some work
                await Task.Delay(200);
                await sm.ReleaseAsync();
            }
        }

        var tasks = new List<Task>();

        for (int i = 0; i < workerCount; i++)
        {
            tasks.Add(Task.Run(async () => await DoSomeWork()));
        }

        await Task.WhenAll(tasks);
        Assert.That(workDone, Is.EqualTo(workerCount));
    }
    
    [Test]
    public async Task CanAcquireAtParallelOnSameSemaphoreObject([Values] bool sessionLess)
    {
        await using var semaphore = await GetSemaphore(sessionLess);

        Assert.That(await semaphore.GetAvailablePermitsAsync(), Is.EqualTo(0));
        await semaphore.InitializeAsync(2);
        Assert.That(await semaphore.GetAvailablePermitsAsync(), Is.EqualTo(2));

        var workerCount = 5;
        var workDone = 0;

        async Task DoSomeWork()
        {
            // Each run will bw in different thread. So, new context is a must.
            AsyncContext.New();

            var acquired = await semaphore.TryAcquireAsync(1, 5_00);

            if (acquired)
            {
                Interlocked.Increment(ref workDone);
                // Do some work
                await Task.Delay(200);
                await semaphore.ReleaseAsync();
            }
        }

        var tasks = new List<Task>();

        for (int i = 0; i < workerCount; i++)
        {
            tasks.Add(Task.Run(async () => await DoSomeWork()));
        }

        await Task.WhenAll(tasks);
        Assert.That(workDone, Is.EqualTo(workerCount));
    }
}
