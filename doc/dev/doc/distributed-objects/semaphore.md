# Semaphore

> [!NOTE]
> ISemaphore is a member of CP Subsystem API. For detailed information, see the [CP SubSystem documentation](../cpsubsystem.md).
>
> The original Java [Semaphore documentation](https://docs.hazelcast.com/imdg/latest/data-structures/isemaphore) may help get a
> better understanding of the .NET ISemaphore implementation.

Hazelcast @Hazelcast.CP.ISemaphore is a distributed implementation of `java.util.concurrent.Semaphore`.

Semaphores offer *permits* to control the execution when performing concurrent activities. To execute a concurrent activity,
a code flow acquires a permits (or waits for a permit to become available). When the execution is completed, the permit is
released.

When a permit is acquired, the number of available permits is decreased. When the permit is released, the count is increased.

See also the [Semaphore Configuration Section](https://docs.hazelcast.com/imdg/latest/cp-subsystem/configuration#semaphore-configuration)
for details.

## Async Context

The original Java Semaphore is thread-based, much like the .NET `lock` statement is. In the distributed world, this
means that the *context* of a semaphore ownership is a unique thread, for a unique client connection. In other words, the
concept of *thread* is extended to the entire distributed system, and the semaphore context is this thread.

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

Hazelcast FencedLock provides, and requires, an implicit `AsyncContext` object. Every Semaphore operation
executes within such a context. See the [Locking](../locking.md) documentation section for more infos about
the `AsyncContext`, and how and when to use it.

## Example

The following simple example creates and uses a ISemaphore:

```csharp
var cancellationSource = new CancellationTokenSource();
var cancellationToken = cancellationSource.Token;

await using var client = await HazelcastClientFactory.StartNewClientAsync(options);
await using var semaphore = await client.CPSubsystem.GetSemaphoreAsync("semaphore-name");

await semahore.InitializeAsync(12); // initializes with 12 permits
await semaphore.AcquireAsync(2); // acquires 2 permits
await semaphore.ReleaseAsync(); // release 1 permit
await semaphore.ReleaseAsync(); // release 1 permit
```

