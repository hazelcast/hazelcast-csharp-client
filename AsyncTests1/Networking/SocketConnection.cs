using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncTests1.Networking
{
    /// <summary>
    /// Represents a socket connection.
    /// </summary>
    /// <remarks>
    /// <para>The socket connection handle message bytes, and manages the network
    /// socket. It is used by the client connection.</para>
    /// </remarks>
    public abstract class SocketConnection
    {
        // see https://devblogs.microsoft.com/dotnet/system-io-pipelines-high-performance-io-in-net/
        //  https://habr.com/en/post/466137/
        // see https://www.stevejgordon.co.uk/an-introduction-to-sequencereader

        // a pipe is a "big" enough thing that should last,
        // so we should not constantly create them

        // TODO - better exception handling
        // TODO - report when closed, either by remote or by failure

        public const string LogName = "SCN";
        protected static readonly Log Log = new Log(LogName);
        private static readonly byte[] ZeroBytes4 = new byte[4];

        private readonly SemaphoreSlim _writer;
        private readonly Func<SocketConnection, ReadOnlySequence<byte>, ValueTask> _onReceiveMessageBytes;

        private Socket _socket;
        private Stream _stream;

        private Task _writing, _reading;

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketConnection"/> class.
        /// </summary>
        /// <param name="onReceiveMessageBytes">An action to execute when receiving a message.</param>
        /// <param name="multithread">Whether this connection should manage multi-threading.</param>
        /// <remarks>
        /// <para>The <paramref name="onReceiveMessageBytes"/> action must process the content of the
        /// bytes sequence before it returns. The memory associated with the sequence is not
        /// guaranteed to remain available after the action has returned.</para>
        /// <para>Classes inheriting <see cref="SocketConnection"/> are expected to assign <see cref="_socket"/>
        /// and <see cref="_stream"/> before allowing operations.</para>
        /// </remarks>
        protected SocketConnection(Func<SocketConnection, ReadOnlySequence<byte>, ValueTask> onReceiveMessageBytes, bool multithread = true)
        {
            _onReceiveMessageBytes = onReceiveMessageBytes ?? throw new ArgumentNullException(nameof(onReceiveMessageBytes));

            if (multithread)
                _writer = new SemaphoreSlim(1);
        }

        /// <summary>
        /// Opens the pipe.
        /// </summary>
        protected void OpenPipe(Socket socket, Stream stream)
        {
            _socket = socket ?? throw new ArgumentNullException(nameof(socket));
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));

            // wire the pipe
            var pipe = new Pipe();
            _writing = WriteAsync(_stream, pipe.Writer);
            _reading = ReadAsync(pipe.Reader);
        }

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="bytes">The message bytes.</param>
        /// <returns>A task that will complete when the message bytes have been sent.</returns>
        public async ValueTask SendAsync(byte[] bytes)
        {
            // TODO: manage a flag to determine whether we're open

            // TODO: should the message length be handled here?
            // TODO don't do it here, sends 2 network packets
            //
            var bytes4 = new byte[4]; // TODO benchmark pooling
            var length = bytes.Length;
            unchecked
            {
                bytes4[3] = (byte) length;
                length >>= 8;
                bytes4[2] = (byte) length;
                length >>= 8;
                bytes4[1] = (byte) length;
                length >>= 8;
                bytes4[0] = (byte) length;
            }

            // avoid slower method (and unsafe methods)
            /*
            for (var i = 3; i >= 0; i--)
            {
                bytes4[i] = (byte)length;
                length >>= 8;
            }
            */

            // send bytes, serialize sending via semaphore
            if (_writer != null) await _writer.WaitAsync();

            // TODO is _stream buffering the bytes? will this create 2 packets?
            //await _stream.WriteAsync(bytes4, 0, 4, CancellationToken.None);
            await _stream.WriteAsync(bytes, 0, bytes.Length, CancellationToken.None);

            _writer?.Release();

            Log.WriteLine($"Sent {bytes.Length} bytes");
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        /// <returns>A task that will complete when the connection has closed.</returns>
        public async ValueTask CloseAsync()
        {
            // send empty message to signal the other end
            Log.WriteLine("Send empty message");
            if (_writer != null) await _writer.WaitAsync();
            await _stream.WriteAsync(ZeroBytes4, 0, 4, CancellationToken.None);
            _writer?.Release();

            // shutdown
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            _stream.Close();

            // wait until the pipe is down, too
            await _reading;
            await _writing;
        }

        /// <summary>
        /// Reads from network, and writes to the pipe.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to read from.</param>
        /// <param name="writer">The <see cref="PipeWriter"/> to write to.</param>
        /// <returns>A task representing the write loop, that completes when the stream
        /// is closed, or when an error occurs.</returns>
        protected static async Task WriteAsync(Stream stream, PipeWriter writer)
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
                        break;

                    // tell the PipeWriter how much was read from the network
                    writer.Advance(bytesRead);
                }
                catch (Exception ex)
                {
                    // TODO: better error handling
                    Log.WriteLine("ERROR!");
                    Log.WriteLine(ex);
                    break;
                }

                // make the data available to the PipeReader
                var result = await writer.FlushAsync();

                if (result.IsCompleted)
                    break;
            }

            // tell the PipeReader that there's no more data coming
            Log.WriteLine("Writer completing");
            writer.Complete();
        }

        /// <summary>
        /// Reads from the pipe, and processes data.
        /// </summary>
        /// <param name="reader">The <see cref="PipeReader"/> to read from.</param>
        /// <returns>A task representing the read loop, that completes when an empty message
        /// is received, or when there is no more data coming (writer completed).</returns>
        protected async Task ReadAsync(PipeReader reader)
        {
            // expected message length
            // -1 means we don't know yet
            var expected = -1;

            // loop reading data from the pipe
            while (true)
            {
                // await data from the pipe
                Log.WriteLine("Await data from the pipe");
                var result = await reader.ReadAsync();
                var buffer = result.Buffer;

                Log.WriteLine($"Received data, buffer size is {buffer.Length} bytes");

                // process data
                while (true)
                {
                    Log.WriteLine("Process data");
                    if (expected < 0)
                    {
                        // we need at least 4 bytes to figure out the expected
                        // message length - otherwise, just keep reading
                        if (buffer.Length < 4)
                            break;

                        // deserialize expected message length (4 bytes)
                        expected = buffer.ReadInt32();
                        buffer = buffer.Slice(4);
                        if (expected == 0)
                        {
                            Log.WriteLine("Zero-length message, break");
                            break;
                        }

                        Log.WriteLine($"Expecting message with size {expected} bytes");
                    }

                    // not enough data, keep reading
                    if (buffer.Length < expected)
                        break;

                    // we have a message, handle it
                    Log.WriteLine("Message complete, handle");
                    await _onReceiveMessageBytes(this, buffer.Slice(0, expected));
                    buffer = buffer.Slice(expected);
                    expected = -1;
                }

                // tell the PipeReader how much of the buffer we have consumed
                reader.AdvanceTo(buffer.Start, buffer.End);

                // shutdown on empty message
                // TODO signal!
                if (expected == 0)
                    break;

                // stop reading if there's no more data coming
                if (result.IsCompleted)
                    break;
            }

            // mark the PipeReader as complete
            Log.WriteLine("Reader completing.");
            reader.Complete();
        }

        public void Dispose()
        {
            _socket?.Dispose();
            _stream?.Dispose();
        }
    }
}
