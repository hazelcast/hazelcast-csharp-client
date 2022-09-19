# Custom Serialization

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
