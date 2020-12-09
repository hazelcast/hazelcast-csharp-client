# Logging

The Hazelcast .NET client uses the logging abstractions proposed by the [Microsoft.Extensions.Logging](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging) namespace. 

By default, the client supports the abstractions, but does not come with any actual implementation. This means that, by default, the client will not output any log information. To actually log, an implementation must be added to the project.

Microsoft provides a range of providers to log to various destinations. In addition, a variety of third-party products such as [Serilog](https://serilog.net/) support complex logging patterns and more destinations (to the filesystem, the Cloud, etc).

## Example: logging to console

For instance, to enable logging to console, a reference to the [Microsoft.Extensions.Logging.Console](https://www.nuget.org/packages/microsoft.extensions.logging.console) NuGet package must be added, and then the Hazelcast client needs to be configured to use that implementation:

```
var hazelcastOptions = HazelcastOptions.Build(configure: (configuration, options) =>
    LoggerFactory.Create(builder =>
        builder
            .AddConfiguration(configuration.GetSection("logging"))
            .AddConsole());
);
```

## Configuration

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

## Re-using the logging system

The logging system is available for the user to log in their application, too. At the moment, the best way to access the logging system is:

```
var loggerFactory = hazelcastOptions.LoggerFactory.Service;
var logger = loggerFactory.CreateLogger<MyClass>();
logger.LogInformation("hello!"):
```

NOTE: in the future, the logging system will be more directly exposed by the client.

NOTE: in DI-based applications, things work a bit differently (to be documented).

