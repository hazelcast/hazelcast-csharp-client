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
using System.Collections.Generic;
using System.Text;

namespace Hazelcast.Exceptions
{
    internal class LockAcquireLimitReachedException : HazelcastException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LockAcquireLimitReachedException"/> class with specific message.
        /// </summary>
        /// <param name="message"></param>
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
    }
}
