﻿// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Aggregation;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.CP;
using Hazelcast.Exceptions;
using Hazelcast.Messaging;
using Hazelcast.Models;
using Hazelcast.Networking;
using Hazelcast.Partitioning;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Protocol.Models;
using Hazelcast.Serialization;
using Hazelcast.Serialization.ConstantSerializers;
using Hazelcast.Testing;
using Hazelcast.Testing.Logging;
using Hazelcast.Testing.TestServer;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using static NSubstitute.Substitute;
namespace Hazelcast.Tests.Clustering
{
    // internal for class MemberPartitionGroup : ISubsetClusterMembers
    internal class MemberPartitionGroupTests
    {
        private List<Server<ServerState>> _servers = new();

        [TearDown]
        public async Task TearDown()
        {
            foreach (var server in _servers)
            {
                await server.StopAsync().CfAwait();
            }
        }

        [TestCase(@"[{ ""raftId"":{ ""seed"":9, ""id"":3, ""name"":""grp1"" }, ""leaderUUID"":""fa270257-5767-45bf-a3c6-bafe17bed525"" }]", "grp1", 9, 3, "fa270257-5767-45bf-a3c6-bafe17bed525", 1)]
        [TestCase(@"[]", "grp1", 0, 0, "", 0)]
        public void TestAuthenticatorCanParseCPGroups(string jsonString, string groupName, int seed, int id, string leaderId, int count)
        {
            ClientAuthenticationCodec.ResponseParameters response = new ClientAuthenticationCodec.ResponseParameters();
            response.KeyValuePairs = new Dictionary<string, string>();
            response.KeyValuePairs[ClusterCPGroups.CPGroupsJsonField] = jsonString;
            response.IsKeyValuePairsExists = true;

            var authenticator = CreateAuthenticator();

            var cpGroups = authenticator.ParseCPGroupLeaderIds(response);

            Assert.That(cpGroups.Count, Is.EqualTo(count));
            foreach (var g in cpGroups)
            {
                Assert.That(g.Key.Name, Is.EqualTo(groupName));
                Assert.That(g.Key.Seed, Is.EqualTo(seed));
                Assert.That(g.Key.Id, Is.EqualTo(id));
                Assert.That(g.Value, Is.EqualTo(Guid.Parse(leaderId)));
            }
        }

        [TestCase("{ \"version\":\"1\", \"groups\":[[ \"fa270257-5767-45bf-a3c6-bafe17bed525\",\"fa756344-97ca-43f5-9d8d-23b960e4d445\"]]}", "fa270257-5767-45bf-a3c6-bafe17bed525", 2, 1)]
        [TestCase("{\"version\":\"1\", \"groups\":[[\"fa270257-5767-45bf-a3c6-bafe17bed525\",\"fa756344-97ca-43f5-9d8d-23b960e4d445\"]]}", "fa270257-5767-45bf-a3c6-bafe17bed525", 2, 1)]
        [TestCase("{\"version\":\"5\", \"groups\":[[\"fa270257-5767-45bf-a3c6-bafe17bed525\"]]}", "fa270257-5767-45bf-a3c6-bafe17bed525", 1, 5)]
        [TestCase("{\"version\":\"1\", \"groups\":[['']]]", "", 0, -1)]
        [TestCase("", "", 0, -1)]
        [TestCase("{\"version\":\"-1\", \"groups\":[[\"fa270257-5767-45bf-a3c6-bafe17bed525\",\"fa756344-97ca-43f5-9d8d-23b960e4d445\",\"fa756344-97ca-43f5-9d8d-23b960e4d445\"]]}", "fa270257-5767-45bf-a3c6-bafe17bed525", 2, -1)]
        public void TestAuthenticatorCanParseMemberList(string memberList, string memberId, int count, int version)
        {
            ClientAuthenticationCodec.ResponseParameters response = new ClientAuthenticationCodec.ResponseParameters();
            response.KeyValuePairs = new Dictionary<string, string>();
            response.KeyValuePairs[MemberPartitionGroup.PartitionGroupRootJsonField] = memberList;
            response.ClusterId = Guid.NewGuid();
            response.IsKeyValuePairsExists = true;
            response.MemberUuid = string.IsNullOrEmpty(memberId) ? Guid.NewGuid() : Guid.Parse(memberId);

            var authenticator = CreateAuthenticator();

            var memberGroup = authenticator.ParsePartitionMemberGroups(response);
            Assert.IsNotNull(memberGroup);
            Assert.AreEqual(count, memberGroup.SelectedGroup.Count);
            Assert.AreEqual(version, memberGroup.Version);
            Assert.AreEqual(response.ClusterId, memberGroup.ClusterId);
            Assert.AreEqual(response.MemberUuid, memberGroup.MemberReceivedFrom);
        }
        private static Authenticator CreateAuthenticator()
        {
            var serializationService = new SerializationServiceBuilder(new NullLoggerFactory())
                .AddDefinitions(new ConstantSerializerDefinitions()) // use constant serializers not CLR serialization
                .AddHook<AggregatorDataSerializerHook>()
                .Build();

            var authenticator = new Authenticator(new AuthenticationOptions(), serializationService, NullLoggerFactory.Instance);
            return authenticator;
        }


        [TestCaseSource(nameof(MemberGroupCases))]
        public void TestMemberPartitionGroupRemovesCorrectMember(MemberGroups group1,
            MemberGroups group2,
            Guid expectedMemberId,
            int expectedVersion,
            int expectedGroupSize)
        {

            ISubsetClusterMembers memberPartitionGroup = new MemberPartitionGroup(new NetworkingOptions(), NullLogger.Instance);

            memberPartitionGroup.SetSubsetMembers(group1);
            memberPartitionGroup.SetSubsetMembers(group2);

            var removedId = memberPartitionGroup.GetSubsetMemberIds().Where(p => p != expectedMemberId).FirstOrDefault();
            memberPartitionGroup.RemoveSubsetMember(removedId);

            Assert.AreEqual(expectedVersion, ((MemberPartitionGroup) memberPartitionGroup).CurrentGroups.Version);
            Assert.That(memberPartitionGroup.GetSubsetMemberIds(), Is.Not.Contains(removedId));

            if (removedId != Guid.Empty)
                Assert.That(((MemberPartitionGroup) memberPartitionGroup).CurrentGroups.SelectedGroup.Count, Is.EqualTo(expectedGroupSize - 1));
        }

        [TestCaseSource(nameof(MemberGroupCases))]
        public void TestMemberPartitionGroupPicksCorrectGroup(MemberGroups group1,
            MemberGroups group2,
            Guid expectedMemberId,
            int expectedVersion,
            int expectedGroupSize)
        {

            ISubsetClusterMembers memberPartitionGroup = new MemberPartitionGroup(new NetworkingOptions(), NullLogger.Instance);

            for (int i = 0; i < 2; i++)
            {
                memberPartitionGroup.SetSubsetMembers(group1);
                if (group2.Version != MemberPartitionGroup.InvalidVersion)
                {
                    memberPartitionGroup.SetSubsetMembers(group2);
                }
            }

            Assert.AreEqual(expectedVersion, ((MemberPartitionGroup) memberPartitionGroup).CurrentGroups.Version);
            Assert.That(memberPartitionGroup.GetSubsetMemberIds(), Contains.Item(expectedMemberId));
            Assert.That(((MemberPartitionGroup) memberPartitionGroup).CurrentGroups.SelectedGroup.Count, Is.EqualTo(expectedGroupSize));
        }

        [TestCaseSource(nameof(MemberGroupCases))]
        public void TestMemberFilteringWorks(MemberGroups group1,
            MemberGroups group2,
            Guid expectedMemberId,
            int expectedVersion,
            int expectedGroupSize)
        {
            // Create a list of members
            var memberIds = group1.Groups.SelectMany(p => p).Distinct().ToList();
            memberIds.AddRange(group2.Groups.SelectMany(p => p).Distinct());
            memberIds = memberIds.Distinct().ToList();
            var members = memberIds.Select(p => new MemberInfo(p, new NetworkAddress("127.0.0.1"),
                new MemberVersion(5, 5, 0), false, new Dictionary<string, string>())).ToList();

            // Prepare the member partition group
            ISubsetClusterMembers memberPartitionGroup = new MemberPartitionGroup(new NetworkingOptions(), NullLogger.Instance);

            var clusterMembers = For<ClusterMembers>(For<ClusterState>(new HazelcastOptions(),
                    null, null, For<Partitioner>(), NullLoggerFactory.Instance),
                For<TerminateConnections>(NullLoggerFactory.Instance), memberPartitionGroup);

            memberPartitionGroup.SetSubsetMembers(group1);

            // Assume that MembersView is received from the server, and picking the filtered members for MultiMember mode.
            var filteredMembers = clusterMembers.FilterMembers(members).Select(p => p.Id).ToList();

            Assert.That(filteredMembers.Count, Is.EqualTo(group1.SelectedGroup.Count));
            foreach (var memberId in filteredMembers)
            {
                Assert.That(group1.SelectedGroup.Contains(memberId), Is.True,
                    $"Member {memberId} is not in the selected group [{string.Join(", ", group1.SelectedGroup.ToArray())}]");
            }

            if (group2.Version != MemberPartitionGroup.InvalidVersion)
            {
                //Assume new CP group is received from the server, then MemberView is received from the server.
                memberPartitionGroup.SetSubsetMembers(group2);
                filteredMembers = clusterMembers.FilterMembers(members).Select(p => p.Id).ToList();

                // If Group 2 is selected than assert members.
                if (((MemberPartitionGroup) memberPartitionGroup).CurrentGroups.MemberReceivedFrom == group2.MemberReceivedFrom)
                {
                    foreach (var memberId in filteredMembers)
                    {
                        Assert.That(group2.SelectedGroup.Contains(memberId), Is.True,
                            $"Member {memberId} is not in the selected group [{string.Join(", ", group2.SelectedGroup.ToArray())}]");
                    }
                }
            }
        }

        [Test]
        public async Task TestServerNotSupportMultiMemberRoutingAndClusterVersion()
        {
            var address0 = NetworkAddress.Parse("127.0.0.1:11003");
            var address1 = NetworkAddress.Parse("127.0.0.1:11004");

            var memberId0 = Guid.NewGuid();
            var memberId1 = Guid.NewGuid();
            var clusterId = Guid.NewGuid();
            var clusterVersion = new ClusterVersion(5, 3);

            var keyValues = new Dictionary<string, string>();

            var state0 = new ServerState()
            {
                MemberIds = new[] { memberId0, memberId1 },
                Addresses = new[] { address0, address1 },
                Id = 0,
                MemberId = memberId0,
                Address = address0,
                KeyValuePairs = keyValues,
                ClusterVersion = clusterVersion
            };

            var server0 = CreateServer(state0, address0, "0", clusterId);
            await server0.StartAsync().CfAwait();

            var state1 = new ServerState()
            {
                MemberIds = new[] { memberId0, memberId1 },
                Addresses = new[] { address0, address1 },
                Id = 0,
                MemberId = memberId1,
                Address = address1,
                KeyValuePairs = keyValues,
                ClusterVersion = clusterVersion
            };

            var server1 = CreateServer(state1, address1, "1", clusterId);
            await server1.StartAsync().CfAwait();

            var client = await CreateClient(RoutingModes.SingleMember, address0, address1);

            Assert.That(client.Cluster.Members.GetMembers().Count(), Is.EqualTo(2));
            Assert.That(client.Cluster.Members.SubsetClusterMembers.GetSubsetMemberIds().Count, Is.EqualTo(0));
            Assert.That(client.ClusterVersion.IsUnknown, Is.True);

        }

        [Test]
        public async Task TestClientThrowsWhenMultiMemberNotSupported()
        {
            var address0 = NetworkAddress.Parse("127.0.0.1:11005");
            var address1 = NetworkAddress.Parse("127.0.0.1:11006");

            var memberId0 = Guid.NewGuid();
            var memberId1 = Guid.NewGuid();
            var clusterId = Guid.NewGuid();
            var clusterVersion = new ClusterVersion(5, 3);

            var keyValues = new Dictionary<string, string>();

            var state0 = new ServerState()
            {
                MemberIds = new[] { memberId0, memberId1 },
                Addresses = new[] { address0, address1 },
                Id = 0,
                MemberId = memberId0,
                Address = address0,
                KeyValuePairs = keyValues,
                ClusterVersion = clusterVersion
            };

            var server0 = CreateServer(state0, address0, "0", clusterId);
            await server0.StartAsync().CfAwait();

            var state1 = new ServerState()
            {
                MemberIds = new[] { memberId0, memberId1 },
                Addresses = new[] { address0, address1 },
                Id = 0,
                MemberId = memberId1,
                Address = address1,
                KeyValuePairs = keyValues,
                ClusterVersion = clusterVersion
            };

            var server1 = CreateServer(state1, address1, "1", clusterId);
            await server1.StartAsync().CfAwait();

            Assert.ThrowsAsync<ConnectionException>(async () =>
            {
                // It must throw if routing mode is multi member and server doesn't support it.
                await CreateClient(RoutingModes.MultiMember, address0);
            });
        }

        [Test]
        public async Task TestClientHandlesClusterVersionAndMemberGroupViews()
        {
            HConsole.Configure(options => options.ConfigureDefaults(this));

            var address0 = NetworkAddress.Parse("127.0.0.1:11005");
            var address1 = NetworkAddress.Parse("127.0.0.1:11006");
            var clusterSize = 2;
            var memberId0 = Guid.NewGuid();
            var memberId1 = Guid.NewGuid();
            var clusterId = Guid.NewGuid();
            var clusterVersion = new ClusterVersion(5, 5);

            var memberCollectionJson2 = "{ \"version\":\"1\", \"groups\":[[" +
                                        $"\"{memberId0}\"," +
                                        $"\"{memberId1}\"" +
                                        "]]}";

            var keyValues = new Dictionary<string, string>
            {
                { MemberPartitionGroup.PartitionGroupRootJsonField, memberCollectionJson2 },
                { Authenticator.ClusterVersionKey, clusterVersion.ToString() }
            };

            var latchViews = new SemaphoreSlim(0);

            var state0 = new ServerState()
            {
                MemberIds = new[] { memberId0, memberId1 },
                Addresses = new[] { address0, address1 },
                Id = 0,
                MemberId = memberId0,
                Address = address0,
                KeyValuePairs = keyValues,
                ClusterVersion = clusterVersion,
                EmitViews = latchViews
            };

            var server0 = CreateServer(state0, address0, "0", clusterId);
            await server0.StartAsync().CfAwait();

            var state1 = new ServerState()
            {
                MemberIds = new[] { memberId0, memberId1 },
                Addresses = new[] { address0, address1 },
                Id = 0,
                MemberId = memberId1,
                Address = address1,
                KeyValuePairs = keyValues,
                ClusterVersion = clusterVersion,
                EmitViews = latchViews
            };

            var server1 = CreateServer(state1, address1, "1", clusterId);
            await server1.StartAsync().CfAwait();

            await using var client = await CreateClient(RoutingModes.MultiMember, address0);

            await AssertEx.SucceedsEventually(() =>
            {
                Assert.That(client.Cluster.Members.GetMembers().Count(), Is.EqualTo(2), "Members count");
                Assert.That(client.Cluster.Connections.Count, Is.EqualTo(2), "Connections count");
                var ids = client.Members.Select(m => m.Member.Id).ToList();
                Assert.That(ids, Contains.Item(memberId0));
                Assert.That(ids, Contains.Item(memberId1));
            }, 10_000, 200);


            Assert.That(((MemberPartitionGroup) client.Cluster.Members.SubsetClusterMembers).CurrentGroups.Version, Is.EqualTo(1));
            Assert.That(client.Cluster.Members.SubsetClusterMembers.GetSubsetMemberIds().Count, Is.EqualTo(2));
            Assert.That(client.ClusterVersion, Is.EqualTo(clusterVersion));


            var newClusterVersions = new ClusterVersion(5, 6);
            state0.ClusterVersion = newClusterVersions;
            state1.ClusterVersion = newClusterVersions;

            // Send member partition group view with one member.
            // Client will eventually connect to one member and will remove the filtered one.
            latchViews.Release(2);

            await AssertEx.SucceedsEventually(() =>
            {
                // Waiting for emitted views.
                Assert.That(client.State == ClientState.Connected);

                Assert.That(((MemberPartitionGroup) client.Cluster.Members.SubsetClusterMembers).CurrentGroups.Version, Is.EqualTo(2), "Partition Group Version");
                Assert.That(client.Cluster.Members.SubsetClusterMembers.GetSubsetMemberIds().Count, Is.EqualTo(1), "Subset Members count");
                Assert.That(client.Cluster.Members.SubsetClusterMembers.GetSubsetMemberIds(), Contains.Item(memberId0));
                Assert.That(client.ClusterVersion, Is.EqualTo(newClusterVersions));
                Assert.That(client.Cluster.Members.GetMembers().Count(), Is.EqualTo(clusterSize), "Members count");

            }, 20_000, 200);
        }

        // Mock server over real TPC connection
        private static async Task<HazelcastClient> CreateClient(RoutingModes routingMode = RoutingModes.MultiMember, params NetworkAddress[] addresses)
        {
            var options = new HazelcastOptionsBuilder()
                .WithHConsoleLogger()
                .With(options =>
                {
                    foreach (var address in addresses)
                    {
                        options.Networking.Addresses.Add(address.ToString());
                    }

                    options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 30_000;
                    options.Networking.RoutingMode.Mode = routingMode;
                    options.Networking.ReconnectMode = ReconnectMode.ReconnectSync;
                }).Build();
            var client = (HazelcastClient) await HazelcastClientFactory.StartNewClientAsync(options);
            return client;
        }
        private Server<ServerState> CreateServer(ServerState state, NetworkAddress address, string name, Guid clusterId = default)
        {
            var loggerFactory = new NullLoggerFactory();
            var server0 = new Server(address, loggerFactory, name)
                .WithMemberId(state.MemberId)
                .WithClusterId(clusterId)
                .WithState(state)
                .HandleFallback(ServerHandler);
            _servers.Add(server0);
            return server0;
        }

        private class ServerState
        {
            public Guid[] MemberIds { get; set; }
            public NetworkAddress[] Addresses { get; set; }
            public int Id { get; set; }
            public Guid MemberId { get; set; }
            public NetworkAddress Address { get; set; }
            public IDictionary<string, string> KeyValuePairs { get; set; }
            public ClusterVersion ClusterVersion { get; set; }
            public SemaphoreSlim EmitViews { get; set; }
        }

        private async ValueTask ServerHandler(ClientRequest<ServerState> request)
        {
            const int partitionsCount = 2;

            switch (request.Message.MessageType)
            {
                // must handle auth
                case ClientAuthenticationServerCodec.RequestMessageType:
                {
                    HConsole.WriteLine(this, $"(server{request.State.Id}) Authentication");
                    var authRequest = ClientAuthenticationServerCodec.DecodeRequest(request.Message);

                    if (request.State.ClusterVersion.Major <= 5 && request.State.ClusterVersion.Minor < 5)
                    {
                        var authResponse = ClientAuthenticationServerCodec.EncodeResponse(
                            0, request.State.Address, request.State.MemberId, SerializationService.SerializerVersion,
                            request.State.ClusterVersion.ToString(), partitionsCount, request.Server.ClusterId, false,
                            Array.Empty<int>(), Array.Empty<byte>(), 0, Enumerable.Empty<MemberInfo>().ToList(),
                            0, new List<KeyValuePair<Guid, IList<int>>>(), new Dictionary<string, string>());
                        await request.RespondAsync(authResponse).CfAwait();
                    }
                    else
                    {
                        var authResponse = ClientAuthenticationServerCodec.EncodeResponse(
                            0, request.State.Address, request.State.MemberId, SerializationService.SerializerVersion,
                            request.State.ClusterVersion.ToString(), partitionsCount, request.Server.ClusterId, false,
                            Array.Empty<int>(), Array.Empty<byte>(), 0, Enumerable.Empty<MemberInfo>().ToList(),
                            0, new List<KeyValuePair<Guid, IList<int>>>(), request.State.KeyValuePairs);
                        await request.RespondAsync(authResponse).CfAwait();
                    }
                    break;
                }

                // must handle events
                case ClientAddClusterViewListenerServerCodec.RequestMessageType:
                {
                    HConsole.WriteLine(this, $"(server{request.State.Id}) AddClusterViewListener");
                    var addRequest = ClientAddClusterViewListenerServerCodec.DecodeRequest(request.Message);
                    var addResponse = ClientAddClusterViewListenerServerCodec.EncodeResponse();

                    await request.RespondAsync(addResponse).CfAwait();

                    _ = Task.Run(async () =>
                    {

                        await Task.Delay(500).CfAwait();

                        const int membersVersion = 1;
                        var memberVersion = new MemberVersion(5, 5, 0);
                        var memberAttributes = new Dictionary<string, string>();
                        var membersEventMessage = ClientAddClusterViewListenerServerCodec.EncodeMembersViewEvent(membersVersion, new[]
                        {
                            new MemberInfo(request.State.MemberIds[0], request.State.Addresses[0], memberVersion, false, memberAttributes),
                            new MemberInfo(request.State.MemberIds[1], request.State.Addresses[1], memberVersion, false, memberAttributes),
                        });
                        await request.RaiseAsync(membersEventMessage).CfAwait();

                        await Task.Delay(500).CfAwait();

                        const int partitionsVersion = 1;
                        var partitionsEventMessage = ClientAddClusterViewListenerServerCodec.EncodePartitionsViewEvent(partitionsVersion, new[]
                        {
                            new KeyValuePair<Guid, IList<int>>(request.State.MemberIds[0], new List<int> { 0 }),
                            new KeyValuePair<Guid, IList<int>>(request.State.MemberIds[1], new List<int> { 1 }),
                        });
                        await request.RaiseAsync(partitionsEventMessage).CfAwait();

                        if (request.State.ClusterVersion.Major == 5 && request.State.ClusterVersion.Minor < 5)
                            return;

                        _ = Task.Run(async () =>
                        {
                            // Wait for signal to emit views
                            await request.State.EmitViews.WaitAsync().CfAwait();

                            var clusterVersionMessage = ClientAddClusterViewListenerServerCodec.EncodeClusterVersionEvent(request.State.ClusterVersion);
                            await request.RaiseAsync(clusterVersionMessage).CfAwait();

                            await Task.Delay(500).CfAwait();

                            var memberGroupList = new List<ICollection<Guid>>
                            {
                                new List<Guid>() { request.State.MemberIds.First() }
                            };
                            var memberGroupListMessage = ClientAddClusterViewListenerServerCodec.EncodeMemberGroupsViewEvent(2, memberGroupList);

                            await request.RaiseAsync(memberGroupListMessage).CfAwait();
                        });
                    });

                    break;
                }
                case ClientPingServerCodec.RequestMessageType:
                {
                    HConsole.WriteLine(this, $"(server{request.State.Id}) Ping");
                    var pingRequest = ClientPingServerCodec.DecodeRequest(request.Message);
                    var pingResponse = ClientPingServerCodec.EncodeResponse();
                    await request.RespondAsync(pingResponse).CfAwait();
                    break;
                }


                // unexpected message = error
                default:
                {
                    // RemoteError.Hazelcast or RemoteError.RetryableHazelcast
                    var messageName = MessageTypeConstants.GetMessageTypeName(request.Message.MessageType);
                    await request.ErrorAsync(RemoteError.Hazelcast, $"MessageType {messageName} (0x{request.Message.MessageType:X}) not implemented.").CfAwait();
                    break;
                }
            }
        }

        public static object[] MemberGroupCases =
        {
            // Case 1: New version wins.
            new object[]
            {
                // Group 1
                new MemberGroups(new List<IList<Guid>>()
                    {
                        // Selected Group
                        new List<Guid>() { Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e"), Guid.Parse("bfc5a01b-884f-421b-b8a0-a7ce643b4085") },
                        new List<Guid>() { Guid.Parse("efdb670f-d0d5-4482-84eb-0354e4278112"), Guid.Parse("6965aaa2-d6eb-483a-bb6c-99388c348bc6"), Guid.Parse("07988585-f4cf-4473-8299-5575fa6bc93a") },
                        new List<Guid>() { Guid.Parse("a722ab7a-d266-4911-91b3-5f9f9c7dbcab"), Guid.Parse("b6c07420-3971-4982-bdd4-4ad52b355b7a"), Guid.Parse("e29f6919-7d8a-4f44-91fe-0b48376993a2") }
                    },
                    1,
                    Guid.Parse("81b1ac67-1238-42d6-84b7-ef869e60f262"),
                    Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e")),

                // Group 2
                new MemberGroups(new List<IList<Guid>>()
                    {
                        new List<Guid>() { Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e"), Guid.Parse("d3c34048-a055-4025-8c00-c70b2dcd47b9") },
                        new List<Guid>() { Guid.Parse("dd8803e6-16ae-4d62-921e-ffe4f5c8ce49"), Guid.Parse("6965aaa2-d6eb-483a-bb6c-99388c348bc6") }
                    },
                    2,
                    Guid.Parse("81b1ac67-1238-42d6-84b7-ef869e60f262"),
                    Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e")),

                Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e"), // Expected group member
                2, // Expected version
                2 // Expected group size
            },

            // Case 2: Bigger overlap wins.
            new object[]
            {
                // Group 1
                new MemberGroups(new List<IList<Guid>>()
                    {
                        new List<Guid>() { Guid.Parse("efdb670f-d0d5-4482-84eb-0354e4278112"), Guid.Parse("6965aaa2-d6eb-483a-bb6c-99388c348bc6") },
                        new List<Guid>()
                        {
                            Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e"), Guid.Parse("d3c34048-a055-4025-8c00-c70b2dcd47b9")
                        }
                    },
                    1,
                    Guid.Parse("81b1ac67-1238-42d6-84b7-ef869e60f262"),
                    Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e")),

                // Group 2
                new MemberGroups(new List<IList<Guid>>()
                    {
                        // Selected Group
                        new List<Guid>()
                        {
                            Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e"), Guid.Parse("d3c34048-a055-4025-8c00-c70b2dcd47b9"),
                            Guid.Parse("03d9a78b-6380-49c8-9bf3-8044f2cfa75d")
                        },
                        new List<Guid>() { Guid.Parse("dd8803e6-16ae-4d62-921e-ffe4f5c8ce49"), Guid.Parse("6965aaa2-d6eb-483a-bb6c-99388c348bc6") }
                    },
                    1,
                    Guid.Parse("81b1ac67-1238-42d6-84b7-ef869e60f262"),
                    Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e")),

                Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e"), // Expected group member
                1, // Expected version
                3 // Expected group size
            },

            // Case 2: New cluster wins
            new object[]
            {
                // Group 1
                new MemberGroups(new List<IList<Guid>>()
                    {
                        new List<Guid>()
                        {
                            Guid.Parse("efdb670f-d0d5-4482-84eb-0354e4278112"),
                            Guid.Parse("6965aaa2-d6eb-483a-bb6c-99388c348bc6")
                        },
                        new List<Guid>()
                        {
                            Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e"),
                            Guid.Parse("d3c34048-a055-4025-8c00-c70b2dcd47b9")
                        }
                    },
                    1,
                    Guid.Parse("81b1ac67-1238-42d6-84b7-ef869e60f262"),
                    Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e")),

                // Group 2
                new MemberGroups(new List<IList<Guid>>()
                    {
                        // Selected Group
                        new List<Guid>()
                        {
                            Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e"),
                            Guid.Parse("d3c34048-a055-4025-8c00-c70b2dcd47b9"),
                            Guid.Parse("03d9a78b-6380-49c8-9bf3-8044f2cfa75d")
                        },
                        new List<Guid>()
                        {
                            Guid.Parse("dd8803e6-16ae-4d62-921e-ffe4f5c8ce49"),
                            Guid.Parse("6965aaa2-d6eb-483a-bb6c-99388c348bc6")
                        }
                    },
                    1,
                    Guid.Parse("a810df4f-a54c-437d-a945-99218688cf31"),
                    Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e")),

                Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e"), // Expected group member
                1, // Expected version
                3 // Expected group size
            },

            // Case 3: Biggest wins
            new object[]
            {
                // Group 1
                new MemberGroups(new List<IList<Guid>>()
                    {
                        new List<Guid>()
                        {
                            Guid.Parse("efdb670f-d0d5-4482-84eb-0354e4278112"),
                            Guid.Parse("6965aaa2-d6eb-483a-bb6c-99388c348bc6")
                        },
                        new List<Guid>()
                        {
                            Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e"),
                            Guid.Parse("d3c34048-a055-4025-8c00-c70b2dcd47b9")
                        }
                    },
                    1,
                    Guid.Parse("81b1ac67-1238-42d6-84b7-ef869e60f262"),
                    Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e")),

                // Group 2
                new MemberGroups(new List<IList<Guid>>()
                    {
                        // Selected Group
                        new List<Guid>()
                        {
                            Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e"),
                            Guid.Parse("d3c34048-a055-4025-8c00-c70b2dcd47b9"),
                            Guid.Parse("03d9a78b-6380-49c8-9bf3-8044f2cfa75d")
                        },
                        new List<Guid>()
                        {
                            Guid.Parse("dd8803e6-16ae-4d62-921e-ffe4f5c8ce49"),
                            Guid.Parse("6965aaa2-d6eb-483a-bb6c-99388c348bc6")
                        }
                    },
                    1,
                    Guid.Parse("a810df4f-a54c-437d-a945-99218688cf31"),
                    Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e")),

                Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e"), // Expected group member
                1, // Expected version
                3 // Expected group size
            },

            // Case 4: PG Group not changing after auth.
            new object[]
            {
                // Group 1
                new MemberGroups(new List<IList<Guid>>()
                    {
                        new List<Guid>()
                        {
                            Guid.Parse("efdb670f-d0d5-4482-84eb-0354e4278112"),
                            Guid.Parse("6965aaa2-d6eb-483a-bb6c-99388c348bc6"),
                            Guid.Parse("d8ee9c15-ac9a-4357-9698-ade761ced554")
                        },
                        // Selected Group
                        new List<Guid>()
                        {
                            Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e"),
                            Guid.Parse("d3c34048-a055-4025-8c00-c70b2dcd47b9")
                        }
                    },
                    1,
                    Guid.Parse("81b1ac67-1238-42d6-84b7-ef869e60f262"),
                    Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e")),

                // Group 2
                new MemberGroups(new List<IList<Guid>>()
                    {
                        new List<Guid>()
                        {
                            Guid.Parse("efdb670f-d0d5-4482-84eb-0354e4278112"),
                            Guid.Parse("6965aaa2-d6eb-483a-bb6c-99388c348bc6"),
                            Guid.Parse("d8ee9c15-ac9a-4357-9698-ade761ced554"),
                            Guid.Parse("79d63bcf-339d-449b-aa55-a2cb4f3bad8b")
                        },
                        new List<Guid>()
                        {
                            Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e"),
                            Guid.Parse("d3c34048-a055-4025-8c00-c70b2dcd47b9")
                        }
                    },
                    2,
                    Guid.Parse("81b1ac67-1238-42d6-84b7-ef869e60f262"),
                    Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e")),
                Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e"), // Expected group member
                2, // Expected version
                2 // Expected group size
            },

            // Case 5: Bigger overlap with auth group wins.
            new object[]
            {
                // Group 1
                new MemberGroups(new List<IList<Guid>>()
                    {
                        new List<Guid>()
                        {
                            Guid.Parse("efdb670f-d0d5-4482-84eb-0354e4278112"),
                            Guid.Parse("6965aaa2-d6eb-483a-bb6c-99388c348bc6"),
                            Guid.Parse("d8ee9c15-ac9a-4357-9698-ade761ced554")
                        },
                        // Selected Group
                        new List<Guid>()
                        {
                            Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e"),
                            Guid.Parse("d3c34048-a055-4025-8c00-c70b2dcd47b9")
                        }
                    },
                    1,
                    Guid.Parse("81b1ac67-1238-42d6-84b7-ef869e60f262"),
                    Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e")),

                // Group 2
                new MemberGroups(new List<IList<Guid>>()
                    {
                        new List<Guid>()
                        {
                            Guid.Parse("efdb670f-d0d5-4482-84eb-0354e4278112"),
                            Guid.Parse("6965aaa2-d6eb-483a-bb6c-99388c348bc6"),
                            Guid.Parse("d8ee9c15-ac9a-4357-9698-ade761ced554"),
                            Guid.Parse("79d63bcf-339d-449b-aa55-a2cb4f3bad8b")
                        },
                        new List<Guid>()
                        {
                            Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e"),
                            Guid.Parse("d3c34048-a055-4025-8c00-c70b2dcd47b9"),
                            Guid.Parse("6878f2be-5153-4b7a-8896-edab76011c9c"),
                            Guid.Parse("7bff264d-426d-44e3-928e-a6200a0a5271"),
                            Guid.Parse("ce74ca59-3061-47c4-b56b-ddf5727fa312")
                        }
                    },
                    2,
                    Guid.Parse("81b1ac67-1238-42d6-84b7-ef869e60f262"),
                    Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e")),
                Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e"), // Expected group member
                2, // Expected version
                5 // Expected group size
            },

            // Case 5: Scale down
            new object[]
            {
                // Group 1
                new MemberGroups(new List<IList<Guid>>()
                    {
                        new List<Guid>()
                        {
                            Guid.Parse("efdb670f-d0d5-4482-84eb-0354e4278112"),
                            Guid.Parse("6965aaa2-d6eb-483a-bb6c-99388c348bc6"),
                            Guid.Parse("d8ee9c15-ac9a-4357-9698-ade761ced554"),
                            Guid.Parse("79d63bcf-339d-449b-aa55-a2cb4f3bad8b")
                        },
                        // Selected Group
                        new List<Guid>()
                        {
                            Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e"),
                            Guid.Parse("d3c34048-a055-4025-8c00-c70b2dcd47b9"),
                            Guid.Parse("6878f2be-5153-4b7a-8896-edab76011c9c"),
                            Guid.Parse("7bff264d-426d-44e3-928e-a6200a0a5271"),
                            Guid.Parse("ce74ca59-3061-47c4-b56b-ddf5727fa312")
                        }
                    },
                    1,
                    Guid.Parse("81b1ac67-1238-42d6-84b7-ef869e60f262"),
                    Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e")),

                // Group 2
                new MemberGroups(new List<IList<Guid>>()
                    {
                        new List<Guid>()
                        {
                            Guid.Parse("efdb670f-d0d5-4482-84eb-0354e4278112"),
                            Guid.Parse("6965aaa2-d6eb-483a-bb6c-99388c348bc6"),
                            Guid.Parse("d8ee9c15-ac9a-4357-9698-ade761ced554"),
                            Guid.Parse("79d63bcf-339d-449b-aa55-a2cb4f3bad8b")
                        },
                        new List<Guid>()
                        {
                            Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e")
                        }
                    },
                    2,
                    Guid.Parse("81b1ac67-1238-42d6-84b7-ef869e60f262"),
                    Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e")),
                Guid.Parse("64082773-bc1b-408c-8ea6-1150c3c6477e"), // Expected group member
                2, // Expected version
                1 // Expected group size
            },

        };
    }
}
