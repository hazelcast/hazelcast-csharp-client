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
using System.IO;
using System.IO.Pipelines;
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

            var pipe = new Pipe();
            var reading = ReadAsync(handler, pipe.Reader);
            var writing = WriteAsync(handler, pipe.Writer);
        }

        // reads from socket and writes to the pipe
        async Task WriteAsync(Socket handler, PipeWriter writer)
        {
            const int minimumBufferSize = 512;

            while (true)
            {
                // allocate at least 512 bytes from the PipeWriter
                var memory = writer.GetMemory(minimumBufferSize);
                try
                {
                    int bytesRead = await handler.ReceiveAsync(memory, SocketFlags.None, CancellationToken.None);
                    if (bytesRead == 0)
                    {
                        break;
                    }
                    // Tell the PipeWriter how much was read from the Socket
                    writer.Advance(bytesRead);
                }
                catch (Exception ex)
                {
                    Log.WriteLine("ERROR!");
                    Log.WriteLine(ex);
                    break;
                }

                // make the data available to the PipeReader
                var result = await writer.FlushAsync();

                if (result.IsCompleted)
                {
                    break;
                }
            }

            // Tell the PipeReader that there's no more data coming
            Log.WriteLine("Complete writer");
            writer.Complete();
        }

        // reads from the pipe and processes lines
        async Task ReadAsync(Socket handler, PipeReader reader)
        {
            var expected = -1;

            // loop reading data from the pipe
            while (true)
            {
                Log.WriteLine("Wait for data");
                var result = await reader.ReadAsync();
                var buffer = result.Buffer;

                Log.WriteLine($"Buffer: {buffer.Length}");

                // whenever we get data,
                // loop processing messages
                var position = 0;
                while (true)
                {
                    Log.WriteLine("Look for message");
                    if (expected < 0)
                    {
                        // not enough data, read more data
                        if (buffer.Length < 4)
                            break;

                        // get message length
                        // expected = deserialize 4 bytes
                        expected = buffer.ReadInt32();
                        if (expected == 0)
                        {
                            Log.WriteLine($"Received zero-length message");
                            break;
                        }
                        Log.WriteLine($"Expecting message with size {expected} bytes");
                        buffer = buffer.Slice(4);
                        position += 4;
                    }

                    // not enough data, read more data
                    if (buffer.Length < expected)
                        break;

                    Log.WriteLine("Handle message");
                    var message = Message.Parse(buffer.Slice(0, expected).ToArray());
                    await ProcessMessage(handler, message);
                    buffer = buffer.Slice(expected);
                    position += expected;
                    expected = -1;
                }

                // tell the PipeReader how much of the buffer we have consumed
                reader.AdvanceTo(buffer.Start, buffer.End);

                // stop reading (FIXME and what else?) on message
                if (expected == 0)
                    break;

                // stop reading if there's no more data coming
                if (result.IsCompleted)
                    break;
            }

            // mark the PipeReader as complete
            Log.WriteLine("Complete reader");
            reader.Complete();
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

        private async Task ProcessMessage(Socket socket, Message message)
        {
            Log.WriteLine("Handling message: " + message);

            // respond to client
            var response = new Message("pong:" + message.Text) { Id = message.Id };
            await SendAsync(socket, response);
        }

        private void HandleMessage(Socket socket, Message message)
        {
            // note: in real life, the server may queue the operation,
            // and write back a response when done, and responses may
            // come out-of-order + may not close the connection if we
            // want to keep it alive

            Log.WriteLine("Handling message: " + message);

            // respond to client
            var response = new Message("pong:" + message.Text) { Id = message.Id };
            SendAsync(socket, response);
        }

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public async ValueTask SendAsync(Socket socket, Message message)
        {
            // note - look at how SendAsync is implemented, we may get closer to metal

            await _semaphore.WaitAsync();

            Log.WriteLine($"Send \"{message}\"");
            var bytes = message.ToPrefixedBytes();

            foreach (var b in bytes)
                Console.Write($"{b:x2} ");
            Console.WriteLine();

            await socket.SendAsync(bytes, SocketFlags.None);
            Log.WriteLine($"Sent {bytes.Length} bytes");

            _semaphore.Release();
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