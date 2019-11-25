// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.IO;

 namespace Hazelcast.Client.Spi
{
    /// <summary>
    /// Marker interface for exceptions to indicate that an operation can be retried. 
    /// E.g. a map.get sent to a machine where the partition has just moved to another machine.
    /// </summary>
    [Serializable]
    class RetryableHazelcastException : HazelcastException
    {
        /// <inheritdoc />
        public RetryableHazelcastException()
        {
        }

        /// <inheritdoc />
        public RetryableHazelcastException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// A <see cref="RetryableHazelcastException"/> that indicates operation is sent to a machine that isn't member of the cluster
    /// </summary>
    [Serializable]
    class TargetNotMemberException : RetryableHazelcastException
    {
        /// <inheritdoc />
        public TargetNotMemberException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Thrown when a client invocation is failed because related target is disconnected, and
    /// whether the invocation runs or not is indeterminate
    /// </summary>
    [Serializable]
    class TargetDisconnectedException : RetryableHazelcastException
    {
        /// <summary>
        /// Constructor version with an Address instance to assign to
        /// </summary>
        /// <param name="address">is an Adress instance to assign to</param>
        public TargetDisconnectedException(Address address) : base("Target[" + address + "] disconnected.")
        {
        }

        /// <summary>
        /// Constructor version with an Address instance and message to assign to
        /// </summary>
        /// <param name="address">is an Adress instance to assign to</param>
        /// <param name="message">is a message to assign to</param>
        public TargetDisconnectedException(Address address, string message)
            : base("Target[" + address + "] disconnected, " + message)
        {
        }

        /// <inheritdoc />
        public TargetDisconnectedException(string msg) : base(msg)
        {
        }
    }
}