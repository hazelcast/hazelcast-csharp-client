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
using System.Threading.Tasks;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.CP;

[TestFixture]
public class CountDownLatchTests : MultiMembersRemoteTestBase
{
    protected override string RcClusterConfiguration => TestFiles.ReadAllText(this, "Cluster/cp.xml");

    protected IHazelcastClient Client;
        
    [SetUp]
    public async Task SetUp()
    {
        // CP-subsystem wants at least 3 members
        for (var i = 0; i < 3; i++) await AddMember();
            
        Client = await CreateAndStartClientAsync();
    }
    
    [Test]
    public async Task GetWithGroup()
    {
        var groupName = CreateUniqueName();
        var objectName = CreateUniqueName() + "@"+ groupName;
        await using var countDownLatch = await Client.CPSubsystem.GetCountDownLatchAsync(objectName);

        Assert.That(countDownLatch.GroupId.Name, Is.EqualTo(groupName));

        await countDownLatch.DestroyAsync();
    }
    
    
    [Test]
    public async Task NormalTest()
    {
        var cdl = await Client.CPSubsystem.GetCountDownLatchAsync(CreateUniqueName());
        Assert.That(await cdl.TrySetCountAsync(4));
        Assert.That(await cdl.TrySetCountAsync(4), Is.False);
        var waiting = cdl.AwaitAsync(TimeSpan.FromSeconds(30));
        for (var i = 4; i > 0; i--)
        {
            Assert.That(waiting.IsCompleted, Is.False);
            Assert.That(await cdl.GetCountAsync(), Is.EqualTo(i));
            await cdl.CountDownAsync();
        }

        await AssertEx.SucceedsEventually(() =>
        {
            Assert.That(waiting.IsCompleted);
        }, 4_000, 500);

        Assert.That(await cdl.GetCountAsync(), Is.EqualTo(0));
        await cdl.CountDownAsync();
        Assert.That(await cdl.GetCountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task TimeoutTest()
    {
        var cdl = await Client.CPSubsystem.GetCountDownLatchAsync(CreateUniqueName());
        Assert.That(await cdl.TrySetCountAsync(4));
        var waiting = cdl.AwaitAsync(TimeSpan.FromSeconds(1));

        await AssertEx.SucceedsEventually(() =>
        {
            Assert.That(waiting.IsCompleted && waiting.Result == false);
        }, 4_000, 500);
    }

    [Test]
    public async Task DestroyTest()
    {
        var cdl = await Client.CPSubsystem.GetCountDownLatchAsync(CreateUniqueName());
        await cdl.DestroyAsync();
    }
}