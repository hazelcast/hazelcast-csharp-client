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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Logging;

namespace Hazelcast.Networking
{
    /// <summary>
    /// Represents a server listener.
    /// </summary>
    public class ServerSocketListener : IDisposable
    {
        private readonly ManualResetEvent _accepted = new ManualResetEvent(false);
        private readonly ISequence<int> _connectionIdSequence = new Int32Sequence();

        private readonly IPEndPoint _endpoint;

        private Socket _socket;
        private CancellationTokenSource _cancellationTokenSource;
        private Action<ServerSocketConnection> _onAcceptConnection;
        private Func<ServerSocketListener, ValueTask> _onShutdown;
        private Task _listeningThenShutdown;
        private int _isActive;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerSocketListener"/> class.
        /// </summary>
        /// <param name="endpoint">The socket endpoint.</param>
        public ServerSocketListener(IPEndPoint endpoint)
        {
            _endpoint = endpoint;
        }

        /// <summary>
        /// Gets or sets the function that accepts connections.
        /// </summary>
        public Action<ServerSocketConnection> OnAcceptConnection
        {
            // note: this action is not async

            get => _onAcceptConnection;
            set
            {
                if (_isActive == 1)
                    throw new InvalidOperationException("Cannot set the property once the listener is active.");

                _onAcceptConnection = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        public Func<ServerSocketListener, ValueTask> OnShutdown
        {
            get => _onShutdown;
            set
            {
                if (_isActive == 1)
                    throw new InvalidOperationException("Cannot set the property once the listener is active.");

                _onShutdown = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Starts listening.
        /// </summary>
        /// <returns>A task that will complete when the listener has started listening.</returns>
        public Task StartAsync()
        {
            XConsole.WriteLine(this, "Start listener");

            if (_onAcceptConnection == null)
                throw new InvalidOperationException("No connection handler has been configured.");

            XConsole.WriteLine(this, "Create listener socket");
            _socket = new Socket(_endpoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                _socket.Bind(_endpoint);
                _socket.Listen(10);
            }
            catch (Exception e)
            {
                XConsole.WriteLine(this, "Failed to bind socket");
                XConsole.WriteLine(this, e);
                throw;
            }

            _cancellationTokenSource = new CancellationTokenSource();

            _listeningThenShutdown = Task.Run(() =>
            {
                XConsole.WriteLine(this, "Start listening");
                Interlocked.Exchange(ref _isActive, 1);
                var waitHandles = new[] { _accepted, _cancellationTokenSource.Token.WaitHandle };
                while (true)
                {
                    // set the event to non-signaled state
                    _accepted.Reset();

                    // start an asynchronous socket to listen for connections
                    XConsole.WriteLine(this, "Listening");
                    _socket.BeginAccept(AcceptCallback, _socket);

                    // TODO consider doing things differently (low)
                    // we could do this and remain purely async, and then onAcceptConnection
                    // could be async too, etc - and we'd need to benchmark to see what is
                    // faster...
                    //var handler = _socket.AcceptAsync();
                    // ...

                    // wait until a connection is accepted or listening is cancelled
                    var n = WaitHandle.WaitAny(waitHandles);
                    if (n == 1) break;
                }
                XConsole.WriteLine(this, "Stop listening");

            }, CancellationToken.None).ContinueWith(ShutdownInternal);

            XConsole.WriteLine(this, "Started listener");

            return Task.CompletedTask;
        }

        private async Task ShutdownInternal(Task task)
        {
            if (Interlocked.CompareExchange(ref _isActive, 0, 1) == 0)
                return;

            XConsole.WriteLine(this, "Shutdown socket");
            //_socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            _socket.Dispose();

            XConsole.WriteLine(this, "Listener is down");

            // notify
            if (_onShutdown != null)
                await _onShutdown(this);
        }

        /// <summary>
        /// Stops listening.
        /// </summary>
        /// <returns>A task that will complete when the listener has stopped listening.</returns>
        public async ValueTask StopAsync()
        {
            XConsole.WriteLine(this, "Stop listener");

            _cancellationTokenSource.Cancel();
            await _listeningThenShutdown;

            XConsole.WriteLine(this, "Stopped listener");
        }

        /// <summary>
        /// Accepts a connection.
        /// </summary>
        /// <param name="result">The status of the asynchronous listening.</param>
        private void AcceptCallback(IAsyncResult result)
        {
            XConsole.WriteLine(this, "Accept connection");

            // signal the main thread to continue
            try
            {
                _accepted.Set();
            }
            catch
            {
                // can happen if the listener has been disposed
                // ignore
            }

            // get the socket that handles the client request
            var listener = (Socket) result.AsyncState;

            Socket handler;
            try
            {
                // may throw if the socket is not connected anymore
                // also when the server is stopping
                handler = listener.EndAccept(result);
            }
            catch (Exception e)
            {
                if (_isActive == 0)
                {
                    XConsole.WriteLine(this, "Ignore exception (listener is down)");
                }
                else if (_cancellationTokenSource.IsCancellationRequested)
                {
                    XConsole.WriteLine(this, "Abort connection (listened is shutting down)");
                }
                else
                {
                    _cancellationTokenSource.Cancel();
                    XConsole.WriteLine(this, "Abort connection");
                }
                XConsole.WriteLine(this, e);
                return;
            }

            try
            {
                // we now have a connection
                var serverConnection = new ServerSocketConnection(_connectionIdSequence.Next, handler);
                _onAcceptConnection(serverConnection);
            }
            catch (Exception e)
            {
                XConsole.WriteLine(this, "Failed to accept a connection");
                XConsole.WriteLine(this, e);
                XConsole.WriteLine(this, _cancellationTokenSource.IsCancellationRequested
                    ? "Abort connection (server is stopping)"
                    : "Abort connection");
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _accepted.TryDispose();
            _cancellationTokenSource.TryDispose();
        }
    }
}