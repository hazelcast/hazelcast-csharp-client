# Canceling

Some of Hazelcast .NET client operations support cancellation via a `CancellationToken`. This section details what actually happens when that token is cancelled.

## Default Operations

When a `CancellationToken` is passed to a method that triggers a server-side operation, the token is ultimately passed to the internal method which sends the request message to the server. At that point:
* If the token is cancelled before the request has been sent, the request is not sent
* If the token is cancelled while retrying to send a request, the retry is aborted

Otherwise, the request message is sent as a whole non-cancellable operation, and the client then waits for a response message from the server. It the token is cancelled during this wait, the wait is aborted and the response message will be ignored when it is received. 

> [!WARNING]
>Note however that, since the request message *has* been sent, the resulting server state is unspecified: the operation *may* have been executed and is *not* rolled back.

## Sql Operations

This section details cancellation behaviors specific to SQL. Refer to the  [SQL documentation](sql.md) for more general information on SQL.

### Sql Queries

Sql queries (see @Hazelcast.Sql.ISqlService `ExecuteQueryAsync` method) fetch the first page of rows, and return an enumerable @Hazelcast.Sql.ISqlQueryResult object representing an open *server-side* query. This query remains open and running on the server, in order to provide more pages of rows.

The `CancellationToken` which is passed to `ExecuteQueryAsync`, if any, works just as with any other server operation as far as fetching the first page of rows is concerned (see above). In addition, it is also passed to the `ISqlQueryResult`.

> [!NOTE]
> If the operation is cancelled before the `ISqlQueryResult` is returned, the SQL service tries its best to notify the server that the query should be closed.

The `ISqlQueryResult` is `IAsyncEnumerable<SqlRow>`. This means that one can either:
* Invoke its `GetAyncEnumerator(CancellationToken enumerationToken)` method
* Enumerate it with `async foreach`, with  `.WithCancellation(CancellationToken enumerationToken)`

In both cases, the enumeration ends up being controlled by a `CancellationToken` which is either `token` (from `ExecuteQueryAsync`), `enumerationToken`, or a combination of both. If `token` or `enumerationToken` is canceled, the enumeration is canceled and throws an `OperationCancelledException`.

It is important to understand that the `ISqlQueryResult` represents a server-side query which needs to be closed in order to release server resources. This is achieved by async-disposing the `ISqlQueryResult` (which is `IAsyncEnumerable`).

If the result is enumerated with `await foreach`, the pattern guarantees that the enumerator will be disposed when exiting. This will, in turn, dispose the result. Should the enumeration be cancelled and abort with an exception, the result would therefore safely be disposed.

With other patterns, it is important to ensure that, in the event of a cancellation, the result is always disposed. This can be achieved by wrapping the `ISqlQueryResult` in a `using` block:

```csharp
await using result = await client.Sql.ExecuteQueryAsync(...);
```

### Sql Commands

Sql commands (see @Hazelcast.Sql.ISqlService `ExecuteCommandAsync` method) do not have a special result type, as they only return the number of affected rows, and therefore the `CancellationToken` works just as with any other server operation.

> [!WARNING]
> Because Sql commands can have side-effect on the server, and cancellation may happen before or after the request message has been sent to the server, it is important to understand that cancellation concerns the client-side async method call only, and that the command may or may not execute on the server.
