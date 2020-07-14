using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Hazelcast.Core;
using Hazelcast.Networking;
using Hazelcast.Testing.Networking;

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
        public async Task ConnectAsyncSuccess()
        {
            var endpoint = IPEndPointEx.Parse("127.0.0.1:11000");

            using var server = new SocketListener(endpoint, SocketListenerMode.AcceptOnce);

            using var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            await socket.ConnectAsync(endpoint, -1);
            // connected!

            try
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                socket.Dispose();
            }
            catch { /* doesn't matter */ }
        }

        [Test]
        public void ConnectAsyncConnectionRefused1()
        {
            var endpoint = IPEndPointEx.Parse("127.0.0.1:11000");

            using var server = new SocketListener(endpoint, SocketListenerMode.ConnectionRefused);

            using var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // for some reason netstandard throws System.Net.Internals.SocketExceptionFactory+ExtendedSocketException
            // which derives from SocketException - use a constraint so NUnit is happy

            Assert.ThrowsAsync(Is.InstanceOf<SocketException>(), async () =>
            {
                // socket exception, connection refused
                await socket.ConnectAsync(endpoint, -1);
            });
        }

        [Test]
        public void ConnectAsyncConnectionRefused2()
        {
            var endpoint = IPEndPointEx.Parse("127.0.0.1:11000");

            using var server = new SocketListener(endpoint, SocketListenerMode.ConnectionRefused);

            using var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // for some reason netstandard throws System.Net.Internals.SocketExceptionFactory+ExtendedSocketException
            // which derives from SocketException - use a constraint so NUnit is happy

            Assert.ThrowsAsync(Is.InstanceOf<SocketException>(), async () =>
            {
                // socket exception, connection refused
                await socket.ConnectAsync(endpoint, -1, CancellationToken.None);
            });
        }

        [Test]
        public void ConnectAsyncConnectionRefused3()
        {
            var endpoint = IPEndPointEx.Parse("127.0.0.1:11000");

            using var server = new SocketListener(endpoint, SocketListenerMode.ConnectionRefused);
            using var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // for some reason netstandard throws System.Net.Internals.SocketExceptionFactory+ExtendedSocketException
            // which derives from SocketException - use a constraint so NUnit is happy

            Assert.ThrowsAsync(Is.InstanceOf<SocketException>(), async () =>
            {
                // socket exception, connection refused
                await socket.ConnectAsync(endpoint, CancellationToken.None);
            });
        }

        [Test]
        public void ConnectAsyncTimeout1()
        {
            var endpoint = NetworkAddress.Parse("www.hazelcast.com:5701").IPEndPoint;
            using var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                // no time for connection to be refused, timeout exception
                await socket.ConnectAsync(endpoint, 1);
            });
        }

        [Test]
        public void ConnectAsyncTimeout2()
        {
            var endpoint = NetworkAddress.Parse("www.hazelcast.com:5701").IPEndPoint;
            using var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                // no time for connection to be refused, timeout exception
                await socket.ConnectAsync(endpoint, 1, CancellationToken.None);
            });
        }

        [Test]
        public void ConnectAsyncCanceled()
        {
            var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5701);

            using var server = new SocketListener(endpoint, SocketListenerMode.AcceptOnce);

            using var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await socket.ConnectAsync(endpoint, -1, new CancellationToken(true));
            });
        }
    }
}
