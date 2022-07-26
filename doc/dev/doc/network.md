# Client Network

The Hazelcast .NET client network is composed of all the connections between the client and the members of the cluster. It is configured via the [Networking](configuration/options.md#networking) configuration options.

## Member Addresses

The address list contains the initial list of cluster member addresses which the client will try to connect to. The client uses this
list to find an alive member. Although it may be enough to give only one address of a member in the cluster
(since all members communicate with each other), it is recommended that you give the addresses for all the members.

You can specify multiple addresses, with or without the port information. If the port part of an address is omitted, then 5701, 5702 and 5703 will be tried in a random order for that address. By default, if the list is empty, the client will try to connect to `localhost`.

By default, the provided list is shuffled and tried in a random order. You can disable this behaviour by setting the configuration option `hazelcast.networking.shuffleAddresses` to `false`. In this case the address list will be tried in the specified order. 

## Operation Mode

The client has two operation modes because of the distributed nature of the data and cluster: smart and unisocket. Smart routing is enabled by default, and is controlled by the `hazelcast.networking.smartRouting` configuration option, but you may want to enable unisocket mode.

### Smart Routing

In the smart mode, the clients connect to each cluster member. Since each data partition uses the well known and consistent hashing algorithm, each client can send an operation to the relevant cluster member, which increases the overall throughput and efficiency. Smart mode is the default mode.

### Unisocket Client

For some cases, the clients can be required to connect to a single member instead of each member in the cluster. Firewalls, security or some custom networking issues can be the reason for these cases.

In the unisocket client mode, the client will only connect to one of the configured addresses. This single member will behave as a gateway to the other members. For any operation requested from the client, it will redirect the request to the relevant member and return the response back to the client returned from this member.

## Reconnect Mode

The client can, at times, become disconnected from the cluster, for instance in case of a brief network issue. By default, the client will then try to reconnect to the cluster automatically. It is possible to prevent this behavior and switching the client to a non-recoverable error state (i.e. the client must be destroyed and a new client must be recreated) through the `hazelcast.networking.reconnect` configuration option.

## Connection Timeout

The connection timeout is controlled by the `hazelcast.networking.connectionTimeoutMilliseconds` configuration option. It is the timeout value in milliseconds for a member to accept the client connection requests. More precisely, it is the client socket connection timeout for connecting to a member.
If the member does not respond within the timeout, the client will retry to connect as many as `ClientNetworkConfig.GetConnectionAttemptPeriod()` times.

This timeout is also used to control other socket connections such as Cloud Discovery.

The default value is `5000` milliseconds.

## Connection Attempt Limit and Period

> [!NOTE]
> This do not apply to version 4 and above of the client. It is kept here for reference only until we document how to achieve the same result in version 4 and above.

If a member does not accept a connection within the specified timeout, the client will retry a specified amount of times waiting for some amount of time  between each tries. Default value for attempts is `2`, and for delay is 3000ms.

## TLS/SSL

You can use TLS/SSL to secure the connection between the clients and members. Please refer to the [TLS/SSL](security/tlsssl.md) section for details.

## Hazelcast Cloud Discovery

Hazelcast Cloud Discovery enables clients to discover the cluster IP addresses through the Hazelcast Orchestrator. It is enabled by assigning a discovery token to the `hazelcast.networking.cloud.discoveryToken` configuration option.

To be able to connect to the provided IP addresses, you will need to use secure TLS/SSL connection between the client and members.
Therefore, you should set an SSL configuration as described in the the [TLS/SSL](security/tlsssl.md) section.

## Failover (Blue/Green Deployment)

.NET Client allows you to set failover cluster(s). In order to use the backup cluster, it should be configured under `hazelcast-failover` in your appsettings.json or via `HazelcastFailoverOptionsBuilder`, and client should be initialized with failover factory methods under `HazelcastClientFactory`. The client will use first client configuration as default for all options. Alternative clients can only change network and authentication options. In case of connection failure, first, client will retry to connect to current cluster. If the connection fails, client will try to connect to next cluster. Another case is that client is blacklisted from cluster, client will do failover without retrying. Client will try alternative clusters until `TryCount` is exhausted. The cluster list that is composed from `clients`, the list is visited as in a circle way, such as
`clients[0] -> clients[1] ->  clients[0] -> ...` When one of the clusters is connected, `TryCount` will be reset. After connection is established, client state will be `ClusterChanged` and then `Connected`.
> Note: `SmartRouting` option cannot be different between given client configurations. Client will use the `clients[0].Networking.SmartRouting` option for all clusters, and it cannot be overwritten by alternative client configurations.

> Note: Please be aware of that failover can occur during very first connection to the cluster. Whenever client cannot connect on `Started` state, failover can also happen. 

[More about failover](failover.md)