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
using Hazelcast.Protocol;

namespace Hazelcast.Exceptions
{
    /// <summary>
    /// Represents a client protocol error.
    /// </summary>
    [Serializable]
    public sealed class ClientProtocolException : HazelcastException
    {
        private const string InnerExceptionPrefix = " ---> ";

        // NOTE
        //
        // NUnit for instance does not rely on ToString() but has its own way of
        // displaying exceptions, which omits the Error and Retryable properties,
        // unless we also store them as data
        //
        private ClientProtocolErrors _error;
        private bool _retryable;

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastException"/> class.
        /// </summary>
        /// <param name="retryable">Whether the operation that threw the exception can be retried.</param>
        /// <param name="error">The client protocol error.</param>
        public ClientProtocolException(ClientProtocolErrors error, bool retryable = false)
            : base(error.ToString())
        {
            Error = error;
            Retryable = retryable;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastException"/> class with a specified error message.
        /// </summary>
        /// <param name="error">The client protocol error.</param>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="retryable">Whether the operation that threw the exception can be retried.</param>
        public ClientProtocolException(ClientProtocolErrors error, string message, bool retryable = false)
            : base(message)
        {
            Error = error;
            Retryable = retryable;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastException"/> class with a reference to
        /// the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="error">The client protocol error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null
        /// reference if no inner exception is specified.</param>
        /// <param name="retryable">Whether the operation that threw the exception can be retried.</param>
        public ClientProtocolException(ClientProtocolErrors error, Exception innerException, bool retryable = false)
            : base(error.ToString(), innerException)
        {
            Error = error;
            Retryable = retryable;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="error">The client protocol error.</param>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null
        /// reference if no inner exception is specified.</param>
        /// <param name="retryable">Whether the operation that threw the exception can be retried.</param>
        public ClientProtocolException(ClientProtocolErrors error, string message, Exception innerException, bool retryable = false)
            : base(message, innerException)
        {
            Error = error;
            Retryable = retryable;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data
        /// about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        public ClientProtocolException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Error = (ClientProtocolErrors) info.GetInt32("error");
            Retryable = info.GetBoolean("retryable");
        }

        /// <summary>
        /// Gets the protocol error.
        /// </summary>
        public ClientProtocolErrors Error
        {
            get => _error;
            set
            {
                _error = value;
                Data["error"] = value;
            }
        }

        /// <summary>
        /// Whether the operation that threw the exception can be retried.
        /// </summary>
        public bool Retryable
        {
            get => _retryable;
            set
            {
                _retryable = value;
                Data["retryable"] = value;
            }
        }

        /// <inheritdoc />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("error", Error);
            info.AddValue("retryable", Retryable);
            base.GetObjectData(info, context);
        }

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
            //var stackTrace = Data["server"];
            if (stackTrace != null)
            {
                s += Environment.NewLine + stackTrace;
            }

            var serverStackTrace = Data["server"];
            if (serverStackTrace != null)
            {
                s = s + Environment.NewLine + InnerExceptionPrefix + Error + Environment.NewLine + serverStackTrace + Environment.NewLine +
                    "   " + "--- End of server stack trace ---";
            }

            return s;
        }
    }
}