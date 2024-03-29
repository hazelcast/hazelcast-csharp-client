# Getting Started

## Quick Start

#### Walkthrough

Prepare a .NET console project:

```shell
mkdir quickstart
cd quickstart
dotnet new console
dotnet add package Hazelcast.Net
dotnet add package Microsoft.Extensions.Logging.Console
```

Edit the `Program.cs` file as you wish. The code below is a minimal example,
that configures logging to the console, and connects a client to a server running
on localhost.

```csharp
public static async Task Main()
{
    // create options
    var options = new HazelcastOptionsBuilder()
        .WithDefault("Logging:LogLevel:Default", LogLevel.None)
        .WithDefault("Logging:LogLevel:Hazelcast", LogLevel.Information)
        .WithLoggerFactory(configuration => LoggerFactory.Create(builder => builder
            .AddConfiguration(configuration.GetSection("logging"))
            .AddSimpleConsole(consoleOptions => 
            {
                consoleOptions.SingleLine = true;
                consoleOptions.TimestampFormat = "hh:mm:ss.fff ";
            })))
        .Build();

    // create and connect a Hazelcast client to a server running on localhost
    await using var client = await HazelcastClientFactory.StartNewClientAsync(options);
    
    // the client is disposed and thus disconnected on exit
}
```

Run the code with:
```shell
dotnet build
dotnet run
```

You should see the log output in the console.

#### Running Preview

Should you want to use a preview version of the Hazelcast .NET Client, built from source at `path/to/Hazelcast.Net`, drop a `nuget.config` file in the `quickstart` directory containing the following:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="hz" value="path/to/Hazelcast.Net/temp/output" />
  </packageSources>
</configuration>
```

Then, force the installation of the preview version with:
```shell
dotnet add package Hazelcast.Net --version 5.3.0-preview.0
```

You can now run the test program again.

#### Download and Install

Using the published [Hazelcast.Net](https://www.nuget.org/packages/Hazelcast.Net/) package from NuGet is the prefered way to download and install the client. Refer to the [Download and Install](download-install.md) page to learn more about how to download the Hazelcast .NET Client and Server, and how to install it. In addition, this page contains more details about required binding redirects when installing in a **.NET Framework** project.

## Using the client

The Hazelcast client is the entry point to all interactions with an Hazelcast cluster. A client is created by the static @Hazelcast.HazelcastClientFactory. After it has been used, it needs to be disposed in order to properly close all connections to servers, and release resources.

For example:

```csharp
var client = await HazelcastClientFactory.StartNewClientAsync();
// ... use the client ...
await client.DisposeAsync();
```

A client is a heavy enough, multi-threaded object. Although a factory can create several, independent clients, it is recommended to store and reuse the client instance, as much as possible.

Here, the client is configured by default, which means by configuration files and environment variables. For more control, the client can be initialized with an @Hazelcast.HazelcastOptions instance, which represents the complete set of options of the Hazelcast client. In fact, the above example is equivalent to:

```csharp
var options = new HazelcastOptionsBuilder().Build();
var client = await HazelcastClientFactory.StartNewClientAsync(options);
// ...
```

Refer to the [Configuration](configuration.md) page for details on the various ways to build an @Hazelcast.HazelcastOptions instance, including handling command-line parameters, as well as a list of all the configurable elements.

## Distributed Objects

The client can be used to obtain *distributed objects* that are managed by the cluster. For instance, the cluster can manage @Hazelcast.DistributedObjects.IHMap`2 objects, which are an asynchronous equivalent of .NET @System.Collections.Generic.IDictionary`2. Each object is identified by a unique name, which is used to retrieve the object. Finally, distributed objects need to be disposed after usage, to ensure they release their resources.

For example:

```csharp
var map = await client.GetMapAsync<string, string>("map-name");
await map.SetAsync("key", "value");
var value = await map.GetAsync("key");
await map.DisposeAsync();
```

The @Hazelcast.IHazelcastClient.GetMapAsync* method returns the existing object with the specified name, or creates a new object with that name on the cluster. That object will continue to live on the cluster after the @Hazelcast.DistributedObjects.IHMap`2 has been disposed. In order to remove the object from the cluster, one must destroy the object.

For example:

```csharp
var map = await client.GetMapAsync<string, string>("dict-name");
await map.DestroyAsync();
```

or 

```csharp
var map = await client.GetMapAsync<string, string>("dict-name");
await client.DestroyAsync(map);
```

## Examples

Complete, working examples are provided in source form in the [Hazelcast.Net.Examples](https://github.com/hazelcast/hazelcast-csharp-client/tree/master/src/Hazelcast.Net.Examples) project, with instruction in the [Examples](examples.md) page.

## Logging

The Hazelcast .NET client uses the logging abstractions proposed by the [Microsoft.Extensions.Logging](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging) namespace. By default, the client supports the abstractions, but does not come with any actual implementation. This means that, by default, the client will not output any log information. To actually log, an implementation must be added to the project.

See the [Logging](logging.md) documentation for details.

## Events

The client exposes client-level events.

For example:

```csharp
var subscriptionId = await client.SubscribeAsync(events => events
    .StateChanged((sender, args) => {
        Console.WriteLine($"Client state changed to: {args.State}.")
    })
);

// ... handle events ...

var success = await client.UnsubscribeAsync(subscriptionId);
```

The `(sender, args)` pattern is used to remain consistent with C# events. Here, `sender` is the object that triggered the event, i.e. `client`, and `args` contains the event data.

> Note: pure C# events (`client.StateChanged += ...`) cannot be used here, as subscribing, un-subscribing and handling events all need to support being asynchronous.

Each distributed object also exposes events in the same way.

For example:

```csharp
var subscriptionId = await dict.SubscribeAsync(events => events
    .EntryAdded((sender, args) => {
        // ...
    })
    .EntryRemoved((sender, args) => {
        // ...
    })
);

// ... handle events ...

var success = await dict.UnsubscribeAsync(subscriptionId);
```

Refer to the [Events](events.md) page for details.

## Transactions

The client is responsible for creating transactions. Transactions by default follow the Microsoft's transaction pattern: they must be disposed, and commit or roll back depending on whether they have been completed.

For example:

```csharp
await using (var transaction = await client.BeginTransactionAsync())
{
    var map = await transaction.GetMapAsync<string, string>("my-map");
    await map.PutAsync("key", "value");
    transaction.Complete();
}
```

Refer to the [Transactions](transactions.md) page for details.

