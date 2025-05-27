// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
    /// Represents exception that the ownership of the lock which is held has lost.
    /// </summary>
    internal class LockOwnershipLostException : HazelcastException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LockOwnershipLostException"/> class.
        /// </summary>
        /// <param name="message"></param>
        public LockOwnershipLostException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LockOwnershipLostException"/> class.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public LockOwnershipLostException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LockOwnershipLostException"/> class.
        /// </summary>
        public LockOwnershipLostException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LockOwnershipLostException"/> class with a reference to
        /// the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null
        /// reference if no inner exception is specified.</param>
        public LockOwnershipLostException(Exception innerException) : base(innerException)
        {
        }
    }
}
