# Hazelcast C# Client

C#/.NET client implementation for [Hazelcast](https://github.com/hazelcast/hazelcast), the open source in-memory data grid. A comparison of features supported by the C# Client vs the Java client can be found [here](http://docs.hazelcast.org/docs/3.5/manual/html/javaclient.html).

C# client is implemented using the [Hazelcast Open Binary Client Protocol](http://hazelcast.org/docs/protocol/1.0-developer-preview/client-protocol.html) 

## Documentation

The docs for the C# client can be reached at [http://docs.hazelcast.org/docs/latest/manual/html/csharpclient.html](http://docs.hazelcast.org/docs/latest/manual/html/csharpclient.html)

## How to build

Hazelcast C# Client is developed using VS2013 Community Edition, which can be [downloaded](https://www.visualstudio.com/en-us/news/vs2013-community-vs.aspx) for free from Microsoft.

## How to run tests

All the tests use NUnit, and require a hazelcast.jar and JVM to run the hazelcast instance. The script `build.bat` will attempt to download hazelcast.jar for the latest snapshot from Maven Central and will run the tests using the downloaded jar. 

## Mail Group

Please join the mail group if you are interested in using or developing Hazelcast.

[http://groups.google.com/group/hazelcast](http://groups.google.com/group/hazelcast)

### License

Hazelcast is available under the Apache 2 License. Please see the [Licensing appendix](http://docs.hazelcast.org/docs/latest/manual/html-single/hazelcast-documentation.html#license-questions) for more information.

### Copyright

Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.

Visit [www.hazelcast.com](http://www.hazelcast.com/) for more info.