<p align="center">
    <img src="./doc/images/hazelcast-black.png" />
    <h2 align="center">.NET Client</h2>
</p>

Hazelcast .NET brings the full power of the [Hazelcast](https://hazelcast.com) high-performance, in-memory computing platform to the Microsoft .NET ecosystem. The 
[Hazelcast .NET Client](https://hazelcast.com/clients/dotnet/) allows you to elastically scale your .NET caches at high read speeds, to access all of Hazelcast data structures such as distributed maps, queues, topics and more. All, with enterprise level security through SSL and mutual authentication.

Documentation for the client is provided on the [documentation site](http://hazelcast.github.io/hazelcast-csharp-client/), with examples, guides and FAQs, and a complete reference documentation for the public API.

The .NET client itself is distributed via NuGet as a package named [Hazelcast.NET](https://www.nuget.org/packages/Hazelcast.Net/). It can be installed like any other NuGet package, either via the Visual Studio GUI, or via the package manager. Note that Hazelcast is composed of two parts: the server, and the client. Browse to [Hazelcast In-Memory Computing](https://hazelcast.com/products/in-memory-computing/) to find out how to obtain and run the server part.

The Hazelcast .NET solution is Open Source, released under the [Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0), and welcomes contributions. The project lives here on [GitHub](https://github.com/hazelcast/hazelcast-csharp-client), where you can obtain the source code, report issues, and interract with the community. Contributions are welcome!


## Extension Packages
In addition to the core `Hazelcast.Net` client package, there are several extension packages that provide additional functionality. These include:

- `Hazelcast.Net.DependencyInjection`: Provides integration with Microsoft's Dependency Injection framework which is also used with ASP.NET.
- `Hazelcast.Net.Caching`: Adds caching capabilities to the Hazelcast .NET client by implementing `IDistributedCache`.
- `Hazelcast.Net.Linq.Async`: Enables LINQ support for asynchronous map querying with Hazelcast.

## Versions

Browse to [this page](http://hazelcast.github.io/hazelcast-csharp-client/versions.html) for details about versions.

See [this branch](https://github.com/hazelcast/hazelcast-csharp-client/tree/3.12.z) for more information about version 3 of the client.

## Code Samples

Check the [Hazelcast.Net.Examples](https://github.com/hazelcast/hazelcast-csharp-client/tree/master/src/Hazelcast.Net.Examples) project.

Here is a simple example that connects to a Hazelcast server running on localhost, puts and gets a value from a distributed map.
```csharp 
var options = new HazelcastOptionsBuilder()
    .With(o=> o.Networking.Addresses.Add("127.0.0.1:5701")) 
    .Build();

// create an Hazelcast client and connect to a server running on localhost
await using var client = await HazelcastClientFactory.StartNewClientAsync(options);

// get the distributed map from the cluster
await using var map = await client.GetMapAsync<string, string>("simple-example");

await map.PutAsync("my-key", "my-value");

var value =  await map.GetAsync("my-key");
```

## Contributing

We encourage any type of contribution in the form of issue reports or pull requests.

### Issue Reports

For issue reports, please share the following information with us to quickly resolve the problems.

* Hazelcast IMDG and the client version that you use
* General information about the environment and the architecture you use like Node.js version, cluster size, number of clients, Java version, JVM parameters, operating system etc.
* Logs and stack traces, if any.
* Detailed description of the steps to reproduce the issue.

### Pull Requests

Contributions are submitted, reviewed and accepted using the pull requests on GitHub. For an enhancement or larger
feature, create a GitHub issue first to discuss.

## Development

Development for future versions takes place in the `master` branch (this branch). The [documentation site](http://hazelcast.github.io/hazelcast-csharp-client/) contains details about building, testing and running the code.

## License

[Apache 2.0 License](LICENSE).

## Copyright

Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.

Visit [www.hazelcast.com](http://www.hazelcast.com) for more information.