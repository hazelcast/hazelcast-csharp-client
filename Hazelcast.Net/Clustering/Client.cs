﻿using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Data;
using Hazelcast.Exceptions;
using Hazelcast.Logging;
using Hazelcast.Messaging;
using Hazelcast.Networking;
using Hazelcast.Protocol;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Represents a client.
    /// </summary>
    public class Client : IAsyncDisposable
    {
        private static readonly byte[] ClientProtocolInitBytes = { 67, 80, 50 }; //"CP2";

        private readonly ConcurrentDictionary<long, Invocation> _invocations
            = new ConcurrentDictionary<long, Invocation>();

        private readonly object _activeLock = new object();
        private readonly ISequence<int> _connectionIdSequence;
        private readonly ISequence<long> _correlationIdSequence;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        private bool _readonlyProperties; // whether some properties (_onXxx) are readonly
        private Action<ClientMessage> _onReceiveEventMessage;
        private Action<Client> _onShutdown;

        private ClientSocketConnection _socketConnection;
        private ClientMessageConnection _messageConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class.
        /// </summary>
        /// <param name="address">The network address.</param>
        /// <param name="correlationIdSequence">A sequence of unique correlation identifiers.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public Client(NetworkAddress address, ISequence<long> correlationIdSequence, ILoggerFactory loggerFactory)
            : this(address, new Int32Sequence(), correlationIdSequence, loggerFactory)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class.
        /// </summary>
        /// <param name="address">The network address.</param>
        /// <param name="connectionIdSequence">A sequence of unique connection identifiers.</param>
        /// <param name="correlationIdSequence">A sequence of unique correlation identifiers.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        /// <remarks>
        /// <para>The <paramref name="connectionIdSequence"/> parameter can be used to supply a
        /// sequence of unique connection identifiers. This can be convenient for tests, where
        /// using unique identifiers across all clients can simplify debugging.</para>
        /// </remarks>
        public Client(NetworkAddress address, ISequence<int> connectionIdSequence, ISequence<long> correlationIdSequence, ILoggerFactory loggerFactory)
        {
            Address = address ?? throw new ArgumentNullException(nameof(address));
            _connectionIdSequence = connectionIdSequence ?? throw new ArgumentNullException(nameof(connectionIdSequence));
            _correlationIdSequence = correlationIdSequence ?? throw new ArgumentNullException(nameof(correlationIdSequence));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<Client>();

            XConsole.Configure(this, config => config.SetIndent(4).SetPrefix("CLIENT"));
        }

        /// <summary>
        /// Gets the unique identifier of this client.
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Whether the client is active.
        /// </summary>
        public bool Active { get; private set; }

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
        /// Gets or sets an action that will be executed when the client receives a message.
        /// </summary>
        public Action<ClientMessage> OnReceiveEventMessage
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
        public Action<Client> OnShutdown
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

            _socketConnection = new ClientSocketConnection(_connectionIdSequence.Next, Address.IPEndPoint) { OnShutdown = SocketShutdown };
            _messageConnection = new ClientMessageConnection(_socketConnection, _loggerFactory) { OnReceiveMessage = ReceiveMessage };
            XConsole.Configure(_messageConnection, config => config.SetIndent(12).SetPrefix($"MSG.CLIENT [{_socketConnection.Id}]"));

            try
            {
                await _socketConnection.ConnectAsync(cancellationToken).CAF();
            }
            catch (Exception e)
            {
                XConsole.WriteLine(this, "Failed to connect. " + e);
                throw;
            }

            if (!await _socketConnection.SendAsync(ClientProtocolInitBytes, ClientProtocolInitBytes.Length, cancellationToken))
                throw new InvalidOperationException("Failed to send protocol bytes.");

            lock (_activeLock) Active = true;
        }

        /// <summary>
        /// Handles connection shutdown.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns>A task that will complete when the shutdown has been handled.</returns>
        private void SocketShutdown(SocketConnectionBase connection)
        {
            lock (_activeLock)
            {
                if (!Active) return;
                Active = false;
            }

            CompleteShutdown();
        }

        /// <summary>
        /// Handles messages.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="message">The message.</param>
        /// <returns>A task that will complete when the message has been handled.</returns>
        private async ValueTask ReceiveMessage(ClientMessageConnection connection, ClientMessage message)
        {
            if (message.IsEvent)
            {
                XConsole.WriteLine(this, $"Receive event [{message.CorrelationId}]" +
                                         XConsole.Lines(this, 1, message.Dump()));
                await ReceiveEvent(message); // should not throw
                return;
            }

            if (message.IsBackupEvent)
            {
                XConsole.WriteLine(this, $"Receive backup event [{message.CorrelationId}]" +
                                         XConsole.Lines(this, 1, message.Dump()));

                // backup events are not supported
                //throw new NotSupportedException("Backup events are not supported here.");
                _logger.LogWarning("Ignoring unsupported backup event.");
                return;
            }

            // message has to be a response
            XConsole.WriteLine(this, $"Receive response [{message.CorrelationId}]" +
                                     XConsole.Lines(this, 1, message.Dump()));

            // find the corresponding invocation
            // and remove invocation
            if (!TryRemoveInvocation(message.CorrelationId, out var invocation))
            {
                // orphan messages are ignored (but logged)
                _logger.LogWarning($"Received message for unknown invocation [{message.CorrelationId}].");
                XConsole.WriteLine(this, $"Unknown invocation [{message.CorrelationId}]");
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

        private ValueTask ReceiveEvent(ClientMessage message)
        {
            // FIXME: async oops, could events be async too?

            try
            {
                XConsole.WriteLine(this, $"Raise event [{message.CorrelationId}].");
                _onReceiveEventMessage(message);
            }
            catch (Exception e)
            {
                // this is a bad enough situation: the event handler failed, and there is
                // no way we can "bubble" the exception up to user's code since it all
                // happen in the background, so we have to swallow the exception

                // at least, make some noise
                // TODO: instrumentation
                _logger.LogWarning(e, $"Failed to raise event [{message.CorrelationId}].");
                XConsole.WriteLine(this, $"Failed to raise event [{message.CorrelationId}]." + e);
            }

            return new ValueTask();
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
                XConsole.WriteLine(this, $"Fail invocation [{message.CorrelationId}].");
                invocation.SetException(exception);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"Failed to fail invocation [{message.CorrelationId}].");
                XConsole.WriteLine(this, $"Failed to fail invocation [{message.CorrelationId}].");
            }

        }

        private void ReceiveResponse(Invocation invocation, ClientMessage message)
        {
            try
            {
                XConsole.WriteLine(this, $"Complete invocation [{message.CorrelationId}].");
                invocation.SetResult(message);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"Failed to complete invocation [{message.CorrelationId}].");
                XConsole.WriteLine(this, $"Failed to complete invocation [{message.CorrelationId}].");

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
            message.CorrelationId = _correlationIdSequence.Next;
            message.Flags |= ClientMessageFlags.BeginFragment | ClientMessageFlags.EndFragment;

            // create the invocation
            var invocation = new Invocation(message, this, cancellationToken);

            return await SendAsync(invocation, cancellationToken);
        }

        /// <summary>
        /// Sends an invocation message.
        /// </summary>
        /// <param name="invocation">The invocation.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the response has been received, and represents the response.</returns>
        public async Task<ClientMessage> SendAsync(Invocation invocation, CancellationToken cancellationToken)
        {
            // adds the invocation, it will be removed when receiving the response (or timeout or...)
            AddInvocation(invocation);

            XConsole.WriteLine(this, $"Send message [{invocation.CorrelationId}]" +
                                     XConsole.Lines(this, 1, invocation.RequestMessage.Dump()));

            // actually send the message
            var success = await _messageConnection.SendAsync(invocation.RequestMessage, cancellationToken).CAF();

            lock (_activeLock)
            {
                // if the message could not be sent, or if the client is not connected
                // anymore, the invocation will never complete and must be removed
                if (!success || !Active)
                {
                    var exceptionMessage = !Active
                        ? "Client is not active."
                        : "Failed to send a message.";

                    RemoveInvocation(invocation);

                    XConsole.WriteLine(this, "Failed: " + exceptionMessage);
                    throw new InvalidOperationException(exceptionMessage);
                }

                // otherwise it's ok to wait for a response
                XConsole.WriteLine(this, "Wait for response...");
            }

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
        /// Shuts the client down in the background.
        /// </summary>
        public void Shutdown()
        {
            Task.Run(ShutdownAsync);
        }

        /// <summary>
        /// Shuts the client down.
        /// </summary>
        /// <returns>A task that will complete when the client has shut down.</returns>
        public async Task ShutdownAsync()
        {
            XConsole.WriteLine(this, "Shutdown");

            lock (_activeLock)
            {
                if (!Active) return;
                Active = false;
            }

            // shutdown the connection
            await _socketConnection.ShutdownAsync();

            CompleteShutdown();
        }

        private void CompleteShutdown()
        {
            // shutdown all pending operations
            foreach (var completion in _invocations.Values)
                completion.SetException(new Exception("shutdown"));

            // invoke
            _onShutdown?.Invoke(this);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            // FIXME: implement + understand link to shudown?
            await ShutdownAsync();
        }
    }
}
