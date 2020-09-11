# Obtaining Hazelcast

Hazelcast is composed of two parts: the server, and the client.

Browse to [Hazelcast IMDG](https://hazelcast.org/imdg/) to find out how to obtain and run the server part.

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