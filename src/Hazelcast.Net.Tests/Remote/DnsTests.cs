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
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Networking;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Remote
{
    [TestFixture]
    public class DnsTests : SingleMemberRemoteTestBase
    {
        private IDisposable HConsoleForTest()

            => HConsole.Capture(options => options
                .ClearAll()
                .Set(x => x.Verbose())
                .Set(this, x => x.SetPrefix("TEST"))
                .Set<AsyncContext>(x => x.Quiet())
                .Set<SocketConnectionBase>(x => x.SetIndent(1).SetLevel(0).SetPrefix("SOCKET")));

        [Test]
        public async Task SingleFailureAtAddressResolutionShouldNotBlowUpClient()
        {
            using var _ = HConsoleForTest();

            using var altDns = HDns.Override(new AltDns(2));

            await using var client = await CreateAndStartClientAsync();

            await AssertEx.SucceedsEventually(() =>
            {
                // client is active and connected
                Assert.That(client.IsActive);
                Assert.That(client.IsConnected);
            }, 4000, 500);

            await Task.Delay(1000);

            // client is still active and connected
            Assert.That(client.IsActive);
            Assert.That(client.IsConnected);
        }

        private class AltDns : IDns
        {
            private readonly int _failAt;
            private int _count;

            public AltDns(int failAt)
            {
                _failAt = failAt;
            }

            public string GetHostName() => Dns.GetHostName();

            public IPHostEntry GetHostEntry(string hostNameOrAddress) => Dns.GetHostEntry(hostNameOrAddress);

            public IPHostEntry GetHostEntry(IPAddress address) => Dns.GetHostEntry(address);

            public IPAddress[] GetHostAddresses(string hostNameOrAddress)
            {
                if (++_count == _failAt)
                    throw new SocketException((int)SocketError.HostNotFound);

                return Dns.GetHostAddresses(hostNameOrAddress);
            }
        }
    }
}
