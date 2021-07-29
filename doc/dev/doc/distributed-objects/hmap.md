# HMap, HMultiMap, HReplicatedMap

A `HMap` map is a distributed key/value store corresponding to a cluster-side [Map](https://docs.hazelcast.com/imdg/latest/data-structures/map.html) which can be considered as a distributed implementation of the well-known C# `IDictionary<K,V>`, with data being partitioned over members of the cluster, thus providing horizontal scalability. It is one of the most important Hazelcast data structures. Additionally, Hazelcast provides the following map-related data structures:

* A `HMultiMap` map is a distributed key/value store corresponding to a cluster-side [MultiMap](https://docs.hazelcast.com/imdg/latest/data-structures/multimap.html): a specialized map that supports storing multiple values under a single key. 
* A `HReplicatedMap` map is a distributed key/value store corresponding to a cluster-side [ReplicatedMap](https://docs.hazelcast.com/imdg/latest/data-structures/replicated-map.html): a specialized map where data is replicated to all members of the cluster, instead of being partitioned, thus providing faster read/write accesses at the cost of higher server memory consumption.

The maps behavior can be configured on the server: see the general [Map documentation](https://docs.hazelcast.com/imdg/latest/data-structures/map.html) for complete details about maps.

## Defining Maps

Maps are fully identified by their type (`HMap`, `HReplicatedMap` or `HMultiMap`) and unique name, regardless of the types specified for keys and values. In other words, an `HMap<string, string>` and an `HMap<int, int>` named with the same name are backed by the *same* cluster structure. Obviously, refering to a map with types other than the expected types can have unspecified consequences (probably, serialization errors) and is not recommended.

Key and value types can be just about any valid .NET type, provided that they can be (de)serialized by the Hazelcast .NET Client (see the [Serialization](../serialization.md) documentation). They do not necessarily need to be (de)serializable by the cluster, as long as the cluster does not need to handle them as objects, and can treat them as plain binary blobs. As soon as the cluster needs to handle the objects themselves (for instance, if in-memory format is configured as `OBJECT`, or if entries are processed by entry processors), the types must also be (de)serializable by the cluster.

Because keys may never be de-serialized on the cluster, the cluster always treat them as binary blobs, for comparison purposes. That is to say, two keys are considered identical by the cluster if their serialized representations are identical.

## Creating & Destroying Maps

A map is obtained from the Hazelcast .NET Client, and is created on-demand: if a map with the specified name already exists on the cluster, it is returned, otherwise it is created on the cluster. For instance:

```csharp
var map = await client.GetMapAsync<string, string>("my-map");
```

Maps should be disposed after usage, in order to release their resources. Note that this only releases *client-side* resources, but the actual data remain available on the cluster for further usage. In order to wipe the map and its data entirely from the cluster, it needs to be destroyed:

```csharp
await map.DestroyAsync();
```

## Using Maps

The `HMap` structure is completely documented in the associated @Hazelcast.DistributedObjects.IHMap`2 reference documentation. It provides methods to manipulate entries, such as:

* `SetAsync(key, value)` and `PutAsync(key, value)` add an entry to the map
* `GetAsync(key)` retrieves the value associated with a key
* `GetKeysAsync()`, `GetValuesAsync()` retrieve the keys and values
* `ContainsKeyAsync(key)`, `ContainsValueAsync(value)` determines whether the map contains a key or a value
* `GetSizeAsync()` gets the number of entries, and `IsEmptyAsync()` determines whether the map is empty
* `RemoveAsync(key)` and `DeleteAsync(key)` remove an entry

The `HMap` structure also supports locks at entry level via methods such as:

* `LockAsync(key)` locks the entry associated with the key
* `UnlockAsync(key)` unlocks an entry that was previously locked
* `IsLockedAsync(key)` determines whether an entry is locked

> [!NOTE]
> Note that locks, due to the asynchronous aspect of the API, are not thread-based but context-based. Refer to the [locking](../locking.md) documentation for complete details.

The `HMap` structure exposes events (see events [general documentation](../events.md)) both at map level and at entry level. A complete list of events is provided in the @Hazelcast.DistributedObjects.MapEventHandlers`2 documentation. The following example illustrates how to subscribe, and unsubscribe, to map events:

```csharp
var id = await map.SubscribeAsync(events => events
    .EntryRemoved((sender, args) => {
        logger.LogInformation($"Key={args.Key} / value={args.Value} removed.")
    })
    .Cleared((sender, args) => {
        logger.LogInformation("The map has been cleared.");
    }));

// ...

await map.UnsubscribeAsync(id);
```

Note that the handler methods passed to e.g. `EntryRemoved` or `Cleared` can be asynchronous, too.
