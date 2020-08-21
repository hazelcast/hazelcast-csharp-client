# About

Version 4 of the Hazelcast .NET client has been massively refactored in order to benefit from the asynchronous features of the C# language. For instance, its low-level networking stack relies on Microsoft's [System.IO.Pipelines](https://docs.microsoft.com/en-us/dotnet/standard/io/pipelines) library. This is the high-performance library that is used, for instance, to power the Kestrel web server. It is constantly improved, as it is the foundation of all high-performance networking in .NET Core 3.x (and the upcoming .NET Core 5.x).

Threading has been greatly simplified and now entirely relies on the async/await pattern. In the current version of the code, all tasks run on the default task scheduler, and there is no limit on, for instance, the amount of concurrent tasks. All tasks run on the default .NET ThreadPool and the default Task scheduler. Depending on feedback, we could consider using custom Task schedulers and/or thread pools.

(to be completed)

## Locking

TODO: move "locking" to its own page?

Due to the systematic usage of `async`/`await` asynchronous patterns, the code for one operation can be executed by many different threads (basically, each time an operation is put on hold by an await, it can switch to another thread). Therefore, using the actual thread identifier as a "lock owner" for locking purpose is not possible.

The "lock owner" is represented by an `AsyncContext`, a class which relies upon the .NET built-in `AsyncLocal<T>` type to maintain values that flow with the asynchronous operation, i.e. are transferred to the new thread when an operation resumes after awaiting. Therefore, when an operation acquires a lock, it owns the lock until it releases it, no matter what thread executes the operation. The AsyncContext uses a sequential number to ensure the uniqueness of the "thread identifier".

In order to start a new, independent task (equivalent to starting a new thread in non `async`/`await` code), one need to explicitly start the code in a new context:

```csharp
// runs in the same context
await DoSomethingAsync(...);

// runs in a new context
await TaskEx.WithNewContext(() => DoSomethingAsync(...));
```

## Logging

In previous versions, the Hazelcast .NET client used to rely on a custom logging solution.

The Hazelcast .NET client now uses the logging abstractions proposed by the [Microsoft.Extensions.Logging](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging) namespace. These abstractions come with a range of providers to log to the file system, or to the console. In addition, a variety of third-party products (such as [Serilog](https://serilog.net/)) support complex logging patterns and destinations (to the Cloud, etc).

This also means that the same logging mechanism can be used by the various libraries used in users' applications.

## Dependency Injection

Dependency injection is becoming more and more common in large .NET applications. The Hazelcast .NET client includes support (via a separate assembly and NuGet package) for the dependency injection abstractions proposed by the [Microsoft.Extensions.DependencyInjection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection) namespace. This allows users to register Hazelcast objects in a dependency injection container.

TODO: document the NuGet package

## Configuration

In previous versions, the Hazelcast .NET client used to rely on a custom configuration solution based upon an XML file.

The Hazelcast .NET client now uses the configuration abstractions proposed by the [Microsoft.Extensions.Configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration) namespace. These abstractions provide built-in support for command-line arguments, environment variables, configuration files or in-memory configuration. For instance, they automatically support one configuration value being supplied via the configuration file, and/or a command-line argument, and/or an environment variable. They automatically parse configuration files into their strongly-typed (classes) counterpart.

This also means that the same configuration mechanism can be used by the various libraries used in users' applications.

The most important consequence for users is that the current XML configuration file is replaced with a very similar JSON file.