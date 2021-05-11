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
using System.Buffers;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Networking;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Testing.TestServer
{
    /// <summary>
    /// Represents a test server.
    /// </summary>
    internal class Server : IAsyncDisposable
    {
        private readonly object _openLock = new object();
        private readonly ConcurrentDictionary<Guid, ServerSocketConnection> _connections = new ConcurrentDictionary<Guid, ServerSocketConnection>();
        private readonly Func<Server, ClientMessageConnection, ClientMessage, ValueTask> _handler;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IPEndPoint _endpoint;
        private readonly object _handlerLock = new object();
        private readonly string _hcname;
        private Task _handlerTask;
        private ServerSocketListener _listener;
        private bool _open;

        /// <summary>
        /// Initializes a new instance of the <see cref="Server"/> class.
        /// </summary>
        /// <param name="address">The socket network address.</param>
        /// <param name="handler">A handler for incoming messages.</param>
        /// <param name="state">A server-state object.</param>
        /// <param name="hcname">An HConsole name complement.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public Server(NetworkAddress address, Func<Server, ClientMessageConnection, ClientMessage, ValueTask> handler, ILoggerFactory loggerFactory, object state = null, string hcname = "")
        {
            Address = address;
            State = state;
            _endpoint = address.IPEndPoint;
            _handler = handler;
            _loggerFactory = loggerFactory;
            _hcname = hcname;

            ClusterId = Guid.NewGuid();
            MemberId = Guid.NewGuid();

            HConsole.Configure(x => x.Configure(this).SetIndent(20).SetPrefix("SERVER".Dot(_hcname)));
        }

        /// <summary>
        /// Gets the address of the server.
        /// </summary>
        public NetworkAddress Address { get; }

        /// <summary>
        /// Gets the server state object.
        /// </summary>
        public object State { get; }

        /// <summary>
        /// Gets the member identifier of the server.
        /// </summary>
        public Guid MemberId { get; set; }

        /// <summary>
        /// Gets the cluster identifier of the server.
        /// </summary>
        public Guid ClusterId { get; set; }

        /// <summary>
        /// Starts the server.
        /// </summary>
        /// <returns>A task that will complete when the server has started.</returns>
        public async Task StartAsync()
        {
            HConsole.WriteLine(this, $"Start server at {_endpoint}");

            _listener = new ServerSocketListener(_endpoint, _hcname) { OnAcceptConnection = AcceptConnection, OnShutdown = ListenerShutdown};

            _open = true;
            await _listener.StartAsync().CfAwait();

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
                    await connection.DisposeAsync().CfAwait();
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
            await listener.StopAsync().CfAwait();
            await listener.DisposeAsync().CfAwait();

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
                HConsole.Configure(x => x.Configure(messageConnection).SetIndent(28).SetPrefix("SVR.MSG".Dot(_hcname)));
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
            await connection.SendAsync(eventMessage).CfAwait();
        }

        private void ReceiveMessage(ClientMessageConnection connection, ClientMessage requestMessage)
        {
            lock (_handlerLock)
            {
                if (_handlerTask == null)
                {
                    _handlerTask = _handler(this, connection, requestMessage).AsTask();
                }
                else
                {
                    _handlerTask = _handlerTask.ContinueWith(_ => _handler(this, connection, requestMessage).AsTask()).Unwrap();
                }
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
        private ValueTask SocketShutdown(SocketConnectionBase connection)
        {
            HConsole.WriteLine(this, "Removing connection " + connection.Id);
            _connections.TryRemove(connection.Id, out _);
            return default;
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await StopAsync().CfAwait();
        }
    }
}
