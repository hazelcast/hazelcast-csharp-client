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
    /// Represents the exception that is thrown when invalid partition group is set.
    /// </summary>
    public sealed class InvalidPartitionGroupException : HazelcastException
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidPartitionGroupException"/> class.
        /// </summary>
        public InvalidPartitionGroupException()
        { }
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidPartitionGroupException"/> class with a specified error message.
        /// </summary>
        /// <param name="message"></param>
        public InvalidPartitionGroupException(string message) : base(message)
        { }
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidPartitionGroupException"/> class with a reference to
        /// </summary>
        /// <param name="innerException"></param>
        public InvalidPartitionGroupException(Exception innerException) : base(innerException)
        { }
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidPartitionGroupException"/> class with a specified error message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public InvalidPartitionGroupException(string message, Exception innerException) : base(message, innerException)
        { }
    }
}
