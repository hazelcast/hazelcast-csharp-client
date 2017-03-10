# Hazelcast C# Client

C#/.NET client implementation for [Hazelcast](https://github.com/hazelcast/hazelcast), the open source in-memory data grid. A comparison of features supported by the C# Client vs the Java client can be found [here](http://docs.hazelcast.org/docs/3.5/manual/html/javaclient.html).

C# client is implemented using the [Hazelcast Open Binary Client Protocol](http://hazelcast.org/docs/protocol/1.0-developer-preview/client-protocol.html) 

## Documentation

The docs for the C# client can be reached at [http://docs.hazelcast.org/docs/latest/manual/html-single/index.html#net-client](http://docs.hazelcast.org/docs/latest/manual/html-single/index.html#net-client)

## How to build

Hazelcast C# Client is developed using VS2013 Community Edition, which can be [downloaded](https://www.visualstudio.com/en-us/news/vs2013-community-vs.aspx) for free from Microsoft.

### Strong name generation

Hazelcast assemblies are signed using a [strong name key](https://msdn.microsoft.com/en-us/library/wd40t7ad.aspx). To be able to build the project, you will need to 
create your own strong name key.

This can be done using the sn.exe tools which ships with .NET framework.

    sn -k hazelcast.snk

Furthermore, you will need to update `Hazelcast.Net/Properties/AssemblyInfo.cs` with the new public key. 

    [assembly: System.Runtime.CompilerServices.InternalsVisibleTo(@"Hazelcast.Test, PublicKey=00240000049e....b3")]

To get the new public key, use the following commands:

    sn -p hazelcast.snk hazelcast.key
    sn -tp hazelcast.key

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
