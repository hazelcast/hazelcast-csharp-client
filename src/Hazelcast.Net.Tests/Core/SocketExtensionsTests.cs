using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Hazelcast.Core;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class SocketExtensionsTests
    {
        [Test]
        public void ArgumentExceptions()
        {
            var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5701);

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await ((Socket) null).ConnectAsync(endpoint, -1);
            });
        }

        [Test]
        public async Task Success()
        {
            static void AcceptCallback(IAsyncResult result)
            {
                var l = (Socket) result.AsyncState;
                var h = l.EndAccept(result);
            }

            var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);
            var listener = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(endpoint);
            listener.Listen(10);
            listener.BeginAccept(AcceptCallback, listener);

            var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            await socket.ConnectAsync(endpoint, -1);

            // connected!

            try
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                socket.Dispose();
            }
            catch { /* doesn't matter */ }

            try
            {
                listener.Shutdown(SocketShutdown.Both);
                listener.Close();
                listener.Dispose();
            }
            catch { /* doesn't matter */ }
        }

        [Test]
        public void Throw1()
        {
            var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5701);
            var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Assert.ThrowsAsync<SocketException>(async () =>
            {
                // socket exception, connection refused
                await socket.ConnectAsync(endpoint, -1);
            });
        }

        [Test]
        public void Throw2()
        {
            var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5701);
            var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Assert.ThrowsAsync<SocketException>(async () =>
            {
                // socket exception, connection refused
                await socket.ConnectAsync(endpoint, -1, CancellationToken.None);
            });
        }

        [Test]
        public void Throw2Default()
        {
            var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5701);
            var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Assert.ThrowsAsync<SocketException>(async () =>
            {
                // socket exception, connection refused
                await socket.ConnectAsync(endpoint, CancellationToken.None);
            });
        }

        [Test]
        public void Throw3()
        {
            var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5701);
            var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                // no time for connection to be refused, timeout exception
                await socket.ConnectAsync(endpoint, 1);
            });
        }

        [Test]
        public void Throw4()
        {
            var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5701);
            var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                // no time for connection to be refused, timeout exception
                await socket.ConnectAsync(endpoint, 1, CancellationToken.None);
            });
        }

        [Test]
        public void Throw5()
        {
            var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5701);
            var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await socket.ConnectAsync(endpoint, -1, new CancellationToken(true));
            });
        }
    }
}
