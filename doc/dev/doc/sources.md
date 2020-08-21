# Sources

The source code for Hazelcast .NET is published on GitHub at [Hazelcast .NET](https://github.com/hazelcast/hazelcast-csharp-client). Clone the repository to get the development branch:

```sh
git clone https://github.com/hazelcast/hazelcast-csharp-client.git 
```

(need to document branches)

The solution can be opened with Microsoft [Visual Studio](https://visualstudio.microsoft.com/) 2019 or JetBrain [Rider](https://www.jetbrains.com/rider/). The code targets netstandard 2.0 and 2.1, and is using C# [version](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/configure-language-version) 8.0.

The main Hazelcast.Net project (which builds the library) is covered by Microsoft's [Roslyn analyzers](https://docs.microsoft.com/en-us/visualstudio/code-quality/roslyn-analyzers-overview) (installed via the [Microsoft.CodeAnalysis.FxCopAnalyzers](https://www.nuget.org/packages/Microsoft.CodeAnalysis.FxCopAnalyzers/) NuGet package) which can detect and warn about many code issues. The whole solution is also regularly analyzed with the [NDepend](https://www.ndepend.com/) tool, which detects all sorts of anti-patterns (circular dependencies, naming inconsistencies...).

Tests rely on the [NUnit](https://nunit.org/) solution, and test coverage is provided by
 [JetBrains dotCover](https://www.jetbrains.com/dotcover/). Benchmarks are powered by [BenchmarkDotNet](https://benchmarkdotnet.org/), documentation is built with Microsoft's [DocFX](https://dotnet.github.io/docfx/) tool.

The client uses the [Hazelcast Open Binary Client Protocol](http://github.com/hazelcast/hazelcast-client-protocol/). The protocol repository is included in the client repository as a Git [submodule](https://git-scm.com/book/en/v2/Git-Tools-Submodules) in order to keep track of *which* exact version of the protocol was used to build the codec files in the client.
