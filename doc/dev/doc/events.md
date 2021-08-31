# Events

Events in the Hazelcast .NET Client rely on a handler-based model close to the C# `event` model, though with a different syntax for adding and removing handlers, due to the asynchronous nature of these operations. Indeed, the following code has limitations:

```csharp
thing.Updated += OnThingUpdated;
thing.Deleted += OnThingDeleted;
```

Here,

* The operations are distinct, whereas Hazelcast subscriptions can handle multiple events at once
* The subscription is synchronous, whereas Hazelcast needs to notify the members of the subscription
* The handlers (e.g. `OnThingUpdated`) are synchronous

## Hazelcast Events

To overcome these limitations, the Hazelcast .NET Client uses the following syntax:

```csharp
var id = await thing.SubscribeAsync(events => events
    .Updated(OnThingUpdated)
    .Deleted(OnThingDeleted));
```

Here, the handlers can be synchronous, for instance:

```csharp
private void OnThingUpdated(Thing sender, ThingUpdatedEventArgs args)
{ 
    ...
}
```

But they can also be asynchronous, for instance:

```csharp
private async ValueTask OnThingUpdated(Thing sender, ThingUpdatedEventArgs args)
{
    await ...
}
```

In the example, the two events are subscribed *at once* and that subscription is represented by the returned `id`, which is a `Guid`. The two events can only be unsubscribed *at once* too, by passing this `id` back to the `UnsubscribeAsync` method:

```csharp
await thing.UnsubscribeAsync(id);
```

Subscribing involves exchanging messages with the server, and takes time. Where traditional C# happily does:

```csharp
thing.Updated += DoThis;
thing.Updated += AlsoDoThis;
```

It would be much more efficient to group the two handlers:

```csharp
var id = await thing.SubscribeAsync(events => events
    .Updated((sender, args) => 
    {
        DoThis(sender, args);
        AlsoDoThis(sender, args);
    }));
```

This is also the only way to guarantee the order of execution of the two handlers, as the order in which events trigger is not specified, and should not be relied upon.

## Client Events

The @Hazelcast.IHazelcastClient exposes the following events:

* `StateChanged` triggers when the client state changes
* `PartitionLost` triggers when a partition is lost
* `PartitionsUpdated` triggers when the partitions table is updated
* `MembersUpdated` triggers when the members list is updated
* `ObjectCreated` triggers when a distributed object is created
* `ObjectDestroyed` triggers when a distributed object is destroyed

The @Hazelcast.IHazelcastClient directly supports subscribing to events. For instance:

```csharp
var id = await client.SubscribeAsync(events => events
    .StateChanged((sender, args) => {
        System.Console.WriteLine($"New client state: {args.State}");
    }));
```

### StateChanged

The `StateChanged` event triggers whenever the state of the client changes. Handles receive an instance of the @Hazelcast.StateChangedEventArgs class, which exposes the following property:
* @Hazelcast.StateChangedEventArgs.State: the new @Hazelcast.ClientState

An @Hazelcast.IHazelcastClient instance goes through the following @Hazelcast.ClientState states:
* @Hazelcast.ClientState.Starting: the client is starting and has not started to connect to members yet (transition state)
* @Hazelcast.ClientState.Started: the client has started, and is now trying to connect to a first member (transition state)
* @Hazelcast.ClientState.Connected: the client is connected to at least one member (operational state)
* @Hazelcast.ClientState.Disconnected: the client has disconnected, due to its last member leaving the cluster, or a network error. Depending on its configuration it will either try to connect again (and transition back to @Hazelcast.ClientState.Connected if successful) or fail and transition to @Hazelcast.ClientState.Shutdown (transition state)
* @Hazelcast.ClientState.ShuttingDown: the client has been disposed, i.e. properly requested to shut down, and is shutting down (transition state)
* @Hazelcast.ClientState.Shutdown: the client has shut down (final state)

### PartitionLost

The `PartitionLost` event triggers whenever the server notifies the client that a partition has been lost, usually because a member carrying that partition has left the cluster. Handlers receive an instance of the @Hazelcast.Events.PartitionLostEventArgs class, which exposes the following properties:
* @Hazelcast.Events.PartitionLostEventArgs.PartitionId: the identifier of the lost partition
* @Hazelcast.Events.PartitionLostEventArgs.LostBackupCount: how many backups were lost
* @Hazelcast.Events.PartitionLostEventArgs.IsAllReplicasInPartitionLost: whether all replicas were lost
* @Hazelcast.Events.PartitionLostEventArgs.Member: the member that was lost

### PartitionsUpdated

The `PartitionsUpdated` event triggers whenever the server notifies the client of a new partitions list. This happens when the partitions list changes, but also periodically when the server wants to ensure that clients are aware of partitions. Handlers do not receive any event arguments.

### MembersUpdated

The `MembersUpdated` event triggers whenever the server notifies the client of a new members list. This happens when members are added or removed from the cluster, but also periodically when the server wants to ensure that clients know about members. Handlers receive an instance of the @Hazelcast.Events.MembersUpdatedEventArgs class, which exposes the following properties:
* @Hazelcast.Events.MembersUpdatedEventArgs.AddedMembers: a collection of @Hazelcast.Models.MemberInfo representing the members that were added to the cluster
* @Hazelcast.Events.MembersUpdatedEventArgs.RemovedMembers: a collection of @Hazelcast.Models.MemberInfo representing the members that were removed from the cluster
* @Hazelcast.Events.MembersUpdatedEventArgs.Members: a collection of @Hazelcast.Models.MemberInfo representing all members in the cluster

### ObjectCreated

The `ObjectCreated` event triggers whenever the server notifies the client that a new distributed object has been created (for instance, when the server creates a new map named `my-map`). Handlers receive an instance of the @Hazelcast.Events.DistributedObjectCreatedEventArgs class, which exposes the following properties:
* @Hazelcast.Events.DistributedObjectLifecycleEventArgs.ServiceName: the internal Hazelcast service name (for instance, for maps, `hz:impl:mapService`)
* @Hazelcast.Events.DistributedObjectLifecycleEventArgs.Name: the name of the created object (for instance, `my-map`)
* @Hazelcast.Events.DistributedObjectLifecycleEventArgs.SourceMemberId: the identifier of the member which triggered the event

### ObjectDestroyed

The `ObjectDestroyed` event triggers whenever the server notifies the client that a distributed object has been destroyed (for instance, when the client requests that the server destroys a map named `my-map`). Handlers receive an instance of the @Hazelcast.Events.DistributedObjectDestroyedEventArgs class, which exposes the following properties:
* @Hazelcast.Events.DistributedObjectLifecycleEventArgs.ServiceName: the internal Hazelcast service name (for instance, for maps, `hz:impl:mapService`)
* @Hazelcast.Events.DistributedObjectLifecycleEventArgs.Name: the name of the destroyed object (for instance, `my-map`)
* @Hazelcast.Events.DistributedObjectLifecycleEventArgs.SourceMemberId: the identifier of the member which triggered the event

## Distributed Objects Events

Each type of distributed object exposes events specific to the type. For instance, @Hazelcast.DistributedObjects.IHList\`1 exposes the `ItemAdded` event:

```csharp
var list = await client.GetListAsync("my-list");
var id = await list.SubscribeAsync(events => events
    .ItemAdded(async (sender, args) => 
    {
        await DoSomethingWithItem(args.Item);
        await DoSomethingElseWithItem(args.Item);
    }))
```

Refer to each distributed object's documentation for details on events.