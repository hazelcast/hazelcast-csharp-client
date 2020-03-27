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

        private ClientConnection _connection;
        private int _messageId;

        public Client(string hostname, int port)
        {
            _hostname = hostname;
            _port = port;
        }

        public void Open()
        {
            _connection = new ClientConnection(_hostname, _port) { OnReceivedMessage = OnReceivedMessage };
            _connection.Open();
        }

        private void OnReceivedMessage(Message response)
        {
            Log.WriteLine($"Received response {response.Id}");
            if (!_completions.TryGetValue(response.Id, out var completion))
                return; // ignore?

            // signal the completion source
            _completions.Remove(response.Id);
            completion.SetResult(response);
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
            await _connection.CloseAsync();

            // shutdown all operations
            foreach (var completion in _completions.Values)
                completion.SetException(new Exception("shutdown"));
        }
    }
}
