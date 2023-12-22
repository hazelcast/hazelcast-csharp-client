# Dynamic Configuration

Starting with version 5.4.0, the Hazelcast .NET client supports dynamic configuration of the cluster as described in the main [Dynamic Configuration for Members](https://docs.hazelcast.com/hazelcast/latest/configuration/dynamic-config) documentation, for the following structures:

* Maps
* Ring buffers

The dynamic configuration feature is accessible via the `IHazelcastClient.DynamicOptions` service. For instance, assuming that `client` is an `IHazelcastClient` instance, the following code can be used to configure a ring buffer:

```
await client.DynamicOptions.ConfigureRingbufferAsync("buffer-name", options =>
{
    options.Name = "buffer-name";
    options.AsyncBackupCount = 1;
    options.BackupCount = 1;
    options.Capacity = 1;
    options.InMemoryFormat = InMemoryFormat.Binary;
    options.MergePolicy.BatchSize = 1;
    options.MergePolicy.Policy = "policy";
    options.TimeToLiveSeconds = 1;
    options.SplitBrainProtectionName = "splitBrainProtectionName";
    options.RingbufferStore.Enabled = true;
    options.RingbufferStore.ClassName = "classNam";
    options.RingbufferStore.FactoryClassName = "factoryClassName";
});
```

Refer to the main [Dynamic Configuration with Programmatic APIs](https://docs.hazelcast.com/hazelcast/latest/configuration/dynamic-config-programmatic-api) for a complete list of options that can be configured, and explainations of their effects. The .NET API closely follows the Java API.