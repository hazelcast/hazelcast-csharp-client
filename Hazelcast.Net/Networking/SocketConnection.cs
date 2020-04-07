using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Logging;

namespace Hazelcast.Networking
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

        /// <summary>
        /// Handles bytes
        /// </summary>
        /// <param name="connection">The originating connection.</param>
        /// <param name="bytes">The bytes to handle.</param>
        /// <returns>Whether to continue handling the available bytes. Otherwise, wait for more bytes.</returns>
        public delegate bool MessageBytesHandler(SocketConnection connection, ref ReadOnlySequence<byte> bytes);

        private readonly CancellationTokenSource _streamReadCancellationTokenSource = new CancellationTokenSource();
        private readonly SemaphoreSlim _writer;

        private MessageBytesHandler _onReceiveMessageBytes;
        private Func<SocketConnection, ReadOnlySequence<byte>, ValueTask> _onReceivePrefixBytes;
        private Func<SocketConnection, ValueTask> _onShutdown;
        private Task _pipeWriting, _pipeReading, _pipeWritingThenShutdown, _pipeReadingThenShutdown;
        private Socket _socket;
        private Stream _stream;
        private int _isActive;
        private int _isShutdown;
        private int _prefixLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketConnection"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the connection.</param>
        /// <param name="prefixLength">An optional prefix length.</param>
        /// <param name="multithread">Whether this connection should manage multi-threading.</param>
        protected SocketConnection(int id, int prefixLength = 0, bool multithread = true)
        {
            Id = id;

            _prefixLength = prefixLength;

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
        public MessageBytesHandler OnReceiveMessageBytes
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
        /// Gets or sets the function that handles prefix bytes.
        /// </summary>
        /// <remarks>
        /// <para>The function must process the content of the bytes sequence before it completes.
        /// The memory associated with the sequence is not guaranteed to remain available after the
        /// function has returned.</para>
        /// <para>The function must be set before the connection is established.</para>
        /// </remarks>
        public Func<SocketConnection, ReadOnlySequence<byte>, ValueTask> OnReceivePrefixBytes
        {
            get => _onReceivePrefixBytes;
            set
            {
                if (_isActive == 1)
                    throw new InvalidOperationException("Cannot set the property once the connection is active.");

                _onReceivePrefixBytes = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Specifies that the connection should expect prefix bytes.
        /// </summary>
        /// <param name="prefixLength">The prefix length.</param>
        /// <param name="onReceivePrefixBytes">The function that handles prefix bytes.</param>
        public void ExpectPrefixBytes(int prefixLength, Func<SocketConnection, ReadOnlySequence<byte>, ValueTask> onReceivePrefixBytes)
        {
            _prefixLength = prefixLength;
            _onReceivePrefixBytes = onReceivePrefixBytes;
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
            if (_prefixLength > 0 && _onReceivePrefixBytes == null)
                throw new InvalidOperationException("Missing prefix bytes handler.");

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

            XConsole.WriteLine(this, "Connection is down");

            // notify
            if (_onShutdown != null)
                await _onShutdown(this);
        }

        /// <summary>
        /// Sends bytes.
        /// </summary>
        /// <param name="bytes">The bytes to send.</param>
        /// <param name="length">The number of bytes to send, or zero to send everything.</param>
        /// <returns>A task that will complete when the message bytes have been sent.</returns>
        public async ValueTask<bool> SendAsync(byte[] bytes, int length = 0)
        {
            if (_isActive == 0)
                return false;

            var count = length <= 0 ? bytes.Length : length;

            // send bytes, serialize sending via semaphore
            if (_writer != null) await _writer.WaitAsync();
            try
            {
                await _stream.WriteAsync(bytes, 0, count, CancellationToken.None);
                LastWriteTime = DateTime.Now;
            }
            catch (Exception e)
            {
                // on error, shutdown and report
                XConsole.WriteLine(this, "SendAsync:ERROR");
                XConsole.WriteLine(this, e);
                _streamReadCancellationTokenSource.Cancel();
                return false;
            }
            _writer?.Release();

            XConsole.WriteLine(this, $"Sent {count} bytes" + bytes.Dump("\n> ", count));
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
            XConsole.WriteLine(this, "Send empty message");
            if (_writer != null) await _writer.WaitAsync();
            try
            {
                await _stream.WriteAsync(ZeroBytes4, 0, 4, CancellationToken.None);
            }
            catch { /* ignore */ }
            _writer?.Release();

            // requests that the pipe stops processing
            XConsole.WriteLine(this, "Cancel pipe");
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
                        XConsole.WriteLine(this, "Pipe writer received no data");
                        break;
                    }

                    LastReadTime = DateTime.Now;
                }
                catch (OperationCanceledException)
                {
                    // expected - just break
                    XConsole.WriteLine(this, "Pipe writer has been cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    // on error, shutdown and break, this will complete the reader
                    XConsole.WriteLine(this, "Pipe writer:ERROR");
                    XConsole.WriteLine(this, ex);
                    break;
                }

                // tell the PipeWriter how much was read from the network
                writer.Advance(bytesRead);

                // make the data available to the PipeReader
                var result = await writer.FlushAsync();

                if (result.IsCompleted)
                {
                    XConsole.WriteLine(this, "Pipe is completed (in writer)");
                    break;
                }
            }

            // tell the PipeReader that there's no more data coming
            XConsole.WriteLine(this, "Pipe writer completing");
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

            // exception?
            if (state.Failed)
            {
                if (state.Exception != null)
                {
                    // TODO what shall we do with the exception?
                    Console.WriteLine("ERROR");
                    Console.WriteLine(state.Exception.SourceException);
                }

            }

            // mark the PipeReader as complete
            XConsole.WriteLine(this, "Pipe reader completing");
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
            XConsole.WriteLine(this, "Pipe reader awaits data from the pipe");
            var result = await state.Reader.ReadAsync();
            state.Buffer = result.Buffer;

            // no data means it's over
            if (state.Buffer.Length == 0)
            {
                XConsole.WriteLine(this, "Pipe reader received no data");
                return false;
            }

            XConsole.WriteLine(this, $"Pipe reader received data, buffer size is {state.Buffer.Length} bytes");

            // process data
            while (await ReadPipeLoop1(state)) { }

            // tell the PipeReader how much of the buffer we have consumed
            state.Reader.AdvanceTo(state.Buffer.Start, state.Buffer.End);

            // shutdown on crash
            if (state.Failed)
                return false;

            // stop reading if there's no more data coming
            if (result.IsCompleted)
            {
                XConsole.WriteLine(this, "Pipe is completed (in reader)");
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
            XConsole.WriteLine(this, "Pipe reader processes data" + state.Buffer.Dump("\n< "));

            if (_prefixLength > 0)
            {
                if (state.Buffer.Length < _prefixLength)
                {
                    XConsole.WriteLine(this, "Pipe reader has not enough data");
                    return false;
                }

                // we have a prefix, handle lit
                try
                {
                    XConsole.WriteLine(this, "Pipe reader received prefix");
                    await _onReceivePrefixBytes(this, state.Buffer.Slice(0, _prefixLength));
                    state.Buffer = state.Buffer.Slice(_prefixLength);
                    _prefixLength = 0;
                }
                catch (Exception e)
                {
                    // error while processing, report and shutdown
                    XConsole.WriteLine(this, "Pipe reader encountered an exception while handling the prefix (shutdown)");
                    XConsole.WriteLine(this, e);
                    state.CaptureExceptionAndFail(e);
                    return false;
                }

                XConsole.WriteLine(this, "Pipe reader processes data");
            }

            XConsole.WriteLine(this, "Handle message bytes" + state.Buffer.Dump("\n< "));
            try
            {
                // handle the bytes (and slice the buffer accordingly)
                return _onReceiveMessageBytes(this, ref state.Buffer);
            }
            catch (Exception e)
            {
                // error while processing, report
                XConsole.WriteLine(this, "Pipe reader encountered an exception while handling message bytes");
                XConsole.WriteLine(this, e);
                state.CaptureExceptionAndFail(e);
                return false;
            }
        }

        /// <summary>
        /// Represents the state of the reading loop.
        /// </summary>
        private sealed class ReadPipeState
        {
            /// <summary>
            /// Gets or sets the pipe reader.
            /// </summary>
            public PipeReader Reader { get; set; }

            /// <summary>
            /// Gets or sets the current buffer.
            /// </summary>
            [SuppressMessage("NDepend", "ND1805:FieldsShouldBeDeclaredAsPrivate", Justification =
                "Has to be a field so we can pass it as a 'ref' to avoid copying the struct.")]
            public ReadOnlySequence<byte> Buffer;

            /// <summary>
            /// Determines whether reading has failed.
            /// </summary>
            public bool Failed { get; private set; }

            /// <summary>
            /// Gets the optional exception that caused the failure.
            /// </summary>
            public ExceptionDispatchInfo Exception { get; private set; }

            /// <summary>
            /// Captures an exception and registers the failure.
            /// </summary>
            /// <param name="e">The exception.</param>
            public void CaptureExceptionAndFail(Exception e)
            {
                // this should never happen, and we cannot do much about it
                if (Exception != null)
                    return;

                Failed = true;
                Exception = ExceptionDispatchInfo.Capture(e);
            }
        }
    }
}
