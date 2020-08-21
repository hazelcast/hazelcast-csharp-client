---
uid: Hazelcast.IHazelcastClient
---
The Hazelcast client is the entry point to all interactions with an Hazelcast cluster. A client is created
by a @Hazelcast.HazelcastClientFactory. Before it can be used, it needs to be opened via the
@Hazelcast.IHazelcastClient.OpenAsync* method. After it has been used, it needs to be disposed 
in order to properly release its resources. For example:

```csharp
var options = HazelcastOptions.Build();
var factory = new HazelcastClientFactory(options);
var client = factory.CreateClient();
await client.OpenAsync();
// ... use the client ...
await client.DisposeAsync();
```

See [Hazelcast Client](../doc/hazelcastClient.md) in the general documentation for more details.

---
uid: Hazelcast.IHazelcastClient.BeginTransactionAsync*
summary: Begins a new transaction.
---
The method returns an @Hazelcast.Transactions.ITransactionContext which can be used to obtain transactional 
distributed objects, and to commit or roll the transaction back.

See general documetnation.... etc...

---
uid: Hazelcast.IHazelcastClient.OpenAsync*
summary: Opens the client.
---
A client must be opened before it can access the servers. There is no corresponding "close"
method: a client is closed when it is disposed.