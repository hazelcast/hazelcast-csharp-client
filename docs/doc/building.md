# Building

The source code for Hazelcast .NET is published on GitHub at [Hazelcast .NET](https://github.com/hazelcast/hazelcast-csharp-client). 
Clone the repository to get the development branch.

(need to document branches)

The solution can be opened with Microsoft [Visual Studio](https://visualstudio.microsoft.com/) 2019 or JetBrain 
[Rider](https://www.jetbrains.com/rider/). The code targets netstandard 2.0 and 2.1, and is using C#
[version](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/configure-language-version) 8.0.

The main Hazelcast.Net project (which builds the library) is covered by Microsoft's 
[Roslyn analyzers](https://docs.microsoft.com/en-us/visualstudio/code-quality/roslyn-analyzers-overview) 
(installed via the [Microsoft.CodeAnalysis.FxCopAnalyzers](https://www.nuget.org/packages/Microsoft.CodeAnalysis.FxCopAnalyzers/) 
NuGet package) which can detect and warn about many code issues. The whole solution is also regularly analyzed 
with the [NDepend](https://www.ndepend.com/) tool, which detects all sorts of anti-patterns (circular dependencies, 
naming inconsistencies...).

Tests rely on the [NUnit](https://nunit.org/) solution, and test coverage is provided by
 [JetBrains dotCover](https://www.jetbrains.com/dotcover/). Benchmarks are powered by [BenchmarkDotNet](https://benchmarkdotnet.org/),
documentation is built with Microsoft's [DocFX](https://dotnet.github.io/docfx/) tool.

The client uses the [Hazelcast Open Binary Client Protocol](http://github.com/hazelcast/hazelcast-client-protocol/).

## Building On Windows

### Requirements



### Building

The solution builds in Visual Studio or Rider.

For a complete build,

From a Powershell console, build with:

```powershell
PS> build/build.ps1 <args>
```

See the build script section below for details and arguments.

## Building On Linux

### Requirements

DotNet Core (version?) must be installed (see [.NET Core on Linux Debian](https://docs.microsoft.com/en-us/dotnet/core/install/linux-debian) for instructions
for Debian).

Powershell (version?) must be installed (see [Installing Powershell Core on Linux](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-core-on-linux)
for instructions).

In order to run tests, Java and Maven are required.

Debian provides packages for recent OpenJDK version (11 and above) but not for the required version 8. Install version 8
via [AdoptOpenJDK](https://adoptopenjdk.net/). According to [this page](https://linuxize.com/post/install-java-on-debian-10/)
the following should be enough:
```sh
# get add-apt-repository
apt-get install software-properties-common

# get the key and add the repository
wget -qO - https://adoptopenjdk.jfrog.io/adoptopenjdk/api/gpg/key/public | apt-key add -
add-apt-repository --yes https://adoptopenjdk.jfrog.io/adoptopenjdk/deb/

# update and install
apt-update
apt-get install adoptopenjdk-8-hotspot
```

However, due to [this issue](https://github.com/AdoptOpenJDK/openjdk-infrastructure/issues/1399) you may have to do:
```sh
wget https://adoptopenjdk.jfrog.io/adoptopenjdk/deb/pool/main/a/adoptopenjdk-8-hotspot/adoptopenjdk-8-hotspot_8u252-b09-2_amd64.deb
apt install ./adoptopenjdk-8-hotspot_8u252-b09-2_amd64.deb

# select the default Java to be 8
update-alternatives --config java
```

Maven can be installed on Debian with:
```sh
$ apt-get install maven
```

### Building

From a shell console, build with:

```sh
$ pwsh build/build.ps1 <args>
```

See the build script section below for details and arguments.

At the moment it is not possible to build the documentation on Linux, as DocFX does not run on .NET Core yet
(see [this issue](https://github.com/dotnet/docfx/issues/138) for details). The upcoming v3 of DocFX will run
on .NET Core.

## Build Script

The `build.ps1` build script is common to Windows and Linux. It accepts the following arguments:

* `-enterprise` whether to test enterprise features (requires an enterprise key)
* `-netcore` on Windows, whether to test ???
* `-serverVersion <version>` the server version to use for tests
* `-targets <targets>` build targets (see below)

Build targets is a comma-separated list of values. Order is not important. Supported values are:
* 'build' builds the application
* 'docs' builds the documentation
* 'tests' runs the tests
* 'coverage' runs the tests coverage
* 'nuget' builds the NuGet package

etc (document)