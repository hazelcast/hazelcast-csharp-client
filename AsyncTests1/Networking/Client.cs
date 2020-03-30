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
        public readonly Log Log = new Log { Prefix = "    CLT" };

        private readonly byte[] _clientProtocolInitBytes = { 67, 80, 50 }; //"CP2";

        private readonly Dictionary<int, TaskCompletionSource<Message>> _completions = new Dictionary<int, TaskCompletionSource<Message>>();
        private readonly object _isConnectedLock = new object();
        private readonly ISequence<int> _connectionIdSequence;
        private readonly string _hostname;
        private readonly int _port;

        private ClientSocketConnection _socketConnection;
        private MessageConnection _connection;
        private int _messageId;
        private bool _isConnected;

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class.
        /// </summary>
        /// <param name="hostname">The server hostname.</param>
        /// <param name="port">The server port.</param>
        public Client(string hostname, int port)
            : this(hostname, port, new Int32Sequence())
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class.
        /// </summary>
        /// <param name="hostname">The server hostname.</param>
        /// <param name="port">The server port.</param>
        /// <param name="connectionIdSequence">A sequence of unique connection identifiers.</param>
        /// <remarks>
        /// <para>The <paramref name="connectionIdSequence"/> parameter can be used to supply a
        /// sequence of unique connection identifiers. This can be convenient for tests, where
        /// using unique identifiers across all clients can simplify debugging.</para>
        /// </remarks>
        public Client(string hostname, int port, ISequence<int> connectionIdSequence) // FIXME should accept an endpoint
        {
            _hostname = hostname;
            _port = port;
            _connectionIdSequence = connectionIdSequence;
        }

        /// <summary>
        /// Connects the client to the server.
        /// </summary>
        /// <returns>A task that will complete when the client is connected.</returns>
        public async ValueTask ConnectAsync()
        {
            // MessageConnection is just a wrapper around a true SocketConnection
            // the SocketConnection must be open *after* everything has been wired

            var host = Dns.GetHostEntry(_hostname);
            var ipAddress = host.AddressList[0];
            var endpoint = new IPEndPoint(ipAddress, _port);

            _socketConnection = new ClientSocketConnection(_connectionIdSequence.Next, endpoint) { OnShutdown = SocketShutdown };
            _connection = new MessageConnection(_socketConnection) { OnReceiveMessage = ReceiveMessage };
            _connection.Log.Prefix = "            CLT.MSG" + _socketConnection.Id;

            await _socketConnection.ConnectAsync();

            // FIXME implement protocol bytes
            //if (!await _socketConnection.SendRawAsync(_clientProtocolInitBytes))
            //    throw new InvalidOperationException("Failed to open.");

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
        private ValueTask ReceiveMessage(MessageConnection connection, Message response)
        {
            Log.WriteLine($"Received response {response.Id}");

            if (_completions.TryGetValue(response.Id, out var completion))
            {
                // signal the completion source
                _completions.Remove(response.Id);
                completion.SetResult(response);
            }

            return new ValueTask();
        }

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="timeoutMilliseconds">The maximum number of milliseconds to get a response.</param>
        /// <returns>A task that will complete when the response has been received, and represents the response.</returns>
        public async Task<Message> SendAsync(Message message, int timeoutMilliseconds = 0)
        {
            lock (_isConnectedLock)
            {
                if (!_isConnected)
                    throw new InvalidOperationException("Not connected.");
            }

            // assign a unique identifier to the message
            // create a corresponding completion source
            message.Id = _messageId++;

            // send the message
            Log.WriteLine($"Send \"{message}\"");
            var success = await _connection.SendAsync(message);

            if (!success)
                throw new InvalidOperationException("Failed to send message.");

            // wait for the response
            var completion = new TaskCompletionSource<Message>();
            lock (_isConnectedLock)
            {
                // only return the completion task if we are still connected
                // to ensure that should we disconnect, it would be handled
                if (!_isConnected)
                    throw new InvalidOperationException("Not connected.");

                Log.WriteLine("Wait for response...");
                _completions[message.Id] = completion;
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
            Log.WriteLine("Shutdown");

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
