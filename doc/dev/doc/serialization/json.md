# HazelcastJsonValue Serialization

You can use the JSON formatted strings as objects in Hazelcast cluster. Starting with Hazelcast IMDG 3.12, the JSON serialization is one of the formerly supported serialization methods. Creating JSON objects in the cluster does not require any server side coding and hence you can just send a JSON formatted string object to the cluster and query these objects by fields.

In order to use JSON serialization, you should use the `HazelcastJsonValue` object for the key or value. Here is an example IMap usage:

```csharp
var map = await client.GetMapAsync<string, HazelcastJsonValue>("map");
```

We constructed a map in the cluster which has `string` as the key and `HazelcastJsonValue` as the value. `HazelcastJsonValue` is a simple wrapper and identifier for the JSON formatted strings. You can get the JSON string from the `HazelcastJsonValue` object by using the `ToString()` method. 

You can construct a `HazelcastJsonValue` using the `HazelcastJsonValue(string jsonString)` constructor. No JSON parsing is performed but it is your responsibility to provide correctly formatted JSON strings. The client will not validate the string, and it will send it to the cluster as it is. If you submit incorrectly formatted JSON strings and, later, if you query those objects, it is highly possible that you will get formatting errors since the server will fail to deserialize or find the query fields.

Here is an example of how you can construct a `HazelcastJsonValue` and put to the map:

```csharp
await map.PutAsync("item1", new HazelcastJsonValue("{ \"age\": 4 }"));
await map.PutAsync("item2", new HazelcastJsonValue("{ \"age\": 20 }"));
```

You can query JSON objects in the cluster using the `Predicate`s of your choice. An example JSON query for querying the values whose age is less than 6 is shown below:

```csharp
    // Get the objects whose age is less than 6
    var result = await map.GetValues(Predicates.IsLessThan("age", 6));

    Console.WriteLine($"Retrieved {result.Count} values whose age is less than 6.");
    Console.WriteLine($"Entry is: {result.First()}");
```
