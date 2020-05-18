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
using System.Text;
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
using Hazelcast.Testing.TestServer;
using Hazelcast.Tests.Testing;
using Hazelcast.Testing.Protocol;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Hazelcast.Tests
{
    [TestFixture]
    public class NetworkingTests
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

        private ClientMessage CreateErrorMessage()
        {
            // can we prepare server messages?
            var errorHolders = new List<ErrorHolder>
            {
                new ErrorHolder(ClientProtocolErrors.RetryableHazelcast, "className", "message", Enumerable.Empty<StackTraceElement>())
            };
            return ErrorsServerCodec.EncodeResponse(errorHolders);
        }


        [Test]
        [Timeout(10_000)]
        public async Task RetryAndTimeout()
        {
            var address = NetworkAddress.Parse("127.0.0.1:11001");

            XConsole.Configure(this, config => config.SetIndent(0).SetPrefix("TEST"));
            XConsole.WriteLine(this, "Begin");

            XConsole.WriteLine(this, "Start server");
            var server = new Server(address, async (conn, msg) =>
            {
                var response = CreateErrorMessage();
                response.CorrelationId = msg.CorrelationId;
                response.MessageType = 0x00; // 0x00 means exception
                response.Flags |= ClientMessageFlags.BeginFragment | ClientMessageFlags.EndFragment;
                await conn.SendAsync(response);
            });
            await server.StartAsync();

            var corSequence = new Int64Sequence();
            var conSequence = new Int32Sequence();

            XConsole.WriteLine(this, "Start client 1");
            var client1 = new Client(address, corSequence, conSequence, new NullLoggerFactory());
            await client1.ConnectAsync();

            XConsole.WriteLine(this, "Send message 1 to client 1");
            var message = ClientPingServerCodec.EncodeRequest();

            Assert.ThrowsAsync<TimeoutException>(async () => await client1.SendAsync(message, 2_000));

            // TODO dispose the client, the server
            await server.StopAsync();
        }

        [Test]
        [Timeout(10_000)]
        public async Task RetryAndSucceed()
        {
            var address = NetworkAddress.Parse("127.0.0.1:11001");

            XConsole.Configure(this, config => config.SetIndent(0).SetPrefix("TEST"));
            XConsole.WriteLine(this, "Begin");

            XConsole.WriteLine(this, "Start server");
            var count = 0;
            var server = new Server(address, async (conn, msg) =>
            {
                ClientMessage response;
                if (++count > 3)
                {
                    response = ClientPingServerCodec.EncodeResponse();
                }
                else
                {
                    response = CreateErrorMessage();
                    response.MessageType = 0x00; // 0x00 means exception
                    response.Flags |= ClientMessageFlags.BeginFragment | ClientMessageFlags.EndFragment;
                }
                response.CorrelationId = msg.CorrelationId;
                await conn.SendAsync(response);
            });
            await server.StartAsync();

            var corSequence = new Int64Sequence();
            var conSequence = new Int32Sequence();

            XConsole.WriteLine(this, "Start client 1");
            var client1 = new Client(address, corSequence, conSequence, new NullLoggerFactory());
            await client1.ConnectAsync();

            XConsole.WriteLine(this, "Send message 1 to client 1");
            var message = ClientPingServerCodec.EncodeRequest();

            await client1.SendAsync(message);

            Assert.AreEqual(4, count);

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
            var server = new Server(address, ReceiveMessage);
            await server.StartAsync();

            var corSequence = new Int64Sequence();
            var conSequence = new Int32Sequence();

            XConsole.WriteLine(this, "Start client 1");
            var client1 = new Clustering.Client(address, corSequence, conSequence, new NullLoggerFactory());
            await client1.ConnectAsync();

            XConsole.WriteLine(this, "Send message 1 to client 1");
            var message = CreateMessage("ping");
            var response = await client1.SendAsync(message);

            XConsole.WriteLine(this, "Got response: " + GetText(response));

            XConsole.WriteLine(this, "Start client 2");
            var client2 = new Clustering.Client(address, corSequence, conSequence, new NullLoggerFactory());
            await client2.ConnectAsync();

            XConsole.WriteLine(this, "Send message 1 to client 2");
            message = CreateMessage("a");
            response = await client2.SendAsync(message);

            XConsole.WriteLine(this, "Got response: " + GetText(response));

            XConsole.WriteLine(this, "Send message 2 to client 1");
            message = CreateMessage("foo");
            response = await client1.SendAsync(message);

            XConsole.WriteLine(this, "Got response: " + GetText(response));

            XConsole.WriteLine(this, "Stop client");
            await client1.ShutdownAsync();

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
            var server = new Server(address, ReceiveMessage);
            await server.StartAsync();

            XConsole.WriteLine(this, "Start client 1");
            var client1 = new Clustering.Client(address, new Int64Sequence(), new NullLoggerFactory());
            await client1.ConnectAsync();

            XConsole.WriteLine(this, "Send message 1 to client 1");
            var message = CreateMessage("ping");
            var response = await client1.SendAsync(message);

            XConsole.WriteLine(this, "Got response: " + GetText(response));

            XConsole.WriteLine(this, "Stop server");
            await server.StopAsync();
            await Task.Delay(1000);

            XConsole.WriteLine(this, "Send message 2 to client 1");
            message = CreateMessage("ping");
            XAssert.ThrowsAsync<InvalidOperationException>(async () => await client1.SendAsync(message));

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
            await client1.ConnectAsync();

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
            var responseMessage = await client1.SendAsync(requestMessage);
            XConsole.WriteLine(this, "Rcvd auth response " +
                                     XConsole.Lines(this, 1, responseMessage.Dump()));
            var response = ClientAuthenticationCodec.DecodeResponse(responseMessage);

            var status = (AuthenticationStatus) response.Status;
            Assert.AreEqual(AuthenticationStatus.Authenticated, status);

            XConsole.WriteLine(this, "Stop client");
            await client1.ShutdownAsync();

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
            await cluster.ConnectAsync();

            // now we can send messages...
            //await cluster.SendAsync(new ClientMessage());

            // events?
            await Task.Delay(4000);

            XConsole.WriteLine(this, "End");
            await Task.Delay(100);
        }

        [Test]
        [Timeout(10_000)]
        [Ignore("poison-safe!")]
        public async Task PoisonTest()
        {
            // must hit an actual server...
            // but: that does not poison the server as there is a max message size
            // so we'd need to do it in a fragmented way, not with frames
            // well, even with fragments... still seems to limit message size...
            // ah, and fragmented segments are not allowed before auth - safe

            /*

HAZELCAST_VERSION="4.0"
HAZELCAST_TEST_VERSION="4.0"
HAZELCAST_LIB=build/temp/lib

CLASSPATH="$HAZELCAST_LIB/hazelcast-enterprise-${HAZELCAST_VERSION}.jar;$HAZELCAST_LIB/hazelcast-enterprise-${HAZELCAST_TEST_VERSION}-tests.jar;$HAZELCAST_LIB/hazelcast-${HAZELCAST_TEST_VERSION}-tests.jar"
LICENSE="-Dhazelcast.enterprise.license.key=UNLIMITED_LICENSE#99Nodes#VuE0OIH7TbfKwAUNmSj1JlyFkr6a53911000199920009119011112151009"
CMD_CONFIGS="-Dhazelcast.config=src/Hazelcast.Tests/Resources/hazelcast.xml -Xms2g -Xmx2g -Dhazelcast.multicast.group=224.206.1.1 -Djava.net.preferIPv4Stack=true"

java  ${LICENSE} ${CMD_CONFIGS} -cp ${CLASSPATH} com.hazelcast.core.server.HazelcastMemberStarter >build/temp/hazelcast-${HAZELCAST_VERSION}-out.log 2>build/temp/hazelcast-${HAZELCAST_VERSION}-err.log &

            */

            // connect to real server
            var address = NetworkAddress.Parse("127.0.0.1:5701");
            var client1 = new Clustering.Client(address, new Int64Sequence(), new NullLoggerFactory());
            await client1.ConnectAsync();
            /*
            // send poison
            var message = new Message(new Frame(new byte[12]));
            message.CorrelationId = 0;
            message.MessageType = 0000;
            message.Flags |= MessageFlags.BeginFragment;
            await client1._connection.SendFrameAsync(message.FirstFrame);

            var bytes = new byte[512];
            message = new Message(new Frame(bytes));
            await client1._connection.SendFrameAsync(message.FirstFrame);

            while (true)
            {
                message = new Message(new Frame(new byte[12]));
                message.CorrelationId = 0;
                message.MessageType = 0000;
                await client1._connection.SendFrameAsync(message.FirstFrame);

                message = new Message(new Frame(bytes));
                await client1._connection.SendFrameAsync(message.FirstFrame);

                // never send the EndFragment nor IsFinal frame?
            }
            */
        }

        [Test]
        public void Sequences1()
        {
            const int origin = 1234;
            var bytes = new byte[4];
            bytes.WriteInt32(0, origin);
            var buffer = new ReadOnlySequence<byte>(bytes);
            var value = BytesExtensions.ReadInt32(ref buffer);
            Assert.AreEqual(origin, value);
        }

    }
}