﻿// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Networking;
using Hazelcast.Testing;
using Hazelcast.Testing.Networking;
using NUnit.Framework;

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
            var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), TestEndPointPort.GetNext());
            using var server = new SocketListener(endpoint, SocketListenerMode.AcceptOnce);
            using var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            await socket.ConnectAsync(endpoint, -1);
            // connected!
        }

        [Test]
        public async Task ConnectAsyncConnectionRefused1()
        {
            var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), TestEndPointPort.GetNext());
            using var server = new SocketListener(endpoint, SocketListenerMode.ConnectionRefused);
            using var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            await AssertEx.ThrowsAsync<SocketException>(async () =>
            {
                // socket exception, connection refused
                await socket.ConnectAsync(endpoint, -1);
            });
        }

        [Test]
        public async Task ConnectAsyncConnectionRefused2()
        {
            var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), TestEndPointPort.GetNext());
            using var server = new SocketListener(endpoint, SocketListenerMode.ConnectionRefused);
            using var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            await AssertEx.ThrowsAsync<SocketException>(async () =>
            {
                // socket exception, connection refused
                await socket.ConnectAsync(endpoint, -1, CancellationToken.None);
            });
        }

        [Test]
        public async Task ConnectAsyncConnectionRefused3()
        {
            var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), TestEndPointPort.GetNext());
            using var server = new SocketListener(endpoint, SocketListenerMode.ConnectionRefused);
            using var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            await AssertEx.ThrowsAsync<SocketException>(async () =>
            {
                // socket exception, connection refused
                await socket.ConnectAsync(endpoint, CancellationToken.None);
            });
        }

        [Test]
        public async Task ConnectAsyncTimeout1()
        {
            // use an external IP that is not going to answer on port 5701
            var endpoint = NetworkAddress.Parse("8.8.8.8:5701").IPEndPoint;
            using var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            await AssertEx.ThrowsAsync<TimeoutException>(async () =>
            {
                // no time for connection to be refused, timeout exception
                await socket.ConnectAsync(endpoint, 1);
            });
        }

        [Test]
        public async Task ConnectAsyncTimeout2()
        {
            // use an external IP that is not going to answer on port 5701
            var endpoint = NetworkAddress.Parse("8.8.8.8:5701").IPEndPoint;
            using var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            await AssertEx.ThrowsAsync<TimeoutException>(async () =>
            {
                // no time for connection to be refused, timeout exception
                await socket.ConnectAsync(endpoint, 1, CancellationToken.None);
            });
        }

        [Test]
        public async Task ConnectAsyncCanceled()
        {
            var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5701);

            using var server = new SocketListener(endpoint, SocketListenerMode.AcceptOnce);

            using var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            var i = 0;
            var m = "";
            const int count = 5;
            while (i++ < count)
            {
                try
                {
                    await socket.ConnectAsync(endpoint, -1, new CancellationToken(true));
                }
                catch (OperationCanceledException e)
                {
                    m = e.Message;
                    break;
                }

                socket.Disconnect(true);
                await Task.Delay(100);
            }

            // fail
            if (i == count)
                Assert.Throws<OperationCanceledException>(() => { }, m);

            //Assert.ThrowsAsync<OperationCanceledException>(async () =>
            //{
            //    await socket.ConnectAsync(endpoint, -1, new CancellationToken(true));
            //});
        }
    }
}
