﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

namespace AsyncTests1.Networking
{
    public class ServerSocketListener
    {
        private static readonly Log Log = new Log("SVR");

        private readonly ManualResetEvent _accepted = new ManualResetEvent(false);

        private readonly string _hostname;
        private readonly int _port;
        private readonly Action<Socket> _onAcceptConnection;

        private Socket _socket;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _task;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerSocketListener"/> class.
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port">The port to listen to.</param>
        /// <param name="onAcceptConnection">An action to execute when accepting a connection.</param>
        public ServerSocketListener(string hostname, int port, Action<Socket> onAcceptConnection)
        {
            _hostname = hostname;
            _port = port;
            _onAcceptConnection = onAcceptConnection;
        }

        /// <summary>
        /// Starts listening.
        /// </summary>
        /// <returns>A task that will complete when the listener has started listening.</returns>
        public Task StartAsync()
        {
            Log.WriteLine("Start listener");

            var host = Dns.GetHostEntry(_hostname);
            var ipAddress = host.AddressList[0];
            var endpoint = new IPEndPoint(ipAddress, _port);

            Log.WriteLine("Create listener socket");
            _socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            _socket.Bind(endpoint);
            _socket.Listen(10);

            _cancellationTokenSource = new CancellationTokenSource();

            _task = Task.Run(() =>
            {
                Log.WriteLine("Start listening");
                while (true)
                {
                    // set the event to non-signaled state
                    _accepted.Reset();

                    // start an asynchronous socket to listen for connections
                    Log.WriteLine("Listening");
                    _socket.BeginAccept(AcceptCallback, _socket);

                    // wait until a connection is accepted or listening is cancelled
                    var n = WaitHandle.WaitAny(new[] { _accepted, _cancellationTokenSource.Token.WaitHandle });
                    if (n == 1) break;
                }
                Log.WriteLine("Stop listening");

                Log.WriteLine("Shutdown socket");
                //_socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
                _socket.Dispose();

            }, CancellationToken.None);

            Log.WriteLine("Started listener");

            return Task.CompletedTask;
        }

        /// <summary>
        /// Stops listening.
        /// </summary>
        /// <returns>A task that will complete when the listener has stopped listening.</returns>
        public async ValueTask StopAsync()
        {
            Log.WriteLine("Stop listener");

            _cancellationTokenSource.Cancel();
            await _task;

            Log.WriteLine("Stopped listener");
        }

        /// <summary>
        /// Accepts a connection.
        /// </summary>
        /// <param name="result">The status of the asynchronous listening.</param>
        private void AcceptCallback(IAsyncResult result)
        {
            Log.WriteLine("Accept connection");

            // signal the main thread to continue
            _accepted.Set();

            // get the socket that handles the client request
            var listener = (Socket) result.AsyncState;

            try
            {
                // may throw if the socket is not connected anymore
                // also when the server is stopping
                var handler = listener.EndAccept(result);

                // we now have a connection
                // TODO: create the connection here
                _onAcceptConnection(handler);
            }
            catch (Exception e)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    Log.WriteLine("Abort connection (server is stopping)");
                }
                else
                {
                    Log.WriteLine("Abort connection");
                    Log.WriteLine(e);
                }

                // TODO: do something with listener?
            }
        }
    }
}