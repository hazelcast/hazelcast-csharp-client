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
using Hazelcast.Networking;
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

            async Task Connect(NetworkAddress address, CancellationToken cancellationToken)
            {
                await mutex.WaitAsync();
                addresses.Add(address);
                mutex.Release();
            }

            var connectAddresses = new ConnectAddresses(Connect, new NullLoggerFactory());

            // -- connects

            connectAddresses.Add(NetworkAddress.Parse("127.0.0.1:1"));
            connectAddresses.Add(NetworkAddress.Parse("127.0.0.1:2"));

            await Task.Delay(500);
            Assert.That(addresses.Count, Is.EqualTo(2));
            Assert.That(addresses, Does.Contain(NetworkAddress.Parse("127.0.0.1:1")));
            Assert.That(addresses, Does.Contain(NetworkAddress.Parse("127.0.0.1:2")));

            // -- can pause while waiting

            await connectAddresses.PauseAsync();

            connectAddresses.Add(NetworkAddress.Parse("127.0.0.1:3"));

            await Task.Delay(500);
            Assert.That(addresses.Count, Is.EqualTo(2));

            await connectAddresses.ResumeAsync();

            await Task.Delay(500);
            Assert.That(addresses.Count, Is.EqualTo(3));
            Assert.That(addresses, Does.Contain(NetworkAddress.Parse("127.0.0.1:3")));

            // -- can pause while connecting

            await mutex.WaitAsync();

            connectAddresses.Add(NetworkAddress.Parse("127.0.0.1:4"));
            await Task.Delay(500);

            var pausing = connectAddresses.PauseAsync();

            await Task.Delay(500);
            Assert.That(pausing.IsCompleted, Is.False);

            mutex.Release();
            await pausing;

            Assert.That(addresses.Count, Is.EqualTo(4));
            Assert.That(addresses, Does.Contain(NetworkAddress.Parse("127.0.0.1:4")));

            connectAddresses.Add(NetworkAddress.Parse("127.0.0.1:5"));

            await Task.Delay(500);
            Assert.That(addresses.Count, Is.EqualTo(4));

            await connectAddresses.ResumeAsync();

            await Task.Delay(500);
            Assert.That(addresses.Count, Is.EqualTo(5));
            Assert.That(addresses, Does.Contain(NetworkAddress.Parse("127.0.0.1:5")));

            // -- can drain empty

            await connectAddresses.PauseAsync();
            await connectAddresses.ResumeAsync(true);

            // -- can drain non-empty

            await mutex.WaitAsync();

            connectAddresses.Add(NetworkAddress.Parse("127.0.0.1:6"));
            connectAddresses.Add(NetworkAddress.Parse("127.0.0.1:7"));
            connectAddresses.Add(NetworkAddress.Parse("127.0.0.1:8"));
            await Task.Delay(500);

            pausing = connectAddresses.PauseAsync();
            mutex.Release();
            await pausing;

            await connectAddresses.ResumeAsync(true);

            Assert.That(addresses.Count, Is.EqualTo(6));
            Assert.That(addresses, Does.Contain(NetworkAddress.Parse("127.0.0.1:6")));

            // -- drained

            connectAddresses.Add(NetworkAddress.Parse("127.0.0.1:9"));

            await Task.Delay(500);
            Assert.That(addresses.Count, Is.EqualTo(7));
            Assert.That(addresses, Does.Contain(NetworkAddress.Parse("127.0.0.1:9")));

            // -- the end

            await connectAddresses.DisposeAsync();

            Assert.Throws<ObjectDisposedException>(() => connectAddresses.Add(NetworkAddress.Parse("127.0.0.1:10")));
        }
    }
}
