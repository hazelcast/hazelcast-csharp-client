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
using System.Text;
using System.Threading.Tasks;

namespace AsyncTests1.Networking
{
    // the ClientConnection is used by the Client, it handles messages
    // it manages a SocketConnection which handles bytes
    // it deals with serialization and de-serialization
    //

    /// <summary>
    /// Represents a message connection.
    /// </summary>
    /// <remarks>
    /// <para>A message connection wraps a socket connection and provides a
    /// message-level communication channel.</para>
    /// </remarks>
    public class MessageConnection
    {
        public readonly Log Log = new Log();
        private readonly SocketConnection _connection;

        private Func<MessageConnection, Message2, ValueTask> _onReceiveMessage;
        private int _bytesLength = -1;
        private Frame2 _currentFrame;
        private Message2 _currentMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageConnection"/> class.
        /// </summary>
        /// <param name="connection">The underlying <see cref="SocketConnection"/>.</param>
        public MessageConnection(SocketConnection connection)
        {
            _connection = connection;
            _connection.OnReceiveMessageBytes = ReceiveMessageBytes;
        }

        /// <summary>
        /// Gets or sets the function that handles messages.
        /// </summary>
        public Func<MessageConnection, Message2, ValueTask> OnReceiveMessage
        {
            get => _onReceiveMessage;
            set
            {
                if (true) // not open already
                    _onReceiveMessage = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        private bool ReceiveMessageBytes(SocketConnection connection, ref ReadOnlySequence<byte> bytes)
        {
            Log.WriteLine($"Received {bytes.Length} bytes");

            if (_bytesLength < 0)
            {
                if (bytes.Length < Frame2.SizeOf.LengthAndFlags)
                    return false;

                var frameLength = Frame2.ReadLength(ref bytes);
                var flags = Frame2.ReadFlags(ref bytes);
                _bytesLength = frameLength - Frame2.SizeOf.LengthAndFlags;

                var frameBytes = _bytesLength == 0
                    ? Array.Empty<byte>()
                    : new byte[_bytesLength]; // TODO postpone alloc?

                _currentFrame = new Frame2(flags, frameBytes);
                Log.WriteLine($"Add frame ({_currentFrame.Length} bytes)");
                if (_currentMessage == null)
                    _currentMessage = new Message2(_currentFrame);
                else
                    _currentMessage.Append(_currentFrame);
            }

            // should we keep buffering in the pipe, or consume here?
            // FIXME doing it asap is probably nice!
            if (bytes.Length < _bytesLength)
                return false;

            bytes.Fill(_currentFrame.Bytes);
            bytes = bytes.Slice(_bytesLength);
            _bytesLength = -1;

            // we now have a fully assembled message
            if (_currentFrame.IsFinal)
            {
                var message = _currentMessage;
                _currentMessage = null;
                Log.WriteLine("Handle message");
                // FIXME don't do this it's here already
                //message.CorrelationId = message.FirstFrame.Next.GetCorrelationId();
                //_onReceiveMessage(this, message); // FIXME async?! no because ref bytes?!
                Task.Run(async () => await _onReceiveMessage(this, message));
            }

            return true;
        }

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>A task that will complete when the message has been sent.</returns>
        public async ValueTask<bool> SendAsync(Message2 message)
        {
            // serialize the message into bytes,
            // and then pass those bytes to the socket connection

            Log.WriteLine("Send message");

            var frame = message.FirstFrame;
            do
            {
                Log.WriteLine($"Send frame ({frame.Length} bytes)");
                if (!await SendFrameAsync(frame))
                    return false;

                // FIXME what happens if we've sent some frames?

                frame = frame.Next;
            } while (frame != null);

            return true;
        }

        private async ValueTask<bool> SendFrameAsync(Frame2 frame)
        {
            var header = ArrayPool<byte>.Shared.Rent(6);
            frame.WriteLengthAndFlags(header);

            if (!await _connection.SendAsync(header, 6))
                return false;

            ArrayPool<byte>.Shared.Return(header);

            if (!await _connection.SendAsync(frame.Bytes))
                return false;

            return true;
        }

        // temp de-serialization stuff
        private static string GetAsciiString(ReadOnlySequence<byte> buffer)
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
    }
}
