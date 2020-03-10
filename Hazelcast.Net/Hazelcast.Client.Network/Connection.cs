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
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.Client.Spi;
using Hazelcast.Config;
using Hazelcast.IO;
using Hazelcast.Logging;
using Hazelcast.Net.Ext;
using Hazelcast.Util;

namespace Hazelcast.Client.Network
{
    /// <summary>Holds the clientSocket to one of the members of Hazelcast ICluster. SSL encryption is used if configured.</summary>
    internal sealed class Connection
    {
        private readonly byte[] ClientProtocolInitBytes = {67, 80, 50}; //"CP2";

        private const int BufferSize = 1 << 17; //128k
        private const int ConnectionTimeout = 30000;
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof(Connection));

        private readonly int _id;
        private readonly ClientMessageDecoder _decoder;
        private readonly ClientMessageWriter _messageWriter = new ClientMessageWriter();
        private readonly ConnectionManager _connectionManager;
        private readonly ByteBuffer _receiveBuffer;
        private readonly ByteBuffer _sendBuffer;
        private readonly BlockingCollection<ClientMessage> _writeQueue = new BlockingCollection<ClientMessage>();

        private readonly Socket _clientSocket;
        private readonly Stream _stream;
        private readonly AtomicBoolean _live;
        private Thread _writeThread;
        private volatile ClientMessage _lastWritable;
        private readonly long _connectionStartTime;
        private IAsyncResult _reading;

        public bool IsAlive => _live.Get();

        public int Id => _id;

        public DateTime LastRead { get; private set; }

        public DateTime LastWrite { get; private set; }

        public long ConnectionStartTime => _connectionStartTime;

        public Address RemoteAddress { get; set; }
        public Guid RemoteGuid { get; set; }
        public string ConnectedServerVersion { get; set; }

        public string CloseReason { get; private set; }
        public Exception CloseCause { get; private set; }

        public Connection(ConnectionManager connectionManager, InvocationService invocationService, int id, Address address,
            ClientNetworkConfig clientNetworkConfig)
        {
            _connectionManager = connectionManager;
            _id = id;

            var isa = address.GetInetSocketAddress();
            var socketOptions = clientNetworkConfig.GetSocketOptions();
            try
            {
                _clientSocket = new Socket(isa.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                if (socketOptions.GetLingerSeconds() > 0)
                {
                    var lingerOption = new LingerOption(true, socketOptions.GetLingerSeconds());
                    _clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingerOption);
                }
                else
                {
                    var lingerOption = new LingerOption(true, 0);
                    _clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, lingerOption);
                }
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
                var socketResult = _clientSocket.BeginConnect(address.GetInetAddress(), address.Port, null, null);

                if (!socketResult.AsyncWaitHandle.WaitOne(connectionTimeout, true) || !_clientSocket.Connected)
                {
                    // NOTE, MUST CLOSE THE SOCKET
                    _clientSocket.Close();
                    throw new IOException("Failed to connect to " + address);
                }
                _sendBuffer = ByteBuffer.Allocate(BufferSize);
                _receiveBuffer = ByteBuffer.Allocate(BufferSize);

                _decoder = new ClientMessageDecoder(invocationService.HandleClientMessage);

                var networkStream = new NetworkStream(_clientSocket, false);
                var sslConfig = clientNetworkConfig.GetSSLConfig();
                if (sslConfig.IsEnabled())
                {
                    var sslStream = new SslStream(networkStream, false,
                        (sender, certificate, chain, sslPolicyErrors) =>
                            RemoteCertificateValidationCallback(sender, certificate, chain, sslPolicyErrors, clientNetworkConfig),
                        null);
                    var certificateName = sslConfig.GetCertificateName() ?? "";
                    var cerPath = sslConfig.GetCertificateFilePath();
                    var enabledSslProtocols = sslConfig.GetSslProtocol();
                    var checkCertificateRevocation = sslConfig.IsCheckCertificateRevocation();

                    var clientCertificates = GetClientCertificatesOrDefault(cerPath, sslConfig);

                    sslStream.AuthenticateAsClient(certificateName, clientCertificates, enabledSslProtocols,
                        checkCertificateRevocation);

                    Logger.Info(
                        $"Client connection ready. Encrypted:{sslStream.IsEncrypted}, MutualAuthenticated:{sslStream.IsMutuallyAuthenticated} using ssl protocol:{sslStream.SslProtocol}");

                    _stream = sslStream;
                }
                else
                {
                    _stream = networkStream;
                }
                _live = new AtomicBoolean(true);
                _connectionStartTime = Clock.CurrentTimeMillis();
            }
            catch (Exception e)
            {
                _clientSocket.Close();
                if (_stream != null)
                {
                    _stream.Close();
                }
                throw new IOException("Cannot init connection.", e);
            }
        }

        static X509Certificate2Collection GetClientCertificatesOrDefault(string cerPath, SSLConfig sslConfig)
        {
            if (cerPath == null)
            {
                return null;
            }

            var clientCertificates = new X509Certificate2Collection();
            try
            {
                clientCertificates.Import(cerPath, sslConfig.GetCertificatePassword(), X509KeyStorageFlags.DefaultKeySet);
            }
            catch (Exception)
            {
                Logger.Finest($"Cannot load client certificate:{cerPath}.");
                throw;
            }

            return clientCertificates;
        }

        private bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors sslPolicyErrors, ClientNetworkConfig clientNetworkConfig)
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

        public bool TryAdd(ClientMessage request)
        {
            if (!IsAlive)
            {
                if (Logger.IsFinestEnabled)
                {
                    Logger.Finest($"Connection is closed, won't write packet -> {request}");
                }
                return false;
            }
            if (_writeQueue.TryAdd(request))
            {
                return true;
            }
            return false;
        }

        public IPEndPoint GetLocalSocketAddress()
        {
            try
            {
                return (IPEndPoint) _clientSocket?.LocalEndPoint;
            }
            catch (ObjectDisposedException)
            {
                return null;
            }
        }

        public void NetworkInit()
        {
            _stream.Write(ClientProtocolInitBytes, 0, ClientProtocolInitBytes.Length);
            StartReadWriteLoop();
        }

        private void BeginRead()
        {
            if (!IsAlive || !_clientSocket.Connected)
            {
                Close("Connection is not alive", null);
                return;
            }
            try
            {
                _reading = _stream.BeginRead(_receiveBuffer.Array(), _receiveBuffer.Position, _receiveBuffer.Remaining(), EndReadCallback,
                    null);
            }
            catch (Exception e)
            {
                Logger.Severe("Fatal Error at BeginRead : " + RemoteAddress, e);
                HandleSocketException(e);
            }
        }

        private void EndReadCallback(IAsyncResult asyncResult)
        {
            if (!IsAlive)
            {
                return;
            }

            _reading = null;

            try
            {
                var receivedByteSize = _stream.EndRead(asyncResult);
                if (receivedByteSize == 0)
                {
                    //socket was closed
                    HandleSocketException(new TargetDisconnectedException(RemoteAddress, "Socket was closed."));
                    return;
                }
                _receiveBuffer.Position += receivedByteSize;
                _receiveBuffer.Flip();

                _decoder.OnRead(_receiveBuffer);
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
                Logger.Severe($"Fatal Error at EndReadCallback : {RemoteAddress}", e);
                HandleSocketException(e);
            }
        }

        private void HandleSocketException(Exception e)
        {
            if (!IsAlive)
            {
                // connection is already closed
                return;
            }

            if (e is SocketException se)
            {
                Logger.Warning($"Got socket error: {se.SocketErrorCode}");
            }
            else
            {
                Logger.Warning("Got exception in connection " + this + ":", e);
            }
            Close(null, e);
        }

        private void StartReadWriteLoop()
        {
            BeginRead();
            _writeThread = new Thread(WriteQueueLoop)
            {
                IsBackground = true, Priority = ThreadPriority.Highest, Name = "hz-connection-write-" + Id
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
                        var complete = _messageWriter.WriteTo(_sendBuffer, _lastWritable);
                        if (complete)
                        {
                            //take one from queue
                            _lastWritable = _writeQueue.TryTake(out var tmp) ? tmp : null;
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
                            LastWrite = DateTime.Now;
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
                if (Logger.IsFinestEnabled)
                {
                    Logger.Finest("Writer thread interrupted, stopping...");
                }
            }
        }

        public void Close(string reason, Exception cause)
        {
            if (!_live.CompareAndSet(true, false))
            {
                return;
            }
            CloseCause = cause;
            CloseReason = reason;
            //
            _writeQueue.CompleteAdding();

            if (_writeThread != null && _writeThread.IsAlive)
            {
                _writeThread.Interrupt();
                _writeThread.Join();
            }
            if (Logger.IsFinestEnabled)
            {
                Logger.Finest($"Closing socket, address: {RemoteAddress} id: {_id}");
            }

            try
            {
                // close the stream
                _stream.Close();

                // give the stream a chance do end a read operation
                // (helps avoid unobserved exception on Linux)
                try
                {
                    _stream.EndRead(_reading);
                }
                catch
                {
                    // nothing
                }

                // and *then* dispose the stream
                _stream.Dispose();

                // and *then* kill the socket
                _clientSocket.Shutdown(SocketShutdown.Both);
                _clientSocket.Close();
            }
            catch (Exception e)
            {
                if (Logger.IsFinestEnabled)
                {
                    Logger.Finest("Exception occured during socket shutdown", e);
                }
            }
            _connectionManager.OnConnectionClose(this);
        }

        private bool Equals(Connection other)
        {
            return _id == other._id;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is Connection other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _id;
        }

        public override string ToString() {
            return "Connection{"
                   + "alive=" + IsAlive
                   + ", connectionId=" + _id
                   + ", remoteAddress=" + RemoteAddress
                   + ", lastReadTime=" + LastRead
                   + ", lastWriteTime=" + LastWrite
                   + ", connected server version=" + ConnectedServerVersion
                   + '}';
        }

    }
}