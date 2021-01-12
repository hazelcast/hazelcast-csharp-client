// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.Networking;

namespace Hazelcast.Testing.TestServer
{
    /// <summary>
    /// Represents a server socket connection.
    /// </summary>
    /// <remarks>
    /// <para>The server socket connection handles a client and its message
    /// bytes, and manages the network socket. It is used by the server.</para>
    /// </remarks>
    internal class ServerSocketConnection : SocketConnectionBase
    {
        // note: the socket is disposed by the SocketConnection parent class
        private readonly Socket _acceptingSocket;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerSocketConnection"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the connection.</param>
        /// <param name="socket">The underlying network socket.</param>
        /// <param name="hcname">An HConsole name complement.</param>
        public ServerSocketConnection(int id, Socket socket, string hcname)
            : base(id)
        {
            _acceptingSocket = socket ?? throw new ArgumentNullException(nameof(socket));

            var prefix = "CONN.SERVER".Dot(hcname);
            HConsole.Configure(x => x
                .Set(this, xx => xx.SetIndent(32).SetPrefix($"{prefix} [{id}]")));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerSocketConnection"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the connection.</param>
        /// <param name="socket">The underlying network socket.</param>
        public ServerSocketConnection(int id, Socket socket)
            : this(id, socket, string.Empty)
        { }

        /// <summary>
        /// Connects to the client.
        /// </summary>
        /// <remarks>
        /// <para>The connection can only be established after its <see cref="SocketConnectionBase.OnReceiveMessageBytes"/> handler
        /// has been set. If the handler has not been set, an exception is thrown.</para>
        /// </remarks>
        public void Accept()
        {
            HConsole.WriteLine(this, "Connect");

            EnsureCanOpenPipe();

            // use a stream, because we may use SSL and require an SslStream
            // TODO implement SSL or provide a Func<Stream, Stream>
            var stream = new NetworkStream(_acceptingSocket, false);

            // wire the pipe
            OpenPipe(_acceptingSocket, stream);

            HConsole.WriteLine(this, "Connected");
        }
    }
}
