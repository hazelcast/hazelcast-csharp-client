// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Networking;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Messaging
{
    /// <summary>
    /// Represents a message connection.
    /// </summary>
    /// <remarks>
    /// <para>A message connection wraps a socket connection and provides a
    /// message-level communication channel.</para>
    /// </remarks>
    internal class ClientMessageConnection : IAsyncDisposable
    {
        private readonly Dictionary<long, ClientMessage> _messages = new();
        private readonly SocketConnectionBase _connection;
        private readonly ISemaphoreSlim _writer;
        private readonly ILogger _logger;
        private int _disposed;
        private Action<ClientMessageConnection, ClientMessage> _onReceiveMessage;
        private int _bytesLength = -1;
        private Frame _currentFrame;
        private bool _finalFrame;
        private ClientMessage _currentMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientMessageConnection"/> class.
        /// </summary>
        /// <param name="connection">The underlying <see cref="SocketConnectionBase"/>.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public ClientMessageConnection(SocketConnectionBase connection, ILoggerFactory loggerFactory)
            : this(connection, new SemaphoreSlimImpl(1, 1), loggerFactory)
        { }

        /// <summary>
        /// (internal for tests only)
        /// Initializes a new instance of the <see cref="ClientMessageConnection"/> class.
        /// </summary>
        /// <param name="connection">The underlying <see cref="SocketConnectionBase"/>.</param>
        /// <param name="writerSemaphore">A writer-controlling semaphore.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        internal ClientMessageConnection(SocketConnectionBase connection, ISemaphoreSlim writerSemaphore, ILoggerFactory loggerFactory)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _connection.OnReceiveMessageBytes = ReceiveMessageBytesAsync;

            _logger = loggerFactory?.CreateLogger<ClientMessageConnection>() ??
                      throw new ArgumentNullException(nameof(loggerFactory));

            // TODO: threading control here could be an option
            // (in case threading control is performed elsewhere)
            _writer = writerSemaphore;
        }

        /// <summary>
        /// Gets the inner socket connection.
        /// </summary>
        public SocketConnectionBase SocketConnection => _connection;

        /// <summary>
        /// Gets or sets the function that handles messages.
        /// </summary>
        public Action<ClientMessageConnection, ClientMessage> OnReceiveMessage
        {
            get => _onReceiveMessage;
            set
            {
                if (_connection.IsActive)
                    throw new InvalidOperationException("Cannot set the property once the connection is active.");
                _onReceiveMessage = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// (internal for tests only)
        /// Gets or sets a function that runs when sending.
        /// </summary>
        internal Action OnSending { get; set; }

        private bool ReceiveMessageBytesAsync(SocketConnectionBase connection, IBufferReference<ReadOnlySequence<byte>> bufferReference)
        {
            var bytes = bufferReference.Buffer;
            HConsole.WriteLine(this, 2, $"Received {bytes.Length} bytes");

            if (_bytesLength < 0)
            {
                if (bytes.Length < FrameFields.SizeOf.LengthAndFlags)
                    return false;

                var frameLength = Frame.ReadLength(ref bytes);
                var flags = Frame.ReadFlags(ref bytes);
                _bytesLength = frameLength - FrameFields.SizeOf.LengthAndFlags;

                // create a frame; rent from pool for non-empty frames to reduce LOH pressure
                // preserve the isFinal status, as adding the frame to a message messes it
                if (_bytesLength == 0)
                {
                    _currentFrame = new Frame(flags);
                }
                else
                {
                    var rented = ArrayPool<byte>.Shared.Rent(_bytesLength);
                    _currentFrame = new Frame(rented, _bytesLength, flags); // pool=null → Dispose returns to ArrayPool<byte>.Shared
                }
                _finalFrame = _currentFrame.IsFinal;

                if (_currentMessage == null)
                {
                    HConsole.WriteLine(this, 2, $"Add {_currentFrame} to new fragment");
                    _currentMessage = new ClientMessage(_currentFrame);
                }
                else
                {
                    HConsole.WriteLine(this, 2, $"Add {_currentFrame} to current fragment");
                    _currentMessage.Append(_currentFrame);
                }
            }

            // NOTE
            // bufferReference.Buffer is a 'readonly struct', so its content (i.e. the content
            // of the bytes variable) *cannot* be modified - we pass bytes as a 'ref' to the
            // Frame.ReadXxx(ref bytes) methods, and they replace bytes with a new value - we
            // then have to update bufferReference.Buffer back

            // TODO: consider buffering here
            // at the moment we are buffering in the pipe, but we have already
            // created the byte array, so ... might be nicer to copy now
            if (bytes.Length < _bytesLength)
            {
                // update the reference, exit
                bufferReference.Buffer = bytes;
                return false;
            }

            // else, fill, and update the reference
            bytes.Fill(_currentFrame.Bytes.Span);
            bufferReference.Buffer = bytes;

            _bytesLength = -1;
            HConsole.WriteLine(this, 2, "Frame is complete");

            // we now have a fully assembled message
            // don't test _currentFrame.IsFinal, adding the frame to a message has messed it
            if (!_finalFrame) return true;

            HConsole.WriteLine(this, 2, "Frame is final");
            var message = _currentMessage;
            _currentMessage = null;
            HConsole.WriteLine(this, 2, "Handle fragment");
            ReceiveFragmentAsync(message);

            return true;
        }

        /// <summary>
        /// (internal for tests only)
        /// Receives a fragment.
        /// </summary>
        internal void ReceiveFragmentAsync(ClientMessage fragment)
        {
            if (fragment.Flags.HasAll(ClientMessageFlags.Unfragmented))
            {
                HConsole.WriteLine(this, "Handle message");

                try
                {
                    _onReceiveMessage(this, fragment);
                }
                catch (Exception e)
                {
                    // TODO: instrumentation
                    _logger.LogError(e, "Failed to handle an incoming message.");
                    HConsole.WriteLine(this, "ERROR\n" + e);
                }

                return;
            }

            // handle a fragmented message
            // TODO: can leak unfinished messages?

            var fragmentId = fragment.FragmentId;

            if (fragment.Flags.HasAll(ClientMessageFlags.BeginFragment))
            {
                // new message
                if (_messages.TryGetValue(fragmentId, out _))
                {
                    // receiving a duplicate fragment begin, ignoring
                    return;
                }

                // start accumulating
                _messages[fragmentId] = new ClientMessage().AppendFragment(fragment.FirstFrame.Next, fragment.LastFrame);
            }
            else if (fragment.Flags.HasAll(ClientMessageFlags.EndFragment))
            {
                // completed message
                if (!_messages.TryGetValue(fragmentId, out var message))
                {
                    // receiving a fragment end for an unknown message, ignoring
                    return;
                }

                // end
                message.AppendFragment(fragment.FirstFrame.Next, fragment.LastFrame);
                _messages.Remove(fragmentId);

                // handle the message
                HConsole.WriteLine(this, "Handle message");

                try
                {
                    _onReceiveMessage(this, message);
                }
                catch (Exception e)
                {
                    // TODO: instrumentation
                    _logger.LogError(e, "Failed to handle an incoming message.");
                    HConsole.WriteLine(this, "ERROR\n" + e);
                }
            }
            else
            {
                // continuing
                if (!_messages.TryGetValue(fragmentId, out var message))
                {
                    // receiving a fragment for an unknown message, ignoring
                    return;
                }

                // continue accumulating
                message.AppendFragment(fragment.FirstFrame.Next, fragment.LastFrame);
            }
        }

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the message has been sent.</returns>
        public async ValueTask<bool> SendAsync(ClientMessage message, CancellationToken cancellationToken = default)
        {
            // serialize the message into bytes,
            // and then pass those bytes to the socket connection

            if (message == null) throw new ArgumentNullException(nameof(message));
            if (message.FirstFrame == null) throw new ArgumentException("Message has no frames.", nameof(message));

            cancellationToken.ThrowIfCancellationRequested();

            // send message, serialize sending via semaphore
            // throws OperationCanceledException if canceled (and semaphore is not acquired)
            if (_writer != null)
            {
                try
                {
                    await _writer.WaitAsync(cancellationToken).CfAwait();
                }
                catch (ObjectDisposedException)
                {
                    // _writer can be non-null but disposed
                    return false;
                }
                catch (Exception e) when (!(e is OperationCanceledException))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw;
                }
            }

            try
            {
                HConsole.WriteLine(this, "Send message");

                // for tests purposes - do *not* rely on this
                OnSending?.Invoke();

                // last chance - after this line, we will send the full message
                cancellationToken.ThrowIfCancellationRequested();

                var sentFrames = await SendFramesAsync(message).CfAwait();
                if (!sentFrames) return false;

                var flushTask = _connection.FlushAsync(); // make sure the message goes out
                if (!flushTask.IsCompletedSuccessfully)
                    await flushTask.CfAwait();
            }
            finally
            {
                
                
                if (_writer != null)
                {
                    try
                    {
                        _writer.Release();
                    }
                    catch (ObjectDisposedException) // _writer can be non-null but disposed
                    { }
                }
            }

            return true;
        }

        private ValueTask<bool> SendFramesAsync(ClientMessage message)
        {
            const int sizeofHeader = FrameFields.SizeOf.LengthAndFlags;

            // Compute total wire size in one pass so we can rent a single buffer and
            // send the entire message in one WriteAsync call.  A single write halves the
            // time the semaphore is held for large-payload messages (e.g. 4 KB PUT), which
            // dramatically reduces tail latency under high concurrency.
            int totalSize = 0;
            for (var f = message.FirstFrame; f != null; f = f.Next)
                totalSize += f.Length;

            var buffer = ArrayPool<byte>.Shared.Rent(totalSize);
            int offset = 0;

            for (var frame = message.FirstFrame; frame != null; frame = frame.Next)
            {
                HConsole.WriteLine(this, 2, $"Send frame ({frame.Length} bytes)");
                if (frame.Owner is HeapData { HasFramePrefix: true } hd)
                {
                    // Pre-framed HeapData: GetWireMemory writes the 6-byte header in-place and
                    // returns [header][payload] as one contiguous region.
                    var wireMemory = hd.GetWireMemory((ushort)frame.Flags);
                    wireMemory.Span.CopyTo(buffer.AsSpan(offset));
                    offset += wireMemory.Length;
                }
                else
                {
                    frame.WriteLengthAndFlags(buffer, offset);
                    if (frame.Bytes.Length > 0)
                        frame.Bytes.Span.CopyTo(buffer.AsSpan(offset + sizeofHeader));
                    offset += frame.Length;
                }
            }

            var sendTask = _connection.SendAsync(buffer, totalSize);

            if (sendTask.IsCompletedSuccessfully)
            {
                var result = sendTask.Result;
                ArrayPool<byte>.Shared.Return(buffer);
                return new ValueTask<bool>(result);
            }

            return SendFramesAsyncSlow(sendTask, buffer);
        }

        private async ValueTask<bool> SendFramesAsyncSlow(ValueTask<bool> sendTask, byte[] buffer)
        {
            try
            {
                return await sendTask.CfAwait();
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Gets the life state of the connection.
        /// </summary>
        /// <param name="now">The reference date.</param>
        /// <param name="period">The life state check period.</param>
        /// <param name="timeout">The life state check timeout.</param>
        /// <returns>The life state of the connection.</returns>
        public ConnectionLifeState GetLifeState(DateTime now, TimeSpan period, TimeSpan timeout)
        {
            var readElapsed = now - _connection.LastReadTime;
            var writeElapsed = now - _connection.LastWriteTime;

            // make sure we read from the client at least every 'timeout', which is greater
            // than the interval, so we *should* have read from the last ping, if nothing else,
            // so no read means that the client not responsive - terminate it
            if (readElapsed > timeout && writeElapsed < period) return ConnectionLifeState.Dead;

            // make sure we write to the client at least every 'period',
            // this should trigger a read when we receive the response
            return writeElapsed > period ? ConnectionLifeState.Cold : ConnectionLifeState.Warm;
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (!_disposed.InterlockedZeroToOne()) return;

            // note: DisposeAsync should not throw (CA1065)
            await _connection.DisposeAsync().CfAwait(); // does not throw
            _writer.Dispose();
        }

    }
}
