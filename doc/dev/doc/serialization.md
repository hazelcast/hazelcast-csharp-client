# Serialization

For data to be sent over a network between cluster members and/or clients, it needs to be serialized into raw bytes. Hazelcast has many serialization options to choose from, depending on what you plan on doing with your data.

You can store any primitive types in a Hazelcast data structure and Hazelcast will serialize them for you, using built-in serializers. But, to store custom classes or objects, you need to tell a cluster how to serialize and deserialize them when they are sent over the network.

Hazelcast supports the following serialization options:

* [IdentifiedDataSerializable](serialization/identified.md) serialization is supported by all clients. It is an Hazelcast-specific method that require that a specific interface be implemented.

* [Portable](serialization/portable.md) serialization is supported by all clients. It is an Hazelcast-specific method that require that a specific interface be implemented. Versionning and partial deserialization are supported. Class definitions are also sent with data, but store only once per class.

* [Custom](serialization/custom.md) serialization is supported by all clients. It does not require classes to implement a specific interface and is the most flexible method. However, it requires more work to implement.

* [Compact](serialization/compact.md) serialization (introduced in version 5.2) is a schema-based Hazelcast-specific method that can handle plain POCOs (no need to implement a specific interface), supports schema evolution and partial deserialization. It is supported by all clients. Class definitions (schemas) are not part of the data but distribued between clients and clusters.

* [HazelcastJsonValue](serialization/json.md) serialization is supported by all clients. It is an Hazelcas-specific method that requires no member-side coding, but requires extra metadata to be stored on members.

## Serialization Priority

When Hazelcast .NET client serializes an object:

* If the object is `null`, use the internal `null` serializer, else
* If a compact serializer has been register for the type of the object, use [Compact](serialization/compact.md) serialization, else
* If the object implements the `Hazelcast.Serialization.IIdentifiedDataSerializable` interface, use [IdentifiedDataSerializable](serialization/identified.md), else
* If the object implements the `Hazelcast.Serialization.IPortable` interface, use [Portable](serialization/portable.md) serialization.
* If the object type is one of the default built-in types (see below), use the corresponding serializer, else
* Look for a [Custom](serialization/custom.md) serializer, i.e., an implementation of `IByteArraySerializer<T>` or `IStreamSerializer<T>`. Custom serializer is searched using the input object’s class and its parent class up to `Object`. If parent class search fails, all interfaces implemented by the class are also checked (note that the *order* in which these interfaces are checked is not specified).
* If the object is Serializable ( `Type.IsSerializable` ) and a Global Serializer is not registered with CLR serialization Override feature, serialize via .NET `BinaryFormatter` (see note below)
* If a Global serializer (see below) has been registered, use it, else
* Use [Compact](serialization/compact.md) serialization reflection-based mode.

Note that, at the moment, there is no built-in automatic support for `IEnumerable<T>` or `T[]` beyond the default types documented above.

## .NET Binary Formatter Serialization

For backward-compatibility reasons, .NET `BinaryFormatter` serialization is enabled by default. However, it is now considered obsolete by Microsoft that issued the following warning:

> [!WARNING]
> The `BinaryFormatter` type is dangerous and is *not* recommended for data processing. Applications should stop using `BinaryFormatter` as soon as possible, even if they believe the data they're processing to be trustworthy. `BinaryFormatter` is insecure and can't be made secure. ([source](https://docs.microsoft.com/en-us/dotnet/standard/serialization/binaryformatter-security-guide))

We therefore recommend that `BinaryFormatter` serialization be entirely disabled. For that purpose, set `SerializationOptions.EnableClrSerialization` to `false`.

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

## Global Serialization

The global serializer is identical to custom serializers from the implementation perspective. It is registered as a fallback serializer to handle all other objects if a serializer cannot be located for them. By default, the global serializer does not handle .NET Serializable instances. However, you can configure it to be responsible for those instances.

When the Global Serialization `OverrideClrSerialization` is set to `true` (it is `false` by default), .NET Serialization serialization is de-activated.

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