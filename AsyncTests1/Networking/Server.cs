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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncTests1.Networking
{
    // a nave TCP server for tests
    //  has poor buffers management
    //  has poor exceptions management
    //
    public class Server
    {
        private static readonly Log Log = new Log("SVR");

        private readonly ManualResetEvent _accepted = new ManualResetEvent(false);

        private readonly string _hostname;
        private readonly int _port;
        private readonly string _eom;

        private Socket _socket;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _task;

        public Server(string hostname, int port, string eom = "/")
        {
            _hostname = hostname;
            _port = port;
            _eom = eom;
        }

        public Task StartAsync()
        {
            Log.WriteLine("Start server");

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

            return Task.CompletedTask;
        }

        public void AcceptCallback(IAsyncResult result)
        {
            Log.WriteLine("Accept connection");

            // signal the main thread to continue
            _accepted.Set();

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

            // create the state object
            var state = new StateObject { Socket = handler };
            handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, SocketFlags.None, ReadCallback, state);
            Log.WriteLine("Listening...");
        }

        public void ReadCallback(IAsyncResult result)
        {
            Log.WriteLine("Read data");

            // retrieve the state object and the handler socket
            // from the asynchronous state object
            var state = (StateObject) result.AsyncState;
            var handler = state.Socket;

            // read data from the client socket
            int bytesRead;
            try
            {
                // may throw if the socket is not connected anymore
                bytesRead = handler.EndReceive(result);
                if (bytesRead <= 0)
                {
                    Log.WriteLine("Stop read");
                    return;
                }
            }
            catch (Exception e)
            {
                Log.WriteLine("Abort read");
                Log.WriteLine(e);
                return;
            }

            // NOTE: yes, working with string builders below is horrible

            // there might be more data, so store the data received so far
            Log.WriteLine($"Append {bytesRead} bytes from socket");
            state.Text.Append(Encoding.ASCII.GetString(state.Buffer, 0, bytesRead));

            // check for end tag, if it is not there, read more data
            var content = state.Text.ToString();
            var pos = content.IndexOf(_eom, StringComparison.Ordinal);
            while (pos > 0)
            {
                // a message has been read from the client
                var text = content.Substring(0, pos);

                content = content.Substring(pos + _eom.Length);
                state.Text.Clear();
                state.Text.Append(content);

                Log.WriteLine($"Read message bytes from socket \n\tData: {text}");
                var message = Message.Parse(text);
                HandleMessage(handler, message);

                pos = content.IndexOf(_eom, StringComparison.Ordinal);
            }

            if (pos == 0)
            {
                // empty message closes the connection
                try
                {
                    // shall we reply to confirm?

                    Log.WriteLine("Empty message, close connection");
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                    handler.Dispose(); // ?
                }
                catch (Exception e)
                {
                    Log.WriteLine(e);
                }
                return;
            }

            // not all data received, get more
            handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, SocketFlags.None, ReadCallback, state);
        }

        private void HandleMessage(Socket socket, Message message)
        {
            // note: in real life, the server may queue the operation,
            // and write back a response when done, and responses may
            // come out-of-order + may not close the connection if we
            // want to keep it alive

            Log.WriteLine("Handling message: " + message);

            // respond to client
            var response = new Message("pong:" + message.Text + _eom) { Id = message.Id };
            Send(socket, response.ToString());
        }

        private static void Send(Socket handler, string data)
        {
            // convert the string data to byte data using ASCII encoding
            var byteData = Encoding.ASCII.GetBytes(data);

            // begin sending the data to the remote device
            handler.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, SendCallback, handler);
        }

        private static void SendCallback(IAsyncResult result)
        {
            try
            {
                // retrieve the socket from the state object
                var handler = (Socket) result.AsyncState;

                // complete sending the data to the remote device
                var bytesSent = handler.EndSend(result);
                Log.WriteLine($"Sent {bytesSent} bytes", bytesSent);

                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();
            }
            catch (Exception e)
            {
                Log.WriteLine(e.ToString());
            }
        }

        public async Task StopAsync()
        {
            Log.WriteLine("Stop server");

            _cancellationTokenSource.Cancel();
            await _task;

            Log.WriteLine("Server stopped");
        }
    }
}