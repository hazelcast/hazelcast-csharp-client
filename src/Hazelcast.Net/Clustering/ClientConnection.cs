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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Data;
using Hazelcast.Exceptions;
using Hazelcast.Messaging;
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
    /// Represents a client connection to a cluster member.
    /// </summary>
    internal class ClientConnection : IAsyncDisposable
    {
        private static readonly byte[] ClientProtocolInitBytes = { 67, 80, 50 }; //"CP2";

        private readonly ConcurrentDictionary<long, Invocation> _invocations
            = new ConcurrentDictionary<long, Invocation>();

        private readonly MessagingOptions _messagingOptions;
        private readonly SocketOptions _socketOptions;
        private readonly ISequence<int> _connectionIdSequence;
        private readonly ISequence<long> _correlationIdSequence;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        private bool _readonlyProperties; // whether some properties (_onXxx) are readonly
        private Func<ClientMessage, CancellationToken, ValueTask> _onReceiveEventMessage;
        private Func<ClientConnection, ValueTask> _onShutdown;

#pragma warning disable CA2213 // Disposable fields should be disposed - is owned by _messageConnection, which is disposed
        private ClientSocketConnection _socketConnection;
#pragma warning restore CA2213 // Disposable fields should be disposed
        private ClientMessageConnection _messageConnection;
        private volatile int _disposed;

        private CancellationTokenSource _bgCancellation;
        private CancellationTokenSource _bgTaskCancellation;
        private Task _bgTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientConnection"/> class.
        /// </summary>
        /// <param name="address">The network address.</param>
        /// <param name="messagingOptions">Messaging options.</param>
        /// <param name="socketOptions">Socket options.</param>
        /// <param name="correlationIdSequence">A sequence of unique correlation identifiers.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public ClientConnection(NetworkAddress address, MessagingOptions messagingOptions, SocketOptions socketOptions, ISequence<long> correlationIdSequence, ILoggerFactory loggerFactory)
            : this(address, messagingOptions, socketOptions, new Int32Sequence(), correlationIdSequence, loggerFactory)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientConnection"/> class.
        /// </summary>
        /// <param name="address">The network address.</param>
        /// <param name="messagingOptions">Messaging options.</param>
        /// <param name="socketOptions">Socket options.</param>
        /// <param name="connectionIdSequence">A sequence of unique connection identifiers.</param>
        /// <param name="correlationIdSequence">A sequence of unique correlation identifiers.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        /// <remarks>
        /// <para>The <paramref name="connectionIdSequence"/> parameter can be used to supply a
        /// sequence of unique connection identifiers. This can be convenient for tests, where
        /// using unique identifiers across all clients can simplify debugging.</para>
        /// </remarks>
        public ClientConnection(NetworkAddress address, MessagingOptions messagingOptions, SocketOptions socketOptions, ISequence<int> connectionIdSequence, ISequence<long> correlationIdSequence, ILoggerFactory loggerFactory)
        {
            Address = address ?? throw new ArgumentNullException(nameof(address));
            _messagingOptions = messagingOptions ?? throw new ArgumentNullException(nameof(messagingOptions));
            _socketOptions = socketOptions ?? throw new ArgumentNullException(nameof(socketOptions));
            _connectionIdSequence = connectionIdSequence ?? throw new ArgumentNullException(nameof(connectionIdSequence));
            _correlationIdSequence = correlationIdSequence ?? throw new ArgumentNullException(nameof(correlationIdSequence));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<ClientConnection>();

            HConsole.Configure(this, config => config.SetIndent(4).SetPrefix("CLIENT"));
        }

        /// <summary>
        /// Gets the unique identifier of this client.
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Whether the client is active.
        /// </summary>
        public bool Active => _disposed == 0;

        /// <summary>
        /// Gets the unique identifier of the cluster member that this client is connected to.
        /// </summary>
        public Guid MemberId { get; private set; }

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
        /// Gets or sets an action that will be executed when the client receives a message.
        /// </summary>
        public Func<ClientMessage, CancellationToken, ValueTask> OnReceiveEventMessage
        {
            get => _onReceiveEventMessage;
            set
            {
                if (_readonlyProperties)
                    throw new InvalidOperationException(ExceptionMessages.PropertyIsNowReadOnly);
                _onReceiveEventMessage = value;
            }
        }

        /// <summary>
        /// Gets or sets an action that will be executed when the client shuts down.
        /// </summary>
        public Func<ClientConnection, ValueTask> OnShutdown
        {
            get => _onShutdown;
            set
            {
                if (_readonlyProperties)
                    throw new InvalidOperationException(ExceptionMessages.PropertyIsNowReadOnly);
                _onShutdown = value;
            }
        }

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
        /// Notifies the client after authentication has been performed.
        /// </summary>
        /// <param name="result">The result of the authentication.</param>
        public void NotifyAuthenticated(AuthenticationResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            MemberId = result.MemberId;
        }

        /// <summary>
        /// Connects the client to the server.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the client is connected.</returns>
        public async ValueTask ConnectAsync(CancellationToken cancellationToken)
        {
            // as soon as we even try to connect, some properties cannot change anymore
            _readonlyProperties = true;

            // MessageConnection is just a wrapper around a true SocketConnection
            // the SocketConnection must be open *after* everything has been wired

            _socketConnection = new ClientSocketConnection(_connectionIdSequence.GetNext(), Address.IPEndPoint, _socketOptions) { OnShutdown = SocketShutdown };
            _messageConnection = new ClientMessageConnection(_socketConnection, _loggerFactory) { OnReceiveMessage = ReceiveMessage };
            HConsole.Configure(_messageConnection, config => config.SetIndent(12).SetPrefix($"MSG.CLIENT [{_socketConnection.Id}]"));

            try
            {
                await _socketConnection.ConnectAsync(cancellationToken).CAF();
            }
            catch (Exception e)
            {
                HConsole.WriteLine(this, "Failed to connect. " + e);
                throw;
            }

            if (!await _socketConnection.SendAsync(ClientProtocolInitBytes, ClientProtocolInitBytes.Length, cancellationToken).CAF())
                throw new InvalidOperationException("Failed to send protocol bytes.");
        }

        /// <summary>
        /// Handles connection shutdown.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns>A task that will complete when the shutdown has been handled.</returns>
        private void SocketShutdown(SocketConnectionBase connection)
        {
            Terminate();
        }

        /// <summary>
        /// Handles messages.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="message">The message.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the message has been handled.</returns>
        private async ValueTask ReceiveMessage(ClientMessageConnection connection, ClientMessage message, CancellationToken cancellationToken)
        {
            if (message.IsEvent)
            {
                HConsole.WriteLine(this, $"Receive event [{message.CorrelationId}]" +
                                         HConsole.Lines(this, 1, message.Dump()));
                await ReceiveEvent(message, cancellationToken).CAF(); // should not throw
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
            if (!TryRemoveInvocation(message.CorrelationId, out var invocation))
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

        private async ValueTask ReceiveEvent(ClientMessage message, CancellationToken cancellationToken)
        {
            try
            {
                HConsole.WriteLine(this, $"Raise event [{message.CorrelationId}].");
                await _onReceiveEventMessage(message, cancellationToken).CAF();
            }
            catch (Exception e)
            {
                // this is a bad enough situation: the event handler failed, and there is
                // no way we can "bubble" the exception up to user's code since it all
                // happen in the background, so we have to swallow the exception

                // at least, make some noise
                // TODO: instrumentation
                _logger.LogWarning(e, $"Failed to raise event [{message.CorrelationId}].");
                HConsole.WriteLine(this, $"Failed to raise event [{message.CorrelationId}]." + e);
            }
        }

        private void ReceiveException(Invocation invocation, ClientMessage message)
        {
            // try to be as safe as possible here

            Exception exception;
            try
            {
                exception = ClientProtocolExceptions.CreateException(ErrorsCodec.Decode(message));
            }
            catch (Exception e)
            {
                exception = e;
            }

            try
            {
                HConsole.WriteLine(this, $"Fail invocation [{message.CorrelationId}].");
                invocation.SetException(exception);
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
                invocation.SetResult(message);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"Failed to complete invocation [{message.CorrelationId}].");
                HConsole.WriteLine(this, $"Failed to complete invocation [{message.CorrelationId}].");

                try
                {
                    invocation.SetException(e);
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
        public async Task<ClientMessage> SendAsync(ClientMessage message, CancellationToken cancellationToken)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            // assign a unique identifier to the message
            // and send in one fragment, with proper flags
            message.CorrelationId = _correlationIdSequence.GetNext();
            message.Flags |= ClientMessageFlags.BeginFragment | ClientMessageFlags.EndFragment;

            // create the invocation
            var invocation = new Invocation(message, _messagingOptions, this, cancellationToken);

            return await SendAsync(invocation, cancellationToken).CAF();
        }

        /// <summary>
        /// Sends an invocation message.
        /// </summary>
        /// <param name="invocation">The invocation.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the response has been received, and represents the response.</returns>
        public async Task<ClientMessage> SendAsync(Invocation invocation, CancellationToken cancellationToken)
        {
            if (invocation == null) throw new ArgumentNullException(nameof(invocation));

            // adds the invocation, it will be removed when receiving the response (or timeout or...)
            AddInvocation(invocation);

            HConsole.WriteLine(this, $"Send message [{invocation.CorrelationId}]" +
                                     HConsole.Lines(this, 1, invocation.RequestMessage.Dump()));

            // actually send the message
            var success = await _messageConnection.SendAsync(invocation.RequestMessage, cancellationToken).CAF();

            if (!Active)
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
                // in case it times out, there's not point cancelling invocation.Task as
                // it is not a real task but just a task continuation source's task
                return await invocation.Task.CAF();
            }
            catch
            {
                RemoveInvocation(invocation);
                throw;
            }
        }

        /// <summary>
        /// Starts a background task attached to the client.
        /// </summary>
        /// <param name="task">The task factory.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <remarks>
        /// <para>There can be only one background task running at a time. This is used to
        /// install subscriptions on a new client. This method is *not* thread safe and
        /// expects the caller to handler thread-safety.</para>
        /// </remarks>
        internal void StartBackgroundTask(Func<CancellationToken, Task> task, CancellationToken cancellationToken)
        {
            _bgCancellation ??= new CancellationTokenSource();
            _bgTaskCancellation = CancellationTokenSource.CreateLinkedTokenSource(_bgCancellation.Token, cancellationToken);
            _bgTask = task(_bgTaskCancellation.Token).ContinueWith(x =>
            {
                if (x.IsFaulted)
                {
                    _logger.LogWarning("Background task failed, die.");
                    Terminate();
                }
                _bgCancellation.Dispose();
                _bgCancellation = null;
                _bgTaskCancellation.Dispose();
                _bgTaskCancellation = null;
                return x;
            }, TaskScheduler.Current);
        }

        /// <summary>
        /// Terminates.
        /// </summary>
        public void Terminate()
        {
            Task.Run(async () => await TerminateAsync().CAF());
        }

        /// <summary>
        /// Terminates.
        /// </summary>
        /// <returns>A task that will complete when the client connection has terminated.</returns>
        public async ValueTask TerminateAsync()
        {
            try
            {
                await DisposeAsync().CAF();
            }
            catch (Exception e)
            {
                // that's all we can do really
                _logger.LogWarning(e, "Caught an exception while terminating.");
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
                return;

            if (_messageConnection == null)
                return;

            var bgTask = _bgTask;
            if (bgTask != null)
            {
                _bgCancellation?.Cancel();
                try
                {
                    await bgTask.CAF();
                }
                catch { /* ignore - no value */ }
            }

            _bgCancellation?.Dispose();
            _bgTaskCancellation?.Dispose();

            try
            {
                // message connection disposes the socket connection
                await _messageConnection.DisposeAsync().CAF();
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"Caught an exception while disposing {_messageConnection.GetType()}.");
            }

            // shutdown all pending operations
            // dealing with race conditions in SendAsync
            foreach (var invocation in _invocations.Values)
                invocation.TrySetException(new TargetDisconnectedException());

            if (_onShutdown == null)
                return;

            try
            {
                await _onShutdown(this).CAF();
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Caught an exception while running onShutdown.");
            }
        }
    }
}
