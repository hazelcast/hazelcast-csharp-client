// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Messaging;
using Hazelcast.Models;
using Hazelcast.Networking;
using Hazelcast.Protocol;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    // NOTES
    //
    // older versions of the code had a background task that would check every invocation
    // and terminate them if their attached client was not alive anymore - but really that
    // should be taken care of by the OnShutdown handler when the socket goes down.
    //
    // also, every invocation has a timeout by default, so unless users set an absurdly
    // long timeout, invocations *will* be collected eventually and we do not leak.

    /// <summary>
    /// Represents a connection to a cluster member.
    /// </summary>
    internal class MemberConnection : IAsyncDisposable
    {
        internal static readonly byte[] ClientProtocolInitBytes = { 67, 80, 50 }; //"CP2";

        private readonly ConcurrentDictionary<long, Invocation> _invocations = new ConcurrentDictionary<long, Invocation>();

        private readonly Authenticator _authenticator;
        private readonly MessagingOptions _messagingOptions;
        private readonly NetworkingOptions _networkingOptions;
        private readonly SslOptions _sslOptions;
        private readonly ISequence<long> _correlationIdSequence;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        private bool _readonlyProperties; // whether some properties (_onXxx) are readonly
        private Action<ClientMessage> _receivedEvent;
        private Func<MemberConnection, ValueTask> _closed;

        private ClientSocketConnection _socketConnection;
        private ClientMessageConnection _messageConnection;

        private readonly object _mutex = new object();
        private volatile bool _disposed;
        private volatile bool _active;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberConnection"/> class.
        /// </summary>
        /// <param name="address">The network address.</param>
        /// <param name="authenticator">The authenticator.</param>
        /// <param name="messagingOptions">Messaging options.</param>
        /// <param name="networkingOptions">Networking options.</param>
        /// <param name="sslOptions">SSL options.</param>
        /// <param name="correlationIdSequence">A sequence of unique correlation identifiers.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public MemberConnection(NetworkAddress address, Authenticator authenticator, MessagingOptions messagingOptions, NetworkingOptions networkingOptions, SslOptions sslOptions, ISequence<long> correlationIdSequence, ILoggerFactory loggerFactory)
        {
            Address = address ?? throw new ArgumentNullException(nameof(address));
            _authenticator = authenticator ?? throw new ArgumentNullException(nameof(authenticator));
            _messagingOptions = messagingOptions ?? throw new ArgumentNullException(nameof(messagingOptions));
            _networkingOptions = networkingOptions ?? throw new ArgumentNullException(nameof(networkingOptions));
            _sslOptions = sslOptions ?? throw new ArgumentNullException(nameof(sslOptions));
            _correlationIdSequence = correlationIdSequence ?? throw new ArgumentNullException(nameof(correlationIdSequence));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<MemberConnection>();

            HConsole.Configure(x => x.Configure<MemberConnection>().SetIndent(4).SetPrefix("MBR.CONN"));
        }

        #region Events

        /// <summary>
        /// Gets or sets an action that will be executed when the connection receives an event message.
        /// </summary>
        public Action<ClientMessage> ReceivedEvent
        {
            get => _receivedEvent;
            set
            {
                if (_readonlyProperties)
                    throw new InvalidOperationException(ExceptionMessages.PropertyIsNowReadOnly);
                _receivedEvent = value;
            }
        }

        /// <summary>
        /// Gets or sets an action that will be executed when the connection has closed.
        /// </summary>
        public Func<MemberConnection, ValueTask> Closed
        {
            get => _closed;
            set
            {
                if (_readonlyProperties)
                    throw new InvalidOperationException(ExceptionMessages.PropertyIsNowReadOnly);
                _closed = value;
            }
        }

        #endregion

        /// <summary>
        /// Gets the unique identifier of this connection.
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Whether the connection is active.
        /// </summary>
        public bool Active => _active;

        /// <summary>
        /// Gets the unique identifier of the cluster member that this connection is connected to.
        /// </summary>
        public Guid MemberId { get; private set; }

        /// <summary>
        /// Gets the unique identifier of the cluster that this connection is connected to.
        /// </summary>
        public Guid ClusterId { get; private set; }

        /// <summary>
        /// Gets the network address the client is connected to.
        /// </summary>
        public NetworkAddress Address { get; }

        /// <summary>
        /// Gets the local endpoint of the socket connection.
        /// </summary>
        public IPEndPoint LocalEndPoint => _socketConnection.LocalEndPoint;

        /// <summary>
        /// Gets the authentication principal.
        /// </summary>
        public string Principal { get; private set; }

        /// <summary>
        /// Gets the date and time when the connection was established.
        /// </summary>
        public DateTimeOffset ConnectTime { get; private set; }

        /// <summary>
        /// Gets the date and time when bytes where last read by the client.
        /// </summary>
        public DateTime LastReadTime => _socketConnection?.LastReadTime ?? DateTime.MinValue;

        /// <summary>
        /// Gets the date and time when bytes where last written by the client.
        /// </summary>
        public DateTime LastWriteTime => _socketConnection?.LastWriteTime ?? DateTime.MinValue;

        /// <summary>
        /// Connects the client to the server.
        /// </summary>
        /// <param name="clusterState">The cluster state.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the client is connected.</returns>
        public async ValueTask<AuthenticationResult> ConnectAsync(ClusterState clusterState, CancellationToken cancellationToken)
        {
            // as soon as we even try to connect, some properties cannot change anymore
            _readonlyProperties = true;

            // MessageConnection is just a wrapper around a true SocketConnection, and
            // the SocketConnection must be open *after* everything has been wired

            _socketConnection = new ClientSocketConnection(Id, Address.IPEndPoint, _networkingOptions, _sslOptions, _loggerFactory)
            { OnShutdown = OnSocketShutdown };

            _messageConnection = new ClientMessageConnection(_socketConnection, _loggerFactory)
            { OnReceiveMessage = ReceiveMessage };

            HConsole.Configure(x => x.Configure(_messageConnection).SetIndent(8).SetPrefix($"CLT.MSG [{Id.ToShortString()}]"));

            AuthenticationResult result;
            try
            {
                // connect
                await _socketConnection.ConnectAsync(cancellationToken).CfAwait();
                _logger.IfDebug()?.LogDebug("Established connection {Id} to {Address}.", Id.ToShortString(), Address);

                // send protocol bytes
                var sent = await _socketConnection.SendAsync(ClientProtocolInitBytes, ClientProtocolInitBytes.Length, cancellationToken).CfAwait();
                if (!sent) throw new ConnectionException("Failed to send protocol bytes.");

                // authenticate (does not return null, throws if it fails to authenticate)
                result = await _authenticator
                    .AuthenticateAsync(this, clusterState.ClusterName, clusterState.ClientId, clusterState.ClientName, clusterState.Options.Labels, cancellationToken)
                    .CfAwait();
            }
            catch
            {
                lock (_mutex) _disposed = true;
                await DisposeInnerConnectionAsync().CfAwait();
                throw;
            }

            MemberId = result.MemberId;
            ClusterId = result.ClusterId;
            ConnectTime = DateTimeOffset.Now;
            Principal = result.Principal;

            bool disposed;
            lock (_mutex)
            {
                disposed = _disposed;
                _active = !_disposed;
            }

            if (disposed)
            {
                await DisposeInnerConnectionAsync().CfAwait();
                throw new ConnectionException("Failed to connect.");
            }

            return result;
        }

        /// <summary>
        /// Handles connection shutdown.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns>A task that will complete when the shutdown has been handled.</returns>
#pragma warning disable IDE0079 // Remove unnecessary suppression - false positive ?!
#pragma warning disable CA1801 // Parameter is never used.
        private async ValueTask OnSocketShutdown(SocketConnectionBase connection)
#pragma warning restore CA1801
#pragma warning restore IDE0079
        {
            await DisposeAsync().CfAwait();
        }

        /// <summary>
        /// Handles messages.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="message">The message.</param>
        /// <returns>A task that will complete when the message has been handled.</returns>
#pragma warning disable IDE0079 // Remove unnecessary suppression - false positive ?!
#pragma warning disable CA1801 // Parameter is never used.
        private void ReceiveMessage(ClientMessageConnection connection, ClientMessage message)
#pragma warning restore CA1801
#pragma warning restore IDE0079
        {
            // proceed, regardless of _active, because why not?

            if (message.IsEvent)
            {
                HConsole.WriteLine(this, $"Receive event {Id.ToShortString()}:{message.CorrelationId}" +
                                         HConsole.Lines(this, 2, message.Dump(HConsole.Level(this))));
                ReceiveEvent(message); // should not throw
                return;
            }

            if (message.IsBackupEvent)
            {
                HConsole.WriteLine(this, $"Receive backup event {Id.ToShortString()}:{message.CorrelationId}" +
                                         HConsole.Lines(this, 2, message.Dump(HConsole.Level(this))));

                // backup events are not supported
                _logger.IfWarning()?.LogWarning("Ignoring unsupported backup event.");
                return;
            }

            // message has to be a response
            HConsole.WriteLine(this, $"Receive response {Id.ToShortString()}:{message.CorrelationId} from {MemberId.ToShortString()} at {Address}" +
                                     HConsole.Lines(this, 2, message.Dump(HConsole.Level(this))));

            // find the corresponding invocation
            // and remove invocation
            if (!_invocations.TryRemove(message.CorrelationId, out var invocation))
            {
                // orphan messages are ignored (but logged)
                _logger.IfWarning()?.LogWarning("Received message for unknown invocation {Id}:{CorrelationId}.", Id.ToShortString(), message.CorrelationId);
                HConsole.WriteLine(this, $"Unknown invocation {Id.ToShortString()}:{message.CorrelationId}");
                return;
            }

            // receive exception or message
            if (message.IsException)
                ReceiveException(invocation, message); // should not throw
            else
                ReceiveResponse(invocation, message); // should not throw
        }

        // ReceiveMessage -> event message
        private void ReceiveEvent(ClientMessage message)
        {
            try
            {
                HConsole.WriteLine(this, $"Raise event {Id.ToShortString()}:{message.CorrelationId}");
                _receivedEvent(message);
            }
            catch (Exception e)
            {
                // _onReceiveEventMessage should just queue the event and not fail - if it fails
                // then some nasty internal error is happening - log, at least, make some noise

                _logger.IfWarning()?.LogWarning(e, "Failed to raise event {Id}:{CorrelationId}.", Id.ToShortString(), message.CorrelationId);
            }
        }

        // ReceiveMessage -> exception message
        private void ReceiveException(Invocation invocation, ClientMessage message)
        {
            Exception exception;
            try
            {
                exception = RemoteExceptions.CreateException(MemberId, ErrorsCodec.Decode(message));
            }
            catch (Exception e)
            {
                exception = e;
            }

            HConsole.WriteLine(this, $"Fail invocation {Id.ToShortString()}:{message.CorrelationId}");
            invocation.TrySetException(exception);
        }

        // ReceiveMessage -> response message
        private void ReceiveResponse(Invocation invocation, ClientMessage message)
        {
            HConsole.WriteLine(this, $"Complete invocation {Id.ToShortString()}:{message.CorrelationId}");

            // returns immediately, releases the invocation task
            invocation.TrySetResult(message);
        }

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>A task that will complete when the response has been received, and represents the response.</returns>
        /// <remarks>
        /// <para>The operation must complete within the default operation timeout specified by the networking options.</para>
        /// </remarks>
        public async Task<ClientMessage> SendAsync(ClientMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            // assign a unique identifier to the message
            // and send in one fragment, with proper flags
            message.CorrelationId = _correlationIdSequence.GetNext();
            message.Flags |= ClientMessageFlags.BeginFragment | ClientMessageFlags.EndFragment;

            // create the invocation
            var invocation = new Invocation(message, _messagingOptions, this);

            // and send
            return await SendAsync(invocation).CfAwait();
        }

        /// <summary>
        /// Sends an invocation message.
        /// </summary>
        /// <param name="invocation">The invocation.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <remarks>After first connection is established, <b>TargetDisconnectedException</b> may be thrown. If
        // the connected member reports an IP address different from the configured one. In this case, connection 
        // will be switched, and an exception will be thrown.</remarks>
        /// <returns>A task that will complete when the response has been received, and represents the response.</returns>
        public Task<ClientMessage> SendAsync(Invocation invocation, CancellationToken cancellationToken = default)
        {
            return SendAsyncInternal(invocation, cancellationToken);
        }

        private async Task<ClientMessage> SendAsyncInternal(Invocation invocation, CancellationToken cancellationToken)
        {
            if (invocation == null) throw new ArgumentNullException(nameof(invocation));

            // _active     false ----> true ----> false
            // _disposed   false            ----> true
            //             ^--------------^
            //               here, ok to send messages, either active, or connecting

            // adds the invocation, so that it can be completed as soon as the response is received
            // it will be removed when receiving the response (or error or timeout or...)
            lock (_mutex)
            {
                if (_disposed) throw new TargetDisconnectedException();
                _invocations[invocation.CorrelationId] = invocation;
            }

            HConsole.WriteLine(this, $"Send message {Id.ToShortString()}:{invocation.CorrelationId} to {MemberId.ToShortString()} at {Address}" +
                                     HConsole.Lines(this, 1, invocation.RequestMessage.Dump(HConsole.Level(this))));

            // actually send the message
            bool success;
            Exception captured = null;
            try
            {
                success = await _messageConnection.SendAsync(invocation.RequestMessage, cancellationToken).CfAwait();
            }
            catch (Exception e)
            {
                HConsole.WriteLine(this, "Exception while sending: " + e);
                captured = e;
                _invocations.TryRemove(invocation.CorrelationId, out _);
                if (_active) throw; // if not active, better throw a disconnected exception below
                success = false;
            }

            if (!success)
            {
                _invocations.TryRemove(invocation.CorrelationId, out _);
                HConsole.WriteLine(this, "Failed to send a message.");

                if (!_active)
                    throw new TargetDisconnectedException(captured);

                // TODO: we need a better exception
                throw new TargetUnreachableException(captured);
            }

            // now wait for the response
            HConsole.WriteLine(this, "Wait for response...");

            try
            {
                // propagate the cancellationToken to the invocation
#if !NETSTANDARD2_0
                await
#endif
                using var reg = cancellationToken.Register(invocation.TrySetCanceled);

                var response = await invocation.Task.CfAwait();
                HConsole.WriteLine(this, "Received response");
                return response;
            }
            catch (Exception e)
            {
                HConsole.WriteLine(this, $"Failed ({e})");
                _invocations.TryRemove(invocation.CorrelationId, out _);
                throw;
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            // note: DisposeAsync should not throw (CA1065)

            bool active;
            lock (_mutex)
            {
                if (_disposed) return;
                _disposed = true;
                active = _active;
                _active = false;
            }

            try
            {
                // if we were connected, we need to trigger the closed event
                if (active) await _closed.AwaitEach(this).CfAwait(); // may throw, never knows
            }
            catch (Exception e)
            {
                _logger.IfWarning()?.LogWarning(e, "Caught an exception while raising Closed.");
            }

            // if if we were not yet active / connected, we might have ONE invocation
            // pending: the authentication one - it is important to abort it too

            // capture all invocations, _disposed is true so no new invocation can be
            // accepted, and if one invocation completes, TrySetException will just do
            // nothing
            var invocations = _invocations.Values;
            foreach (var invocation in invocations)
                invocation.TrySetException(new TargetDisconnectedException()); // does not throw

            // ConnectAsync would deal with the situation
            if (!active) return;

            // then kill our inner connection
            await DisposeInnerConnectionAsync().CfAwait();

            _logger.IfDebug()?.LogDebug("Connection {Id} closed and disposed.", Id.ToShortString());

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize - DisposeAsync too!
            GC.SuppressFinalize(this);
#pragma warning restore CA1816
        }

        private async Task DisposeInnerConnectionAsync()
        {
            // tear down inner connections
            if (_messageConnection != null) // also disposes the socket connection
                await _messageConnection.DisposeAsync().CfAwait(); // does not throw
            else if (_socketConnection != null)
                await _socketConnection.DisposeAsync().CfAwait(); // does not throw

            _messageConnection = null;
            _socketConnection = null;
        }
    }
}
