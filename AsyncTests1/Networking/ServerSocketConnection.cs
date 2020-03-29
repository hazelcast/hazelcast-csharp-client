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
using System.Net.Sockets;
using System.Threading.Tasks;

namespace AsyncTests1.Networking
{
    /// <summary>
    /// Represents a server socket connection.
    /// </summary>
    /// <remarks>
    /// <para>The server socket connection handles a client and its message
    /// bytes, and manages the network socket. It is used by the server.</para>
    /// </remarks>
    public class ServerSocketConnection : SocketConnection
    {
        /*
        private readonly ManualResetEvent _accepted = new ManualResetEvent(false);

        private CancellationTokenSource _cancellationTokenSource;
        private Task _task;
        */

        public ServerSocketConnection(Socket socket, Func<SocketConnection, ReadOnlySequence<byte>, ValueTask> onReceiveMessageBytes)
            : base(onReceiveMessageBytes)
        {
            // use a stream, because we may use SSL and require an SslStream
            // TODO: implement SSL or provide a Func<Stream, Stream>
            var stream = new NetworkStream(socket, false);

            // wire the pipe
            OpenPipe(socket, stream);
        }

        /*
        public override ValueTask OpenAsync()
        {
            Log.WriteLine("Start server");

            var host = Dns.GetHostEntry(Hostname);
            var ipAddress = host.AddressList[0];

            Log.WriteLine("Create listener socket");
            _socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            var endpoint = new IPEndPoint(ipAddress, Port);
            _socket.Bind(endpoint);
            _socket.Listen(10);

            _cancellationTokenSource = new CancellationTokenSource();

            _task = Task.Run(() =>
            {
                Log.WriteLine("Start listening");
                while (true)
                {
                    // Set the event to non-signaled state.
                    _accepted.Reset();

                    // Start an asynchronous socket to listen for connections.
                    Log.WriteLine("Waiting for a connection...");
                    _socket.BeginAccept(AcceptCallback, _socket);

                    // wait until a connection is made before continuing
                    // (or, for the server to be stopped)
                    var n = WaitHandle.WaitAny(new[] { _accepted, _cancellationTokenSource.Token.WaitHandle });
                    if (n == 1) break;
                }
                Log.WriteLine("Stop listening");

                Log.WriteLine("Shutdown socket");
                //_socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
                _socket.Dispose();

            }, CancellationToken.None);

            return new ValueTask();
        }

        private void AcceptCallback(IAsyncResult result)
        {
            Log.WriteLine("Accept connection");

            // signal the main thread to continue
            _accepted.Set();

            // get the socket that handles the client request
            var listener = (Socket) result.AsyncState;

            Socket handler;
            Stream stream;
            try
            {
                // may throw if the socket is not connected anymore
                // also when the server is stopping
                handler = listener.EndAccept(result);

                // use a stream, because we may use SSL and require an SslStream
                // TODO: implement SSL or provide a Func<Stream, Stream>
                stream = new NetworkStream(_socket, false);
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
                // do something with listener?
                return;
            }

            var pipe = new Pipe();
            var reading = ReadAsync(stream, pipe.Reader);
            var writing = WriteAsync(stream, pipe.Writer);
        }
        */
    }
}