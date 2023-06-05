// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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

#nullable enable

using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Networking;
using Hazelcast.Protocol.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hazelcast.Testing.TestServer;

/// <summary>
/// Represents a test server.
/// </summary>
internal class Server : IAsyncDisposable
{
    private readonly object _openLock = new();
    private readonly ConcurrentDictionary<Guid, ServerSocketConnection> _connections = new();
    private readonly Dictionary<int, Func<ClientRequest, ValueTask>> _handlers = new();
    private readonly ILoggerFactory _loggerFactory;
    private readonly IPEndPoint _endpoint;
    private readonly object _handlerLock = new();
    private readonly string _hcName;
    private Func<ClientRequest, ValueTask>? _handler;
    private Task? _handling;
    private ServerSocketListener? _listener;
    private bool _open;

    /// <summary>
    /// Initializes a new instance of the <see cref="Server"/> class.
    /// </summary>
    /// <param name="address">The socket network address.</param>
    /// <param name="hcName">An HConsole name complement.</param>
    /// <param name="loggerFactory">A logger factory.</param>
    public Server(NetworkAddress address, ILoggerFactory? loggerFactory = null, string hcName = "")
    {
        Address = address;
        _endpoint = address.IPEndPoint;
        _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
        _hcName = hcName;

        ClusterId = Guid.NewGuid();
        MemberId = Guid.NewGuid();

        HConsole.Configure(x => x.Configure(this).SetIndent(20).SetPrefix("SERVER".Dot(_hcName)));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Server"/> class.
    /// </summary>
    /// <param name="address">The socket network address.</param>
    /// <param name="handler">A handler for incoming messages.</param>
    /// <param name="hcName">An HConsole name complement.</param>
    /// <param name="loggerFactory">A logger factory.</param>
    [Obsolete("Use the other public constructor.", false)]
    public Server(NetworkAddress address, Func<Server, ClientMessageConnection, ClientMessage, ValueTask> handler, ILoggerFactory? loggerFactory = null, string hcName = "")
        : this(address, loggerFactory, hcName)
    {
        _handler = request => handler(request.Server, request.Connection, request.Message);
    }

    /// <summary>
    /// Assigns the handler for a request message type.
    /// </summary>
    /// <param name="messageType">The type of the request message.</param>
    /// <param name="handler">The handler function.</param>
    /// <returns>This server.</returns>
    public Server Handle(int messageType, Func<ClientRequest, ValueTask> handler)
    {
        _handlers[messageType] = handler;
        return this;
    }

    /// <summary>
    /// Assigns the fallback handler.
    /// </summary>
    /// <param name="handler">The handler function.</param>
    /// <returns>This server.</returns>
    public Server HandleFallback(Func<ClientRequest, ValueTask> handler)
    {
        _handler = handler;
        return this;
    }

    /// <summary>
    /// Gets the address of the server.
    /// </summary>
    public NetworkAddress Address { get; }

    /// <summary>
    /// Gets the number of active connections.
    /// </summary>
    public int ConnectionCount => _connections.Count;

    /// <summary>
    /// Gets the member identifier of the server.
    /// </summary>
    public Guid MemberId { get; set; }

    /// <summary>
    /// Sets the member identifier of the server.
    /// </summary>
    /// <param name="memberId">The member identifier of the server.</param>
    /// <returns>This server.</returns>
    public Server WithMemberId(Guid memberId)
    {
        MemberId = memberId;
        return this;
    }

    /// <summary>
    /// Gets the cluster identifier of the server.
    /// </summary>
    public Guid ClusterId { get; set; }

    /// <summary>
    /// Sets the cluster identifier of the server.
    /// </summary>
    /// <param name="clusterId">The cluster identifier of the server.</param>
    public Server WithClusterId(Guid clusterId)
    {
        ClusterId = clusterId;
        return this;
    }

    /// <summary>
    /// Assigns a state object to the server.
    /// </summary>
    /// <typeparam name="TState">The type of the state object.</typeparam>
    /// <param name="state">The state object.</param>
    /// <returns>A server with a state.</returns>
    public Server<TState> WithState<TState>(TState state) => new(this, state);

    /// <summary>
    /// Starts the server.
    /// </summary>
    /// <returns>A task that will complete when the server has started.</returns>
    public async Task<Server> StartAsync()
    {
        HConsole.WriteLine(this, $"Start server at {_endpoint}");

        _listener = new ServerSocketListener(_endpoint, _hcName)
        {
            OnAcceptConnection = AcceptConnection,
            OnShutdown = ListenerShutdown
        };

        _open = true;
        await _listener.StartAsync().CfAwait();

        HConsole.WriteLine(this, "Server started");
        return this;
    }

    /// <summary>
    /// Handles the <see cref="ServerSocketListener"/> shutting down.
    /// </summary>
    /// <param name="arg">The <see cref="ServerSocketListener"/>.</param>
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
            HConsole.Configure(x => x.Configure(messageConnection).SetIndent(28).SetPrefix("SVR.MSG".Dot(_hcName)));
            serverConnection.OnShutdown = SocketShutdown;
            serverConnection.ExpectPrefixBytes(3, ReceivePrefixBytes);
            serverConnection.Accept();
            _connections[serverConnection.Id] = serverConnection;
        }
    }

    private void ReceiveMessage(ClientMessageConnection connection, ClientMessage requestMessage)
    {
        lock (_handlerLock)
        {
            if (_handling == null)
            {
                _handling = HandleClientRequest(new ClientRequest(this, connection, requestMessage)).AsTask();
            }
            else
            {
                _handling = _handling.ContinueWith(_ =>
                    HandleClientRequest(new ClientRequest(this, connection, requestMessage)).AsTask()).Unwrap();
            }
        }
    }

    /*
    private AsyncQueue<ClientRequest> _queue = new();

    private async ValueTask HandleClientRequests(CancellationToken cancellationToken)
    {
        await foreach (var request in _queue.WithCancellation(cancellationToken))
        {
            if (request == null) continue; // should not happen, better be safe
            await HandleClientRequest(request).CfAwait();
        }
    }
    */

    private async ValueTask HandleClientRequest(ClientRequest request)
    {
        HConsole.WriteLine(this, $"Handle {MessageTypeConstants.GetMessageTypeName(request.Message.MessageType)} message.");

        if (_handlers.TryGetValue(request.Message.MessageType, out var handler))
        {
            await handler(request).CfAwait();
        }
        else if (_handler != null)
        {
            await _handler(request).CfAwait();
        }
        else
        {
            // RemoteError.Hazelcast or RemoteError.RetryableHazelcast
            var messageName = MessageTypeConstants.GetMessageTypeName(request.Message.MessageType);
            var errorMessage = $"Received unsupported {messageName} (0x{request.Message.MessageType:X}).";
            await request.ErrorAsync(RemoteError.Hazelcast, errorMessage).CfAwait();
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
        await StopAsync().CfAwaitNoThrow();
    }
}