// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;
using Hazelcast.Core;

namespace Hazelcast.Testing.TestServer;

internal class Server<TState> : IAsyncDisposable
{
    private readonly Server _server;

    public Server(Server server, TState state)
    {
        _server = server;
        State = state;
    }

    /// <summary>
    /// Gets the number of active connections.
    /// </summary>
    public int ConnectionCount => _server.ConnectionCount;

    /// <summary>
    /// Gets the state object.
    /// </summary>
    public TState State { get; }

    /// <summary>
    /// Gets the cluster identifier of the server.
    /// </summary>
    public Guid ClusterId => _server.ClusterId;

    /// <summary>
    /// Starts the server.
    /// </summary>
    /// <returns>A task that will complete when the server has started.</returns>
    public async Task<Server<TState>> StartAsync()
    {
        await _server.StartAsync().CfAwait();
        return this;
    }

    public Task StopAsync() => _server.StopAsync();

    /// <summary>
    /// Assigns the handler for a request message type.
    /// </summary>
    /// <param name="messageType">The type of the request message.</param>
    /// <param name="handler">The handler function.</param>
    /// <returns>This server.</returns>
    public Server<TState> Handle(int messageType, Func<ClientRequest<TState>, ValueTask> handler)
    {
        _server.Handle(messageType, request => handler(new ClientRequest<TState>(request, State)));
        return this;
    }

    /// <summary>
    /// Assigns the fallback handler.
    /// </summary>
    /// <param name="handler">The handler function.</param>
    /// <returns>This server.</returns>
    public Server<TState> HandleFallback(Func<ClientRequest<TState>, ValueTask> handler)
    {
        _server.HandleFallback(request => handler(new ClientRequest<TState>(request, State)));
        return this;
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        return _server.DisposeAsync();
    }
}