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
using Hazelcast.Data;
using Hazelcast.Exceptions;
using Hazelcast.Logging;
using Hazelcast.Messaging;
using Hazelcast.Networking;
using Hazelcast.Protocol;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Protocol.Data;
using Hazelcast.Serialization;
using Hazelcast.Testing;
using Hazelcast.Testing.TestServer;
using Hazelcast.Tests.Testing;
using Hazelcast.Testing.Protocol;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Hazelcast.Tests
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

        private async ValueTask ReceiveMessage(ClientMessageConnection connection, ClientMessage message)
        {
            XConsole.WriteLine(this, "Respond");
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

            await connection.SendAsync(response);
            XConsole.WriteLine(this, "Responded");
        }

        private ClientMessage CreateErrorMessage(ClientProtocolErrors error)
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

            XConsole.Configure(this, config => config.SetIndent(0).SetPrefix("TEST"));
            XConsole.WriteLine(this, "Begin");

            XConsole.WriteLine(this, "Start server");
            Server server = null;
            server = new Server(address, async (conn, msg) =>
            {
                async Task ResponseAsync(ClientMessage response)
                {
                    response.CorrelationId = msg.CorrelationId;
                    response.Flags |= ClientMessageFlags.BeginFragment | ClientMessageFlags.EndFragment;
                    await conn.SendAsync(response);
                }

                async Task EventAsync(ClientMessage eventMessage)
                {
                    eventMessage.CorrelationId = msg.CorrelationId;
                    eventMessage.Flags |= ClientMessageFlags.BeginFragment | ClientMessageFlags.EndFragment;
                    await conn.SendAsync(eventMessage);
                }

                switch (msg.MessageType)
                {
                    // must handle auth
                    case ClientAuthenticationServerCodec.RequestMessageType:
                        var authRequest = ClientAuthenticationServerCodec.DecodeRequest(msg);
                        var authResponse = ClientAuthenticationServerCodec.EncodeResponse(
                            0, address, Guid.NewGuid(), SerializationService.SerializerVersion,
                            "4.0", 1, Guid.NewGuid(), false);
                        await ResponseAsync(authResponse);
                        break;

                    // must handle events
                    case ClientAddClusterViewListenerServerCodec.RequestMessageType:
                        var addRequest = ClientAddClusterViewListenerServerCodec.DecodeRequest(msg);
                        var addResponse = ClientAddClusterViewListenerServerCodec.EncodeResponse();
                        await ResponseAsync(addResponse);

                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(500);
                            var eventMessage = ClientAddClusterViewListenerServerCodec.EncodeMembersViewEvent(1, new[]
                            {
                                new MemberInfo(Guid.NewGuid(), address, new MemberVersion(4, 0, 0), false, new Dictionary<string, string>()),
                            });
                            await EventAsync(eventMessage);
                        });

                        break;

                    default:
                        XConsole.WriteLine(server, "Respond with error.");
                        var response = CreateErrorMessage(ClientProtocolErrors.RetryableHazelcast);
                        await ResponseAsync(response);
                        break;
                }
            }, LoggerFactory);
            AddDisposable(server);
            await server.StartAsync();

            XConsole.WriteLine(this, "Start client");
            var client = (HazelcastClient)new HazelcastClientFactory(configuration =>
            {
                configuration.Networking.Addresses.Add("127.0.0.1:11001");
            }).CreateClient();
            AddDisposable(client);
            await client.OpenAsync();

            XConsole.WriteLine(this, "Send message");
            var message = ClientPingServerCodec.EncodeRequest();

            var token = new CancellationTokenSource(3_000).Token;
            Assert.ThrowsAsync<TaskCanceledException>(async () => await client.Cluster.SendAsync(message, token));

            // TODO dispose the client, the server
            await server.StopAsync();
        }

        [Test]
        [Timeout(10_000)]
        public async Task CanRetryAndSucceed()
        {
            var address = NetworkAddress.Parse("127.0.0.1:11001");

            XConsole.Configure(this, config => config.SetIndent(0).SetPrefix("TEST"));
            XConsole.WriteLine(this, "Begin");

            XConsole.WriteLine(this, "Start server");
            var count = 0;
            Server server = null;
            server = new Server(address, async (conn, msg) =>
            {
                XConsole.WriteLine(server, "Handle request.");
                ClientMessage response;
                if (++count > 3)
                {
                    XConsole.WriteLine(server, "Respond with success.");
                    response = ClientPingServerCodec.EncodeResponse();
                }
                else
                {
                    XConsole.WriteLine(server, "Respond with error.");
                    response = CreateErrorMessage(ClientProtocolErrors.RetryableHazelcast);
                    response.Flags |= ClientMessageFlags.BeginFragment | ClientMessageFlags.EndFragment;
                }
                response.CorrelationId = msg.CorrelationId;
                await conn.SendAsync(response);
            }, LoggerFactory);
            AddDisposable(server);
            await server.StartAsync();

            XConsole.WriteLine(this, "Start client");
            var client = (HazelcastClient) new HazelcastClientFactory(configuration =>
            {
                configuration.Networking.Addresses.Add("127.0.0.1:11001");
            }).CreateClient();
            AddDisposable(client);
            await client.OpenAsync();

            XConsole.WriteLine(this, "Send message");
            var message = ClientPingServerCodec.EncodeRequest();

            var token = new CancellationTokenSource(3_000).Token;
            await client.Cluster.SendAsync(message, token); // default is 120s

            NUnit.Framework.Assert.AreEqual(4, count);

            // TODO dispose the client, the server
            await server.StopAsync();
        }

        [Test]
        [Timeout(20_000)]
        public async Task TimeoutsIfServerIsTooSlow()
        {
            var address = NetworkAddress.Parse("127.0.0.1:11001");

            XConsole.Configure(this, config => config.SetIndent(0).SetPrefix("TEST"));
            XConsole.WriteLine(this, "Begin");

            XConsole.WriteLine(this, "Start server");
            Server server = null;
            server = new Server(address, async (conn, msg) =>
            {
                XConsole.WriteLine(server, "Handle request (slowww...)");
                await Task.Delay(10_000);

                XConsole.WriteLine(server, "Respond with success.");
                var response = ClientPingServerCodec.EncodeResponse();
                response.CorrelationId = msg.CorrelationId;
                await conn.SendAsync(response);
            }, LoggerFactory);
            AddDisposable(server);
            await server.StartAsync();

            XConsole.WriteLine(this, "Start client");
            var client = (HazelcastClient)new HazelcastClientFactory(configuration =>
            {
                configuration.Networking.Addresses.Add("127.0.0.1:11001");
            }).CreateClient();
            AddDisposable(client);
            await client.OpenAsync();

            XConsole.WriteLine(this, "Send message");
            var message = ClientPingServerCodec.EncodeRequest();

            Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                var token = new CancellationTokenSource(3_000).Token;
                await client.Cluster.SendAsync(message, token); // default is 120s
            });

            // TODO dispose the client, the server
            await server.StopAsync();
        }

        [Test]
        [Timeout(10_000)]
        public async Task Test()
        {
            //var host = Dns.GetHostEntry(_hostname);
            //var ipAddress = host.AddressList[0];
            //var endpoint = new IPEndPoint(ipAddress, _port);

            var address = NetworkAddress.Parse("127.0.0.1:11001");

            XConsole.Configure(this, config => config.SetIndent(0).SetPrefix("TEST"));
            XConsole.WriteLine(this, "Begin");

            XConsole.WriteLine(this, "Start server");
            var server = new Server(address, ReceiveMessage, LoggerFactory);
            await server.StartAsync();

            var clientFactory = new HazelcastClientFactory(configuration =>
            {
                configuration.Networking.Addresses.Add("127.0.0.1:11001");
            });

            XConsole.WriteLine(this, "Start client 1");
            var client1 = (HazelcastClient) clientFactory.CreateClient();
            AddDisposable(client1);
            await client1.OpenAsync();

            XConsole.WriteLine(this, "Send message 1 to client 1");
            var message = CreateMessage("ping");
            var response = await client1.Cluster.SendAsync(message, CancellationToken.None);

            XConsole.WriteLine(this, "Got response: " + GetText(response));

            XConsole.WriteLine(this, "Start client 2");
            var client2 = (HazelcastClient)clientFactory.CreateClient();
            AddDisposable(client2);
            await client2.OpenAsync();

            XConsole.WriteLine(this, "Send message 1 to client 2");
            message = CreateMessage("a");
            response = await client2.Cluster.SendAsync(message, CancellationToken.None);

            XConsole.WriteLine(this, "Got response: " + GetText(response));

            XConsole.WriteLine(this, "Send message 2 to client 1");
            message = CreateMessage("foo");
            response = await client1.Cluster.SendAsync(message, CancellationToken.None);

            XConsole.WriteLine(this, "Got response: " + GetText(response));

            //XConsole.WriteLine(this, "Stop client");
            //await client1.CloseAsync();

            XConsole.WriteLine(this, "Stop server");
            await server.StopAsync();
            await Task.Delay(1000);

            XConsole.WriteLine(this, "End");
            await Task.Delay(100);
        }

        [Test]
        [Timeout(10_000)]
        public async Task ServerShutdown()
        {
            var address = NetworkAddress.Parse("127.0.0.1:11000");

            XConsole.Configure(this, config => config.SetIndent(0).SetPrefix("TEST"));
            XConsole.WriteLine(this, "Begin");

            XConsole.WriteLine(this, "Start server");
            var server = new Server(address, ReceiveMessage, LoggerFactory);
            AddDisposable(server);
            await server.StartAsync();

            var clientFactory = new HazelcastClientFactory(configuration =>
            {
                configuration.Networking.Addresses.Add("127.0.0.1:11000");
            });

            XConsole.WriteLine(this, "Start client 1");
            var client1 = (HazelcastClient)clientFactory.CreateClient();
            AddDisposable(client1);
            await client1.OpenAsync();

            XConsole.WriteLine(this, "Send message 1 to client 1");
            var message = CreateMessage("ping");
            var response = await client1.Cluster.SendAsync(message, CancellationToken.None);

            XConsole.WriteLine(this, "Got response: " + GetText(response));

            XConsole.WriteLine(this, "Stop server");
            await server.StopAsync();
            await Task.Delay(1000);

            XConsole.WriteLine(this, "Send message 2 to client 1");
            message = CreateMessage("ping");
            Assert.ThrowsAsync<HazelcastClientNotActiveException>(async () => await client1.Cluster.SendAsync(message, CancellationToken.None));

            XConsole.WriteLine(this, "End");
            await Task.Delay(100);
        }

        [Test]
        [Timeout(10_000)]
        public async Task Auth()
        {
            // need to start a real server (not the RC thing!)

            var address = NetworkAddress.Parse("sgay-l4");

            XConsole.Configure(this, config => config.SetIndent(0).SetPrefix("TEST"));
            XConsole.WriteLine(this, "Begin");

            XConsole.WriteLine(this, "Start client ");
            var client1 = new Client(address, new Int64Sequence(), new NullLoggerFactory());
            await client1.ConnectAsync(CancellationToken.None);

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
            XConsole.WriteLine(this, "Send auth request");
            var invocation = new Invocation(requestMessage, CancellationToken.None);
            var responseMessage = await client1.SendAsync(invocation, CancellationToken.None);
            XConsole.WriteLine(this, "Rcvd auth response " +
                                     XConsole.Lines(this, 1, responseMessage.Dump()));
            var response = ClientAuthenticationCodec.DecodeResponse(responseMessage);

            var status = (AuthenticationStatus) response.Status;
            NUnit.Framework.Assert.AreEqual(AuthenticationStatus.Authenticated, status);

            XConsole.WriteLine(this, "Stop client");
            await client1.DisposeAsync();

            XConsole.WriteLine(this, "End");
            await Task.Delay(100);
        }

        [Test]
        [Timeout(10_000)]
        public async Task Cluster()
        {
            // this test expects a server

            XConsole.Configure(this, config => config.SetIndent(0).SetPrefix("TEST"));
            XConsole.WriteLine(this, "Begin");

            XConsole.WriteLine(this, "Cluster?");

            var serializationService = new SerializationServiceBuilder(new NullLoggerFactory())
                .SetVersion(1)
                .Build();

            var configuration = new HazelcastConfiguration();

            configuration.Networking.Addresses.Add("sgay-l4");
            configuration.Security.Authenticator.Creator = ()
                => new Authenticator(configuration.Security);

            var cluster = new Cluster("dev", "hz.client",
                new HashSet<string>(),
                configuration.Cluster,
                configuration.Networking,
                configuration.LoadBalancing,
                configuration.Security,
                serializationService,
                new NullLoggerFactory());
            await cluster.ConnectAsync(CancellationToken.None);

            // now we can send messages...
            //await cluster.SendAsync(new ClientMessage());

            // events?
            await Task.Delay(4000);

            XConsole.WriteLine(this, "End");
            await Task.Delay(100);
        }

        [Test]
        public void Sequences1()
        {
            const int origin = 1234;
            var bytes = new byte[4];
            bytes.WriteInt32(0, origin);
            var buffer = new ReadOnlySequence<byte>(bytes);
            var value = BytesExtensions.ReadInt32(ref buffer);
            NUnit.Framework.Assert.AreEqual(origin, value);
        }

        [Test]
        [Timeout(20_000)]
        public async Task SocketTimeout1()
        {
            var server = new Server(NetworkAddress.Parse("127.0.0.1:11000"), (connection, message) => new ValueTask(), LoggerFactory);
            await server.StartAsync();

            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // server is listening, can connect within 1s timeout
            await socket.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000), 1_000);

            socket.Close();
            await server.StopAsync();
        }

        [Test]
        [Timeout(20_000)]
        public void SocketTimeout2()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // server is not listening, connecting results in timeout after 1s
            Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                await socket.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000), 1_000);
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
        [Timeout(20_000)]
        public async Task SocketTimeout3()
        {
            var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000));
            serverSocket.Listen(1);
            serverSocket.BeginAccept(result =>
            {
                var listener = (Socket) result.AsyncState;
                Thread.Sleep(1000);
                listener.EndAccept(result);
            }, serverSocket);

            var socket1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await socket1.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000), 1_000);

            var socket2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await socket2.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000), 1_000);

            Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                var socket3 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await socket3.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000), 1_000);
            });
        }

        [Test]
        [Timeout(60_000)]
        public async Task SocketTimeout4()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                await socket.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000), 60_000);
                Assert.Fail("Expected an exception.");
            }
            catch (TimeoutException)
            {
                Assert.Fail("Did not expect TimeoutException.");
            }
            catch (Exception e)
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