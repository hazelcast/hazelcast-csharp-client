# Events

Events in the Hazelcast .NET Client rely on a handler-based model close to the C# `event` model, though with a different syntax for adding and removing handlers, due to the asynchronous nature of these operations. Indeed, the following code has limitations:

```csharp
thing.Updated += OnThingUpdated;
thing.Deleted += OnThingDeleted;
```

Here,

* The two operations are distinct, whereas Hazelcast supports subscriptions that handle multiple events at once;
* The subscription (`+=` operation) is synchronous, whereas Hazelcast needs to notify the members of the subscription;
* The handlers (e.g. `OnThingUpdated`) are synchronous.

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

In the example, the two events are subscribed *at once* and that subscription is represented by the returned `id`, which is a `Guid`. The two events can only be unsubscribed *at once* too, by passing this `id`:

```csharp
await thing.UnsubscribeAsync(id);
```

## Client Events

The `IHazelcastClient` exposes the following events:

* `StateChanged` triggers when the client state changes (TODO: link to client lifecycle doc)
* `PartitionLost` triggers when (TODO: complete)
* `PartitionsUpdated` triggers when the partitions table is updated
* `MembersUpdated` triggers when the members list is updated
* `ObjectCreated` triggers when a distributed object is created
* `ObjectDestroyed` triggers when a distributed object is destroyed

TODO: detail each event args

## Distributed Objects Events

TODO: complete