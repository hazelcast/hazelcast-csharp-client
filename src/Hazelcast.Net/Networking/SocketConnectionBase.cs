// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;

namespace Hazelcast.Networking
{
    /// <summary>
    /// Represents a socket connection.
    /// </summary>
    /// <remarks>
    /// <para>The socket connection handle message bytes, and manages the network socket.</para>
    /// </remarks>
    internal abstract partial class SocketConnectionBase : IAsyncDisposable
    {
        private readonly CancellationTokenSource _streamReadCancellationTokenSource = new CancellationTokenSource();

        private Func<SocketConnectionBase, IBufferReference<ReadOnlySequence<byte>>, bool> _onReceiveMessageBytes;
        private Func<SocketConnectionBase, ReadOnlySequence<byte>, ValueTask> _onReceivePrefixBytes;
        private Action<SocketConnectionBase> _onShutdown;
        private Task _pipeWriting, _pipeReading, _pipeWritingThenShutdown, _pipeReadingThenShutdown;
        private Socket _socket;
        private Stream _stream;
        private Pipe _pipe;
        private int _isActive;
        private int _isShutdown;
        private int _prefixLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketConnectionBase"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the connection.</param>
        /// <param name="prefixLength">An optional prefix length.</param>
        protected SocketConnectionBase(int id, int prefixLength = 0)
        {
            Id = id;

            _prefixLength = prefixLength;
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
        public Func<SocketConnectionBase, IBufferReference<ReadOnlySequence<byte>>, bool> OnReceiveMessageBytes
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
        public Func<SocketConnectionBase, ReadOnlySequence<byte>, ValueTask> OnReceivePrefixBytes
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
        public void ExpectPrefixBytes(int prefixLength, Func<SocketConnectionBase, ReadOnlySequence<byte>, ValueTask> onReceivePrefixBytes)
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
        public Action<SocketConnectionBase> OnShutdown
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
        public IPEndPoint RemoteEndPoint => _socket?.RemoteEndPoint as IPEndPoint;

        /// <summary>
        /// Gets the local endpoint of the connection.
        /// </summary>
        public IPEndPoint LocalEndPoint => _socket?.LocalEndPoint as IPEndPoint;

        /// <summary>
        /// Gets the pipe - for unit tests exclusively.
        /// </summary>
        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        protected Pipe Pipe => _pipe;

        /// <summary>
        /// (protected for tests only) Gets the stream read cancellation token source.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        protected CancellationTokenSource StreamReadCancellationTokenSource => _streamReadCancellationTokenSource;

        /// <summary>
        /// Ensures that the socket can open the pipe.
        /// </summary>
        protected void EnsureCanOpenPipe()
        {
            // _onShutdown is not mandatory, but validate _onReceiveMessageBytes
            if (_onReceiveMessageBytes == null)
                throw new InvalidOperationException("Missing message bytes handler.");
            if (_prefixLength > 0 && _onReceivePrefixBytes == null)
                throw new InvalidOperationException("Missing prefix bytes handler.");
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

            EnsureCanOpenPipe();

            Interlocked.Exchange(ref _isActive, 1);

            CreateTime = DateTime.Now;

            // wire the pipe
            _pipe = new Pipe();
            _pipeWriting = WritePipeAsync(_stream, _pipe.Writer);
            _pipeWritingThenShutdown = _pipeWriting.ContinueWith(ShutdownInternal, TaskScheduler.Current);
            _pipeReading = ReadPipeAsync(_pipe.Reader);
            _pipeReadingThenShutdown = _pipeReading.ContinueWith(ShutdownInternal, TaskScheduler.Current);
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

            HConsole.WriteLine(this, "Bringing connection down");

            Interlocked.Exchange(ref _isActive, 0);

            // ensure the pipe writing task aborts
            _streamReadCancellationTokenSource.Cancel();

            // ensure everything is down by awaiting the other task
            await (task == _pipeReading ? _pipeWriting : _pipeReading).CAF();

            // kill socket and stream
            try
            {
                _stream.Close();
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
            catch { /* ignore */ }

            HConsole.WriteLine(this, "Connection is down");

            // notify
            _onShutdown?.Invoke(this);
        }

        /// <summary>
        /// Sends bytes.
        /// </summary>
        /// <param name="bytes">The bytes to send.</param>
        /// <param name="length">The number of bytes to send.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the message bytes have been sent.</returns>
        // virtual for tests
        public virtual async ValueTask<bool> SendAsync(byte[] bytes, int length, CancellationToken cancellationToken = default)
        {
            if (_isActive == 0)
                return false;

            // send bytes
            try
            {
                await _stream.WriteAsync(bytes, 0, length, cancellationToken).CAF();
                LastWriteTime = DateTime.Now;
            }
            catch (Exception e)
            {
                // on error, shutdown and report
                HConsole.WriteLine(this, "SendAsync:ERROR");
                HConsole.WriteLine(this, e);
                _streamReadCancellationTokenSource.Cancel();
                return false;
            }

            HConsole.WriteLine(this, $"Sent {length} bytes" + HConsole.Lines(this, 1 , bytes.Dump(length)));
            return true;
        }

        /// <summary>
        /// Flushes the connection.
        /// </summary>
        /// <returns>A task that will complete when the connection is flushed.</returns>
        // virtual for tests
        public virtual async ValueTask FlushAsync()
        {
            await _stream.FlushAsync().CAF();
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
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            const int minimumBufferSize = 512;

            while (true)
            {
                // allocate at least 512 bytes from the PipeWriter
                var memory = writer.GetMemory(minimumBufferSize);
                int bytesRead;
                try
                {
                    HConsole.WriteLine(this, "Pipe writer waiting for data");
                    bytesRead = await stream.ReadAsync(memory, _streamReadCancellationTokenSource.Token).CAF();

                    if (bytesRead == 0)
                    {
                        HConsole.WriteLine(this, "Pipe writer received no data");
                        break;
                    }

                    LastReadTime = DateTime.Now;
                }
                catch (OperationCanceledException)
                {
                    // expected - just break
                    HConsole.WriteLine(this, "Pipe writer has been cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    // on error, shutdown and break, this will complete the reader
                    HConsole.WriteLine(this, "Pipe writer:ERROR");
                    HConsole.WriteLine(this, ex);
                    break;
                }

                // tell the PipeWriter how much was read from the network
                HConsole.WriteLine(this, $"Pipe writer received {bytesRead} bytes");
                writer.Advance(bytesRead);

                // make the data available to the PipeReader
                var result = await writer.FlushAsync().CAF();

                if (result.IsCompleted)
                {
                    HConsole.WriteLine(this, "Pipe is completed (in writer)");
                    break;
                }
            }

            // tell the PipeReader that there's no more data coming
            HConsole.WriteLine(this, "Pipe writer completing");
            await writer.CompleteAsync().CAF();
        }

        /// <summary>
        /// Reads from the pipe, and processes data.
        /// </summary>
        /// <param name="reader">The <see cref="PipeReader"/> to read from.</param>
        /// <returns>A task representing the read loop, that completes when an empty message
        /// is received, or when there is no more data coming (writer completed).</returns>
        protected async Task ReadPipeAsync(PipeReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            // loop reading data from the pipe
            var state = new ReadPipeState { Reader = reader };
            while (await ReadPipeLoop0(state).CAF()) { }

            // exception?
            if (state.Failed)
            {
                if (state.Exception != null)
                {
                    // TODO what shall we do with the exception?
                    HConsole.WriteLine(this, "ERROR " + state.Exception.SourceException);
                }
            }

            // mark the PipeReader as complete
            HConsole.WriteLine(this, "Pipe reader completing");
            await reader.CompleteAsync().CAF();
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
            HConsole.WriteLine(this, "Pipe reader awaits data from the pipe");
            var result = await state.Reader.ReadAsync().CAF();
            state.Buffer = result.Buffer;

            // no data means it's over
            if (state.Buffer.Length == 0)
            {
                HConsole.WriteLine(this, "Pipe reader received no data");
                return false;
            }

            HConsole.WriteLine(this, $"Pipe reader received data, buffer size is {state.Buffer.Length} bytes");

            // process data
            while (await ReadPipeLoop1(state).CAF()) { }

            // tell the PipeReader how much of the buffer we have consumed
            state.Reader.AdvanceTo(state.Buffer.Start, state.Buffer.End);

            // shutdown on crash
            if (state.Failed)
                return false;

            // stop reading if there's no more data coming
            if (result.IsCompleted)
            {
                HConsole.WriteLine(this, "Pipe is completed (in reader)");
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
            HConsole.WriteLine(this, "Pipe reader processes data" + HConsole.Lines(this, 1, state.Buffer.Dump()));

            if (_prefixLength > 0)
            {
                if (state.Buffer.Length < _prefixLength)
                {
                    HConsole.WriteLine(this, "Pipe reader has not enough data");
                    return false;
                }

                // we have a prefix, handle lit
                try
                {
                    HConsole.WriteLine(this, "Pipe reader received prefix");
                    await _onReceivePrefixBytes(this, state.Buffer.Slice(0, _prefixLength)).CAF();
                    state.Buffer = state.Buffer.Slice(_prefixLength);
                    _prefixLength = 0;
                }
                catch (Exception e)
                {
                    // error while processing, report and shutdown
                    HConsole.WriteLine(this, "Pipe reader encountered an exception while handling the prefix (shutdown)");
                    HConsole.WriteLine(this, e);
                    state.CaptureExceptionAndFail(e);
                    return false;
                }

                HConsole.WriteLine(this, "Pipe reader processes data");
            }

            HConsole.WriteLine(this, "Handle message bytes" + HConsole.Lines(this, 1, state.Buffer.Dump()));
            try
            {
                // handle the bytes (and slice the buffer accordingly)
                return _onReceiveMessageBytes(this, state);
            }
            catch (Exception e)
            {
                // error while processing, report
                HConsole.WriteLine(this, "Pipe reader encountered an exception while handling message bytes");
                HConsole.WriteLine(this, e);
                state.CaptureExceptionAndFail(e);
                return false;
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            // note: DisposeAsync should not throw (CA1065)

            if (Interlocked.CompareExchange(ref _isActive, 0, 1) == 0)
                return;

            // requests that the pipe stops processing - which will trigger
            // ShutdownInternal which will close the socket etc
            HConsole.WriteLine(this, "Cancel pipe");
            _streamReadCancellationTokenSource.Cancel();

            // wait for everything to be down
            await Task.WhenAll(_pipeWritingThenShutdown, _pipeReadingThenShutdown).CAF();
            HConsole.WriteLine(this, "Pipe is down");

            // dispose, ignore exceptions
            try
            {
                _stream.Close();
                _stream.Dispose();
            }
            catch { /* ignore */ }

            try
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Dispose();
            }
            catch { /* ignore */ }

            // ok to dispose again
            _streamReadCancellationTokenSource.Dispose();
        }
    }
}
