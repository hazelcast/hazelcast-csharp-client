# AtomicLong

> [!NOTE]
> IAtomicLong is a member of CP Subsystem API. For detailed information, see the [CP SubSystem documentation](../cpsubsystem.md).

Hazelcast @Hazelcast.CP.IAtomicLong is the distributed implementation of `java.util.concurrent.atomic.AtomicLong` and offers most of its operations such as Get, Set, GetAndSet, CompareAndSet and IncrementAndGet. You can also think of it as implementing most of @System.Interlocked methods for `long` (`System.Int64`) distributed values. Since @Hazelcast.CP.IAtomicLong is a distributed implementation, these operations involve remote calls and thus their performances differ from local, in-memory, atomic longs.

The following example code creates an instance, increments it by a million and prints the count.

```csharp
await using var client = await HazelcastClientFactory.StartNewClientAsync();
await using var counter = await client.CPSubSystem.GetAtomicLongAsync("counter-unique-name");

for (int i = 0; i < 1000 * 1000; i++ )
{
    if (i % 500000 == 0)
        Console.WriteLine($"At: {i}");

    await counter.IncrementAndGetAsync();
}
Console.WriteLine($"Count is {await counter.GetAsync()}");
```

When you start other instances with the code above, you will see the count as member count times a million.

Note that sending functions to, and executing functions on, AtomicLong as documented for the Java client (see [this page](https://docs.hazelcast.com/hazelcast/latest/data-structures/iatomiclong)) are not supported by the C# client.