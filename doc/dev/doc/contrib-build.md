# Building

### Requirements

For day to day development, the solution builds in Visual Studio or Rider. However, it is possible to build it entirely via our custom PowerShell script. 

The minimal requires are:
* PowerShell 6.2+
* .NET 2.1, 3.1 and 5.0 SDKs
* Java runtime, if you want to run tests

Visual Studio 2019, or at least the Visual Studio Build Tools 2019, can be downloaded from the [Visual Studio](https://visualstudio.microsoft.com/) site. .NET can be downloaded from the [Download .NET](https://dotnet.microsoft.com/download) page. You can verify whether .NET Core is installed, and which versions are supported, by running `dotnet --info` in a command window.

PowerShell can be installed on Windows through the [Windows Store](https://www.microsoft.com/store/apps/9MZ1SNWT0N5D); the [PowerShell](https://microsoft.com/powershell) documentation describes other means of installation for Windows and the various flavours of Linux.

The [OpenJDK](https://openjdk.java.net/) provides open Java JDKs for Windows and Linux.

## Building On Windows

For a complete build, start a Powershell console and build using the `hz.ps1` script:

```powershell
PS> ./hs.ps1 build
```

See the build script section below for details and arguments.

## Building On Linux

For a complete build, from a shell console, use the `hz.sh` script:

```sh
$ ./hz.sh build
```

See the build script section below for details and arguments.

Note that `hs.sh` is just a proxy to `hz.ps1`: the actual build actions are always performed by `hz.ps1`, which is common to Windows and Linux.

It is not possible to build the .NET Framework version of the Hazelcast .NET client on Linux, as the .NET Framework is not supported on Linux. All other targets build on Linux. At the moment it is not possible to build the documentation on Linux, as DocFX does not run on .NET Core yet (see [this issue](https://github.com/dotnet/docfx/issues/138) for details). The upcoming v3 of DocFX will run on .NET Core.

## Build Script

The `hz.[hs|ps1]` script accepts options, commands, and command arguments.

```powershell
PS> ./hz.[sh|ps1] [<options>] [<commands>] [<commargs>] [--- <rawargs>]
```

To list all options and command, run `./hz.[sh|ps1] help`. 

Examples of valid usages:

```powershell
./hz.ps1 build                                 # builds the code
./hz.ps1 build,test                            # builds the code and run the tests
./hz.ps1 -cover test                           # runs the tests with test coverage
./hz.ps1 test -cover                           # same
./hz.ps1 set-version -version 1.2.3            # updates the version
./hz.ps1 run-remote-controller                 # runs a remote controller for tests
./hz.ps1 run-server -server 4.2                # runs version 4.2 of the server
./hz.ps1 run-example ~Soak1 --- --hazelcast.   # runs an example
```

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