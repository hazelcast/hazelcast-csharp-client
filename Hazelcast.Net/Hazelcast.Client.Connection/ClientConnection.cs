// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.Client.Spi;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.Logging;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Connection
{
    /// <summary>Holds the clientSocket to one of the members of Hazelcast ICluster.</summary>
    /// <remarks>Holds the clientSocket to one of the members of Hazelcast ICluster.</remarks>
    internal sealed class ClientConnection
    {
        public const int BufferSize = 1 << 15; //32k
        public const int SocketReceiveBufferSize = 1 << 15; //32k
        public const int SocketSendBufferSize = 1 << 15; //32k
        private const int ConnectionTimeout = 30000;
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof (ClientConnection));
        private readonly ClientMessageBuilder _builder;
        private readonly ClientConnectionManager _clientConnectionManager;
        private readonly int _id;

        private readonly ByteBuffer _receiveBuffer;
        private readonly ByteBuffer _sendBuffer;
        private readonly BlockingCollection<ISocketWritable> _writeQueue = new BlockingCollection<ISocketWritable>();
        private volatile Socket _clientSocket;
        private volatile ISocketWritable _lastWritable;
        private volatile bool _live;
        private bool _isOwner;
        private volatile IMember _member;
        private Thread _writeThread;

        /// <exception cref="System.IO.IOException"></exception>
        public ClientConnection(ClientConnectionManager clientConnectionManager,
            ClientInvocationService invocationService,
            int id,
            Address address,
            ClientNetworkConfig clientNetworkConfig)
        {
            _clientConnectionManager = clientConnectionManager;
            _id = id;

            var isa = address.GetInetSocketAddress();
            var socketOptions = clientNetworkConfig.GetSocketOptions();
            var socketFactory = socketOptions.GetSocketFactory();

            if (socketFactory == null)
            {
                socketFactory = new DefaultSocketFactory();
            }
            _clientSocket = socketFactory.CreateSocket();
            try
            {
                _clientSocket = new Socket(isa.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                var lingerOption = new LingerOption(true, 5);
                if (socketOptions.GetLingerSeconds() > 0)
                {
                    lingerOption.LingerTime = socketOptions.GetLingerSeconds();
                }
                _clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingerOption);
                _clientSocket.NoDelay = socketOptions.IsTcpNoDelay();

                _clientSocket.ReceiveTimeout = socketOptions.GetTimeout() > 0 ? socketOptions.GetTimeout() : -1;

                var bufferSize = socketOptions.GetBufferSize()*1024;
                if (bufferSize < 0)
                {
                    bufferSize = BufferSize;
                }

                _clientSocket.SendBufferSize = bufferSize;
                _clientSocket.ReceiveBufferSize = bufferSize;

                _clientSocket.UseOnlyOverlappedIO = true;

                var connectionTimeout = clientNetworkConfig.GetConnectionTimeout() > -1
                    ? clientNetworkConfig.GetConnectionTimeout()
                    : ConnectionTimeout;
                var socketResult = _clientSocket.BeginConnect(address.GetHost(), address.GetPort(), null, null);

                if (!socketResult.AsyncWaitHandle.WaitOne(connectionTimeout, true) || !_clientSocket.Connected)
                {
                    // NOTE, MUST CLOSE THE SOCKET
                    _clientSocket.Close();
                    throw new IOException("Failed to connect to " + address);
                }
                _sendBuffer = ByteBuffer.Allocate(BufferSize);
                _receiveBuffer = ByteBuffer.Allocate(BufferSize);

                _builder = new ClientMessageBuilder(invocationService.HandleClientMessage);

                _live = true;
            }
            catch (Exception e)
            {
                _clientSocket.Close();
                throw new IOException("Cannot connect! Socket error:" + e.Message);
            }
        }

        public bool Live
        {
            get { return _live; }
        }

        internal int Id
        {
            get { return _id; }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void Close()
        {
            Release();
        }

        public Address GetAddress()
        {
            return _member != null ? _member.GetAddress() : null;
        }

        public IPEndPoint GetLocalSocketAddress()
        {
            try
            {
                return _clientSocket != null ? (IPEndPoint) _clientSocket.LocalEndPoint : null;
            }
            catch (ObjectDisposedException)
            {
                return null;
            }
        }

        public IMember GetMember()
        {
            return _member;
        }

        public void SetRemoteMember(IMember member)
        {
            _member = member;
        }

        public void SwitchToNonBlockingMode()
        {
            if (_clientSocket.Blocking)
            {
                _sendBuffer.Clear();
                _receiveBuffer.Clear();
                //clientSocket.Blocking = false;
                StartAsyncProcess();
            }
            //ignore other cases
        }

        public override string ToString()
        {
            var localSocketAddress = GetLocalSocketAddress();
            if (localSocketAddress != null)
            {
                return "Connection[" + Id + "][" + _member + " -> " + localSocketAddress + "]";
            }
            return "Connection [" + _member + " -> CLOSED ]";
        }

        public bool WriteAsync(ISocketWritable packet)
        {
            if (!_live)
            {
                if (Logger.IsFinestEnabled())
                {
                    Logger.Finest("Connection is closed, won't write packet -> " + packet);
                }
                return false;
            }
            return _writeQueue.TryAdd(packet);
        }

        internal Socket GetSocket()
        {
            return _clientSocket;
        }

        /// <exception cref="System.IO.IOException"></exception>
        internal void Init()
        {
            _clientSocket.Send(Encoding.UTF8.GetBytes(Protocols.ClientBinaryNew));
        }

        internal void SetOwner()
        {
            _isOwner = true;
        }

        internal bool IsOwner()
        {
            return _isOwner;
        }
        private void BeginRead()
        {
            if (!CheckLive())
            {
                Close();
                return;
            }
            try
            {
                try
                {
                    SocketError socketError;
                    _clientSocket.BeginReceive(
                        _receiveBuffer.Array(),
                        _receiveBuffer.Position,
                        _receiveBuffer.Remaining(),
                        SocketFlags.None, out socketError, EndReadCallback, null);
                }
                catch (Exception e)
                {
                    Logger.Severe("Exception at Socket.Read for endPoint: " + GetAddress(), e);
                    HandleSocketException(e);
                }
            }
            catch (Exception e)
            {
                Logger.Severe("Fatal Error at BeginRead : " + GetAddress(), e);
                HandleSocketException(e);
            }
        }

        private bool CheckLive()
        {
            if (!_live)
            {
                return false;
            }
            if (!_clientSocket.Connected)
            {
                Task.Factory.StartNew(Close);
                return false;
            }
            return true;
        }

        private void EndReadCallback(IAsyncResult asyncResult)
        {
            if (_clientSocket == null)
            {
                return;
            }
            try
            {
                //if (ThreadUtil.debug) { Console.Write(" ER"+id); }
                SocketError socketError;
                var receivedByteSize = _clientSocket.EndReceive(asyncResult, out socketError);

                if (receivedByteSize <= 0)
                {
                    //Socket Closed
                    Close();
                }

                if (socketError != SocketError.Success)
                {
                    HandleSocketException(new SocketException((int) socketError));
                    return;
                }
                _receiveBuffer.Position += receivedByteSize;
                _receiveBuffer.Flip();

                _builder.OnData(_receiveBuffer);

                if (_receiveBuffer.HasRemaining())
                {
                    _receiveBuffer.Compact();
                }
                else
                {
                    _receiveBuffer.Clear();
                }
            }
            catch (Exception e)
            {
                Logger.Severe("Fatal Error at EndReadCallback : " + GetAddress(), e);
                HandleSocketException(e);
            }
            finally
            {
                BeginRead();
            }
        }

        private void HandleSocketException(Exception e)
        {
            var se = e as SocketException;
            if (se != null)
            {
                Logger.Warning(string.Format("Got socket error: {0}", se.SocketErrorCode));
            }
            else
            {
                Logger.Warning(e.Message, e);
            }
            _clientConnectionManager.DestroyConnection(this);
        }

        /// <exception cref="System.IO.IOException"></exception>
        private void Release()
        {
            if (!_live)
            {
                return;
            }
            _live = false;
            _writeQueue.CompleteAdding();

            if (_writeThread != null && _writeThread.IsAlive)
            {
                _writeThread.Interrupt();
            }
            if (Logger.IsFinestEnabled())
            {
                Logger.Finest(string.Format("Closing socket, address: {0} id: {1}", GetAddress(), _id));
            }

            _clientSocket.Shutdown(SocketShutdown.Both);
            _clientSocket.Close();

            _clientSocket = null;
        }

        private void StartAsyncProcess()
        {
            BeginRead();
            _writeThread = new Thread(WriteQueueLoop)
            {
                IsBackground = true,
                Priority = ThreadPriority.Highest,
                Name =
                    "hz-connection-write-" + Id
            };
            _writeThread.Start();
        }

        private void WriteQueueLoop()
        {
            while (!_writeQueue.IsAddingCompleted)
            {
                try
                {
                    if (_lastWritable == null)
                    {
                        _lastWritable = _writeQueue.Take();
                    }
                }
                catch (InvalidOperationException)
                {
                    //BlockingCollection is empty
                    if (_writeQueue.IsAddingCompleted)
                    {
                        return;
                    }
                }

                while (_sendBuffer.HasRemaining() && _lastWritable != null)
                {
                    var complete = _lastWritable.WriteTo(_sendBuffer);
                    if (complete)
                    {
                        //grap one from queue
                        ISocketWritable tmp;
                        _writeQueue.TryTake(out tmp);
                        _lastWritable = tmp;
                    }
                    else
                    {
                        break;
                    }
                }
                if (_sendBuffer.Position > 0)
                {
                    _sendBuffer.Flip();
                    try
                    {
                        SocketError socketError;
                        var sendByteSize = _clientSocket.Send(
                            _sendBuffer.Array(),
                            _sendBuffer.Position,
                            _sendBuffer.Remaining(),
                            SocketFlags.None, out socketError);

                        if (sendByteSize <= 0)
                        {
                            Close();
                            return;
                        }

                        if (socketError != SocketError.Success)
                        {
                            HandleSocketException(new SocketException((int) socketError));
                            return;
                        }

                        _sendBuffer.Position += sendByteSize;
                        //logger.Info("SEND BUFFER CALLBACK: pos:" + sendBuffer.Position + " remaining:" + sendBuffer.Remaining());

                        //if success case
                        if (_sendBuffer.HasRemaining())
                        {
                            _sendBuffer.Compact();
                        }
                        else
                        {
                            _sendBuffer.Clear();
                        }
                    }
                    catch (Exception e)
                    {
                        _lastWritable = null;
                        HandleSocketException(e);
                    }
                }
            }
        }
    }
}