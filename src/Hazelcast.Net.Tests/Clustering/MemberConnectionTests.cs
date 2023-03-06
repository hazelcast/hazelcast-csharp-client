// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Testing.Networking;
using Hazelcast.Testing.TestServer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
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

            var address = NetworkAddress.Parse($"127.0.0.1:{TestEndPointPort.GetNext()}");

            var state = new ServerState
            {
                Id = 0,
                MemberId = Guid.NewGuid(),
                Address = address
            };

            await using var server = new Server(address, loggerFactory, "0")
                .WithMemberId(state.MemberId)
                .WithState(state)
                .HandleFallback(ServerHandler);
            await server.StartAsync();

            var messaging = Mock.Of<IClusterMessaging>();
            var serializationService = HazelcastClientFactory.CreateSerializationService(options.Serialization, messaging, loggerFactory);
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
        public async Task TestUnchangedMembersListDoesNotLog()
        {
            var options = new HazelcastOptionsBuilder()
                .With(o => o.Networking.SmartRouting = false)
                .With(o => o.Networking.UsePublicAddresses = false)
                .Build();

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(h => h.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var clusterState = new ClusterState(options, clusterName: "dev", clientName: "client", new Partitioner(), loggerFactoryMock.Object);
            var clusterMembers = new ClusterMembers(clusterState, new TerminateConnections(loggerFactoryMock.Object));

            var memberList = new List<MemberInfo> { NewMemberInfo(true), NewMemberInfo(true) };

            await clusterMembers.SetMembersAsync(1, memberList);//will print since list new -> 1
            await clusterMembers.SetMembersAsync(2, memberList);//won't print since list steady

            memberList.Add(NewMemberInfo(true));
            await clusterMembers.SetMembersAsync(3, memberList);//will print since list new -> 2
            await clusterMembers.SetMembersAsync(3, memberList);//won't print since list steady
            
            loggerMock.Verify(m =>
                    m.Log<It.IsAnyType>(LogLevel.Information,
                        0,
                        It.IsAny<It.IsAnyType>(),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Exactly(2),
          "Member list prints too verbose.");
        }

        [Test]
        public void GetMemberForSql()
        {
            using var _ = HConsoleForTest();

            var options = new HazelcastOptionsBuilder()
                .With(o => o.Networking.SmartRouting = false)
                .With(o => o.Networking.UsePublicAddresses = false)
                .Build();

            var loggerFactory = NullLoggerFactory.Instance;

            var clusterState = new ClusterState(options, clusterName: "dev", clientName: "client", new Partitioner(), loggerFactory);
            var clusterMembers = new ClusterMembers(clusterState, new TerminateConnections(loggerFactory));

            foreach (var members in new[]
            {
                new[] { NewMemberInfo(true), NewMemberInfo(true), NewMemberInfo(true) },
                new[] { NewMemberInfo(false), NewMemberInfo(false), NewMemberInfo(true) },
                new[] { NewMemberInfo(false), NewMemberInfo(false), NewMemberInfo(false) }
            })
            {
                var allMembersLite = members.All(m => m.IsLiteMember);

                var membersTable = new MemberTable(1, members);
                clusterMembers.Accessor().Members = membersTable;

                if (allMembersLite)
                {
                    Assert.AreEqual(clusterMembers.GetMemberForSql(), null);
                    Assert.AreEqual(clusterMembers.GetMemberForSql(), null);
                }
                else
                {
                    Assert.AreEqual(clusterMembers.GetMemberForSql().IsLiteMember, false);
                    Assert.AreEqual(clusterMembers.GetMemberForSql()?.IsLiteMember, false);
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
            var messaging = Mock.Of<IClusterMessaging>();
            var serializationService = HazelcastClientFactory.CreateSerializationService(options.Serialization, messaging, loggerFactory);
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

        [Test]
        public async Task FailCurentInvocationWhenDisposed()
        {
            var loggerFactory = new NullLoggerFactory();
            var address = NetworkAddress.Parse($"127.0.0.1:{TestEndPointPort.GetNext()}");
            var state = new ServerState
            {
                Id = 0,
                MemberId = Guid.NewGuid(),
                Address = address
            };
            await using var server = new Server(address, loggerFactory, "0")
                .WithMemberId(state.MemberId)
                .WithState(state)
                .HandleFallback(ServerHandler);
            await server.StartAsync();

            var options = new HazelcastOptionsBuilder().Build();
            var messaging = Mock.Of<IClusterMessaging>();
            var serializationService = HazelcastClientFactory.CreateSerializationService(options.Serialization, messaging, loggerFactory);
            var authenticator = new Authenticator(options.Authentication, serializationService, loggerFactory);

            ISequence<long> correlationIdSequence = new Int64Sequence();

            var memberConnection = new MemberConnection(address, authenticator,
                options.Messaging, options.Networking, options.Networking.Ssl,
                correlationIdSequence,
                loggerFactory);

            const string clusterName = "dev";
            const string clientName = "client";
            var clusterState = new ClusterState(options,
                clusterName, clientName,
                new Partitioner(), loggerFactory);

            await memberConnection.ConnectAsync(clusterState, CancellationToken.None);

            // prepare and send a message
            var message = ClientPingServerCodec.EncodeRequest();
            message.CorrelationId = correlationIdSequence.GetNext();
            message.Flags |= ClientMessageFlags.BeginFragment | ClientMessageFlags.EndFragment;
            var invocation = new Invocation(message, options.Messaging);
            var invoking = memberConnection.SendAsync(invocation);

            // the server does *not* respond to pings, so the invocation is pending
            // disposing the member connection should terminate the invocation
            await memberConnection.DisposeAsync();

            // bam
            await AssertEx.ThrowsAsync<TargetDisconnectedException>(async () => await invoking);
        }

        internal class ServerState
        {
            public int Id { get; set; }
            public Guid MemberId { get; set; }
            public NetworkAddress Address { get; set; }
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
                        var authResponse = ClientAuthenticationServerCodec.EncodeResponse(
                            0, request.State.Address, request.State.MemberId, SerializationService.SerializerVersion,
                            "4.0", partitionsCount, request.Server.ClusterId, false);
                        await request.RespondAsync(authResponse).CfAwait();
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
                            var memberVersion = new MemberVersion(4, 0, 0);
                            var memberAttributes = new Dictionary<string, string>();
                            var membersEventMessage = ClientAddClusterViewListenerServerCodec.EncodeMembersViewEvent(membersVersion, new[]
                            {
                                new MemberInfo(request.State.MemberId, request.State.Address, memberVersion, false, memberAttributes)
                            });
                            await request.RaiseAsync(membersEventMessage).CfAwait();

                            await Task.Delay(500).CfAwait();

                            const int partitionsVersion = 1;
                            var partitionsEventMessage = ClientAddClusterViewListenerServerCodec.EncodePartitionsViewEvent(partitionsVersion, new[]
                            {
                                new KeyValuePair<Guid, IList<int>>(request.State.MemberId, new List<int> { 0 }),
                                new KeyValuePair<Guid, IList<int>>(request.State.MemberId, new List<int> { 1 }),
                            });
                            await request.RaiseAsync(partitionsEventMessage).CfAwait();
                        });

                        break;
                    }

                // create object
                case ClientPingServerCodec.RequestMessageType:
                    {
                        HConsole.WriteLine(this, $"(server{request.State.Id}) Ping");
                        var pingRequest = ClientPingServerCodec.DecodeRequest(request.Message);

                        // no response, will timeout

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

        private MemberInfo NewMemberInfo(bool isDataMember) => new MemberInfo(id: Guid.NewGuid(),
            NetworkAddress.Parse("localhost"), new MemberVersion(1, 0, 0),
            isLiteMember: !isDataMember, attributes: new Dictionary<string, string>());

        private MemberConnection NewActiveMemberConnection(MemberInfo member, Authenticator authenticator, ILoggerFactory loggerFactory)
        {
            var connection = new MemberConnection(member.Address,
                authenticator, new MessagingOptions(), new NetworkingOptions(),
                new SslOptions(), new Int64Sequence(), loggerFactory
            );

            connection.Accessor().Connected = true;
            connection.Accessor().MemberId = member.Id;

            return connection;
        }
    }
}
