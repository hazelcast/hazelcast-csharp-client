// Copyright (c) 2008, Hazelcast, Inc. All Rights Reserved.
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

using System.Collections.Generic;

namespace Hazelcast.Core
{
    /// <summary>
    ///     IMultiExecutionCallback allows to get notified when an execution is completed on each member
    ///     which task is submitted to.
    /// </summary>
    /// <remarks>
    ///     IMultiExecutionCallback allows to get notified when an execution is completed on each member
    ///     which task is submitted to. After all executions are completed on all submitted members,
    ///     <see cref="OnComplete(System.Collections.Generic.IDictionary{IMember, object})" />
    ///     method is called with map of all results.
    /// </remarks>
    /// <seealso cref="IExecutorService" />
    /// <seealso cref="IExecutionCallback{T}" />
    public interface IMultiExecutionCallback
    {
        /// <summary>Called after all executions are completed.</summary>
        /// <remarks>Called after all executions are completed.</remarks>
        /// <param name="values">map of IMember-Response pairs</param>
        void OnComplete(IDictionary<IMember, object> values);

        /// <summary>Called when an execution is completed on a member.</summary>
        /// <remarks>Called when an execution is completed on a member.</remarks>
        /// <param name="member">member which task is submitted to.</param>
        /// <param name="value">result of execution</param>
        void OnResponse(IMember member, object value);
    }
}