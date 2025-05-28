// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Models;
using Hazelcast.Networking;
using Hazelcast.Testing;
using Hazelcast.Testing.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Hazelcast.Tests.Clustering
{
    [TestFixture]
    public class ConnectMembersTests
    {
        [Test]
        public async Task Test()
        {
            HConsole.Configure(x => x.ConfigureDefaults(this));

            var addresses = new List<NetworkAddress>();
            var mutex = new SemaphoreSlim(1);

            static MemberInfo MemberInfo(NetworkAddress address)
            {
                return new MemberInfo(Guid.NewGuid(), address, new MemberVersion(0, 0, 0), false, new Dictionary<string, string>());
            }

            var loggerFactory = LoggerFactory.Create(builder => builder
                .AddFilter(level => true)
                .AddHConsole());
            var logger = loggerFactory.CreateLogger("ConnectMembers");
            var queue = new MemberConnectionQueue(x => true,x => true, loggerFactory);

            // background task that pretend to connect members
            var dequeuedRequests = 0;
            async Task ConnectMembers(MemberConnectionQueue memberConnectionQueue, CancellationToken cancellationToken)
            {
                await foreach (var request in memberConnectionQueue.WithCancellation(cancellationToken))
                {
                    dequeuedRequests++;
                    await mutex.WaitAsync().CfAwait();
                    logger.LogDebug($"Connect request={dequeuedRequests} member={request.Member.Id.ToShortString()} {request.Member.Address}");
                    addresses.Add(request.Member.Address);
                    request.Complete(true);
                    mutex.Release();
                }
            }

            var cancellation = new CancellationTokenSource();
            var connecting = ConnectMembers(queue, cancellation.Token);

            queue.Resume(); // the queue is initially suspended, resume

            // -- connects
            HConsole.WriteLine(this, "Connects...");

            queue.Add(MemberInfo(NetworkAddress.Parse("127.0.0.1:1")));
            queue.Add(MemberInfo(NetworkAddress.Parse("127.0.0.1:2")));

            await AssertEx.SucceedsEventually(() =>
            {
                Assert.That(addresses.Count, Is.EqualTo(2));
                Assert.That(addresses, Does.Contain(NetworkAddress.Parse("127.0.0.1:1")));
                Assert.That(addresses, Does.Contain(NetworkAddress.Parse("127.0.0.1:2")));
            }, 20_000, 500);


            // -- can suspend while waiting
            HConsole.WriteLine(this, "Can suspend while waiting...");

            await queue.SuspendAsync(); // suspend

            queue.Add(MemberInfo(NetworkAddress.Parse("127.0.0.1:3")));

            await Task.Delay(500);
            Assert.That(addresses.Count, Is.EqualTo(2)); // nothing happened

            queue.Resume(); // resume => processes the queue

            await AssertEx.SucceedsEventually(() =>
            {
                Assert.That(addresses.Count, Is.EqualTo(3));
                Assert.That(addresses, Does.Contain(NetworkAddress.Parse("127.0.0.1:3")));
            }, 20_000, 500);


            // -- suspending waits for current connection
            HConsole.WriteLine(this, "Suspending waits for current connection...");

            await mutex.WaitAsync(); // blocks the connections

            queue.Add(MemberInfo(NetworkAddress.Parse("127.0.0.1:4")));
            await Task.Delay(500);

            var task = queue.SuspendAsync();
            Assert.That(task.IsCompleted, Is.False);

            await Task.Delay(500);
            Assert.That(task.IsCompleted, Is.False);

            await Task.Delay(500);
            Assert.That(task.IsCompleted, Is.False); // still waiting for suspension

            mutex.Release(); // resume => should enable suspend
            await task;

            Assert.That(addresses.Count, Is.EqualTo(4)); // the pending connection happened

            queue.Add(MemberInfo(NetworkAddress.Parse("127.0.0.1:5")));

            await Task.Delay(500);
            Assert.That(addresses.Count, Is.EqualTo(4)); // but a new one waits

            queue.Resume(); // until we resume

            await AssertEx.SucceedsEventually(() =>
            {
                Assert.That(addresses.Count, Is.EqualTo(5));
                Assert.That(addresses, Does.Contain(NetworkAddress.Parse("127.0.0.1:5")));
            }, 20_000, 500);


            // -- can drain when empty
            HConsole.WriteLine(this, "Can drain when empty...");

            await queue.SuspendAsync();
            queue.Resume(true);


            // -- can drain when non-empty
            HConsole.WriteLine(this, "Can drain when non-empty...");

            var dr = dequeuedRequests;

            // acquire the mutex - the first item we enqueue will be picked but processing will hang
            await mutex.WaitAsync();

            queue.Add(MemberInfo(NetworkAddress.Parse("127.0.0.1:6"))); // that one should be picked immediately
            queue.Add(MemberInfo(NetworkAddress.Parse("127.0.0.1:7"))); // that one goes to the queue because the previous one hangs
            queue.Add(MemberInfo(NetworkAddress.Parse("127.0.0.1:8"))); // same
            await Task.Delay(500);

            task = queue.SuspendAsync();  // this can only complete after :6 has been processed to completion
            mutex.Release();              // which requires that we release the mutex
            await task;                   // and then we can be suspended

            Assert.That(dequeuedRequests, Is.EqualTo(dr + 1));
            await Task.Delay(500);
            Assert.That(dequeuedRequests, Is.EqualTo(dr + 1));

            // this should first drain everything from the queue then resume
            queue.Resume(true);
            Assert.That(dequeuedRequests, Is.EqualTo(dr + 1));
            await Task.Delay(500);
            Assert.That(dequeuedRequests, Is.EqualTo(dr + 1));

            Assert.That(addresses.Count, Is.EqualTo(6)); // one of them goes in

            // -- drained

            queue.Add(MemberInfo(NetworkAddress.Parse("127.0.0.1:9")));

            await Task.Delay(500);
            Assert.That(addresses.Count, Is.EqualTo(7));
            Assert.That(addresses, Does.Contain(NetworkAddress.Parse("127.0.0.1:9")));

            // -- the end
            HConsole.WriteLine(this, "End");

            await queue.DisposeAsync();
            await AssertEx.SucceedsEventually(() => Assert.That(connecting.IsCompleted), 20_000, 500);

            // ok, but nothing will happen since the queue has been disposed
            queue.Add(MemberInfo(NetworkAddress.Parse("127.0.0.1:10")));
        }

        [Test]
        public async Task TestDelayedQueue()
        {
            HConsole.Configure(x => x.ConfigureDefaults(this));

            static MemberInfo MemberInfo(NetworkAddress address)
            {
                return new MemberInfo(Guid.NewGuid(), address, new MemberVersion(0, 0, 0), false, new Dictionary<string, string>());
            }

            var loggerFactory = LoggerFactory.Create(builder => builder
                .AddFilter(level => true)
                .AddHConsole());
            var logger = loggerFactory.CreateLogger("ConnectMembers");
            var queue = new MemberConnectionQueue(x => true,x => true, loggerFactory);
            var memberCount = new Dictionary<Guid, int>();

            // background task that pretend to connect members
            var dequeuedRequests = 0;
            const int successCount = 3; // third try will succeed
            async Task ConnectMembers(MemberConnectionQueue memberConnectionQueue, CancellationToken cancellationToken)
            {
                await foreach (var request in memberConnectionQueue.WithCancellation(cancellationToken))
                {
                    dequeuedRequests++;
                    if (!memberCount.TryGetValue(request.Member.Id, out var count)) count = 0;
                    memberCount[request.Member.Id] = ++count;
                    logger.LogDebug($"Connect request={dequeuedRequests} member={request.Member.Id.ToShortString()} count={count} result={(count == successCount ? "success" : "failed")}");
                    request.Complete(count == successCount);
                }
            }

            var cancellation = new CancellationTokenSource();
            var connecting = ConnectMembers(queue, cancellation.Token);

            queue.Resume(); // the queue is initially suspended, resume

            // -- connects

            var stopwatch = Stopwatch.StartNew();
            queue.Add(MemberInfo(NetworkAddress.Parse("127.0.0.1:1")));
            queue.Add(MemberInfo(NetworkAddress.Parse("127.0.0.1:2")));

            await AssertEx.SucceedsEventually(() =>
            {
                Assert.That(dequeuedRequests, Is.EqualTo(2 * successCount));
            }, 30_000, 100);

            var elapsed = stopwatch.Elapsed;

            cancellation.Cancel();
            await connecting.CfAwaitCanceled();

            // each member retried twice = twice the 1s delay = 2s
            // we should not have completed faster than that, even so the code runs fully in-memory
            Assert.That(elapsed, Is.GreaterThanOrEqualTo(TimeSpan.FromSeconds(2)));
            HConsole.WriteLine(this, $"Elapsed: {elapsed}");

            Assert.That(queue.Count, Is.EqualTo(0));
        }
    }
}
