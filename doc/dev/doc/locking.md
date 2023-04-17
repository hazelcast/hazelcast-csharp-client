# Locking

On the server (member) side, Hazelcast uses a unique number to identify the owner of locks, and historically that number has always been the thread unique identifier. As a consequence, the locking model in previous versions of the Hazelcast client closely match the thread-based model that .NET provides with, for instance, the `lock` statement.

Due to the systematic usage of asynchronous patterns, the code for one operation can be executed by many different threads (basically, each time an operation is put on hold by an `await` statement, it can resume its execution on any other thread). Therefore, using the actual thread identifier as a "lock owner" identifier is not possible anymore.

The Hazelcast .NET client introduces different ways to manage the context of a lock in an asynchronous pattern.

## Implicit Context

For things such as maps, that would implicitly rely on the thread identifier to identify their lock context, the Hazelcast .NET client implicitly manages a "lock context".
This allows the API to remain similar enough to the version 3 API.
The lock context is represented by an `AsyncContext` instance.
This is a class which relies upon the .NET built-in `AsyncLocal<T>` type to maintain values that flow with the asynchronous operation, i.e. are transferred to the new thread when an operation resumes after awaiting.
Therefore, when an operation acquires a lock, it owns the lock until it releases it, no matter what thread executes the operation.
The `AsyncContext` uses a sequential number to ensure the uniqueness of the identifier.

In order to execute work in a new context (which would correspond to executing work on a different thread for previous versions), one has to use a new context:

```csharp
// executes in the same, current context
await DoSomethingAsync(...);

using (AsyncContext.New())
{
    // executes in a new context
    await DoSomethingAsync(...);
}
```

Due to the way `AsyncLocal<T>` variables work, *any task* started from within the `using` block executes in the new context, even if it continues to execute after the `using` block has exited:

```csharp
Task task;

using (AsyncContext.New())
{
    // starts in a new context
    var task = DoSomethingAsync(...);
}

// the entire task executes in the new context
await task;
```

Essentially, when the `using` block is exited, the previous `AsyncContext` is restored, but the new one that was created remains attached to the tasks that were started.

## Explicit Context

On the other hand, [fenced locks](distributed-objects/fencedlock.md), which are part of the [CP subsystem](cpsubsystem.md), rely on an explicit object to represent the lock context. Every fenced lock operation requires a context, and operate on that context. Therefore, acquiring a lock is performed as:
```csharp
var context = new LockContext();
var lock = await client.CPSubSystem.GetLockAsync(lockName);
await lock.LockAsync(context);
// ...
await lock.UnlockAsync(context);
```

The lock context is re-entrant: it is possible to lock several times *for the same context* as long as the lock is unlocked the same number of times. On the other hand, trying to lock for any other context object would block.

It is up to the application code to manage the lock context appropriately.