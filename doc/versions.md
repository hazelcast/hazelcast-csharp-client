# Versions

Hazelcast .NET brings the full power of the [Hazelcast](https://hazelcast.com) high-performance, in-memory computing platform to the Microsoft .NET ecosystem. The [Hazelcast .NET Client](https://hazelcast.com/clients/dotnet/) allows you to elastically scale your .NET caches at high read speeds, to access all of Hazelcast data structures such as distributed maps, queues, topics and more. All, with enterprise level security through SSL and mutual authentication.

## .NET support

The following table defines the supported .NET versions for the various Hazelcast .NET Client versions.  
Note that .NET Framework runs on Windows exclusively, whereas .NET Core and .NET 5.0+ run on Windows, Linux and MacOS.

|Version|.NET Framework 4.5-4.6.1|.NET Framework 4.6.2-4.8|.NET Core 2.1|.NET Core 3.1 (LTS)|.NET 5.0|.NET 6.0 (LTS)|
|-|:-:|:-:|:-:|:-:|:-:|:-:|
|5.1|No|Yes|No|Yes|Yes|**Yes**|
|5.0|No|Yes|**No** (note)|Yes|**Yes**|No|
|4.x|**No** (note)|Yes|Yes|Yes|No|No|
|3.x|Yes|Yes|No|No|No|No|

> [!NOTE]
> As per Microsoft's **.NET Framework** [Support Policy](https://docs.microsoft.com/en-us/lifecycle/products/microsoft-net-framework), versions 4.5.2, 4.6, and 4.6.1 will reach end of support on April 26, 2022. We do *not* support these versions starting with Hazelcast 4. We recommend that all Hazelcast 3 users migrate to at least .NET Framework 4.6.2, and ideally to .NET Framework 4.8.  
> As per Microsoft's **.NET and .NET Core** [Support Policy](https://docs.microsoft.com/en-us/lifecycle/products/microsoft-net-and-net-core) and [LifeCycle](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core), versions 2.1 and 2.2 are not supported anymore. Version 3.1 is supported until Dec 3, 2022 and version 5.0 until May 8, 2022. We recommend that all users use versions of .NET supported by Microsoft.

## Hazelcast .NET Client versions

### Current

* 5.0.0 [general documentation](xref:doc-index-5-0-0) and [API reference](xref:api-index-5-0-0)

### Preview

* <devdoc>$version [general documentation](dev/doc/index.md) and [API reference](dev/api/index.md)</devdoc>

### Previous

* 4.1.0 [general documentation](xref:doc-index-4-1-0) and [API reference](xref:api-index-4-1-0)
* 4.0.2 [general documentation](xref:doc-index-4-0-2) and [API reference](xref:api-index-4-0-2)
* 4.0.1 [general documentation](xref:doc-index-4-0-1) and [API reference](xref:api-index-4-0-1)
* 4.0.0 [general documentation](xref:doc-index-4-0-0) and [API reference](xref:api-index-4-0-0)
* 3.12.3 [README](xref:doc-index-3-12-3) and [API reference](xref:api-index-3-12-3)
* 3.12.2 [README](xref:doc-index-3-12-2) and [API reference](xref:api-index-3-12-2)

