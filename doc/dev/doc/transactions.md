# Transactions

The Hazelcast .NET client supports transaction-aware versions of:

* Lists: @Hazelcast.DistributedObjects.IHTxList`1
* Sets: @Hazelcast.DistributedObjects.IHTxSet`1
* Queues: @Hazelcast.DistributedObjects.IHTxQueue`1
* Maps: @Hazelcast.DistributedObjects.IHTxMap`2
* Multi-maps: @Hazelcast.DistributedObjects.IHTxMultiMap`2

This page documents the .NET client-side aspects of transactions. See the [Transactions](https://docs.hazelcast.com/hazelcast/latest/transactions/creating-a-transaction-interface.html) section of the Reference Manual for more details.

## Transaction Management

A transaction is started through the client, which then returns an @Hazelcast.Transactions.ITransactionContext instance. Transactions by default follow the Microsoft's transaction pattern: they must be disposed, and commit or roll back depending on whether they have been completed.

For example:

```csharp
await using (var transaction = await client.BeginTransactionAsync())
{
    // ... do transaction work ...
    transaction.Complete();
}
```

Here, the transaction will commit when `transaction` is disposed, because it has been completed. Had it not been completed, it would have rolled back. Note that the explicit pattern is also supported, although less recommended:

```csharp
var transaction = await client.BeginTransactionAsync();
// ... do transaction work ...
await transactionContext.CommitAsync();  // commmit, or...
await transactionContext.DisposeAsync(); // roll back
await transaction.DisposeAsync();
```

## Transaction-aware Objects

Transaction-aware objects are obtained from the transaction context.

For example:

```csharp
await using (var transaction = await client.BeginTransactionAsync())
{
    var queue = await transaction.GetQueueAsync<string>("my-queue");
    var map = await transaction.GetMapAsync<string, string>("my-map");
    var set = await transaction.GetSetAsync<string>("my-set");

    try
    {
        var o = await queue.PollAsync();
        // process the object...
        await map.PutAsync("1", "value-1");
        await set.AddAsync("value");
        // do more things...

        transaction.Complete();
    }
    catch
    {
        // report the error
        // don't Complete = transaction will roll back automatically
    }
}
```

In this example, either all operations are executed, or none.

## Transaction Options

Transactions can be configured via @Hazelcast.Transactions.TransactionOptions that can be passed to the @Hazelcast.IHazelcastClient.BeginTransactionAsync(Hazelcast.Transactions.TransactionOptions) method. These options are:

* @Hazelcast.Transactions.TransactionOptions.Durability specifies the durability of the transaction (see below)
* @Hazelcast.Transactions.TransactionOptions.Timeout specifies the timeout of the transaction
* @Hazelcast.Transactions.TransactionOptions.Type can be either `TwoPhase` (by default) or `OnePhase`, See the [Reference Manual](https://docs.hazelcast.com/hazelcast/latest/transactions/creating-a-transaction-interface.html) for details

The *durability* of a transaction is the number of members in the cluster that can take over if a member fails during a transaction commit or rollback. This value only has meaning when the @Hazelcast.Transactions.TransactionOptions.Type is `TwoPhase`.
