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
using Hazelcast.Logging;
using Hazelcast.Networking;

namespace Hazelcast.Messaging
{
    /// <summary>
    /// Represents a message connection.
    /// </summary>
    /// <remarks>
    /// <para>A message connection wraps a socket connection and provides a
    /// message-level communication channel.</para>
    /// </remarks>
    public class ClientMessageConnection // TODO: IDisposable?
    {
        private readonly Dictionary<long, ClientMessage> _messages = new Dictionary<long, ClientMessage>();
        private readonly SocketConnectionBase _connection;
        private readonly SemaphoreSlim _writer;

        private Func<ClientMessageConnection, ClientMessage, ValueTask> _onReceiveMessage;
        private int _bytesLength = -1;
        private Frame _currentFrame;
        private bool _finalFrame;
        private ClientMessage _currentMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientMessageConnection"/> class.
        /// </summary>
        /// <param name="connection">The underlying <see cref="SocketConnectionBase"/>.</param>
        public ClientMessageConnection(SocketConnectionBase connection)
        {
            _connection = connection;
            _connection.OnReceiveMessageBytes = ReceiveMessageBytes;

            // TODO consider threading an option (if controlled by owner?)
            _writer = new SemaphoreSlim(1);
        }

        /// <summary>
        /// Gets or sets the function that handles messages.
        /// </summary>
        public Func<ClientMessageConnection, ClientMessage, ValueTask> OnReceiveMessage
        {
            get => _onReceiveMessage;
            set
            {
                if (_connection.IsActive)
                    throw new InvalidOperationException("Cannot set the property once the connection is active.");
                _onReceiveMessage = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        private bool ReceiveMessageBytes(SocketConnectionBase connection, ref ReadOnlySequence<byte> bytes)
        {
            XConsole.WriteLine(this, $"Received {bytes.Length} bytes");

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
                    XConsole.WriteLine(this, $"Add {_currentFrame} to new fragment");
                    _currentMessage = new ClientMessage(_currentFrame);
                }
                else
                {
                    XConsole.WriteLine(this, $"Add {_currentFrame} to current fragment");
                    _currentMessage.Append(_currentFrame);
                }
            }

            // TODO consider buffering here
            // at the moment we are buffering in the pipe, but we have already
            // created the byte array, so ... might be nicer to copy now
            if (bytes.Length < _bytesLength)
                return false;

            bytes.Fill(_currentFrame.Bytes);
            _bytesLength = -1;
            XConsole.WriteLine(this, $"Frame is complete");

            // we now have a fully assembled message
            // don't test _currentFrame.IsFinal, adding the frame to a message has messed it
            if (_finalFrame)
            {
                XConsole.WriteLine(this, "Frame is final");
                var message = _currentMessage;
                _currentMessage = null;
                XConsole.WriteLine(this, "Handle fragment");
                HandleFragment(message);
            }

            return true;
        }

        private void HandleFragment(ClientMessage fragment)
        {
            if (fragment.Flags.Has(ClientMessageFlags.Unfragmented))
            {
                XConsole.WriteLine(this, "Handle message");
                // FIXME consider async?
                // this method cannot be async because of the ref bytes, and then
                // should onReceiveMessage be async or not?
                Task.Run(async () =>
                {
                    // FIXME this is really bad, because it's swallowing exceptions!
                    try
                    {
                        await _onReceiveMessage(this, fragment);
                    }
                    catch (Exception e)
                    {
                        XConsole.WriteLine(this, "ERROR\n" + e);
                    }
                });
                return;
            }

            // handle a fragmented message
            // TODO what shall we do with weird cases?!

            var fragmentId = fragment.FirstFrame.ReadFragmentId();

            if (fragment.Flags.Has(ClientMessageFlags.BeginFragment))
            {
                // new message
                if (_messages.TryGetValue(fragmentId, out var message))
                {
                    // receiving a duplicate fragment begin, ignoring
                    return;
                }

                // start accumulating
                _messages[fragmentId] = new ClientMessage().AppendFragment(fragment.FirstFrame.Next, fragment.LastFrame);
            }
            else if (fragment.Flags.Has(ClientMessageFlags.EndFragment))
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
                XConsole.WriteLine(this, "Handle message");
                Task.Run(async () => await _onReceiveMessage(this, message));
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

            // send message, serialize sending via semaphore
            // throws OperationCanceledException if canceled (and semaphore is not acquired)
            if (_writer != null) await _writer.WaitAsync(cancellationToken);

            try
            {
                XConsole.WriteLine(this, "Send message");

                var frame = message.FirstFrame;
                do
                {
                    if (cancellationToken.IsCancellationRequested)
                        throw new OperationCanceledException();

                    XConsole.WriteLine(this, $"Send frame ({frame.Length} bytes)");
                    if (!await SendFrameAsync(frame, cancellationToken))
                        return false;

                    // note that we may have sent some frames already, and that could
                    // confuse the server greatly (to never see the end of a message?)

                    frame = frame.Next;
                } while (frame != null);
            }
            finally
            {
                _writer?.Release();
            }

            return true;
        }

        private async ValueTask<bool> SendFrameAsync(Frame frame, CancellationToken cancellationToken)
        {
            const int sizeofHeader = FrameFields.SizeOf.LengthAndFlags;

            var header = ArrayPool<byte>.Shared.Rent(sizeofHeader);
            frame.WriteLengthAndFlags(header);

            if (!await _connection.SendAsync(header, sizeofHeader, cancellationToken))
                return false;

            ArrayPool<byte>.Shared.Return(header);

            return frame.Length <= sizeofHeader ||
                   await _connection.SendAsync(frame.Bytes, frame.Bytes.Length, cancellationToken);
        }
    }
}
