using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncTests1.Networking
{
    // the Client is used by the user, it handles messages
    // it manages the ClientConnection
    // it deals with correlating calls and responses
    //
    public class Client
    {
        private static readonly Log Log = new Log("CLT");

        private readonly Dictionary<int, TaskCompletionSource<Message>> _completions = new Dictionary<int, TaskCompletionSource<Message>>();
        private readonly string _hostname;
        private readonly int _port;

        private ClientSocketConnection _socketConnection;
        private MessageConnection _connection;
        private int _messageId;

        public Client(string hostname, int port)
        {
            _hostname = hostname;
            _port = port;
        }

        public async ValueTask OpenAsync()
        {
            // MessageConnection is just a wrapper around a true SocketConnection
            // the SocketConnection must be open *after* everything has been wired

            _socketConnection = new ClientSocketConnection(_hostname, _port);
            _connection = new MessageConnection(_socketConnection) { OnReceiveMessage = ReceiveMessage };
            await _socketConnection.OpenAsync();
        }

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

        public async Task<Message> SendAsync(Message message)
        {
            // assign a unique identifier to the message
            // create a corresponding completion source
            message.Id = _messageId++;
            var completion = new TaskCompletionSource<Message>();
            _completions[message.Id] = completion;

            // send the message
            Log.WriteLine($"Send \"{message}\"");
            await _connection.SendAsync(message);

            // wait for the response
            Log.WriteLine("Wait for response...");
            return await completion.Task;
        }

        public async Task CloseAsync()
        {
            Log.WriteLine("Closing");
            await _socketConnection.CloseAsync();

            // shutdown all operations
            foreach (var completion in _completions.Values)
                completion.SetException(new Exception("shutdown"));
        }
    }
}
