# Distributed Computing

This chapter explains how you can use Hazelcast IMDG's entry processor implementation in the .NET client.

## Using EntryProcessor

Hazelcast supports entry processing. An entry processor is a function that executes your code on a map entry in an atomic way.

An entry processor is a good option if you perform bulk processing on an `IMap`. Usually you perform a loop of keys -- executing `IMap.Get(key)`, mutating the value and finally putting the entry back in the map using `IMap.Put(key,value)`. If you perform this process from a client or from a member where the keys do not exist, you effectively perform two network hops for each update: the first to retrieve the data and the second to update the mutated value.

If you are doing the process described above, you should consider using entry processors. An entry processor executes a read and updates upon the member where the data resides. This eliminates the costly network hops described above.

> **NOTE: Entry processor is meant to process a single entry per call. Processing multiple entries and data structures in an entry processor is not supported as it may result in deadlocks on the server side.**

Hazelcast sends the entry processor to each cluster member and these members apply it to the map entries. Therefore, if you add more members, your processing completes faster.

## Processing Entries

The `IMap` interface provides the following functions for entry processing:

* `executeOnKey` processes an entry mapped by a key.
* `executeOnKeys` processes entries mapped by a list of keys.
* `executeOnEntries` can process all entries in a map with a defined predicate. Predicate is optional.

In the .NET client, an `EntryProcessor` should be `IIdentifiedDataSerializable` or `IPortable` because the server should be able to deserialize it to process.

The following is an example for `EntryProcessor` which is `IIdentifiedDataSerializable`.

```c#
public class IdentifiedEntryProcessor : IEntryProcessor, IIdentifiedDataSerializable
{
    internal const int ClassId = 1;

    private string value;

    public IdentifiedEntryProcessor(string value = null)
    {
        this.value = value;
    }

    public void ReadData(IObjectDataInput input)
    {
        value = input.ReadUTF();
    }

    public void WriteData(IObjectDataOutput output)
    {
        output.WriteUTF(value);
    }

    public int GetFactoryId()
    {
        return IdentifiedFactory.FactoryId;
    }

    public int GetId()
    {
        return ClassId;
    }
}
```

Now, you need to make sure that the Hazelcast member recognizes the entry processor. For this, you need to implement the Java equivalent of your entry processor and its factory, and create your own compiled class or JAR files. For adding your own compiled class or JAR files to the server's `CLASSPATH`, see the [Adding User Library to CLASSPATH section](#1212-adding-user-library-to-classpath).

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

After the above implementations and configuration are done and you start the server where your library is added to its `CLASSPATH`, you can use the entry processor in the `IMap` functions. See the following example.

```c#
map.ExecuteOnKey("key", new IdentifiedEntryProcessor("processed"));
Console.WriteLine("Value for key is : {0}", map.Get("key"));
//Output:
//Value for key is : processed
```