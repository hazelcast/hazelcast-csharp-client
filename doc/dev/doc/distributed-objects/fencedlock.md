# FencedLock

> [!NOTE]
> IFencedLock is a member of CP Subsystem API. For detailed information, see the [CP SubSystem documentation](../cpsubsystem.md).
>
> The original [FencedLock documentation](https://docs.hazelcast.com/hazelcast/latest/data-structures/fencedlock) may help get a
> better understanding of the .NET IFencedLock implementation.

Hazelcast @Hazelcast.CP.IFencedLock is a linearizable and distributed implementation of `java.util.concurrent.locks.Lock`, meaning that if you lock using a FencedLock, the critical section that it guards is guaranteed to be executed by only one thread in the entire cluster. Even though locks are great for synchronization, they can lead to problems if not used properly. Also note that Hazelcast Lock does not support fairness.
Since @Hazelcast.CP.IFencedLock is a distributed implementation, these operations involve remote calls and thus their performances differ from local, in-memory, references.

IFencedLock is CP with respect to the CAP principle. It works on top of the Raft consensus algorithm. It offers 
linearizability during crash-stop failures and network partitions. If a network partition occurs, it remains 
available on at most one side of the partition.

By default, IFencedLock is reentrant. Once a caller acquires the lock, it can acquire the lock reentrantly as many
times as it wants in a linearizable manner. You can configure the reentrancy behavior via the cluster configuration.
For instance, reentrancy can be disabled and FencedLock can work as a non-reentrant mutex. You can also set a 
custom reentrancy limit. When the reentrancy limit is already reached, IFencedLock does not block a lock call. 
Instead, it fails with an exception or a specified return value.

Distributed locks are unfortunately not equivalent to single-node mutexes because of the complexities in distributed 
systems, such as uncertain communication patterns, and independent and partial failures. In an asynchronous network, 
no lock service can guarantee mutual exclusion, because there is no way to distinguish between a slow and a crashed 
process. This can be mitigated with *fences* (see [CP SubSystem FencedLock documentation](https://docs.hazelcast.com/hazelcast/latest/data-structures/fencedlock)
for details): lock holders are ordered by a monotonic fencing token, which increments each time the lock is assigned 
to a new owner. This fencing token can be passed to external services or resources to ensure sequential execution of 
the side effects performed by lock holders.

## Lock Context

The original Java FencedLock is thread-based, much like the .NET `lock` statement is. In the distributed world, this
means that the *context* of a lock ownership is a unique thread, for a unique client connection. In other words, the
concept of *thread* is extended to the entire distributed system, and the lock context is this thread.

This however does not work well with .NET asynchronous programming model, just as the `lock` statement does not either.
Indeed, the following code is illegal (and would not compile) because the asynchronous `await` could cause the execution
flow to continue on any thread.

```csharp
lock (mutex) // locks acquired by the current thread
{
    // this is still the current thread
    await DoSomething();
    // this can be any thread!
}
```

In C# programming, developers know that they cannot mix the `lock` statement with asynchronous programming, and 
typically use an explicit structure such as a semaphore, which becomes the lock context:

```csharp
await semaphore.WaitAsync();
try
{
    await DoSomething();
}
finally 
{
    semaphore.Release();
}
```

Hazelcast FencedLock provides, and requires, an explicit lock context object. Every FencedLock operation
executes within that context, which needs to be passed around in code. The code then becomes:

```csharp
var lockContext = new LockContext();
await fencedLock.LockAsync(lockContext);
try
{
    await DoSomething();
}
finally 
{
    await fencedLock.UnlockAsync(lockContext);
}
```

## Example

The following simple example creates and uses a IFencedLock:

```csharp
var cancellationSource = new CancellationTokenSource();
var cancellationToken = cancellationSource.Token;

await using var client = await HazelcastClientFactory.StartNewClientAsync(options);
await using var fencedLock = await client.CPSubsystem.GetLockAsync("lock-name");

var lockContext = new LockContext();

await fencedLock.LockAsync(lockContext); // acquires the lock for lockContext (count = 1)
await fencedLock.LockAsync(lockContext); // re-enters the lock for lockContext (count = 2)

var otherContext = new LockContext();
var task = Task.Run(async () => {
    // acquires the lock for otherContext
    // blocks as long as the lock is owned by lockContext
    await fencedLock.LockAsync(otherContext);
});

await fencedLock.UnlockAsync(lockContext); // exits the lock for lockContext (count = 1)
await fencedLock.UnlockAsync(lockContext); // releases the lock for lockContext

await task; // completes now that lockContext does not own the lock anymore
await fencedLock.UnlockAsync(otherContext); // releases the lock for otherContext

await fencedLock.DestroyAsync();
```

## Notes

Locks are fail-safe. If a member holds a lock and some other members go down, the cluster will keep your 
locks safe and available. Moreover, when a member leaves the cluster, all the locks acquired by that dead 
member will be removed so that those locks are immediately available for live members.

Locks are not automatically removed. If a lock is not used anymore, Hazelcast does not automatically 
perform garbage collection in the lock. This can lead to an OutOfMemoryError. If you create locks on 
the fly, make sure they are destroyed.

Locks are re-entrant. The same context can lock multiple times on the same lock. Note that for other 
contexts to be able to require this lock, the owner of the lock must call unlock as many times as the 
owner called lock.

Refer to the [FencedLock documentation](https://docs.hazelcast.com/hazelcast/latest/data-structures/fencedlock) for a
> better understanding of FencedLock, fencing, etc.