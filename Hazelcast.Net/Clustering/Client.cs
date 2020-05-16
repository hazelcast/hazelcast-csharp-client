using System;
using System.Collections.Concurrent;
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
    public class Client
    {
        private static readonly byte[] ClientProtocolInitBytes = { 67, 80, 50 }; //"CP2";

        private readonly ConcurrentDictionary<long, Invocation> _invocations
            = new ConcurrentDictionary<long, Invocation>();

        private readonly object _isConnectedLock = new object();
        private readonly ISequence<int> _connectionIdSequence;
        private readonly ISequence<long> _correlationIdSequence;
        private readonly ILogger _logger; // FIXME: assign

        // FIXME: see defaultInvocationTimeout???

        private bool _readonlyProperties; // whether some properties (_onXxx) are readonly
        private Action<ClientMessage> _onReceiveEventMessage;
        private Action<Client> _onShutdown;

        private ClientSocketConnection _socketConnection;
        private ClientMessageConnection _messageConnection;
        private bool _isConnected;

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class.
        /// </summary>
        /// <param name="address">The network address.</param>
        /// <param name="correlationIdSequence">A unique sequence of correlation identifiers.</param>
        public Client(NetworkAddress address, ISequence<long> correlationIdSequence)
            : this(address, correlationIdSequence, new Int32Sequence())
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class.
        /// </summary>
        /// <param name="address">The network address.</param>
        /// <param name="correlationIdSequence">A unique sequence of correlation identifiers.</param>
        /// <param name="connectionIdSequence">A sequence of unique connection identifiers.</param>
        /// <remarks>
        /// <para>The <paramref name="connectionIdSequence"/> parameter can be used to supply a
        /// sequence of unique connection identifiers. This can be convenient for tests, where
        /// using unique identifiers across all clients can simplify debugging.</para>
        /// </remarks>
        public Client(NetworkAddress address, ISequence<long> correlationIdSequence, ISequence<int> connectionIdSequence)
        {
            Address = address;
            _correlationIdSequence = correlationIdSequence;
            _connectionIdSequence = connectionIdSequence;

            XConsole.Configure(this, config => config.SetIndent(4).SetPrefix("CLIENT"));
        }

        /// <summary>
        /// Gets the unique identifier of this client.
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets the unique identifier of the cluster member that this client is connected to.
        /// </summary>
        public Guid MemberId { get; private set; }

        /// <summary>
        /// Gets the network address the client is connected to.
        /// </summary>
        public NetworkAddress Address { get; }

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
        /// <returns>A task that will complete when the client is connected.</returns>
        public async ValueTask ConnectAsync()
        {
            // as soon as we even try to connect, some properties cannot change anymore
            _readonlyProperties = true;

            // MessageConnection is just a wrapper around a true SocketConnection
            // the SocketConnection must be open *after* everything has been wired

            _socketConnection = new ClientSocketConnection(_connectionIdSequence.Next, Address.IPEndPoint) { OnShutdown = SocketShutdown };
            _messageConnection = new ClientMessageConnection(_socketConnection) { OnReceiveMessage = ReceiveMessage };
            XConsole.Configure(_messageConnection, config => config.SetIndent(12).SetPrefix($"MSG.CLIENT [{_socketConnection.Id}]"));

            await _socketConnection.ConnectAsync();

            if (!await _socketConnection.SendAsync(ClientProtocolInitBytes))
                throw new InvalidOperationException("Failed to send protocol bytes.");

            lock (_isConnectedLock) _isConnected = true;
        }

        /// <summary>
        /// Handles connection shutdown.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns>A task that will complete when the shutdown has been handled.</returns>
        private ValueTask SocketShutdown(SocketConnectionBase connection)
        {
            foreach (var completion in _invocations.Values)
                completion.SetException(new Exception("shutdown"));

            _onShutdown?.Invoke(this);

            return new ValueTask();
        }

        /// <summary>
        /// Handles messages.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="message">The message.</param>
        /// <returns>A task that will complete when the message has been handled.</returns>
        private ValueTask ReceiveMessage(ClientMessageConnection connection, ClientMessage message)
        {
            // FIXME: async oops!
            // could be remain async all the way?

            if (message.IsEvent)
            {
                XConsole.WriteLine(this, $"Receive event [{message.CorrelationId}]" +
                                         XConsole.Lines(this, 1, message.Dump()));
                ReceiveEvent(message);
                return new ValueTask();
            }

            if (message.IsBackupEvent)
            {
                XConsole.WriteLine(this, $"Receive backup event [{message.CorrelationId}]" +
                                         XConsole.Lines(this, 1, message.Dump()));

                // backup events are not supported
                //throw new NotSupportedException("Backup events are not supported here.");
                _logger.LogWarning($"Ignoring unsupported backup event.");
                return new ValueTask();
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
                return new ValueTask();
            }

            // TODO: threading and scheduling?
            // the code here, and whatever will happen when completion.SetResult(message) runs,
            // ie. the continuations on the completion tasks (unless configure-await?) runs on
            // the same thread and blocks the networking layer

            // receive exception or message - try our best to not throw here
            if (message.IsException)
                ReceiveException(invocation, message);
            else
                ReceiveResponse(invocation, message);

            return new ValueTask();
        }

        private void ReceiveEvent(ClientMessage message)
        {
            try
            {
                XConsole.WriteLine(this, $"Raise event [{message.CorrelationId}].");
                _onReceiveEventMessage(message);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"Failed to raise event [{message.CorrelationId}].");
                XConsole.WriteLine(this, $"Failed to raise event [{message.CorrelationId}].");
                /* nothing much we can do */
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
        /// <param name="timeoutMilliseconds">The optional maximum number of milliseconds to get a response.</param>
        /// <returns>A task that will complete when the response has been received, and represents the response.</returns>
        public async Task<ClientMessage> SendAsync(ClientMessage message, int timeoutMilliseconds = 0)
            => await SendAsync(message, _correlationIdSequence.Next, timeoutMilliseconds);

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="correlationId">The correlation identifier.</param>
        /// <param name="timeoutMilliseconds">The optional maximum number of milliseconds to get a response.</param>
        /// <returns>A task that will complete when the response has been received, and represents the response.</returns>
        public async Task<ClientMessage> SendAsync(ClientMessage message, long correlationId, int timeoutMilliseconds = 0)
        {
            lock (_isConnectedLock)
            {
                if (!_isConnected)
                    throw new InvalidOperationException("Not connected.");
            }

            // assign a unique identifier to the message
            // and send in one fragment, with proper flags
            message.CorrelationId = correlationId;
            message.Flags |= ClientMessageFlags.BeginFragment | ClientMessageFlags.EndFragment;

            // create the invocation
            var invocation = new Invocation(message, timeoutMilliseconds);

            while (true)
            {
                try
                {
                    return await SendAsync(invocation);
                }
                catch (Exception exception)
                {
                    if (exception is ClientProtocolException protocolException)
                    {
                        // FIXME more things are retryable
                        // maybe we can retry
                        if (protocolException.Retryable && 
                            invocation.TryRetry(() => _correlationIdSequence.Next, out var delay))
                        {
                            if (delay > 0) await Task.Delay(delay);
                            continue;
                        }
                    }

                    // else... it's bad enough
                    throw;
                }
            }
        }

        /// <summary>
        /// Sends an invocation message.
        /// </summary>
        /// <param name="invocation">The invocation.</param>
        /// <returns>A task that will complete when the response has been received, and represents the response.</returns>
        private async Task<ClientMessage> SendAsync(Invocation invocation)
        {
            AddInvocation(invocation);

            XConsole.WriteLine(this, $"Send message [{invocation.CorrelationId}]" +
                                     XConsole.Lines(this, 1, invocation.RequestMessage.Dump()));

            // actually send the message
            var timeout = invocation.RemainingMilliseconds;
            var success = await _messageConnection.SendAsync(invocation.RequestMessage, timeout);

            lock (_isConnectedLock)
            {
                // if the message could not be sent, or if the client is not connected
                // anymore, the invocation will never complete and must be removed
                if (!success || !_isConnected)
                {
                    var exceptionMessage = !_isConnected
                        ? "Client is not connected."
                        : "Failed to send a message.";

                    RemoveInvocation(invocation);

                    XConsole.WriteLine(this, "Failed: " + exceptionMessage);
                    throw new InvalidOperationException(exceptionMessage);
                }

                // otherwise it's ok to wait for a response
                XConsole.WriteLine(this, "Wait for response...");
            }

            // no timeout = just return the invocation completion task
            if (!invocation.HasTimeout)
                return await invocation.Task;

            // timeout = wait until either the invocation completes, or the timeout is reached
            var timeoutTask = Task.Delay(invocation.RemainingMilliseconds);
            await Task.WhenAny(invocation.Task, timeoutTask);

            // success = return result, no need to remove the completion, it's handled elsewhere
            if (invocation.Task.IsCompletedSuccessfully())
                return await invocation.Task;

            // timeout: remove the invocation and throw
            RemoveInvocation(invocation);
            throw new TimeoutException();
        }

        /// <summary>
        /// Shuts the client down.
        /// </summary>
        /// <returns>A task that will complete when the client has shut down.</returns>
        public async Task ShutdownAsync()
        {
            // TODO: consider making Client IDisposable

            XConsole.WriteLine(this, "Shutdown");

            lock (_isConnectedLock)
            {
                if (!_isConnected) return;
                _isConnected = false;
            }

            // shutdown the connection
            await _socketConnection.ShutdownAsync();

            // shutdown all pending operations
            foreach (var completion in _invocations.Values)
                completion.SetException(new Exception("shutdown"));
        }
    }
}
