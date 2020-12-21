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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Models;
using Hazelcast.Networking;
using Hazelcast.Testing;
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
            var addresses = new List<NetworkAddress>();
            var mutex = new SemaphoreSlim(1);

            static MemberInfo MemberInfo(NetworkAddress address)
            {
                return new MemberInfo(Guid.NewGuid(), address, new MemberVersion(0, 0, 0), false, new Dictionary<string, string>());
            }

            var queue = new MemberConnectionQueue(new NullLoggerFactory());

            // background task that connect members
            async Task ConnectMembers(MemberConnectionQueue memberConnectionQueue, CancellationToken cancellationToken)
            {
                await foreach (var (member, token) in memberConnectionQueue.WithCancellation(cancellationToken))
                {
                    await mutex.WaitAsync().CfAwait();
                    if (!token.IsCancellationRequested) addresses.Add(member.Address);
                    mutex.Release();
                }
            }

            var connecting = ConnectMembers(queue, default);

            // -- connects

            queue.Add(MemberInfo(NetworkAddress.Parse("127.0.0.1:1")));
            queue.Add(MemberInfo(NetworkAddress.Parse("127.0.0.1:2")));

            await AssertEx.SucceedsEventually(() =>
            {
                Assert.That(addresses.Count, Is.EqualTo(2));
                Assert.That(addresses, Does.Contain(NetworkAddress.Parse("127.0.0.1:1")));
                Assert.That(addresses, Does.Contain(NetworkAddress.Parse("127.0.0.1:2")));
            }, 2000, 200);

            // -- can suspend while waiting

            queue.Suspend(); // suspend

            queue.Add(MemberInfo(NetworkAddress.Parse("127.0.0.1:3")));

            await Task.Delay(500);
            Assert.That(addresses.Count, Is.EqualTo(2)); // nothing happened

            queue.Resume(); // resume => will process

            await AssertEx.SucceedsEventually(() =>
            {
                Assert.That(addresses.Count, Is.EqualTo(3));
                Assert.That(addresses, Does.Contain(NetworkAddress.Parse("127.0.0.1:3")));
            }, 2000, 200);

            // -- can suspend while connecting

            await mutex.WaitAsync(); // block

            queue.Add(MemberInfo(NetworkAddress.Parse("127.0.0.1:4")));
            await Task.Delay(500);

            queue.Suspend();
            mutex.Release(); // resume => should cancel current connect

            Assert.That(addresses.Count, Is.EqualTo(3));

            queue.Add(MemberInfo(NetworkAddress.Parse("127.0.0.1:5")));

            await Task.Delay(500);
            Assert.That(addresses.Count, Is.EqualTo(3));

            queue.Resume();

            await AssertEx.SucceedsEventually(() =>
            {
                Assert.That(addresses.Count, Is.EqualTo(4));
                Assert.That(addresses, Does.Contain(NetworkAddress.Parse("127.0.0.1:5")));
            }, 2000, 200);

            // -- can drain empty

            queue.Suspend();
            queue.Resume(true);

            // -- can drain non-empty

            await mutex.WaitAsync();

            queue.Add(MemberInfo(NetworkAddress.Parse("127.0.0.1:6")));
            queue.Add(MemberInfo(NetworkAddress.Parse("127.0.0.1:7")));
            queue.Add(MemberInfo(NetworkAddress.Parse("127.0.0.1:8")));
            await Task.Delay(500);

            queue.Suspend();
            mutex.Release();

            queue.Resume(true);

            Assert.That(addresses.Count, Is.EqualTo(4));

            // -- drained

            queue.Add(MemberInfo(NetworkAddress.Parse("127.0.0.1:9")));

            await Task.Delay(500);
            Assert.That(addresses.Count, Is.EqualTo(5));
            Assert.That(addresses, Does.Contain(NetworkAddress.Parse("127.0.0.1:9")));

            // -- the end

            await queue.DisposeAsync();
            await AssertEx.SucceedsEventually(() => Assert.That(connecting.IsCompleted), 2000, 200);

            // ok, but nothing will happen since the queue has been disposed
            queue.Add(MemberInfo(NetworkAddress.Parse("127.0.0.1:10")));
        }
    }
}
