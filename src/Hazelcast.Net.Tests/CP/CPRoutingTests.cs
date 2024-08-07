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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.CP;
using Hazelcast.Messaging;
using Hazelcast.Networking;
using Hazelcast.Testing;
using NUnit.Framework;
namespace Hazelcast.Tests.CP
{
    [Category("enterprise")]
    [Timeout(30_000)]
    public class CPRoutingTests : MultiMembersRemoteTestBase
    {
        protected override string RcClusterConfiguration => TestFiles.ReadAllText(this, "Cluster/cp.xml");

        [SetUp]
        public async Task SetUp()
        {
            // CP-subsystem wants at least 3 members
            for (var i = 0; i < 3; i++) await AddMember();
        }

        [Test]
        public async Task TestCPRequestRoutingToLeader()
        {
            const string groupName = "myGroup";
            const string cpObjectName = "myAtomicLong";
            var options = new HazelcastOptionsBuilder()
                .With((config =>
                {
                    config.Networking.CPDirectToLeaderEnabled = true;
                    config.Networking.RoutingMode.Mode = RoutingModes.AllMembers;
                    var lastMember = RcMembers.Values.Last();
                    config.Networking.Addresses.Add($"{lastMember.Host}:{lastMember.Port}");
                    config.ClusterName = RcCluster.Id;
                    config.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 10_000;
                })).Build();


            // Assert multiple that the addAndGet operation is sent to the leader
            // First round creates new group
            // Second round uses the existing group
            for (var i = 0; i < 2; i++)
            {
                var (msgList, leaderMemberId) = await AssertCPRoutes(options, cpObjectName, groupName);

                var msgCount = 0;

                foreach (var (msg, id) in msgList)
                {
                    if (msg.OperationName == "AtomicLong.AddAndGet" && id == leaderMemberId)
                        msgCount++;
                }

                // There is one addAndGet operation sent to the leader
                Assert.That(msgCount, Is.EqualTo(1));    
            }
        }
        private static async Task<(List<(ClientMessage, Guid)>, Guid)> AssertCPRoutes(HazelcastOptions options, string cpObjectName, string groupName)
        {
            var client = HazelcastClientFactory.CreateClient(options);

            // Capture the message sent to the leader
            var msgList = new List<(ClientMessage, Guid)>();
            client.Cluster.Messaging.SendingMessage += (msg, targetMemberId) =>
            {
                msgList.Add((msg, targetMemberId));
                return default;
            };

            AsyncContext.Ensure();
            var cts = new CancellationTokenSource();
            await client.StartAsync(cts.Token);


            Assert.That(client.Cluster.Members.GetMembers().Count(), Is.EqualTo(3));

            await AssertEx.SucceedsEventually(() =>
            {
                // Authentications override the cp list, wait for event.
                Assert.That(client.Cluster.Members.ClusterCPGroups.Count, Is.GreaterThan(0));
            }, 10_000, 200);


            // Create a CP object
            await using var cpAtomicLong = await client.CPSubsystem.GetAtomicLongAsync($"{cpObjectName}@{groupName}");
            Assert.That(cpAtomicLong.GroupId.Name, Is.EqualTo(groupName));
            var leaderMemberId = client.Cluster.Members.ClusterCPGroups.GetLeaderMemberId((CPGroupId) cpAtomicLong.GroupId);

            var val = await cpAtomicLong.AddAndGetAsync(1);

            // Dispose the client to stop the SendingMessage listener
            await client.DisposeAsync();
            return (msgList, leaderMemberId);
        }
        
    }
}
