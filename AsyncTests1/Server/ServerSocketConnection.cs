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
using System.Net.Sockets;
using AsyncTests1.Logging;
using AsyncTests1.Networking;

namespace AsyncTests1.Server
{
    /// <summary>
    /// Represents a server socket connection.
    /// </summary>
    /// <remarks>
    /// <para>The server socket connection handles a client and its message
    /// bytes, and manages the network socket. It is used by the server.</para>
    /// </remarks>
    public class ServerSocketConnection : SocketConnection
    {
        private readonly Socket _socket;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerSocketConnection"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the connection.</param>
        /// <param name="socket">The underlying network socket.</param>
        public ServerSocketConnection(int id, Socket socket)
            : base(id)
        {
            _socket = socket ?? throw new ArgumentNullException(nameof(socket));
            XConsole.Setup(this, 32, "SVR.CON({id})");
        }

        /// <summary>
        /// Connects to the client.
        /// </summary>
        /// <remarks>
        /// <para>The connection can only be established after its <see cref="SocketConnection.OnReceiveMessageBytes"/> handler
        /// has been set. If the handler has not been set, an exception is thrown.</para>
        /// </remarks>
        public void Accept()
        {
            XConsole.WriteLine(this, "Connect");

            if (OnReceiveMessageBytes == null)
                throw new InvalidOperationException("No message bytes handler has been configured.");

            // use a stream, because we may use SSL and require an SslStream
            // TODO implement SSL or provide a Func<Stream, Stream>
            var stream = new NetworkStream(_socket, false);

            // wire the pipe
            OpenPipe(_socket, stream);

            XConsole.WriteLine(this, "Connected");
        }
    }
}