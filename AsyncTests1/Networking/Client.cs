using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AsyncTests1.Networking
{
    public class Client
    {
        private static readonly Log Log = new Log("CLT");

        private readonly string _hostname;
        private readonly int _port;
        private readonly string _eom;

        private readonly IConnection _connection;
        private int _messageId;
        private readonly Dictionary<int, TaskCompletionSource<Message>> _completions = new Dictionary<int, TaskCompletionSource<Message>>();

        public Client(string hostname, int port, string eom = "/")
        {
            _hostname = hostname;
            _port = port;
            _eom = eom;

            _connection = new Connection3(_hostname, _port, _eom);
            _connection.OnReceivedMessage = OnReceivedMessage;
            _connection.Open();
        }

        private void OnReceivedMessage(Message response)
        {
            Log.WriteLine($"Received response {response.Id}");
            if (!_completions.TryGetValue(response.Id, out var completion))
                return; // ignore?

            _completions.Remove(response.Id);
            completion.SetResult(response);
        }

        public async Task<Message> SendAsync(Message message)
        {
            message.Id = _messageId++;
            Log.WriteLine($"Send \"{message}\"");
            var completion = new TaskCompletionSource<Message>();
            _completions[message.Id] = completion;
            await _connection.SendAsync(message);
            Log.WriteLine("Wait for response...");
            return await completion.Task;
        }

        public async Task CloseAsync()
        {
            Log.WriteLine("Closing");
            await _connection.CloseAsync();
            foreach (var completion in _completions.Values)
                completion.SetException(new Exception("shutdown"));
        }
    }
}
