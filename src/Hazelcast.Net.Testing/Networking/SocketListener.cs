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

namespace Hazelcast.Testing.Networking
{
    public class SocketListener : IDisposable
    {
        private const int PortRange = 10; // offset goes +0 to +9
        private static int _portOffset;

        private Socket _listener;
        private Socket _socket;

        public SocketListener(string address, SocketListenerMode mode)
            : this(IPEndPointEx.Parse(address), mode)
        { }

        public SocketListener(IPEndPoint endpoint, SocketListenerMode mode)
        {
            // offset port - shutting down a socket and releasing it takes time, so if we
            // keep using the same port, the retry loop below always kicks and slows the
            // tests

            endpoint.Port += _portOffset;
            if (++_portOffset == PortRange) _portOffset = 0;

            // note
            // listen backlog 0 is equivalent to 1, backlog -1 means "system queue size"
            //
            // on Windows, if the queue is full, further connections are refused (and client throws)
            // but on Linux... it's different http://veithen.io/2014/01/01/how-tcp-backlog-works-in-linux.html

            // non-Windows: just don't listen = connection refused
            // Windows is more tricky, see below
            if (!OS.IsWindows && mode == SocketListenerMode.ConnectionRefused)
                return;

            _listener = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // retry loop
            var i = 0;
            while (true)
            {
                try
                {
                    _listener.Bind(endpoint);
                    break;
                }
                catch
                {
                    Thread.Sleep(500);
                    if (i++ == 10) throw;
                }
            }

            switch (mode)
            {
                case SocketListenerMode.Default:
                case SocketListenerMode.ConnectionRefused:
                    // listen, backlog 1 connection in the queue, never accept it
                    // so all subsequent requests will be refused
                    _listener.Listen(1);
                    _socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    _socket.Connect(endpoint);
                    break;
                case SocketListenerMode.AcceptOnce:
                    // listen, accept first connection, backlog others
                    // so they are not accepted, but not refused
                    _listener.Listen(-1);
                    _listener.BeginAccept(AcceptCallback, _listener);
                    break;
            }
        }

        private static void AcceptCallback(IAsyncResult result)
        {
            try
            {
                var l = (Socket) result.AsyncState;
                var h = l.EndAccept(result);
            }
            catch { /* can happen */ }
        }

        public void Dispose()
        {
            try
            {
                if (_socket != null)
                {
                    _socket.Shutdown(SocketShutdown.Both);
                    _socket.Disconnect(false);
                    _socket.Close();
                }
            }
            catch { /* doesn't matter */ }

            try
            {
                _socket.Dispose();
            }
            catch { /* doesn't matter */ }

            try
            {
                _listener.Shutdown(SocketShutdown.Both);
                _listener.Close();
                _listener.Disconnect(false);
            }
            catch { /* doesn't matter */ }

            try
            {
                _listener.Dispose();
            }
            catch { /* doesn't matter */ }

            _socket = null;
            _listener = null;
        }
    }
}
