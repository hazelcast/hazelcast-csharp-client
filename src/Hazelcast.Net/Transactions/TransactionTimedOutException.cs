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
using Hazelcast.Exceptions;

namespace Hazelcast.Transactions
{
    /// <summary>
    /// Represents the exception that is thrown when a transaction times out.
    /// </summary>
    [Serializable]
    internal class TransactionTimedOutException : TransactionException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionTimedOutException"/> class.
        /// </summary>
        public TransactionTimedOutException()
            : base(ExceptionMessages.TransactionTimedOutException)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionTimedOutException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public TransactionTimedOutException(string message)
            : base(message)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionTimedOutException"/> class with a reference to
        /// the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null
        /// reference if no inner exception is specified.</param>
        public TransactionTimedOutException(Exception innerException)
            : base(ExceptionMessages.TransactionTimedOutException, innerException)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionTimedOutException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null
        /// reference if no inner exception is specified.</param>
        public TransactionTimedOutException(string message, Exception innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionTimedOutException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data
        /// about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        public TransactionTimedOutException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
