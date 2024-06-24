# Distributed Objects

Distributed objects are managed by an Hazelcast cluster, and accessed via the Hazelcast .NET client. Currently, the client supports the following distributed objects:

* [HMap](distributed-objects/hmap.md) - a distributed key/value store corresponding to a cluster-side [Map](https://docs.hazelcast.com/hazelcast/latest/data-structures/map)
* [HMultiMap](distributed-objects/hmap.md) - a distributed key/value store corresponding to a cluster-side [MultiMap](https://docs.hazelcast.com/hazelcast/latest/data-structures/multimap)
* [HReplicatedMap](distributed-objects/hmap.md) - a distributed key/value store corresponding to a cluster-side [ReplicatedMap](https://docs.hazelcast.com/hazelcast/latest/data-structures/replicated-map)
* [HList](distributed-objects/hlist.md) - a distributed list store corresponding to a cluster-side [List](https://docs.hazelcast.com/hazelcast/latest/data-structures/list)
* [HQueue](distributed-objects/hqueue.md) - a distributed queue store corresponding to a cluster-side [Queue](https://docs.hazelcast.com/hazelcast/latest/data-structures/queue)
* [HRingBuffer](distributed-objects/hringbuffer.md) - a distributed ring-buffer corresponding to a cluster-side [Map](https://docs.hazelcast.com/hazelcast/latest/data-structures/ringbuffer)
* [HSet](distributed-objects/hset.md) - a distributed set store corresponding to a cluster-side [Set](https://docs.hazelcast.com/hazelcast/latest/data-structures/set)
* [HTopic](distributed-objects/htopic.md) - a distributed message-publishing store corresponding to a cluster-side [Topic](https://docs.hazelcast.com/hazelcast/latest/data-structures/topic)

Distributed objects are obtained from the Hazelcast .NET Client and are fully identified by their unique name. If an object of the specified type and with the specified name already exists on the cluster, it is returned, otherwise it is created on the cluster. For instance:

```csharp
var map = await client.GetMapAsync<string, string>("my-map");
```

Distributed objects should be disposed when not used, in order to release their resources. Note that this only releases *client-side* resources, but the actual data remain available on the cluster for further usage.

In order to wipe a distributed object entirely from the cluster, the object needs to be destroyed:

```csharp
await map.DestroyAsync();
```

## Transactions

In a transaction context, *transactional* versions of some distributed objects can be retrieved:

* `HTxList` - a transactional version of an `HList` object
* `HTxMap` - a transactional version of an `HMap` object
* `HTxMultiMap` - a transactional version of an `HMultiMap` object
* `HTxQueue` - a transactional version of an `HQueue` object
* `HTxSet` - a transactional version of an `HSet` object

For instance:

```csharp
using (var tx = await client.BeginTransactionAsync())
{
    using (var txmap = await tx.GetMapAsync<string, string>("my-map"))
    {
        // ...    
    }

    tx.Complete();
}
```

Transactional objects expose a subset of the methods of the original object, which are performed in a transactional way and are either commited (if the transaction is completed) or rolled back. Refer to the [Transactions](transactions.md) page for more details.

## Object Names and Types

The name of a distributed object is unique accross its type: there can only be one `HMap` object named `"my_map"`, but there can also be an `HList` object named `"my-map"`.

"Type", here, means the *generic definition* of the type, e.g. `HMap<,>` or `HList<>` and *not* the complete type (e.g. `HList<string>`).

This means that there is, in reality, one unique `HMap<,>` object named `"my-map"`, and that `client.GetMapAsync<string, string>("my-map")` would refer to the exact same object as `client.GetMap<int, int>("my-map")`. The consequences of refering to an object with different types are not specified: it *may* work if types can be implicitly casted, and will *not* work if they cannot. This is not recommended.