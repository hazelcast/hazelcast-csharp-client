﻿// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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

#nullable enable

using System;
using System.Runtime.Serialization;
using Hazelcast.Exceptions;

namespace Hazelcast.Serialization
{
    /// <summary>
    /// Represents an exception that is thrown when an error occurs while serializing or de-serializing objects.
    /// </summary>
    #if !NET8_0_OR_GREATER
    [Serializable]
#endif
    public class SerializationException : HazelcastException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationException"/> class.
        /// </summary>
        public SerializationException()
            : base(ExceptionMessages.SerializationException)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public SerializationException(string message)
            : base(message)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationException"/> class with a reference to
        /// the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null
        /// reference if no inner exception is specified.</param>
        public SerializationException(Exception innerException)
            : base(ExceptionMessages.SerializationException, innerException)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null
        /// reference if no inner exception is specified.</param>
        public SerializationException(string message, Exception innerException)
            : base(message, innerException)
        { }
#if !NET8_0_OR_GREATER
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data
        /// about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        [Obsolete("This constructor is obsolete due to BinaryFormatter being obsolete. Use the constructor without this parameter.")]
        protected SerializationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
#endif
    }
}
