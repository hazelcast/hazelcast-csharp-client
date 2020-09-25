## Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
##
## Licensed under the Apache License, Version 2.0 (the "License");
## you may not use this file except in compliance with the License.
## You may obtain a copy of the License at
##
## http://www.apache.org/licenses/LICENSE-2.0
##
## Unless required by applicable law or agreed to in writing, software
## distributed under the License is distributed on an "AS IS" BASIS,
## WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
## See the License for the specific language governing permissions and
## limitations under the License.

## Hazelcast.NET Build Script

param (

    # Commands.
    # (make sure it remains in the first position)
    [string[]]
    $commands = @( "clean", "build", "docsIf", "tests" ),

    # Whether to test enterprise features.
    [switch]
    $enterprise = $false,

    # The Hazelcast server version.
    [string]
    $server = "4.0",

    # Target framework(s).
    [Alias("f")]
    [string]
    $framework, # defaults to all

    # Configuration.
    # May need "Debug" for testing and covering things such as HConsole.
    [Alias("c")]
    [string]
    $configuration = "Release",

    # Tests filter.
    # Can use eg "namespace==Hazelcast.Tests.Core" to only run and cover some tests.
    # NUnit selection: https://docs.nunit.org/articles/nunit/running-tests/Test-Selection-Language.html
    #  test|name|class|namespace|method|cat ==|!=|=~|!~ 'value'|/value/|"value"
    #  "class == /Hazelcast.Tests.Networking.NetworkAddressTests/"
    #  "test == /Hazelcast.Tests.Networking.NetworkAddressTests/Parse"
    # DotCover filter: https://www.jetbrains.com/help/dotcover/Running_Coverage_Analysis_from_the_Command_LIne.html#filters
    #  +:<select>=<value> -:<select>=<value> separated with ';'
    #  eg -:module=AdditionalTests;-:type=MainTests.Unit*;-:type=MainTests.IntegrationTests;function=TestFeature1;
    #  <select> can be: +:module=*;class=*;function=*;  -:myassembly  * is supported
    [Alias("tf")]
    [string]
    $testFilter,

    [Alias("cf")]
    [string]
    $coverageFilter,

    # Whether to sign the assembly
    [switch]
    $sign = $false,

    # Whether to cover the tests
    [switch]
    $cover = $false,

    # Version to build
    [string]
    $version # defaults to what's in src/Directory.Build.props
)

# die - PowerShell display of errors is a pain
function Die($message) {
    [Console]::Error.WriteLine()
    [Console]::ForegroundColor = 'red'
    [Console]::Error.WriteLine($message)
    [Console]::ResetColor()
    [Console]::Error.WriteLine()
    Exit
}

# process commands
# in case it was passed by a script and not processed as an array
# PowerShell can be weird at times ;(
if ($commands.Length -eq 1 -and $commands[0].Contains(',')) {
    $commands = $commands[0].Replace(" ", "").Split(',')
}

# clear rogue environment variable
$env:FrameworkPathOverride=""

# this will be SystemDefault by default, and on some oldish environment (Windows 8...) it
# may not enable Tls12 by default, and use Tls10, and that will prevent us from connecting
# to some SSL/TLS servers (for instance, NuGet) => explicitly add Tls12
[Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol `
    -bor [Net.SecurityProtocolType]::Tls12

# determine platform
$platform = "windows"
if ($isLinux) { $platform = "linux" }
if ($isWindows) { $platform = "windows" }
if ($isMacOS) { $platform = "macOS" }
if (-not $isWindows -and $platform -eq "windows") { $isWindows = $true }

# validate commands and define actions ($doXxx)
foreach ($t in $commands) {
    switch ($t.Trim().ToLower()) {
        "help" {
            Write-Output "Hazelcast .NET Command Line"
            Write-Output ""
            Write-Output "usage hz.[ps1|sh] [<option>] [<commands>]"
            Write-Output ""
            Write-Output "<commands> is a csv list of:"
            Write-Output "  clean       : cleans the solution"
            Write-Output "  build       : builds the solution"
            Write-Output "  docs        : builds the documentation"
            Write-Output "  docsIf      : builds the documentation if supported by platform"
            Write-Output "  tests       : runs the tests"
            Write-Output "  cover       : when running tests, also perform code coverage analysis"
            Write-Output "  nuget       : builds the NuGet package(s)"
            Write-Output "  nupush      : pushes the NuGet package(s)"
            Write-Output "  rc          : runs the remote controller for tests"
            Write-Output "  server      : runs a server for tests"
            Write-Output "  docsServe   : serves the documentation (alias: ds)"
            Write-Output "  failedTests : details failed tests (alias: ft)"
            Write-Output ""
            Write-Output "When no command is specified, the script does 'clean,build,docsIf,tests'. Note that"
            Write-Output "building the documentation is not supported on non-Windows platforms as DocFX is not"
            Write-Output "supported on .NET Core yet."
            Write-Output ""
            Write-Output "<options> is:"
            Write-Output "  -enterprise                    : whether to run enterprise tests"
            Write-Output "  -sign                          : whether to sign assemblies"
            Write-Output "  -cover                         : whether to do test coverage"
            Write-Output "  -server <version>              : the server version for tests"
            Write-Output "  -framework <version>           : the framework to build (default is all, alias: -f)"
            Write-Output "  -configuration <configuration> : the build configuration (default is Release, alias: -c)"
            Write-Output "  -testFilter <filter>           : a test filter (default is all, alias: -tf)"
            Write-Output "  -coverageFilter <filter>       : a coverage filter (default is all, alias: -cf)"
            Write-Output "  -version <version>             : the version to build"
            Write-Output ""
            Write-Output "Server <version> must match a released Hazelcast IMDG server version, e.g. 4.0 or"
            Write-Output "4.1-SNAPSHOT. Server JARs are automatically downloaded for tests."
            Write-Output ""
            Write-Output "Framework <version> must match a valid .NET target framework moniker, e.g. net462"
            Write-Output "or netcoreapp3.1. Check the project files (.csproj) for supported versions."
            Write-Output ""
            Write-Output "The <version> to build must be a valid SemVer version such as 3.2.1 or 6.7.8-preview.2,"
            Write-Output "if no value is specified then the version is obtained from src/Directory.Build.props."
            Write-Output ""
            Write-Output "Configuration is Release by default but can be forced to Debug."
            Write-Output ""
            Write-Output "Signing assemblies requires the privage signing key in build/hazelcast.snk file."
            Write-Output ""
            Write-Output "Running enterprise tests require an enterprise key, which can be supplied either"
            Write-Output "via the HAZELCAST_ENTERPRISE_KEY environment variable, or the build/enterprise.key"
            Write-Output "file."
            Write-Output ""
            Write-Output "Pushing the NuGet packages requires a NuGet API key, via the NUGET_API_KEY"
            Write-Output "environment variable."
            Write-Output ""
            Write-Output "Test <filter> can be used to filter the tests to run, it must respect the NUnit test"
            Write-Output "selection language, which is detailed at:"
            Write-Output "https://docs.nunit.org/articles/nunit/running-tests/Test-Selection-Language.html"
            Write-Output "Example: -tf `"test == /Hazelcast.Tests.NearCache.NearCacheRecoversFromDistortionsTest/`""
            Write-Output ""
            Write-Output "Coverage <filter> can be used to filter the tests to cover, it must respect the"
            Write-Output "dotCover language, which is detailed at:"
            Write-Output "https://www.jetbrains.com/help/dotcover/Running_Coverage_Analysis_from_the_Command_LIne.html#filters"
            Write-Output ""
            exit 0
		}
        "clean"       { $doClean = $true }
        "build"       { $doBuild = $true }
        "docs"        { $doDocs = $true }
        "docsIf"      { $doDocs = $isWindows }
        "tests"       { $doTests = $true }
        "nuget"       { $doNuget = $true }
        "nupush"      { $doNupush = $true }
        "rc"          { $doRc = $true }
        "server"      { $doServer = $true }
        "docsServe"   { $doDocsServe = $true }
        "ds"          { $doDocsServe = $true }
        "failedtests" { $doFailedTests = $true }
        "ft"          { $doFailedTests = $true }
        default {
            Die "Unknown command '$($t.Trim())' - use 'help' to list valid commands."
        }
    }
}

# validate the version to build
$hasVersion = $false
if (-not [System.String]::IsNullOrWhiteSpace($version)) {

    if (-not ($version -match '^(\d+\.\d+\.\d+)(?:\-([a-z0-9\.\-]*))?$')) {
        Die "Version `"$version`" is not a valid SemVer version"
    }

    $versionPrefix = $Matches.1
    $versionSuffix = $Matches.2
    $hasVersion = $true
}

# set versions and configure
$hzVersion = $server
$hzRCVersion = "0.7-SNAPSHOT" # use appropriate version
$hzLocalBuild = $false # $true to skip downloading dependencies
$hzToolsCache = 12 #days
$hzVsMajor = 16 # force VS major version, default to 16 (VS2019) for now
$hzVsPreview = $false # whether to look for previews of VS

# determine java code repositories for tests
$mvnOssSnapshotRepo = "https://oss.sonatype.org/content/repositories/snapshots"
$mvnEntSnapshotRepo = "https://repository.hazelcast.com/snapshot/"
$mvnOssReleaseRepo = "http://repo1.maven.apache.org/maven2"
$mvnEntReleaseRepo = "https://repository.hazelcast.com/release/"

if ($server.Contains("SNAPSHOT")) {
    $mvnOssRepo = $mvnOssSnapshotRepo
    $mvnEntRepo = $mvnEntSnapshotRepo
} else {
    $mvnOssRepo = $mvnOssReleaseRepo
    $mvnEntRepo = $mvnEntReleaseRepo
}

# prepare directories
$scriptRoot = "$PSScriptRoot" # expected to be ./build/
$slnRoot = [System.IO.Path]::GetFullPath("$scriptRoot")

$srcDir = [System.IO.Path]::GetFullPath("$slnRoot/src")
$tmpDir = [System.IO.Path]::GetFullPath("$slnRoot/temp")
$outDir = [System.IO.Path]::GetFullPath("$slnRoot/temp/output")
$docDir = [System.IO.Path]::GetFullPath("$slnRoot/doc")
$buildDir = [System.IO.Path]::GetFullPath("$slnRoot/build")

if ($isWindows) { $userHome = $env:USERPROFILE } else { $userHome = $env:HOME }

# validate commands / platform
if ($doDocs -and -not $isWindows) {
    Die "DocFX is not supported on platform '$platform', cannot build documentation."
}

# validate enterprise key
$enterpriseKey = $env:HAZELCAST_ENTERPRISE_KEY
if (($doTests -or $doRc) -and $enterprise -and [System.String]::IsNullOrWhiteSpace($enterpriseKey)) {

    if (test-path "$buildDir/enterprise.key") {
        $enterpriseKey = (gc "$buildDir/enterprise.key")[0].Trim()
        $env:HAZELCAST_ENTERPRISE_KEY = $enterpriseKey
    }
    else {
        Die "Enterprise features require an enterprise key, either in`n- HAZELCAST_ENTERPRISE_KEY environment variable, or`n- $buildDir/enterprise.key file."
    }
}

# validate nuget key
$nugetApiKey = $env:NUGET_API_KEY
if ($doNupush -and [System.String]::IsNullOrWhiteSpace($nugetApiKey)) {
    Die "Pushing to NuGet requires a NuGet API key in NUGET_API_KEY environment variable."
}

# determine framework(s)
$frameworks = @( "net462", "netcoreapp2.1", "netcoreapp3.1" )
if (-not $isWindows) {
    $frameworks = @( "netcoreapp2.1", "netcoreapp3.1" )
}
if (-not [System.String]::IsNullOrWhiteSpace($framework)) {
    $framework = $framework.ToLower()
    if (-not $frameworks.Contains($framework)) {
        Die "Framework '$framework' is not supported on platform '$platform', supported frameworks are: $([System.String]::Join(", ", $frameworks))."
    }
    $frameworks = @( $framework )
}

# determine tests categories
if(!($enterprise)) {
    if (-not [System.String]::IsNullOrWhiteSpace($testFilter)) { $testFilter += " && " } else { $testFilter = "" }
    $testFilter += "cat != enterprise"
}

 # do not cover tests themselves, nor the testing plumbing
if (-not [System.String]::IsNullOrWhiteSpace($coverageFilter)) { $coverageFilter += ";" }
$coverageFilter += "-:Hazelcast.Net.Tests;-:Hazelcast.Net.Testing"

# set server version (to filter tests)
$env:HAZELCAST_SERVER_VERSION=$server.TrimEnd("-SNAPSHOT")

# finds latest version of a NuGet package in the ~/.nuget cache
function findLatestVersion($path) {
    if ([System.IO.Directory]::Exists($path)) {
        $v = gci $path | `
            foreach-object { [Version]::Parse($_.Name) } | `
                sort -descending | `
                select -first 1
    }
    else {
        $l = [System.IO.Path]::GetDirectoryname($path).Length
        $l = $path.Length - $l
        $v = gci "$path.*" | `
                foreach-object { [Version]::Parse($_.Name.SubString($l)) } | `
                    sort -descending | `
                    select -first 1
    }
    return $v
}

# ensures that a command exists in the path
function ensureCommand($command) {
    $r = get-command $command 2>&1
    if ($r.Name -eq $null) {
        Die "Command '$command' is missing."
    }
    else {
        Write-Output "Detected $command"
        Write-Output "  at '$($r.Source)'"
    }
}

# ensure we have NuGet
function ensureNuGet() {
    if (-not $hzLocalBuild)
    {
        $source = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
        if ((test-path $nuget) -and ((ls $nuget).CreationTime -lt [DateTime]::Now.AddDays(-$hzToolsCache)))
        {
            Remove-Item $nuget -force -errorAction SilentlyContinue > $null
        }
        if (-not (test-path $nuget))
        {
            Write-Output "Download NuGet..."
            Invoke-WebRequest $source -OutFile $nuget
            if (-not $?) { Die "Failed to download NuGet." }
            Write-Output "  -> $nuget"
        }
        else {
            Write-Output "Detected NuGet"
            Write-Output "  at '$nuget'"
        }
    }
    elseif (-not (test-path $nuget))
    {
        Die "Failed to locate NuGet.exe."
    }
}

# ensure we have vswhere -> $script:vswhere
function ensureVsWhere() {
    Write-Output ""
    Write-Output "Detected VsWhere"
    $v = $vswhereVersion
    Write-Output "  v$v"
    $dir = "$userHome/.nuget/packages/vswhere/$v"
    $vswhere = "$dir/tools/vswhere.exe"
    Write-Output "  at '$vswhere'"

    $script:vswhere = $vswhere
}

# find MsBuild -> $script:msBuild
function ensureMsBuild() {
    $vsPath = ""
    $vsVer = ""
    $msBuild = ""

    $vsMajor = $hzVsMajor

    $vsPaths = new-object System.Collections.Generic.List[System.String]
    $vsVersions = new-object System.Collections.Generic.List[System.Version]
    $vsNames = new-object System.Collections.Generic.List[System.String]

    Write-Output ""

    # parse vswhere output
    $params = @()
    if ($hzVsPreview) { $params += "-prerelease" }
    &$vswhere @params | ForEach-Object {
        if ($_.StartsWith("installationPath:")) { $vsPaths.Add($_.SubString("installationPath:".Length).Trim()) }
        if ($_.StartsWith("installationVersion:")) { $vsVersions.Add([System.Version]::Parse($_.SubString("installationVersion:".Length).Trim())) }
        if ($_.StartsWith("displayName:")) { $vsNames.Add($_.SubString("displayName:".Length).Trim()) }
    }

    # get higest version lower than or equal to vsMajor
    $vsIx1 = -1
    $vsIx2 = -1
    $vsVersion = [System.Version]::Parse("0.0.0.0")
    $vsVersions | ForEach-Object {

        $vsIx1 = $vsIx1 + 1
        Write-Output "Detected $($vsNames[$vsIx1]) ($_)"
        Write-Output "  at '$($vsPaths[$vsIx1])'"

        if ($_.Major -le $vsMajor -and $_ -gt $vsVersion) {
            $vsVersion = $_
            $vsIx2 = $vsIx1
        }
    }
    if ($vsIx2 -ge 0) {
        $vsPath = $vsPaths[$vsIx2]
        $vsName = $vsNames[$vsIx2]

        if ($vsVersion.Major -eq 16) {
            $msBuild = "$vsPath/MSBuild/Current/Bin/MsBuild.exe"
        }
        elseif ($vsVersion.Major -eq 15) {
            $msBuild = "$vsPath/MSBuild/$($vsVersion.Major).0/Bin/MsBuild.exe"
        }
    }

    gci 'C:/Program Files (x86)/Microsoft Visual Studio/*/BuildTools/MSBuild/*/Bin/MSBuild.exe' | ForEach-Object {
        $msBuildVersion = &$_ -nologo -version
        $msBuildVersion = [System.Version]::Parse($msBuildVersion)
        $msBuildExe = $_.FullName.Replace('\', '/')
        $toolsPath = $msBuildExe.SubString(0, $msBuildExe.IndexOf("/MSBuild/"))
        $toolsYear = $msBuildExe.SubString('C:/Program Files (x86)/Microsoft Visual Studio/'.Length, 4)
        Write-Output "Detected BuildTools $toolsYear ($msBuildVersion)"
        Write-Output "  at '$toolsPath'"

        if ($msBuildVersion.Major -le $vsMajor -and $msBuildVersion -gt $vsVersion)  {
            $msBuild = $msBuildExe
            $vsVersion = $msBuildVersion
            $vsName = "BuildTools "
        }
    }

    if (-not (test-path $msBuild)) {
        Die "Failed to locate MsBuild.exe."
    }
    else {
        Write-Output "Selecting $vsName ($vsVersion)"
        Write-Output "  at '$msBuild'"
    }

    $script:msBuild = $msBuild
}

# ensure docfx -> $script:docfx
function ensureDocFx() {
    Write-Output ""
    Write-Output "Detected DocFX"
    $v = $docfxVersion
    Write-Output "  v$v"
    $dir = "$userHome/.nuget/packages/docfx.console/$v"
    $docfx = "$dir/tools/docfx.exe"
    Write-Output "  at '$docfx'"

    $script:docfx = $docfx
}

# ensure memberpage
function ensureMemberPage() {
    Write-Output ""
    Write-Output "Detected DocFX MemberPage"
    $v = $memberpageVersion
    Write-Output "  v$v"
    $dir = "$userHome/.nuget/packages/memberpage/$v"
    Write-Output "  -> $dir"
}

function ensureJar($jar, $repo, $artifact) {
    if(Test-Path "$tmpDir/lib/$jar") {
        Write-Output "Detected $jar"
    } else {
        Write-Output "Downloading $jar ..."
        &"mvn" -q "dependency:get" "-DrepoUrl=$repo" "-Dartifact=$artifact" "-Ddest=$tmpDir/lib/$jar"
    }
    $s = ";"
    if (-not $isWindows) { $s = ":" }
    $classpath = $script:classpath
    if (-not [System.String]::IsNullOrWhiteSpace($classpath)) { $classpath += $s }
    $classpath += "$tmpDir/lib/$jar"
    $script:classpath = $classpath
}

# say hello
Write-Output "Hazelcast .NET Command Line"
Write-Output ""

if ($doBuild) {
    Write-Output "Build"
    Write-Output "  Platform       : $platform"
    Write-Output "  Commands       : $commands"
    Write-Output "  Configuration  : $configuration"
    Write-Output "  Framework      : $([System.String]::Join(", ", $frameworks))"
    Write-Output "  Building to    : $outDir"
    Write-Output "  Sign code      : $sign"
    if ($hasVersion) {
        Write-Output "  Version        : $version"
    }
    Write-Output ""
}

if ($doTests) {
    Write-Output "Tests"
    Write-Output "  Server version : $server"
    Write-Output "  Enterprise     : $enterprise"
    Write-Output "  Filter         : $testFilter"
    Write-Output "  Results        : $tmpDir/tests/results"
    Write-Output ""
}

if ($doTests -and $cover) {
    Write-Output "Tests Coverage"
    Write-Output "  Filter         : $coverageFilter"
    Write-Output "  Reports        : $tmpDir/tests/cover"
    Write-Output ""
}

if ($doNuget) {
    Write-Output "Nuget Package"
    Write-Output "  Configuration  : $configuration"
    Write-Output "  To             : $tmpDir/output"
    Write-Output ""
}

if ($doRc) {
    Write-Output "Remote Controller"
    Write-Output "  Server version : $server"
    Write-Output "  Enterprise     : $enterprise"
    Write-Output "  Logging to     : $tmpDir/rc"
    Write-Output ""
}

if ($doServer) {
    Write-Output "Server"
    Write-Output "  Server version : $server"
    Write-Output "  Enterprise     : $enterprise"
    Write-Output "  Configuration  : $buildDir/hazelcast-$hzVersion.xml"
    Write-Output "  Logging to     : $tmpDir/server"
    Write-Output ""
}

if ($doDocsServe) {
    Write-Output "Documentation Server"
    Write-Output "  Path           : $tmpdir/docfx.site"
    Write-Output ""
}

if ($doFailedTests) {
    Write-Output "Failed Tests"
    Write-Output "  Path           : $tmpdir/tests/results"
    Write-Output ""
}

# cleanup, prepare
if ($doClean) {
    Write-Output ""
    Write-Output "Clean solution..."

    # remove all the bins and objs recursively
    gci $slnRoot -include bin,obj -Recurse | foreach ($_) {
        Write-Output "  $($_.fullname)"
        remove-item $_.fullname -Force -Recurse
    }

    # clears output
    if (test-path $outDir) {
        Write-Output "  $outDir"
        remove-item $outDir -force -recurse
    }

    # clears tests (results, cover...)
    if (test-path "$tmpDir/tests") {
        Write-Output "  $tmpDir/tests"
        remove-item "$tmpDir/tests" -force -recurse
    }

    # clears logs (server, rc...)
    if (test-path "$tmpDir") {
        gci $tmpDir -include *.log -Recurse | foreach ($_) {
            Write-Output "  $($_.fullname)"
            remove-item $_.fullname -Force
        }
    }

    # clears docs
    if (test-path "$tmpDir/docfx.site") {
        Write-Output "  $tmpDir/docfx.site"
        remove-item "$tmpDir/docfx.site" -force -recurse
    }

    Write-Output ""
}

if (-not (test-path $tmpDir)) { mkdir $tmpDir >$null }
if (-not (test-path $outDir)) { mkdir $outDir >$null }

# ensure we have NuGet
if ($isWindows) {
    $nuget = "$tmpDir/nuget.exe"
    ensureNuGet
}

# ensure we have dotnet for build and tests
if ($doBuild -or -$doTests) {
  ensureCommand "dotnet"
  $sdks = (&dotnet --list-sdks)
  $v21 = ($sdks | select-string -pattern "^2\.1" | foreach-object { $_.ToString().Split(' ')[0] } | select -last 1)
  if ($v21 -eq $null) {
        Die "Could not find dotnet SDK version 2.1.x"
  }
  $v31 = ($sdks | select-string -pattern "^3\.1" | foreach-object { $_.ToString().Split(' ')[0] } | select -last 1)
  if ($v31 -eq $null) {
        Die "Could not find dotnet SDK version 3.1.x"
  }
}

# use NuGet to ensure we have the required packages for building and testing
Write-Output ""
Write-Output "Restore NuGet packages for building and testing..."
if ($isWindows) {
    &$nuget restore "$buildDir/build.proj" -Verbosity Quiet
}
else {
    dotnet restore "$buildDir/build.proj"
}

# get the required packages version (as specified in build.proj)
$buildAssets = (get-content "$buildDir/obj/project.assets.json" | ConvertTo-Json | ConvertFrom-Json).Value | ConvertFrom-Json
$buildLibs = $buildAssets.Libraries
$buildLibs.PSObject.Properties.Name | Foreach-Object {
    $p = $_.Split('/')
    $name = $p[0].ToLower()
    $version = $p[1]
    if ($name -eq "vswhere") { $vswhereVersion = $version }
    if ($name -eq "nunit.consolerunner") { $nunitVersion = $version }
    if ($name -eq "jetbrains.dotcover.commandlinetools") { $dotcoverVersion = $version }
    if ($name -eq "docfx.console") { $docfxVersion = $version }
    if ($name -eq "memberpage") { $memberpageVersion = $version }
}

if ($doBuild -and $isWindows) {
    $vswhere = ""
    ensureVsWhere
    $msBuild = ""
    ensureMsBuild
}

# ensure we can sign
if ($doBuild -and $sign) {
    if (!(test-path "$buildDir\hazelcast.snk")) {
        Die "Cannot sign code, missing key file $buildDir\hazelcast.snk"
    }
}

# ensure we have docfx for documentation
if ($doDocs -or $doDocsServe) {
    ensureDocFx
    ensureMemberPage
}

# ensure Java and Maven for tests
$java = "java"
$javaFix=@()
if ($isWindows) { $java = "javaw" }
if ($doTests -or $doRc -or $doServer) {
    Write-Output ""
    ensureCommand $java
    ensureCommand "mvn"

    # sad java
    $v = & java -version 2>&1
    $v = $v[0].ToString()
    $p0 = $v.IndexOf('"')
    $p1 = $v.LastIndexOf('"')
    $v = $v.SubString($p0+1,$p1-$p0-1)

    Write-Output ""
    Write-Output "Running Java v$v"

    if (-not $v.StartsWith("1.8")) {
        # starting with Java 9 ... weird things can happen
        $javaFix = @( "-Dcom.google.inject.internal.cglib.\$experimental_asm7=true",  "--add-opens java.base/java.lang=ALL-UNNAMED" )
        $env:MAVEN_OPTS="-Dcom.google.inject.internal.cglib.\$experimental_asm7=true --add-opens java.base/java.lang=ALL-UNNAMED"

        $javaFix = $javaFix + ( `
            `
            "--add-modules", "java.se", `
            "--add-exports", "java.base/jdk.internal.ref=ALL-UNNAMED", `
            "--add-opens",   "java.base/java.lang=ALL-UNNAMED", `
            "--add-opens",   "java.base/java.nio=ALL-UNNAMED",  `
            "--add-opens",   "java.base/sun.nio.ch=ALL-UNNAMED", `
            "--add-opens",   "java.management/sun.management=ALL-UNNAMED", `
            "--add-opens",   "jdk.management/com.sun.management.internal=ALL-UNNAMED", `
            `
            "--add-opens",   "java.base/java.io=ALL-UNNAMED" `
        )
	}
}

# build the solution
# on Windows, build with MsBuild - else use dotnet
if ($doBuild) {
    Write-Output ""
    Write-Output "Build solution..."
    if ($isWindows) {
        $msBuildArgs = @(
            "$slnRoot/Hazelcast.Net.sln", `
            "/p:Configuration=$configuration", `
            "/target:`"Restore;Build`""
            #/p:TargetFramework=$framework
        )

        if ($signAssembly) {
            $msBuildArgs += "/p:SignAssembly=true"
            $msBuildArgs += "/p:PublicSign=false"
            $msBuildArgs += "/p:AssemblyOriginatorKeyFile=`"$buildDir\hazelcast.snk`""
        }

        if ($hasVersion) {
            $msBuildArgs += "/p:AssemblyVersion=$versionPrefix"
            $msBuildArgs += "/p:FileVersion=$versionPrefix"
            $msBuildArgs += "/p:VersionPrefix=$versionPrefix"
            $msBuildArgs += "/p:VersionSuffix=$versionSuffix"
        }

        &$msBuild $msBuildArgs
    }
    else {
        $buildArgs = @(
            "$slnRoot/Hazelcast.Net.sln", `
            "-c", "$configuration"
            # "-f", "$framework"
        )
        if ($hasVersion) {
            $packArgs += "/p:AssemblyVersion=$versionPrefix"
            $packArgs += "/p:FileVersion=$versionPrefix"
            $packArgs += "/p:VersionPrefix=$versionPrefix"
            $packArgs += "/p:VersionSuffix=$versionSuffix"
        }

        dotnet build $buildArgs
    }

    # if it failed, we can stop here
    if ($LASTEXITCODE) {
        Die "Build failed, aborting."
    }
}

# build documentation
if ($doDocs) {
    Write-Output ""
    Write-Output "Build documentation..."

    # clear target
    if (test-path "$tmpDir/docfx.site") {
        remove-item -recurse -force "$tmpDir/docfx.site"
    }

    $template = "default,$userHome/.nuget/packages/memberpage/$memberpageVersion/content,$docDir/templates/hz"
    Write-Output $template

    &$docfx metadata "$docDir/docfx.json" # --disableDefaultFilter
    &$docfx build "$docDir/docfx.json" --template $template
}

function getJavaKerberosArgs() {
    return @( `
        "-Djava.util.logging.config.file=krb5/logging.properties", `
        "-Djava.security.krb5.realm=HZ.LOCAL", `
        "-Djava.security.krb5.kdc=SERVER19.HZ.LOCAL", `
        "-Djava.security.krb5.conf=krb5/krb5.conf" `
    )
}

function StartRemoteController() {

    if (-not (test-path "$tmpDir/rc")) { mkdir "$tmpDir/rc" >$null }

    Write-Output ""
    Write-Output "Starting Remote Controller..."

    # start the remote controller
    $args = @( `
        "-Dhazelcast.enterprise.license.key=$enterpriseKey", `
        "-cp", "$script:classpath", `
        "com.hazelcast.remotecontroller.Main" `
    )

    $args = $args + $javaFix

    # uncomment to test Kerberos (but don't commit)
    #$args = $args + getJavaKerberosArgs

    $script:remoteController = Start-Process -FilePath $java -ArgumentList $args `
        -RedirectStandardOutput "$tmpDir/rc/stdout-$hzVersion.log" `
        -RedirectStandardError "$tmpDir/rc/stderr-$hzVersion.log" `
        -PassThru
    Start-Sleep -Seconds 4

    if ($script:remoteController.HasExited) {
        Die "Remote controller has exited immediately."
	}
    else {
        Write-Output "Started remote controller with pid=$($script:remoteController.Id)"
    }
}

function StartServer() {

    if (-not (test-path "$tmpDir/server")) { mkdir "$tmpDir/server" >$null }

    # ensure we have a configuration file
    if (!(test-path "$buildDir/hazelcast-$hzVersion.xml")) {
        Die "Missing server configuration file $buildDir/hazelcast-$hzVersion.xml"
    }

    Write-Output ""
    Write-Output "Starting Server..."

    # depending on server version, different starter class
    $mainClass = "com.hazelcast.core.server.HazelcastMemberStarter" # 4.0
    if ($server.StartsWith("3.")) {
        $mainClass = "com.hazelcast.core.server.StartServer" # 3.x
    }

    # start the server
    $args = @( `
        "-Dhazelcast.enterprise.license.key=$enterpriseKey", `
        "-cp", "$script:classpath", `
        "-Dhazelcast.config=$buildDir/hazelcast-$hzVersion.xml", `
        "-server", "-Xms2g", "-Xmx2g", "-Dhazelcast.multicast.group=224.206.1.1", "-Djava.net.preferIPv4Stack=true", `
        "$mainClass" `
    )

    $args = $args + $javaFix

    # uncomment to test Kerberos (but don't commit)
    #$args = $args + getJavaKerberosArgs

    $script:serverProcess = Start-Process -FilePath $java -ArgumentList $args `
        -RedirectStandardOutput "$tmpDir/server/stdout-$hzVersion.log" `
        -RedirectStandardError "$tmpDir/server/stderr-$hzVersion.log" `
        -PassThru
    Start-Sleep -Seconds 4

    if ($script:serverProcess.HasExited) {
        Die "Server has exited immediately."
	}
    else {
        Write-Output "Started server with pid=$($script:serverProcess.Id)"
    }
}

function StopRemoteController() {

    # stop the remote controller
    Write-Output ""
    if ($script:remoteController -and $script:remoteController.Id -and -not $script:remoteController.HasExited) {
        Write-Output "Stopping remote controller..."
        Stop-Process -Force -Id $script:remoteController.Id
        #$script:remoteController.Kill($true)
	}
    else {
        Write-Output "Remote controller is not running."
	}
}

function StopServer() {

    # stop the server
    Write-Output ""
    if ($script:serverProcess -and $script:serverProcess.Id -and -not $script:serverProcess.HasExited) {
        Write-Output "Stopping server..."
        Stop-Process -Force -Id $script:serverProcess.Id
        #$script:server.Kill($true)
	}
    else {
        Write-Output "Server is not running."
	}
}

function CollectTestResults($fwk, $file) {
    $script:testResults = $script:testResults + $file
}

function RunDotNetCoreTests($f) {

    # run .NET Core unit tests
    # note:
    #   on some machines (??) MSBuild does not copy the NUnit adapter to the bin directory,
    #   but the 'dotnet test' command does copy it, provided that we don't use the --no-build
    #   option - it does not do a full build anyways - just sets tests up
    #
    # used to have to do (but not anymore):
    #    <PackageReference Include="NunitXml.TestLogger" Version="2.1.62" />
    #    --logger:"nunit;LogFilePath=$tmpDir/tests/results/tests-$f.xml"
    # instead: http://blog.prokrams.com/2019/12/16/nunit3-filter-dotnet/
    #
    if ($cover) {
        $coveragePath = "$tmpDir/tests/cover/cover-$f"
        if (!(test-path $coveragePath)) {
            mkdir $coveragePath > $null
        }

        $dotCoverArgs = @( "test", `
            "$srcDir/Hazelcast.Net.Tests/Hazelcast.Net.Tests.csproj", `
            "-c", "$configuration", `
            "--no-restore", `
            "-f", "$f", `
            "-v", "normal", `
            `
            "--dotCoverFilters=$coverageFilter", `
            "--dotCoverAttributeFilters=System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute", `
            "--dotCoverOutput=$coveragePath/index.html", `
            "--dotCoverReportType=HTML", `
            `
            "--", `
            `
            "NUnit.WorkDirectory=`"$tmpDir/tests/results`"", `
            "NUnit.TestOutputXml=`".`"", `
            "NUnit.Labels=Before" )

        if ($testFilter -ne "") { $dotCoverArgs += "NUnit.Where=`"$testFilter`"" }

        Write-Output "exec: dotnet dotcover $dotCoverArgs"
        pushd "$srcDir/Hazelcast.Net.Tests"
        &dotnet dotcover $dotCoverArgs
        popd
    }
    else {
        $dotnetArgs = @( `
            "$srcDir/Hazelcast.Net.Tests/Hazelcast.Net.Tests.csproj", `
            "-c", "$configuration", `
            "--no-restore", `
            "-f", "$f", `
            "-v", "normal", `
            `
            "--", `
            `
            "NUnit.WorkDirectory=`"$tmpDir/tests/results`"", `
            "NUnit.TestOutputXml=`".`"", `
            "NUnit.Labels=Before" )

        if ($testFilter -ne "") { $dotnetArgs += "NUnit.Where=`"$testFilter`"" }

        Write-Output "exec: dotnet $dotnetArgs"
        &dotnet test $dotnetArgs
    }

    # NUnit adapter does not support configuring the file name, move
    if (test-path "$tmpDir/tests/results/Hazelcast.Net.Tests.xml") {
        move-item -force "$tmpDir/tests/results/Hazelcast.Net.Tests.xml" "$tmpDir/tests/results/results-$f.xml"
    }
    elseif (test-path "$tmpDir/tests/results/results-$f.xml") {
        rm "$tmpDir/tests/results/results-$f.xml"
    }
    CollectTestResults $f "$tmpDir/tests/results/results-$f.xml"
}

function RunDotNetFrameworkTests($f) {

    # run .NET Framework unit tests
    $testDLL="$srcDir/Hazelcast.Net.Tests/bin/${configuration}/${f}/Hazelcast.Net.Tests.dll"

    switch ($f) {
        "net462" { $nuf = "net-4.6.2" }
        default { Die "Framework '$f' not supported here." }
    }

    $v = $nunitVersion
    $nunit = "$userHome/.nuget/packages/nunit.consolerunner/$v/tools/nunit3-console.exe"
    $nunitArgs=@("`"${testDLL}`"", "--labels=Before", "--result=`"$tmpDir/tests/results/results-$f.xml`"", "--framework=$nuf")

    if ($testFilter -ne "") { $nunitArgs += @("--where=`"${testFilter}`"") }

    if ($cover) {

        $coveragePath = "$tmpDir/tests/cover/cover-$f"
        if (!(test-path $coveragePath)) {
            mkdir $coveragePath > $null
        }

        $v = $dotcoverVersion
        $dotCover = "$userHome/.nuget/packages/jetbrains.dotcover.commandlinetools/$v/tools/dotCover.exe"

        # note: separate attributes filters with ';'
        $dotCoverArgs = @( "cover", `
            "--Filters=$coverageFilter", `
            "--AttributeFilters=System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute", `
            "--TargetWorkingDir=.", `
            "--Output=$coveragePath/index.html", `
            "--ReportType=HTML", `
            "--TargetExecutable=${nunit}", `
            "--") + $nunitArgs

        Write-Output "exec: $dotCover $dotCoverArgs"
        &$dotCover $dotCoverArgs

    } else {

        Write-Output "exec: $nunit $nunitArgs"
        &$nunit $nunitArgs
    }

    CollectTestResults $f "$tmpDir/tests/results/results-$f.xml"
}

if ($doTests -or $doRc -or $doServer) {
   # prepare server/rc
    Write-Output ""
    Write-Output "Prepare server/rc..."
    if (-not (test-path "$tmpDir/lib")) { mkdir "$tmpDir/lib" >$null }
    [string]$classpath=""

    # ensure we have the remote controller + hazelcast test jar
    ensureJar "hazelcast-remote-controller-${hzRCVersion}.jar" $mvnOssSnapshotRepo "com.hazelcast:hazelcast-remote-controller:${hzRCVersion}"
    ensureJar "hazelcast-${hzVersion}-tests.jar" $mvnOssRepo "com.hazelcast:hazelcast:${hzVersion}:jar:tests"

    if ($enterprise) {
        # ensure we have the hazelcast enterprise server + test jar
        ensureJar "hazelcast-enterprise-${hzVersion}.jar" $mvnEntRepo "com.hazelcast:hazelcast-enterprise:${hzVersion}"
        ensureJar "hazelcast-enterprise-${hzVersion}-tests.jar" $mvnEntRepo "com.hazelcast:hazelcast-enterprise:${hzVersion}:jar:tests"
    } else {
        # ensure we have the hazelcast server jar
        ensureJar "hazelcast-${hzVersion}.jar" $mvnOssRepo "com.hazelcast:hazelcast:${hzVersion}"
    }
}

$testsSuccess = $true
if ($doTests) {

    # run tests
    $testResults = @()

    try {
        StartRemoteController

        Write-Output ""
        Write-Output "Run tests..."
        foreach ($framework in $frameworks) {
            Write-Output ""
            Write-Output "Run tests for $framework..."
            if ($framework -eq "net462") {
                # must run tests with .NET Framework for Framework tests
                RunDotNetFrameworkTests $framework
            }
            else {
                # anything else can run with 'dotnet test'
                RunDotNetCoreTests $framework
            }
        }
    }
    finally {
        StopRemoteController
    }

    Write-Output ""
    Write-Output "Summary:"
    foreach ($testResult in $testResults) {

        $fwk = [System.IO.Path]::GetFileNameWithoutExtension($testResult).TrimStart("result-")

        if (test-path $testResult) {
            $xml = [xml] (gc $testResult)

            $run = $xml."test-run"
            $total = $run.total
            $passed = $run.passed
            $failed = $run.failed
            $skipped = $run.skipped
            $inconclusive = $run.inconclusive

            Write-Output `
                "  $($fwk.PadRight(16)) :  total $total = $passed passed, $failed failed, $skipped skipped, $inconclusive inconclusive."

            if ($failed -gt 0) {

                $testsSuccess = $false

                foreach ($testCase in $run.SelectNodes("//test-case [@result='Failed']")) {
                    Write-Output "    $($testCase.fullname.TrimStart('Hazelcast.Net.')) failed"
                    if ($doFailedTests) {
                        Write-Output $testCase.failure.message.innerText
                        Write-Output $testCase.failure."stack-trace".innerText
                        Write-Output ""
                    }
                }
            }
        }
        else {
            $testsSuccess = $false
            Write-Output `
                "  $($fwk.PadRight(16)) :  FAILED (no test report)."
        }
    }
}

if ($doRc) {
    try {
        StartRemoteController

        Write-Output ""
        Write-Output "Remote controller is running..."
        Read-Host "Press ENTER to stop"
    }
    finally {
        StopRemoteController
    }
}

if ($doServer) {
    try {
        StartServer

        Write-Output ""
        Write-Output "Server is running..."
        Read-Host "Press ENTER to stop"
    }
    finally {
        StopServer
    }
}

if ($doDocsServe) {
    if (-not (test-path "$tmpDir\docfx.site")) {
        Die "Missing documentation directory."
    }

    Write-Output ""
    Write-Output "Documentation server is running..."
    Write-Output "Press ENTER to stop"
    &$docfx serve "$tmpDir\docfx.site"
}

if ($doNuget -and -not $testsSuccess) {
    Write-Output ""
    Write-Output "Tests failed, skipping building NuGet packages..."
    $doNuget = $false
}

if ($doNuget) {
    Write-Output ""
    Write-Output "Build NuGet packages..."

    # creates the nupkg (which contains Hazelcast.Net.dll)
    # creates the snupkg (which contains Hazelcast.Net.pdb with source code reference)
    # https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-pack

    $packArgs = @(
        "$srcDir\Hazelcast.Net\Hazelcast.Net.csproj", `
        "--no-build", "--nologo", `
        "-o", "$tmpDir\output", `
        "-c", "$configuration"
    )
    if ($hasVersion) {
        $packArgs += "/p:AssemblyVersion=$versionPrefix"
        $packArgs += "/p:FileVersion=$versionPrefix"
        $packArgs += "/p:VersionPrefix=$versionPrefix"
        $packArgs += "/p:VersionSuffix=$versionSuffix"
    }
    &dotnet pack $packArgs
}

if ($doNupush -and -not $testsSuccess) {
    Write-Output ""
    Write-Output "Tests failed, skipping pushing NuGet packages..."
    $doNupush = $false
}

if ($doNupush) {
    Write-Output ""
    Write-Output "Push NuGet packages..."

    if (-not $hasVersion)
    {
        $xml = [xml] (gc "$srcDir/Directory.Build.props")
        $versionPrefix = $xml.project.propertygroup.versionprefix | where { -not [System.String]::IsNullOrWhiteSpace($_) }
        $versionSuffix = $xml.project.propertygroup.versionsuffix | where { -not [System.String]::IsNullOrWhiteSpace($_) }
        $version = $versionPrefix.Trim()
        if (-not [System.String]::IsNullOrWhiteSpace($versionSuffix)) {
            $version += "-$($versionSuffix.Trim())"
        }
    }

    &$nuget push "$tmpDir\output\Hazelcast.Net.$version.nupkg" -ApiKey $nugetApiKey -Source "https://api.nuget.org/v3/index.json"
}

Write-Output ""
Write-Output "Done."

# eof
