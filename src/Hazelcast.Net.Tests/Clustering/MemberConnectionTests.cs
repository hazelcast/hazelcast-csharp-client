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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Messaging;
using Hazelcast.Models;
using Hazelcast.Networking;
using Hazelcast.Partitioning;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Protocol.Models;
using Hazelcast.Serialization;
using Hazelcast.Testing;
using Hazelcast.Testing.Accessors;
using Hazelcast.Testing.Protocol;
using Hazelcast.Testing.TestServer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Hazelcast.Tests.Clustering
{
    [TestFixture]
    public class MemberConnectionTests
    {
        private IDisposable HConsoleForTest()

            => HConsole.Capture(options => options
                .ClearAll()
                .Configure().SetMaxLevel()
                .Configure(this).SetPrefix("TEST")
                .Configure<AsyncContext>().SetMinLevel()
                .Configure<SocketConnectionBase>().SetIndent(1).SetLevel(0).SetPrefix("SOCKET"));

        [Test]
        public async Task Test()
        {
            using var _ = HConsoleForTest();

            var options = new HazelcastOptionsBuilder().Build();
            options.Messaging.RetryTimeoutSeconds = 1;

            var loggerFactory = new NullLoggerFactory();

            var address = NetworkAddress.Parse("127.0.0.1:11000");

            var state = new ServerState
            {
                Id = 0,
                MemberId = Guid.NewGuid(),
                Address = address
            };

            await using var server = new Server(address, ServerHandler, loggerFactory, state, "0")
            {
                MemberId = state.MemberId,
            };
            await server.StartAsync();

            var serializationService = HazelcastClientFactory.CreateSerializationService(options.Serialization, loggerFactory);
            var authenticator = new Authenticator(options.Authentication, serializationService, loggerFactory);

            ISequence<long> correlationIdSequence = new Int64Sequence();

            var memberConnection = new MemberConnection(address, authenticator,
                options.Messaging, options.Networking, options.Networking.Ssl,
                correlationIdSequence,
                loggerFactory);

            var memberConnectionHasClosed = false;
            memberConnection.Closed += connection =>
            {
                memberConnectionHasClosed = true;
                return default;
            };

            var clusterName = "dev";
            var clientName = "client";
            var clusterState = new ClusterState(options,
                clusterName, clientName,
                new Partitioner(), loggerFactory);

            await memberConnection.ConnectAsync(clusterState, CancellationToken.None);

            // so far, so good
            // now, try something that will timeout
            //
            //
            // send async without a timeout uses the timeout in messagingOptions.RetryTimeoutSeconds
            // in order to determine whether to retry again and again

            var message = ClientPingServerCodec.EncodeRequest();

            // SendAsync prepares the message
            message.CorrelationId = correlationIdSequence.GetNext();
            message.Flags |= ClientMessageFlags.BeginFragment | ClientMessageFlags.EndFragment;

            // SendAsync then creates the invocation
            var invocation = new Invocation(message, options.Messaging);

            // don't send: server does not answer to pings, and we would wait forever

            //await AssertEx.ThrowsAsync<TaskTimeoutException>(async () =>
            //    await memberConnection.SendAsync(invocation));
            //
            //Assert.That(invocation.Task.IsCompleted);

            // OTOH we can try to retry...
            await AssertEx.SucceedsEventually(async () =>
            {
                await AssertEx.ThrowsAsync<TaskTimeoutException>(async () =>
                {
                    // ReSharper disable once MethodSupportsCancellation
                    await invocation.WaitRetryAsync(correlationIdSequence.GetNext);
                });
            }, 4000, 200);

            await memberConnection.DisposeAsync();

            Assert.That(memberConnection.Active, Is.False);
            Assert.That(memberConnectionHasClosed);
        }

        [Test]
        public void FindMemberOfLargerSameVersionGroup()
        {
            using var _ = HConsoleForTest();

            foreach (var members in new[]
            {
                new[] { NewMemberInfo(true), NewMemberInfo(true), NewMemberInfo(true) },
                new[] { NewMemberInfo(false), NewMemberInfo(false), NewMemberInfo(true) },
                new[] { NewMemberInfo(false), NewMemberInfo(false), NewMemberInfo(false) }
            })
            {
                var allMembersLite = members.All(m => m.IsLiteMember);

                var membersTable = new MemberTable(1, members);

                if (allMembersLite)
                {
                    Assert.AreEqual(membersTable.FindMemberOfLargerSameVersionGroup(), null);
                    Assert.AreEqual(membersTable.FindMemberOfLargerSameVersionGroup(), null);
                }
                else
                {
                    Assert.AreEqual(membersTable.FindMemberOfLargerSameVersionGroup().IsLiteMember, false);
                    Assert.AreEqual(membersTable.FindMemberOfLargerSameVersionGroup()?.IsLiteMember, false);
                }
            }
        }

        [Test]
        public async Task GetConnectionForSql_NoSmartRouting()
        {
            using var _ = HConsoleForTest();

            var options = new HazelcastOptionsBuilder()
                .With(o => o.Networking.SmartRouting = false)
                .With(o => o.Networking.UsePublicAddresses = false)
                .Build();

            var loggerFactory = NullLoggerFactory.Instance;
            var serializationService = HazelcastClientFactory.CreateSerializationService(options.Serialization, loggerFactory);
            var authenticator = new Authenticator(options.Authentication, serializationService, loggerFactory);

            var clusterState = new ClusterState(options, clusterName: "dev", clientName: "client", new Partitioner(), loggerFactory);
            var clusterMembers = new ClusterMembers(clusterState, new TerminateConnections(loggerFactory));

            foreach (var members in new[]
            {
                new[] { NewMemberInfo(true), NewMemberInfo(true), NewMemberInfo(true) },
                new[] { NewMemberInfo(false), NewMemberInfo(false), NewMemberInfo(true) },
                new[] { NewMemberInfo(false), NewMemberInfo(false), NewMemberInfo(false) }
            })
            {
                var membersById = members.ToDictionary(m => m.Id);
                var allMembersLite = members.All(m => m.IsLiteMember);

                await clusterMembers.SetMembersAsync(version: 1, members);
                clusterMembers.Accessor().Connections = members.ToDictionary(
                    m => m.Id, m => NewActiveMemberConnection(m, authenticator, loggerFactory)
                );

                Assert.AreEqual(membersById[clusterMembers.GetConnectionForSql().MemberId].IsLiteMember, allMembersLite);
                Assert.AreEqual(membersById[clusterMembers.GetConnectionForSql().MemberId].IsLiteMember, allMembersLite);
            }
        }

        internal class ServerState
        {
            public int Id { get; set; }
            public Guid MemberId { get; set; }
            public NetworkAddress Address { get; set; }
        }

        private async ValueTask ServerHandler(Server s, ClientMessageConnection conn, ClientMessage msg)
        {
            async Task SendResponseAsync(ClientMessage response)
            {
                response.CorrelationId = msg.CorrelationId;
                response.Flags |= ClientMessageFlags.BeginFragment | ClientMessageFlags.EndFragment;
                await conn.SendAsync(response).CfAwait();
            }

            async Task SendEventAsync(ClientMessage eventMessage, long correlationId)
            {
                eventMessage.CorrelationId = correlationId;
                eventMessage.Flags |= ClientMessageFlags.BeginFragment | ClientMessageFlags.EndFragment;
                await conn.SendAsync(eventMessage).CfAwait();
            }

            async Task SendErrorAsync(RemoteError error, string message)
            {
                var errorHolders = new List<ErrorHolder>
                    {
                        new ErrorHolder(error, "?", message, Enumerable.Empty<StackTraceElement>())
                    };
                var response = ErrorsServerCodec.EncodeResponse(errorHolders);
                await SendResponseAsync(response).CfAwait();
            }

            var state = (ServerState) s.State;
            var address = s.Address;

            const int partitionsCount = 2;

            switch (msg.MessageType)
            {
                // must handle auth
                case ClientAuthenticationServerCodec.RequestMessageType:
                    {
                        HConsole.WriteLine(this, $"(server{state.Id}) Authentication");
                        var authRequest = ClientAuthenticationServerCodec.DecodeRequest(msg);
                        var authResponse = ClientAuthenticationServerCodec.EncodeResponse(
                            0, address, s.MemberId, SerializationService.SerializerVersion,
                            "4.0", partitionsCount, s.ClusterId, false);
                        await SendResponseAsync(authResponse).CfAwait();
                        break;
                    }

                // must handle events
                case ClientAddClusterViewListenerServerCodec.RequestMessageType:
                    {
                        HConsole.WriteLine(this, $"(server{state.Id}) AddClusterViewListener");
                        var addRequest = ClientAddClusterViewListenerServerCodec.DecodeRequest(msg);
                        var addResponse = ClientAddClusterViewListenerServerCodec.EncodeResponse();
                        await SendResponseAsync(addResponse).CfAwait();

                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(500).CfAwait();

                            const int membersVersion = 1;
                            var memberVersion = new MemberVersion(4, 0, 0);
                            var memberAttributes = new Dictionary<string, string>();
                            var membersEventMessage = ClientAddClusterViewListenerServerCodec.EncodeMembersViewEvent(membersVersion, new[]
                            {
                                new MemberInfo(state.MemberId, state.Address, memberVersion, false, memberAttributes)
                            });
                            await SendEventAsync(membersEventMessage, msg.CorrelationId).CfAwait();

                            await Task.Delay(500).CfAwait();

                            const int partitionsVersion = 1;
                            var partitionsEventMessage = ClientAddClusterViewListenerServerCodec.EncodePartitionsViewEvent(partitionsVersion, new[]
                            {
                                new KeyValuePair<Guid, IList<int>>(state.MemberId, new List<int> { 0 }),
                                new KeyValuePair<Guid, IList<int>>(state.MemberId, new List<int> { 1 }),
                            });
                            await SendEventAsync(partitionsEventMessage, msg.CorrelationId).CfAwait();
                        });

                        break;
                    }

                // create object
                case ClientPingServerCodec.RequestMessageType:
                    {
                        HConsole.WriteLine(this, $"(server{state.Id}) Ping");
                        var pingRequest = ClientPingServerCodec.DecodeRequest(msg);

                        // no response, will timeout

                        break;
                    }

                // unexpected message = error
                default:
                    {
                        // RemoteError.Hazelcast or RemoteError.RetryableHazelcast
                        var messageName = MessageTypeConstants.GetMessageTypeName(msg.MessageType);
                        await SendErrorAsync(RemoteError.Hazelcast, $"MessageType {messageName} (0x{msg.MessageType:X}) not implemented.").CfAwait();
                        break;
                    }
            }
        }

        private MemberInfo NewMemberInfo(bool isDataMember) => new MemberInfo(id: Guid.NewGuid(),
            NetworkAddress.Parse("localhost"), new MemberVersion(1, 0, 0),
            isLiteMember: !isDataMember, attributes: new Dictionary<string, string>());

        private MemberConnection NewActiveMemberConnection(MemberInfo member, Authenticator authenticator, ILoggerFactory loggerFactory)
        {
            var connection = new MemberConnection(member.Address,
                authenticator, new MessagingOptions(), new NetworkingOptions(),
                new SslOptions(), new Int64Sequence(), loggerFactory
            );

            connection.Accessor().Active = true;
            connection.Accessor().MemberId = member.Id;

            return connection;
        }
    }
}
