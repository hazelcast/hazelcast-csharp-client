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
    /// <summary>
    /// Represents a socket connection.
    /// </summary>
    /// <remarks>
    /// <para>The socket connection handle message bytes, and manages the network socket.</para>
    /// </remarks>
    public abstract class SocketConnection
    {
        private static readonly byte[] ZeroBytes4 = new byte[4];

        private readonly CancellationTokenSource _streamReadCancellationTokenSource = new CancellationTokenSource();
        public readonly Log Log = new Log();
        private readonly SemaphoreSlim _writer;

        private Func<SocketConnection, ReadOnlySequence<byte>, ValueTask> _onReceiveMessageBytes;
        private Func<SocketConnection, ValueTask> _onShutdown;
        private Task _pipeWriting, _pipeReading, _pipeWritingThenShutdown, _pipeReadingThenShutdown;
        private Socket _socket;
        private Stream _stream;
        private int _isActive;
        private int _isShutdown;

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketConnection"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the connection.</param>
        /// <param name="multithread">Whether this connection should manage multi-threading.</param>
        protected SocketConnection(int id, bool multithread = true)
        {
            Id = id;

            if (multithread)
                _writer = new SemaphoreSlim(1);
        }

        /// <summary>
        /// Gets the unique identifier of the socket.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets or sets the function that handles message bytes.
        /// </summary>
        /// <remarks>
        /// <para>The function must process the content of the bytes sequence before it completes.
        /// The memory associated with the sequence is not guaranteed to remain available after the
        /// function has returned.</para>
        /// <para>The function must be set before the connection is established.</para>
        /// </remarks>
        public Func<SocketConnection, ReadOnlySequence<byte>, ValueTask> OnReceiveMessageBytes
        {
            get => _onReceiveMessageBytes;
            set
            {
                if (_isActive == 1)
                    throw new InvalidOperationException("Cannot set the property once the connection is active.");

                _onReceiveMessageBytes = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Gets or sets the function that handle shutdowns.
        /// </summary>
        /// <remarks>
        /// <para>The function must be set before the connection is established.</para>
        /// </remarks>
        public Func<SocketConnection, ValueTask> OnShutdown
        {
            get => _onShutdown;
            set
            {
                if (_isActive == 1)
                    throw new InvalidOperationException("Cannot set the property once the connection is active.");

                _onShutdown = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Gets a value indicating whether the connection is active.
        /// </summary>
        public bool IsActive => _isActive == 1;

        /// <summary>
        /// Gets the date and time when the connection was created.
        /// </summary>
        public DateTime CreateTime { get; private set; }

        /// <summary>
        /// Gets the date and time when bytes were last written by the connection.
        /// </summary>
        public DateTime LastWriteTime { get; private set; }

        /// <summary>
        /// Gets the date and time when bytes were last read by the connection.
        /// </summary>
        public DateTime LastReadTime { get; private set; }

        /// <summary>
        /// Gets the remote endpoint of the connection.
        /// </summary>
        public EndPoint RemotEndPoint => _socket?.RemoteEndPoint;

        /// <summary>
        /// Gets the local endpoint of the connection.
        /// </summary>
        public EndPoint LocalEndPoint => _socket?.LocalEndPoint;

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

            // _onShutdown is not mandatory, but validate _onReceiveMessageBytes
            if (_onReceiveMessageBytes == null)
                throw new InvalidOperationException("Missing message bytes handler.");

            Interlocked.Exchange(ref _isActive, 1);

            CreateTime = DateTime.Now;

            // wire the pipe
            var pipe = new Pipe();
            _pipeWriting = WritePipeAsync(_stream, pipe.Writer);
            _pipeWritingThenShutdown = _pipeWriting.ContinueWith(ShutdownInternal);
            _pipeReading = ReadPipeAsync(pipe.Reader);
            _pipeReadingThenShutdown = _pipeReading.ContinueWith(ShutdownInternal);
        }

        /// <summary>
        /// Shuts the connection down after a task has completed.
        /// </summary>
        /// <param name="task">The completed task.</param>
        /// <returns>A task that will complete when the connection is down.</returns>
        private async ValueTask ShutdownInternal(Task task)
        {
            // only once
            if (Interlocked.CompareExchange(ref _isShutdown, 1, 0) == 1)
                return;

            Interlocked.Exchange(ref _isActive, 0);

            // ensure everything is down by awaiting the other task
            await (task == _pipeReading ? _pipeWriting : _pipeReading);

            // kill socket and stream
            try
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
                _stream.Close();
            }
            catch { /* ignore */ }

            Log.WriteLine("Connection is down");

            // notify
            if (_onShutdown != null)
                await _onShutdown(this);
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
        public async ValueTask<bool> SendAsync(byte[] bytes)
        {
            if (bytes.Length < 4)
                throw new ArgumentException("Must be at least 4 bytes.", nameof(bytes));

            if (_isActive == 0)
                return false;

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
                LastWriteTime = DateTime.Now;
            }
            catch (Exception e)
            {
                // on error, shutdown and report
                Log.WriteLine("SendAsync:ERROR");
                Log.WriteLine(e);
                _streamReadCancellationTokenSource.Cancel();
                return false;
            }
            _writer?.Release();

            Log.WriteLine($"Sent {bytes.Length} bytes");
            return true;
        }

        /// <summary>
        /// Sends raw bytes.
        /// </summary>
        /// <param name="bytes">The bytes to send.</param>
        /// <returns>A task that will complete when the message bytes have been sent.</returns>
        public async ValueTask<bool> SendRawAsync(byte[] bytes)
        {
            if (_isActive == 0)
                return false;

            // send bytes, serialize sending via semaphore
            if (_writer != null) await _writer.WaitAsync();
            try
            {
                await _stream.WriteAsync(bytes, 0, bytes.Length, CancellationToken.None);
                LastWriteTime = DateTime.Now;
            }
            catch (Exception e)
            {
                // on error, shutdown and report
                Log.WriteLine("SendRawAsync:ERROR");
                Log.WriteLine(e);
                _streamReadCancellationTokenSource.Cancel();
                return false;
            }
            _writer?.Release();

            Log.WriteLine($"Sent {bytes.Length} raw bytes");
            return true;
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        /// <returns>A task that will complete when the connection has closed.</returns>
        public async ValueTask ShutdownAsync()
        {
            if (Interlocked.CompareExchange(ref _isActive, 0, 1) == 0)
                return;

            // notify other end with an empty message
            Log.WriteLine("Send empty message");
            if (_writer != null) await _writer.WaitAsync();
            try
            {
                await _stream.WriteAsync(ZeroBytes4, 0, 4, CancellationToken.None);
            }
            catch { /* ignore */ }
            _writer?.Release();

            // requests that the pipe stops processing
            Log.WriteLine("Cancel pipe");
            _streamReadCancellationTokenSource.Cancel();

            // wait for everything to be down
            await _pipeWritingThenShutdown;
            await _pipeReadingThenShutdown;
        }

        /// <summary>
        /// Reads from network, and writes to the pipe.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to read from.</param>
        /// <param name="writer">The <see cref="PipeWriter"/> to write to.</param>
        /// <returns>A task representing the write loop, that completes when the stream
        /// is closed, or when an error occurs.</returns>
        protected async Task WritePipeAsync(Stream stream, PipeWriter writer)
        {
            const int minimumBufferSize = 512;

            while (true)
            {
                // allocate at least 512 bytes from the PipeWriter
                var memory = writer.GetMemory(minimumBufferSize);
                int bytesRead;
                try
                {
                    bytesRead = await stream.ReadAsync(memory, _streamReadCancellationTokenSource.Token);
                    if (bytesRead == 0)
                    {
                        Log.WriteLine("Pipe writer received no data");
                        break;
                    }

                    LastReadTime = DateTime.Now;
                }
                catch (OperationCanceledException)
                {
                    // expected - just break
                    Log.WriteLine("Pipe writer has been cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    // on error, shutdown and break, this will complete the reader
                    Log.WriteLine("Pipe writer:ERROR");
                    Log.WriteLine(ex);
                    break;
                }

                // tell the PipeWriter how much was read from the network
                writer.Advance(bytesRead);

                // make the data available to the PipeReader
                var result = await writer.FlushAsync();

                if (result.IsCompleted)
                {
                    Log.WriteLine("Pipe is completed (in writer)");
                    break;
                }
            }

            // tell the PipeReader that there's no more data coming
            Log.WriteLine("Pipe writer completing");
            writer.Complete();
        }

        /// <summary>
        /// Reads from the pipe, and processes data.
        /// </summary>
        /// <param name="reader">The <see cref="PipeReader"/> to read from.</param>
        /// <returns>A task representing the read loop, that completes when an empty message
        /// is received, or when there is no more data coming (writer completed).</returns>
        protected async Task ReadPipeAsync(PipeReader reader)
        {
            // loop reading data from the pipe
            var state = new ReadPipeState { Reader = reader };
            while (await ReadPipeLoop0(state)) { }

            // mark the PipeReader as complete
            Log.WriteLine("Pipe reader completing");
            reader.Complete();
        }

        /// <summary>
        /// Reads from the pipe, and processes data.
        /// </summary>
        /// <param name="state">The reading state.</param>
        /// <returns>A task that will complete when data has been read and processed,
        /// and represents whether to continue reading.</returns>
        private async ValueTask<bool> ReadPipeLoop0(ReadPipeState state)
        {
            // await data from the pipe
            Log.WriteLine("Pipe reader awaits data from the pipe");
            var result = await state.Reader.ReadAsync();
            state.Buffer = result.Buffer;

            // no data means it's over
            if (state.Buffer.Length == 0)
            {
                Log.WriteLine("Pipe reader received no data");
                return false;
            }

            Log.WriteLine($"Pipe reader received data, buffer size is {state.Buffer.Length} bytes");

            // process data
            while (await ReadPipeLoop1(state)) { }

            // tell the PipeReader how much of the buffer we have consumed
            state.Reader.AdvanceTo(state.Buffer.Start, state.Buffer.End);

            // shutdown on empty message
            if (state.Expected == 0)
                return false;

            // stop reading if there's no more data coming
            if (result.IsCompleted)
            {
                Log.WriteLine("Pipe is completed (in reader)");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Processes data from the pipe.
        /// </summary>
        /// <param name="state">The reading state.</param>
        /// <returns>A task that will complete when data has been processed,
        /// and represents whether to continue processing.</returns>
        private async ValueTask<bool> ReadPipeLoop1(ReadPipeState state)
        {
            Log.WriteLine("Pipe reader processes data");
            if (state.Expected < 0)
            {
                // we need at least 4 bytes to figure out the expected
                // message length - otherwise, just keep reading
                if (state.Buffer.Length < 4)
                {
                    Log.WriteLine("Pipe reader has not enough data");
                    return false;
                }

                // deserialize expected message length (4 bytes)
                state.Expected = state.Buffer.ReadInt32();
                state.Buffer = state.Buffer.Slice(4);
                if (state.Expected == 0)
                {
                    Log.WriteLine("Pipe reader received zero-length message, shutdown");
                    return false;
                }

                Log.WriteLine($"Pipe reader expecting message with size {state.Expected} bytes");
            }

            // not enough data, keep reading
            if (state.Buffer.Length < state.Expected)
            {
                Log.WriteLine("Pipe reader has not enough data");
                return false;
            }

            // we have a message, handle it
            Log.WriteLine("Pipe reader has complete message, handle");
            try
            {
                await _onReceiveMessageBytes(this, state.Buffer.Slice(0, state.Expected));
            }
            catch (Exception e)
            {
                // error while processing, report
                Log.WriteLine("Pipe reader:ERROR");
                Log.WriteLine(e);
            }
            state.Buffer = state.Buffer.Slice(state.Expected);
            state.Expected = -1;

            return true;
        }

        /// <summary>
        /// Represents the state of the reading loop.
        /// </summary>
        private class ReadPipeState
        {
            /// <summary>
            /// Gets or sets the pipe reader.
            /// </summary>
            public PipeReader Reader;

            /// <summary>
            /// Gets or sets the current buffer.
            /// </summary>
            public ReadOnlySequence<byte> Buffer;

            /// <summary>
            /// Gets or sets the expected message length.
            /// </summary>
            /// <remarks>
            /// <para>A value of -1 means that we do not know yet.</para>
            /// </remarks>
            public int Expected = -1;
        }
    }
}
