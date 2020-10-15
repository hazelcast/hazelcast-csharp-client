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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Networking;
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
        private readonly Dictionary<long, ClientMessage> _messages = new Dictionary<long, ClientMessage>();
        private readonly SocketConnectionBase _connection;
        private readonly IHSemaphore _writer;
        private readonly ILogger _logger;

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
            : this(connection, new HSemaphore(1, 1), loggerFactory)
        { }

        /// <summary>
        /// (internal for tests only)
        /// Initializes a new instance of the <see cref="ClientMessageConnection"/> class.
        /// </summary>
        /// <param name="connection">The underlying <see cref="SocketConnectionBase"/>.</param>
        /// <param name="writerSemaphore">A writer-controlling semaphore.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        internal ClientMessageConnection(SocketConnectionBase connection, IHSemaphore writerSemaphore, ILoggerFactory loggerFactory)
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
            HConsole.WriteLine(this, $"Received {bytes.Length} bytes");

            if (_bytesLength < 0)
            {
                if (bytes.Length < FrameFields.SizeOf.LengthAndFlags)
                    return false;

                var frameLength = Frame.ReadLength(ref bytes);
                var flags = Frame.ReadFlags(ref bytes);
                _bytesLength = frameLength - FrameFields.SizeOf.LengthAndFlags;

                // TODO: refactor byte[] allocations in frames
                var frameBytes = _bytesLength == 0
                    ? Array.Empty<byte>()
                    : new byte[_bytesLength];

                // create a frame
                // preserve the isFinal status, as adding the frame to a message messes it
                _currentFrame = new Frame(frameBytes, flags);
                _finalFrame = _currentFrame.IsFinal;

                if (_currentMessage == null)
                {
                    HConsole.WriteLine(this, $"Add {_currentFrame} to new fragment");
                    _currentMessage = new ClientMessage(_currentFrame);
                }
                else
                {
                    HConsole.WriteLine(this, $"Add {_currentFrame} to current fragment");
                    _currentMessage.Append(_currentFrame);
                }
            }

            // update the reference
            bufferReference.Buffer = bytes;

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
            bytes.Fill(_currentFrame.Bytes);
            bufferReference.Buffer = bytes;

            _bytesLength = -1;
            HConsole.WriteLine(this, $"Frame is complete");

            // we now have a fully assembled message
            // don't test _currentFrame.IsFinal, adding the frame to a message has messed it
            if (!_finalFrame) return true;

            HConsole.WriteLine(this, "Frame is final");
            var message = _currentMessage;
            _currentMessage = null;
            HConsole.WriteLine(this, "Handle fragment");
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
                    await _writer.WaitAsync(cancellationToken).CAF();
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

                var sentFrames = await SendFramesAsync(message, cancellationToken).CAF();
                if (!sentFrames) return false;

                await _connection.FlushAsync().CAF(); // make sure the message goes out
            }
            finally
            {
                _writer?.Release();
            }

            return true;
        }

        private async ValueTask<bool> SendFramesAsync(ClientMessage message, CancellationToken cancellationToken)
        {
            byte[] buffer = null;
            var length = 0;

            const int sizeofHeader = FrameFields.SizeOf.LengthAndFlags;

            // the default array pool allocates buckets of arrays of sizes:
            //   int maxSize = 16 << binIndex;
            // so 0 => 16 bytes, 1 => 32 bytes, 2 => 64 bytes etc. and therefore
            // the most efficient limit should be 16, 32, 64... ie 2^n
            // best size could probably only be picked via benchmarking?
            // trade-off between in-memory copy vs network traffic
            const int minLength = 1024;

            var frame = message.FirstFrame;

            do
            {
                if (frame.Length > minLength)
                {
                    // frame wont fit in buffer at all
                    // flush buffer
                    if (length > 0)
                    {
                        var sent = await _connection.SendAsync(buffer, length, default).CAF();
                        if (!sent) break;
                        length = 0;
                    }

                    // send this frame only
                    HConsole.WriteLine(this, $"Send frame ({frame.Length} bytes)");
                    var sentFrame = await SendFrameAsync(frame, default).CAF();
                    if (!sentFrame) break;
                }
                else
                {
                    // going to use a buffer
                    buffer ??= ArrayPool<byte>.Shared.Rent(minLength);

                    // if it won't fit in the buffer, flush the buffer
                    if (length + frame.Length > buffer.Length)
                    {
                        var sent = await _connection.SendAsync(buffer, length, default).CAF();
                        if (!sent) break;
                        length = 0;
                    }

                    // copy frame to buffer
                    HConsole.WriteLine(this, $"Send frame ({frame.Length} bytes)");
                    frame.WriteLengthAndFlags(buffer, length);
                    if (frame.Length > sizeofHeader)
                        frame.Bytes.CopyTo(buffer, length + sizeofHeader);
                    length += frame.Length;
                }

                frame = frame.Next;
            } while (frame != null);

            var allSent = frame == null;
            if (allSent && length > 0)
                allSent &= await _connection.SendAsync(buffer, length, default).CAF();

            if (buffer != null)
                ArrayPool<byte>.Shared.Return(buffer);

            return allSent;
        }

        // this is only for large frames
        private async ValueTask<bool> SendFrameAsync(Frame frame, CancellationToken cancellationToken)
        {
            const int sizeofHeader = FrameFields.SizeOf.LengthAndFlags;

            var header = ArrayPool<byte>.Shared.Rent(sizeofHeader);
            frame.WriteLengthAndFlags(header);

            var sentHeader = await _connection.SendAsync(header, sizeofHeader, cancellationToken).CAF();
            ArrayPool<byte>.Shared.Return(header);

            if (!sentHeader) return false;
            //if (frame.Length <= sizeofHeader) return true;

            return await _connection.SendAsync(frame.Bytes, frame.Bytes.Length, cancellationToken).CAF();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await _connection.DisposeAsync().CAF();
            _writer.Dispose();
        }
    }
}
