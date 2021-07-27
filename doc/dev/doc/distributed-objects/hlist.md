# HList

A `HList` list is a distributed list corresponding to a cluster-side [List](https://docs.hazelcast.com/imdg/latest/data-structures/list.html) which can be considered as a distributed implementation of the well-known C# `IList<T>`. A `HList` is a specialized `IHCollection`.

The list behavior can be configured on the server: see the general [List documentation](https://docs.hazelcast.com/imdg/latest/data-structures/list.html) for complete details about lists.

## Defining Lists

Maps are fully identified by their type and unique name, regardless of the types specified for list items. In other words, an `HList<string>` and an `HList<int>` named with the same name are backed by the *same* cluster structure. Obviously, refering to a list with types other than the expected types can have unspecified consequences (probably, serialization errors) and is not recommended.

The items type can be just about any valid .NET type, provided that it can be (de)serialized by the Hazelcast .NET Client (see the [Serialization](../serialization.md) documentation). It does not necessarily need to be (de)serializable by the cluster, as long as the cluster does not need to handle them as objects, and can treat them as plain binary blobs. As soon as the cluster needs to handle the objects themselves, the types must also be (de)serializable by the cluster.

## Creating & Destroying Lists

A list is obtained from the Hazelcast .NET Client, and is created on-demand: if a list with the specified name already exists on the cluster, it is returned, otherwise it is created on the cluster. For instance:

```csharp
var list = await client.GetListAsync<string>("my-list");
```

Lists should be disposed after usage, in order to release their resources. Note that this only releases *client-side* resources, but the actual data remain available on the cluster for further usage. In order to wipe the list and its data entirely from the cluster, it needs to be destroyed:

```csharp
await list.DestroyAsync();
```

## Using Lists

The `HList` structure is completely documented in the associated @Hazelcast.DistributedObjects.IHList`1 reference documentation. It provides methods to manipulate entries, such as:

* `AddAsync(item)` adds an item to the list
* `GetAsync(index)` retrieves the item in the specified position
* `GetAllAsync()` retrieve the items
* `GetSizeAsync()` gets the number of items, and `IsEmptyAsync()` determines whether the map is empty
* `RemoveAsync(index)` remove an item

The `HList` structure exposes events (see events [general documentation](../events.md)) at list level. A complete list of events is provided in the @Hazelcast.DistributedObjects.CollectionEventHandlers`1 documentation. The following example illustrates how to subscribe, and unsubscribe, to list events:

```csharp
var id = await list.SubscribeAsync(events => events
    .ItemAdded((sender, args) => {
        logger.LogInformation($"Item {args.Item} was added.")
    }));

// ...

await list.UnsubscribeAsync(id);
```

Note that the handler methods passed to e.g. `EntryRemoved` or `Cleared` can be asynchronous, too.

