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
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Networking;
using Hazelcast.Testing;
using Hazelcast.Testing.Logging;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Hazelcast.Tests.Remote
{
    [TestFixture]
    public class ReconnectTests : ClusterRemoteTestBase
    {
        private IDisposable HConsoleForTest()

            => HConsole.Capture(options => options
                .ClearAll()
                //.Set(x => x.Verbose())
                //.Set(this, x => x.SetPrefix("TEST"))
                //.Set<AsyncContext>(x => x.Quiet())
                //.Set<SocketConnectionBase>(x => x.SetIndent(1).SetLevel(0).SetPrefix("SOCKET"))
                .Set(x => x.Quiet())
                .Set<HConsoleLoggerProvider>(x => x.Verbose())
            );

        [Test]
        public async Task ReconnectAsync()
        {
            using var _ = HConsoleForTest();

            // add one member

            var member = await RcClient.StartMemberAsync(RcCluster);

            // connect & use a client

            var options = new HazelcastOptionsBuilder()
                .With((configuration, o) =>
                {
                    o.ClusterName = RcCluster.Id;

                    o.Networking.Addresses.Clear();
                    o.Networking.Addresses.Add("127.0.0.1:5701");

                    o.Networking.ReconnectMode = ReconnectMode.ReconnectAsync;
                    o.Networking.ConnectionRetry.InitialBackoffMilliseconds = 1_000; // initially wait for 1s
                    o.Networking.ConnectionRetry.Multiplier = 1; // and keep waiting for 1s
                    o.Networking.ConnectionRetry.Jitter = 0; // exactly
                    o.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 240_000; // give up after 4mn
                    
                    o.AddSubscriber(events => events
                        .StateChanged((sender, args) =>
                        {
                            HConsole.WriteLine(this, $"client state changed: {args.State}");
                            o.LoggerFactory.Service.CreateLogger<ReconnectTests>().LogDebug("Client state changed: {State}", args.State);
                        }));
                })
                .WithHConsoleLogger()
                .WithUserSecrets(GetType().Assembly)
                .Build();

            HConsole.WriteLine(this, "Start client");
            var client = await HazelcastClientFactory.StartNewClientAsync(options);
            var map = await client.GetMapAsync<string, string>("test-map");
            await map.SetAsync("key", "value");
            Assert.That(await map.GetAsync("key"), Is.EqualTo("value"));

            // kill the member

            HConsole.WriteLine(this, "Stop member");
            await RcClient.StopMemberWaitClosedAsync(client, RcCluster, member);

            // using the client throws
            // and the client is frantically trying to reconnect

            HConsole.WriteLine(this, "Use client");
            await AssertEx.ThrowsAsync<ClientOfflineException>(async () =>
            {
                await map.GetAsync("key");
            });

            // add a member again
            // at some point, the client will reconnect

            HConsole.WriteLine(this, "Start member");
            member = await RcClient.StartMemberAsync(RcCluster);

            // use the client
            // initially, it should keep throwing, but eventually it should work

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var count = 0;

            HConsole.WriteLine(this, "Use client");
            await AssertEx.SucceedsEventually(async () =>
            {
                HConsole.WriteLine(this, $"Attempt {count++} at {stopwatch.ElapsedMilliseconds}ms");
                // of course the value will be gone, but after a while this should not throw
                Assert.That(await map.GetAsync("key"), Is.Null);
            }, 30_000, 500);

            // we're done
        }
    }
}
