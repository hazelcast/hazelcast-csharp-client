// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;

namespace Hazelcast.Exceptions
{
    /// <summary>
    /// Represents the exception that is throw when an async task times out.
    /// </summary>
    [Serializable] // CA2237
    public sealed class TaskTimeoutException : HazelcastException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskTimeoutException"/> class with a specified error message and the task that timed out.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="task">The task that timed out.</param>
        public TaskTimeoutException(string message, Task task)
            : base(string.IsNullOrWhiteSpace(message) ? ExceptionMessages.Timeout : message)
        {
            Task = task ?? throw new ArgumentNullException(nameof(task));
        }

        /// <summary>
        /// Gets the task that timed out (and may still be executing).
        /// </summary>
        public Task Task { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskTimeoutException"/>.
        /// </summary>
        /// <remarks>
        /// <para>This constructor method is provided to comply with CA1032 and ensure that the
        /// exception class is a good .NET citizen. It is not meant to be used in code.</para>
        /// </remarks>
        public TaskTimeoutException()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskTimeoutException"/> with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <remarks>
        /// <para>This constructor method is provided to comply with CA1032 and ensure that the
        /// exception class is a good .NET citizen. It is not meant to be used in code.</para>
        /// </remarks>
        public TaskTimeoutException(string message) 
            : base(message)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskTimeoutException"/> with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null
        /// reference if no inner exception is specified.</param>
        /// <remarks>
        /// <para>This constructor method is provided to comply with CA1032 and ensure that the
        /// exception class is a good .NET citizen. It is not meant to be used in code.</para>
        /// </remarks>
        public TaskTimeoutException(string message, Exception innerException) 
            : base(message, innerException)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data
        /// about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        /// <remarks>
        /// <para>This constructor method is provided to comply with CA2229 and ensure that the
        /// exception class is a good .NET citizen. It is not meant to be used in code.</para>
        /// </remarks>
        private TaskTimeoutException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
