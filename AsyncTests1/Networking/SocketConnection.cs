using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncTests1.Networking
{
    // the SocketConnection is used by the ClientConnection, it handles bytes
    // it manages the network socket
    //
    public class SocketConnection
    {
        // see https://devblogs.microsoft.com/dotnet/system-io-pipelines-high-performance-io-in-net/
        //  https://habr.com/en/post/466137/

        // a pipe is a "big" enough thing that should last,
        // so we should not constantly create them

        public const string LogName = "SCN";
        private static readonly Log Log = new Log(LogName);

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private readonly string _hostname;
        private readonly int _port;

        private Socket _socket;
        private Stream _stream;
        private Task _writing, _reading;

        public SocketConnection(string hostname, int port)
        {
            _hostname = hostname;
            _port = port;
        }

        // could this be async?
        public void Open()
        {
            Log.WriteLine("Open");
            
            var host = Dns.GetHostEntry(_hostname);
            var ipAddress = host.AddressList[0];
            var endpoint = new IPEndPoint(ipAddress, _port);

            _socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Log.WriteLine("Connect to server");
            _socket.Connect(endpoint); // fixme async?

            // use a stream, because we may use SSL and require an SslStream
            _stream = new NetworkStream(_socket, false);

            var pipe = new Pipe();
            _writing = WriteAsync(_stream, pipe.Writer);
            _reading = ReadAsync(pipe.Reader);

            Log.WriteLine("Opened");
        }

        // reads from socket and writes to the pipe
        private async Task WriteAsync(Stream stream, PipeWriter writer)
        {
            const int minimumBufferSize = 512;

            while (true)
            {
                // allocate at least 512 bytes from the PipeWriter
                var memory = writer.GetMemory(minimumBufferSize);
                try
                {
                    var bytesRead = await stream.ReadAsync(memory, CancellationToken.None);
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

                // make the data available to the PipeReader
                var result = await writer.FlushAsync();

                if (result.IsCompleted)
                {
                    break;
                }
            }

            // Tell the PipeReader that there's no more data coming
            Log.WriteLine("Complete writer");
            writer.Complete();
        }

        // reads from the pipe and processes data
        private async Task ReadAsync(PipeReader reader)
        {
            var expected = -1;

            // loop reading data from the pipe
            while (true)
            {
                Log.WriteLine("Wait for data");
                var result = await reader.ReadAsync();
                var buffer = result.Buffer;

                // see https://www.stevejgordon.co.uk/an-introduction-to-sequencereader

                Log.WriteLine($"Buffer: {buffer.Length}");

                // whenever we get data,
                // loop processing messages
                while (true)
                {
                    Log.WriteLine("Look for message");
                    if (expected < 0)
                    {
                        // not enough data, read more data
                        if (buffer.Length < 4)
                            break;

                        // get message length
                        // expected = deserialize 4 bytes
                        expected = buffer.ReadInt32();
                        Log.WriteLine($"Expecting message with size {expected} bytes");
                        buffer = buffer.Slice(4);
                    }

                    // not enough data, read more data
                    if (buffer.Length < expected)
                        break;

                    Log.WriteLine("Handle message");
                    //ProcessMessage(buffer.Slice(0, expected));
                    OnReceivedBytes(buffer.Slice(0, expected));
                    buffer = buffer.Slice(expected);
                    expected = -1;
                }

                // tell the PipeReader how much of the buffer we have consumed
                reader.AdvanceTo(buffer.Start, buffer.End);

                // stop reading if there's no more data coming
                if (result.IsCompleted)
                    break;
            }

            // mark the PipeReader as complete
            Log.WriteLine("Complete reader");
            reader.Complete();
        }

        public Action<ReadOnlySequence<byte>> OnReceivedBytes { get; set; }

        public async ValueTask SendAsync(byte[] bytes)
        {
            // note - look at how SendAsync is implemented, we may get closer to metal

            await _semaphore.WaitAsync();

            //foreach (var b in bytes)
            //    Console.Write($"{b:x2} ");
            //Console.WriteLine();

            await _stream.WriteAsync(bytes, 0, bytes.Length, CancellationToken.None);
            Log.WriteLine($"Sent {bytes.Length} bytes");

            _semaphore.Release();
        }

        public async ValueTask CloseAsync()
        {
            Log.WriteLine("Send empty message");
            await SendAsync(new byte[4]);

            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            _stream.Close();

            await _reading;
            await _writing;
        }
    }
}
