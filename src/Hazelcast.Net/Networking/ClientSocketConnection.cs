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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Networking
{
    /// <summary>
    /// Represents a client socket connection.
    /// </summary>
    /// <remarks>
    /// <para>The client socket connection connects to the server, handle message
    /// bytes, and manages the network socket. It is used by the client connection.</para>
    /// </remarks>
    internal class ClientSocketConnection : SocketConnectionBase
    {
        private readonly IPEndPoint _endpoint;
        private readonly SocketOptions _socketOptions;
        private readonly SslOptions _sslOptions;
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientSocketConnection"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the connection.</param>
        /// <param name="endpoint">The socket endpoint.</param>
        /// <param name="options">Socket options.</param>
        /// <param name="sslOptions">SSL options.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        /// <param name="prefixLength">An optional prefix length.</param>
        public ClientSocketConnection(int id, IPEndPoint endpoint, SocketOptions options, SslOptions sslOptions, ILoggerFactory loggerFactory, int prefixLength = 0)
            : base(id, prefixLength)
        {
            _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            _socketOptions = options ?? throw new ArgumentNullException(nameof(options));
            _sslOptions = sslOptions ?? throw new ArgumentNullException(nameof(sslOptions));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            HConsole.Configure(this, config => config.SetIndent(16).SetPrefix($"CONN.CLIENT [{id}]"));
        }

        /// <summary>
        /// Connect to the server.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the connection has been established.</returns>
        /// <remarks>
        /// <para>The connection can only be established after its <see cref="SocketConnectionBase.OnReceiveMessageBytes"/> handler
        /// has been set. If the handler has not been set, an exception is thrown.</para>
        /// </remarks>
        public async ValueTask ConnectAsync(CancellationToken cancellationToken)
        {
            HConsole.WriteLine(this, "Open");

            if (OnReceiveMessageBytes == null)
                throw new InvalidOperationException("No message bytes handler has been configured.");

            // create the socket
            var socket = new Socket(_endpoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, _socketOptions.KeepAlive);
            socket.NoDelay = _socketOptions.TcpNoDelay;
            socket.ReceiveBufferSize = socket.SendBufferSize = _socketOptions.BufferSizeKiB * 1024;

            socket.LingerState = _socketOptions.LingerSeconds > 0
                ? new LingerOption(true, _socketOptions.LingerSeconds)
                : new LingerOption(false, 1);

            // connect to server
            HConsole.WriteLine(this, "Connect to server");
            await socket.ConnectAsync(_endpoint, _socketOptions.ConnectionTimeoutMilliseconds, cancellationToken).CAF();
            HConsole.WriteLine(this, "Connected to server");

            // use a stream, because we may use SSL and require an SslStream
#pragma warning disable CA2000 // Dispose objects before losing scope - transferred to OpenPipe
            Stream stream = new NetworkStream(socket, false);
#pragma warning restore CA2000
            if (_sslOptions.Enabled)
            {
                stream = await new SslLayer(_sslOptions, _loggerFactory).GetStreamAsync(stream).CAF();
            }

            // wire the pipe
            OpenPipe(socket, stream);

            HConsole.WriteLine(this, "Opened");
        }
    }
}
