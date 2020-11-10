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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    ///     Distributed implementation of ExecutorService.
    ///     IExecutorService provides additional methods like executing tasks
    ///     on a specific member, on a member who is owner of a specific key,
    ///     executing a tasks on multiple members and listening execution result using a callback.
    /// </summary>
    /// <seealso cref="IExecutionCallback{T}" />
    /// <seealso cref="IMultiExecutionCallback" />
    internal interface IExecutor
    {
        // TODO: for anything returning Task<TResult> could we want Task<ExecutionResult<TResult> instead?

        // the original code, when it returns a 'future', can cancel the execution
        // future.cancel(bool evenIfAlreadyRunning)
        // here, we have CancellationToken on all methods
        // (should we have timeouts too?)

        Task ExecuteOnAllAsync(IExecutable executable, CancellationToken cancellationToken);
        IEnumerable<Task<ExecutionResult<TResult>>> ExecuteOnAllAsync<TResult>(IExecutable<TResult> executable, CancellationToken cancellationToken);

        Task ExecuteAsync(IExecutable executable, CancellationToken cancellationToken); // on random member
        Task<TResult> ExecuteAsync<TResult>(IExecutable<TResult> executable, CancellationToken cancellationToken); // on random member

        Task ExecuteAsync(IExecutable executable, object key, CancellationToken cancellationToken); // on key owner
        Task<TResult> ExecuteAsync<TResult>(IExecutable<TResult> executable, object key, CancellationToken cancellationToken); // on key owner

        Task ExecuteAsync(IExecutable executable, Guid memberId, CancellationToken cancellationToken); // on member
        Task<TResult> ExecuteAsync<TResult>(IExecutable<TResult> executable, Guid memberId, CancellationToken cancellationToken); // on member

        Task ExecuteAsync(IExecutable executable, IEnumerable<Guid> memberIds, CancellationToken cancellationToken); // on members
        IEnumerable<Task<ExecutionResult<TResult>>> ExecuteAsync<TResult>(IExecutable<TResult> executable, IEnumerable<Guid> memberIds, CancellationToken cancellationToken); // on members
    }
}
