# LINQ Provider 
> [!WARNING]
> LINQ support is currently in BETA version. There may be breaking changes on further releases.

Hazelcast .Net Client provides a LINQ provider over `IHMap`. Provider is currently in BETA version.
You can use programmatic LINQ functions instead string SQL statements to query over your distributed map.
To benefit from LINQ support, `Hazelcast.Net.Linq.Async` package should be added your dependency. The package is an
extension of `Hazelcast.Net`. It depends on it. The provider uses Hazelcast .Net Client underneath. Both packages are in NuGet. 

#### Supported LINQ Operations

- Where
- Select

#### Remarks
Linq provider translates your expression to SQL statements, and send it to server via SQL Service of the Client. 
It requires the same steps as SQL. The map should be mapped on the server side, and your property names should match 
with configured column names on mapping. For primitive types `__key` and `this` keywords will be used. For complex types,
property name will be used as it is. Also, note that properties should be publicly-settable. Otherwise, the result object cannot be
reconstructed. To reach the provider, `AsAsyncQueryable()` should be invoked. You can async enumerate over the query object. `ToXXXAsync()`
extensions are not supported at the moment.

[More details about mapping.](https://docs.hazelcast.com/hazelcast/latest/sql/mapping-to-maps)

#### Example
```csharp
var map2 = await client.GetMapAsync<int, string>("simpleMap");

var query = map2.AsAsyncQueryable() // Access to LINQ provider of the map.
                .Where(p => p.Key > 10); // Query entries by key is bigger than 10.

await foreach (var entry in query)
      Console.WriteLine($"Key: {entry.Key}, Value: {entry.Value}");
      
// The SQL statement that will be produced for the query above.      
// SELECT m0.__key, m0.this FROM simpleMap m0 WHERE (m0.__key > ?)"
```
Here, `AsAsyncQueryable()` extension method comes with `Hazelcast.Net.Linq.Async`, and it returns the LINQ provider.
You can add your queries over `query` object. You can execute and consume the query result with `await foreach`. 
In this context, we did not project over the original type. So, the `entry` will be `HKeyValuePair` struct. 
You can reach key and value of the entry. 

Execution and data fetching will be invoked when enumeration is started. The provider generate the query and execute it 
with client's configuration.

>__Note:__ In future, we are planning to have options that can configure LINQ provider, such as cursor size of a SQL query or naming convention of properties.

Please, visit for other examples to `Hazelcast.Net.Examples` on [GitHub](https://github.com/hazelcast/hazelcast-csharp-client/tree/master/src/Hazelcast.Net.Examples/Sql). 






