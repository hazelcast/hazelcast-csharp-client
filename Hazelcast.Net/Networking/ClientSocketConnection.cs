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
using System.Net;
using System.Net.Sockets;
using Hazelcast.Logging;

namespace Hazelcast.Networking
{
    /// <summary>
    /// Represents a client socket connection.
    /// </summary>
    /// <remarks>
    /// <para>The client socket connection connects to the server, handle message
    /// bytes, and manages the network socket. It is used by the client connection.</para>
    /// </remarks>
    public class ClientSocketConnection : SocketConnection
    {
        private readonly IPEndPoint _endpoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientSocketConnection"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the connection.</param>
        /// <param name="endpoint">The socket endpoint.</param>
        /// <param name="prefixLength">An optional prefix length.</param>
        /// <param name="multithread">Whether this connection should manage multi-threading.</param>
        public ClientSocketConnection(int id, IPEndPoint endpoint, int prefixLength = 0, bool multithread = true)
            : base(id, prefixLength, multithread)
        {
            _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            XConsole.Setup(this, 16, "CLT.CON({id})");
        }

        /// <summary>
        /// Connect to the server.
        /// </summary>
        /// <returns>A task that will complete when the connection has been established.</returns>
        /// <remarks>
        /// <para>The connection can only be established after its <see cref="SocketConnection.OnReceiveMessageBytes"/> handler
        /// has been set. If the handler has not been set, an exception is thrown.</para>
        /// </remarks>
        public async ValueTask ConnectAsync()
        {
            XConsole.WriteLine(this, "Open");

            if (OnReceiveMessageBytes == null)
                throw new InvalidOperationException("No message bytes handler has been configured.");

            // create the socket
            var socket = new Socket(_endpoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // connect to server
            XConsole.WriteLine(this, "Connect to server");
            await socket.ConnectAsync(_endpoint);
            XConsole.WriteLine(this, "Connected to server");

            // use a stream, because we may use SSL and require an SslStream
            // TODO implement SSL or provide a Func<Stream, Stream>
            var stream = new NetworkStream(socket, false);

            // wire the pipe
            OpenPipe(socket, stream);

            XConsole.WriteLine(this, "Opened");
        }
    }
}