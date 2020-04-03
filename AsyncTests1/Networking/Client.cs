using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace AsyncTests1.Networking
{
    /// <summary>
    /// Represents a client.
    /// </summary>
    public class Client
    {
        private readonly byte[] _clientProtocolInitBytes = { 67, 80, 50 }; //"CP2";

        private readonly Dictionary<int, TaskCompletionSource<Message2>> _completions = new Dictionary<int, TaskCompletionSource<Message2>>();
        private readonly object _isConnectedLock = new object();
        private readonly ISequence<int> _connectionIdSequence;
        private readonly IPEndPoint _endpoint;

        private ClientSocketConnection _socketConnection;
        private MessageConnection _connection;
        private int _messageId;
        private bool _isConnected;

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class.
        /// </summary>
        /// <param name="endpoint">The socket endpoint.</param>
        public Client(IPEndPoint endpoint)
            : this(endpoint, new Int32Sequence())
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class.
        /// </summary>
        /// <param name="endpoint">The socket endpoint.</param>
        /// <param name="connectionIdSequence">A sequence of unique connection identifiers.</param>
        /// <remarks>
        /// <para>The <paramref name="connectionIdSequence"/> parameter can be used to supply a
        /// sequence of unique connection identifiers. This can be convenient for tests, where
        /// using unique identifiers across all clients can simplify debugging.</para>
        /// </remarks>
        public Client(IPEndPoint endpoint, ISequence<int> connectionIdSequence)
        {
            _endpoint = endpoint;
            _connectionIdSequence = connectionIdSequence;
            XConsole.Setup(this, 4, "CLT");
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
            _connection = new MessageConnection(_socketConnection) { OnReceiveMessage = ReceiveMessage };
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
        private ValueTask SocketShutdown(SocketConnection connection)
        {
            foreach (var completion in _completions.Values)
                completion.SetException(new Exception("shutdown"));

            return new ValueTask();
        }

        /// <summary>
        /// Handles response messages..
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="response">The response message.</param>
        /// <returns>A task that will complete when the response message has been handled.</returns>
        private ValueTask ReceiveMessage(MessageConnection connection, Message2 response)
        {
            XConsole.WriteLine(this, $"Received response ID:{response.CorrelationId}");

            if (_completions.TryGetValue((int) response.CorrelationId, out var completion)) // fixme id size
            {
                // signal the completion source
                _completions.Remove((int) response.CorrelationId); // FIXME id size
                completion.SetResult(response);
            }
            else
            {
                XConsole.WriteLine(this, $"No completion for ID:{response.CorrelationId}");
            }

            return new ValueTask();
        }

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="timeoutMilliseconds">The maximum number of milliseconds to get a response.</param>
        /// <returns>A task that will complete when the response has been received, and represents the response.</returns>
        public async Task<Message2> SendAsync(Message2 message, int timeoutMilliseconds = 0)
        {
            lock (_isConnectedLock)
            {
                if (!_isConnected)
                    throw new InvalidOperationException("Not connected.");
            }

            // assign a unique identifier to the message
            // create a corresponding completion source
            message.CorrelationId = _messageId++;

            // send the message
            XConsole.WriteLine(this, $"Send message ID:{message.CorrelationId}");
            var success = await _connection.SendAsync(message);

            if (!success)
                throw new InvalidOperationException("Failed to send message.");

            // wait for the response
            var completion = new TaskCompletionSource<Message2>();
            lock (_isConnectedLock)
            {
                // only return the completion task if we are still connected
                // to ensure that should we disconnect, it would be handled
                if (!_isConnected)
                    throw new InvalidOperationException("Not connected.");

                XConsole.WriteLine(this, "Wait for response...");
                _completions[(int) message.CorrelationId] = completion; // FIXME id size
            }

            if (timeoutMilliseconds <= 0)
                return await completion.Task;

            var timeoutTask = Task.Delay(timeoutMilliseconds);
            await Task.WhenAny(completion.Task, timeoutTask);
            if (completion.Task.IsCompletedSuccessfully) return await completion.Task;
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
