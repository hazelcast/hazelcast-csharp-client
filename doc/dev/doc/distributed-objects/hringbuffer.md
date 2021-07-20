# HRingBuffer

A `HRingBuffer` list is a distributed ring-buffer corresponding to a cluster-side [List](https://docs.hazelcast.com/imdg/latest/data-structures/ringbuffer.html). Content in a ring-buffer is stored in a ring-like structure. A ringbuffer has a capacity so it won't grow beyond that capacity and endanger the stability of the system. If that capacity is exceeded, than the oldest item in the ringbuffer is overwritten.

The ring-buffer behavior can be configured on the server: see the general [Queue documentation](https://docs.hazelcast.com/imdg/latest/data-structures/ringbuffer.html) for complete details about ring-buffers.

## Defining Ring-Buffers

Ring-buffers are fully identified by their type and unique name, regardless of the types specified for bufferered items. In other words, an `HRingBuffer<string>` and an `HRingBuffer<int>` named with the same name are backed by the *same* cluster structure. Obviously, refering to a ring-buffer with types other than the expected types can have unspecified consequences (probably, serialization errors) and is not recommended.

The items type can be just about any valid .NET type, provided that it can be (de)serialized by the Hazelcast .NET Client (see the [Serialization](serialization.md) documentation). It does not necessarily need to be (de)serializable by the cluster, as long as the cluster does not need to handle them as objects, and can treat them as plain binary blobs. As soon as the cluster needs to handle the objects themselves, the types must also be (de)serializable by the cluster.

## Creating & Destroying Ring-Buffers

A ring-buffer is obtained from the Hazelcast .NET Client, and is created on-demand: if a ring-buffer with the specified name already exists on the cluster, it is returned, otherwise it is created on the cluster. For instance:

```csharp
var ringbuffer = await client.GetRingBufferAsync<string>("my-ring-buffer");
```

Ring-buffers should be disposed after usage, in order to release their resources. Note that this only releases *client-side* resources, but the actual data remain available on the cluster for further usage. In order to wipe the ring-buffer and its data entirely from the cluster, it needs to be destroyed:

```csharp
await ringbuffer.DestroyAsync();
```

## Using Ring-Buffers

The `HRingBuffer` structure is completely documented in the associated @Hazelcast.DistributedObjects.IHRingBuffer`1 reference documentation. It provides methods to manipulate entries, such as:

* `AddAsync(item)` adds an item to the ring-buffer
* `GetCapacityAsync()` gets the total capacity of the ring-buffer
* `GetRemainingCapacityAsync()` gets the remaining capacity of the ring-buffer
* `ReadOneAsync()` reads one item from the ring-buffer

