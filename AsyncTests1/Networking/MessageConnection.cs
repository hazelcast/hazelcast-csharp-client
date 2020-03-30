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

        private Func<MessageConnection, Message, ValueTask> _onReceiveMessage;

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
        public Func<MessageConnection, Message, ValueTask> OnReceiveMessage
        {
            get => _onReceiveMessage;
            set
            {
                if (true) // not open already
                    _onReceiveMessage = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Handles message bytes.
        /// </summary>
        /// <param name="connection">The underlying <see cref="SocketConnection"/>.</param>
        /// <param name="bytes">The message bytes.</param>
        /// <returns>A task that will complete when the bytes have been processed.</returns>
        private async ValueTask ReceiveMessageBytes(SocketConnection connection, ReadOnlySequence<byte> bytes)
        {
            // deserialize the bytes into a message,
            // and then pass that message to the upper layer

            Log.WriteLine($"Received {bytes.Length} bytes message");
            var text = GetAsciiString(bytes);
            var message = Message.Parse(text);

            if (_onReceiveMessage == null)
                throw new InvalidOperationException("No message handler has been configured.");

            await _onReceiveMessage(this, message);
        }

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>A task that will complete when the message has been sent.</returns>
        public async Task SendAsync(Message message)
        {
            // serialize the message into bytes,
            // and then pass those bytes to the socket connection

            Log.WriteLine($"Send \"{message}\"");
            var bytes = message.ToPrefixedBytes();
            await _connection.SendAsync(bytes);
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