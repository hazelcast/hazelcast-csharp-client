// Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Util;

namespace Hazelcast.Core
{
    /// <summary>
    ///     IExecutionCallback allows to asynchronously get notified when the execution is completed,
    ///     either successfully or with error.
    /// </summary>
    /// <remarks>
    ///     IExecutionCallback allows to asynchronously get notified when the execution is completed,
    ///     either successfully or with error.
    /// </remarks>
    /// <seealso cref="IExecutorService.Submit{T}(Callable{T}, IExecutionCallback{T})" />
    /// <seealso cref="IExecutorService.SubmitToMember{T}(Callable{T}, IMember, IExecutionCallback{T})" />
    /// <seealso cref="IExecutorService.SubmitToKeyOwner{T}(Callable{T}, object, IExecutionCallback{T})" />
    public interface IExecutionCallback<T>
    {
        /// <summary>Called when an execution is completed with an error.</summary>
        /// <remarks>Called when an execution is completed with an error.</remarks>
        /// <param name="t">exception thrown</param>
        void OnFailure(Exception t);

        /// <summary>Called when an execution is completed successfully.</summary>
        /// <remarks>Called when an execution is completed successfully.</remarks>
        /// <param name="response">result of execution</param>
        void OnResponse(T response);
    }
}