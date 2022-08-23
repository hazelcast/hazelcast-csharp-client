// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Networking;
using Hazelcast.Testing;
using Hazelcast.Testing.Networking;
using Microsoft.Extensions.Logging;
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
            var s = new ClientSocketConnection(Guid.NewGuid(), endpoint, new NetworkingOptions(), new SslOptions(), new NullLoggerFactory());
            var c = new ClientMessageConnection(s, new NullLoggerFactory());

            Assert.ThrowsAsync<ArgumentNullException>(async () => _ = await c.SendAsync(null));
        }

        [Test]
        public async Task OnReceiveMessage()
        {
            var endpoint = NetworkAddress.Parse("127.0.0.1:11000").IPEndPoint;
            var s = new ClientSocketConnection(Guid.NewGuid(), endpoint, new NetworkingOptions(), new SslOptions(), new NullLoggerFactory());
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
            var s = new ClientSocketConnection(Guid.NewGuid(), endpoint, new NetworkingOptions(), new SslOptions(), new NullLoggerFactory());
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
        [Timeout(20_000)]
        public async Task Cancel()
        {
            static async ValueTask ServerHandler(Hazelcast.Testing.TestServer.Server s, ClientMessageConnection c, ClientMessage m)
            {
                await c.SendAsync(new ClientMessage(new Frame(new byte[64])));
            }

            var address = NetworkAddress.Parse("127.0.0.1:11000");
            await using var server = new Hazelcast.Testing.TestServer.Server(address, ServerHandler, new NullLoggerFactory());
            await server.StartAsync();

            await using var socket = new ClientSocketConnection(Guid.NewGuid(), address.IPEndPoint, new NetworkingOptions(), new SslOptions(), new NullLoggerFactory());
            var m = new ClientMessageConnection(socket, new NullLoggerFactory());
            await socket.ConnectAsync(default);

            await socket.SendAsync(MemberConnection.ClientProtocolInitBytes, MemberConnection.ClientProtocolInitBytes.Length);

            // testing & code coverage - OnSending should *not* be used outside of tests
            var cancellation = new CancellationTokenSource();
            m.OnSending = () => cancellation.Cancel();

            await AssertEx.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await m.SendAsync(new ClientMessage(new Frame(new byte[64], (FrameFlags) ClientMessageFlags.Unfragmented)), cancellation.Token);
            });
        }

        [Test]
        public async Task SendTrue()
        {
            var socket = new TestSocketConnection(Guid.NewGuid());
            var m = new ClientMessageConnection(socket, new NullLoggerFactory());

            // cannot send a message with no frames
            await AssertEx.ThrowsAsync<ArgumentException>(async () => await m.SendAsync(new ClientMessage()));

            // can send a message with frames
            var message = new ClientMessage(new Frame(new byte[16]));
            socket.Count = 10;
            Assert.That(await m.SendAsync(message));
            Assert.That(socket.Count, Is.EqualTo(9));

            // can buffer frames
            message = new ClientMessage(new Frame(new byte[16]));
            message.Append(new Frame(new byte[16]));
            message.Append(new Frame(new byte[16]));
            socket.Count = 10;
            Assert.That(await m.SendAsync(message));
            Assert.That(socket.Count, Is.EqualTo(9));

            // can send a message with a large frame
            message = new ClientMessage(new Frame(new byte[16]));
            message.Append(new Frame(new byte[4096])); // will be sent as header, then body
            message.Append(new Frame(new byte[16]));
            socket.Count = 10;
            Assert.That(await m.SendAsync(message));
            Assert.That(socket.Count, Is.EqualTo(6));

            // can send a message with frames
            message = new ClientMessage(new Frame(new byte[768]));
            message.Append(new Frame(new byte[256]));
            socket.Count = 10;
            Assert.That(await m.SendAsync(message));
            Assert.That(socket.Count, Is.EqualTo(8));
        }

        [Test]
        public async Task SendFalse()
        {
            var socket = new TestSocketConnection(Guid.NewGuid());
            var m = new ClientMessageConnection(socket, new NullLoggerFactory());

            // send a message with frames
            var message = new ClientMessage(new Frame(new byte[16]));
            socket.Count = 0;
            Assert.That(await m.SendAsync(message), Is.False);

            // send a message with a large frame
            message = new ClientMessage(new Frame(new byte[16]));
            message.Append(new Frame(new byte[4096])); // will be sent as header, then body
            socket.Count = 0; // fail while flushing the buffer
            Assert.That(await m.SendAsync(message), Is.False);

            // send a message with a large frame
            message = new ClientMessage(new Frame(new byte[16]));
            message.Append(new Frame(new byte[4096])); // will be sent as header, then body
            socket.Count = 1; // failing while writing big frame header
            Assert.That(await m.SendAsync(message), Is.False);

            // send a message with a large frame
            message = new ClientMessage(new Frame(new byte[16]));
            message.Append(new Frame(new byte[4096])); // will be sent as header, then body
            socket.Count = 1; // failing while writing big frame body
            Assert.That(await m.SendAsync(message), Is.False);

            // send a message with frames
            message = new ClientMessage(new Frame(new byte[768]));
            message.Append(new Frame(new byte[256]));
            socket.Count = 0; // failing while flushing the buffer
            Assert.That(await m.SendAsync(message), Is.False);

            // send a message with frames
            message = new ClientMessage(new Frame(new byte[16]));
            message.Append(new Frame(new byte[16]));
            message.Append(new Frame(new byte[16]));
            socket.Count = 0; // failing while flushing the buffer
            Assert.That(await m.SendAsync(message), Is.False);
        }

        [Test]
        public async Task SendExceptions()
        {
            var socket = new TestSocketConnection(Guid.NewGuid());
            socket.Count = int.MaxValue;

            var message = new ClientMessage(new Frame(new byte[16]));

            var s = new TestSemaphore(() => throw new ObjectDisposedException("semaphore"));
            var m = new ClientMessageConnection(socket, s, new NullLoggerFactory());

            Assert.That(await m.SendAsync(message), Is.False);

            s = new TestSemaphore(() => throw new NullReferenceException());
            m = new ClientMessageConnection(socket, s, new NullLoggerFactory());

            await AssertEx.ThrowsAsync<NullReferenceException>(async () => await m.SendAsync(message));

            var c = new CancellationTokenSource();
            s = new TestSemaphore(() =>
            {
                c.Cancel();
                throw new NullReferenceException();
            });
            m = new ClientMessageConnection(socket, s, new NullLoggerFactory());

            await AssertEx.ThrowsAsync<OperationCanceledException>(async () => await m.SendAsync(message, c.Token));

            s = new TestSemaphore(() => throw new OperationCanceledException());
            m = new ClientMessageConnection(socket, s, new NullLoggerFactory());

            await AssertEx.ThrowsAsync<OperationCanceledException>(async () => await m.SendAsync(message));
        }

        [Test]
        public async Task GitHubIssue661()
        {
            // if the socket connection drops while we are sending frames, it's going to report that
            // it is shutting down to MemberConnection, which in turns will terminate and dispose the
            // message connection - and the SendAsync operation should *not* throw but return false.

            // in order to simulate a socket connection drop, we use a special test socket connection
            // that will run some code before actually sending, and we use that code to dispose the
            // message connection.

            // issue #661: this test fails, messageConnection.SendAsync throws an ObjectDisposedException.

            ClientMessageConnection messageConnection = null;
            var socketConnection = new TestSocketConnection2(() => messageConnection.DisposeAsync()); ;
            messageConnection = new ClientMessageConnection(socketConnection, new NullLoggerFactory());

            var message = new ClientMessage(new Frame(new byte[16]));
            var sent = await messageConnection.SendAsync(message);

            Assert.That(sent, Is.False);
        }

        private class TestSemaphore : IHSemaphore
        {
            private readonly Action _waitAsync;

            public TestSemaphore(Action waitAsync)
            {
                _waitAsync = waitAsync;
            }

            public void Dispose()
            { }

            public Task WaitAsync(CancellationToken cancellationToken)
            {
                _waitAsync();
                return Task.CompletedTask;
            }

            public void Release()
            { }
        }

        private class TestSocketConnection : SocketConnectionBase
        {
            public TestSocketConnection(Guid id, int prefixLength = 0)
                : base(id, prefixLength)
            { }

            public int Count { get; set; }

            public override ValueTask<bool> SendAsync(byte[] bytes, int length, CancellationToken cancellationToken = default)
            {
                return new ValueTask<bool>(Count-- > 0);
            }

            public override ValueTask FlushAsync()
            {
                return new ValueTask();
            }
        }

        private class TestSocketConnection2 : SocketConnectionBase
        {
            private readonly Func<ValueTask> _sending;

            public TestSocketConnection2(Func<ValueTask> sending)
                : base(Guid.NewGuid())
            {
                _sending = sending;
            }

            public override async ValueTask<bool> SendAsync(byte[] bytes, int length, CancellationToken cancellationToken = default)
            {
                await _sending();
                return await base.SendAsync(bytes, length, cancellationToken);
            }
        }
    }
}
