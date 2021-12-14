# FlakeIdGenerator

A `FlakeIdGenerator` is a cluster-wide unique identifier generator. The identifiers are `long` primitive values in the range from 0 to `long.MaxValue` and are k-ordered (i.e. roughly ordered). Refer to the [FlakeIdGenerator section](https://docs.hazelcast.com/hazelcast/latest/data-structures/flake-id-generator) of the Hazelcast Reference Manual for more details.

The identifiers contain a timestamp component, and a member identifier component which is assigned when the member joins the cluster. This allows identifiers to be ordered and unique without any coordination between members, thus making the generator safe even in split-brain scenario.

## Using FlakeIdGenerator

A FlakeIdGenerator is obtained from the Hazelcast .NET Client. For instance:

```csharp
var generator = await client.GetFlakeIdGeneratorAsync("my-generator");
var id1 = await generator.GetNewIdAsync();
var id2 = await generator.GetNewIdAsync();
await generator.DisposeAsync();
```

## Configuring FlakeIdGenerator

To avoid frequent round-trips to the members, a client usually prefetches a batch of identifiers. The size of each batch can be configured via the @Hazelcast.DistributedObjects.FlakeIdGeneratorOptions.PrefetchCount option: allowed values are between 1 and 100,000 inclusive, and the default value is 100.

In order to preserve rough ordering, a batch of identifiers is only valid for a given amount of time, which can be configured via the @Hazelcast.DistributedObjects.FlakeIdGeneratorOptions.PrefetchValidityPeriod option. If you do not care about ordering, this option can be set to `Timeout.InfiniteTimeSpan`. The default value is 10 minutes.

Each generator can be configured, based upon its name. For instance:

```csharp
options.FlakeIdGenerator["my-generator"] = new FlakeIdGeneratorOptions
{
    PrefetchCount = 40,
    PrefetchValidityPeriod = Timeout.InfiniteTimeSpan
}
```

If no configuration exists for a specified generator name, then the default configuration is used. The default configuration for all generators can be modified via the special `*` wildcard name:

```csharp
options.FlakeIdGenerator["*"] = new FlakeIdGeneratorOptions
{
    PrefetchCount = 40,
    PrefetchValidityPeriod = Timeout.InfiniteTimeSpan
}
```

The configuration for a specified name is determined via the `IPatternMatcher` configured via the @Hazelcast.HazelcastOptions.PatternMatcher property, and therefore wildcards are supported.

See also the general [Configuration](../configuration.md) documentation.

