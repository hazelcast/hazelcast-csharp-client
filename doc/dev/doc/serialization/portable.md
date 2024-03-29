# Portable Serialization

As an alternative to the existing serialization methods, Hazelcast offers portable serialization. To use it, you need to implement the `IPortable` interface. Portable serialization has the following advantages:

- Supporting multiversion of the same object type.
- Fetching individual fields without having to rely on the reflection.
- Querying and indexing support without deserialization and/or reflection.

In order to support these features, a serialized `IPortable` object contains meta information like the version and concrete location of the each field in the binary data. This way Hazelcast is able to navigate in the binary data and deserialize only the required field without actually deserializing the whole object which improves the query performance.

With multiversion support, you can have two members where each of them having different versions of the same object, and Hazelcast will store both meta information and use the correct one to serialize and deserialize portable objects depending on the member. This is very helpful when you are doing a rolling upgrade without shutting down the cluster.

Also note that portable serialization is totally language independent and is used as the binary protocol between Hazelcast server and clients.

A sample portable implementation of a `Customer` class looks like the following:

```csharp
public class Customer : IPortable
{
    public const int ClassId = 1;

    public string Name { get; set; }
    public int Id { get; set; }
    public DateTime LastOrder { get; set; }

    int IPortable.FactoryId => SamplePortableFactory.FactoryId;
    int IPortable.ClassId => ClassId;

    public void WritePortable(IPortableWriter writer)
    {
        writer.WriteInt("id", Id);
        writer.WriteString("name", Name);
        writer.WriteLong("lastOrder", LastOrder.ToFileTimeUtc());
    }

    public void ReadPortable(IPortableReader reader)
    {
        Id = reader.ReadInt("id");
        Name = reader.ReadString("name");
        LastOrder = DateTime.FromFileTimeUtc(reader.ReadLong("lastOrder"));
    }
}
```

Similar to `IIdentifiedDataSerializable`, a Portable object must provide a `ClassId` and a `FactoryId` properties. The factory object will be used to create the Portable object given the classId.

A sample `IPortableFactory` could be implemented as following:

```csharp
public class SamplePortableFactory : IPortableFactory
{
    public const int FactoryId = 1;

    public IPortable Create(int classId)
    {
        if (classId == Customer.ClassId)  return new Customer();
        return null;
    }
}
```

The last step is to register the `IPortableFactory` to the `SerializationOptions`.

**Programmatic Configuration:**
```c#
var hazelcastOptions = new HazelcastOptionsBuilder().Build();
var factory = new SamplePortableFactory();
hazelcastOptions.Serialization
    .AddPortableFactory(SamplePortableFactory.FactoryId, factory);
```

**Declarative Configuration:**
```json
{
    "hazelcast": {
        "serialization": {
            "portableFactories": [
                {
                    "id": 1,
                    "typeName": "SamplePortableFactory"
                }
            ]
        }
    }
}
```

Note that the identifier that is passed to the `SerializationConfig` is same as the value of the `FactoryId` of the `Customer` class.
