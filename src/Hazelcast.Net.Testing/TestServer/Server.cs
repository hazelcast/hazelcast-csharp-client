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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
    internal class Server : IAsyncDisposable
    {
        private readonly object _openLock = new object();
        private readonly ConcurrentDictionary<int, ServerSocketConnection> _connections = new ConcurrentDictionary<int, ServerSocketConnection>();
        private readonly Func<Server, ClientMessageConnection, ClientMessage, ValueTask> _handler;
        private readonly ILoggerFactory _loggerFactory;
        private readonly NetworkAddress _address;
        private readonly IPEndPoint _endpoint;
        private readonly Guid _clusterId;
        private readonly Guid _memberId;
        private readonly object _handlerLock = new object();
        private Task _handlerTask;
        private ServerSocketListener _listener;
        private bool _open;

        /// <summary>
        /// Initializes a new instance of the <see cref="Server"/> class.
        /// </summary>
        /// <param name="address">The socket network address.</param>
        /// <param name="handler">A handler for incoming messages.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public Server(NetworkAddress address, Func<Server, ClientMessageConnection, ClientMessage, ValueTask> handler, ILoggerFactory loggerFactory)
        {
            _address = address;
            _endpoint = address.IPEndPoint;
            _handler = handler;
            _loggerFactory = loggerFactory;

            _clusterId = Guid.NewGuid();
            _memberId = Guid.NewGuid();

            HConsole.Configure(x => x
                .Set(this, xx => xx.SetIndent(20).SetPrefix("SERVER")));
        }

        /// <summary>
        /// Starts the server.
        /// </summary>
        /// <returns>A task that will complete when the server has started.</returns>
        public async Task StartAsync()
        {
            HConsole.WriteLine(this, $"Start server at {_endpoint}");

            _listener = new ServerSocketListener(_endpoint) { OnAcceptConnection = AcceptConnection, OnShutdown = ListenerShutdown};
            HConsole.Configure(x => x
                .Set(_listener, xx => xx.SetIndent(24).SetPrefix("LISTENER")));
            _open = true;
            await _listener.StartAsync().CAF();

            HConsole.WriteLine(this, "Server started");
        }

        private async ValueTask ListenerShutdown(ServerSocketListener arg)
        {
            HConsole.WriteLine(this, "Listener is down");

            lock (_openLock)
            {
                _open = false;
            }

            // shutdown all existing connections
            foreach (var connection in _connections.Values.ToList())
            {
                try
                {
                    await connection.DisposeAsync().CAF();
                }
                catch { /* ignore */ }
            }

            HConsole.WriteLine(this, "Connections are down");
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

            HConsole.WriteLine(this, "Stop server");

            // stop accepting new connections
            await listener.StopAsync().CAF();
            await listener.DisposeAsync().CAF();

            HConsole.WriteLine(this, "Server stopped");
        }

        /// <summary>
        /// Handles new connections.
        /// </summary>
        /// <param name="serverConnection">The new connection.</param>
        private void AcceptConnection(ServerSocketConnection serverConnection)
        {
            // the connection we receive is not wired yet
            // must wire it properly before accepting

            lock (_openLock)
            {
                if (!_open) throw new InvalidOperationException("Cannot accept connections (closed).");

                var messageConnection = new ClientMessageConnection(serverConnection, _loggerFactory) { OnReceiveMessage = ReceiveMessage };
                HConsole.Configure(x => x
                    .Set(messageConnection, xx => xx.SetIndent(28).SetPrefix("MSG.SERVER")));
                serverConnection.OnShutdown = SocketShutdown;
                serverConnection.ExpectPrefixBytes(3, ReceivePrefixBytes);
                serverConnection.Accept();
                _connections[serverConnection.Id] = serverConnection;
            }
        }

        protected async Task SendAsync(ClientMessageConnection connection, ClientMessage eventMessage, long correlationId)
        {
            eventMessage.CorrelationId = correlationId;
            eventMessage.Flags |= ClientMessageFlags.BeginFragment | ClientMessageFlags.EndFragment;
            await connection.SendAsync(eventMessage).CAF();
        }

        private void ReceiveMessage(ClientMessageConnection connection, ClientMessage requestMessage)
        {
            lock (_handlerLock)
            {
                if (_handlerTask == null)
                {
                    _handlerTask = ReceiveMessageAsync(connection, requestMessage);
                }
                else
                {
                    _handlerTask = _handlerTask.ContinueWith(_ => ReceiveMessageAsync(connection, requestMessage)).Unwrap();
                }
            }
        }

        private async Task ReceiveMessageAsync(ClientMessageConnection connection, ClientMessage requestMessage)
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
                    await _handler(this, connection, requestMessage).CAF();
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
            HConsole.WriteLine(this, "Removing connection " + connection.Id);
            _connections.TryRemove(connection.Id, out _);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await StopAsync().CAF();
        }
    }
}
