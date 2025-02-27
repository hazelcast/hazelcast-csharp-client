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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;

namespace Hazelcast.Testing.TestServer;

/// <summary>
/// Represents a server listener.
/// </summary>
/// <remarks>
/// <para>The server listener is not thread-safe in some ways, e.g. trying to stop it
/// while it is starting, and other exotic operations, are going to produce unspecified
/// results.</para>
/// <para>The server listener is not fully multi-threaded.</para>
/// </remarks>
internal sealed class ServerSocketListener : IAsyncDisposable
{
    private readonly IPEndPoint _endpoint;
    private readonly string _hcname;

    private Action<ServerSocketConnection> _onAcceptConnection;
    private Func<ServerSocketListener, ValueTask> _onShutdown;
    private Socket _listeningSocket;
    private bool _stopped;
    private Task _accepting;
    private int _isActive;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerSocketListener"/> class.
    /// </summary>
    /// <param name="endpoint">The socket endpoint.</param>
    /// <param name="hcname">An HConsole name complement.</param>
    public ServerSocketListener(IPEndPoint endpoint, string hcname)
    {
        _endpoint = endpoint;
        _hcname = hcname;

        HConsole.Configure(x => x.Configure(this).SetIndent(24).SetPrefix("LISTENER".Dot(hcname)));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerSocketListener"/> class.
    /// </summary>
    /// <param name="endpoint">The socket endpoint.</param>
    public ServerSocketListener(IPEndPoint endpoint)
        : this(endpoint, string.Empty)
    { }

    /// <summary>
    /// Gets or sets the action that will be executed when a connection has been accepted.
    /// </summary>
    public Action<ServerSocketConnection> OnAcceptConnection
    {
        // note: this action is not async

        get => _onAcceptConnection;
        set
        {
            if (_isActive == 1 || _stopped)
                throw new InvalidOperationException("Cannot set the property once the listener is active.");

            _onAcceptConnection = value ?? throw new ArgumentNullException(nameof(value));
        }
    }

    /// <summary>
    /// Gets or sets the action that will be executed when the listener has shut down.
    /// </summary>
    public Func<ServerSocketListener, ValueTask> OnShutdown
    {
        get => _onShutdown;
        set
        {
            if (_isActive == 1 || _stopped)
                throw new InvalidOperationException("Cannot set the property once the listener is active.");

            _onShutdown = value ?? throw new ArgumentNullException(nameof(value));
        }
    }

    /// <summary>
    /// Starts listening.
    /// </summary>
    /// <returns>A task that will complete when the listener has started listening.</returns>
    /// <remarks>
    /// <para>The listener must be disposed in order to stop. It cannot be restarted.</para>
    /// </remarks>
    public Task StartAsync()
    {
        if (_stopped)
            throw new InvalidOperationException("Cannot start a listener that has been stopped.");

        HConsole.WriteLine(this, "Start listener");

        if (_onAcceptConnection == null)
            throw new InvalidOperationException("No connection handler has been configured.");

        HConsole.WriteLine(this, "Create listener socket");
        _listeningSocket = new Socket(_endpoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            _listeningSocket.Bind(_endpoint);
            _listeningSocket.Listen(10);
        }
        catch (Exception e)
        {
            HConsole.WriteLine(this, "Failed to bind socket");
            HConsole.WriteLine(this, e);
            throw;
        }

        _accepting = AcceptAsync(_listeningSocket);

        HConsole.WriteLine(this, "Started listener");

        return Task.CompletedTask;
    }

    // reference: https://github.com/davidfowl/DotNetCodingPatterns/blob/main/2.md
    // except here we want to be able to control when to stop accepting

    private async Task AcceptAsync(Socket socket)
    {
        HConsole.WriteLine(this, "Start listening");
        Interlocked.Exchange(ref _isActive, 1);

        while (!_stopped)
        {
            try
            {
                Accept(await socket.AcceptAsync().CfAwait());
            }
            catch { /* stopped */ }
        }

        HConsole.WriteLine(this, "Stop listening");
        if (Interlocked.CompareExchange(ref _isActive, 0, 1) == 0)
            return;

        // notify
        if (_onShutdown != null) await _onShutdown.AwaitEach(this).CfAwait();
    }

    /// <summary>
    /// Accepts a connection.
    /// </summary>
    private void Accept(Socket socket)
    {
        HConsole.WriteLine(this, "Accept connection");

        try
        {
            // we now have a connection
            _onAcceptConnection(new ServerSocketConnection(Guid.NewGuid(), socket, _hcname));
        }
        catch (Exception e)
        {
            HConsole.WriteLine(this, "Failed to accept a connection");
            HConsole.WriteLine(this, e);

            try
            {
                socket.Dispose();
            }
            catch { /* ignore */}
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_stopped) return;

        try
        {
            _stopped = true;
            if (_listeningSocket != null) _listeningSocket.Dispose();
            if (_accepting != null) await _accepting.CfAwait();
            HConsole.WriteLine(this, "Stopped listener");
        }
        catch { /* ignore */ }
    }
}