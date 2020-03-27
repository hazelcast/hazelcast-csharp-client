using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncTests1.Networking
{
    // same as 2 but with streams

    public class Connection3 : IConnection
    {
        // see https://devblogs.microsoft.com/dotnet/system-io-pipelines-high-performance-io-in-net/
        //  https://habr.com/en/post/466137/

        // a pipe is a "big" enough thing that should last,
        // so we should not constantly create them

        private static readonly Log Log = new Log("CON");

        private readonly string _hostname;
        private readonly int _port;
        private readonly string _eom;

        private Socket _socket;
        private Stream _stream;
        private Task _writing, _reading;

        public Connection3(string hostname, int port, string eom = "/")
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

            _stream = new NetworkStream(_socket, false);

            var pipe = new Pipe();
            _writing = WriteAsync(_stream, pipe.Writer);
            _reading = ReadAsync(pipe.Reader);
        }

        // reads from socket and writes to the pipe
        async Task WriteAsync(Stream stream, PipeWriter writer)
        {
            const int minimumBufferSize = 512;

            while (true)
            {
                // Allocate at least 512 bytes from the PipeWriter
                Memory<byte> memory = writer.GetMemory(minimumBufferSize);
                try
                {
                    int bytesRead = await stream.ReadAsync(memory, CancellationToken.None);
                    if (bytesRead == 0)
                    {
                        break;
                    }
                    // Tell the PipeWriter how much was read from the Socket
                    writer.Advance(bytesRead);
                }
                catch (Exception ex)
                {
                    Log.WriteLine("ERROR!");
                    Log.WriteLine(ex);
                    break;
                }

                // Make the data available to the PipeReader
                FlushResult result = await writer.FlushAsync();

                if (result.IsCompleted)
                {
                    break;
                }
            }

            // Tell the PipeReader that there's no more data coming
            writer.Complete();
        }

        // reads from the pipe and processes lines
        async Task ReadAsync(PipeReader reader)
        {
            while (true)
            {
                ReadResult result = await reader.ReadAsync();

                ReadOnlySequence<byte> buffer = result.Buffer;
                SequencePosition? position;

                do
                {
                    // Look for a EOM in the buffer FIXME true EOM?
                    position = buffer.PositionOf((byte)'/');

                    if (position != null)
                    {
                        // Process the line
                        ProcessMessage(buffer.Slice(0, position.Value));

                        // Skip the line + the \n character (basically position) - FIXME EOM
                        buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
                    }
                }
                while (position != null);

                // Tell the PipeReader how much of the buffer we have consumed
                reader.AdvanceTo(buffer.Start, buffer.End);

                // Stop reading if there's no more data coming
                if (result.IsCompleted)
                {
                    break;
                }
            }

            // Mark the PipeReader as complete
            reader.Complete();
        }

        private void ProcessMessage(ReadOnlySequence<byte> slice)
        {
            var text = GetAsciiString(slice);
            // and then we can process them
            OnReceivedMessage(Message.Parse(text));

            // note that the pipe itself can implement back-pressure control
        }

        public Action<Message> OnReceivedMessage { get; set; }

        string GetAsciiString(ReadOnlySequence<byte> buffer)
        {
            if (buffer.IsSingleSegment)
            {
                return Encoding.ASCII.GetString(buffer.First.Span);
            }

            return string.Create((int)buffer.Length, buffer, (span, sequence) =>
            {
                foreach (var segment in sequence)
                {
                    Encoding.ASCII.GetChars(segment.Span, span);

                    span = span.Slice(segment.Length);
                }
            });
        }

        // would it make any sense to *also* use pipelines for sending?

        // most basic way - serialize writes and return only when actually written out
        // of course we could queue but what would be the point?
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public async ValueTask SendAsync(Message message)
        {
            // note - look at how SendAsync is implemented, we may get closer to metal

            await _semaphore.WaitAsync();

            Log.WriteLine($"Send \"{message}\" ({message.ToString().Length} bytes)");
            var bytes = message.ToBytes();

            //var count = await _socket.SendAsync(bytes, SocketFlags.None, CancellationToken.None);
            await _stream.WriteAsync(bytes, 0, bytes.Length, CancellationToken.None);
            var count = bytes.Length;

            Log.WriteLine($"Sent {count} bytes");
            //await _socket.SendAsync(Encoding.UTF8.GetBytes(_eom), SocketFlags.None, CancellationToken.None);
            await _stream.WriteAsync(Encoding.UTF8.GetBytes(_eom), 0, 1, CancellationToken.None);
            Log.WriteLine("Sent EOM");

            _semaphore.Release();
        }

        public async ValueTask CloseAsync()
        {
            Log.WriteLine("Send empty message");
            Log.WriteLine("Sent 0 bytes");

            //await _socket.SendAsync(Encoding.UTF8.GetBytes(_eom), SocketFlags.None, CancellationToken.None);
            await _stream.WriteAsync(Encoding.UTF8.GetBytes(_eom), 0, 1, CancellationToken.None);

            Log.WriteLine("Sent EOM");

            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            _stream.Close();

            await _reading;
            await _writing;
        }
    }
}
