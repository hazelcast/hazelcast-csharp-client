# HReliableTopic

A `HReliableTopic` topic is the durable version of [HTopic](/htopic.md) backed with a [HRingBuffer](/hringbuffer.md).

The reliable topic behavior can be configured on the server: see the general [Documentation](https://docs.hazelcast.com/imdg/latest/data-structures/reliable-topic) for complete details about reliable topics.

## Defining Topics

Topics are fully identified by their type and unique name, regardless of the types specified for topic messages. In other words, an `HReliableTopic<string>` and an `HReliableTopic<int>` named with the same name are backed by the *same* cluster structure which is a [RingBuffer](/hringbuffer.md). Obviously, refering to a topic with types other than the expected types can have unspecified consequences (probably, serialization errors) and is not recommended.

The messages type can be just about any valid .NET type, provided that it can be (de)serialized by the Hazelcast .NET Client (see the [Serialization](../serialization.md) documentation). It does not necessarily need to be (de)serializable by the cluster, as long as the cluster does not need to handle them as objects, and can treat them as plain binary blobs. As soon as the cluster needs to handle the objects themselves, the types must also be (de)serializable by the cluster.

## Creating & Destroying Topics

A reliable topic is obtained from the Hazelcast .NET Client, and is created on-demand: if a reliable topic with the specified name already exists on the cluster, it is returned, otherwise it is created on the cluster. For instance:

```csharp
var rTopic = await client.GetReliableTopicAsync<string>("my-reliableTopic");
```

## Configuring the HReliableTopic

There are three different parts can be configured. One is server side configuration, size of the backed ring buffer, TTL, overflow policy etc. Second is the reliable topic behavior on the client side, such as `ReliableTopicOptions.BatchSize` and `ReliableTopicOptions.Policy`. The batch size sets the number of messages read by the listener at once. And, overflow policy defines the behavior during publishing a message over `HReliableTopic`. Third one is for listener. The subscription is made to a `HReliableTopic` results in a listener. Some of the behaviors of the listener can be configured. For example, Los tolerancy, initial sequnce to start from, storing the sequence of the last read message and whether terminate the listener in case of an exception.

> [!NOTE]
> To have a durable listener, set `IsLossTolerant` to `false` and `StoreSequence` to `true`.

Topics should be disposed after usage, in order to release their resources. Note that this only releases *client-side* resources, but the actual data remain available on the cluster for further usage. In order to wipe the topic and its data entirely from the cluster, it needs to be destroyed:

```csharp
await rTopic.DestroyAsync();
```

## Using Reliable Topics

The `HReliableTopic` structure is completely documented in the associated @Hazelcast.DistributedObjects.ReliableTopic`1 reference documentation. It provides a method to publish messages:

* `PublishAsync(message)` publishes a message

The `HReliableTopic` structure exposes events in a way similar to `HTopic`, but with some additions. When a subscription is made to a `HReliableTopic`, a background task is spawned. It listens to messages from the backing `HRingBuffer`, and triggers the corresponding `Message` topic events.

In addition, 
* The `Exception` event is raised when a message can not be processed, either because it can not be deserialized, or because an exception is thrown by one of the `Message` event handlers and interrupts the handling of the message. In this situation, by default, the subscription terminates, because it is not supposed to "skip" messages. It is however possible to cancel that termination by setting the `Cancel` event arguments property to `true`, in which case the subscription will move on to the next messages. 

* The `Terminated` event is raised when the subscription terminates, either because of a non-canceled exception (see above), or when anything goes wrong with the underlying buffer (overload, loss...), or when it is actively terminated by e.g. disposing the reliable topic instance. Finally, the behavior of the subscription can be configured via the `ReliableTopicEventHandlerOptions`.

>[!NOTE] The similar event exist in other Hazelcast clients as the `onCancel` function callback at the listener interface.

```csharp
var id = await topic.SubscribeAsync(events => events
    .Message((sender, args) => {
        logger.LogInformation($"Got message {args.Payload} at {args.PublishTime}.");
    })
    .Terminated((sender, args) =>{
        logger.LogInformation($"Listener disposed at sequence {args.Sequence}.");
     }),
     .Exception((sender, args) =>
    {
        // Terminate the subscription if client goes offline.
        if (args.Exception is ClientOfflineException)
            args.Cancel = true;
    }),
    // Setting StoreSequence=true and IsLossTolerant=false means listener is durable.
    new ReliableTopicEventHandlerOptions() {InitialSequence = -1, StoreSequence = true, IsLossTolerant = false});

// ...

await rTopic.UnsubscribeAsync(id);
```

Note that, as with all events in the .NET client, the handler methods passed when subscribing can be asynchronous.