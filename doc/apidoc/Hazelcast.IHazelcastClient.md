---
uid: Hazelcast.IHazelcastClient
---
The Hazelcast client is the entry point to all interactions with an Hazelcast cluster. A client is created
by a @Hazelcast.HazelcastClientFactory. Before it can be used, it needs to be opened via the
@Hazelcast.IHazelcastClient.OpenAsync* method. After it has been used, it needs to be disposed 
in order to properly release its resources. For example:

```csharp
var options = new HazelcastOptionsBuilder.Build();
var client = await HazelcastClientFactory.StartNewClientAsync();
// ... use the client ...
await client.DisposeAsync();
```

---
uid: Hazelcast.IHazelcastClient.BeginTransactionAsync*
summary: Begins a new transaction.
---
The method returns an @Hazelcast.Transactions.ITransactionContext which can be used to obtain transactional 
distributed objects, and to commit or roll the transaction back.

See general documetnation.... etc...

