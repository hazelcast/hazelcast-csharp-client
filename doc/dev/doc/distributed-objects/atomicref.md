# AtomicReference

> [!NOTE]
> IAtomicReference is a member of CP Subsystem API. For detailed information, see the [CP SubSystem documentation](../cpsubsystem.md).

Hazelcast @Hazelcast.CP.IAtomicReference is the distributed implementation of `java.util.concurrent.atomic.AtomicReference` and offers most of its operations such as Get, Set, GetAndSet and CompareAndSet. You can also think of GetAndSet, CompareAndSet as @System.Interlocked Exchange and CompareExchange methods for distributed values. Since @Hazelcast.CP.IAtomicReference is a distributed implementation, these operations involve remote calls and thus their performances differ from local, in-memory, references.

The following example code adds a dot the end of any string added to the shared reference:

```csharp
var cancellationSource = new CancellationTokenSource();
var cancellationToken = cancellationSource.Token;

await using var client = await HazelcastClientFactory.StartNewClientAsync(options);
await using var sentence = await client.CPSubsystem.GetAtomicReferenceAsync<string>("sentence-unique-key");

while (!cancellationToken.IsCancellationRequested)
{
    var value = await sentence.GetAsync();

    if (value != null && !value.EndsWith("."))
    {
        var newValue = value + ".";

        if (!await sentence.CompareAndSetAsync(value, newValue))
            continue;
    }

    await Task.Delay(100, cancellationToken);
}

await sentence.DestroyAsync();
```

Note that sending functions to, and executing functions on, `AtomicReference` as documented for the Java client (see [this page](https://docs.hazelcast.com/hazelcast/latest/data-structures/iatomicreference)) are not supported by the C# client.