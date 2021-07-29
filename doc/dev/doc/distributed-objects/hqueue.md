# HQueue

A `HQueue` list is a distributed queue corresponding to a cluster-side [List](https://docs.hazelcast.com/imdg/latest/data-structures/queue.html) which can be considered as a distributed implementation of the well-known C# `Queue<T>`. A `HQueue` is a specialized `IHCollection`.

The queue behavior can be configured on the server: see the general [Queue documentation](https://docs.hazelcast.com/imdg/latest/data-structures/queue.html) for complete details about queues.

## Defining Queues

Queues are fully identified by their type and unique name, regardless of the types specified for queue items. In other words, an `HQueue<string>` and an `HQueue<int>` named with the same name are backed by the *same* cluster structure. Obviously, refering to a queue with types other than the expected types can have unspecified consequences (probably, serialization errors) and is not recommended.

The items type can be just about any valid .NET type, provided that it can be (de)serialized by the Hazelcast .NET Client (see the [Serialization](../serialization.md) documentation). It does not necessarily need to be (de)serializable by the cluster, as long as the cluster does not need to handle them as objects, and can treat them as plain binary blobs. As soon as the cluster needs to handle the objects themselves, the types must also be (de)serializable by the cluster.

## Creating & Destroying Queues

A queue is obtained from the Hazelcast .NET Client, and is created on-demand: if a queue with the specified name already exists on the cluster, it is returned, otherwise it is created on the cluster. For instance:

```csharp
var queue = await client.GetQueueAsync<string>("my-queue");
```

Queues should be disposed after usage, in order to release their resources. Note that this only releases *client-side* resources, but the actual data remain available on the cluster for further usage. In order to wipe the queue and its data entirely from the cluster, it needs to be destroyed:

```csharp
await queue.DestroyAsync();
```

## Using Queue

The `HQueue` structure is completely documented in the associated @Hazelcast.DistributedObjects.IHQueue`1 reference documentation. It provides methods to manipulate entries, such as:

* `AddAsync(item)` adds an item to the queue
* `OfferAsync(item)` tries to add an item to the queue, if possible
* `GetElementAsync()` retrieves (but does not remove) an item from the queue
* `TakeAsync()` removes and returns the head item from the queue
* `GetSizeAsync()` gets the number of items, and `IsEmptyAsync()` determines whether the queue is empty

The `HQueue` structure exposes events (see events [general documentation](../events.md)) at queue level. A complete list of events is provided in the @Hazelcast.DistributedObjects.CollectionItemEventHandlers`1 documentation. The following example illustrates how to subscribe, and unsubscribe, to queue events:

```csharp
var id = await queue.SubscribeAsync(events => events
    .ItemAdded((sender, args) => {
        logger.LogInformation($"Item {args.Item} was added.")
    }));

// ...

await queue.UnsubscribeAsync(id);
```

Note that the handler methods passed to e.g. `EntryRemoved` or `Cleared` can be asynchronous, too.

