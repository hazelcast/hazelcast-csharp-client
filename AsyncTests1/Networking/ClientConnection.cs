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

    public class MessageConnection
    {
        public const string LogName = "CCN";
        private static readonly Log Log = new Log(LogName);

        private SocketConnection _connection;
        private readonly Func<MessageConnection, Message, ValueTask> _onReceiveMessage;

        public MessageConnection(SocketConnection connection, Func<MessageConnection, Message, ValueTask> onReceiveMessage)
        {
            _connection = connection;
            _onReceiveMessage = onReceiveMessage;
        }

        public MessageConnection(Func<Func<SocketConnection, ReadOnlySequence<byte>, ValueTask>, SocketConnection> socketFactory,
            Func<MessageConnection, Message, ValueTask> onReceiveMessage)
        {
            _connection = socketFactory(ReceiveMessageBytes);
            _onReceiveMessage = onReceiveMessage;
        }

        private async ValueTask ReceiveMessageBytes(SocketConnection connection, ReadOnlySequence<byte> bytes)
        {
            // deserialize the bytes into a message,
            // and then pass that message to the upper layer

            Log.WriteLine($"Received {bytes.Length} bytes message");
            var text = GetAsciiString(bytes);
            var message = Message.Parse(text);
            await _onReceiveMessage(this, message);
        }

        public async Task SendAsync(Message message)
        {
            // serialize the message into bytes,
            // and then pass those bytes to the socket connection

            Log.WriteLine($"Send \"{message}\"");
            var bytes = message.ToPrefixedBytes();
            await _connection.SendAsync(bytes);
        }

        string GetAsciiString(ReadOnlySequence<byte> buffer)
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

    public class ClientConnection
    {
        public const string LogName = "CCN";
        private static readonly Log Log = new Log(LogName);

        private readonly string _hostname;
        private readonly int _port;

        private ClientSocketConnection _connection;

        public ClientConnection(string hostname, int port)
        {
            _hostname = hostname;
            _port = port;
        }

        // could this be async?
        public void Open()
        {
            Log.WriteLine("Open");

            _connection = new ClientSocketConnection(_hostname, _port, ReceiveMessageBytes); // TODO: pass in ctor
            _connection.OpenAsync().AsTask().Wait();// FIXME

            Log.WriteLine("Opened");
        }

        private ValueTask ReceiveMessageBytes(SocketConnection connection, ReadOnlySequence<byte> bytes)
        {
            // deserialize the bytes into a message,
            // and then pass that message to the upper layer

            Log.WriteLine($"Received {bytes.Length} bytes message");
            var text = GetAsciiString(bytes);
            OnReceivedMessage(Message.Parse(text));
            return new ValueTask();
        }

        public async Task CloseAsync()
        {
            await _connection.CloseAsync();
        }

        public async Task SendAsync(Message message)
        {
            // serialize the message into bytes,
            // and then pass those bytes to the socket connection

            Log.WriteLine($"Send \"{message}\"");
            var bytes = message.ToPrefixedBytes();
            await _connection.SendAsync(bytes);
        }

        public Action<Message> OnReceivedMessage { get; set; }

        string GetAsciiString(ReadOnlySequence<byte> buffer)
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