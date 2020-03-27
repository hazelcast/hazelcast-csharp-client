using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncTests1.Networking
{
    public class Client
    {
        private static readonly Log Log = new Log("CLT");

        private readonly string _hostname;
        private readonly int _port;
        private readonly string _eom;

        private Socket _socket;
        private TaskCompletionSource<Message> _completion;

        public Client(string hostname, int port, string eom = "/")
        {
            _hostname = hostname;
            _port = port;
            _eom = eom;
        }

        public void Open()
        {
            var host = Dns.GetHostEntry(_hostname);
            var ipAddress = host.AddressList[0];
            var endpoint = new IPEndPoint(ipAddress, _port);

            _socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Log.WriteLine("Connect to server");
            _socket.Connect(endpoint); // fixme async?

            Log.WriteLine("Listening...");
            var state = new StateObject { Socket = _socket };
            _socket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, SocketFlags.None, ReadCallback, state);
        }

        public void ReadCallback(IAsyncResult result)
        {
            Log.WriteLine("Read data");

            // retrieve the state object and the handler socket
            // from the asynchronous state object
            var state = (StateObject) result.AsyncState;
            var handler = state.Socket;

            // read data from the client socket
            int bytesRead;
            try
            {
                // may throw if the socket is not connected anymore
                bytesRead = handler.EndReceive(result);
                if (bytesRead <= 0)
                    return;
            }
            catch (Exception e)
            {
                Log.WriteLine("Abort read");
                Log.WriteLine(e);
                return;
            }

            // there might be more data, so store the data received so far
            state.Text.Append(Encoding.ASCII.GetString(state.Buffer, 0, bytesRead));

            // check for end tag, if it is not there, read more data
            var content = state.Text.ToString();
            if (content.IndexOf(_eom, StringComparison.Ordinal) > -1)
            {
                // all the data has been read from the client
                Log.WriteLine($"Read {content.Length} bytes from socket \n\tData: {content}");

                var text = content.Substring(0, content.Length - _eom.Length);
                var response = Message.Parse(text);
                HandleResponse(response);
            }
            else
            {
                // not all data received, get more
                handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, SocketFlags.None, ReadCallback, state);
            }
        }

        private void HandleResponse(Message response)
        {
            _completion.SetResult(response);
        }

        public async ValueTask<Message> SendAsync(Message message)
        {
            // note - look at how SendAsync is implemented, we may get closer to metal

            Log.WriteLine($"Send \"{message}\" ({message.ToString().Length} bytes)");
            var bytes = message.ToBytes();
            var count = await _socket.SendAsync(bytes, SocketFlags.None, CancellationToken.None);
            Log.WriteLine($"Sent {count} bytes");
            await _socket.SendAsync(Encoding.UTF8.GetBytes(_eom), SocketFlags.None, CancellationToken.None);
            Log.WriteLine("Sent EOM");

            _completion = new TaskCompletionSource<Message>();
            return await _completion.Task;
        }

        public async ValueTask CloseAsync()
        {
            Log.WriteLine("Send empty message");
            Log.WriteLine("Sent 0 bytes");
            await _socket.SendAsync(Encoding.UTF8.GetBytes(_eom), SocketFlags.None, CancellationToken.None);
            Log.WriteLine("Sent EOM");
        }
    }
}
