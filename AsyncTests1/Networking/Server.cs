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

using System.Buffers;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AsyncTests1.Networking
{
    /// <summary>
    /// Represents a server.
    /// </summary>
    public class Server
    {
        private readonly Dictionary<int, ServerSocketConnection> _connections = new Dictionary<int, ServerSocketConnection>();

        private readonly IPEndPoint _endpoint;

        private ServerSocketListener _listener;
        private Task _listening;

        /// <summary>
        /// Initializes a new instance of the <see cref="Server"/> class.
        /// </summary>
        /// <param name="endpoint">The socket endpoint.</param>
        public Server(IPEndPoint endpoint)
        {
            _endpoint = endpoint;
            XConsole.Setup(this, 20, "SVR");
        }

        /// <summary>
        /// Starts the server.
        /// </summary>
        /// <returns>A task that will complete when the server has started.</returns>
        public async Task StartAsync()
        {
            XConsole.WriteLine(this, "Start server");

            _listener = new ServerSocketListener(_endpoint) { OnAcceptConnection = AcceptConnection, OnShutdown = ListenerShutdown};
            XConsole.Setup(_listener, 24, "LST");
            await _listener.StartAsync();

            XConsole.WriteLine(this, "Server started");
        }

        private async ValueTask ListenerShutdown(ServerSocketListener arg)
        {
            XConsole.WriteLine(this, "Listener is down");

            // shutdown all existing connections
            foreach (var connection in _connections.Values)
                await connection.ShutdownAsync();

            XConsole.WriteLine(this, "Connections are down");
        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        /// <returns>A task that will complete when the server has stopped.</returns>
        public async Task StopAsync()
        {
            XConsole.WriteLine(this, "Stop server");

            // stop accepting new connections
            await _listener.StopAsync();

            XConsole.WriteLine(this, "Server stopped");
        }

        /// <summary>
        /// Handles new connections.
        /// </summary>
        /// <param name="serverConnection">The new connection.</param>
        private void AcceptConnection(ServerSocketConnection serverConnection)
        {
            // the connection we receive is not wired yet
            // must wire it properly before accepting

            var messageConnection = new MessageConnection(serverConnection) { OnReceiveMessage = ReceiveMessage };
            XConsole.Setup(messageConnection, 28, "SVR.MSG");
            serverConnection.OnShutdown = SocketShutdown;
            serverConnection.ExpectPrefixBytes(3, ReceivePrefixBytes);
            serverConnection.Accept();
            _connections[serverConnection.Id] = serverConnection;
        }

        private ValueTask ReceivePrefixBytes(SocketConnection connection, ReadOnlySequence<byte> bytes)
        {
            // do nothing for now - just accept them
            return new ValueTask();
        }

        /// <summary>
        /// Handles a connection shutdown.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns>A task that will complete when the connection shutdown has been handled.</returns>
        private ValueTask SocketShutdown(SocketConnection connection)
        {
            XConsole.WriteLine(this, "Removing connection " + connection.Id);
            _connections.Remove(connection.Id);
            return new ValueTask();
        }

        /// <summary>
        /// Handles messages.
        /// </summary>
        /// <param name="connection">The connection receiving the message.</param>
        /// <param name="message">The message.</param>
        /// <returns>A task that will complete when the message has been handled.</returns>
        private async ValueTask ReceiveMessage(MessageConnection connection, Message message)
        {
            XConsole.WriteLine(this, "Respond");
            var text = Encoding.UTF8.GetString(message.FirstFrame.Bytes);
            var responseText = text switch
            {
                "a" => "alpha",
                "b" => "bravo",
                _ => "wut?"
            };

            var response = new Message()
                //.Append(Frame2.Begin)
                .Append(new Frame(FrameFlags.Begin, new byte[64])) // header stuff
                .Append(new Frame(FrameFlags.End | FrameFlags.Final, Encoding.UTF8.GetBytes(responseText)));
            //.Append(Frame2.End); // FIXME is it final too?

            response.CorrelationId = message.CorrelationId;

            await connection.SendAsync(response);
            XConsole.WriteLine(this, "Responded");
        }
    }
}
