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
using System.Threading.Tasks;
using NUnit.Framework;

namespace AsyncTests1.Networking
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

        [Test]
        [Timeout(10_000)]
        public async Task Test()
        {
            //var host = Dns.GetHostEntry(_hostname);
            //var ipAddress = host.AddressList[0];
            //var endpoint = new IPEndPoint(ipAddress, _port);

            var endpoint = IPEndPoint.Parse("127.0.0.1:11000");

            var log = new Log("TST");
            log.WriteLine("Begin");

            log.WriteLine("Start server");
            var server = new Server(endpoint);
            await server.StartAsync();

            var sequence = new Int32Sequence();

            log.WriteLine("Start client 1");
            var client1 = new Client(endpoint, sequence);
            await client1.ConnectAsync();

            log.WriteLine("Send message 1 to client 1");
            var message = new Message("ping");
            var response = await client1.SendAsync(message);

            log.WriteLine("Got response: " + response.Text);

            log.WriteLine("Start client 2");
            var client2 = new Client(endpoint, sequence);
            await client2.ConnectAsync();

            log.WriteLine("Send message 1 to client 2");
            message = new Message("a");
            response = await client2.SendAsync(message);

            log.WriteLine("Got response: " + response.Text);

            log.WriteLine("Send message 2 to client 1");
            message = new Message("foo");
            response = await client1.SendAsync(message);

            log.WriteLine("Got response: " + response.Text);

            log.WriteLine("Stop client");
            await client1.ShutdownAsync();

            log.WriteLine("Stop server");
            await server.StopAsync();
            await Task.Delay(1000);

            log.WriteLine("End");
            await Task.Delay(100);
        }

        [Test]
        [Timeout(10_000)]
        public async Task ServerShutdown()
        {
            var endpoint = IPEndPoint.Parse("127.0.0.1:11000");

            var log = new Log("TST");
            log.WriteLine("Begin");

            log.WriteLine("Start server");
            var server = new Server(endpoint);
            await server.StartAsync();

            log.WriteLine("Start client 1");
            var client1 = new Client(endpoint);
            await client1.ConnectAsync();

            log.WriteLine("Send message 1 to client 1");
            var message = new Message("ping");
            var response = await client1.SendAsync(message);

            log.WriteLine("Got response: " + response.Text);

            log.WriteLine("Stop server");
            await server.StopAsync();
            await Task.Delay(1000);

            log.WriteLine("Send message 2 to client 1");
            message = new Message("ping");
            Assert.ThrowsAsync<InvalidOperationException>(async () => await client1.SendAsync(message));

            log.WriteLine("End");
            await Task.Delay(100);
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

            Assert.AreEqual(origin, value);
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
            Assert.AreEqual(origin, value);
        }
    }
}