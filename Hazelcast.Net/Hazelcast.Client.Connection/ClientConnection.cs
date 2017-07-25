// Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.
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
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
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
    /// <summary>Holds the clientSocket to one of the members of Hazelcast ICluster. SSL encription is used if configured.</summary>
    internal sealed class ClientConnection
    {
        public const int BufferSize = 1 << 15; //32k
        public const int SocketReceiveBufferSize = 1 << 15; //32k
        public const int SocketSendBufferSize = 1 << 15; //32k

        private const int ConnectionTimeout = 30000;
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof(ClientConnection));

        private readonly ClientMessageBuilder _builder;
        private readonly ClientConnectionManager _clientConnectionManager;
        private readonly int _id;
        private readonly ByteBuffer _receiveBuffer;
        private readonly ByteBuffer _sendBuffer;
        private readonly BlockingCollection<ISocketWritable> _writeQueue = new BlockingCollection<ISocketWritable>();

        private readonly Socket _clientSocket;
        private readonly Stream _stream;
        private volatile bool _isHeartBeating = true;
        private bool _isOwner;
        private volatile ISocketWritable _lastWritable;
        private readonly AtomicBoolean _live;
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
            var socketFactory = socketOptions.GetSocketFactory() ?? new DefaultSocketFactory();
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

                var bufferSize = socketOptions.GetBufferSize() * 1024;
                if (bufferSize < 0)
                {
                    bufferSize = BufferSize;
                }

                _clientSocket.SendBufferSize = bufferSize;
                _clientSocket.ReceiveBufferSize = bufferSize;

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

                var networkStream = new NetworkStream(_clientSocket, false);
                if (clientNetworkConfig.GetSSLConfig().IsEnabled())
                {
                    var sslStream = new SslStream(networkStream, false,
                        (sender, certificate, chain, sslPolicyErrors) =>
                            RemoteCertificateValidationCallback(sender, certificate, chain, sslPolicyErrors,
                                clientNetworkConfig), null);
                    var certificateName = clientNetworkConfig.GetSSLConfig().GetCertificateName() ?? "";
                    sslStream.AuthenticateAsClient(certificateName);
                    _stream = sslStream;
                }
                else
                {
                    _stream = networkStream;
                }
                _live = new AtomicBoolean(true);
            }
            catch (Exception e)
            {
                _clientSocket.Close();
                if (_stream != null)
                {
                    _stream.Close();
                }
                throw new IOException("Cannot connect! Socket error:" + e.Message);
            }
        }

        public bool Live
        {
            get { return _live.Get(); }
        }

        public int Id
        {
            get { return _id; }
        }

        public DateTime LastRead { get; private set; }

        public bool IsHeartBeating
        {
            get { return _isHeartBeating; }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void Close()
        {
            if (!_live.CompareAndSet(true, false))
            {
                return;
            }
            _writeQueue.CompleteAdding();

            if (_writeThread != null && _writeThread.IsAlive)
            {
                _writeThread.Interrupt();
            }
            if (Logger.IsFinestEnabled())
            {
                Logger.Finest(string.Format("Closing socket, address: {0} id: {1}", GetAddress(), _id));
            }

            try
            {
                _stream.Close();
                _clientSocket.Shutdown(SocketShutdown.Both);
                _clientSocket.Close();

            }
            catch (Exception e)
            {
                if (Logger.IsFinestEnabled())
                {
                    Logger.Finest("Exception occured during socket shutdown", e);
                }
            }
        }

        public Address GetAddress()
        {
            return _member != null ? _member.GetAddress() : null;
        }

        public IPEndPoint GetLocalSocketAddress()
        {
            try
            {
                return _clientSocket != null ? (IPEndPoint)_clientSocket.LocalEndPoint : null;
            }
            catch (ObjectDisposedException)
            {
                return null;
            }
        }

        public IMember Member
        {
            get { return _member; }

            set { _member = value; }
        }

        public void HeartbeatFailed()
        {
            _isHeartBeating = false;
        }

        public void HeartbeatSucceeded()
        {
            _isHeartBeating = true;
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
            if (!Live)
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
        public void Init(ISocketInterceptor socketInterceptor)
        {
            var initBytes = Encoding.UTF8.GetBytes(Protocols.ClientBinaryNew);
            _stream.Write(initBytes, 0, initBytes.Length);
            if (socketInterceptor != null)
            {
                socketInterceptor.OnConnect(_clientSocket);
            }
            StartReadWriteLoop();
        }

        public bool IsOwner()
        {
            return _isOwner;
        }

        public void SetOwner()
        {
            _isOwner = true;
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
                _stream.BeginRead(_receiveBuffer.Array(), _receiveBuffer.Position, _receiveBuffer.Remaining(),
                    EndReadCallback, null);
            }
            catch (Exception e)
            {
                Logger.Severe("Fatal Error at BeginRead : " + GetAddress(), e);
                HandleSocketException(e);
            }
        }

        private bool CheckLive()
        {
            if (!Live)
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
            if (!Live)
            {
                return;
            }

            try
            {
                var receivedByteSize = _stream.EndRead(asyncResult);
                if (receivedByteSize == 0)
                {
                    //socket was closed
                    HandleSocketException(new TargetDisconnectedException(GetAddress(), "Socket was closed."));
                    return;
                }
                _receiveBuffer.Position += receivedByteSize;
                _receiveBuffer.Flip();

                _builder.OnData(_receiveBuffer);
                LastRead = DateTime.Now;
                if (_receiveBuffer.HasRemaining())
                {
                    _receiveBuffer.Compact();
                }
                else
                {
                    _receiveBuffer.Clear();
                }
                BeginRead();
            }
            catch (Exception e)
            {
                Logger.Severe("Fatal Error at EndReadCallback : " + GetAddress(), e);
                HandleSocketException(e);
            }
        }

        private void HandleSocketException(Exception e)
        {
            if (!Live)
            {
                // connection is already closed
                return;
            }

            var se = e as SocketException;
            if (se != null)
            {
                Logger.Warning(string.Format("Got socket error: {0}", se.SocketErrorCode));
            }
            else
            {
                Logger.Warning("Got exception in connection " + this + ":", e);
            }
            _clientConnectionManager.DestroyConnection(this, e);
        }

        private void StartReadWriteLoop()
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
            try
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
                    catch (Exception)
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
                            _stream.Write(_sendBuffer.Array(), _sendBuffer.Position, _sendBuffer.Remaining());
                            _sendBuffer.Clear();
                        }
                        catch (Exception e)
                        {
                            _lastWritable = null;
                            HandleSocketException(e);
                        }
                    }
                }

            }
            catch (ThreadInterruptedException)
            {
                if (Logger.IsFinestEnabled())
                {
                    Logger.Finest("Writer thread interreptud, stopping...");
                }
            }
        }

        private bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors, ClientNetworkConfig clientNetworkConfig)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            var validation = true;

            if (sslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateChainErrors))
            {
                var isValidateChain = clientNetworkConfig.GetSSLConfig().IsValidateCertificateChain();
                if (isValidateChain)
                {
                    Logger.Warning("Certificate error:" + sslPolicyErrors);
                    validation = false;
                }
                else
                {
                    Logger.Info("SSL Configured to ignore Certificate chain validation. Ignoring:");
                }
                foreach (var status in chain.ChainStatus)
                {
                    Logger.Info("Certificate chain status:" + status.StatusInformation);
                }
            }
            if (sslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateNameMismatch))
            {
                var isValidateName = clientNetworkConfig.GetSSLConfig().IsValidateCertificateName();
                if (isValidateName)
                {
                    Logger.Warning("Certificate error:" + sslPolicyErrors);
                    validation = false;
                }
                else
                {
                    Logger.Info("Certificate name mismatched but client is configured to ignore Certificate name validation.");
                }
            }
            if (sslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateNotAvailable))
            {
                Logger.Warning("Certificate error:" + sslPolicyErrors);
                validation = false;
            }
            return validation;
        }
    }
}