# Table of Contents

* [Building the Project](#building-the-project)
  * [Strong Name Generation](#strong-name-generation)
* [Running the Tests](#running-the-tests)
* [Features](#features)
* [Sample Code](#sample-code)
* [Configuring and Starting](#configuring-and-starting)
* [Mail Group](#mail-group)
* [License](#license)
* [Copyright](#copyright)



This is the repository of C#/.NET client implementation for [Hazelcast](https://github.com/hazelcast/hazelcast), the open source in-memory data grid. A comparison of features supported by the C# Client vs. the Java client can be found [here](http://docs.hazelcast.org/docs/latest/manual/html-single/index.html#hazelcast-clients-feature-comparison).

C# client is implemented using the [Hazelcast Open Binary Client Protocol](http://hazelcast.org/docs/protocol/1.0-developer-preview/client-protocol.html) 


# Building the Project

Hazelcast C# Client is developed using VS2013 Community Edition, which can be [downloaded](https://www.visualstudio.com/en-us/news/vs2013-community-vs.aspx) for free from Microsoft.

## Strong Name Generation

Hazelcast assemblies are signed using a [strong name key](https://msdn.microsoft.com/en-us/library/wd40t7ad.aspx). To be able to build the project, you will need to 
create your own strong name key.

This can be done using the sn.exe tools which ships with .NET framework.

    sn -k hazelcast.snk

Furthermore, you will need to update `Hazelcast.Net/Properties/AssemblyInfo.cs` with the new public key. 

    [assembly: System.Runtime.CompilerServices.InternalsVisibleTo(@"Hazelcast.Test, PublicKey=00240000049e....b3")]

To get the new public key, use the following commands:

    sn -p hazelcast.snk hazelcast.key
    sn -tp hazelcast.key

# Running the Tests

All the tests use NUnit, and require a hazelcast.jar and JVM to run the hazelcast instance. The script `build.bat` will attempt to download hazelcast.jar for the latest snapshot from Maven Central and will run the tests using the downloaded jar. 


# Features

You can use the native .NET client to connect to Hazelcast client members. You need to add `HazelcastClient3x.dll` into your .NET project references. The API is very similar to the Java native client. 

.NET Client has the following distributed objects.

* `IMap<K,V>`
* `IMultiMap<K,V>`
* `IQueue<E>`
* `ITopic<E>`
* `IHList<E>`
* `IHSet<E>`
* `IIdGenerator`
* `ILock`
* `ISemaphore`
* `ICountDownLatch`
* `IAtomicLong`
* `ITransactionContext`
* `IRingbuffer`
	
ITransactionContext can be used to obtain:

* `ITransactionalMap<K,V>`
* `ITransactionalMultiMap<K,V>`
* `ITransactionalList<E>`
* `ITransactionalSet<E>`
* `ITransactionalQueue<E>`

At present the following features are not available in the .NET Client as they are in the Java Client:

* Distributed Executor Service
* Replicated Map
* JCache

# Sample Code

A code example is shown below.

```csharp
using Hazelcast.Config;
using Hazelcast.Client;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;

using System.Collections.Generic;

namespace Hazelcast.Client.Example
{
  public class SimpleExample
  {

    public static void Test()
    {
      var clientConfig = new ClientConfig();
      clientConfig.GetNetworkConfig().AddAddress( "10.0.0.1" );
      clientConfig.GetNetworkConfig().AddAddress( "10.0.0.2:5702" );

      // Portable Serialization setup up for Customer Class
      clientConfig.GetSerializationConfig()
          .AddPortableFactory( MyPortableFactory.FactoryId, new MyPortableFactory() );

      IHazelcastInstance client = HazelcastClient.NewHazelcastClient( clientConfig );
      // All cluster operations that you can do with ordinary HazelcastInstance
      IMap<string, Customer> mapCustomers = client.GetMap<string, Customer>( "customers" );
      mapCustomers.Put( "1", new Customer( "Joe", "Smith" ) );
      mapCustomers.Put( "2", new Customer( "Ali", "Selam" ) );
      mapCustomers.Put( "3", new Customer( "Avi", "Noyan" ) );

      ICollection<Customer> customers = mapCustomers.Values();
      foreach (var customer in customers)
      {
        //process customer
      }
    }
  }

  public class MyPortableFactory : IPortableFactory
  {
    public const int FactoryId = 1;

    public IPortable Create( int classId ) {
      if ( Customer.Id == classId )
        return new Customer();
      else
        return null;
    }
  }

  public class Customer : IPortable
  {
    private string name;
    private string surname;

    public const int Id = 5;

    public Customer( string name, string surname )
    {
      this.name = name;
      this.surname = surname;
    }

    public Customer() {}

    public int GetFactoryId()
    {
      return MyPortableFactory.FactoryId;
    }

    public int GetClassId()
    {
      return Id;
    }

    public void WritePortable( IPortableWriter writer )
    {
      writer.WriteUTF( "n", name );
      writer.WriteUTF( "s", surname );
    }

    public void ReadPortable( IPortableReader reader )
    {
      name = reader.ReadUTF( "n" );
      surname = reader.ReadUTF( "s" );
    }
  }
}
```


# Configuring and Starting

You can configure the Hazelcast .NET client via API or XML. To start the client, you can pass a configuration or leave it empty to use default values.

*NOTE: .NET and Java clients are similar in terms of configuration. Therefore, you can refer to [Java Client](#hazelcast-java-client) section for configuration aspects. Please also refer to the .NET API documentation.*

After configuration, you can obtain a client using one of the static methods of Hazelcast, as shown below.


```csharp
IHazelcastInstance client = HazelcastClient.NewHazelcastClient(clientConfig);

...


IHazelcastInstance defaultClient = HazelcastClient.NewHazelcastClient();

...

IHazelcastInstance xmlConfClient = Hazelcast
    .NewHazelcastClient(@"..\Hazelcast.Net\Resources\hazelcast-client.xml");
```

The `IHazelcastInstance` interface is the starting point where all distributed objects can be obtained.

```csharp
var map = client.GetMap<int,string>("mapName");

...

var lock= client.GetLock("thelock");
```


# Mail Group

Please join the mail group if you are interested in using or developing Hazelcast: 

[http://groups.google.com/group/hazelcast](http://groups.google.com/group/hazelcast)

# License

Hazelcast is available under the Apache 2 License. Please see the [Licensing appendix](http://docs.hazelcast.org/docs/latest/manual/html-single/hazelcast-documentation.html#license-questions) for more information.

# Copyright

Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.

Visit [www.hazelcast.com](http://www.hazelcast.com/) for more information.
