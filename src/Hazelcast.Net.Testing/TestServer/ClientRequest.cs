// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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

using System.Linq;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Messaging;
using Hazelcast.Protocol.Models;
using Hazelcast.Testing.Protocol;

namespace Hazelcast.Testing.TestServer;

internal class ClientRequest<TState> : ClientRequest
{
    public ClientRequest(ClientRequest request, TState state)
        : base(request.Server, request.Connection, request.Message)
    {
        State = state;
    }

    /// <summary>
    /// Gets the state object.
    /// </summary>
    public TState State { get; }
}

/// <summary>
/// Represents a client request handled by a server.
/// </summary>
internal class ClientRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClientRequest"/> class.
    /// </summary>
    /// <param name="server">The server.</param>
    /// <param name="connection">The client connection.</param>
    /// <param name="message">The message.</param>
    public ClientRequest(Server server, ClientMessageConnection connection, ClientMessage message)
    {
        Server = server;
        Connection = connection;
        Message = message;
    }

    /// <summary>
    /// Gets the server.
    /// </summary>
    public Server Server { get; }

    /// <summary>
    /// Gets the client connection.
    /// </summary>
    public ClientMessageConnection Connection { get; }

    /// <summary>
    /// Gets the message.
    /// </summary>
    public ClientMessage Message { get; }

    /// <summary>
    /// Responds to the request.
    /// </summary>
    /// <param name="responseMessage">The response message.</param>
    public async Task RespondAsync(ClientMessage responseMessage)
    {
        responseMessage.CorrelationId = Message.CorrelationId;
        responseMessage.Flags |= ClientMessageFlags.BeginFragment | ClientMessageFlags.EndFragment;
        await Connection.SendAsync(responseMessage).CfAwait();
    }

    /// <summary>
    /// Responds to the request with an error.
    /// </summary>
    /// <param name="errorCode">The error code.</param>
    /// <param name="errorMessage">The error message.</param>
    public async Task ErrorAsync(RemoteError errorCode, string? errorMessage = null)
    {
        var errorHolders = new[]
        {
            new ErrorHolder(errorCode, "?", errorMessage ?? "error", Enumerable.Empty<StackTraceElement>())
        };
        var responseMessage = ErrorsServerCodec.EncodeResponse(errorHolders);
        await RespondAsync(responseMessage);
    }

    /// <summary>
    /// Raises an event in response to the request.
    /// </summary>
    /// <param name="eventMessage">The event message.</param>
    /// <param name="correlationId">The event correlation identifier.</param>
    public async Task RaiseAsync(ClientMessage eventMessage, long correlationId = -1)
    {
        eventMessage.CorrelationId = correlationId > 0 ? correlationId : Message.CorrelationId;
        eventMessage.Flags |= ClientMessageFlags.BeginFragment | ClientMessageFlags.EndFragment;
        await Connection.SendAsync(eventMessage).CfAwait();
    }
}
