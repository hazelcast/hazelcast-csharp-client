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
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Data;
using Hazelcast.Messaging;
using Hazelcast.Networking;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Testing.TestServer
{
    /// <summary>
    /// Represents a test server.
    /// </summary>
    public class Server : IAsyncDisposable
    {
        private readonly Dictionary<int, ServerSocketConnection> _connections = new Dictionary<int, ServerSocketConnection>();
        private readonly Func<ClientMessageConnection, ClientMessage, ValueTask> _handler;
        private readonly ILoggerFactory _loggerFactory;
        private readonly NetworkAddress _address;
        private readonly IPEndPoint _endpoint;
        private readonly Guid _clusterId;
        private readonly Guid _memberId;
        private ServerSocketListener _listener;

        /// <summary>
        /// Initializes a new instance of the <see cref="Server"/> class.
        /// </summary>
        /// <param name="address">The socket network address.</param>
        /// <param name="handler">A handler for incoming messages.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public Server(NetworkAddress address, Func<ClientMessageConnection, ClientMessage, ValueTask> handler, ILoggerFactory loggerFactory)
        {
            _address = address;
            _endpoint = address.IPEndPoint;
            _handler = handler;
            _loggerFactory = loggerFactory;

            _clusterId = Guid.NewGuid();
            _memberId = Guid.NewGuid();

            HzConsole.Configure(this, config => config.SetIndent(20).SetPrefix("SERVER"));
        }

        /// <summary>
        /// Starts the server.
        /// </summary>
        /// <returns>A task that will complete when the server has started.</returns>
        public async Task StartAsync()
        {
            HzConsole.WriteLine(this, $"Start server at {_endpoint}");

            _listener = new ServerSocketListener(_endpoint) { OnAcceptConnection = AcceptConnection, OnShutdown = ListenerShutdown};
            HzConsole.Configure(_listener, config => config.SetIndent(24).SetPrefix("LISTENER"));
            await _listener.StartAsync().CAF();

            HzConsole.WriteLine(this, "Server started");
        }

        private async ValueTask ListenerShutdown(ServerSocketListener arg)
        {
            HzConsole.WriteLine(this, "Listener is down");

            // shutdown all existing connections
            foreach (var connection in _connections.Values)
            {
                try
                {
                    await connection.DisposeAsync().CAF();
                }
                catch { /* ignore */ }
            }

            HzConsole.WriteLine(this, "Connections are down");
        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        /// <returns>A task that will complete when the server has stopped.</returns>
        public async Task StopAsync()
        {
            var listener = _listener;
            _listener = null;

            if (listener == null) return;

            HzConsole.WriteLine(this, "Stop server");

            // stop accepting new connections
            await listener.StopAsync().CAF();
            await listener.DisposeAsync().CAF();

            HzConsole.WriteLine(this, "Server stopped");
        }

        /// <summary>
        /// Handles new connections.
        /// </summary>
        /// <param name="serverConnection">The new connection.</param>
        private void AcceptConnection(ServerSocketConnection serverConnection)
        {
            // the connection we receive is not wired yet
            // must wire it properly before accepting

            var messageConnection = new ClientMessageConnection(serverConnection, _loggerFactory) { OnReceiveMessage = ReceiveMessage };
            HzConsole.Configure(messageConnection, config => config.SetIndent(28).SetPrefix("MSG.SERVER"));
            serverConnection.OnShutdown = SocketShutdown;
            serverConnection.ExpectPrefixBytes(3, ReceivePrefixBytes);
            serverConnection.Accept();
            _connections[serverConnection.Id] = serverConnection;
        }

        protected async Task SendAsync(ClientMessageConnection connection, ClientMessage eventMessage, long correlationId)
        {
            eventMessage.CorrelationId = correlationId;
            eventMessage.Flags |= ClientMessageFlags.BeginFragment | ClientMessageFlags.EndFragment;
            await connection.SendAsync(eventMessage).CAF();
        }

        private async ValueTask ReceiveMessage(ClientMessageConnection connection, ClientMessage requestMessage)
        {
            var correlationId = requestMessage.CorrelationId;

            switch (requestMessage.MessageType)
            {
                // handle authentication
                case ClientAuthenticationServerCodec.RequestMessageType:
                {
                    var request = ClientAuthenticationServerCodec.DecodeRequest(requestMessage);
                    var responseMessage = ClientAuthenticationServerCodec.EncodeResponse(
                        0, _address, _memberId, SerializationService.SerializerVersion,
                        "4.0", 1, _clusterId, false);
                    await SendAsync(connection, responseMessage, correlationId).CAF();
                    break;
                }

                // handle events
                case ClientAddClusterViewListenerServerCodec.RequestMessageType:
                {
                    var request = ClientAddClusterViewListenerServerCodec.DecodeRequest(requestMessage);
                    var responseMessage = ClientAddClusterViewListenerServerCodec.EncodeResponse();
                    await SendAsync(connection, responseMessage, correlationId).CAF();

                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(500).CAF();
                        var eventMessage = ClientAddClusterViewListenerServerCodec.EncodeMembersViewEvent(1, new[]
                        {
                            new MemberInfo(_memberId, _address, new MemberVersion(4, 0, 0), false, new Dictionary<string, string>()),
                        });
                        await SendAsync(connection, eventMessage, correlationId).CAF();
                    });
                    break;
                }

                // handle others
                default:
                    await _handler(connection, requestMessage).CAF();
                    break;
            }
        }

        private static ValueTask ReceivePrefixBytes(SocketConnectionBase connection, ReadOnlySequence<byte> bytes)
        {
            // do nothing for now - just accept them
            return new ValueTask();
        }

        /// <summary>
        /// Handles a connection shutdown.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns>A task that will complete when the connection shutdown has been handled.</returns>
        private void SocketShutdown(SocketConnectionBase connection)
        {
            HzConsole.WriteLine(this, "Removing connection " + connection.Id);
            _connections.Remove(connection.Id);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await StopAsync().CAF();
        }
    }
}
