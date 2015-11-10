// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;
using Hazelcast.Util;

namespace Hazelcast.Core
{
    /// <summary>
    ///     Distributed implementation of ExecutorService.
    ///     IExecutorService provides additional methods like executing tasks
    ///     on a specific member, on a member who is owner of a specific key,
    ///     executing a tasks on multiple members and listening execution result using a callback.
    /// </summary>
    /// <seealso cref="IExecutionCallback{T}" />
    /// <seealso cref="IMultiExecutionCallback" />
    public interface IExecutorService : IDistributedObject
    {
        /// <summary>Executes task on all of known cluster members</summary>
        /// <param name="command">task</param>
        void ExecuteOnAllMembers(Runnable command);

        /// <summary>Executes task on owner of the specified key</summary>
        /// <param name="command">task</param>
        /// <param name="key">key</param>
        void ExecuteOnKeyOwner(Runnable command, object key);

        /// <summary>Executes task on the specified member</summary>
        /// <param name="command">task</param>
        /// <param name="member">member</param>
        void ExecuteOnMember(Runnable command, IMember member);

        /// <summary>Executes task on each of the specified members</summary>
        /// <param name="command">task</param>
        /// <param name="members">members</param>
        void ExecuteOnMembers(Runnable command, ICollection<IMember> members);

        /// <summary>Submits task to a random member.</summary>
        /// <remarks>
        ///     Submits task to a random member. Caller will be notified for the result of the task by
        ///     <see cref="IExecutionCallback{T}.OnResponse(T)" />
        ///     or
        ///     <see cref="IExecutionCallback{T}.OnFailure(System.Exception)" />
        ///     .
        /// </remarks>
        /// <param name="task">task</param>
        /// <param name="callback">callback</param>
        void Submit<T>(Runnable task, IExecutionCallback<T> callback);

        /// <summary>Submits task to a random member.</summary>
        /// <remarks>
        ///     Submits task to a random member. Caller will be notified for the result of the task by
        ///     <see cref="IExecutionCallback{T}.OnResponse(T)" />
        ///     or
        ///     <see cref="IExecutionCallback{T}.OnFailure(System.Exception)" />
        ///     .
        /// </remarks>
        /// <param name="task">task</param>
        /// <param name="callback">callback</param>
        void Submit<T>(Callable<T> task, IExecutionCallback<T> callback);

        /// <summary>
        ///     Submits task to all cluster members and returns
        ///     map of IMember-Future pairs representing pending completion of the task on each member
        /// </summary>
        /// <param name="task">task</param>
        /// <returns>map of IMember-Future pairs representing pending completion of the task on each member</returns>
        IDictionary<IMember, Task<T>> SubmitToAllMembers<T>(Callable<T> task);

        /// <summary>Submits task to the all cluster members.</summary>
        /// <remarks>
        ///     Submits task to the all cluster members. Caller will be notified for the result of the each task by
        ///     <see cref="IMultiExecutionCallback.OnResponse(IMember, object)"  />
        ///     , and when all tasks are completed,
        ///     <see cref="IMultiExecutionCallback.OnComplete(System.Collections.Generic.IDictionary{IMember, object})" />
        ///     will be called.
        /// </remarks>
        /// <param name="task">task</param>
        /// <param name="callback">callback</param>
        void SubmitToAllMembers(Runnable task, IMultiExecutionCallback callback);

        /// <summary>Submits task to the all cluster members.</summary>
        /// <remarks>
        ///     Submits task to the all cluster members. Caller will be notified for the result of the each task by
        ///     <see cref="IMultiExecutionCallback.OnResponse(IMember, object)" />
        ///     , and when all tasks are completed,
        ///     <see cref="IMultiExecutionCallback.OnComplete(System.Collections.Generic.IDictionary{IMember, object})" />
        ///     will be called.
        /// </remarks>
        /// <param name="task">task</param>
        /// <param name="callback">callback</param>
        void SubmitToAllMembers<T>(Callable<T> task, IMultiExecutionCallback callback);

        /// <summary>
        ///     Submits task to owner of the specified key and returns a Future
        ///     representing that task.
        /// </summary>
        /// <remarks>
        ///     Submits task to owner of the specified key and returns a Future
        ///     representing that task.
        /// </remarks>
        /// <param name="task">task</param>
        /// <param name="key">key</param>
        /// <returns>a Future representing pending completion of the task</returns>
        Task<T> SubmitToKeyOwner<T>(Callable<T> task, object key);

        /// <summary>Submits task to owner of the specified key.</summary>
        /// <remarks>
        ///     Submits task to owner of the specified key. Caller will be notified for the result of the task by
        ///     <see cref="IExecutionCallback{T}.OnResponse(T)" />
        ///     or
        ///     <see cref="IExecutionCallback{T}.OnFailure(System.Exception)" />
        ///     .
        /// </remarks>
        /// <param name="task">task</param>
        /// <param name="key">the specified key</param>
        /// <param name="callback">callback</param>
        void SubmitToKeyOwner<T>(Runnable task, object key, IExecutionCallback<T> callback);

        /// <summary>Submits task to owner of the specified key.</summary>
        /// <remarks>
        ///     Submits task to owner of the specified key. Caller will be notified for the result of the task by
        ///     <see cref="IExecutionCallback{T}.OnResponse(T)"/>
        ///     or
        ///     <see cref="IExecutionCallback{T}.OnFailure(System.Exception)" />
        ///     .
        /// </remarks>
        /// <param name="task">task</param>
        /// <param name="key">key to execute on</param>
        /// <param name="callback">callback</param>
        void SubmitToKeyOwner<T>(Callable<T> task, object key, IExecutionCallback<T> callback);

        /// <summary>
        ///     Submits task to specified member and returns a Future
        ///     representing that task.
        /// </summary>
        /// <remarks>
        ///     Submits task to specified member and returns a Future
        ///     representing that task.
        /// </remarks>
        /// <param name="task">task</param>
        /// <param name="member">member</param>
        /// <returns>a Future representing pending completion of the task</returns>
        Task<T> SubmitToMember<T>(Callable<T> task, IMember member);

        /// <summary>Submits task to the specified member.</summary>
        /// <remarks>
        ///     Submits task to the specified member. Caller will be notified for the result of the task by
        ///     <see cref="IExecutionCallback{T}.OnResponse(T)" />
        ///     or
        ///     <see cref="IExecutionCallback{T}.OnFailure(System.Exception)" />
        ///     .
        /// </remarks>
        /// <param name="task">task</param>
        /// <param name="member">the specified member</param>
        /// <param name="callback">callback</param>
        void SubmitToMember<T>(Runnable task, IMember member, IExecutionCallback<T> callback);

        /// <summary>Submits task to the specified member.</summary>
        /// <remarks>
        ///     Submits task to the specified member. Caller will be notified for the result of the task by
        ///     <see cref="IExecutionCallback{T}.OnResponse(T)" />
        ///     or
        ///     <see cref="IExecutionCallback{T}.OnFailure(System.Exception)" />
        ///     .
        /// </remarks>
        /// <param name="task">task</param>
        /// <param name="member">Member to execute tasks on</param>
        /// <param name="callback">callback</param>
        void SubmitToMember<T>(Callable<T> task, IMember member, IExecutionCallback<T> callback);

        /// <summary>
        ///     Submits task to given members and returns
        ///     map of IMember-Future pairs representing pending completion of the task on each member
        /// </summary>
        /// <param name="task">task</param>
        /// <param name="members">members</param>
        /// <returns>map of IMember-Future pairs representing pending completion of the task on each member</returns>
        IDictionary<IMember, Task<T>> SubmitToMembers<T>(Callable<T> task, ICollection<IMember> members);

        /// <summary>Submits task to the specified members.</summary>
        /// <remarks>
        ///     Submits task to the specified members. Caller will be notified for the result of the each task by
        ///     <see cref="IMultiExecutionCallback.OnResponse(IMember, object)"/>
        ///     , and when all tasks are completed,
        ///     <see cref="IMultiExecutionCallback.OnComplete(System.Collections.Generic.IDictionary{IMember, object})" />
        ///     will be called.
        /// </remarks>
        /// <param name="task">task</param>
        /// <param name="members">the specified members</param>
        /// <param name="callback">callback</param>
        void SubmitToMembers(Runnable task, ICollection<IMember> members, IMultiExecutionCallback callback);

        /// <summary>Submits task to the specified members.</summary>
        /// <remarks>
        ///     Submits task to the specified members. Caller will be notified for the result of the each task by
        ///     <see cref="IMultiExecutionCallback.OnResponse(IMember, object)">IMultiExecutionCallback.OnResponse(IMember, object)</see>
        ///     , and when all tasks are completed,
        ///     <see cref="IMultiExecutionCallback.OnComplete(System.Collections.Generic.IDictionary{IMember, object})">
        ///         IMultiExecutionCallback.OnComplete(System.Collections.Generic.IDictionary
        ///         &lt;IMember, object&gt;)
        ///     </see>
        ///     will be called.
        /// </remarks>
        /// <param name="task">task</param>
        /// <param name="members">List of members to execute on</param>
        /// <param name="callback">callback</param>
        void SubmitToMembers<T>(Callable<T> task, ICollection<IMember> members, IMultiExecutionCallback callback);
    }
}