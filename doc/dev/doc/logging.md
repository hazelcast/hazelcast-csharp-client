# Logging

The Hazelcast .NET client uses the logging abstractions proposed by the [Microsoft.Extensions.Logging](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging) namespace. 

By default, the client supports the abstractions, but does not come with any actual implementation. This means that, by default, the client will not output any log information. To actually log, an implementation must be added to the project.

Microsoft provides a range of providers to log to various destinations. In addition, a variety of third-party products such as [Serilog](https://serilog.net/) support complex logging patterns and more destinations (to the filesystem, the Cloud, etc).

## Quick start: logging to console

To enable logging to console, add a NuGet reference to the [Microsoft.Extensions.Logging.Console](https://www.nuget.org/packages/microsoft.extensions.logging.console) NuGet package, and then configure the Hazelcast client to use that implementation:

```
var hazelcastOptions = new HazelcastOptionsBuilder()
    .With(args)
    .WithConsoleLogger(LogLevel.Information)
    .Build();
```

Where the `WithConsoleLogger` is:

```
public static HazelcastOptionsBuilder WithConsoleLogger(this HazelcastOptionsBuilder builder, LogLevel hazelcastLogLevel = LogLevel.None)
{
    return builder
        .With("Logging:LogLevel:Default", "None")
        .With("Logging:LogLevel:System", "Information")
        .With("Logging:LogLevel:Microsoft", "Information")
        .With("Logging:LogLevel:Hazelcast", hazelcastLogLevel.ToString())
        .With((configuration, options) =>
        {
            // configure logging factory and add the console provider
            options.LoggerFactory.Creator = () => LoggerFactory.Create(loggingBuilder =>
                loggingBuilder
                    .AddConfiguration(configuration.GetSection("logging"))
                    .AddConsole());
        });
}
```

Note that the in-memory option set with `.With("Logging:LogLevel:Hazelcast", hazelcastLogLevel.ToString())` statement takes precedence over everything else (command-line, environment variables...) and you may want to comment it out when experimenting with the code.

You can find this example as [LoggingExample.cs](https://github.com/hazelcast/hazelcast-csharp-client/blob/master/src/Hazelcast.Net.Examples/LoggingExample.cs) in our examples project. If you [build](http://hazelcast.github.io/hazelcast-csharp-client/4.1.0/doc/contrib-build.html) and [run](http://hazelcast.github.io/hazelcast-csharp-client/4.1.0/doc/examples.html) this example, it will produce the following output:

```
info: Hazelcast.Examples.LoggingExample.A[0]
      This is an INFO message from Hazelcast.Examples.LoggingExamples.A
warn: Hazelcast.Examples.LoggingExample.A[0]
      This is a WARNING message from Hazelcast.Examples.LoggingExamples.A
warn: Hazelcast.Examples.LoggingExample.B[0]
      This is a WARNING message from Hazelcast.Examples.LoggingExamples.B
```

You can experiment changing the log levels in code, or via the command line, or via environment variables:

```
PS> ./hz.ps1 run-example Logging --- --Logging:LogLevel:Hazelcast=Debug
```

Also note that options set with `With("Logging:LogLevel:Hazelcast", "Debug")` method calls take precedence over command line options.

## Other implementations

Using a different implementation consists in
* Adding a NuGet reference to a different NuGet package, such as [Microsoft.Extensions.Logging.AzureAppServices](https://www.nuget.org/packages/Microsoft.Extensions.Logging.AzureAppServices/) in order to log to Azure App Services, or a Serilog package to log to a file
* Replacing the `AddConsole()` call above with the appropriate method, as defined by the implementation

## Configuration

The example above defines *log levels*:
* `None` by default
* `Information` for loggers named `System.*` or `Microsoft.*`
* `Information` for loggers named `Hazelcast.*`

The various existing [LogLevel](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loglevel) values allows for fine-tuning of what should, or should not, be logged. We recommend running Hazelcast with the `Information` log level by default, though running with the `Debug` log level may help troubleshooting issues (but should not be used in production environment).

Logging is configured as per [Microsoft.Extensions.Logging](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging) conventions. In a configuration file, one would need a *logging* section distinct from the *hazelcast* section:

```
{
    "hazelcast": {
        ...
    },
    "logging": {
        "logLevel": {
            "Default": "Debug",
            "System": "Information",
            "Microsoft": "Information",
            "Hazelcast.Examples.MyApp", "Information" 
        }
    }
}
```

Refer to Microsoft's documentation for more details.

Logging can also be configured programmatically with statements such as `.With("Logging:LogLevel:Hazelcast", LogLevel.Debug.ToString())` as per the example above.

## Re-using the logging system

The logging system is available for the user to log in their application, too. At the moment, the best way to access the logging system is:

```
var loggerFactory = hazelcastOptions.LoggerFactory.Service;
var logger = loggerFactory.CreateLogger<MyClass>();
logger.LogInformation("hello!"):
```

NOTE: in the future, the logging system will be more directly exposed by the client, e.g. `hazelcastClient.LoggerFactory.CreateLogger<MyClass>()`.

## Dependency injection applications

In a typical application relying on dependency injection, the logger factory and loggers are registered in a container and injected in code. The [ContainerExample](https://github.com/hazelcast/hazelcast-csharp-client/blob/master/src/Hazelcast.Net.Examples/Client/ContainerExample.cs) and [HostedExample](https://github.com/hazelcast/hazelcast-csharp-client/blob/master/src/Hazelcast.Net.Examples/Client/HostedExample.cs) are good starting points to understand how to wire Hazelcast in such applications.


