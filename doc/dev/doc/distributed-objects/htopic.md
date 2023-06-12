# HTopic

A `HTopic` topic is a distributed topic corresponding to a cluster-side [List](https://docs.hazelcast.com/imdg/latest/data-structures/topic.html).

The topic behavior can be configured on the server: see the general [Documentation](https://docs.hazelcast.com/imdg/latest/data-structures/topic.html) for complete details about topics.

## Defining Topics

Topics are fully identified by their type and unique name, regardless of the types specified for topic messages. In other words, an `HTopic<string>` and an `HTopic<int>` named with the same name are backed by the *same* cluster structure. Obviously, refering to a topic with types other than the expected types can have unspecified consequences (probably, serialization errors) and is not recommended.

The messages type can be just about any valid .NET type, provided that it can be (de)serialized by the Hazelcast .NET Client (see the [Serialization](../serialization.md) documentation). It does not necessarily need to be (de)serializable by the cluster, as long as the cluster does not need to handle them as objects, and can treat them as plain binary blobs. As soon as the cluster needs to handle the objects themselves, the types must also be (de)serializable by the cluster.

## Creating & Destroying Topics

A topic is obtained from the Hazelcast .NET Client, and is created on-demand: if a topic with the specified name already exists on the cluster, it is returned, otherwise it is created on the cluster. For instance:

```csharp
var optic = await client.GetTopicAsync<string>("my-topic");
```

Topics should be disposed after usage, in order to release their resources. Note that this only releases *client-side* resources, but the actual data remain available on the cluster for further usage. In order to wipe the topic and its data entirely from the cluster, it needs to be destroyed:

```csharp
await topic.DestroyAsync();
```

## Using Topics

The `HTopic` structure is completely documented in the associated @Hazelcast.DistributedObjects.IHTopic`1 reference documentation. It provides a method to publish messages:

* `PublishAsync(message)` publishes a message

The `HTopic` structure exposes events (see events [general documentation](../events.md)) at topic level. A complete list of events is provided in the @Hazelcast.DistributedObjects.CollectionItemEventHandlers`1 documentation. The following example illustrates how to subscribe, and unsubscribe, to topic events:

```csharp
var id = await topic.SubscribeAsync(events => events
    .Message((sender, args) => {
        logger.LogInformation($"Got message {args.Payload} at {args.PublishTime}.")
    }));

// ...

await topic.UnsubscribeAsync(id);
```

Note that, as with all events in the .NET client, the handler methods passed when subscribing can be asynchronous.