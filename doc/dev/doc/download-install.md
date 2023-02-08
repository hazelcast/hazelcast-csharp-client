# Download and Install

Hazelcast is composed of two parts: the server, and the client. The client requires a working Hazelcast cluster, composed of one or more servers, in order to run. This cluster handles storage and manipulation of the user data. The client is a library which connects to the cluster, and gives access to such data.

## Hazelcast Client

### Requirements

The Hazelcast .NET client is distributed as a NuGet package which targets [.NET Standard](https://docs.microsoft.com/en-us/dotnet/standard/net-standard) versions 2.0 and 2.1, and specific .NET version starting with 5.0. It can therefore be used in any application targetting .NET versions that support these .NET Standard versions:

* .NET Framework 4.6.2 and above, on Windows
* .NET Core, on Windows, Linux and MacOS

See the [versions](../../versions.md) page for details on which exact version are supported by the client.

### Distribution

The .NET client is distributed via NuGet as a package named [Hazelcast.Net](https://www.nuget.org/packages/Hazelcast.Net/). 
It can be installed like any other NuGet package, either via the Visual Studio GUI, or via the package manager:

```
PM> Install-Package Hazelcast.Net
```

Or via the .NET CLI:

```
> dotnet add package Hazelcast.Net
```

Or manually added to the project as a package reference:

```
<PackageReference Include="Hazelcast.Net" Version="4.0.0" />
```

### Binding Redirects

When including the `Hazelcast.Net` package in a **.NET Framework** project, be aware that some binding redirects may be required. Although we try hard to align all our dependencies, some inconsistencies even within Microsoft's own packages mean that it is not possible to avoid redirects entirely. You can enable `<AutoGenerateBindingRedirects>` in your project file, and Visual Studio should populate your application config files with the appropriate binding redirects.

Alternatively, those redirects *should* be sufficient at the moment:

```xml
<assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
  <dependentAssembly>
    <assemblyIdentity name="System.Buffers" publicKeyToken="cc7b13ffcd2ddd51" culture="neutral" />
    <bindingRedirect oldVersion="0.0.0.0-4.0.3.0" newVersion="4.0.3.0" />
  </dependentAssembly>
</assemblyBinding>
<assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
  <dependentAssembly>
    <assemblyIdentity name="System.Runtime.CompilerServices.Unsafe" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
   <bindingRedirect oldVersion="0.0.0.0-5.0.0.0" newVersion="5.0.0.0" />
  </dependentAssembly>
</assemblyBinding>
```

## Hazelcast Server

Browse to [Hazelcast In-Memory Computing](https://hazelcast.com/products/in-memory-computing/) to find out all about the Hazelcast server.

Hazelcast IMDG cluster consists of one or more cluster members. These members generally run on multiple virtual or physical machines and are connected to each other via network. Any data put on the cluster is partitioned to multiple members transparent to the user. It is therefore very easy to scale the system by adding new members as the data grows. Hazelcast IMDG cluster also offers resilience. Should any hardware or software problem causes a crash to any member, the data on that member is recovered from backups and the cluster continues to operate without any downtime. Hazelcast clients are an easy way to connect to a Hazelcast IMDG cluster and perform tasks on distributed data structures that live on the cluster.

There are many different ways to run a Hazelcast cluster or member. The [Installing and Upgrading](https://docs.hazelcast.com/imdg/latest/installation/installing-upgrading.html) section of the Reference Manual details options to install and run a cluster, while the [Deploying in Cloud](https://docs.hazelcast.com/imdg/latest/installation/deploying-in-cloud.html) section details options to run a cluster in the cloud.

If you want to start one member to experiment with the Hazelcast .NET client, two simple ways are possible.

> [!NOTE]
> Running the Hazelcast server requires a Java Runtime Environment. The [Supported JVMs](https://docs.hazelcast.com/imdg/latest/installation/supported-jvms.html) page of the reference details which JVMs are supported. For a quick start, OpenJDK provided by [Adoptium](https://adoptopenjdk.net/) (either version 8, 11 or 16) are OK.

### Standalone JARs

You can download the standalone JARs from the [download page](https://hazelcast.com/get-started/download/). After extracting the downloaded archive, you should find a start script (`start.sh` or `start.bat` depending on your platform) in the `bin` directory, which you can use to start a member.

### Powershell Script

The Hazelcast .NET client repository on [GitHub](https://github.com/hazelcast/hazelcast-csharp-client) provides a Powershell script which can be used to download and run a test member. For instance, this downloads and starts a member version 4.2:

```pwsh
PS> ./hz.ps1 run-server -server 4.2
```

If the version is not specified (i.e. `./hz.ps1 run-server`), the latest version supported by the client is used.