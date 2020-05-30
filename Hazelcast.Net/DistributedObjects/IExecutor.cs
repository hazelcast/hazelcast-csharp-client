using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Data;
using Hazelcast.DistributedObjects.Implementation;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

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
    public interface IExecutor
    {
        // TODO: for anything returning Task<TResult> could we want Task<(Guid MemberId, TResult Result) instead?

        // the original code, when it returns a 'future', can cancel the execution
        // future.cancel(bool evenIfAlreadyRunning)
        // here, we have CancellationToken on all methods
        // (should we have timeouts too?)

        Task ExecuteOnAllAsync(IExecutable executable, CancellationToken cancellationToken);
        IEnumerable<Task<(Guid MemberId, TResult Result)>> ExecuteOnAllAsync<TResult>(IExecutable<TResult> executable, CancellationToken cancellationToken);

        Task ExecuteAsync(IExecutable executable, CancellationToken cancellationToken); // on random member
        Task<TResult> ExecuteAsync<TResult>(IExecutable<TResult> executable, CancellationToken cancellationToken); // on random member

        Task ExecuteAsync(IExecutable executable, object key, CancellationToken cancellationToken); // on key owner
        Task<TResult> ExecuteAsync<TResult>(IExecutable<TResult> executable, object key, CancellationToken cancellationToken); // on key owner

        Task ExecuteAsync(IExecutable executable, Guid memberId, CancellationToken cancellationToken); // on member
        Task<TResult> ExecuteAsync<TResult>(IExecutable<TResult> executable, Guid memberId, CancellationToken cancellationToken); // on member

        Task ExecuteAsync(IExecutable executable, IEnumerable<Guid> memberIds, CancellationToken cancellationToken); // on members
        IEnumerable<Task<(Guid MemberId, TResult Result)>> ExecuteAsync<TResult>(IExecutable<TResult> executable, IEnumerable<Guid> memberIds, CancellationToken cancellationToken); // on members
    }
}
