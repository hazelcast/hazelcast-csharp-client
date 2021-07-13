# Serialization

Serialization is the process of converting an object into a stream of bytes to store the object in memory, a file or database, or transmit it through network.
Its main purpose is to save the state of an object in order to be able to recreate it when needed. The reverse process is called deserialization.
Hazelcast offers you its own native serialization methods. You will see these methods throughout the section.

## Default Types

Hazelcast serializes all your objects before sending them to the server. The built-in primitive types are serialized natively and you cannot override this behavior. The following table is the conversion of types for Java server side.

| .NET    | Java      |
|---------|-----------|
| bool    | Boolean   |
| byte    | Byte      |
| char    | Character |
| short   | Short     |
| int     | Integer   |
| long    | Long      |
| float   | Float     |
| double  | Double    |
| string  | String    |

and

|        .NET                |       Java              |
|----------------------------|-------------------------|
| DateTime                   | java.util.Date          |
| System.Numeric.BigInteger  | java.math.BigInteger    |
| Guid                       | java.util.UUID          |


Arrays of the above types can be serialized as `bool[]`, `byte[]`, `short[]`, `int[]`, `long[]`, `float[]`, `double[]`,  `char[]` and `string[]`.

## Serialization Priority

When Hazelcast .NET client serializes an object into `IData`:

1. It first checks whether the object is null.
2. If the above check fails, then Hazelcast checks if it is an instance of `Hazelcast.Serialization.IIdentifiedDataSerializable`.
3. If the above check fails, then Hazelcast checks if it is an instance of `Hazelcast.Serialization.IPortable`.
4. If the above check fails, then Hazelcast checks if it is an instance of one of the default types (see above default types).
5. If the above check fails, then Hazelcast looks for a user-specified Custom Serializer, i.e., an implementation of `IByteArraySerializer<T>` or `IStreamSerializer<T>`. Custom serializer is searched using the input object’s class and its parent class up to `Object`. If parent class search fails, all interfaces implemented by the class are also checked (note that the *order* in which these interfaces are checked is not specified).
6. If the above check fails, then Hazelcast checks if it is Serializable ( `Type.IsSerializable` ) and a Global Serializer is not registered with CLR serialization Override feature.
7. If the above check fails, Hazelcast will use the registered Global Serializer if one exists.

Note that, at the moment, there is no built-in automatic support for `IEnumerable<T>` or `T[]` beyond the default types documented above.

## IdentifiedDataSerializable Serialization

For a faster serialization of objects, Hazelcast recommends to implement the `IdentifiedDataSerializable` interface. The following is an example of an object implementing this interface:

```csharp
public class Employee : IIdentifiedDataSerializable
{
    public const int ClassId = 100;

    public int Id { get; set; }
    public string Name { get; set; }

    public void ReadData(IObjectDataInput input)
    {
        Id = input.ReadInt();
        Name = input.ReadString();
    }

    public void WriteData(IObjectDataOutput output)
    {
        output.WriteInt(Id);
        output.WriteString(Name);
    }

    int IIdentifiedDataSerializable.FactoryId => SampleDataSerializableFactory.FactoryId;
    int IIdentifiedDataSerializable.ClassId => 100;
}
```


IdentifiedDataSerializable uses the `Class` and `FactoryId` properties to reconstitute the object. To complete the implementation `IDataSerializableFactory` should also be implemented and registered into `SerializationOptions`. The factory's responsibility is to return an instance of the right `IIdentifiedDataSerializable` object, given the class identifier.

A sample `IDataSerializableFactory` could be implemented as following:

```csharp
public class SampleDataSerializableFactory : IDataSerializableFactory
{
    public const int FactoryId = 1000;

    public IIdentifiedDataSerializable Create(int typeId)
    {
        if (typeId == Employee.ClassId) return new Employee();
        return null;
    }
}
```

The last step is to register the `IDataSerializableFactory` to the `SerializationOptions`.

**Programmatic Configuration:**
```csharp
var hazelcastOptions = new HazelcastOptionsBuilder().Build();
var factory = new SampleDataSerializableFactory();
hazelcastOptions.Serialization
    .AddDataSerializableFactory(SampleDataSerializableFactory.FactoryId, factory);
```

**Declarative Configuration:**
```json
{
    "hazelcast": {
        "serialization": {
            "dataSerializableFactories": [
                {
                    "id": 1000,
                    "typeName": "SampleDataSerializableFactory"
                }
            ]
        }
    }
}
```

Note that the identifier that is passed to the `SerializationOptions` is same as value of the `FactoryId` of the `Employee` class.

Here (and in all examples below), `typeName` is the fully-qualified CLR type name. Depending on your code, it could be `"MyFactory"` but may have to be `"My.Namespace.Factory"` or even `"My.Namespace.Factory, My.Assembly"`.

## Portable Serialization

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

## Custom Serialization

Hazelcast lets you plug a custom serializer to be used for serialization of objects.

Let's say you have a class `CustomSerializableType` and you would like to customize the serialization, since you may want to use an external serializer for only one class.

```csharp
public class CustomSerializableType
{
    public string Value { get; set; }
}
```

Let's say your custom `CustomSerializer` will serialize `CustomSerializableType`.

```csharp
public class CustomSerializer : IStreamSerializer<CustomSerializableType>
{
    public const int TypeId = 10;

    public void Write(IObjectDataOutput output, CustomSerializableType t)
    {
        var array = Encoding.UTF8.GetBytes(t.Value);
        output.WriteInt(array.Length);
        output.Write(array);
    }

    public CustomSerializableType Read(IObjectDataInput input)
    {
        var len = input.ReadInt();
        var array = new byte[len];
        input.Read(array, 0, array.Length);
        return new CustomSerializableType { Value = Encoding.UTF8.GetString(array) };
    }

    int ISerializer.TypeId => TypeId;

    public void Dispose()
    { }
}
```

Note that the serializer `TypeId` must be unique as Hazelcast will use it to lookup the `CustomSerializer` while it deserializes the object.
Now the last required step is to register the `CustomSerializer` to the configuration.

**Programmatic Configuration:**

```c#
var hazelcastOptions = new HazelcastOptionsBuilder().Build();
hazelcastOptions.Serialization.Serializers.Add(
    new SerializerOptions {
        SerializedType = typeof(CustomSerializableType),
        Creator = () => new CustomSerializer()
    }
);
```

**Declarative Configuration:**
```json
{
    "hazelcast": {
        "serialization": {
            "serializers": [
                {
                    "serializedTypeName": "CustomSerializableType",
                    "typeName": "CustomSerializer"
                }
            ]
        }
    }
}
```

From now on, Hazelcast will use `CustomSerializer` to serialize `CustomSerializableType` objects.

## JSON Serialization

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

## Global Serialization

The global serializer is identical to custom serializers from the implementation perspective. It is registered as a fallback serializer to handle all other objects if a serializer cannot be located for them. By default, the global serializer does not handle .NET Serializable instances. However, you can configure it to be responsible for those instances.

A custom serializer should be registered for a specific class type. The global serializer will handle all class types if all the steps in searching for a serializer, as described previously, fail.

**Use cases**

- Third party serialization frameworks can be integrated using the global serializer.
- For your custom objects, you can implement a single serializer to handle all of them.

A sample global serializer that integrates with a third party serializer is shown below.

```csharp
public class GlobalSerializer : IStreamSerializer<object>
{
    public const int TypeId = 20;

    public void Write(IObjectDataOutput output, object obj)
    {
        output.write(MyFavoriteSerializer.Serialize(obj))
    }

    public object Read(IObjectDataInput input)
    {
        return MyFavoriteSerializer.Deserialize(input);
    }

    int ISerializer.TypeId => TypeId;
}
```

You should register the global serializer in the configuration.

**Programmatic Configuration:**
```csharp
var hazelcastOptions = new HazelcastOptionsBuilder().Build();
hazelcastOptions.Serialization.GlobalSerializer.Creator =
    () => new GlobalSerializer();
```

**Declarative Configuration:**
```json
{
    "hazelcast": {
        "serialization": {
            "globalSerializer": {
                "serializedTypeName": "CustomSerializableType",
                "overrideClrSerialization": true
            }
        }
    }
}
```