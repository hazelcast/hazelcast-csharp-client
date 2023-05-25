// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Models;
using Hazelcast.Networking;
using Hazelcast.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Hazelcast.Tests.Clustering;

[TestFixture]
[Timeout(4_000)]
public class MemberConnectionQueueTests
{
    private static readonly NetworkAddress MemberAddress = NetworkAddress.Parse("127.0.0.1:5701");
    private static readonly MemberVersion MemberVersion = new(5, 0, 0);
    private static readonly Dictionary<string, string> MemberAttributes = new();

    MemberInfo NewMemberInfo(Guid id) => new(id, MemberAddress, MemberVersion, false, MemberAttributes);

    [Test]
    public async Task Works()
    {
        await using var queue = new MemberConnectionQueue(id => true, NullLoggerFactory.Instance);
        queue.Resume(); // queue is initially suspended

        var id1 = Guid.NewGuid();
        queue.Add(NewMemberInfo(id1));

        await Task.Delay(100);

        var id2 = Guid.NewGuid();
        queue.Add(NewMemberInfo(id2));

        // can enumerate the first request
        var enumerator = queue.GetAsyncEnumerator();
        Assert.That(await enumerator.MoveNextAsync());
        Assert.That(enumerator.Current.Member.Id, Is.EqualTo(id1));

        // cannot enumerate any further until the request is completed
        await AssertEx.ThrowsAsync<InvalidOperationException>(async () => await enumerator.MoveNextAsync());
        
        // once the first request is completed, can enumerate the next request
        enumerator.Current.Complete(true);
        Assert.That(await enumerator.MoveNextAsync());
        Assert.That(enumerator.Current.Member.Id, Is.EqualTo(id2));

        // cannot enumerate any further until the request is completed
        await AssertEx.ThrowsAsync<InvalidOperationException>(async () => await enumerator.MoveNextAsync());

        // complete the request
        enumerator.Current.Complete(true);

        // the queue is now empty
        Assert.That(queue.Count, Is.Zero);
    }

    [Test]
    public async Task FailedRequestsAreQueuedAgain()
    {
        await using var queue = new MemberConnectionQueue(id => true, NullLoggerFactory.Instance);
        queue.Resume(); // queue is initially suspended

        var id1 = Guid.NewGuid();
        queue.Add(NewMemberInfo(id1));

        // can enumerate the first request
        var enumerator = queue.GetAsyncEnumerator();
        Assert.That(await enumerator.MoveNextAsync());
        Assert.That(enumerator.Current.Member.Id, Is.EqualTo(id1));

        // cannot enumerate any further until the request is completed
        await AssertEx.ThrowsAsync<InvalidOperationException>(async () => await enumerator.MoveNextAsync());

        // ...
        var time1 = Clock.Milliseconds;
        enumerator.Current.Complete(false);
        var time2 = Clock.Milliseconds;
        Assert.That(await enumerator.MoveNextAsync());
        var time3 = Clock.Milliseconds;
        Assert.That(enumerator.Current.Member.Id, Is.EqualTo(id1));
        Assert.That(time2, Is.LessThan(enumerator.Current.Time));
        Assert.That(time3, Is.GreaterThan(enumerator.Current.Time - MemberConnectionQueue.TimeMargin));
    }

    [Test]
    public async Task SuspendWhileFree()
    {
        await using var queue = new MemberConnectionQueue(id => true, NullLoggerFactory.Instance);
        queue.Resume(); // queue is initially suspended

        var id1 = Guid.NewGuid();
        queue.Add(NewMemberInfo(id1));

        var enumerator = queue.GetAsyncEnumerator();

        await queue.SuspendAsync();

        var movingNext = enumerator.MoveNextAsync();
        await Task.Delay(500);
        Assert.That(movingNext.IsCompleted, Is.False);

        queue.Resume();
        await AssertEx.SucceedsEventually(() =>
        {
            Assert.That(movingNext.IsCompleted, Is.True);
        }, 2_000, 200);
    }

    [Test]
    public async Task SuspendWhileBusy([Values] bool success)
    {
        await using var queue = new MemberConnectionQueue(id => true, NullLoggerFactory.Instance);
        queue.Resume(); // queue is initially suspended

        var id1 = Guid.NewGuid();
        queue.Add(NewMemberInfo(id1));

        var id2 = Guid.NewGuid();
        queue.Add(NewMemberInfo(id2));

        var enumerator = queue.GetAsyncEnumerator();
        Assert.That(await enumerator.MoveNextAsync());

        var suspending = queue.SuspendAsync();
        await Task.Delay(500);
        Assert.That(suspending.IsCompleted, Is.False);

        enumerator.Current.Complete(success);

        await AssertEx.SucceedsEventually(() =>
        {
            Assert.That(suspending.IsCompleted, Is.True);
        }, 2_000, 200);
    }

    [Test]
    public async Task EnforceOneEnumerator()
    {
        await using var queue = new MemberConnectionQueue(id => true, NullLoggerFactory.Instance);
        queue.Resume(); // queue is initially suspended

        // can get an enumerator
        var enumerator = queue.GetAsyncEnumerator();

        // cannot get another enumerator
        Assert.Throws<InvalidOperationException>(() => queue.GetAsyncEnumerator());

        // can release the enumerator and get another one
        await enumerator.DisposeAsync();
        enumerator = queue.GetAsyncEnumerator();
    }
}