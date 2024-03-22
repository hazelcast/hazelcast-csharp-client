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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Hazelcast.Exceptions;
using Hazelcast.Networking;
using Hazelcast.Testing;
using Hazelcast.Testing.Logging;
using NUnit.Framework;

namespace Hazelcast.Tests.Clustering
{
    [TestFixture]
    [Explicit("Stress tests, take time.")]
    public class MembersLifecycleTests : MultiMembersRemoteTestBase
    {
        private const int RunCount = 16;
        private const int MinutesPerRun = 4;
        private const int MembersTotal = 5;
        private const int InvocationTimeoutMinutes = 4; // default is 2' but ... not always enough

        [Test]
        [Timeout(RunCount * MinutesPerRun * 60 * 1000)]
        public async Task Test()
        {
            HConsole.Configure(options => options.ConfigureDefaults(this).Configure().SetLevel(1));
 
            for (var i = 0; i < RunCount; i++)
            {
                HConsole.WriteLine(this, $"-------- RUN #{i} --------");
                await Run();

                // don't leave anything behind this run!
                foreach (var member in RcMembers.Values)
                {
                    await RemoveMember(Guid.Parse(member.Uuid));
                }
            }
        }

        private string RandomValue => $"map-{RandomProvider.Random.Next(9999):0000}";

        private async Task UseClient(IHMap<string, string> map, int durationMilliseconds, int  pauseMilliseconds)
        {
            var count = durationMilliseconds / pauseMilliseconds;

            for (var i = 0; i < count; i++)
            {
                await UseClientOnce(map);
                await Task.Delay(pauseMilliseconds);
            }
        }

        private async Task UseClientOnce(IHMap<string, string> map)
        {
            var stopwatch = Stopwatch.StartNew();

            string key, value;

            try
            {
                key = value = RandomValue;
                stopwatch.Restart();
                await map.SetAsync(key, value);
                HConsole.WriteLine(this, $"Set map value ({(int)stopwatch.Elapsed.TotalSeconds}s)");
            }
            catch (TargetUnreachableException)
            {
                HConsole.WriteLine(this, "Failed to set map value: target unreachable");
            }

            try
            {
                key = RandomValue;
                stopwatch.Restart();
                value = await map.GetAsync(key);
                HConsole.WriteLine(this, $"Got map value ({(int)stopwatch.Elapsed.TotalSeconds}s)");
                if (value != null) Assert.That(value, Is.EqualTo(key));
            }
            catch (TargetUnreachableException)
            {
                HConsole.WriteLine(this, "Failed to get map value: target unreachable");
            }
        }

        // this creates members & connect to the cluster, then removes all but 1 member,
        // all while using the client - things should go smoothly
        private async Task Run()
        {
            var totalStopwatch = Stopwatch.StartNew();
            var stopwatch = new Stopwatch();

            HConsole.WriteLine(this, $"Begin run on cluster {RcCluster.Id.Substring(0, 7)}");

            // add all members
            for (var i = 0; i < MembersTotal; i++)
            {
                HConsole.WriteLine(this, $"Add member #{i}...");
                stopwatch.Restart();
                var member = await AddMember();
                HConsole.WriteLine(this, $"Added {member.Uuid.Substring(0, 7)} at {member.Host}:{member.Port} ({(int)stopwatch.Elapsed.TotalSeconds}s)");
            }

            // prepare options
            var options = new HazelcastOptionsBuilder()
                .WithHConsoleLogger()
                .Build();

            options.Networking.Addresses.Clear();
            options.Networking.Addresses.Add("127.0.0.1:5701");
            options.ClusterName = RcCluster.Id;
            options.Messaging.RetryTimeoutSeconds = InvocationTimeoutMinutes * 60;

            // subscribe to members updated
            var membersCount = 0;
            var createdObject = false;
            options.AddSubscriber(events => events
                .MembersUpdated((sender, args) =>
                {
                    HConsole.WriteLine(this, $"Handle MembersUpdated ({args.Members.Count} members)");
                    membersCount = args.Members.Count;
                })
                .ObjectCreated((sender, args) =>
                {
                    HConsole.WriteLine(this, $"Object {args.ServiceName}:{args.Name} created");
                    createdObject = true;
                }));

            // start client
            HConsole.WriteLine(this, "Start client...");
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);
            await ((HazelcastClient) client).DestroyAsync(ServiceNames.Map, "test-map").CfAwait();

            // immediately begin using the client - while connections to all members are established
            HConsole.WriteLine(this, "Get map...");
            await using var map = await client.GetMapAsync<string, string>("test-map-" + RandomProvider.Random.Next(1000));

            await AssertEx.SucceedsEventually(() =>
            {
                Assert.That(createdObject);
            }, 2000, 200);

            HConsole.WriteLine(this, "Use map...");
            await UseClient(map, 2000, 200);

            // all members are here
            await AssertEx.SucceedsEventually(() =>
            {
                Assert.That(membersCount, Is.EqualTo(MembersTotal));
            }, 2000, 200);

            // keep using the client, but remove members down to 1 last member
            HConsole.WriteLine(this, "Remove members...");
            while (RcMembers.Count > 1)
            {
                var (memberId, member) = RcMembers.First();
                HConsole.WriteLine(this, $"Remove member {member.Uuid.Substring(0, 7)} at {member.Host}:{member.Port}...");
                stopwatch.Restart();
                await RemoveMember(memberId);
                HConsole.WriteLine(this, $"Removed member {member.Uuid.Substring(0, 7)} ({(int)stopwatch.Elapsed.TotalSeconds}s)");

                await Task.Delay(500);

                await UseClientOnce(map);
            }

            // take time for things to stabilize

            // keep using the client
            HConsole.WriteLine(this, "Use map...");
            await UseClient(map, 2000, 200);

            // all members but one are gone
            await AssertEx.SucceedsEventually(() =>
            {
                Assert.That(membersCount, Is.EqualTo(1));
            }, 8000, 200);

            // now terminate the client
            HConsole.WriteLine(this, "Dispose client...");
            await client.DisposeAsync();

            HConsole.WriteLine(this, $"End ({(int)totalStopwatch.Elapsed.TotalSeconds}s)");
        }
    }
}
