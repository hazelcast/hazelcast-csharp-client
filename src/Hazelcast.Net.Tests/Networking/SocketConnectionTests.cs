using System;
using System.Net;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Networking;
using Hazelcast.Testing.Networking;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Hazelcast.Tests.Networking
{
    [TestFixture]
    public class SocketConnectionTests
    {
        [Test]
        public async Task Exceptions()
        {
            var endpoint = NetworkAddress.Parse("127.0.0.1:5701").IPEndPoint;
            var options = new SocketOptions();

            await using var connection = new ClientSocketConnection(0, endpoint, options, 3);

            // OnReceiveMessageBytes is missing
            Assert.ThrowsAsync<InvalidOperationException>(async () => await connection.ConnectAsync(default));

            using var server = new SocketListener(endpoint, SocketListenerMode.AcceptOnce);

            connection.OnReceiveMessageBytes = (x, y) => new ValueTask<bool>(true);

            // OnReceivePrefixBytes is missing
            Assert.ThrowsAsync<InvalidOperationException>(async () => await connection.ConnectAsync(default));

            await using var connection2 = new ClientSocketConnection(0, endpoint, options);

            connection2.OnReceiveMessageBytes = (x, y) => new ValueTask<bool>(true);
            await connection2.ConnectAsync(default);

            Assert.Throws<InvalidOperationException>(() => connection2.OnReceiveMessageBytes = (x, y) => new ValueTask<bool>(true));
            Assert.Throws<InvalidOperationException>(() => connection2.OnReceivePrefixBytes = (x, y) => new ValueTask());
            Assert.Throws<InvalidOperationException>(() => connection2.OnShutdown = x => { });
        }

        [Test]
        public async Task ConnectAsyncSuccess()
        {
            var endpoint = IPEndPointEx.Parse("127.0.0.1:11000");
            var options = new SocketOptions();

            using var server = new SocketListener(endpoint, SocketListenerMode.AcceptOnce);

            await using var socket = new ClientSocketConnection(0, endpoint, options);
            socket.OnReceiveMessageBytes = (x, y) => new ValueTask<bool>(true);
            await socket.ConnectAsync(default);
            // connected!
        }

        [Test]
        public async Task ConnectAsyncSuccessDontLinger()
        {
            var endpoint = IPEndPointEx.Parse("127.0.0.1:11000");
            var options = new SocketOptions { LingerSeconds = 0 };

            using var server = new SocketListener(endpoint, SocketListenerMode.AcceptOnce);

            await using var socket = new ClientSocketConnection(0, endpoint, options);
            socket.OnReceiveMessageBytes = (x, y) => new ValueTask<bool>(true);
            await socket.ConnectAsync(default);
            // connected!
        }

        [Test]
        public async Task SendReceive()
        {
            static async ValueTask ServerHandler(Hazelcast.Testing.TestServer.Server s, ClientMessageConnection c, ClientMessage m)
            {
                await c.SendAsync(new ClientMessage(new Frame(new byte[64])));
            }

            var now = DateTime.Now;

            var address = NetworkAddress.Parse("127.0.0.1:11000");
            await using var server = new Hazelcast.Testing.TestServer.Server(address, ServerHandler, new NullLoggerFactory());
            await server.StartAsync();

            await using var socket = new ClientSocketConnection(0, address.IPEndPoint, new SocketOptions());
            var m = new ClientMessageConnection(socket, new NullLoggerFactory());
            await socket.ConnectAsync(default);

            await Task.Delay(100);
            await socket.SendAsync(MemberConnection.ClientProtocolInitBytes, MemberConnection.ClientProtocolInitBytes.Length);
            await m.SendAsync(new ClientMessage(new Frame(new byte[64], (FrameFlags) ClientMessageFlags.Unfragmented)));
            await Task.Delay(100);

            Assert.That(socket.CreateTime, Is.GreaterThan(now));
            Assert.That(socket.LastWriteTime, Is.GreaterThan(socket.CreateTime));
            Assert.That(socket.LastReadTime, Is.GreaterThan(socket.LastWriteTime));
        }
    }
}
