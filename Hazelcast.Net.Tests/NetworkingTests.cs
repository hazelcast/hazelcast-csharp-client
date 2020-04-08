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
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Logging;
using Hazelcast.Messaging;
using Hazelcast.Tests.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests
{
    [TestFixture]
    public class NetworkingTests
    {
        // NOTES
        //
        // read
        //  danger of completion source
        //    https://devblogs.microsoft.com/premier-developer/the-danger-of-taskcompletionsourcet-class/
        //
        // sending: we can either queue messages, or just send them immediately
        //  it does not make a diff for user, who's going to wait anyways
        //  but queueing may prevent flooding the server
        //  and, in order to be multi-threaded, we HAVE to serialize the sending
        //  of messages through the socket - either by queuing or by forcing
        //  the client to wait - which is kind of a nice way to apply back-
        //  pressure?
        //
        // TODO
        //  must: implement something, lock in connection
        //  alternative: implement queuing in connection
        //
        // receiving: we process messages immediately so there is no queuing of
        //  messages - should we have some?
        //
        // TODO: understand the schedulers in HZ code


        private ClientMessage CreateMessage(string text)
        {
            //return new Message2(text);

            // FIXME tons of frames = tons of packet unless we can write a sequence?
            // FIXME what is the structure of a message, frame-wise?

            // first frame has message type, correlation id and partition id - what else?
            // which frame is Begin, End, Final?

            var message = new ClientMessage()
                .Append(new Frame(new byte[64])) // header stuff
                .Append(new Frame(Encoding.UTF8.GetBytes(text)));
            return message;
        }

        private string GetText(ClientMessage message)
            => Encoding.UTF8.GetString(message.FirstFrame.Next.Bytes);

        [Test]
        [Timeout(10_000)]
        public async Task Test()
        {
            //var host = Dns.GetHostEntry(_hostname);
            //var ipAddress = host.AddressList[0];
            //var endpoint = new IPEndPoint(ipAddress, _port);

            var endpoint = NetStandardCompatibility.IPEndPoint.Parse("127.0.0.1:11001");

            XConsole.Setup(this, 0, "TST");
            XConsole.WriteLine(this, "Begin");

            XConsole.WriteLine(this, "Start server");
            var server = new Server.Server(endpoint);
            await server.StartAsync();

            var sequence = new Int32Sequence();

            XConsole.WriteLine(this, "Start client 1");
            var client1 = new Client.Client(endpoint, sequence);
            await client1.ConnectAsync();

            XConsole.WriteLine(this, "Send message 1 to client 1");
            var message = CreateMessage("ping");
            var response = await client1.SendAsync(message);

            XConsole.WriteLine(this, "Got response: " + GetText(response));

            XConsole.WriteLine(this, "Start client 2");
            var client2 = new Client.Client(endpoint, sequence);
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
            var endpoint = NetStandardCompatibility.IPEndPoint.Parse("127.0.0.1:11000");

            XConsole.Setup(this, 0, "TST");
            XConsole.WriteLine(this, "Begin");

            XConsole.WriteLine(this, "Start server");
            var server = new Server.Server(endpoint);
            await server.StartAsync();

            XConsole.WriteLine(this, "Start client 1");
            var client1 = new Client.Client(endpoint);
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
            var endpoint = NetStandardCompatibility.IPEndPoint.Parse("127.0.0.1:5701");
            var client1 = new Client.Client(endpoint);
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
            var origin = 1234;

            var o = origin;
            var bytes = new byte[4];
            for (var i = 3; i >= 0; i--)
            {
                Console.WriteLine($"{i}: {(byte) o}");
                bytes[i] = (byte) o;
                o >>= 8;
            }

            var buffer = new ReadOnlySequence<byte>(bytes);

            var value = 0;
            var e = buffer.GetEnumerator();
            var j = 0;
            while (j < 4)
            {
                e.MoveNext();
                var m = e.Current;
                var k = 0;
                var l = m.Span.Length;
                while (k < l && j < 4)
                {
                    value <<= 8;
                    Console.WriteLine(m.Span[k]);
                    value |= m.Span[k++];
                    j++;
                }
            }

            XAssert.AreEqual(origin, value);
        }

        [Test]
        public void Sequences2()
        {
            var origin = 1234;

            var bytes = new byte[4];
            for (var i = 0; i < 4; i++)
            {
                bytes[i] = (byte)origin;
                origin >>= 8;
            }

            var buffer = new ReadOnlySequence<byte>(bytes);
            var value = buffer.ReadInt32();
            XAssert.AreEqual(origin, value);
        }
    }
}