﻿// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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

        private readonly ConcurrentDictionary<long, Invocation> _invocations
            = new ConcurrentDictionary<long, Invocation>();

        private readonly Authenticator _authenticator;
        private readonly MessagingOptions _messagingOptions;
        private readonly NetworkingOptions _networkingOptions;
        private readonly SslOptions _sslOptions;
        private readonly ISequence<int> _connectionIdSequence;
        private readonly ISequence<long> _correlationIdSequence;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        private bool _readonlyProperties; // whether some properties (_onXxx) are readonly
        private Action<ClientMessage> _receivedEvent;
        private Func<MemberConnection, ValueTask> _closed;

#pragma warning disable CA2213 // Disposable fields should be disposed - is owned by _messageConnection, which is disposed
        private ClientSocketConnection _socketConnection;
#pragma warning restore CA2213 // Disposable fields should be disposed
        private ClientMessageConnection _messageConnection;
        private volatile int _disposed;

        private BackgroundTask _bgTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberConnection"/> class.
        /// </summary>
        /// <param name="address">The network address.</param>
        /// <param name="authenticator">The authenticator.</param>
        /// <param name="messagingOptions">Messaging options.</param>
        /// <param name="networkingOptions">Networking options.</param>
        /// <param name="sslOptions">SSL options.</param>
        /// <param name="connectionIdSequence">A sequence of unique connection identifiers.</param>
        /// <param name="correlationIdSequence">A sequence of unique correlation identifiers.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        /// <remarks>
        /// <para>The <paramref name="connectionIdSequence"/> parameter can be used to supply a
        /// sequence of unique connection identifiers. This can be convenient for tests, where
        /// using unique identifiers across all clients can simplify debugging.</para>
        /// </remarks>
        public MemberConnection(NetworkAddress address, Authenticator authenticator, MessagingOptions messagingOptions, NetworkingOptions networkingOptions, SslOptions sslOptions, ISequence<int> connectionIdSequence, ISequence<long> correlationIdSequence, ILoggerFactory loggerFactory)
        {
            Address = address ?? throw new ArgumentNullException(nameof(address));
            _authenticator = authenticator ?? throw new ArgumentNullException(nameof(authenticator));
            _messagingOptions = messagingOptions ?? throw new ArgumentNullException(nameof(messagingOptions));
            _networkingOptions = networkingOptions ?? throw new ArgumentNullException(nameof(networkingOptions));
            _sslOptions = sslOptions ?? throw new ArgumentNullException(nameof(sslOptions));
            _connectionIdSequence = connectionIdSequence ?? throw new ArgumentNullException(nameof(connectionIdSequence));
            _correlationIdSequence = correlationIdSequence ?? throw new ArgumentNullException(nameof(correlationIdSequence));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<MemberConnection>();

            HConsole.Configure(x => x.Set(this, config => config.SetIndent(4).SetPrefix("CLIENT")));
        }

        #region Events

        /// <summary>
        /// Gets or sets an action that will be executed when the connection receives a message.
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
        /// Gets the unique identifier of this client.
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Whether the client is active.
        /// </summary>
        public bool Active => _disposed == 0;

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
        /// Gets the date and time when bytes where last read by the client.
        /// </summary>
        public DateTime LastReadTime => _socketConnection?.LastReadTime ?? DateTime.MinValue;

        /// <summary>
        /// Gets the date and time when bytes where last written by the client.
        /// </summary>
        public DateTime LastWriteTime => _socketConnection?.LastWriteTime ?? DateTime.MinValue;

        /// <summary>
        /// Adds an invocation.
        /// </summary>
        /// <param name="invocation">The invocation.</param>
        private void AddInvocation(Invocation invocation)
            => _invocations[invocation.CorrelationId] = invocation;

        /// <summary>
        /// Removes an invocation.
        /// </summary>
        /// <param name="invocation">The invocation.</param>
        private void RemoveInvocation(Invocation invocation)
            => _invocations.TryRemove(invocation.CorrelationId, out _);

        /// <summary>
        /// Tries to remove an invocation.
        /// </summary>
        /// <param name="correlationId">The correlation identifier.</param>
        /// <param name="invocation">The invocation.</param>
        /// <returns>Whether an invocation with the specified correlation identifier was removed.</returns>
        private bool TryRemoveInvocation(long correlationId, out Invocation invocation)
            => _invocations.TryRemove(correlationId, out invocation);

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

            _socketConnection = new ClientSocketConnection(_connectionIdSequence.GetNext(), Address.IPEndPoint, _networkingOptions, _sslOptions, _loggerFactory)
                { OnShutdown = OnSocketShutdown };

            _messageConnection = new ClientMessageConnection(_socketConnection, _loggerFactory)
                { OnReceiveMessage = ReceiveMessage };

            HConsole.Configure(x => x.Set(_messageConnection, config => config.SetIndent(12).SetPrefix($"MSG.CLIENT [{_socketConnection.Id}]")));

            AuthenticationResult result;
            try
            {
                // connect
                await _socketConnection.ConnectAsync(cancellationToken).CfAwait();

                // send protocol bytes
                var sent = await _socketConnection.SendAsync(ClientProtocolInitBytes, ClientProtocolInitBytes.Length, cancellationToken).CfAwait();
                if (!sent) throw new InvalidOperationException("Failed to send protocol bytes.");

                // authenticate (does not return null)
                result = await _authenticator
                    .AuthenticateAsync(this, clusterState.ClusterName, clusterState.ClientId, clusterState.ClientName, clusterState.Options.Labels, cancellationToken)
                    .CfAwait();
            }
            catch
            {
                await _socketConnection.DisposeAsync().CfAwaitNoThrow();
                _socketConnection = null;
                _messageConnection = null;
                throw;
            }

            MemberId = result.MemberId;
            ClusterId = result.ClusterId;

            return result;
        }

        /// <summary>
        /// Handles connection shutdown.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns>A task that will complete when the shutdown has been handled.</returns>
        private void OnSocketShutdown(SocketConnectionBase connection)
        {
            Terminate();
        }

        /// <summary>
        /// Handles messages.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="message">The message.</param>
        /// <returns>A task that will complete when the message has been handled.</returns>
        private void ReceiveMessage(ClientMessageConnection connection, ClientMessage message)
        {
            if (message.IsEvent)
            {
                HConsole.WriteLine(this, $"Receive event [{message.CorrelationId}]" +
                                         HConsole.Lines(this, 1, message.Dump()));
                ReceiveEvent(message); // should not throw
                return;
            }

            if (message.IsBackupEvent)
            {
                HConsole.WriteLine(this, $"Receive backup event [{message.CorrelationId}]" +
                                         HConsole.Lines(this, 1, message.Dump()));

                // backup events are not supported
                //throw new NotSupportedException("Backup events are not supported here.");
                _logger.LogWarning("Ignoring unsupported backup event.");
                return;
            }

            // message has to be a response
            HConsole.WriteLine(this, $"Receive response [{message.CorrelationId}]" +
                                     HConsole.Lines(this, 1, message.Dump()));

            // find the corresponding invocation
            // and remove invocation
#pragma warning disable CA2000 // Dispose objects before losing scope - invocations are disposed by ClusterMessaging
            if (!TryRemoveInvocation(message.CorrelationId, out var invocation))
#pragma warning restore CA2000
            {
                // orphan messages are ignored (but logged)
                _logger.LogWarning($"Received message for unknown invocation [{message.CorrelationId}].");
                HConsole.WriteLine(this, $"Unknown invocation [{message.CorrelationId}]");
                return;
            }

            // TODO: threading and scheduling?
            // the code here, and whatever will happen when completion.SetResult(message) runs,
            // ie. the continuations on the completion tasks (unless configure-await?) runs on
            // the same thread and blocks the networking layer

            // receive exception or message
            if (message.IsException)
                ReceiveException(invocation, message); // should not throw
            else
                ReceiveResponse(invocation, message); // should not throw
        }

        private void ReceiveEvent(ClientMessage message)
        {
            try
            {
                HConsole.WriteLine(this, $"Raise event [{message.CorrelationId}].");
                _receivedEvent(message);
            }
            catch (Exception e)
            {
                // _onReceiveEventMessage should just queue the event and not fail - if it fails
                // then some nasty internal error is happening - log, at least, make some noise

                _logger.LogWarning(e, $"Failed to raise event [{message.CorrelationId}].");
            }
        }

        private void ReceiveException(Invocation invocation, ClientMessage message)
        {
            // try to be as safe as possible here

            Exception exception;
            try
            {
                exception = RemoteExceptions.CreateException(ErrorsCodec.Decode(message));
            }
            catch (Exception e)
            {
                exception = e;
            }

            try
            {
                HConsole.WriteLine(this, $"Fail invocation [{message.CorrelationId}].");
                invocation.TrySetException(exception);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"Failed to fail invocation [{message.CorrelationId}].");
                HConsole.WriteLine(this, $"Failed to fail invocation [{message.CorrelationId}].");
            }

        }

        private void ReceiveResponse(Invocation invocation, ClientMessage message)
        {
            try
            {
                HConsole.WriteLine(this, $"Complete invocation [{message.CorrelationId}].");

                // returns immediately, releases the invocation task
                invocation.TrySetResult(message);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"Failed to complete invocation [{message.CorrelationId}].");
                HConsole.WriteLine(this, $"Failed to complete invocation [{message.CorrelationId}].");

                try
                {
                    invocation.TrySetException(e);
                }
                catch { /* nothing much we can do */ }
            }
        }

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the response has been received, and represents the response.</returns>
        /// <remarks>
        /// <para>The operation must complete within the default operation timeout specified by the networking options.</para>
        /// </remarks>
        public async Task<ClientMessage> SendAsync(ClientMessage message, CancellationToken cancellationToken)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            // assign a unique identifier to the message
            // and send in one fragment, with proper flags
            message.CorrelationId = _correlationIdSequence.GetNext();
            message.Flags |= ClientMessageFlags.BeginFragment | ClientMessageFlags.EndFragment;

            // create the invocation
            using var invocation = new Invocation(message, _messagingOptions, this, cancellationToken);

            return await SendAsync(invocation).CfAwait();
        }

        /// <summary>
        /// Sends an invocation message.
        /// </summary>
        /// <param name="invocation">The invocation.</param>
        /// <returns>A task that will complete when the response has been received, and represents the response.</returns>
        public async Task<ClientMessage> SendAsync(Invocation invocation)
        {
            // this cannot be canceled, it will wait for a response forever, until either the
            // server responds, or the connection is closed, or something happens - but there
            // is no timeout
            return await SendAsyncInternal(invocation, CancellationToken.None);
        }

        // TaskEx.RunWithTimeout invokes SendAsyncInternal with a cancellation token composed
        // of the original invocation token and the timeout source - and that composed token
        // will cancel if either the original token cancels, or the timeout is reached.
        //
        // Internally, if the invocation monitors its own cancellation token and, if it cancels,
        // the invocation.Task is canceled too. But that is not the timeout token.

        private async Task<ClientMessage> SendAsyncInternal(Invocation invocation, CancellationToken cancellationToken)
        {
            if (invocation == null) throw new ArgumentNullException(nameof(invocation));

            // adds the invocation, it will be removed when receiving the response (or timeout or...)
            AddInvocation(invocation);

            HConsole.WriteLine(this, $"Send message [{invocation.CorrelationId}]" +
                                     HConsole.Lines(this, 1, invocation.RequestMessage.Dump()));

            // actually send the message
            var success = false;
            var active = true;
            try
            {
                success = await _messageConnection.SendAsync(invocation.RequestMessage, cancellationToken).CfAwait();
            }
            catch
            {
                RemoveInvocation(invocation);
                active = Active;
                if (active) throw;

                // not active anymore: ignore this exception, throw a disconnected exception below
            }

            if (!Active || !active)
            {
                // if the client is not active anymore, the invocation *may* have been failed
                // already, but there is a race condition here (what-if the client disposed
                // right before the invocation was added?) and so we'd better take care of the
                // situation here
                // if the client disposes later on, we now *know* that the invocation is in
                // the list and will be taken care of
                RemoveInvocation(invocation);
                throw new TargetDisconnectedException();
            }

            if (!success)
            {
                RemoveInvocation(invocation);
                HConsole.WriteLine(this, "Failed to send a message.");
                throw new InvalidOperationException("Failed to send a message.");
            }

            HConsole.WriteLine(this, "Wait for response...");

            // and then wait for the response
            try
            {
                // we also need to monitor the composite token in case it cancels because
                // of a timeout (in which case the invocation's own token won't necessarily
                // cancel) and cancel the invocation Task accordingly, else the 'await'
                // below might never return.
                using var reg = cancellationToken.Register(invocation.TrySetCanceled);

                var response = await invocation.Task.CfAwait();
                HConsole.WriteLine(this, "Received response.");
                return response;
            }
            catch (Exception e)
            {
                HConsole.WriteLine(this, $"Failed ({e}).");
                RemoveInvocation(invocation);
                throw;
            }
        }

        /// <summary>
        /// Starts a background task attached to the connection.
        /// </summary>
        /// <param name="task">The task factory.</param>
        /// <remarks>
        /// <para>Currently, only one background task at a time is supported, and this is *not*
        /// thread-safe so it's up to the caller to handle thread-safety.</para>
        /// </remarks>
        internal void StartBackgroundTask(Func<CancellationToken, Task> task)
        {
            _bgTask = BackgroundTask.Run(async token =>
            {
                try
                {
                    await task(token).CfAwait();
                    _bgTask = null; // self-remove
                }
                catch
                {
                    _logger.LogWarning("Background task failed, terminate the connection.");
                    _bgTask = null; // self-remove *before* disposing else catch-22
                    await TerminateAsync().CfAwait(); // terminate the connection
                }
            });
        }

        /// <summary>
        /// Terminates.
        /// </summary>
        public void Terminate()
        {
            Task.Run(TerminateAsync);
        }

        /// <summary>
        /// Terminates.
        /// </summary>
        /// <returns>A task that will complete when the client connection has terminated.</returns>
        public async ValueTask TerminateAsync()
        {
            await DisposeAsync().CfAwait(); // does not throw
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            // note: DisposeAsync should not throw (CA1065)

            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
                return;

            if (ClusterId == default)
            {
                // the connection was never fully opened
                await DisposeTransientAsync().CfAwait(); // does not throw
            }
            else
            {
                // the connection was fully opened
                await DisposeOpenedAsync().CfAwait(); // does not throw
            }
        }

        // disposes a connection that was never fully opened
        private async ValueTask DisposeTransientAsync()
        {
            if (_messageConnection != null)
            {
                // also disposes the socket connection
                await _messageConnection.DisposeAsync().CfAwait(); // does not throw
            }
            else if (_socketConnection != null)
            {
                await _socketConnection.DisposeAsync().CfAwait(); // does not throw
            }
        }

        // disposes a connection that was fully opened
        private async ValueTask DisposeOpenedAsync()
        {
            // raise event
            try
            {
                await _closed(this).CfAwait(); // assume it may throw
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Caught an exception while raising Closed.");
            }

            // stop the background task
            var bgTask = _bgTask;
            if (bgTask != null)
            {
                await bgTask.CompletedOrCancelAsync(true).CfAwait(); // does not throw
            }

            // also disposes the socket connection
            await _messageConnection.DisposeAsync().CfAwait(); // does not throw

            // shutdown all pending operations
            foreach (var invocation in _invocations.Values)
                invocation.TrySetException(new TargetDisconnectedException()); // does not throw
        }
    }
}
