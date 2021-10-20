# Configuration Sources

Configuration follows the [Microsoft.Extensions.Configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration)
patterns. The Hazelcast client configuration is represented by the @Hazelcast.HazelcastOptions class. When simply instantiated, this
class contains the default options (i.e. it does not even read the options file):
```csharp
var options = new HazelcastOptions();
```

For anything more realistic though, different approaches are available, as detailed below.

This page does not document the options themselves. Options that can be configured are fully documented on the [Options](options.md) page.

### Simple Environment

In a simple, non-hosted environment without dependency injection, options need to be *built* using the
@Hazelcast.HazelcastOptionsBuilder:

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        var options = new HazelcastOptionsBuilder.With(args).Build();
    }
}
```

This will determine the application environment (`<env>`) from the `DOTNET_ENVIRONMENT` and `ASPNETCORE_ENVIRONMENT` variables (or, if not specified, default to `Production`), and then gather configuration keys from the following ordered sources:

* `appsettings.json` file
* `appsettings.<env>.json` file
* Environment variables (using double-underscore separator, e.g. `hazelcast__clientName`)
* Command line arguments (using colon separator, e.g. `hazelcast:clientName`)
* `hazelcast.json` file
* `hazelcast.<env>.json` file
* Hazelcast-specific environment variables (using dot separator, e.g. `hazelcast.clientName`)
* Hazelcast-specific command line arguments (using dot separator, e.g. `hazelcast.clientName`)
* Optional in-memory key/values

The Hazelcast-specific sources for environment variables and command line arguments only exist to support the non-standard dot separator, and complement the original sources.

The @Hazelcast.HazelcastOptionsBuilder provides ways to override the name and location of the `hazelcast.json` and
`hazelcast.<env>.json` files, the `<env>` environment name, and accepts optional in-memory key/values.

Every Hazelcast option can therefore be specified via the traditional .NET Core methods. For instance, specifying one
cluster server address can be done via the following Json fragment in any of the Json files:

```json
{
    "hazelcast": {
        "networking": {
            "addresses": [ "server:port" ]
        }
    }
}
```

It can alternatively be specified by setting an environment variable (note that the dotted format may not be supported on every platform):

```sh
hazelcast__networking__addresses__0=server:port   ## supported on all platforms
hazelcast:networking:addresses:0=server:port      ## not supported on all platforms
hazelcast.networking.addresses.0=server:port
```

It can alternatively be specified with command line arguments:

```sh
$ myApp hazelcast:networking:addresses:0=server:port
$ myApp hazelcast.networking.addresses.0=server:port
```

All the .NET Core supported formats are supported (i.e. `/arg value`, `/arg=value`, `--arg value`, etc.). See
the [documentation](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/#command-line) for details.

Finally, the method supports direct, in-memory key/values, where values can use either the dot or colon separator:

```csharp
var options = HazelcastOptions.Build(args, new[]
{
    new KeyValuePair<string, string>("hazelcast.networking.addresses.0", "server:port"),
});
```

This is where the fluent @HazelcastOptionsBuilder may be more convenient:

```csharp
var options = new HazelcastOptionsBuilder.With("hazelcast.networking.addresses.0", "server:port").Build();
```

### Container Environment

In a container environment, one can rely on dependency injection to manage configuration. An @Microsoft.Extensions.Configuration.IConfiguration must be created, in order to add Hazelcast to the services:

```csharp
var configuration = new ConfigurationBuilder()
    // add default configuration (appsettings.json, etc)
    .AddDefaults(args)
    // add Hazelcast-specific configuration
    .AddHazelcast(args)
    .Build();

// create the service collection
var services = new ServiceCollection();

// add Hazelcast-specific services
services.AddHazelcast(configuration); 
```

Configuration keys will be gathered from the same sources and in the same order as before, and options will be registered in the service container, and available via dependency injection:

```csharp
public class MyService
{
    private readonly HazelcastOptions _options;

    public MyService(IOptions<HazelcastOptions> ioptions)
    {
        _options = ioptions.Value;
    }

    public async Task DoSomethingAsync()
    {
        await using var client = HazelcastClientFactory.StartNewClientAsync(_options);
        // ...
    }
}
```

Also, the traditional Microsoft Dependency Injection patterns are supported:

```csharp
services.Configure<HazelcastOptions>(options => 
{
    options.Networking.Addresses.Add("server:port");
});
```

Note: The required extension methods are not part of the Hazelcast.Net NuGet packages, but are provided as part of the [Hazelcast.Net.DependencyInjection](https://www.nuget.org/packages/Hazelcast.Net.DependencyInjection/) project (on NuGet).

### Hosted Environment

In a .NET Core hosted environment (see [.NET Generic Host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host)), the host supplies the @Microsoft.Extensions.Configuration.IConfiguration instance, and manages dependency injection. All that is needed is to tell the host how to handle the Hazelcast-specific configuration (e.g. `hazelcast.json`), and to add Hazelcast to services.

For example:

```csharp
Host.CreateDefaultBuilder(args)
    .ConfigureHazelcast(args) // configure Hazelcast services
    .ConfigureServices((hostingContext, services) =>
    {
        services.AddHazelcast(hostingContext.Configuration); // register Hazelcast services
    });
```

Just as with the previous container environment, configuration keys will be gathered from the same sources and in the same order as before, and options will be registered in the service container, and available via dependency injection.

In a typical WebAPI application, this means that the `Program` class would probably contain code similar to:

```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureHazelcast(args) // configure Hazelcast services
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>(); 
        });
```

And the `Startup` class would probably contain code similar to:

```csharp
// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers();
    services.AddHazelcast(Configuration); // register Hazelcast services

    // ... add more services ...
}
```

Note: The required extension methods are not part of the Hazelcast.Net NuGet packages, but are provided as part of the [Hazelcast.Net.DependencyInjection](https://www.nuget.org/packages/Hazelcast.Net.DependencyInjection/) project (on NuGet).
