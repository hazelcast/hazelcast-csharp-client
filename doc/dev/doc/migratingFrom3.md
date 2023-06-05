# Migrating from v3

Starting with version 4, the Hazelcast .NET client has been massively refactored in order to benefit from the asynchronous features of the .NET platform and the C# language.

For instance, the low-level networking stack now relies on Microsoft's high-performance [System.IO.Pipelines](https://docs.microsoft.com/en-us/dotnet/standard/io/pipelines) which also powers the Kestrel web server. It is constantly improved, and is the foundation of all high-performance networking in modern .NET.

Unfortunately, the move from synchronous to asynchronous coding patterns impacts the client API in large ways. Although the Hazelcast *concepts* have not changed, they are exposed in a quite different API. This document proposes to introduce you to the new API and serve as a companion on your migration path from version 3 to more recent versions (as of this writing, version 5).

> [!WARNING]
> Migrating existing code to an asynchronous programming model is not a trivial operation and requires some understanding of how asynchronous code functions in .NET, especially when running one .NET Framework platform. To help your transition, we gather a list of [asynchronous pitfalls](async-pitfalls.md) and pointers to documentations.

## Configuring a client instance

Up to version 3, the Hazelcast .NET client provided two ways of configuration. You could load a declarative configuration from an XML file:
```
var client = HazelcastClient.NewHazelcastClient("path/to/config.xml");
```

Alternatively, you could create a `ClientConfig` object and programmatically configure the client:
```
var config = new ClientConfig();
config.GetNetworkConfig().AddAddress("127.0.0.1:5701);
var client = HazelcastClient.NewHazelcastClient(config);
```

Starting with version 4, the Hazelcast .NET client relies on the configuration abstractions proposed by the [Microsoft.Extensions.Configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration) namespace. These abstractions provide built-in support for command-line arguments, environment variables, configuration files or in-memory configuration. They allow us to merge these various sources (see the [configuration sources](configuration/sources.md) page for a complete reference).

The v3 configuration XML file is replaced with a very similar JSON file, and this file is only one of the configuration *sources*. The `ClientConfig` object is replaced by a `HazelcastOptions` object, which is built by the `HazelcastOptionsBuilder` object. And, creating a new client instance always require options. Thus, the declarative and programmatic ways are merged into one:
```
var options = new HazelcastOptionsBuilder().Build();
var client = await HazelcastClientFactory.StartNewClientAsync(options);
```

The `HazelcastOptionsBuilder` merges the various sources, including the default .NET `appsettings.json` file or the specific `hazelcast.json` file. It also provide ways to register command-line arguments, or programmatically alter the options. For instance, the following code passes the command-line `args` to the `HazelcastOptionsBuilder`, and provides a configuration delegate to add addresses to the networking configuration. The final, resulting `HazelcastOptions` will be the result of the merge of all the sources. This allows you to, for instance, use a default JSON file *but* override some values via an environment variable.

```
var hazelcastOptions = new HazelcastOptionsBuilder
    .WithArgs(args)
    .With(options => options.Networking.Addresses.Add("127.0.0.1:5701"))
    .Build();
var client = await HazelcastClientFactory.StartNewClientAsync(hazelcastOptions);
```

The cluster name could be provided via the JSON file:
```json
{
    "hazelcast": {
        "clusterName": "dev"
    }
}
```

Or, via a command-line option:
```sh
program --hazelcast:clusterName=dev
```

Or, via an environment variable:
```sh
set hazelcast__clusterName=dev
```

The [configuration](configuration.md) sections has more details about configuration.

## Starting a client instance

Once options have been gathered, one can start a client instance. In version 3 one would do:
```csharp
var client = HazelcastClient.NewHazelcastClient(config);
```

The new syntax is quite similar:
```csharp
var client = await HazelcastClientFactory.StartNewClientAsync(hazelcastOptions);
```

However, there is a *big* difference, introduced by the `await` keyword: the `StartNewClientAsync` is asynchronous. It does not return an `IHazelcastClient` instance, but a `Task<IHazelcastClient>` which represents the asynchronous creation of the client and its connection to the cluster, and will complete once the client is connected.

The major benefit is that the current thread will not be blocked by the client network I/Os. The drawback, when migrating from v3, is that asynchronous code is viral. If you used to create a client in a normal method:
```csharp
public void DoSomething()
{
    var config = ...;
    var client = HazelcastClient.NewHazelcastClient(config);
    // use the client
}
```

You cannot simply replace your code with the new syntax, as the compiler will simply reject code such as:
```csharp
public void DoSomething()
{
    var options = ...;
    var client = await HazelcastClientFactory.StartNewClientAsync(hazelcastOptions);
    // use the client
}
```

Your own method has been infected by asynchronous code, and now needs to become asynchronous too:
```csharp
public async Task DoSomething()
{
    var options = ...;
    var client = await HazelcastClientFactory.StartNewClientAsync(hazelcastOptions);
    // use the client
}
```

And, of course, this will bubble up to every place in your application. If you are not familiar with asynchronous programming in .NET, you may want to read [these pages](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/async-scenarios) from Microsoft.

### The (a)synchronous impedance mismatch

If you Google around, you will find patterns that try to solve the (a)synchronous impedance mismatch. In other words, to let you break the asynchronous chain at some point, so that you can use the new asynchronous client in a code base that is mostly synchronous. You will probably end up with code such as:
```csharp
public void DoSomething()
{
    var options = ...;
    var client = HazelcastClientFactory.StartNewClientAsync(hazelcastOptions).GetAwaiter().GetResult();
    // use the client
}
```

While this code *can* work, it keeps the current thread busy and can lead to deadlocks. We recommend you avoid using such patterns unless you fully understand the implications and the underlying mechanisms of .NET asynchronous code.

## Using a client instance

Once a client instance has been obtained, using it is not much different from version 3, except that everything is asynchronous. For instance, this code taken from the version 3 documentation would add a new value to a map and then read the value back:
```csharp
var map = client.GetMap<string, string>("my-distributed-map");
map.Put("key", "value");
var value = map.Get("key");
```

The new API counterpart is:
```csharp
var map = await client.GetMapAsync<string, string>("my-distributed-map");
await map.PutAsync("key", "value");
var value = await map.GetAsync("key");
```

As you can see, using the client is *generally* quite similar to version 3, apart from the asynchronous difference.

The following sections describes the aspects that have changed and require more attention.

## Concepts

### Logging

In previous versions, the Hazelcast .NET client relied on a custom built-in logging solution.

The Hazelcast .NET client now uses the logging abstractions proposed by the [Microsoft.Extensions.Logging](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging) namespace. These abstractions come with a range of providers to log to the console, and other various destinations. In addition, a variety of third-party products (such as [Serilog](https://serilog.net/)) support complex logging patterns and more destinations (to the filesystem, the Cloud, etc).

This also means that the same logging mechanism can be used by the various libraries used in users' applications.

Note that, by default, the Hazelcast .NET client does *not* provide an actual logging provider. This mean that, by default, you will *not* see any log output, neither to the console nor to any file. To actually *see* the log, which can contain some precious troubleshooting information, your application will need to register the logging provider of your choice, via the configuration options.

The [logging](logging.md) page provides instructions on how to register Microsoft's own console logging provider, which will allows you to see the Hazelcast .NET client's log in the output console. It also provides pointers to advanced providers that can write to files or Azure App Services.

### Locking

Previous versions of the Hazelcast .NET Client attached locks to threads, in a way similar to the thread-based model that .NET provides with, for instance, the `lock` statement. Due to the systematic usage of asynchronous patterns, this is not applicable anymore. For locks that were available in version 3, i.e. map locks, the Hazelcast .NET client introduces an `AsyncContext` class, which represents the lock ownership, and flows with async operations. i.e. are transferred to the new thread when an operation resumes after awaiting. Therefore, when an operation acquires a lock, it owns the lock until it releases it, no matter what thread executes the operation. The `AsyncContext` uses a sequential number to ensure the uniqueness of the identifier.

Starting a new task does *not* necessarily begin a new context. Contexts are created explicitly, with a `using (AsyncContext.New())` pattern. The whole block executes with a new context, which flows to any task started within the block. For instance:
```csharp
// executes in the same, current context
await DoSomethingAsync(...);

using (AsyncContext.New())
{
    // executes in a new context
    await DoSomethingAsync(...);
}
```

On the other hand, [fenced locks](distributed-objects/fencedlock.md), which are part of the [CP subsystem](cpsubsystem.md) and were introduced with version 4, use a different and explicit pattern. They are documented on the [Locking](locking.md) page which has more details on locking patterns.

### Events

In previous versions, the Hazelcast .NET Client use *listeners* to handle events. The following code, from the version 3 documentation, shows how to register a listener that would receive notifications whenever an entry is added to a map:
```csharp
public class MyEntryAddedListener<K, V> : EntryAddedListener<K, V>
{
    public void EntryAdded(EntryEvent<K, V> entryEvent)
    {
        Console.WriteLine(entryEvent);
    }
}

map.AddEntryListener(new MyEntryAddedListener<string, string>());
```

Current versions move to a handler-based model closer to the C# `event` model, though with a different syntax for adding and removing handlers, due to the asynchronous nature of these operations. The above code thus becomes:
```csharp
private void OnEntryAdded(IHMap<string, string> map, MapEntryAddedEventArgs<string, string> args)
{
    // do things...
}

await map.SubscribeAsync(events => events.EntryAdded(OnEntryAdded));
```

Refer to the [Events](events.md) page for details.
