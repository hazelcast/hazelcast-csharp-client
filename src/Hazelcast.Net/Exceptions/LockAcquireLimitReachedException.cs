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

namespace Hazelcast.Exceptions
{
    /// <summary>
    /// Represents the exception that is thrown when the aquisition count limit on the lock is exceeded.
    /// </summary>
    internal class LockAcquireLimitReachedException : HazelcastException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LockAcquireLimitReachedException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public LockAcquireLimitReachedException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LockAcquireLimitReachedException"/> class with specific message and inner exception.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public LockAcquireLimitReachedException(string message, Exception innerException) : base(message, innerException)
        {
        }



        /// <summary>
        /// Initializes a new instance of the <see cref="LockAcquireLimitReachedException"/> class.
        /// </summary>
        public LockAcquireLimitReachedException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LockAcquireLimitReachedException"/> class with a reference to
        /// the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null
        /// reference if no inner exception is specified.</param>
        public LockAcquireLimitReachedException(Exception innerException) : base(innerException)
        {
        }
    }
}
