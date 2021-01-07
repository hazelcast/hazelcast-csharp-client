# Obtaining Hazelcast .NET

Hazelcast is composed of two parts: the server, and the client.

Browse to [Hazelcast IMDG](https://hazelcast.org/imdg/) to find out how to obtain and run the server part.

## Requirements

The Hazelcast .NET client is distributed as a NuGet package which targets [.NET Standard](https://docs.microsoft.com/en-us/dotnet/standard/net-standard) versions 2.0 and 2.1. It can therefore be used in any application targetting .NET versions that support these .NET Standard versions:

* .NET Framework 4.6.2 and above, on Windows
* .NET Core 2.1 and 3.1, on Windows, Linux and MacOS

The upcoming [.NET 5](https://devblogs.microsoft.com/dotnet/introducing-net-5/) version supports .NET Standard 2.1, and therefore should execute the Hazelcast .NET client without issues, but that is not supported yet.

## Getting the client

The .NET client is distributed via NuGet as a package named [Hazelcast.NET](https://www.nuget.org/packages/Hazelcast.Net/). 
It can be installed like any other NuGet package, either via the Visual Studio GUI, or via the package manager:

```
PM> Install-Package Hazelcast.NET
```

Or via the .NET CLI:

```
> dotnet add package Hazelcast.NET
```

Or manually added to the project as a package reference:

```
<PackageReference Include="Hazelcast.NET" Version="4.0.0" />
```

## Notes

The Hazelcast .NET client uses the logging abstractions proposed by the [Microsoft.Extensions.Logging](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging) namespace. By default, the client supports the abstractions, but does not come with any actual implementation. This means that, by default, the client will not output any log information. To actually log, an implementation must be added to the project.

See the [Logging](logging.md) documentation for details.