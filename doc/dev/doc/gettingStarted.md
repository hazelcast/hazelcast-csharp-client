# Getting Started

## Hazelcast client

The Hazelcast client is the entry point to all interactions with an Hazelcast cluster. A client is created by the static @Hazelcast.HazelcastClientFactory. After it has been used, it needs to be disposed in order to properly close all connections to servers, and release resources.

For example:

```csharp
var client = await HazelcastClientFactory.StartNewClientAsync();
// ... use the client ...
await client.DisposeAsync();
```

A client is a heavy enough, multi-threaded object. Although a factory can create several, independent clients, it is recommended to store and reuse the client instance, as much as possible.

Here, the client is configured by default, which means by configuration files and environment variables. For more control, the client can be initialized with an @Hazelcast.HazelcastOptions instance, which represents the complete set of options of the Hazelcast client. In fact, the above example is equivalent to:

```csharp
var options = HazelcastOptions.Build();
var client = await HazelcastClientFactory.StartNewClientAsync(options);
// ...
```


Refer to the [Configuration](configuration.md) page for details on the various ways to build an @Hazelcast.HazelcastOptions instance, including handling command-line parameters, and to the @Hazelcast.HazelcastOptions reference for a list of all the configurable elements.

## Logging

The Hazelcast .NET client uses the logging abstractions proposed by the [Microsoft.Extensions.Logging](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging) namespace. By default, the client supports the abstractions, but does not come with any actual implementation. This means that, by default, the client will not output any log information. To actually log, an implementation must be added to the project.

See the [Logging](logging.md) documentation for details.

## Distributed Objects

The client can be used to obtain *distributed objects* that are managed by the cluster. For instance, the cluster can manage @Hazelcast.DistributedObjects.IHMap`2 objects, which are an asynchronous equivalent of .NET @System.Collections.Generic.IDictionary`2. Each object is identified by a unique name, which is used to retrieve the object. Finally, distributed objects need to be disposed after usage, to ensure they release their resources.

For example:

```csharp
var map = await client.GetMapAsync<string, string>("map-name");
await map.SetAsync("key", "value");
var value = await map.GetAsync("key");
await map.DisposeAsync();
```

The @Hazelcast.IHazelcastClient.GetMapAsync* method returns the existing object with the specified name, or creates a new object with that name on the cluster. That object will continue to live on the cluster after the @Hazelcast.DistributedObjects.IHMap`2 has been disposed. In order to remove the object from the cluster, one must destroy the object.

For example:

```csharp
var map = await client.GetMapAsync<string, string>("dict-name");
await map.DestroyAsync();
```

or 

```csharp
var map = await client.GetMapAsync<string, string>("dict-name");
await client.DestroyAsync(map);
```

## Transactions

The client is responsible for creating transactions. Transactions by default follow the Microsoft's transaction pattern: they must be disposed, and commit or roll back depending on whether they have been completed.

For example:

```csharp
await using (var transaction = await client.BeginTransactionAsync())
{
    // ... do transaction work ...
    transaction.Complete();
}
```

Here, the transaction will commit when `transaction` is disposed, because it has been completed. Had it not been completed, it would have rolled back. Note that the explicit pattern is also supported, although less recommended:

```csharp
var transaction = await client.BeginTransactionAsync();
// ... do transaction work ...
await transactionContext.CommitAsync();  // commmit, or...
await transactionContext.DisposeAsync(); // roll back
await transaction.DisposeAsync();
```

Refer to the [Transactions](transactions.md) page for details.

## Events

The client exposes client-level events.

For example:

```csharp
var subscriptionId = await client.SubscribeAsync(events => events
    .StateChanged((sender, args) => {
        Console.WriteLine($"Client state changed to: {args.State}.")
    })
);

// ... handle events ...

var success = await client.UnsubscribeAsync(subscriptionId);
```

The `(sender, args)` pattern is used to remain consistent with C# events. Here, `sender` is the object that triggered the event, i.e. `client`, and `args` contains the event data.

> Note: pure C# events (`client.StateChanged += ...`) cannot be used here, as subscribing, un-subscribing and handling events all need to support being asynchronous.

Each distributed object also exposes events in the same way.

For example:

```csharp
var subscriptionId = await dict.SubscribeAsync(events => events
    .EntryAdded((sender, args) => {
        // ...
    })
    .EntryRemoved((sender, args) => {
        // ...
    })
);

// ... handle events ...

var success = await dict.UnsubscribeAsync(subscriptionId);
```

Refer to the [Events](events.md) page for details.