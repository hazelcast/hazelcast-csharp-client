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
                .Configure().SetMaxLevel()
                .Configure(this).SetPrefix("TEST")
                .Configure<AsyncContext>().SetMinLevel()
                .Configure<SocketConnectionBase>().SetIndent(1).SetLevel(0).SetPrefix("SOCKET"));

        [Test]
        public async Task SingleFailureAtAddressResolutionShouldNotBlowUpClient()
        {
            using var _ = HConsoleForTest();
            HConsole.WriteLine(this, "Begin");

            // the alt DNS will throw on the Nth invocation of GetHostAddresses
            // the original test threw at '2' and that was when getting an address to connect to
            // the original code & test was not resilient to failures in other places, either
            //
            // 1: getting an address to connect to
            // 2: decoding the auth response (fatal)
            // 3: decoding the members view event (fatal)
            // ...
            var altDns = new AltDns(1);
            using var dnsOverride = HDns.Override(altDns);

            await using var client = await CreateAndStartClientAsync(options =>
            {
                // make sure to use a hostname, not 127.0.0.1, to use the DNS
                options.Networking.Addresses.Clear();
                options.Networking.Addresses.Add("localhost:5701");
            });

            await AssertEx.SucceedsEventually(() =>
            {
                // client is active and connected
                Assert.That(client.IsActive);
                Assert.That(client.IsConnected);
            }, 4000, 500);

            await Task.Delay(1000);

            // dns has been used
            Assert.That(altDns.Count, Is.GreaterThan(0));

            // client is still active and connected
            Assert.That(client.IsActive);
            Assert.That(client.IsConnected);
        }

        private class AltDns : IDns
        {
            private readonly int _failAt;

            public AltDns(int failAt)
            {
                _failAt = failAt;
                HConsole.Configure(x => x.Configure(this).SetPrefix("ADNS"));
            }

            public int Count { get; private set; }

            public string GetHostName() => Dns.GetHostName();

            public IPHostEntry GetHostEntry(string hostNameOrAddress) => Dns.GetHostEntry(hostNameOrAddress);

            public IPHostEntry GetHostEntry(IPAddress address) => Dns.GetHostEntry(address);

            public IPAddress[] GetHostAddresses(string hostNameOrAddress)
            {
                Count++;

                HConsole.TraceLine(this, $"GetHostAddresses {hostNameOrAddress} {Count}/{_failAt}");

                if (Count == _failAt)
                    throw new SocketException((int)SocketError.HostNotFound);

                return Dns.GetHostAddresses(hostNameOrAddress);
            }
        }
    }
}
