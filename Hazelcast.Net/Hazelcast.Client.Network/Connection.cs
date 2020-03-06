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
using System.Threading.Tasks;
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
    /// <summary>
    /// Represents a socket connection to a member of the cluster.
    /// </summary>
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

        private readonly Socket _socket;
        private readonly Stream _stream;
        private readonly AtomicBoolean _live;
        private Thread _writeThread;
        private volatile ClientMessage _lastWritable;
        private readonly long _connectionStartTime;
        private IAsyncResult _reading;

        /// <summary>
        /// Gets a value indicating whether the connection is alive.
        /// </summary>
        public bool IsAlive => _live.Get();

        /// <summary>
        /// Gets the unique identifier of the connection.
        /// </summary>
        public int Id => _id;

        /// <summary>
        /// Gets the date and time of the last read.
        /// </summary>
        public DateTime LastRead { get; private set; }

        /// <summary>
        /// Gets the date and time of the last write.
        /// </summary>
        public DateTime LastWrite { get; private set; }

        public long ConnectionStartTime => _connectionStartTime;

        public Address RemoteAddress { get; set; }
        public Guid RemoteGuid { get; set; }
        public string ConnectedServerVersion { get; set; }

        /// <summary>
        /// Gets the reason for which the connection was closed.
        /// </summary>
        public string CloseReason { get; private set; }

        /// <summary>
        /// Gets the optional exception that caused the connection to be closed.
        /// </summary>
        public Exception CloseException { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Connection"/> class.
        /// </summary>
        /// <param name="connectionManager">The connection manager.</param>
        /// <param name="invocationService">The invocation service.</param>
        /// <param name="id">The unique identifier of the connection.</param>
        /// <param name="address">The address.</param>
        /// <param name="clientNetworkConfig">The network configuration.</param>
        public Connection(ConnectionManager connectionManager, InvocationService invocationService, int id, Address address, ClientNetworkConfig clientNetworkConfig)
        {
            _connectionManager = connectionManager;
            _id = id;

            try
            {
                _socket = CreateSocket(address, clientNetworkConfig);

                _sendBuffer = ByteBuffer.Allocate(BufferSize);
                _receiveBuffer = ByteBuffer.Allocate(BufferSize);

                _decoder = new ClientMessageDecoder(invocationService.HandleClientMessage);

                Stream stream = new NetworkStream(_socket, false);

                var sslConfig = clientNetworkConfig.GetSSLConfig();
                if (sslConfig.IsEnabled())
                {
                    var sslStream = new SslStream(stream, false,
                        (sender, certificate, chain, sslPolicyErrors) =>
                            RemoteCertificateValidationCallback(sender, certificate, chain, sslPolicyErrors, clientNetworkConfig),
                        null);
                    stream = sslStream;

                    var certificateName = sslConfig.GetCertificateName() ?? "";
                    var cerPath = sslConfig.GetCertificateFilePath();
                    var enabledSslProtocols = sslConfig.GetSslProtocol();
                    var checkCertificateRevocation = sslConfig.IsCheckCertificateRevocation();

                    var clientCertificates = GetClientCertificatesOrDefault(cerPath, sslConfig);

                    sslStream.AuthenticateAsClient(certificateName, clientCertificates, enabledSslProtocols, checkCertificateRevocation);

                    Logger.Info($"Client connection ready. Encrypted:{sslStream.IsEncrypted}, MutualAuthenticated:{sslStream.IsMutuallyAuthenticated} using ssl protocol:{sslStream.SslProtocol}");
                }

                _stream = stream;
                _live = new AtomicBoolean(true);
                _connectionStartTime = Clock.CurrentTimeMillis();
            }
            catch (Exception e)
            {
                if (_stream != null)
                {
                    _stream.Close();
                    _stream.Dispose();
                }
                if (_socket != null)
                {
                    _socket.Close();
                    _socket.Dispose();
                }
                throw new IOException($"Failed to open connection to {address} (see inner exception).", e);
            }
        }

        /// <summary>
        /// Tries to queue a message.
        /// </summary>
        /// <param name="request">The message.</param>
        /// <returns>A value indicating whether the message could be added.</returns>
        public bool TryQueue(ClientMessage request)
        {
            if (!IsAlive)
            {
                if (Logger.IsFinestEnabled)
                {
                    Logger.Finest($"Connection is closed, won't write packet -> {request}");
                }
                return false;
            }

            return _writeQueue.TryAdd(request);
        }

        public IPEndPoint GetLocalSocketAddress()
        {
            try
            {
                return (IPEndPoint) _socket?.LocalEndPoint;
            }
            catch (ObjectDisposedException)
            {
                return null;
            }
        }

        /// <summary>
        /// Initializes the network connection.
        /// </summary>
        public void NetworkInit()
        {
            _stream.Write(ClientProtocolInitBytes, 0, ClientProtocolInitBytes.Length);

            StartReader();
            StartWriter();
        }

        #region Security

        private static X509Certificate2Collection GetClientCertificatesOrDefault(string cerPath, SSLConfig sslConfig)
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

        private static bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors, ClientNetworkConfig clientNetworkConfig)
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

        #endregion

        #region Socket

        /// <summary>
        /// Creates the socket.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="clientNetworkConfig">The network configuration.</param>
        /// <returns>The socket.</returns>
        private Socket CreateSocket(Address address, ClientNetworkConfig clientNetworkConfig)
        {
            var isa = address.GetInetSocketAddress();
            var socketOptions = clientNetworkConfig.GetSocketOptions();

            var socket = new Socket(isa.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            if (socketOptions.GetLingerSeconds() > 0)
            {
                var lingerOption = new LingerOption(true, socketOptions.GetLingerSeconds());
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingerOption);
            }
            else
            {
                var lingerOption = new LingerOption(true, 0);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, lingerOption);
            }
            socket.NoDelay = socketOptions.IsTcpNoDelay();
            socket.ReceiveTimeout = socketOptions.GetTimeout() > 0 ? socketOptions.GetTimeout() : -1;

            var bufferSize = socketOptions.GetBufferSize() * 1024;
            if (bufferSize < 0)
            {
                bufferSize = BufferSize;
            }

            socket.SendBufferSize = bufferSize;
            socket.ReceiveBufferSize = bufferSize;

            var connectTimeout = clientNetworkConfig.GetConnectionTimeout() > -1
                ? clientNetworkConfig.GetConnectionTimeout()
                : ConnectionTimeout;

            var connectResult = socket.BeginConnect(address.GetInetAddress(), address.Port, null, null);

            if (connectResult.Wait(connectTimeout) && socket.Connected)
                return socket;

            // make sure to close & dispose the socket!
            socket.Close();
            socket.Dispose();
            throw new IOException($"Failed to open socket to {address}.");
        }

        #endregion

        #region Reader

        /// <summary>
        /// Starts the reader.
        /// </summary>
        private void StartReader()
        {
            if (!IsAlive || !_socket.Connected)
            {
                Close("Connection is not alive", null);
                return;
            }
            try
            {
                var buffer = _receiveBuffer.Array();
                var offset = _receiveBuffer.Position;
                var count = _receiveBuffer.Remaining();

                // using our own TaskToApm implementation, as we want to have access to
                // the underlying task, so that we can deal with exceptions - something
                // that BeginRead does not support - but, underneath, BeginRead uses
                // TaskToApm (the internal version)
                // _reading = _stream.BeginRead(buffer, offset, count, EndReadCallback, null);
                var task = _stream.ReadAsync(buffer, offset, count, CancellationToken.None);
                _reading = TaskToApm.Begin(task, EndReadCallback, null);

                // deal with exceptions - on Linux, if a connection is not properly closed,
                // the socket ends up throwing an exception that would remains unhandled
                // TODO: figure out why we don't properly close connections? (removing this
                //   line should randomly break a few tests on Linux)
                task.IgnoreExceptions(TaskContinuationOptions.NotOnRanToCompletion);
            }
            catch (Exception e)
            {
                Logger.Severe($"Caught exception when starting to read from {RemoteAddress}.", e);
                CloseOnException(e);
            }
        }

        /// <summary>
        /// Reader callback.
        /// </summary>
        /// <param name="asyncResult"></param>
        private void EndReadCallback(IAsyncResult asyncResult)
        {
            if (!IsAlive)
                return;

            _reading = null;

            try
            {
                // complete the read
                var receivedByteSize = TaskToApm.End<int>(asyncResult);
                if (receivedByteSize == 0)
                {
                    // socket was closed
                    CloseOnException(new TargetDisconnectedException(RemoteAddress, "Socket was closed."));
                    return;
                }

                _receiveBuffer.Position += receivedByteSize;
                _receiveBuffer.Flip();

                // decode
                _decoder.OnRead(_receiveBuffer);

                // housekeeping
                LastRead = DateTime.Now;
                if (_receiveBuffer.HasRemaining())
                    _receiveBuffer.Compact();
                else
                    _receiveBuffer.Clear();

                // read more
                StartReader();
            }
            catch (Exception e)
            {
                Logger.Severe($"Caught exception while reading from {RemoteAddress}.", e);
                CloseOnException(e);
            }
        }

        #endregion

        #region Writer

        /// <summary>
        /// Starts the writer.
        /// </summary>
        private void StartWriter()
        {
            _writeThread = new Thread(WriterThread)
            {
                IsBackground = true,
                Priority = ThreadPriority.Highest,
                Name = "hz-connection-write-" + Id
            };
            _writeThread.Start();
        }

        /// <summary>
        /// Writer thread body.
        /// </summary>
        private void WriterThread()
        {
            if (Logger.IsFinestEnabled)
                Logger.Finest("Enter writer thread.");

            try
            {
                while (WriterThreadLoop()) { }
            }
            catch (ThreadInterruptedException)
            {
                // this is fine
                if (Logger.IsFinestEnabled)
                    Logger.Finest("Writer thread interrupted!");
            }
            catch (Exception e)
            {
                // this is bad
                Logger.Severe("Caught exception in writer thread.", e);
            }

            if (Logger.IsFinestEnabled)
                Logger.Finest("Exit writer thread.");
        }

        /// <summary>
        /// Writer thread loop.
        /// </summary>
        /// <returns></returns>
        private bool WriterThreadLoop()
        {
            if (_writeQueue.IsAddingCompleted)
                return false; // no more messages

            try
            {
                if (_lastWritable == null)
                    _lastWritable = _writeQueue.Take();
            }
            catch // any exception implies that the queue is empty and completed
            {
                if (_writeQueue.IsAddingCompleted)
                    return false; // no more messages
            }

            // populate send buffer with client message
            while (_sendBuffer.HasRemaining() && _lastWritable != null)
            {
                var complete = _messageWriter.WriteTo(_sendBuffer, _lastWritable);
                if (!complete) break;

                // try to get next message from queue
                _lastWritable = _writeQueue.TryTake(out var tmp) ? tmp : null;
            }

            // flush send buffer to stream
            if (_sendBuffer.Position <= 0)
                return true; // can wait for more messages

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
                CloseOnException(e); // closes the connection & interrupts this thread
            }

            // can wait for more messages
            return true;
        }

        #endregion

        #region Connection Management

        /// <summary>
        /// Closes the connection on exception.
        /// </summary>
        /// <param name="e">The exception.</param>
        private void CloseOnException(Exception e)
        {
            Logger.Warning(e is SocketException se
                ? $"Closing connection on socket exception with code {se.SocketErrorCode}."
                : $"Closing connection on {e.GetType().Name} ({this})", e);

            Close("Caught exception.", e);
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        /// <param name="reason">The reason for closing the connection.</param>
        /// <param name="cause">The optional exception causing the connection to be closed.</param>
        public void Close(string reason, Exception cause = null)
        {
            if (!_live.CompareAndSet(true, false))
                return;

            CloseReason = reason;
            CloseException = cause;

            // stops the writing thread
            _writeQueue.CompleteAdding();

            if (_writeThread != null && _writeThread.IsAlive)
            {
                _writeThread.Interrupt();
                _writeThread.Join();
            }

            if (Logger.IsFinestEnabled)
                Logger.Finest($"Closing connection id:{_id} socket to {RemoteAddress}.");

            try
            {
                // close and dispose the stream
                _stream.Close();
                _stream.Dispose();

                // shutdown, close and dispose the socket
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
                _socket.Dispose();
            }
            catch (Exception e)
            {
                if (Logger.IsFinestEnabled)
                    Logger.Finest("Caught exception while closing the connection.", e);
            }

            // notify connection manager
            _connectionManager.OnConnectionClose(this);
        }

        #endregion

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