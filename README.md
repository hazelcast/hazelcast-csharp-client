# Hazelcast C# Client

C#/.NET client implementation for [Hazelcast](https://github.com/hazelcast/hazelcast), the open source in-memory data grid. A comparison of features supported by the C# Client vs the Java client can be found [here](http://docs.hazelcast.org/docs/3.5/manual/html/javaclient.html).

C# client is implemented using the [Hazelcast Open Binary Client Protocol](http://hazelcast.org/docs/protocol/1.0-developer-preview/client-protocol.html) 

## Getting Started

### Installation

You can install .net client from [NuGet Repo](https://www.nuget.org/packages/Hazelcast.Net/) using PackageManager:

```
PM> Install-Package Hazelcast.Net
```

### Configuration
You can configure the Hazelcast .Net Client via API or XML. To start the client, you can pass a configuration or leave it empty to use default values.

For programatic configuration; please see `Hazelcast.Config` packege and related API doc. 

For XML configuration; .Net Client uses the same [XML schema](http://www.hazelcast.com/schema/client-config) of Hazelcast Java Client.

Sample client configuration:

```cs
var clientConfig = new ClientConfig();
clientConfig.GetNetworkConfig().AddAddress( "10.0.0.1" );
clientConfig.GetNetworkConfig().AddAddress( "10.0.0.2:5702" );

// Portable Serialization setup up 
clientConfig.GetSerializationConfig()
  .AddPortableFactory( MyPortableFactory.FactoryId, new MyPortableFactory() );

```

### Starting the Client
After configuration, you can obtain a client using one of the static methods of Hazelcast, as shown below.

```cs
//client with default configuration
IHazelcastInstance defaultClient = HazelcastClient.NewHazelcastClient();

//client with custom configuration
IHazelcastInstance client = HazelcastClient.NewHazelcastClient(clientConfig);

//client configured from an xml configuration file 
IHazelcastInstance xmlConfClient = Hazelcast.NewHazelcastClient(@"..\Hazelcast.Net\Resources\hazelcast-client.xml");
```

### Basic Usage

All distributed objects can be obtained from client `IHazelcastInstance`. An example for map is shown below:

```cs
var map = client.GetMap<string,string>("mapName");
map.Put("key", "value");

```
Please see `Hazelcast.Examples` project on this repo for various code samples.

## Advanced Topics

Please refer to [Hazelcast Reference Manual](http://docs.hazelcast.org/docs/latest/manual/html-single/index.html).

## How to build

You can build the source by calling the batch file `build.bat`.

### Strong name generation

Hazelcast assemblies are signed using a [strong name key](https://msdn.microsoft.com/en-us/library/wd40t7ad.aspx). To be able to build the project, you will need to 
create your own strong name key.

This can be done using the sn.exe tool which ships with .NET framework.

```
sn -k hazelcast.snk
```

Furthermore, you will need to update `Hazelcast.Net/Properties/AssemblyInfo.cs` with the new public key. 

```
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo(@"Hazelcast.Test, PublicKey=00240000049e....b3")]
```

To get the new public key, use the following commands:

```
sn -p hazelcast.snk hazelcast.key
sn -tp hazelcast.key
```

## How to run tests

All the tests use NUnit, and require a hazelcast.jar and JVM to run the hazelcast instance. The script `build.bat` will attempt to download hazelcast.jar for the latest snapshot from Maven Central and will run the tests using the downloaded jar. 

## Mail Group

Please join the mail group if you are interested in using or developing Hazelcast.

[http://groups.google.com/group/hazelcast](http://groups.google.com/group/hazelcast)

### License

Hazelcast is available under the Apache 2 License. Please see the [Licensing appendix](http://docs.hazelcast.org/docs/latest/manual/html-single/hazelcast-documentation.html#license-questions) for more information.

### Copyright

Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.

Visit [www.hazelcast.com](http://www.hazelcast.com/) for more info.
