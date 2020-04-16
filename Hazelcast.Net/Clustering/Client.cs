using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Data;
using Hazelcast.Logging;
using Hazelcast.Messaging;
using Hazelcast.Networking;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Represents a client.
    /// </summary>
    public class Client
    {
        private readonly byte[] _clientProtocolInitBytes = { 67, 80, 50 }; //"CP2";

        private readonly Dictionary<long, TaskCompletionSource<ClientMessage>> _completions
            = new Dictionary<long, TaskCompletionSource<ClientMessage>>();

        private readonly object _isConnectedLock = new object();
        private readonly ISequence<int> _connectionIdSequence;
        private readonly ISequence<long> _correlationIdSequence;
        private readonly IPEndPoint _endpoint;

        private Action<ClientMessage> _receiveEventMessage; // fixme onReceiveEventMessage <Client, ClientMessage>?
        private Action<Client> _onShutdown;
        private ClientSocketConnection _socketConnection;
        private ClientMessageConnection _connection;
        private bool _isConnected;

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class.
        /// </summary>
        /// <param name="endpoint">The socket endpoint.</param>
        /// <param name="correlationIdSequence">A unique sequence of correlation identifiers.</param>
        public Client(IPEndPoint endpoint, ISequence<long> correlationIdSequence)
            : this(endpoint, correlationIdSequence, new Int32Sequence())
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class.
        /// </summary>
        /// <param name="endpoint">The socket endpoint.</param>
        /// <param name="correlationIdSequence">A unique sequence of correlation identifiers.</param>
        /// <param name="connectionIdSequence">A sequence of unique connection identifiers.</param>
        /// <remarks>
        /// <para>The <paramref name="connectionIdSequence"/> parameter can be used to supply a
        /// sequence of unique connection identifiers. This can be convenient for tests, where
        /// using unique identifiers across all clients can simplify debugging.</para>
        /// </remarks>
        public Client(IPEndPoint endpoint, ISequence<long> correlationIdSequence, ISequence<int> connectionIdSequence)
        {
            _endpoint = endpoint;
            _correlationIdSequence = correlationIdSequence;
            _connectionIdSequence = connectionIdSequence;
            XConsole.Setup(this, 4, "CLT");
        }

        /// <summary>
        /// Gets the unique identifier of this client.
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        // todo document
        public Guid MemberId { get; private set; }

        // todo document
        public Action<ClientMessage> ReceiveEventMessage
        {
            get => _receiveEventMessage;
            set
            {
                // FIXME tests it's ok etc
                _receiveEventMessage = value;
            }
        }

        // todo document
        public Action<Client> OnShutdown
        {
            get => _onShutdown;
            set
            {
                // FIXME test it's ok etc
                _onShutdown = value;
            }
        }

        // todo document
        public void Update(MemberInfo2 info) // rename 'complete' or somethin?
        {
            MemberId = info.MemberId;
        }

        /// <summary>
        /// Connects the client to the server.
        /// </summary>
        /// <returns>A task that will complete when the client is connected.</returns>
        public async ValueTask ConnectAsync()
        {
            // MessageConnection is just a wrapper around a true SocketConnection
            // the SocketConnection must be open *after* everything has been wired

            _socketConnection = new ClientSocketConnection(_connectionIdSequence.Next, _endpoint) { OnShutdown = SocketShutdown };
            _connection = new ClientMessageConnection(_socketConnection) { OnReceiveMessage = ReceiveMessage };
            XConsole.Setup(_connection, 12, $"CLT.MSG({_socketConnection.Id})");

            await _socketConnection.ConnectAsync();

            if (!await _socketConnection.SendAsync(_clientProtocolInitBytes))
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
            foreach (var completion in _completions.Values)
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
            XConsole.WriteLine(this, $"Received message ID:{message.CorrelationId}");

            if (message.IsEvent)
            {
                _receiveEventMessage(message);
                return new ValueTask();
            }

            if (message.IsBackupEvent)
            {
                // backup events are not supported
                throw new NotSupportedException();
            }

            // message has to be a response
            ReceiveResponseMessage(message);
            return new ValueTask();
        }

        /// <summary>
        /// Handles response messages.
        /// </summary>
        /// <param name="message">The message.</param>
        private void ReceiveResponseMessage(ClientMessage message)
        {
            // message is a response: use the correlation id to find the completion
            // source corresponding to the request, and signal this completion source by
            // setting either its result (if the response is successful) or its exception
            // (it the response is an exception).

            if (!_completions.TryGetValue(message.CorrelationId, out var completion))
            {
                // TODO log a warning
                XConsole.WriteLine(this, $"No completion for ID:{message.CorrelationId}");
                return;
            }

            // FIXME what about clearing event handlers?
            _completions.Remove(message.CorrelationId);

            // TODO consider switching tread?
            // the code here, and whatever will happen when completion.SetResult(message) runs,
            // ie. the continuations on the completion tasks (unless configure-await?) runs on
            // the same thread and blocks the networking layer

            if (message.IsException)
            {
                // TODO handle exception
                XConsole.WriteLine(this, "Message is an exception, report.");
                completion.SetException(new Exception()); // TODO try/catch this too
                return;
            }

            try
            {
                XConsole.WriteLine(this, "Message is ok, report.");
                completion.SetResult(message);
            }
            catch
            {
                // TODO log a warning
                XConsole.WriteLine(this, $"Failed to set result for ID:{message.CorrelationId}");
            }
        }

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="timeoutMilliseconds">The optional maximum number of milliseconds to get a response.</param>
        /// <returns>A task that will complete when the response has been received, and represents the response.</returns>
        // todo: maybe we don't need that one and require explicit correlation id?
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
            message.CorrelationId = correlationId;

            // send in one fragment, set flags
            message.Flags |= ClientMessageFlags.BeginFragment | ClientMessageFlags.EndFragment;

            // send the message
            XConsole.WriteLine(this, $"Send message ID:{message.CorrelationId}");
            var success = await _connection.SendAsync(message);

            // FIXME is there a race condition here?
            // we haven't registered the completion yet: what happens if a response is received?

            if (!success)
                throw new InvalidOperationException("Failed to send message.");

            // create a completion source
            var completion = new TaskCompletionSource<ClientMessage>();
            lock (_isConnectedLock)
            {
                // only return the completion task if we are still connected
                // to ensure that should we disconnect, it would be handled
                if (!_isConnected)
                    throw new InvalidOperationException("Not connected.");

                XConsole.WriteLine(this, "Wait for response...");
                _completions[message.CorrelationId] = completion;
            }

            // wait for the response
            if (timeoutMilliseconds <= 0)
                return await completion.Task;

            var timeoutTask = Task.Delay(timeoutMilliseconds);
            await Task.WhenAny(completion.Task, timeoutTask);
            if (completion.Task.IsCompletedSuccessfully()) return await completion.Task;
            throw new TimeoutException();
        }

        /// <summary>
        /// Shuts the client down.
        /// </summary>
        /// <returns>A task that will complete when the client has shut down.</returns>
        public async Task ShutdownAsync()
        {
            XConsole.WriteLine(this, "Shutdown");

            lock (_isConnectedLock)
            {
                if (!_isConnected) return;
                _isConnected = false;
            }

            // shutdown the connection
            await _socketConnection.ShutdownAsync();

            // shutdown all pending operations
            foreach (var completion in _completions.Values)
                completion.SetException(new Exception("shutdown"));
        }
    }
}
