// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Protocol.Models;

namespace Hazelcast.Protocol
{
    /// <summary>
    /// Represents an exception that was thrown remotely on a server.
    /// </summary>
#if !NET8_0_OR_GREATER
    [Serializable]
#endif
    public sealed class RemoteException : HazelcastException
    {
        private const string InnerExceptionPrefix = " ---> ";

        // NOTE
        //
        // NUnit for instance does not rely on ToString() but has its own way of
        // displaying exceptions, which omits the Error and Retryable properties,
        // unless we also store them as data in Data[] !!

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastException"/> class.
        /// </summary>
        public RemoteException()
            : this(Guid.Empty, RemoteError.Undefined, "Remote Exception", null)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastException"/> class.
        /// </summary>
        /// <param name="memberId">The unique identifier of the member that threw the exception.</param>
        public RemoteException(Guid memberId)
            : this(memberId, RemoteError.Undefined, "Remote Exception", null)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public RemoteException(string message)
            : this(Guid.Empty, RemoteError.Undefined, message, null)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastException"/> class.
        /// </summary>
        /// <param name="memberId">The unique identifier of the member that threw the exception.</param>
        /// <param name="message">The message that describes the error.</param>
        public RemoteException(Guid memberId, string message)
            : this(memberId, RemoteError.Undefined, message, null)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public RemoteException(string message, Exception innerException)
            : this(Guid.Empty, RemoteError.Undefined, message, innerException)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastException"/> class.
        /// </summary>
        /// <param name="memberId">The unique identifier of the member that threw the exception.</param>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public RemoteException(Guid memberId, string message, Exception innerException)
            : this(memberId, RemoteError.Undefined, message, innerException)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastException"/> class.
        /// </summary>
        /// <param name="memberId">The unique identifier of the member that threw the exception.</param>
        /// <param name="retryable">Whether the operation that threw the exception can be retried.</param>
        /// <param name="error">The client protocol error.</param>
        public RemoteException(Guid memberId, RemoteError error, bool retryable = false)
            : this(memberId, error, error.ToString(), null, retryable: retryable)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastException"/> class with a specified error message.
        /// </summary>
        /// <param name="memberId">The unique identifier of the member that threw the exception.</param>
        /// <param name="error">The client protocol error.</param>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="retryable">Whether the operation that threw the exception can be retried.</param>
        public RemoteException(Guid memberId, RemoteError error, string message, bool retryable = false)
            : this(memberId, error, message, null, retryable: retryable)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastException"/> class with a reference to
        /// the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="memberId">The unique identifier of the member that threw the exception.</param>
        /// <param name="error">The client protocol error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null
        /// reference if no inner exception is specified.</param>
        /// <param name="retryable">Whether the operation that threw the exception can be retried.</param>
        public RemoteException(Guid memberId, RemoteError error, Exception innerException, bool retryable = false)
            : this(memberId, error, error.ToString(), innerException, retryable: retryable)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="memberId">The unique identifier of the member that threw the exception.</param>
        /// <param name="error">The client protocol error.</param>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null
        /// reference if no inner exception is specified.</param>
        /// <param name="serverStackTrace">A string representation of the frames on the server call stack.</param>
        /// <param name="retryable">Whether the operation that threw the exception can be retried.</param>
        public RemoteException(Guid memberId, RemoteError error, string message, Exception innerException, string serverStackTrace = "", bool retryable = false)
            : base(message, innerException)
        {
            MemberId = memberId;
            Error = error;
            Retryable = retryable;
            ServerStackTrace = serverStackTrace;
        }

#if !NET8_0_OR_GREATER
        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data
        /// about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        [Obsolete("This constructor is obsolete due to BinaryFormatter being obsolete. Use the constructor without this parameter.")]
        private RemoteException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            MemberId = info.GetGuid(nameof(MemberId));
            Error = (RemoteError) info.GetInt32(nameof(Error));
            Retryable = info.GetBoolean(nameof(Retryable));
            ServerStackTrace = info.GetString(nameof(ServerStackTrace));
        }
#endif
        /// <summary>
        /// Gets the unique identifier of the member which threw the exception.
        /// </summary>
        public Guid MemberId
        {
            get => (Guid) Data[nameof(MemberId)];
            set => Data[nameof(MemberId)] = value;
        }

        /// <summary>
        /// Gets the protocol error.
        /// </summary>
        public RemoteError Error
        {
            get => (RemoteError) Data[nameof(Error)];
            set => Data[nameof(Error)] = value;
        }

        /// <summary>
        /// Whether the operation that threw the exception can be retried.
        /// </summary>
        public bool Retryable
        {
            get => (bool) Data[nameof(Retryable)];
            set => Data[nameof(Retryable)] = value;
        }

        /// <summary>
        /// Gets a string representation of the frames on the server call stack.
        /// </summary>
        public string ServerStackTrace
        {
            get => (string) Data[nameof(ServerStackTrace)];
            set => Data[nameof(ServerStackTrace)] = value;
        }
#if !NET8_0_OR_GREATER
        /// <inheritdoc />
        [Obsolete("This constructor is obsolete due to BinaryFormatter being obsolete. Use the constructor without this parameter.")]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));

            info.AddValue(nameof(MemberId), MemberId);
            info.AddValue(nameof(Error), Error);
            info.AddValue(nameof(Retryable), Retryable);
            info.AddValue(nameof(ServerStackTrace), ServerStackTrace);
            base.GetObjectData(info, context);
        }
#endif
        /// <inheritdoc />
        public override string ToString()
        {
            // customize the exception name
            var s = GetType() + " (" + Error + (Retryable ? ",Retryable" : "") + ")";

            // the rest is exactly what the original Exception does
            var message = Message;
            if (!string.IsNullOrEmpty(message))
            {
                s += ": " + message;
            }

            if (InnerException != null)
            {
                s = s + Environment.NewLine + InnerExceptionPrefix + InnerException + Environment.NewLine +
                    "   " + "--- End of inner exception ---";
            }

            var stackTrace = StackTrace;
            if (stackTrace != null)
            {
                s += Environment.NewLine + stackTrace;
            }

            var serverStackTrace = ServerStackTrace;
            if (serverStackTrace != null)
            {
                s = s + Environment.NewLine + InnerExceptionPrefix + Error + Environment.NewLine + serverStackTrace + Environment.NewLine +
                    "   " + "--- End of remote stack trace ---";
            }

            return s;
        }
    }
}
