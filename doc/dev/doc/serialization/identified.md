# IdentifiedDataSerializable Serialization

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
