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
using System.Runtime.Serialization;

namespace Hazelcast.Exceptions
{
    /// <summary>
    /// Represents the exception that is thrown when the Hazelcast client is invoked but is not connected.
    /// </summary>
    /// <remarks>
    /// <para>The <see cref="State"/> property provides the <see cref="ConnectionState"/> of the client
    /// at the time the exception was thrown. The client may be either not connected at all, in which
    /// case retrying an operation will not succeed. Or, it may be  temporarily disconnected and trying
    /// to reconnect, in which case retrying an operation may eventually succeed.</para>
    /// </remarks>
    [Serializable]
    public sealed class ClientNotConnectedException : HazelcastException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientNotConnectedException"/> class.
        /// </summary>
        public ClientNotConnectedException()
            : base(ExceptionMessages.ClientNotConnectedException)
        {
            State = 0; // unknown
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientNotConnectedException"/> class.
        /// </summary>
        /// <param name="state">The client state.</param>
        public ClientNotConnectedException(ConnectionState state)
            : base(ExceptionMessages.ClientNotConnectedException)
        {
            State = state;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientNotConnectedException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ClientNotConnectedException(string message)
            : base(message)
        {
            State = 0; // unknown
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientNotConnectedException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="state">The client state.</param>
        public ClientNotConnectedException(string message, ConnectionState state)
            : base(message)
        {
            State = state;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientNotConnectedException"/> class with a reference to
        /// the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public ClientNotConnectedException(Exception innerException)
            : base(ExceptionMessages.ClientNotConnectedException, innerException)
        {
            State = 0; // unknown
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientNotConnectedException"/> class with a reference to
        /// the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        /// <param name="state">The client state.</param>
        public ClientNotConnectedException(Exception innerException, ConnectionState state)
            : base(ExceptionMessages.ClientNotConnectedException, innerException)
        {
            State = state;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientNotConnectedException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public ClientNotConnectedException(string message, Exception innerException)
            : base(message, innerException)
        {
            State = 0; // unknown
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientNotConnectedException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        /// <param name="state">The client state.</param>
        public ClientNotConnectedException(string message, Exception innerException, ConnectionState state)
            : base(message, innerException)
        {
            State = state;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientNotConnectedException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        private ClientNotConnectedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            State = (ConnectionState) info.GetInt32("state");
        }

        /// <inheritdoc />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));

            info.AddValue("state", State);
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Gets the connection state.
        /// </summary>
        public ConnectionState State { get; }
    }
}
