# Migrating from v3

Version 4 of the Hazelcast .NET client has been massively refactored in order to benefit from the asynchronous features of the C# language. 

For instance, the low-level networking stack relies on Microsoft's high-performance [System.IO.Pipelines](https://docs.microsoft.com/en-us/dotnet/standard/io/pipelines) which is used, for instance, to power the Kestrel web server. It is constantly improved, as it is the foundation of all high-performance networking in .NET Core 3.x and above.

Although the *concepts* have not changed much, the version 4 API is quite different from version 3. This page documents the most important differences.

## Threading, Async and Tasks

Threading has been greatly simplified and now entirely relies on the async/await pattern. In the current version of the code, all tasks run on the default task scheduler, and there is no limit on, for instance, the amount of concurrent tasks. All tasks run on the default .NET ThreadPool and the default Task scheduler. Depending on feedback, we could consider using custom Task schedulers and/or thread pools.

## Configuration

In previous versions, the Hazelcast .NET client used to rely on a custom configuration solution based upon an XML file.

The Hazelcast .NET client now uses the configuration abstractions proposed by the [Microsoft.Extensions.Configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration) namespace. These abstractions provide built-in support for command-line arguments, environment variables, configuration files or in-memory configuration. For instance, they automatically support one configuration value being supplied via the configuration file, and/or a command-line argument, and/or an environment variable. They automatically parse configuration files into their strongly-typed (classes) counterpart.

This also means that the same configuration mechanism can be used by the various libraries used in users' applications.

The most important consequence for users is that the current XML configuration file is replaced with a very similar JSON file.

Refer to the [Configuration](configuration.md) page for details on configuration.

## Logging

In previous versions, the Hazelcast .NET client used to rely on a custom logging solution.

The Hazelcast .NET client now uses the logging abstractions proposed by the [Microsoft.Extensions.Logging](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging) namespace. These abstractions come with a range of providers to log to the console, and other various destinations. In addition, a variety of third-party products (such as [Serilog](https://serilog.net/)) support complex logging patterns and more destinations (to the filesystem, the Cloud, etc).

This also means that the same logging mechanism can be used by the various libraries used in users' applications.

## Locking

Previous versions of the Hazelcast .NET Client attached locks to threads, in a way similar to the thread-based model that .NET provides with, for instance, the `lock` statement. Due to the systematic usage of `async`/`await` asynchronous patterns, this is not applicable anymore, and is replaced with a new model based upon an `AsyncContext` class. In order to execute work in a new context (which would correspond to executing work on a different thread for previous versions), one has to use a new context:

```csharp
// executes in the same, current context
await DoSomethingAsync(...);

using (AsyncContext.New())
{
    // executes in a new context
    await DoSomethingAsync(...);
}
```

Refer to the [Locking](locking.md) page for details.

## Events

In previous versions, the Hazelcast .NET Client use *listeners* to handle events. Current versions move to a handler-based model closer to the C# `event` model, though with a different syntax for adding and removing handlers, due to the asynchronous nature of these operations. For instance:

```csharp
// subscribe
var id = await client.SubscribeAsync(events => events
    .MembersUpdated((sender, args) => HandleMembersUpdated(sender, args)));

// handle
private void HandleMembersUpdated(IHazelcastClient client, MembersUpdatedEventArgs args)
{
    ...
}

// unsubscribe
await client.UnsubscribeAsync(id);
```

Refer to the [Events](events.md) page for details.

## Dependency Injection

Dependency injection is becoming more and more common in large .NET applications. The Hazelcast .NET client includes support (via a separate assembly and NuGet package) for the dependency injection abstractions proposed by the [Microsoft.Extensions.DependencyInjection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection) namespace. This allows users to register Hazelcast objects in a dependency injection container.


