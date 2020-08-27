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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Messaging;
using Hazelcast.Networking;
using Hazelcast.Testing.Networking;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Hazelcast.Tests.Messaging
{
    [TestFixture]
    public class ClientMessageConnectionTests
    {
        [Test]
        public void ArgumentExceptions()
        {
            var endpoint = NetworkAddress.Parse("127.0.0.1:11000").IPEndPoint;
            var s = new ClientSocketConnection(1, endpoint, new SocketOptions(), new SslOptions(), new NullLoggerFactory());
            var c = new ClientMessageConnection(s, new NullLoggerFactory());

            Assert.ThrowsAsync<ArgumentNullException>(async () => _ = await c.SendAsync(null));
        }

        [Test]
        public async Task OnReceiveMessage()
        {
            var endpoint = NetworkAddress.Parse("127.0.0.1:11000").IPEndPoint;
            var s = new ClientSocketConnection(1, endpoint, new SocketOptions(), new SslOptions(), new NullLoggerFactory());
            var c = new ClientMessageConnection(s, new NullLoggerFactory()) { OnReceiveMessage = OnReceiveMessageNotImplemented };

            Assert.That(c.OnReceiveMessage, Is.Not.Null);
            Assert.Throws<NotImplementedException>(() => c.OnReceiveMessage(default, default));

            c = new ClientMessageConnection(s, new NullLoggerFactory());

            using var l = new SocketListener(endpoint, SocketListenerMode.AcceptOnce);

            await s.ConnectAsync(default);

            Assert.Throws<InvalidOperationException>(() => c.OnReceiveMessage = OnReceiveMessageNotImplemented);
        }

        private static void OnReceiveMessageNotImplemented(ClientMessageConnection conn, ClientMessage msg)
            => throw new NotImplementedException();

        [Test]
        public void HandleFragments()
        {
            var endpoint = NetworkAddress.Parse("127.0.0.1:11000").IPEndPoint;
            var s = new ClientSocketConnection(1, endpoint, new SocketOptions(), new SslOptions(), new NullLoggerFactory());
            var c = new ClientMessageConnection(s, new NullLoggerFactory());

            ClientMessage recvd = null;

            void OnReceiveMessageLocal(ClientMessageConnection conn, ClientMessage msg)
            {
                recvd = msg;
            }

            static ClientMessage NewFragment(ClientMessageFlags flags, long fragmentId = 0)
            {
                var m = new ClientMessage();
                m.Append(new Frame(new byte[64]));
                m.Flags = flags;
                if (fragmentId > 0) m.FragmentId = fragmentId;
                m.Append(new Frame(new byte[4]));
                m.Append(new Frame(new byte[4]));
                return m;
            }

            // un-fragmented message
            c.OnReceiveMessage = OnReceiveMessageLocal;
            var fragment = NewFragment(ClientMessageFlags.Unfragmented);
            c.ReceiveFragmentAsync(fragment);
            Assert.That(recvd, Is.SameAs(fragment));

            // exception
            recvd = null;
            c.OnReceiveMessage = OnReceiveMessageNotImplemented;
            c.ReceiveFragmentAsync(fragment);
            Assert.That(recvd, Is.Null);

            // ...
            c.OnReceiveMessage = OnReceiveMessageLocal;
            fragment = NewFragment(ClientMessageFlags.BeginFragment, 1);
            recvd = null;
            c.ReceiveFragmentAsync(fragment); // begin
            Assert.That(recvd, Is.Null);
            fragment = NewFragment(ClientMessageFlags.BeginFragment, 1 );
            recvd = null;
            c.ReceiveFragmentAsync(fragment); // ignore duplicate begin
            Assert.That(recvd, Is.Null);
            fragment = NewFragment(ClientMessageFlags.Default, 1 );
            recvd = null;
            c.ReceiveFragmentAsync(fragment); // accumulate
            Assert.That(recvd, Is.Null);
            fragment = NewFragment(ClientMessageFlags.EndFragment, 2 );
            recvd = null;
            c.ReceiveFragmentAsync(fragment); // ignore non-matching end
            Assert.That(recvd, Is.Null);
            fragment = NewFragment(ClientMessageFlags.EndFragment, 1 );
            recvd = null;
            c.ReceiveFragmentAsync(fragment); // end
            Assert.That(recvd, Is.Not.Null);

            Assert.That(recvd.Count(), Is.EqualTo(6));

            fragment = NewFragment(ClientMessageFlags.BeginFragment, 4);
            recvd = null;
            c.ReceiveFragmentAsync(fragment); // begin
            Assert.That(recvd, Is.Null);
            fragment = NewFragment(ClientMessageFlags.Default, 4);
            recvd = null;
            c.ReceiveFragmentAsync(fragment); // accumulate
            Assert.That(recvd, Is.Null);
            c.OnReceiveMessage = OnReceiveMessageNotImplemented;
            fragment = NewFragment(ClientMessageFlags.EndFragment, 4);
            recvd = null;
            c.ReceiveFragmentAsync(fragment); // end
            Assert.That(recvd, Is.Null);
        }

        [Test]
        public async Task Cancel()
        {
            static async ValueTask ServerHandler(Hazelcast.Testing.TestServer.Server s, ClientMessageConnection c, ClientMessage m)
            {
                await c.SendAsync(new ClientMessage(new Frame(new byte[64])));
            }

            var address = NetworkAddress.Parse("127.0.0.1:11000");
            await using var server = new Hazelcast.Testing.TestServer.Server(address, ServerHandler, new NullLoggerFactory());
            await server.StartAsync();

            await using var socket = new ClientSocketConnection(0, address.IPEndPoint, new SocketOptions(), new SslOptions(), new NullLoggerFactory());
            var m = new ClientMessageConnection(socket, new NullLoggerFactory());
            await socket.ConnectAsync(default);

            await socket.SendAsync(MemberConnection.ClientProtocolInitBytes, MemberConnection.ClientProtocolInitBytes.Length);

            // testing & code coverage - OnSending should *not* be used outside of tests
            var cancellation = new CancellationTokenSource();
            m.OnSending = () => cancellation.Cancel();

            Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await m.SendAsync(new ClientMessage(new Frame(new byte[64], (FrameFlags) ClientMessageFlags.Unfragmented)), cancellation.Token);
            });
        }
    }
}
