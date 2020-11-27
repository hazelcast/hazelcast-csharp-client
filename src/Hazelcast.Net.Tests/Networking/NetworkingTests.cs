// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Messaging;
using Hazelcast.Models;
using Hazelcast.Networking;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Protocol.Models;
using Hazelcast.Serialization;
using Hazelcast.Testing;
using Hazelcast.Testing.Protocol;
using Hazelcast.Testing.TestServer;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Hazelcast.Tests.Networking
{
    [TestFixture]
    public class NetworkingTests : HazelcastTestBase
    {
        private ClientMessage CreateMessage(string text)
        {
            var message = new ClientMessage()
                .Append(new Frame(new byte[64])) // header stuff
                .Append(new Frame(Encoding.UTF8.GetBytes(text)));
            return message;
        }

        private string GetText(ClientMessage message)
            => Encoding.UTF8.GetString(message.FirstFrame.Next.Bytes);

        private async Task HandleAsync(Server server, ClientMessageConnection connection, ClientMessage requestMessage,
            Func<Server, ClientMessageConnection, ClientMessage, ValueTask> handler)
        {
            var correlationId = requestMessage.CorrelationId;

            async Task SendResponseAsync(ClientMessage response)
            {
                response.CorrelationId = requestMessage.CorrelationId;
                response.Flags |= ClientMessageFlags.BeginFragment | ClientMessageFlags.EndFragment;
                await connection.SendAsync(response).CAF();
            }

            async Task SendEventAsync(ClientMessage eventMessage, long correlationId)
            {
                eventMessage.CorrelationId = correlationId;
                eventMessage.Flags |= ClientMessageFlags.BeginFragment | ClientMessageFlags.EndFragment;
                await connection.SendAsync(eventMessage).CAF();
            }

            switch (requestMessage.MessageType)
            {
                // handle authentication
                case ClientAuthenticationServerCodec.RequestMessageType:
                    {
                        var request = ClientAuthenticationServerCodec.DecodeRequest(requestMessage);
                        var responseMessage = ClientAuthenticationServerCodec.EncodeResponse(
                            0, server.Address, server.MemberId, SerializationService.SerializerVersion,
                            "4.0", 1, server.ClusterId, false);
                        await SendResponseAsync(responseMessage).CAF();
                        break;
                    }

                // handle events
                case ClientAddClusterViewListenerServerCodec.RequestMessageType:
                    {
                        var request = ClientAddClusterViewListenerServerCodec.DecodeRequest(requestMessage);
                        var responseMessage = ClientAddClusterViewListenerServerCodec.EncodeResponse();
                        await SendResponseAsync(responseMessage).CAF();

                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(500).CAF();
                            var eventMessage = ClientAddClusterViewListenerServerCodec.EncodeMembersViewEvent(1, new[]
                            {
                            new MemberInfo(server.MemberId, server.Address, new MemberVersion(4, 0, 0), false, new Dictionary<string, string>()),
                        });
                            await SendEventAsync(eventMessage, correlationId).CAF();
                        });
                        break;
                    }

                // handle others
                default:
                    await handler(server, connection, requestMessage).CAF();
                    break;
            }
        }

        private async ValueTask ReceiveMessage(Server server, ClientMessageConnection connection, ClientMessage message)
        {
            HConsole.WriteLine(this, "Respond");
            var text = Encoding.UTF8.GetString(message.FirstFrame.Bytes);
#if NETSTANDARD2_1
            var responseText = text switch
            {
                "a" => "alpha",
                "b" => "bravo",
                _ => "??"
            };
#else
            var responseText =
                text == "a" ? "alpha" :
                text == "b" ? "bravo" :
                "??";
#endif
            // this is very basic stuff and does not respect HZ protocol
            // the 64-bytes header is nonsense etc
            var response = new ClientMessage()
                .Append(new Frame(new byte[64])) // header stuff
                .Append(new Frame(Encoding.UTF8.GetBytes(responseText)));

            response.CorrelationId = message.CorrelationId;
            response.MessageType = 0x1; // 0x00 means exception

            // send in one fragment, set flags
            response.Flags |= ClientMessageFlags.BeginFragment | ClientMessageFlags.EndFragment;

            await connection.SendAsync(response).CAF();
            HConsole.WriteLine(this, "Responded");
        }

        private ClientMessage CreateErrorMessage(RemoteError error)
        {
            // can we prepare server messages?
            var errorHolders = new List<ErrorHolder>
            {
                new ErrorHolder(error, "className", "message", Enumerable.Empty<StackTraceElement>())
            };
            return ErrorsServerCodec.EncodeResponse(errorHolders);
        }

        [Test]
        [Timeout(10_000)]
        public async Task CanRetryAndTimeout()
        {
            var address = NetworkAddress.Parse("127.0.0.1:11001");

            HConsole.Configure(x => x.Set(this, config => config.SetIndent(0).SetPrefix("TEST")));
            HConsole.WriteLine(this, "Begin");

            HConsole.WriteLine(this, "Start server");
            await using var server = new Server(address, async (xsvr, xconn, xmsg)
                => await HandleAsync(xsvr, xconn, xmsg, async (svr, conn, msg) =>
                {
                    async Task ResponseAsync(ClientMessage response)
                    {
                        response.CorrelationId = msg.CorrelationId;
                        response.Flags |= ClientMessageFlags.BeginFragment | ClientMessageFlags.EndFragment;
                        await conn.SendAsync(response).CAF();
                    }

                    async Task EventAsync(ClientMessage eventMessage)
                    {
                        eventMessage.CorrelationId = msg.CorrelationId;
                        eventMessage.Flags |= ClientMessageFlags.BeginFragment | ClientMessageFlags.EndFragment;
                        await conn.SendAsync(eventMessage).CAF();
                    }

                    switch (msg.MessageType)
                    {
                        // must handle auth
                        case ClientAuthenticationServerCodec.RequestMessageType:
                            var authRequest = ClientAuthenticationServerCodec.DecodeRequest(msg);
                            var authResponse = ClientAuthenticationServerCodec.EncodeResponse(
                                0, address, Guid.NewGuid(), SerializationService.SerializerVersion,
                                "4.0", 1, Guid.NewGuid(), false);
                            await ResponseAsync(authResponse).CAF();
                            break;

                        // must handle events
                        case ClientAddClusterViewListenerServerCodec.RequestMessageType:
                            var addRequest = ClientAddClusterViewListenerServerCodec.DecodeRequest(msg);
                            var addResponse = ClientAddClusterViewListenerServerCodec.EncodeResponse();
                            await ResponseAsync(addResponse).CAF();

                            _ = Task.Run(async () =>
                            {
                                await Task.Delay(500).CAF();
                                var eventMessage = ClientAddClusterViewListenerServerCodec.EncodeMembersViewEvent(1, new[]
                                {
                                    new MemberInfo(Guid.NewGuid(), address, new MemberVersion(4, 0, 0), false, new Dictionary<string, string>()),
                                });
                                await EventAsync(eventMessage).CAF();
                            });

                            break;

                        default:
                            HConsole.WriteLine(svr, "Respond with error.");
                            var response = CreateErrorMessage(RemoteError.RetryableHazelcast);
                            await ResponseAsync(response).CAF();
                            break;
                    }
                }), LoggerFactory);
            await server.StartAsync().CAF();

            HConsole.WriteLine(this, "Start client");
            var options = HazelcastOptions.Build(configure: (configuration, options) =>
            {
                options.Networking.Addresses.Add("127.0.0.1:11001");
            });
            await using var client = (HazelcastClient) await HazelcastClientFactory.StartNewClientAsync(options);

            HConsole.WriteLine(this, "Send message");
            var message = ClientPingServerCodec.EncodeRequest();

            var token = new CancellationTokenSource(3_000).Token;
            Assert.ThrowsAsync<TaskCanceledException>(async () => await client.Cluster.Messaging.SendAsync(message, token).CAF());

            // TODO dispose the client, the server
            await server.StopAsync().CAF();
        }

        [Test]
        [Timeout(10_000)]
        public async Task CanRetryAndSucceed()
        {
            var address = NetworkAddress.Parse("127.0.0.1:11001");

            HConsole.Configure(x => x.Set(this, config => config.SetIndent(0).SetPrefix("TEST")));
            HConsole.WriteLine(this, "Begin");

            HConsole.WriteLine(this, "Start server");
            var count = 0;
            await using var server = new Server(address, async (xsvr, xconn, xmsg)
                => await HandleAsync(xsvr, xconn, xmsg, async (svr, conn, msg) =>
                {
                    HConsole.WriteLine(svr, "Handle request.");
                    ClientMessage response;
                    if (++count > 3)
                    {
                        HConsole.WriteLine(svr, "Respond with success.");
                        response = ClientPingServerCodec.EncodeResponse();
                    }
                    else
                    {
                        HConsole.WriteLine(svr, "Respond with error.");
                        response = CreateErrorMessage(RemoteError.RetryableHazelcast);
                        response.Flags |= ClientMessageFlags.BeginFragment | ClientMessageFlags.EndFragment;
                    }
                    response.CorrelationId = msg.CorrelationId;
                    await conn.SendAsync(response).CAF();
                }), LoggerFactory);
            await server.StartAsync().CAF();

            HConsole.WriteLine(this, "Start client");
            var options = HazelcastOptions.Build(configure: (configuration, options) =>
            {
                options.Networking.Addresses.Add("127.0.0.1:11001");
            });
            await using var client = (HazelcastClient) await HazelcastClientFactory.StartNewClientAsync(options);

            HConsole.WriteLine(this, "Send message");
            var message = ClientPingServerCodec.EncodeRequest();

            var token = new CancellationTokenSource(3_000).Token;
            await client.Cluster.Messaging.SendAsync(message, token); // default is 120s

            Assert.AreEqual(4, count);

            await server.StopAsync().CAF();
        }

        [Test]
        [Timeout(20_000)]
        public async Task TimeoutsIfServerIsTooSlow()
        {
            var address = NetworkAddress.Parse("127.0.0.1:11001");

            HConsole.Configure(x => x.Set(this, config => config.SetIndent(0).SetPrefix("TEST")));
            HConsole.WriteLine(this, "Begin");

            HConsole.WriteLine(this, "Start server");
            await using var server = new Server(address, async (xsvr, xconn, xmsg)
                => await HandleAsync(xsvr, xconn, xmsg, async (svr, conn, msg) =>
                {
                    HConsole.WriteLine(svr, "Handle request (slowww...)");
                    await Task.Delay(10_000).CAF();

                    HConsole.WriteLine(svr, "Respond with success.");
                    var response = ClientPingServerCodec.EncodeResponse();
                    response.CorrelationId = msg.CorrelationId;
                    await conn.SendAsync(response).CAF();
                }), LoggerFactory);
            await server.StartAsync().CAF();

            HConsole.WriteLine(this, "Start client");
            var options = HazelcastOptions.Build(configure: (configuration, options) =>
            {
                options.Networking.Addresses.Add("127.0.0.1:11001");
            });
            await using var client = (HazelcastClient) await HazelcastClientFactory.StartNewClientAsync(options);

            HConsole.WriteLine(this, "Send message");
            var message = ClientPingServerCodec.EncodeRequest();

            Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                var token = new CancellationTokenSource(3_000).Token;
                await client.Cluster.Messaging.SendAsync(message, token); // default is 120s
            });

            // TODO dispose the client, the server
            await server.StopAsync().CAF();
        }

        [Test]
        [Timeout(10_000)]
        public async Task Test()
        {
            //var host = Dns.GetHostEntry(_hostname);
            //var ipAddress = host.AddressList[0];
            //var endpoint = new IPEndPoint(ipAddress, _port);

            var address = NetworkAddress.Parse("127.0.0.1:11001");

            HConsole.Configure(x => x.Set(this, config => config.SetIndent(0).SetPrefix("TEST")));
            HConsole.WriteLine(this, "Begin");

            HConsole.WriteLine(this, "Start server");
            var server = new Server(address, async (xsvr, xconn, xmsg)
                => await HandleAsync(xsvr, xconn, xmsg, ReceiveMessage), LoggerFactory);
            await server.StartAsync().CAF();

            var options = HazelcastOptions.Build(Array.Empty<string>(), configure: (configuration, options) =>
            {
                options.Networking.Addresses.Add("127.0.0.1:11001");
            });

            HConsole.WriteLine(this, "Start client 1");
            await using var client1 = (HazelcastClient) await HazelcastClientFactory.StartNewClientAsync(options);

            HConsole.WriteLine(this, "Send message 1 to client 1");
            var message = CreateMessage("ping");
            var response = await client1.Cluster.Messaging.SendAsync(message, CancellationToken.None).CAF();

            HConsole.WriteLine(this, "Got response: " + GetText(response));

            HConsole.WriteLine(this, "Start client 2");
            await using var client2 = (HazelcastClient) await HazelcastClientFactory.StartNewClientAsync(options);

            HConsole.WriteLine(this, "Send message 1 to client 2");
            message = CreateMessage("a");
            response = await client2.Cluster.Messaging.SendAsync(message, CancellationToken.None).CAF();

            HConsole.WriteLine(this, "Got response: " + GetText(response));

            HConsole.WriteLine(this, "Send message 2 to client 1");
            message = CreateMessage("foo");
            response = await client1.Cluster.Messaging.SendAsync(message, CancellationToken.None).CAF();

            HConsole.WriteLine(this, "Got response: " + GetText(response));

            //XConsole.WriteLine(this, "Stop client");
            //await client1.CloseAsync().CAF();

            HConsole.WriteLine(this, "Stop server");
            await server.StopAsync().CAF();
            await Task.Delay(1000).CAF();

            HConsole.WriteLine(this, "End");
            await Task.Delay(100).CAF();
        }

        [Test]
        [Timeout(10_000)]
        public async Task ServerShutdown()
        {
            var address = NetworkAddress.Parse("127.0.0.1:11000");

            HConsole.Configure(x => x.Set(this, config => config.SetIndent(0).SetPrefix("TEST")));
            HConsole.WriteLine(this, "Begin");

            HConsole.WriteLine(this, "Start server");
            await using var server = new Server(address, async (xsvr, xconn, xmsg)
                => await HandleAsync(xsvr, xconn, xmsg, ReceiveMessage), LoggerFactory);
            await server.StartAsync().CAF();

            var options = HazelcastOptions.Build(Array.Empty<string>(), configure: (configuration, options) =>
            {
                options.Networking.Addresses.Add("127.0.0.1:11000");
            });

            HConsole.WriteLine(this, "Start client 1");
            await using var client1 = (HazelcastClient) await HazelcastClientFactory.StartNewClientAsync(options);

            HConsole.WriteLine(this, "Send message 1 to client 1");
            var message = CreateMessage("ping");
            var response = await client1.Cluster.Messaging.SendAsync(message, CancellationToken.None).CAF();

            HConsole.WriteLine(this, "Got response: " + GetText(response));

            HConsole.WriteLine(this, "Stop server");
            await server.StopAsync().CAF();
            await Task.Delay(1000).CAF();

            HConsole.WriteLine(this, "Send message 2 to client 1");
            message = CreateMessage("ping");
            Assert.ThrowsAsync<ClientNotConnectedException>(async () => await client1.Cluster.Messaging.SendAsync(message, CancellationToken.None).CAF());

            HConsole.WriteLine(this, "End");
            await Task.Delay(100).CAF();
        }

        [Test]
        [Timeout(10_000)]
        [Ignore("Requires a real server, obsolete")]
        public async Task Auth()
        {
            // need to start a real server (not the RC thing!)

            //var address = NetworkAddress.Parse("sgay-l4");
            var address = NetworkAddress.Parse("localhost");

            HConsole.Configure(x => x.Set(this, config => config.SetIndent(0).SetPrefix("TEST")));
            HConsole.WriteLine(this, "Begin");

            HConsole.WriteLine(this, "Start client ");
            var client1 = new MemberConnection(address, new MessagingOptions(), new SocketOptions(), new SslOptions(), new Int32Sequence(), new Int64Sequence(), new NullLoggerFactory());
            await client1.ConnectAsync(CancellationToken.None).CAF();

            // RC assigns a GUID but the default cluster name is 'dev'
            var clusterName = "dev";
            var username = (string) null; // null
            var password = (string) null; // null
            var clientId = Guid.NewGuid();
            var clientType = "CSP"; // CSharp
            var serializationVersion = (byte) 0x01;
            var clientVersion = "4.0";
            var clientName = "hz.client_0";
            var labels = new HashSet<string>();
            var requestMessage = ClientAuthenticationCodec.EncodeRequest(clusterName, username, password, clientId, clientType, serializationVersion, clientVersion, clientName, labels);
            HConsole.WriteLine(this, "Send auth request");
            var invocation = new Invocation(requestMessage, new MessagingOptions(), null, CancellationToken.None);
            var responseMessage = await client1.SendAsync(invocation, CancellationToken.None).CAF();
            HConsole.WriteLine(this, "Rcvd auth response " +
                                     HConsole.Lines(this, 1, responseMessage.Dump()));
            var response = ClientAuthenticationCodec.DecodeResponse(responseMessage);

            var status = (AuthenticationStatus) response.Status;
            NUnit.Framework.Assert.AreEqual(AuthenticationStatus.Authenticated, status);

            HConsole.WriteLine(this, "Stop client");
            await client1.DisposeAsync().CAF();

            HConsole.WriteLine(this, "End");
            await Task.Delay(100).CAF();
        }

        [Test]
        [Timeout(10_000)]
        [Ignore("Requires a real server, obsolete")]
        public async Task Cluster()
        {
            // this test expects a server

            HConsole.Configure(x => x.Set(this, config => config.SetIndent(0).SetPrefix("TEST")));
            HConsole.WriteLine(this, "Begin");

            HConsole.WriteLine(this, "Cluster?");

            var serializationService = new SerializationServiceBuilder(new NullLoggerFactory())
                .SetVersion(1)
                .Build();

            var options = new HazelcastOptions();

            //options.Networking.Addresses.Add("sgay-l4");
            options.Networking.Addresses.Add("localhost");

            var cluster = new Cluster(options, serializationService, new NullLoggerFactory());
            await cluster.Connections.ConnectAsync(CancellationToken.None).CAF();

            // now we can send messages...
            //await cluster.SendAsync(new ClientMessage()).CAF();

            // events?
            await Task.Delay(4000).CAF();

            HConsole.WriteLine(this, "End");
            await Task.Delay(100).CAF();
        }

        [Test]
        public void Sequences1()
        {
            const int origin = 1234;
            var bytes = new byte[4];
            bytes.WriteInt(0, origin);
            var buffer = new ReadOnlySequence<byte>(bytes);
            var value = BytesExtensions.ReadInt(ref buffer);
            NUnit.Framework.Assert.AreEqual(origin, value);
        }

        [Test]
        [Timeout(20_000)]
        public async Task SocketTimeout1()
        {
            await using var server = new Server(NetworkAddress.Parse("127.0.0.1:11000"), (svr, connection, message) => new ValueTask(), LoggerFactory);
            await server.StartAsync().CAF();

            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);

            // server is listening, can connect within 1s timeout
            await socket.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000), 1_000).CAF();

            socket.Close();
            await server.StopAsync().CAF();
        }

        [Test]
        [Timeout(20_000)]
        public void SocketTimeout2()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);

            // server is not listening, connecting results in timeout after 1s
            Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                await socket.ConnectAsync(NetworkAddress.Parse("www.hazelcast.com:5701").IPEndPoint, 500).CAF();
            });

            // socket has been properly closed and disposed
            Assert.Throws<ObjectDisposedException>(() =>
            {
                socket.Send(Array.Empty<byte>());
            });

            // can dispose multiple times
            socket.Close();
            socket.Dispose();
        }

        [Test]
        [Timeout(60_000)]
        public async Task SocketTimeout3()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);

            try
            {
                var endpoint = NetworkAddress.Parse("127.0.0.1:11000").IPEndPoint;
                await socket.ConnectAsync(endpoint, 60_000).CAF();
                Assert.Fail("Expected an exception.");
            }
            catch (TimeoutException)
            {
                Assert.Fail("Did not expect TimeoutException.");
            }
            catch (Exception)
            {
                // ok
            }

            // socket is not ready (but not disposed)
            Assert.Throws<SocketException>(() =>
            {
                socket.Send(Array.Empty<byte>());
            });
        }
    }
}
