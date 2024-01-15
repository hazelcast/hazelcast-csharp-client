# AtomicLong

> [!NOTE]
> ICountDownLatch is a member of CP Subsystem API. For detailed information, see the [CP SubSystem documentation](../cpsubsystem.md).

Hazelcast @Hazelcast.CP.ICountDownLatch is the distributed implementation of `java.util.concurrent.CountDownLatch`. It is a
cluster-wide synchronization aid that allows one or more threads to wait until a set of operations being performed in other threads completes.

The following example code creates a latch, and waits on it:

```csharp
await using var client = await HazelcastClientFactory.StartNewClientAsync();
await using var latch = await client.CPSubSystem.GetCountDownLatchAsync("latch-unique-name");

await latch.TrySetCountAsync(4);

var waiting = latch.AwaitAsync(TimeSpan.FromSeconds(30));
// waiting is NOT completed
latch.CountDownAsync();
// latch.GetCountAsync() would be 3
latch.CountDownAsync();
// latch.GetCountAsync() would be 2
latch.CountDownAsync();
// latch.GetCountAsync() would be 1
latch.CountDownAsync();
// latch.GetCountAsync() is now zero

await waiting; // waiting is completed
```
