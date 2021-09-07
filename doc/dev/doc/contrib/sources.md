# Sources

The source code for Hazelcast .NET is published on GitHub at [Hazelcast .NET](https://github.com/hazelcast/hazelcast-csharp-client). Clone the repository to get the development branch:

```sh
git clone --recurse-submodules https://github.com/hazelcast/hazelcast-csharp-client.git 
```

Note that the repository relies on Git [submodules](https://git-scm.com/book/en/v2/Git-Tools-Submodules), and therefore the `--recurse-submodules` is required.

## Branches

Development of new features takes place in the `master` branch. Maintenance of released versions take place in `X.Y.z` branches, e.g. version `4.1` is maintained in the `4.1.z` branch. 

## Tools

The code uses C# [version](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/configure-language-version) 8.0 (as per the `src/Directory.Build.props` file) though we plan to migrate to 9.0. The `Hazelcast.Net` package targets netstandard 2.0 and 2.1, and is supported on .NET Framework 4.6.2 and later, .NET Core 2.1 (LTS), .NET Core 3.1 (LTS).

The solution can be opened with Microsoft [Visual Studio](https://visualstudio.microsoft.com/) 2019 or JetBrains [Rider](https://www.jetbrains.com/rider/), but can also be fully built via our custom PowerShell script (see the [Building](building.md) page).

The main Hazelcast.Net project (which builds the library) is covered by Microsoft's [Roslyn analyzers](https://docs.microsoft.com/en-us/visualstudio/code-quality/roslyn-analyzers-overview) (installed via the [Microsoft.CodeAnalysis.FxCopAnalyzers](https://www.nuget.org/packages/Microsoft.CodeAnalysis.FxCopAnalyzers/) NuGet package) which can detect and warn about many code issues. The whole solution is also regularly analyzed with the [NDepend](https://www.ndepend.com/) tool, which detects all sorts of anti-patterns (circular dependencies, naming inconsistencies...).

Tests rely on the [NUnit](https://nunit.org/) solution. Test coverage is provided by
 [JetBrains dotCover](https://www.jetbrains.com/dotcover/) and results are published [here](../../cover/index.md). Benchmarks are powered by [BenchmarkDotNet](https://benchmarkdotnet.org/), documentation is built with Microsoft's [DocFX](https://dotnet.github.io/docfx/) tool.

The client uses the [Hazelcast Open Binary Client Protocol](http://github.com/hazelcast/hazelcast-client-protocol/). The protocol repository is included in the client repository as a Git [submodule](https://git-scm.com/book/en/v2/Git-Tools-Submodules) in order to keep track of *which* exact version of the protocol was used to build the codec files in the client.
