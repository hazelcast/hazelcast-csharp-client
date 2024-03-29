# Client Messaging

Client messaging controls the operations between the client and each member. Messaging is configured via the [Messaging](configuration/options.md#messaging) configuration options, and is also impacted by 

## Operation Failure

While sending the requests to related members, operations can fail due to various reasons. Read-only operations are retried by default. If you want to enable retry for the other operations, you can enable "redo operations" via the `hazelcast.networking.redoOperations` configuration option.

When an operation is not retried, it fails immediately with an exception.

> [!WARNING]
> An operation that fails *may* have been performed. For instance, the client can send a queue insert operation request to the cluster, and *then* lose the connection. The operation will report an error, but the client cannot determine whether the cluster has received the request and processed it or not. Re-running the operation could lead to a duplicate insert.

Operations are retried for as much as `hazelcast.messaging.retryTimeoutSeconds` seconds before failing. They are initially retried without any delay (i.e. as fast as possible), for as much as `hazelcast.messaging.maxFastInvocationCount` attempts. Then, a delay is introduced between each attempt. This delay is at least `hazelcast.messaging.minRetryDelayMilliseconds` and increases after each failure: currently it is multiplied by 2 on each failure, and cannot exceed the timeout delay.

Special cases can be configured via dedicated configuration options:

* When an operation fails because the underlying socket connection drops, and the operation should be retried according to the rules above, and `hazelcast.messaging.retryOnConnectionLost` is `true`, it is retried. It is possible to prevent retries only in this situation by setting the configuration option to `false`.
* When an operation fails because the the client is not connected anymore, but remains active and is trying to reconnect, and the operation should be retried according to the rules above, and `hazelcast.messaging.retryOnClientReconnecting` is `true`, it is retried. It is possible to prevent retries only in this situation by setting the configuration option to `false`.



