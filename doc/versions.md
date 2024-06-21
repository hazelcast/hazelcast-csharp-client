# Versions

Hazelcast .NET brings the full power of the [Hazelcast](https://hazelcast.com) high-performance, in-memory computing platform to the Microsoft .NET ecosystem. The [Hazelcast .NET Client](https://hazelcast.com/clients/dotnet/) allows you to elastically scale your .NET caches at high read speeds, to access all of Hazelcast data structures such as distributed maps, queues, topics and more. All, with enterprise level security through SSL and mutual authentication.

Versions lifecycle and support period follows the Hazelcast [Version Support Windows](https://support.hazelcast.com/s/article/Version-Support-Windows) policy.

### Current Version

* <curdoc>5.4.0 [general documentation](xref:doc-index-5-4-0) and [API reference](xref:api-index-5-4-0)</curdoc>

### Preview

* <devdoc>$version [general documentation](dev/doc/index.md) and [API reference](dev/api/index.md)</devdoc>

### Previous Versions

<prevdoc></prevdoc>
* 5.3.0 [general documentation](xref:doc-index-5-3-0) and [API reference](xref:api-index-5-3-0)
* *5.2.2* [general documentation](xref:doc-index-5-2-2) and [API reference](xref:api-index-5-2-2)
* 5.2.1 [general documentation](xref:doc-index-5-2-1) and [API reference](xref:api-index-5-2-1)
* 5.2.0 [general documentation](xref:doc-index-5-2-0) and [API reference](xref:api-index-5-2-0)
* **5.1.1** [general documentation](xref:doc-index-5-1-1) and [API reference](xref:api-index-5-1-1)
* 5.1.0 [general documentation](xref:doc-index-5-1-0) and [API reference](xref:api-index-5-1-0)
* **5.0.2** [general documentation](xref:doc-index-5-0-2) and [API reference](xref:api-index-5-0-2)
* 5.0.1 [general documentation](xref:doc-index-5-0-1) and [API reference](xref:api-index-5-0-1)
* 5.0.0 [general documentation](xref:doc-index-5-0-0) and [API reference](xref:api-index-5-0-0)
* **4.1.0** [general documentation](xref:doc-index-4-1-0) and [API reference](xref:api-index-4-1-0)

### Unsupported Versions

* **4.0.2** [general documentation](xref:doc-index-4-0-2) and [API reference](xref:api-index-4-0-2)
* 4.0.1 [general documentation](xref:doc-index-4-0-1) and [API reference](xref:api-index-4-0-1)
* 4.0.0 [general documentation](xref:doc-index-4-0-0) and [API reference](xref:api-index-4-0-0)
* **3.12.3** [README](xref:doc-index-3-12-3) and [API reference](xref:api-index-3-12-3)
* 3.12.2 [README](xref:doc-index-3-12-2) and [API reference](xref:api-index-3-12-2)

## .NET support

The following table defines the .NET versions that were active and supported by each version of the Hazelcast .NET Client, at the time it was released. The Hazelcast .NET Client remains supported on these .NET versions for as long as they have not reached their end of support from Microsoft.

|Version|.NET Framework<br/>4.5-4.6.1|.NET Framework<br/>4.6.2-4.8|.NET Core<br/>2.1|.NET Core<br/>3.1 (LTS)|.NET<br/>5.0|.NET<br/>6.0 (LTS)|.NET<br/>7.0|
|-|:-:|:-:|:-:|:-:|:-:|:-:|:-:|
|5.2|No|Yes|No|**No** (note)|**No** (note)|Yes|**Yes**|
|5.1|No|Yes|No|Yes|Yes|**Yes**|No|
|5.0|No|Yes|**No** (note)|Yes|**Yes**|No|No|
|4.x|**No** (note)|Yes|Yes|Yes|No|No|No|
|3.x|Yes|Yes|No|No|No|No|No|

Note that .NET Framework runs on Windows exclusively, whereas .NET Core and .NET 5.0+ run on Windows, Linux and MacOS.

> [!NOTE]
> As per Microsoft's **.NET Framework** [Support Policy](https://docs.microsoft.com/en-us/lifecycle/products/microsoft-net-framework), versions 4.5.2, 4.6, and 4.6.1 will reach end of support on April 26, 2022. We do *not* support these versions starting with Hazelcast 4. We recommend that all Hazelcast 3 users migrate to at least .NET Framework 4.6.2, and ideally to .NET Framework 4.8.
>
> As per Microsoft's **.NET and .NET Core** [Support Policy](https://docs.microsoft.com/en-us/lifecycle/products/microsoft-net-and-net-core) and [LifeCycle](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core), versions 2.1 to 5.0 are not supported anymore. We recommend that all users use versions of .NET supported by Microsoft.