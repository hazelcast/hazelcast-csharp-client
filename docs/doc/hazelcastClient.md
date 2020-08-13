# Hazelcast client

The Hazelcast client is the entry point to all interactions with an Hazelcast cluster. A client is created
by a @Hazelcast.HazelcastClientFactory. Before it can be used, it needs to be opened via the @Hazelcast.IHazelcastClient.ConnectAsync*
method. After it has been used, it needs to be disposed in order to properly release its resources.

For example:

```csharp
var factory = new HazelcastClientFactory();
var client = factory.CreateClient();
await client.ConnectAsync();
// ... use the client ...
await client.DisposeAsync();
```

A client is a heavy enough, multi-threaded object. Although a factory can create several, independent clients, it is recommended 
to store and reuse the client instance. It is *not* recommended to frequently create and dispose clients, as that could have an 
impact on performances.

Here, the client factory is configured by default, which means by configuration files and environment variables. For more control,
the factory can be initialized with an @Hazelcast.HazelcastOptions instance, which represents the complete set of options of the Hazelcast
client. In fact, the above example is equivalent to:

```csharp
var options = HazelcastOptions.Build();
var factory = new HazelcastClientFactory(options);
// ...
```


Refer to the [Configuration](configuration.md) page for details on the various ways to build an @Hazelcast.HazelcastOptions instance,
including handling command-line parameters, and to the @Hazelcast.HazelcastOptions reference for a list of all the configurable elements.

## Distributed Objects

The client can be used to obtain *distributed objects* that are managed by the cluster. For instance, the cluster can manage
@Hazelcast.DistributedObjects.IHMap`2 objects, which are an asynchronous equivalent of .NET @System.Collections.Generic.IDictionary`2.
Each object is identified by a unique name, which is used to retrieve the object. Finally, distributed objects need to be disposed
after usage, to ensure they release their resources.

For example:

```csharp
var map = await client.GetMapAsync<string, string>("map-name");
await map.SetAsync("key", "value");
var value = await map.GetAsync("key");
await map.DisposeAsync();
```

The @Hazelcast.IHazelcastClient.GetMapAsync* method returns the existing object with the specified name, or creates a new object with
that name on the cluster. That object will continue to live on the cluster after the @Hazelcast.DistributedObjects.IHMap`2 has
been disposed. In order to remove the object from the cluster, one must destroy the object.

For example:

```csharp
var map = await client.GetMapAsync<string, string>("map-name");
await client.DestroyAsync(map);
```

It is also possible to destroy objects with their name, and their corresponding *service name*:

```csharp
await client.DestroyAsync(HMap.ServiceName, "map-name");
```

## Transactions

The client is responsible for creating transactions.

For example:

```csharp
var transactionContext = await client.BeginTransactionAsync();
// ... do transaction work ...
await transactionContext.CommitAsync();
await transactionContext.DisposeAsync();
```

Refer to the [Transactions](transactions.md) page for details.

## Events

The client exposes client-level events.

For example:

```csharp
var subscriptionId = await client.SubscribeAsync(handle => handle
    .ClientStateChanged((sender, args) => {
        Console.WriteLine($"Client state changed to: {args.State}.")
    })
);

// ... handle events ...

var success = await client.UnsubscribeAsync(subscriptionId);
```

The `(sender, args)` pattern is used to remain consistent with C# events. Here, `sender` is the
object that triggered the event, i.e. `client`, and `args` contains the event data.

> Note: pure C# events (`client.StateChanged += ...`) cannot be used here, as subscribing, un-subscribing
and handling events all need to support being asynchronous.

Each distributed object also exposes events in the same way.

For example:

```csharp
var subscriptionId = await map.SubscribeAsync(handle => handle
    .EntryAdded((sender, args) => {
        // ...
    })
    .EntryRemoved((sender, args) => {
        // ...
    })
);

// ... handle events ...

var success = await map.UnsubscribeAsync(subscriptionId);
```

Refer to the [Events](events.md) page for details.