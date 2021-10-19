# Monitoring

The Hazelcast .NET client can collect and send metrics to the cluster. These metrics can then be analyzed using the [Hazelcast Management Center](https://hazelcast.com/product-features/management-center/). It is, for instance, possible to monitor the clients that are connected to the cluster.

Metrics, and the Management Center, are fully documented [here](https://docs.hazelcast.com/management-center/latest/index.html). The Hazelcast .NET client sends the following pieces of information:

* Client name, type and address
* Client connection timestamp
* Memory stats (commited size, max size, total size...)
* CPU stats (CPU time, CPUs count...)


Metrics are configured via the [Metrics](configuration/options.md#metrics) configuration options: metrics can be enabled via the `hazelcast.metrics.enabled` configuration option. When enabled, metrics are sent to the cluster every `hazelcast.metrics.periodSeconds` seconds.