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

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Represents the result of a distributed operation.
    /// </summary>
    /// <typeparam name="TResult">The type of the result of the operation.</typeparam>
#pragma warning disable CA1815 // Override equals and operator equals on value types - not meant to be compared
    public readonly struct ExecutionResult<TResult>
#pragma warning restore CA1815
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionResult{TResult}"/> struct.
        /// </summary>
        /// <param name="memberId">The unique identifier of the member which performed the operation.</param>
        /// <param name="result">The result of the operation.</param>
        public ExecutionResult(Guid memberId, TResult result)
        {
            MemberId = memberId;
            Result = result;
        }

        /// <summary>
        /// Gets the unique identifier of the member which performed the operation.
        /// </summary>
        public Guid MemberId { get; }

        /// <summary>
        /// Gets the result of the operation.
        /// </summary>
        public TResult Result { get; }
    }
}