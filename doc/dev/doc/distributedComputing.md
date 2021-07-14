# Distributed Computing

This chapter explains how you can use Hazelcast IMDG's entry processor implementation in the .NET client.

## Using EntryProcessor

Hazelcast supports entry processing. An entry processor is a function that executes your code on a map entry in an atomic way.

An entry processor is a good option if you perform bulk processing on an `IHMap`. Usually you perform a loop of keys - executing `IHMap.GetAsync(key)`, mutating the value and finally putting the entry back in the map using `IHMap.PutAsync(key,value)`. If you perform this process from a client or from a member where the keys do not exist, you effectively perform two network hops for each update: the first to retrieve the data and the second to update the mutated value.

If you are doing the process described above, you should consider using entry processors. An entry processor executes a read and updates upon the member where the data resides. This eliminates the costly network hops described above.

> **NOTE: Entry processor is meant to process a single entry per call. Processing multiple entries and data structures in an entry processor is not supported as it may result in deadlocks on the server side.**

Hazelcast sends the entry processor to each cluster member and these members apply it to the map entries. Therefore, if you add more members, your processing completes faster.

## Processing Entries

The `Hazelcast.DistributedObjects.IHMap` interface provides the following functions for entry processing:

* `ExecuteAsync<T>(IEntryProcessor<T>, TKey)` processes an entry mapped by a key.
* `ExecuteAsync<T>(IEntryProcessor<T>, IEnumerable<TKey>)` processes entries mapped by a list of keys.
* `ExecuteAsync<T>(IEntryProcessor<T>, IPredicate)` processes all entries in a map with a defined predicate.
* `ExecuteAsync<T>(IEntryProcessor<T>)` processes all entries in a map.

In the .NET client, an `IEntryProcessor` should be `IIdentifiedDataSerializable` or `IPortable` because the server should be able to deserialize it to process.

The following is an example for `IEntryProcessor` which is `IIdentifiedDataSerializable`.

```csharp
public class IdentifiedEntryProcessor : IEntryProcessor<string>, IIdentifiedDataSerializable
{
    public const int FactoryIdConst = 5; // Id of corresponding IDataSerializableFactory
    public const int ClassIdConst = 1; // corresponds to Java's IdentifiedEntryProcessor.CLASS_ID

    public int FactoryId => FactoryIdConst;
    public int ClassId => ClassIdConst;

    private string _value;

    public IdentifiedEntryProcessor(string value)
    {
        _value = value;
    }

    public void ReadData(IObjectDataInput input)
    {
        _value = input.ReadString();
    }

    public void WriteData(IObjectDataOutput output)
    {
        output.WriteString(_value);
    }
}
```

Now, you need to make sure that the Hazelcast member recognizes the entry processor. For this, you need to implement the Java equivalent of your entry processor and its factory, and create your own compiled class or JAR files. For adding your own compiled class or JAR files to the server's `CLASSPATH`, see [Adding User Library to CLASSPATH](https://docs.hazelcast.com/imdg/latest/clusters/deploying-code-from-clients.html#adding-user-library-to-classpath).

The following is the Java equivalent of the entry processor in .NET client given above:

```java
import com.hazelcast.map.AbstractEntryProcessor;
import com.hazelcast.nio.ObjectDataInput;
import com.hazelcast.nio.ObjectDataOutput;
import com.hazelcast.nio.serialization.IdentifiedDataSerializable;
import java.io.IOException;
import java.util.Map;

public class IdentifiedEntryProcessor extends AbstractEntryProcessor<String, String> implements IdentifiedDataSerializable {
     static final int CLASS_ID = 1;
     private String value;

    public IdentifiedEntryProcessor() {
    }

     @Override
    public int getFactoryId() {
        return IdentifiedFactory.FACTORY_ID;
    }

     @Override
    public int getId() {
        return CLASS_ID;
    }

     @Override
    public void writeData(ObjectDataOutput out) throws IOException {
        out.writeUTF(value);
    }

     @Override
    public void readData(ObjectDataInput in) throws IOException {
        value = in.readUTF();
    }

     @Override
    public Object process(Map.Entry<String, String> entry) {
        entry.setValue(value);
        return value;
    }
}
```

You can implement the above processor’s factory as follows:

```java
import com.hazelcast.nio.serialization.DataSerializableFactory;
import com.hazelcast.nio.serialization.IdentifiedDataSerializable;

public class IdentifiedFactory implements DataSerializableFactory {
    public static final int FACTORY_ID = 5;

     @Override
    public IdentifiedDataSerializable create(int typeId) {
        if (typeId == IdentifiedEntryProcessor.CLASS_ID) {
            return new IdentifiedEntryProcessor();
        }
        return null;
    }
}
```

Now you need to configure the `hazelcast.xml` to add your factory as shown below.

```xml
<hazelcast>
    <serialization>
        <data-serializable-factories>
            <data-serializable-factory factory-id="5">
                IdentifiedFactory
            </data-serializable-factory>
        </data-serializable-factories>
    </serialization>
</hazelcast>
```

The code that runs on the entries is implemented in Java on the server side. The client side entry processor is used to specify which entry processor should be called. For more details about the Java implementation of the entry processor, see the [Entry Processor section](https://docs.hazelcast.org/docs/latest/manual/html-single/index.html#entry-processor) in the Hazelcast IMDG Reference Manual.

After the above implementations and configuration are done and you start the server where your library is added to its `CLASSPATH`, you can use the entry processor in the `IHMap` functions. See the following example.

```csharp
var map = await client.GetMapAsync<string, string>("processing-map");
await map.ExecuteAsync(new IdentifiedEntryProcessor("processed"), "key");
Console.WriteLine($"Value for key is: {await map.GetAsync("key")}");
//Output:
//Value for key is: processed
```