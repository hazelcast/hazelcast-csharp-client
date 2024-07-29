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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Networking;
using Hazelcast.Testing;
using Hazelcast.Testing.Conditions;
using Hazelcast.Testing.Remote;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Thrift.Protocol;
namespace Hazelcast.Tests.Clustering
{
    [Category("enterprise")]
    [ServerCondition("5.5")]
    [Timeout(30_000)]
    public class MemberPartitionGroupServerTests : MultiMembersRemoteTestBase
    {
        protected override string RcClusterConfiguration => Resources.ClusterPGEnabled;

        [OneTimeSetUp]
        public async Task TearDown()
        {
            await CreateCluster();
        }

        [TestCase(RoutingStrategy.PartitionGroups)]
        public async Task TestMultiMemberRoutingWorks(RoutingStrategy routingStrategy)
        {
            var address1 = "127.0.0.1:5701";
            var address2 = "127.0.0.1:5702";
            var address3 = "127.0.0.1:5703";

            // create a client with the given routing strategy
            var client1 = await CreateClient(routingStrategy, address1);
            var client2 = await CreateClient(routingStrategy, address2);
            var client3 = await CreateClient(routingStrategy, address3);

            AssertClientOnlySees(client1, address1);
            AssertClientOnlySees(client2, address2);
            AssertClientOnlySees(client3, address3);

            // Scale Up
            var member4 = await AddMember();

            AssertClientOnlySees(client1, address1);
            AssertClientOnlySees(client2, address2);
            AssertClientOnlySees(client3, address3);

            // Scale Down
            await RemoveMember(member4.Uuid);

            AssertClientOnlySees(client1, address1);
            AssertClientOnlySees(client2, address2);
            AssertClientOnlySees(client3, address3);

            // Kill a member and check if clients are still connected correct members
            var memberId3 = RcMembers.Values.Where(m => address3.Equals($"{m.Host}:{m.Port}")).Select(m => m.Uuid).First();
            await RemoveMember(memberId3);

            AssertClientOnlySees(client1, address1);
            AssertClientOnlySees(client2, address2);

            await AssertEx.SucceedsEventually(()
                    => Assert.That(client3.State, Is.EqualTo(ClientState.Disconnected)),
                5000, 500);

            Member member3;
            var nAddress3 = NetworkAddress.Parse(address3);
            while (true)
            {
                member3 = await AddMember();
                var created = new NetworkAddress(member3.Host, member3.Port);
                if (created == nAddress3) break;

                await RemoveMember(member3.Uuid);
            }

            await AssertEx.SucceedsEventually(()
                    => Assert.That(client3.State, Is.EqualTo(ClientState.Connected)),
                10_000, 500);

            // Check if client1 is connected to the new member
            // cannot use the old member id since we can only either kill or create member
            Assert.That(client3.Members.Count(), Is.EqualTo(1));
            Assert.That(client3.Members.First().Member.ConnectAddress, Is.EqualTo(nAddress3));

            AssertClientOnlySees(client1, address1);
            AssertClientOnlySees(client2, address2);
            AssertClientOnlySees(client3, address3);
        }

        [TestCase(RoutingModes.MultiMember)]
        [TestCase(RoutingModes.SingleMember)]
        [TestCase(RoutingModes.AllMembers)]
        public async Task TestClientCollectsClusterEvents(RoutingModes routingMode)
        {
            var address1 = "127.0.0.1:5701";
            var address2 = "127.0.0.1:5702";
            var keyCount = 3 * 271;

            // create a client with the given routing strategy for catching the events.
            var client1 = await CreateClient(RoutingStrategy.PartitionGroups, address1, routingMode);
            var client1Count = 0;
            var mapName = $"map_{routingMode}";
            var map1 = await client1.GetMapAsync<int, int>(mapName);

            await map1.SubscribeAsync((eventHandler) =>
            {
                eventHandler.EntryAdded((sender, args) =>
                {
                    Interlocked.Increment(ref client1Count);
                });
            }, false);


            // create a dummy client  for creating events
            var client2 = await CreateClient(RoutingStrategy.PartitionGroups, address2, RoutingModes.SingleMember);

            var map2 = await client2.GetMapAsync<int, int>(map1.Name);

            for (int i = 0; i < keyCount; i++)
            {
                await map2.PutAsync(i, i);
            }

            await AssertEx.SucceedsEventually(async () =>
            {
                Assert.That(await map1.GetSizeAsync(), Is.GreaterThan(271), "Entry Count");
                Assert.That(client1Count, Is.GreaterThan(271), "Event Count");
            }, 10_000, 200);
        }

        private void AssertClientOnlySees(HazelcastClient client, string address)
        {
            var member = RcMembers.Values.FirstOrDefault(m => address.Equals($"{m.Host}:{m.Port}"));

            Assert.IsNotNull(member);

            var members = client.Cluster.Members;
            var memberId = Guid.Parse(member.Uuid);

            Assert.That(members.GetMembers().Count(), Is.EqualTo(1));
            Assert.That(members.GetMembers().First().Uuid, Is.EqualTo(memberId));
            Assert.That(members.SubsetClusterMembers.GetSubsetMemberIds().Count(), Is.EqualTo(1));
            Assert.That(members.SubsetClusterMembers.GetSubsetMemberIds(), Contains.Item(memberId));
        }
        private async Task<HazelcastClient> CreateClient(RoutingStrategy routingStrategy, string address, RoutingModes routingMode = RoutingModes.MultiMember)
        {
            var options = new HazelcastOptionsBuilder()
                .With(args =>
                {
                    args.ClientName = $"Client:{address}";
                    args.Networking.Addresses.Add(address);
                    args.ClusterName = RcCluster.Id;
                    args.Networking.RoutingMode.Mode = routingMode;
                    args.Networking.RoutingMode.Strategy = routingStrategy;
                    args.Networking.ReconnectMode = ReconnectMode.ReconnectSync;

                    args.AddSubscriber(on => on.StateChanged((client, eventArgs) =>
                    {
                        Console.WriteLine(eventArgs.State);
                    }));

                    args.LoggerFactory.Creator = () => Microsoft.Extensions.Logging.LoggerFactory.Create(
                        conf => conf.AddConsole().SetMinimumLevel(LogLevel.Debug));

                })
                .Build();

            var client = (HazelcastClient) await HazelcastClientFactory.StartNewClientAsync(options);
            return client;
        }
        private async Task CreateCluster(int size = 3)
        {
            for (int i = 0; i < size; i++)
            {
                await AddMember();
            }
        }
    }
}
