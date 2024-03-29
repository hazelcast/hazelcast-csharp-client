# Dependency Injection

The [Hazelcast.Net.DependencyInjection](https://www.nuget.org/packages/Hazelcast.Net.DependencyInjection/) NuGet package provides utilities for simplifying the usage of the Hazelcast .NET Client in applications which rely on dependency injection, following the standard Microsoft best practices for managing options, etc.

## Registering Options

Registering the Hazelcast options into the service container is achieved via the `AddHazelcastOptions` method. This method accepts one parameter, which is an `Action<HazelcastOptionsBuilder>` delegate. It can be used to configure the options just as one would do with any `HazelcastOptionsBuilder`. For instance:

```csharp
services.AddHazelcastOptions(builder => builder
    .With(options => {
        options.Networking.Addresses.Add("localhost:5701");
        options.Networking.ReconnectMode = ReconnectMode.ReconnectAsync;
    }));
```

An equivalent `AddHazelcastFailoverOptions` method exists to register failover options, via an `Action<HazelcastFailoverOptionsBuilder>` delegate.

Options are then injected as `IOptions<HazelcastOptions>`.

# Injecting the Hazelcast Client

Due to the fact that an `IHazelcastClient` instance must be connected to a cluster through an asynchronous operation, it is not possible to register and directly inject such an instance. Instead, the best practice for the Hazelcast .NET Client consists in injecting *options*, and using the `HazelcastClientFactory` to create a client wherever needed.

```csharp
public class MyClass
{
    private readonly HazelcastOptions _options;

    public MyClass(IOptions<HazelcastOptions> options)
    {
        _options = options.Value;
    }

    public async Task UseClientAsync()
    {
        await using var client = await HazelcastClientFactory.StartNewClientAsync(_options);

        // ... use the client ...
    }
}
```

Alternatively, it is possible to register a client *provider* through the following pattern:

```csharp
public class HazelcastClientProvider : IAsyncDisposable
{
    private readonly HazelcastOptions _options;
    private IHazelcastClient _client;

    public HazelcastClientProvider(IOptions<HazelcastOptions> options)
    {
        _options = options.Value;
    }

    public async Task<IHazelcastClient> GetClientAsync()
    {
        return _client ??= await HazelcastClientFactory.StartNewClientAsync(_options);
    }

    public async ValueTask DisposeAsync()
    {
        if (_client != null) await _client.DisposeAsync();
    }
}

public class MyClass
{
    private readonly HazelcastClientProvider _clientProvider;

    public MyClass(HazelcastClientProvider clientProvider)
    {
        _clientProvider = clientProvider;
    }

    public async Task UseClientAsync()
    {
        var client = await _clientProvider.GetClientAsync();

        // ... use the client ...
    }
}
```

Remember that the client is disconnected from the cluster when it is disposed. Disposing too soon, or forgetting to dispose the client, may lead to errors or connection leaks.