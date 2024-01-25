# CPMap

> [!NOTE]
> CPMap is a member of CP Subsystem API. For detailed information, see the [CP SubSystem documentation](../cpsubsystem.md).
> [!WARNING]
> It is only available in Hazelcast Enterprise.

Hazelcast CPMap is a strongly consistent mapping structure under CP Subsystem. It offers linearizability, and supports atomic `CompareAndSetAsync`, `DeleteAsync`, `GetAsync`, `PutAsync`, `RemoveAsync` and `SetAsync` operations. Hazelcast .Net Client also supports these atomic operations, for implementation details, please, see [CPMap documentation](https://docs.hazelcast.com/hazelcast/latest/data-structures/cpmap).

In order to use `CPMap`, CP Subsystem must be enabled on the server side. Moreover, if no Raft group specified while creating the structure, `CPMap` will be created under `default` Raft group.

## Example

The following simple example creates a `CPMap` under `myGroup` Raft group. If the group has not been initialized yet, 
first; the raft group will be created. Then, the structure will be initialized.

```csharp

// create an Hazelcast client and connect to a enterprise server running on localhost
// note that that server should be properly configured for CP with at least 3 members
await using var client = await HazelcastClientFactory.StartNewClientAsync(options);

// Get a CPMap under "myGroup" Raft group.
var map = await client.CPSubsystem.GetMapAsync<int, string>("myMap@myGroup");

var (key, val) = (1, "my-value");

// Set a value
// Note: Set does not return back the old value that is associated with the key. If you require the previous value,
// consider using PutAsync.
await map.SetAsync(key, val);

// Get value that is map to the key.
// If key does not exist, the return value will be null. However, we know that the key-value pair exists, and
// ignore the possible null warning.
var currentVal = await map.GetAsync(key)!;

Console.WriteLine($"Key: {key}, Expected Value: {val}, Actual Value:{currentVal}");

// Let's change the value of the key by using CompareAndSetAsync
// The expected value will be compared to current value which is associated to given key. 
// If they are equal, new value will be set.
var newValue = "my-new-value";
var isSet = await map.CompareAndSetAsync(key, currentVal, newValue);

Console.WriteLine($"Key: {key}, Expected Value: {currentVal}, New Value:{newValue}, Is set successfully done:{isSet}");


// Assume that we do not need the map anymore. So, it is better to destroy the map on the cluster and release the resources.
// Note that Hazelcast does NOT do garbage collection on CPMap.
await map.DestroyAsync();
```
