# Building

## Building On Windows

### Requirements

Although the solution builds in Visual Studio or Rider, a complete build requires Powershell, and Visual Studio 2019 or at least the Visual Studio Build Tools 2019, which you can download from the [Visual Studio](https://visualstudio.microsoft.com/) site.

.NET Core is also required. You can download it from the [Download .NET](https://dotnet.microsoft.com/download) page. Recent 2.1 and 3.1 SDKs are required. You can verify whether .NET Core is installed, and which versions are supported, by running `dotnet --info` in a command window.

### Building

For a complete build, start a Powershell console and build with:

```powershell
PS> ./hz.ps1 <options> <targets>
```

See the build script section below for details and arguments.

## Building On Linux

It is possible to build the Hazelcast .NET library on Linux, along with .NET Core App 2.1 and 3.1 tests and examples. It is not possible to build the .NET Framework 4.6.2 tests or examples, as .NET Framework is not supported on Linux.

At the moment it is not possible to build the documentation on Linux, as DocFX does not run on .NET Core yet (see [this issue](https://github.com/dotnet/docfx/issues/138) for details). The upcoming v3 of DocFX will run on .NET Core.

### Requirements

.NET Core must be installed (see [.NET Core on Linux Debian](https://docs.microsoft.com/en-us/dotnet/core/install/linux-debian) for instructions for Debian). Recent 2.1 and 3.1 SDKs are required. In addition, a recent 2.2 runtime is required by dotCover to run test coverage. You can verify whether .NET Core is installed, and which versions are supported, by running `dotnet --info` in a command window.

Powershell must be installed (see [Installing Powershell Core on Linux](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-core-on-linux)
for instructions).

In order to run tests, Java is required. For Debian:

```sh
apt-get update
apt-get install openjdk-11-jre
```

### Building

From a shell console, build with:

```sh
$ ./hz.sh <options> <targets>
```

See the build script section below for details and arguments.

## Build Script

On Linux, `hz.sh` is just a proxy to `hz.ps1`. The actual build is always performed by `hz.ps1`, which is common to Windows and Linux. It accepts the following options:

* `-enterprise` test enterprise features
* `-server <version>` the server version to use for tests
* `-framework <version>` the framework version to build (default: all)
* `-configuration <Release|Debug>` the configuration to build (default: `Release`)
* `-testFilter <filter>` filter for tests
* `-coverageFilter <filter>` filter for tests coverage

Server `<version>` must match a released Hazelcast IMDG server version, e.g. `4.0` or `4.1-SNAPSHOT`. Server JARs are automatically downloaded for tests.

Framework `<version>` must match a valid .NET target framework moniker, e.g. `net462` or `netcoreapp3.1`. Check the project files (`.csproj`) for supported versions.


Build targets is a comma-separated list of values. Order is not important. Supported values are:
* `clean` cleans the solution (removes all bin, obj, and temporary directories)
* `build` builds the solution
* `docs` builds the documentation
* `docsIf` builds the documentation if the platform supports it
* `tests` runs the tests
* `cover` when running the tests, also perform code coverage analysis
* `nuget` builds the NuGet package(s)
* `rc` runs the remote controller for tests
* `docsServe` serves the documentation site (alias: `ds`)
* `failedTests` outputs extra details about failed tests (alias: `ft`)

When no target is specified, the script runs `clean`, `build`, `docsIf` and `tests`.

For example, after a complete build, one can rebuild and serve the documentation with:

```powershell
PS> ./hz.ps1 docs,docsServe
```

When the `-enterprise` option is set, in order to test the enterprise features, the `HAZELCAST_ENTERPRISE_KEY` environment variable must contain a valid Hazelcast Enterprise key.

## SDK Selection

The `global.json` file at the root of the project contains:

```json
{
  "sdk": {
    "allowPrerelease": false
  }
}
```

This ensures that any use of the `dotnet` command actuallys use the lastest stable release installed on the machine, and avoids any pre-release versions, as these may break the build. Should you want to experiment with pre-releases of the .NET SDK, change `false` to `true` (but do not commit the change!).

For more details, see the [Select the .NET Core version to use](https://docs.microsoft.com/en-us/dotnet/core/versions/selection) and [global.json overview](https://docs.microsoft.com/en-us/dotnet/core/tools/global-json) articles from Microsoft.