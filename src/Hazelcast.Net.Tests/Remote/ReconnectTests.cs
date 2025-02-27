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
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Networking;
using Hazelcast.Testing;
using Hazelcast.Testing.Configuration;
using Hazelcast.Testing.Logging;
using Hazelcast.Testing.Remote;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Hazelcast.Tests.Remote
{
    [TestFixture]
    public class ReconnectTests : ClusterRemoteTestBase
    {
        private readonly List<Member> _members = new List<Member>();

        [TearDown]
        public async Task TearDown()
        {
            // TODO: this should be built-in
            //await RcClient.StopAllMembersAsync(RcCluster);
            foreach (var member in _members)
                await RcClient.StopMemberAsync(RcCluster, member);
        }

        [Test]
        public async Task ReconnectAsync([Values] bool reconnectAsync)
        {
            HConsole.Configure(options => options.ConfigureDefaults(this));

            // add one member

            var member = await RcClient.StartMemberAsync(RcCluster);
            _members.Add(member);

            // connect & use a client

            var options = new HazelcastOptionsBuilder()
                .With((configuration, o) =>
                {
                    // note: default is sync, ie invocations are put on hold waiting for reconnection
                    // alternatively, with async, invocations immediately throw
                    o.Networking.ReconnectMode = reconnectAsync ? ReconnectMode.ReconnectAsync : ReconnectMode.ReconnectSync;

                    o.ClusterName = RcCluster.Id;

                    o.Networking.Addresses.Clear();
                    o.Networking.Addresses.Add("127.0.0.1:5701");

                    o.Networking.ConnectionRetry.InitialBackoffMilliseconds = 1_000; // initially wait for 1s
                    o.Networking.ConnectionRetry.Multiplier = 1; // and keep waiting for 1s
                    o.Networking.ConnectionRetry.Jitter = 0; // exactly
                    o.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 240_000; // give up after 4mn

                    o.Messaging.RetryTimeoutSeconds = 4; // ok to retry invocations for a few s then fail w/ TaskTimeoutException

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
            _members.Remove(member);

            // using the client throws
            // and the client is frantically trying to reconnect

            HConsole.WriteLine(this, "Use client");
            if (reconnectAsync)
                // async: throws immediately, does not even try the invocation
                await AssertEx.ThrowsAsync<ClientOfflineException>(async () => await map.GetAsync("key"));
            else
                // sync: tries the invocation, eventually times out
                await AssertEx.ThrowsAsync<TaskTimeoutException>(async () => await map.GetAsync("key"));

            // add a member again
            // at some point, the client will reconnect

            HConsole.WriteLine(this, "Start member");
            member = await RcClient.StartMemberAsync(RcCluster);
            _members.Add(member);

            // use the client
            // initially, it should keep throwing, but eventually it should work

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var count = 0;
            const int max = 60; // try 60 times ie for 30 seconds

            HConsole.WriteLine(this, "Use client");
            while (count < max)
            {
                HConsole.WriteLine(this, $"Attempt {count++} at {stopwatch.ElapsedMilliseconds}ms");
                try
                {
                    // of course the value will be gone, but after a while this should not throw
                    var value = await map.GetAsync("key");
                    break;
                }
                catch (Exception ex)
                {
                    // as long as it fails, we'll get the proper exception
                    Assert.That(ex, reconnectAsync ? Is.InstanceOf<ClientOfflineException>() : Is.InstanceOf<TaskTimeoutException>());
                }

                await Task.Delay(500);
            }

            // must break before max tries
            Assert.That(count, Is.LessThan(max));

            // we're done
        }
    }
}
