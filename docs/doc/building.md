# Building

## Building On Windows

### Requirements

Although the solution builds in Visual Studio or Rider, a complete build requires Powershell, and
Visual Studio 2019 or at least the Visual Studio Build Tools 2019,
which you can download from the [Visual Studio](https://visualstudio.microsoft.com/) site.

### Building

For a complete build, start a Powershell console and build with:

```powershell
PS> build/build.ps1 <args>
```

See the build script section below for details and arguments.

## Building On Linux

It is possible to build the Hazelcast .NET library on Linux, along with .NET Core App 2.1 and 3.1 tests and examples. It
is not possible to build the .NET Framework 4.6.2 tests or examples, as .NET Framework is not supported on Linux.

At the moment it is not possible to build the documentation on Linux, as DocFX does not run on .NET Core yet
(see [this issue](https://github.com/dotnet/docfx/issues/138) for details). The upcoming v3 of DocFX will run
on .NET Core.

### Requirements

.NET Core must be installed (see [.NET Core on Linux Debian](https://docs.microsoft.com/en-us/dotnet/core/install/linux-debian) for instructions
for Debian).

Powershell must be installed (see [Installing Powershell Core on Linux](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-core-on-linux)
for instructions).

In order to run tests, Java and Maven are required. For Debian:

```sh
apt-get update
apt-get install openjdk-11-jre
apt-get install maven
```

If Maven gives warnings about "WARNING: An illegal reflective access operation has occurred", you may have to define
the following environment variable before building:

```sh
export MAVEN_OPTS="-Dcom.google.inject.internal.cglib.\$experimental_asm7=true --add-opens java.base/java.lang=ALL-UNNAMED"
```

### Building

From a shell console, build with:

```sh
$ pwsh build/build.ps1 <args>
```

See the build script section below for details and arguments.

## Build Script

The `build.ps1` build script is common to Windows and Linux. It accepts the following arguments:

* `-enterprise` whether to test enterprise features (requires an enterprise key)
* `-netcore` on Windows, whether to test ???
* `-serverVersion <version>` the server version to use for tests
* `-t[argets] <targets>` build targets (see below)

Build targets is a comma-separated list of values. Order is not important. Supported values are:
* 'build' builds the application
* 'docs' builds the documentation
* 'tests' runs the tests
* 'coverage' runs the tests coverage
* 'nuget' builds the NuGet package

When no target is specified, the script runs a complete build, including tests.

For example, after a complete build, one can rebuild the documentation with:

```powershell
PS> build/build.ps1 -t docs
```