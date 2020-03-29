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
using System.Collections.Generic;
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

        private readonly Dictionary<int, ServerSocketConnection> _connections = new Dictionary<int, ServerSocketConnection>();

        private readonly string _hostname;
        private readonly int _port;

        private ServerSocketListener _listener;

        public Server(string hostname, int port)
        {
            _hostname = hostname;
            _port = port;
        }

        public async Task StartAsync()
        {
            Log.WriteLine("Start server");

            _listener = new ServerSocketListener(_hostname, _port, AcceptConnection);
            await _listener.StartAsync();

            Log.WriteLine("Server started");
        }

        public async Task StopAsync()
        {
            Log.WriteLine("Stop server");

            await _listener.StopAsync();

            Log.WriteLine("Server stopped");
        }

        private void AcceptConnection(Socket socket)
        {
            // FIXME we should receive a connection already, not the raw socket?

            var messageConnection = new MessageConnection(onReceiveBytes
                => new ServerSocketConnection(socket, onReceiveBytes), ReceiveMessage);

            var connection = new ServerSocketConnection(socket, ReceiveMessageBytes);
            _connections[_connections.Count] = connection;
        }

        // TODO
        // we should get notified when a connection closes
        // we should have a way to notify each connection we're shutting down (?)

        private static async ValueTask ReceiveMessage(MessageConnection connection, Message message)
        {
            var responseText = message.Text switch
            {
                "a" => "alpha",
                "b" => "bravo",
                _ => "wut?"
            };
            var response = new Message(responseText) { Id = message.Id };
            await connection.SendAsync(response);
        }

        private static async ValueTask ReceiveMessageBytes(SocketConnection connection, ReadOnlySequence<byte> bytes)
        {
            var message = Message.Parse(bytes.ToArray());
            var responseText = message.Text switch
            {
                "a" => "alpha",
                "b" => "bravo",
                _ => "wut?"
            };
            var response = new Message(responseText) { Id = message.Id };
            await connection.SendAsync(response.ToPrefixedBytes());
        }
    }
}