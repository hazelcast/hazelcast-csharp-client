using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects.Implementation.Executor
{
    // tasks run on servers, not on the client
    // there must be an equivalent counterpart on the server

    // simple basic "echo" task:
    /*
    public class Echo implements Callable<String>, Serializable, HazelcastInstanceAware
    {
        String input = null;

        private transient HazelcastInstance hazelcastInstance;

        public Echo()
        { }

        public void setHazelcastInstance(HazelcastInstance hazelcastInstance)
        {
            this.hazelcastInstance = hazelcastInstance;
        }

        public Echo(String input)
        {
            this.input = input;
        }

        public String call()
        {
            return hazelcastInstance.getCluster().getLocalMember().toString() + ":" + input;
        }
    }
    */

    // Future<string> future = executor.submit(new Echo("myInput"));
    // String result = future.get();
    //
    // the documentation only mentions submit, not execute - is it deprecated-ish?

    // now, a future can be cancelled: future.cancel(bool evenIfAlreadyRunning)

    // callback - invokes onResponse or onFailure = completing a Task, really
    // question: can we cancel an entry processor's job? what happens?

    // TODO: this is *not* implemented in our current code ?!



    internal class Executor : DistributedObjectBase, IExecutor
    {
        // uh refactor this ctor
        public Executor(string serviceName, string name, Cluster cluster, ISerializationService serializationService, ILoggerFactory loggerFactory)
            : base(serviceName, name, cluster, serializationService, loggerFactory)
        { }

        public Task ExecuteOnAllAsync(IExecutable executable, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Task<(Guid MemberId, TResult Result)>> ExecuteOnAllAsync<TResult>(IExecutable<TResult> executable, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task ExecuteAsync(IExecutable executable, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();

            // doh - now we'd need to send 'packets' which are not supported now
            //var executableData = ToSafeData(executable);
            //var requestMessage =
        }

        public Task<TResult> ExecuteAsync<TResult>(IExecutable<TResult> executable, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task ExecuteAsync(IExecutable executable, object key, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<TResult> ExecuteAsync<TResult>(IExecutable<TResult> executable, object key, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task ExecuteAsync(IExecutable executable, Guid memberId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<TResult> ExecuteAsync<TResult>(IExecutable<TResult> executable, Guid memberId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task ExecuteAsync(IExecutable executable, IEnumerable<Guid> memberIds, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Task<(Guid MemberId, TResult Result)>> ExecuteAsync<TResult>(IExecutable<TResult> executable, IEnumerable<Guid> memberIds, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
