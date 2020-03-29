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
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace AsyncTests1.Networking
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
        private readonly string _hostname;
        private readonly int _port;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientSocketConnection"/> class.
        /// </summary>
        /// <param name="hostname">The server hostname.</param>
        /// <param name="port">The server port.</param>
        /// <param name="onReceiveMessageBytes">An action to execute when receiving a message.</param>
        /// <param name="multithread">Whether this connection should manage multi-threading.</param>
        /// <remarks>
        /// <para>The <paramref name="onReceiveMessageBytes"/> action must process the content of the
        /// bytes sequence before it returns. The memory associated with the sequence is not
        /// guaranteed to remain available after the action has returned.</para>
        /// </remarks>
        public ClientSocketConnection(string hostname, int port, Func<SocketConnection, ReadOnlySequence<byte>, ValueTask> onReceiveMessageBytes, bool multithread = true)
            : base(onReceiveMessageBytes, multithread)
        {
            if (string.IsNullOrWhiteSpace(hostname))
                throw new ArgumentException("Value cannot be null nor empty.", nameof(hostname));
            _hostname = hostname;

            if (port <= 0)
                throw new ArgumentOutOfRangeException(nameof(port), "Value must be greater than zero.");
            _port = port;
        }

        /// <summary>
        /// Opens the connection.
        /// </summary>
        /// <returns>A task that will complete when the connection has been opened.</returns>
        public async ValueTask OpenAsync()
        {
            Log.WriteLine("Open");

            // create the socket
            // TODO: directly work with IPs
            var host = Dns.GetHostEntry(_hostname);
            var ipAddress = host.AddressList[0];
            var socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // connect to server
            Log.WriteLine("Connect to server");
            var endpoint = new IPEndPoint(ipAddress, _port);
            await socket.ConnectAsync(endpoint);
            Log.WriteLine("Connected to server");

            // use a stream, because we may use SSL and require an SslStream
            // TODO: implement SSL or provide a Func<Stream, Stream>
            var stream = new NetworkStream(socket, false);

            // wire the pipe
            OpenPipe(socket, stream);

            Log.WriteLine("Opened");
        }
    }
}