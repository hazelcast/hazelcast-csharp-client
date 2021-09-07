# Data Affinity

*Data affinity* ensures that related map entries exist on the same cluster member. If related data is on the same member, operations can be executed without the cost of extra network calls and extra wire data. Hazelcast has a standard way of routing all operations on an entry to the member which owns and manages the entry, based upon the key of the entry. For instance, getting the value of the entry, setting this value, locking the entry, etc. are operations that are all performed by the same member.

This is achieved through *partitioning*, which consists in assigning a *partition identifier* to each map entry. The cluster is configured to support a configurable number of partitions (see [Partition Group Configuration](https://docs.hazelcast.com/imdg/latest/clusters/partition-group-configuration.html) in the Reference Manual for details), which are distributed randomly and equally among members.

When the client establishes its first connection to the cluster, it receives data from the server, including:

* the total number of partitions
* a table mapping partition identifiers to member identifiers.

## Partitioning

The partition identifier is determined via the following logic:

* From the map entry key, a *partition key* is determined: by default, it is the map entry key itself, but it is possible to use an @Hazelcast.Partitioning.Strategies.IPartitioningStrategy implementation to specify a different object to use
* The partition key is hashed to an `Int32` value, using an optimized algorithm
* This hash is modded with the total number of partitions to obtain the partition identifier

The @Hazelcast.Partitioning.Strategies.IPartitioningStrategy interface provides one unique method, @Hazelcast.Partitioning.Strategies.IPartitioningStrategy.GetPartitionKey(System.Object), which returns the partition key. In this case:

* If the returned partition key is `null`, the map entry key is used as the partition key
* If the returned partition key implements `IData`, the hash is directly obtained from the `IData.PartitionHash` property
* Otherwise, the partition key is hashed as explained above

## Strategies

Partitioning strategies are explained in details in the [Reference Manual](https://docs.hazelcast.com/hazelcast/lastest/performance/data-affinity.html#partitioningstrategy).

> [!NOTE]
> As of version 4.1 the following strategies are internal classes, and it is not possible to configure the global strategy. This will be added in further releases.

The Hazelcast .NET client proposes different built-in strategies:

* `Hazelcast.Partitioning.Strategies.PartitionAwarePartitioningStrategy` only for objects implementing @Hazelcast.Partitioning.Strategies.IPartitionAware (else returns `null`), returns the result of the @Hazelcast.Partitioning.Strategies.IPartitionAware.GetPartitionKey method
* `Hazelcast.Partitioning.Strategies.StringPartitioningStrategy` only for string keys (else returns `null`), trims chars before the first `@` char, so "abc" becomes "abc" and "abc@def" becomes "def", and returns the resulting string

The global strategy can be configured via the `hazelcast.partitioning.globalStrategy` configuration option. The default global strategy is the `PartitionAwarePartitioningStrategy`.

At server level, it is possible to configure the local strategy used for each map. However, this is not supported by clients. On the other hand, you can implement a custom strategy which applies different logics depending on the type of the object.
