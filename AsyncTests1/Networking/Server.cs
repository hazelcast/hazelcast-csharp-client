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

using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncTests1.Networking
{
    // a nave TCP server for tests
    //  has poor buffers management
    //  has poor exceptions management
    //
    public class Server
    {
        public readonly Log Log = new Log { Prefix = "                        SVR" };

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

            _listener = new ServerSocketListener(_hostname, _port) { OnAcceptConnection = AcceptConnection };
            _listener.Log.Prefix = "                          LST";
            await _listener.StartAsync();

            Log.WriteLine("Server started");
        }

        public async Task StopAsync()
        {
            Log.WriteLine("Stop server");

            await _listener.StopAsync();

            Log.WriteLine("Server stopped");
        }

        private void AcceptConnection(ServerSocketConnection serverConnection)
        {
            // the connection we receive is not wired yet
            // must wire it properly before accepting

            var messageConnection = new MessageConnection(serverConnection) { OnReceiveMessage = ReceiveMessage };
            messageConnection.Log.Prefix = "                            SVR.MSG";
            serverConnection.Accept();
            _connections[_connections.Count] = serverConnection;
        }

        // TODO
        // we should get notified when a connection closes
        // we should have a way to notify each connection we're shutting down (?)

        private async ValueTask ReceiveMessage(MessageConnection connection, Message message)
        {
            Log.WriteLine("Respond");
            var responseText = message.Text switch
            {
                "a" => "alpha",
                "b" => "bravo",
                _ => "wut?"
            };
            var response = new Message(responseText) { Id = message.Id };
            await connection.SendAsync(response);
            Log.WriteLine("Responded");
        }
    }
}