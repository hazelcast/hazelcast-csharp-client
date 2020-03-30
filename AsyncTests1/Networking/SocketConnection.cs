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
    /// <para>The socket connection handle message bytes, and manages the network socket.</para>
    /// </remarks>
    public abstract class SocketConnection
    {
        // TODO - better exception handling
        // TODO - report when closed, either by remote or by failure

        private static readonly byte[] ZeroBytes4 = new byte[4];

        public readonly Log Log = new Log();
        private readonly SemaphoreSlim _writer;

        private Func<SocketConnection, ReadOnlySequence<byte>, ValueTask> _onReceiveMessageBytes;
        private Socket _socket;
        private Stream _stream;
        private Task _writing, _reading;
        private int _isOpen;

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketConnection"/> class.
        /// </summary>
        /// <param name="multithread">Whether this connection should manage multi-threading.</param>
        protected SocketConnection(bool multithread = true)
        {
            if (multithread)
                _writer = new SemaphoreSlim(1);
        }

        /// <summary>
        /// Gets or sets the function that handles message bytes.
        /// </summary>
        /// <remarks>
        /// <para>The function must process the content of the bytes sequence before it completes.
        /// The memory associated with the sequence is not guaranteed to remain available after the
        /// function has returned.</para>
        /// <para>The function must be set before <see cref="OpenPipe"/> is invoked.</para>
        /// </remarks>
        public Func<SocketConnection, ReadOnlySequence<byte>, ValueTask> OnReceiveMessageBytes
        {
            get => _onReceiveMessageBytes;
            set
            {
                if (true) // no open already
                    _onReceiveMessageBytes = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Opens the pipe.
        /// </summary>
        /// <remarks>
        /// <para>The <see cref="OnReceiveMessageBytes"/> function must be set before this function is invoked.</para>
        /// </remarks>
        protected void OpenPipe(Socket socket, Stream stream)
        {
            _socket = socket ?? throw new ArgumentNullException(nameof(socket));
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));

            if (_onReceiveMessageBytes == null)
                throw new InvalidOperationException("Missing message bytes handler.");

            Interlocked.Exchange(ref _isOpen, 1);

            // wire the pipe
            var pipe = new Pipe();
            _writing = WriteAsync(_stream, pipe.Writer);
            _reading = ReadAsync(pipe.Reader);
        }

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="bytes">The complete message bytes buffer.</param>
        /// <returns>A task that will complete when the message bytes have been sent.</returns>
        /// <remarks>
        /// <para>The complete message bytes buffer must include provision for 4 trailing
        /// bytes that will represent the length of the message, and are filled by this
        /// method.</para>
        /// </remarks>
        public async ValueTask SendAsync(byte[] bytes)
        {
            if (bytes.Length < 4)
                throw new ArgumentException("Must be at least 4 bytes.", nameof(bytes));

            // TODO: manage a flag to determine whether we're open
            //
            // set message length, using a fast-enough yet avoiding 'unsafe' code
            var length = bytes.Length - 4;
            unchecked
            {
                bytes[3] = (byte) length;
                length >>= 8;
                bytes[2] = (byte) length;
                length >>= 8;
                bytes[1] = (byte) length;
                length >>= 8;
                bytes[0] = (byte) length;
            }

            // send bytes, serialize sending via semaphore
            if (_writer != null) await _writer.WaitAsync();
            try
            {
                await _stream.WriteAsync(bytes, 0, bytes.Length, CancellationToken.None);
            }
            catch
            {
                // fixme what shall we do?
                // or should it be TrySendAsync?
            }
            _writer?.Release();

            Log.WriteLine($"Sent {bytes.Length} bytes");
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        /// <returns>A task that will complete when the connection has closed.</returns>
        public async ValueTask ShutdownAsync()
        {
            // send empty message to signal the other end
            // (ignore errors)
            Log.WriteLine("Send empty message");
            if (_writer != null) await _writer.WaitAsync();
            try
            {
                await _stream.WriteAsync(ZeroBytes4, 0, 4, CancellationToken.None);
            }
            catch { /* ignore */ }
            _writer?.Release();

            // shutdown
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            _stream.Close();

            // wait until the pipe is down, too
            // TODO can this throw?
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
        protected async Task WriteAsync(Stream stream, PipeWriter writer)
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

        // TODO: implement IDisposable?
        /// <inheritdoc />
        public void Dispose()
        {
            _socket?.Dispose();
            _stream?.Dispose();
        }
    }
}
