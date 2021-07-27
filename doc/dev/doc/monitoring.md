# Monitoring

The Hazelcast .NET client can collect and send metrics to the cluster. These metrics can then be analyzed using the [Hazelcast Management Center](https://hazelcast.com/product-features/management-center/). It is, for instance, possible to monitor the clients that are connected to the cluster.

Metrics are configured via the [Metrics](configuration/options.md#metrics) configuration options.

Metrics can be enabled via the `hazelcast.metrics.enabled` configuration option. When enabled, metrics are sent to the cluster every `hazelcast.metrics.periodSeconds` seconds.