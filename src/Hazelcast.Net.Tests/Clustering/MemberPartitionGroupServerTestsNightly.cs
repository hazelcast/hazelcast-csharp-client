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
using System.Threading.Tasks;
using Hazelcast.Networking;
using Hazelcast.Testing;
using Hazelcast.Testing.Conditions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
namespace Hazelcast.Tests.Clustering
{
    [Category("enterprise,nightly")]
    [ServerCondition("5.5")]
    [Timeout(120_000)]
    public class MemberPartitionGroupServerTestsNightly : MultiMembersRemoteTestBase
    {
        protected override string RcClusterConfiguration => Resources.ClusterPGEnabled;

        [SetUp]
        public async Task Setup()
        {
            await CreateCluster();
        }
        [TearDown]
        public async Task TearDown()
        {
            await MembersOneTimeTearDown();
        }

        [TestCase(RoutingStrategy.PartitionGroups)]
        public async Task TestMultiMemberRoutingConnectsNextGroupWhenDisconnected(RoutingStrategy routingStrategy)
        {
            var address1 = "127.0.0.1:5701";
            var address2 = "127.0.0.1:5702";
            var address3 = "127.0.0.1:5703";

            var addreses = new string[] { address1, address2, address3 };

            // create a client with the given routing strategy
            var client = await CreateClient(routingStrategy, addreses, "client1");

            // it should connect to first address
            Assert.That(client.Cluster.Connections.Count, Is.EqualTo(1));

            var connectedAddress = client.Members.First(p => p.IsConnected).Member.ConnectAddress.ToString();

            AssertClientOnlySees(client, connectedAddress);

            var effectiveMembers = client.Cluster.Members.GetMembersForConnection();
            Assert.That(effectiveMembers.Count(), Is.EqualTo(1));
            Assert.That(effectiveMembers.Select(p => p.ConnectAddress.ToString()), Contains.Item(connectedAddress));
            // Kill the connected member so that client can go to next group
            var connectedMember = RcMembers.Values.Where(m => connectedAddress.Equals($"{m.Host}:{m.Port}")).Select(m => m.Uuid).First();
            RemoveMember(connectedMember);

            await AssertEx.SucceedsEventually(() => Assert.That(client.State, Is.EqualTo(ClientState.Disconnected)), 60_000, 10);

            await AssertEx.SucceedsEventually(() =>
            {
                Assert.That(client.Cluster.Connections.Count, Is.EqualTo(1));
                Assert.That(client.State, Is.EqualTo(ClientState.Connected));
                var reConnectedAddress = client.Members.First(p => p.IsConnected).Member.ConnectAddress.ToString();
                effectiveMembers = client.Cluster.Members.GetMembersForConnection();

                Assert.That(reConnectedAddress, Is.Not.EqualTo(connectedAddress));
                Assert.That(client.Cluster.Connections.Count, Is.EqualTo(1));
                Assert.That(client.Members.Where(p => p.IsConnected).Select(p => p.Member.ConnectAddress.ToString()), Contains.Item(reConnectedAddress));
                Assert.That(effectiveMembers.Count(), Is.EqualTo(1));
                Assert.That(effectiveMembers.Select(p => p.ConnectAddress.ToString()), Contains.Item(reConnectedAddress));
            }, 60_000, 500);

        }

        private void AssertClientOnlySees(HazelcastClient client, string address, int clusterSize = 3)
        {
            var member = RcMembers.Values.FirstOrDefault(m => address.Equals($"{m.Host}:{m.Port}"));

            Assert.IsNotNull(member);

            var members = client.Cluster.Members;
            var memberId = Guid.Parse(member.Uuid);

            Assert.That(members.GetMembers().Count(), Is.EqualTo(clusterSize), "Current cluster size " + RcMembers.Count);
            Assert.That(client.Cluster.Connections.Count, Is.EqualTo(1));
            Assert.True(client.Cluster.Connections.Contains(memberId), "Member is not connected");
            Assert.That(members.SubsetClusterMembers.GetSubsetMemberIds().Count(), Is.EqualTo(1));
            Assert.That(members.SubsetClusterMembers.GetSubsetMemberIds(), Contains.Item(memberId));
        }
        private async Task<HazelcastClient> CreateClient(RoutingStrategy routingStrategy, string[] address, string clientName, RoutingModes routingMode = RoutingModes.MultiMember)
        {
            var options = new HazelcastOptionsBuilder()
                .With(args =>
                {
                    for (int i = 0; i < address.Length; i++)
                    {

                        args.Networking.Addresses.Add(address[i]);
                    }
                    args.ClientName = clientName;
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
