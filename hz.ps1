## Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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

    # [Parameter] arguments:
    # ref: https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_functions_advanced_parameters?view=powershell-7.1
    # Mandatory
    # Position
    # ParameterSetName
    # ValueFromPipeline
    # ValueFromPipelineByPropertyName
    # ValueFromRemainingArguments
    # HelpMessages

    # Whether to test enterprise features.
    [switch]
    $enterprise = $false,

    # The Hazelcast default server version.
    # Stick with -SNAPSHOT so we always test against the latest snapshot by default,
    # otherwise we end up testing against test JARs with obsolete SSL certs, etc.
    [string]
    $server = "4.0-SNAPSHOT",

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

    # Test selector
    # Is a simplified version of $testFilter which ends up adding a test filter as
    # "name =~ /$framework.$test/"
    [alias("t")]
    [string]
    $test,

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
    $version, # defaults to what's in src/Directory.Build.props

    [alias("nr")]
    [switch]
    $noRestore, # don't restore NuGet global packages (assume they are there already)

    [alias("lr")]
    [switch]
    $localRestore, # restore NuGet packages locally

    [alias("d")]
    [string]
    $defineConstants, # define additional build constants

    [alias("cp")]
    [string]
    $classpath, # additional classpath for rc/server

    [alias("repro")]
    [switch]
    $reproducible,

    [string]
    $serverConfig,

    # Commands.
    [Parameter(ValueFromRemainingArguments, Position=0)]
    [string[]]
    $commands = @( "help" )
)

# die - PowerShell display of errors is a pain
function Die($message) {
    [Console]::Error.WriteLine()
    [Console]::ForegroundColor = 'red'
    [Console]::Error.WriteLine($message)
    [Console]::ResetColor()
    [Console]::Error.WriteLine()
    Exit 1
}

# say hello
Write-Output "Hazelcast .NET Command Line"

# process commands
# command can come as a string[1] containing "a b c", or containing "a, b, c",
# or as a real string[n] ... PowerShell can be weird at times ;(
if ($commands.Length -eq 1 -and $commands[0].Contains(',')) {
    $commands[0] = $commands[0].Replace(",", " ")
}
if ($commands.Length -eq 1 -and $commands[0].Contains(' ')) {
    $commands = $commands[0].Split(" ", [StringSplitOptions]::RemoveEmptyEntries)
}

# process define constants - it's a mess
# see https://github.com/dotnet/sdk/issues/9562
if (![string]::IsNullOrWhiteSpace($defineConstants)) {
    $defineConstants = $defineConstants.Replace(",", ";")
}

# clear rogue environment variable
$env:FrameworkPathOverride=""

# this will be SystemDefault by default, and on some oldish environment (Windows 8...) it
# may not enable Tls12 by default, and use Tls10, and that will prevent us from connecting
# to some SSL/TLS servers (for instance, NuGet) => explicitly add Tls12
[Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol `
    -bor [Net.SecurityProtocolType]::Tls12

# determine PowerShellVersion (see also $psVersionTable)
# PowerShell 2.0 is integrated since Windows 7 and Server 2008 R2
#            3.0                             8            2012
#            4.0                             8.1          2012 R2
#            5.0                             10
#            5.1                             10AU         2016
$psVersion = (get-host | select-object Version).Version
$minVersion = [System.Version]::Parse("5.1.0.0")
if ($psVersion -lt $minVersion) {
    Write-Output ""
    Write-Output "This script requires at least version $($minVersion.Major).$($minVersion.Minor) of PowerShell, but you seem to be running version $($psVersion.Major).$($psVersion.Minor)."

    try {
        $x = (pwsh --version)
        Write-Output "However we have detected the 'pwsh' command in your PATH, which provides $x."
        Write-Output "Maybe you invoked PowerShell with the old 'powershell' command? Please use 'pwsh' instead."
    }
    catch {
        Write-Output "We recommend you install the most recent stable version available for download at:"
        Write-Output "https://github.com/PowerShell/PowerShell/releases"
        Write-Output "Please note that this version will need to be invoked with 'pwsh' not 'powershell'."
    }

    Die "Unsupported PowerShell version: $($psVersion.Major).$($psVersion.Minor)"
}

function isAtLeastPs($version) {
  return ($psVersion -ge [System.Version]::Parse($version))
}

# determine platform
$platform = "windows"
if ($isLinux) { $platform = "linux" }
if ($isWindows) { $platform = "windows" }
if ($isMacOS) { $platform = "macOS" }
if (-not $isWindows -and $platform -eq "windows") { $isWindows = $true }

# report
Write-Output "PS $psVersion on $platform"
Write-Output ""

# validate commands and define actions ($doXxx)
foreach ($t in $commands) {

    if ($t.Trim().StartsWith("-")) {
        Die "Unknown option '$($t.Trim())' - use 'help' to list valid options."
    }

    switch ($t.Trim().ToLower()) {
        "help" {
            Write-Output "Hazelcast .NET Command Line"
            Write-Output "PowerShell $psVersion"
            Write-Output ""
            Write-Output "usage hz.[ps1|sh] [<options>] [<commands>]"
            Write-Output ""
            Write-Output "When no command is specified, the script displays this documentation."
            Write-Output ""
            Write-Output ""
            Write-Output "<commands> is a csv list of:"
            Write-Output ""
            Write-Output "  clean : cleans the solution"
            Write-Output ""
            Write-Output "  setver : sets the new version"
            Write-Output "        Updates the version in src/Directory.Build.props with the version specified via"
            Write-Output "        the -version option. If this option is missing, has no effect."
            Write-Output ""
            Write-Output "  tagver : tags the new version"
            Write-Output "        Creates the vX.Y.Z tag corresponding to the version in src/Directory.Build.props"
            Write-Output "        or the version specified via the -version option, if any."
            Write-Output ""
            Write-Output "  build : builds the solution"
            Write-Output ""
            Write-Output "  docs : builds the documentation (if supported by platform)"
            Write-Output "        Building the documentation is not supported on non-Windows platforms as DocFX is not"
            Write-Output "        supported on .NET Core yet."
            Write-Output ""
            Write-Output "  srvdocs : serves the documentation"
            Write-Output ""
            Write-Output "  pubdocs : publishes the documentation release (if supported by platform)"
            Write-Output "        The documentation still needs to be pushed to GitHub manually."
            Write-Output ""
            Write-Output "  tests : runs the tests"
            Write-Output ""
            Write-Output "  nupack : packs the NuGet package(s)"
            Write-Output ""
            Write-Output "  nupush : pushes the NuGet package(s)"
            Write-Output "        Pushing the NuGet packages requires a NuGet API key, which must be supplied via the"
            Write-Output "        NUGET_API_KEY environment variable."
            Write-Output ""
            Write-Output "  rc : runs the remote controller for tests"
            Write-Output ""
            Write-Output "  server : runs a server for tests"
            Write-Output ""
            Write-Output "  failedTests : details failed tests (alias: ft)"
            Write-Output "  codecs      : build the codecs files"
            Write-Output ""
            Write-Output "  codecs : build the codecs files"
            Write-Output ""
            Write-Output ""
            Write-Output "<options> are:"
            Write-Output ""
            Write-Output "  -enterprise : whether to run enterprise tests"
            Write-Output "        Running enterprise tests require an enterprise key, which can be supplied either"
            Write-Output "        via the HAZELCAST_ENTERPRISE_KEY environment variable, or the build/enterprise.key"
            Write-Output "        file."
            Write-Output ""
            Write-Output "  -sign : whether to sign assemblies (when building)"
            Write-Output "        Signing assemblies requires the private signing key in build/hazelcast.snk file."
            Write-Output ""
            Write-Output "  -cover : whether to do test coverage (when running tests)"
            Write-Output ""
            Write-Output "  -server <version> : the server version for tests (when running tests, or rc, or server)"
            Write-Output "        Server <version> must match a released Hazelcast IMDG server version, e.g. 4.0 or"
            Write-Output "        4.1-SNAPSHOT. Server JARs are automatically downloaded."
            Write-Output ""
            Write-Output "  -framework <version> : the framework to build (default is all, alias: -f)"
            Write-Output "        Framework <version> must match a valid .NET target framework moniker, e.g. net462"
            Write-Output "        or netcoreapp3.1. Check the project files (.csproj) for supported versions."
            Write-Output ""
            Write-Output "  -configuration <configuration> : the build configuration (when building, alias: -c)"
            Write-Output "        Configuration is 'Release' by default but can be forced to be 'Debug'."
            Write-Output ""
            Write-Output "  -testFilter <filter> : a test filter (when running tests, default is all, alias: -tf)"
            Write-Output "        Test <filter> can be used to filter the tests to run, it must respect the NUnit test"
            Write-Output "        selection language, which is detailed at:"
            Write-Output "        https://docs.nunit.org/articles/nunit/running-tests/Test-Selection-Language.html"
            Write-Output "        Example: -tf `"test == /Hazelcast.Tests.NearCache.NearCacheRecoversFromDistortionsTest/`""
            Write-Output ""
            Write-Output "  -coverageFilter <filter> : a coverage filter (when running tests, default is all, alias: -cf)"
            Write-Output "        Coverage <filter> can be used to filter the tests to cover, it must respect the"
            Write-Output "        dotCover language, which is detailed at:"
            Write-Output "        https://www.jetbrains.com/help/dotcover/Running_Coverage_Analysis_from_the_Command_LIne.html#filters"
            Write-Output ""
            Write-Output "  -test <name> : a simplified test filter (when running tests, alias: -t)"
            Write-Output "        The simplified test <name> filter which is equivalent to the full `"name =~ /<name>/`" filter."
            Write-Output ""
            Write-Output "  -version <version> : the version to build (when building, or setting/tagging the version)"
            Write-Output "        The <version> to build must be a valid SemVer version such as 3.2.1 or 6.7.8-preview.2,"
            Write-Output "        if no value is specified then the version is obtained from src/Directory.Build.props."
            Write-Output ""
            Write-Output "  -defineConstants <constants> : define additional build constants (when building, alias: d)"
            Write-Output ""
            Write-Output "  -classPath <classpath> : define an additional classpath (alias: cp)."
            Write-Output "        The classpath is appended to the RC or server classpath."
            Write-Output ""
            Write-Output "  -reproducible : mark the build as reproducible (alias: repro)"
            Write-Output ""
            Write-Output ""
            exit 0
		}

        "clean"       { $doClean = $true }
        "setver"      { $doSetVersion = $true }
        "tagver"      { $doTagVersion = $true }
        "build"       { $doBuild = $true }
        "docs"        { $doDocs = $true }
        "pubdocs"     { $doDocsRelease = $true }
        "srvdocs"     { $doDocsServe = $true }
        "tests"       { $doTests = $true }
        "nupack"       { $doNupack = $true }
        "nupush"      { $doNupush = $true }
        "rc"          { $doRc = $true }
        "server"      { $doServer = $true }
        "failedtests" { $doFailedTests = $true }
        "codecs"      { $doCodecs = $true }

        default { Die "Unknown command '$($t.Trim())' - use 'help' to list valid commands." }
    }
}

# validate the version to build
$hasVersion = $false
$versionPrefix = ""
$versionSuffix = ""
if (-not [System.String]::IsNullOrWhiteSpace($version)) {

    if (-not ($version -match '^(\d+\.\d+\.\d+)(?:\-([a-z0-9\.\-]*))?$')) {
        Die "Version `"$version`" is not a valid SemVer version"
    }

    $versionPrefix = $Matches.1
    $versionSuffix = $Matches.2

    $version = $versionPrefix.Trim()
    if (-not [System.String]::IsNullOrWhiteSpace($versionSuffix)) {
        $version += "-$($versionSuffix.Trim())"
    }
    $hasVersion = $true
}

# set versions and configure
$hzVersion = $server
$hzRCVersion = "0.7-SNAPSHOT" # use appropriate version
#$hzRCVersion = "0.5-SNAPSHOT" # for 3.12.x

# determine java code repositories for tests
$mvnOssSnapshotRepo = "https://oss.sonatype.org/content/repositories/snapshots"
$mvnEntSnapshotRepo = "https://repository.hazelcast.com/snapshot"
$mvnOssReleaseRepo = "https://repo1.maven.org/maven2"
$mvnEntReleaseRepo = "https://repository.hazelcast.com/release"

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

if ([string]::IsNullOrWhiteSpace($serverConfig)) {
    $serverConfig = "$buildDir/hazelcast-$hzVersion.xml"
}

# nuget packages
$nugetPackages = "$userHome/.nuget"
if ($localRestore) {
    $nugetPackages = "$slnRoot/.nuget"
    if (-not (Test-Path $nugetPackages)) { mkdir $nugetPackages }
}

# validate commands / platform
if ($doDocs -and -not $isWindows) {
    Write-Output "DocFX is not supported on '$platform', cannot build documentation."
    $doDocs = $false
}
if ($doDocsRelease -and -not $isWindows) {
    Write-Output "DocFX is not supported on '$platform', cannot release documentation."
    $doDocsRelease = $false
}

# get current version
$propsXml = [xml] (Get-Content "$srcDir/Directory.Build.props")
$currentVersionPrefix = $propsXml.project.propertygroup.versionprefix | Where-Object { -not [System.String]::IsNullOrWhiteSpace($_) }
$currentVersionSuffix = $propsXml.project.propertygroup.versionsuffix | Where-Object { -not [System.String]::IsNullOrWhiteSpace($_) }
$currentVersion = $currentVersionPrefix.Trim()
if (-not [System.String]::IsNullOrWhiteSpace($currentVersionSuffix)) {
    $currentVersion += "-$($currentVersionSuffix.Trim())"
}

# set version
if ($hasVersion)
{
    $isNewVersion = ($version -ne $currentVersion)
}
else
{
    $versionPrefix = $currentVersionPrefix
    $versionSuffix = $currentVersionSuffix
    $version = $currentVersion
    $isNewVersion = $false
}
$isPreRelease = -not [System.String]::IsNullOrWhiteSpace($versionSuffix)

# get doc destination according to version
if ($isPreRelease) {
    $docDstDir = "dev"
    $docMessage = "Update dev documentation ($version)"
}
else {
    $docDstDir = $versionPrefix
    $docMessage = "Version $version documentation"
}

# validate enterprise key
$enterpriseKey = $env:HAZELCAST_ENTERPRISE_KEY
if (($doTests -or $doRc) -and $enterprise -and [System.String]::IsNullOrWhiteSpace($enterpriseKey)) {

    if (test-path "$buildDir/enterprise.key") {
        $enterpriseKey = @(gc "$buildDir/enterprise.key")[0].Trim()
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
if (-not [System.String]::IsNullOrWhiteSpace($test)) {
    if (-not [System.String]::IsNullOrWhiteSpace($testFilter)) { $testFilter += " && " } else { $testFilter = "" }
    $testFilter += "name =~ /$test/"
}

# determine tests name
$testName = "<FRAMEWORK>.{C}.{m}{a}"

 # do not cover tests themselves, nor the testing plumbing
if (-not [System.String]::IsNullOrWhiteSpace($coverageFilter)) { $coverageFilter += ";" }
$coverageFilter += "-:Hazelcast.Net.Tests;-:Hazelcast.Net.Testing;-:ExpectedObjects"

# set server version (to filter tests)
$env:HAZELCAST_SERVER_VERSION=$server.TrimEnd("-SNAPSHOT")

# finds latest version of a NuGet package in the NuGet cache
function findLatestVersion($path) {
    if ([System.IO.Directory]::Exists($path)) {
        $v = Get-ChildItem $path | `
            foreach-object { [Version]::Parse($_.Name) } | `
                Sort-Object -descending | `
                Select-Object -first 1
    }
    else {
        $l = [System.IO.Path]::GetDirectoryname($path).Length
        $l = $path.Length - $l
        $v = Get-ChildItem "$path.*" | `
                foreach-object { [Version]::Parse($_.Name.SubString($l)) } | `
                    Sort-Object -descending | `
                    Select-Object -first 1
    }
    return $v
}

# ensures that a command exists in the path
function ensureCommand($command) {
    $r = get-command $command 2>&1
    if ($nul -eq $r.Name) {
        Die "Command '$command' is missing."
    }
    else {
        Write-Output "Detected $command at '$($r.Source)'"
    }
}

function invokeWebRequest($url, $dest) {
    $args = @{ Uri = $url }
    if (![System.String]::IsNullOrWhiteSpace($dest)) {
        $args.OutFile = $dest
        $args.PassThru = $true
    }
    
    # "Indicates that the cmdlet uses the response object for HTML content without Document
    # Object Model (DOM) parsing. This parameter is required when Internet Explorer is not
    # installed on the computers, such as on a Server Core installation of a Windows Server
    # operating system."
    #
    # "This parameter has been deprecated. Beginning with PowerShell 6.0.0, all Web requests
    # use basic parsing only. This parameter is included for backwards compatibility only 
    # and any use of it has no effect on the operation of the cmdlet."
    #
    if (-not (isAtLeastPs("6.0.0"))) {
        $args.UseBasicParsing = $true
    }

    $pp = $progressPreference
    $progressPreference = 'SilentlyContinue'

    if (isAtLeastPs("7.0.0")) {
        $args.SkipHttpErrorCheck = $true
    }
    
    try {
        $r = invoke-webRequest @args
        if ($null -ne $r) { 
            if ($r.StatusCode -ne 200) {
                Write-Output "--> $($r.StatusCode) $($r.StatusDescription)"
            }
            return $r 
        }
        return @{ StatusCode = 999; StatusDescription = "Error" }
    }
    catch [System.Net.WebException] {
        return @{ StatusCode = 999; StatusDescription = "Error" }
    }
    finally {
        $progressPreference = $pp
    }
}

#
function fixServerVersion($version) {
    
    $version = $script:hzVersion

    if (-not ($version.EndsWith("-SNAPSHOT"))) {
        return;
    }
        
    $url = "$mvnOssSnapshotRepo/com/hazelcast/hazelcast/$version/maven-metadata.xml"
    $response = invokeWebRequest $url
    if ($response.StatusCode -eq 200) {
        return;
    }
    
    Write-Output "Server $version is not available"
    
    $url2 = "$mvnOssSnapshotRepo/com/hazelcast/hazelcast/maven-metadata.xml"
    $response2 = invokeWebRequest $url2
    if ($response2.StatusCode -ne 200) {
        Die "Error: could not download metadata"
    }
    
    $metadata = [xml] $response2.Content
    $version = $version.SubString(0, $version.Length - "-SNAPSHOT".Length)
    $nodes = $metadata.SelectNodes("//version [starts-with(., '$version')]")
    
    if ($nodes.Count -lt 1) {
        Die "Error: could not find a proper server version"
    }
    
    $version2 = $nodes[0].innerText
    
    Write-Output "Found server $version2, updating"
    $script:hzVersion = $version2
    
    Write-Output ""
}

# get a Maven artifact
function getMvn($repoUrl, $group, $artifact, $jversion, $classifier, $dest) {

    if ($jversion.EndsWith("-SNAPSHOT")) {
        $url = "$repoUrl/$group/$artifact/$jversion/maven-metadata.xml"
        $response = invokeWebRequest $url
        if ($response.StatusCode -ne 200) {
            Die "Failed to download $url ($($response.StatusCode))"
        }

        $metadata = [xml] $response.Content
        $xpath = "//snapshotVersion [extension='jar'"
        if (![System.String]::IsNullOrWhiteSpace($classifier)) {
            $xpath += " and classifier='$classifier'"
        }
        else {
            $xpath += " and not(classifier)"
        }
        $xpath += "]"
        $jarVersion = "-" + $metadata.SelectNodes($xpath)[0].value
    }
    else {
        $jarVersion = "-" + $jversion
    }

    $url = "$repoUrl/$group/$artifact/$jversion/$artifact$jarVersion"
    if (![System.String]::IsNullOrWhiteSpace($classifier)) {
        $url += "-$classifier"
    }
    $url += ".jar"
    $response = invokeWebRequest $url $dest
    if ($response.StatusCode -ne 200) {
        Die "Failed to download $url ($($response.StatusCode))"
    }
}

# ensure docfx -> $script:docfx
function ensureDocFx() {
    $v = $docfxVersion
    Write-Output "  v$v"
    $dir = "$nugetPackages/docfx.console/$v"
    $docfx = "$dir/tools/docfx.exe"

    Write-Output ""
    Write-Output "Detected DocFX at '$docfx'"

    $script:docfx = $docfx
}

# ensure memberpage
function ensureMemberPage() {
    $v = $memberpageVersion
    Write-Output "  v$v"
    $dir = "$nugetPackages/memberpage/$v"

    Write-Output ""
    Write-Output "Detected DocFX MemberPage at $dir"
}

function ensureJar($jar, $repo, $artifact) {
    if(Test-Path "$tmpDir/lib/$jar") {
        Write-Output "Detected $jar"
    } else {
        Write-Output "Downloading $jar ..."

        $parts = $artifact.Split(':')
        $group = $parts[0].Replace('.', '/')
        $art = $parts[1]
        $ver = $parts[2]

        $cls = $null
        if ($parts.Length -eq 5 -and $parts[4] -eq "tests") {
            $cls = "tests"
        }

        getMvn $repo $group $art $ver $cls "$tmpDir/lib/$jar"
    }
    $s = ";"
    if (-not $isWindows) { $s = ":" }
    $classpath = $script:classpath
    if (-not [System.String]::IsNullOrWhiteSpace($classpath)) { $classpath += $s }
    $classpath += "$tmpDir/lib/$jar"
    $script:classpath = $classpath
}

# clear source file
function clrSrc($file) {
    $txt = get-content $file -raw
    $txt = $txt.Replace("`r`n", "`n"); # crlf to lf
    $txt = $txt.Replace("`r", "`n"); # including single cr
    $txt = [System.Text.RegularExpressions.Regex]::Replace($txt, "[ `t]+$", "", [System.Text.RegularExpressions.RegexOptions]::Multiline); # trailing spaces
    $txt = $txt.TrimEnd("`n") + "`n"; # end file with one single lf
    $txt = $txt.Replace("`n", [System.Environment]::NewLine); # lf to actual system newline (as expected by Git)
    set-content $file $txt -noNewLine
}

# ensure we have git, and validate git submodules
ensureCommand "git"
foreach ($x in (git submodule status))
{
    if ($x.StartsWith("-"))
    {
        Write-Output "ERROR: some Git submodules are missing, please ensure that submodules"
        Write-Output "are initialized and updated. You can initialize and update all submodules"
        Write-Output "at once with `"git submodule update --init`"."
        Write-Output ""
        exit
    }
}
Write-Output "Found required Git submodules"
Write-Output ""

# make sure we have a correct server version (and maybe fix it)
# this is so we can specify 4.0-SNAPSHOT and get 4.0.x-SNAPSHOT
fixServerVersion

Write-Output "Version"
$s = $version
if ($isPreRelease) { $s += ", pre-release" }
if ($isNewVersion) { $s += ", new version (was $currentVersion)" }
Write-Output "  Version        : $s"
$s = ""
if ($doSetVersion) { $s = "set version" }
if ($doTagVersion) { if ($s -ne "") { $s += ", " } $s += "tag version" }
if ($s -eq "") { $s = "none" }
Write-Output "  Action         : $s"
Write-Output ""

if ($doCodecs) {
    Write-Output "Codecs"
    Write-Output "  Source         : protocol/cs"
    Write-Output "  Filters        : protocol/cs/__init__.py"
    Write-Output ""
}

if ($doBuild) {
    Write-Output "Build"
    Write-Output "  Platform       : $platform"
    Write-Output "  Commands       : $commands"
    Write-Output "  Configuration  : $configuration"
    Write-Output "  Define         : $defineConstants"
    Write-Output "  Framework      : $([System.String]::Join(", ", $frameworks))"
    Write-Output "  Building to    : $outDir"
    Write-Output "  Sign code      : $sign"
    Write-Output "  Version        : $version"
    Write-Output ""
}

if ($doTests) {
    Write-Output "Tests"
    Write-Output "  Server version : $server"
    Write-Output "  Enterprise     : $enterprise"
    Write-Output "  Filter         : $testFilter"
    Write-Output "  Test Name      : $testName"
    Write-Output "  Results        : $tmpDir/tests/results"
    Write-Output ""
}

if ($doTests -and $cover) {
    Write-Output "Tests Coverage"
    Write-Output "  Filter         : $coverageFilter"
    Write-Output "  Reports & logs : $tmpDir/tests/cover"
    Write-Output ""
}

if ($doNupack) {
    Write-Output "Nuget Package"
    Write-Output "  Configuration  : $configuration"
    Write-Output "  Version        : $version"
    Write-Output "  To             : $tmpDir/output"
    Write-Output ""
}

if ($doRc) {
    Write-Output "Remote Controller"
    Write-Output "  Server version : $hzVersion"
    Write-Output "  RC Version     : $hzRCVersion"
    Write-Output "  Enterprise     : $enterprise"
    Write-Output "  Logging to     : $tmpDir/rc"
    Write-Output ""
}

if ($doServer) {
    Write-Output "Server"
    Write-Output "  Server version : $hzVersion"
    Write-Output "  Enterprise     : $enterprise"
    Write-Output "  Configuration  : $serverConfig"
    Write-Output "  Logging to     : $tmpDir/server"
    Write-Output ""
}

if ($doDocs) {
    $r = "release"
    if ($isPreRelease) { $r = "pre-$r" }
    Write-Output "Build Documentation"
    Write-Output "  Version        : $version"
    Write-Output "  Version Path   : $docDstDir ($r)"
    Write-Output "  Path           : $tmpdir/docfx.out"
    Write-Output ""
}

if ($doDocsServe) {
    Write-Output "Documentation Server"
    Write-Output "  Path           : $tmpdir/docfx.out"
    Write-Output ""
}

if ($doDocsRelease) {
    Write-Output "Release Documentation"
    Write-Output "  Source         : $tmpdir/docfx.out"
    Write-Output "  Pages repo     : $tmpdir/gh-pages"
    Write-Output "  Message        : $docMessage"
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
    if (test-path "$tmpDir\tests") {
        Write-Output "  $tmpDir\tests"
        remove-item "$tmpDir\tests" -force -recurse
    }

    # clears logs (server, rc...)
    if (test-path "$tmpDir") {
        gci $tmpDir -include *.log -Recurse | foreach ($_) {
            Write-Output "  $($_.fullname)"
            remove-item $_.fullname -Force
        }
    }

    # clears docs
    if (test-path "$tmpDir\docfx.out") {
        Write-Output "  $tmpDir\docfx.out"
        remove-item "$tmpDir\docfx.out" -force -recurse
    }
    if (test-path "$docDir\templates\hz\Plugins") {
        Write-Output "  $docDir\templates\hz\Plugins"
        remove-item "$docDir\templates\hz\Plugins" -force -recurse
    }

    Write-Output ""
}

if (-not (test-path $tmpDir)) { mkdir $tmpDir >$null }
if (-not (test-path $outDir)) { mkdir $outDir >$null }

function getSdk($sdks, $v) {

    # trust dotnet to return the sdks ordered by version, so last is the highest version
    # exclude versions containing "-" ie anything pre-release FIXME why?
    $sdk = $sdks `
        | Select-String -pattern "^$v" `
        | Foreach-Object { $_.ToString().Split(' ')[0] } `
        | Select-String -notMatch -pattern "-" `
        | Select-Object -last 1

    if ($null -eq $sdk) { return "n/a" } 
    else { return $sdk.ToString() }
}

function ensureDotnet() {

    ensureCommand "dotnet"

    $dotnetVersion = (&dotnet --version)
    Write-Output "  Version $dotnetVersion"

    $sdks = (&dotnet --list-sdks)
    
    $v21 = getSdk $sdks "2.1"
    if ($null -eq $v21) {
        Write-Output ""
        Write-Output "This script requires Microsoft .NET Core 2.1.x SDK, which can be downloaded at: https://dotnet.microsoft.com/download/dotnet-core"
        Die "Could not find dotnet SDK version 2.1.x"
    }
    $v31 = getSdk $sdks "3.1"
    if ($null -eq $v31) {
        Write-Output ""
        Write-Output "This script requires Microsoft .NET Core 3.1.x SDK, which can be downloaded at: https://dotnet.microsoft.com/download/dotnet-core"
        Die "Could not find dotnet SDK version 3.1.x"
    }
    $v50 = getSdk $sdks "5.0"
    if ($null -eq $v50) {
        Write-Output ""
        Write-Output "This script requires Microsoft .NET Core 5.0.x SDK, which can be downloaded at: https://dotnet.microsoft.com/download/dotnet-core"
        Die "Could not find dotnet SDK version 5.0.x"
    }
    if ($v50 -lt "5.0.200") { # 5.0.200+ required for proper reproducible builds
        Write-Output ""
        Write-Output "This script requires Microsoft .NET Core 5.0.200+ SDK, which can be downloaded at: https://dotnet.microsoft.com/download/dotnet-core"
        Die "Could not find dotnet SDK version 5.0.200+"
    }
    $v60 = getSdk $sdks "6.0" # 6.0 is not required
    
    Write-Output "  SDKs 2.1:$v21, 3.1:$v31, 5.0:$v50, 6.0:$v60"
}

# ensure we have dotnet (always)
ensureDotnet

# use NuGet to ensure we have the required packages for building and testing
if ($noRestore) {
    Write-Output ""
    Write-Output "Skip global NuGet packages restore (assume we have them already)"
}
else {
    Write-Output ""
    Write-Output "Restore global NuGet packages..."
    dotnet restore "$buildDir/build.proj" --packages $nugetPackages
}

# get the required packages version (as specified in build.proj)
$buildAssets = (get-content "$buildDir/obj/project.assets.json" -raw) | ConvertFrom-Json
$buildLibs = $buildAssets.Libraries
$buildLibs.PSObject.Properties.Name | Foreach-Object {
    $p = $_.Split('/')
    $name = $p[0].ToLower()
    $pversion = $p[1]
    if ($name -eq "nunit.consolerunner") { $nunitVersion = $pversion }
    if ($name -eq "jetbrains.dotcover.commandlinetools") { $dotcoverVersion = $pversion }
    if ($name -eq "docfx.console") { $docfxVersion = $pversion }
    if ($name -eq "memberpage") { $memberpageVersion = $pversion }
}

# ensure we have python for protocol
if ($doCodecs) {
    ensureCommand "python"
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

    # sad java
    $v = & java -version 2>&1
    $v = $v[0].ToString()
    $p0 = $v.IndexOf('"')
    $p1 = $v.LastIndexOf('"')
    $v = $v.SubString($p0+1,$p1-$p0-1)

    Write-Output ""
    Write-Output "Detected Java v$v"

    if (-not $v.StartsWith("1.8")) {
        # starting with Java 9 ... weird things can happen
        $javaFix = @( "-Dcom.google.inject.internal.cglib.\$experimental_asm7=true",  "--add-opens java.base/java.lang=ALL-UNNAMED" )

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

# manage the version
if ($doSetVersion)
{
    if ($isNewVersion) {
        Write-Output "Set version: commit version change"

        $text = [System.IO.File]::ReadAllText("$srcDir/Directory.Build.props")

        $text = [System.Text.RegularExpressions.Regex]::Replace($text,
            "\<AssemblyVersion\>.*\</AssemblyVersion\>",
            "<AssemblyVersion>$versionPrefix</AssemblyVersion>")

        $text = [System.Text.RegularExpressions.Regex]::Replace($text,
            "\<FileVersion\>.*\</FileVersion\>",
            "<FileVersion>$versionPrefix</FileVersion>")

        $text = [System.Text.RegularExpressions.Regex]::Replace($text,
            "\<VersionPrefix\>.*\</VersionPrefix\>",
            "<VersionPrefix>$versionPrefix</VersionPrefix>")

        $text = [System.Text.RegularExpressions.Regex]::Replace($text,
            "\<VersionSuffix\>.*\</VersionSuffix\>",
            "<VersionSuffix>$versionSuffix</VersionSuffix>")

        $utf8bom = New-Object System.Text.UTF8Encoding $true
        [System.IO.File]::WriteAllText("$srcDir/Directory.Build.props", $text, $utf8bom)

        git add "$srcDir/Directory.Build.props"
        git commit -m "Version $version"
    }
    else {
        Write-Output "Set version: no change"
    }
}

if ($doTagVersion)
{
    git rev-parse "refs/tags/v$version" >$null 2>&1
    if ($LASTEXITCODE -eq 0)
    {
        Write-Output "Version: tag v$version already exists."
    }
    else
    {
        Write-Output "Version: creating tag v$version"
        git tag "v$version"
    }
}

# generate codecs
if ($doCodecs) {

    # note: codec files contain the client-side methods, along with the
    # server-side methods enclosed in '#if SERVER_CODEC' blocks. files are
    # copied to the client project, which uses them raw, and linked into
    # the testing projects, which defines SERVER_CODEC.

    # wipe existing codecs from the C# and protocol repositories
    Write-Output "Clear codecs"
    rm -force $srcDir/Hazelcast.Net/Protocol/Codecs/*.cs
    rm -force $srcDir/Hazelcast.Net/Protocol/CustomCodecs/*.cs

    Write-Output "Generate codecs"
    python $slnRoot/protocol/generator.py -l cs --no-binary -r $slnRoot

    Write-Output ""
}

function Get-TopologicalSort {
  param(
      [Parameter(Mandatory = $true, Position = 0)]
      [hashtable] $edgeList
  )

  # Make sure we can use HashSet
  Add-Type -AssemblyName System.Core

  # Clone it so as to not alter original
  #$currentEdgeList = [hashtable] (Get-ClonedObject $edgeList)
  $currentEdgeList = $edgeList

  # algorithm from http://en.wikipedia.org/wiki/Topological_sorting#Algorithms
  $topologicallySortedElements = New-Object System.Collections.ArrayList
  $setOfAllNodesWithNoIncomingEdges = New-Object System.Collections.Queue

  $fasterEdgeList = @{}

  # Keep track of all nodes in case they put it in as an edge destination but not source
  $allNodes = New-Object -TypeName System.Collections.Generic.HashSet[object] -ArgumentList (,[object[]] $currentEdgeList.Keys)

  foreach($currentNode in $currentEdgeList.Keys) {
      $currentDestinationNodes = [array] $currentEdgeList[$currentNode]
      if($currentDestinationNodes.Length -eq 0) {
          $setOfAllNodesWithNoIncomingEdges.Enqueue($currentNode)
      }

      foreach($currentDestinationNode in $currentDestinationNodes) {
          if(!$allNodes.Contains($currentDestinationNode)) {
              [void] $allNodes.Add($currentDestinationNode)
          }
      }

      # Take this time to convert them to a HashSet for faster operation
      $currentDestinationNodes = New-Object -TypeName System.Collections.Generic.HashSet[object] -ArgumentList (,[object[]] $currentDestinationNodes )
      [void] $fasterEdgeList.Add($currentNode, $currentDestinationNodes)        
  }

  # Now let's reconcile by adding empty dependencies for source nodes they didn't tell us about
  foreach($currentNode in $allNodes) {
      if(!$currentEdgeList.ContainsKey($currentNode)) {
          [void] $currentEdgeList.Add($currentNode, (New-Object -TypeName System.Collections.Generic.HashSet[object]))
          $setOfAllNodesWithNoIncomingEdges.Enqueue($currentNode)
      }
  }

  $currentEdgeList = $fasterEdgeList

  while($setOfAllNodesWithNoIncomingEdges.Count -gt 0) {        
      $currentNode = $setOfAllNodesWithNoIncomingEdges.Dequeue()
      [void] $currentEdgeList.Remove($currentNode)
      [void] $topologicallySortedElements.Add($currentNode)

      foreach($currentEdgeSourceNode in $currentEdgeList.Keys) {
          $currentNodeDestinations = $currentEdgeList[$currentEdgeSourceNode]
          if($currentNodeDestinations.Contains($currentNode)) {
              [void] $currentNodeDestinations.Remove($currentNode)

              if($currentNodeDestinations.Count -eq 0) {
                  [void] $setOfAllNodesWithNoIncomingEdges.Enqueue($currentEdgeSourceNode)
              }                
          }
      }
  }

  if($currentEdgeList.Count -gt 0) {
      throw "Graph has at least one cycle!"
  }

  return $topologicallySortedElements
}

# build the solution
# on Windows, build with MsBuild - else use dotnet
if ($doBuild) {

    # process define constants - it's a mess
    # see https://github.com/dotnet/sdk/issues/9562
    if (![string]::IsNullOrWhiteSpace($defineConstants)) {
        $defineConstants = $defineConstants.Replace(";", "%3B") # escape ';'
    }
    
    Write-Output ""
    Write-Output "Resolve projects dependencies..."
    $projs = Get-ChildItem -path $srcDir -recurse -depth 1 -include *.csproj
    $t = @{}
    $sc = [System.IO.Path]::DirectorySeparatorChar
    $projs | Foreach-Object {
        $proj = $_
        
        # exclude
        if (!$isWindows -and $proj.BaseName -eq "Hazelcast.Net.DocAsCode") { return } # continue
        
        $x = [xml] (Get-Content $proj); 
        $n = $x.SelectNodes("//ProjectReference/@Include");
        $k = $proj.FullName.SubString($srcDir.Length + 1).Replace("\", $sc).Replace("/", $sc)
        if ($t[$k] -eq $null) { $t[$k] = @() }

        $n | Foreach-Object {
            $dep = $_.Value
            $d = $dep.SubString("../".Length).Replace("\", $sc).Replace("/", $sc)
            Write-Output "  $k -> $d"
            $t[$k] += $d
        }
    }
    
    Write-Output ""
    Write-Output "Reorder projects..."
    $projs = Get-TopologicalSort $t
    $projs | Foreach-Object {
       Write-Output "  $_ "
    }

    Write-Output ""
    Write-Output "Build projets..."
    $buildArgs = @(
        "-c", "$configuration",
        "--packages", $nugetPackages
        # "-f", "$framework"
    )

    if ($reproducible) {
        $buildArgs += "-p:ContinuousIntegrationBuild=true"
    }

    if ($sign) {
        $buildArgs += "-p:ASSEMBLY_SIGNING=true"
        $buildArgs += "-p:AssemblyOriginatorKeyFile=`"$buildDir\hazelcast.snk`""
    }

    if (![string]::IsNullOrWhiteSpace($defineConstants)) {
        $buildArgs += "-p:DefineUserConstants=`"$defineConstants`""
    }

    if ($hasVersion) {
        $buildArgs += "-p:AssemblyVersion=$versionPrefix"
        $buildArgs += "-p:FileVersion=$versionPrefix"
        $buildArgs += "-p:VersionPrefix=$versionPrefix"
        $buildArgs += "-p:VersionSuffix=$versionSuffix"
    }
    
    $projs | foreach {
        Write-Output ""
        Write-Output "> dotnet build "$srcDir\$_" $buildArgs"
        dotnet build "$srcDir\$_" $buildArgs
        
        # if it failed, we can stop here
        if ($LASTEXITCODE) {
            Die "Build failed, aborting."
        }
    }
    
    Write-Output ""
}

# build documentation
if ($doDocs) {
    Write-Output ""
    Write-Output "Build documentation..."

    # clear target
    if (test-path "$tmpDir/docfx.out") {
        remove-item -recurse -force "$tmpDir/docfx.out"
    }

    # clear temp
    if (test-path "$docDir/obj") {
        remove-item -recurse -force "$docDir/obj"
    }

    # prepare templates
    $template = "default,$nugetPackages/memberpage/$memberpageVersion/content,$docDir/templates/hz"

    # clear plugins
    if (test-path "$docDir/templates/hz/Plugins") {
        remove-item -recurse -force "$docDir/templates/hz/Plugins"
    }
    mkdir "$docDir/templates/hz/Plugins" >$null 2>&1

    # copy our plugin dll
    $target = "net48"
    $pluginDll = "$srcDir/Hazelcast.Net.DocAsCode/bin/$configuration/$target/Hazelcast.Net.DocAsCode.dll"
    if (-not (test-path $pluginDll)) {
        Die "Could not find Hazelcast.Net.DocAsCode.dll, make sure to build the solution first.`nIn: $srcDir/Hazelcast.Net.DocAsCode/bin/$configuration/$target"
    }
    cp $pluginDll "$docDir/templates/hz/Plugins/"

    # copy our plugin dll dependencies
    # not *everything* needs to be copied, only ... some
    #cp "$srcDir/Hazelcast.Net.DocAsCode/bin/$configuration/$target/System.*.dll" "$docDir/templates/hz/Plugins/"

    # prepare docfx.json
    get-content "$docDir/_docfx.json" |
        foreach-object { $_ -replace "__DEST__", $docDstDir } |
        set-content "$docDir/docfx.json"

    # build
    Write-Output "Docs: Generate metadata..."
    &$docfx metadata "$docDir/docfx.json" # --disableDefaultFilter
    if ($LASTEXITCODE) { Die "Error." }
    Write-Output "Docs: Build..."
    &$docfx build "$docDir/docfx.json" --template $template
    if ($LASTEXITCODE) { Die "Error." }

    # post-process
    Write-Output "Docs: Post-process..."
    if ($docDstDir -eq "dev") {
        $devwarnMessage = "<div id=`"devwarn`">This page documents a development version of the Hazelcast .NET client. " +
                          "Its content is not final and remains subject to changes.</div>"
        $devwarnClass = "devwarn"
        $devdoc_doc = "<li>development / in-progress version <a href=`"dev/doc/index.html`">$version</a></li>"
        $devdoc_api = "<li>development / in-progress version <a href=`"dev/api/index.html`">$version</a></li>"
    }
    else {
        $devwarnMessage = ""
        $devwarnClass = ""
        $devdoc_doc = ""
        $devdoc_api = ""
    }

    get-childitem -recurse -path "$tmpDir/docfx.out/$docDstDir" -filter *.html |
        foreach-object {
            $text = get-content -path $_
            $text = $text `
                -replace "<!-- DEVWARN -->", $devwarnMessage `
                -replace "DEVWARN", $devwarnClass
            set-content -path $_ -value $text
        }

    get-childitem -path "$tmpDir/docfx.out" -filter "*-index.html" |
        foreach-object {
            $text = get-content -path $_
            $text = $text `
              -replace "<li><!--DEVDOC_DOC--></li>", $devdoc_doc `
              -replace "<li><!--DEVDOC_API--></li>", $devdoc_api
            set-content -path $_ -value $text
        }
}

# release documentation
if ($doDocsRelease) {
    Write-Output ""
    Write-Output "Release documentation"

    $pages = "$tmpDir/gh-pages"
    $docs = "$tmpDir/docfx.out"

    if (-not (test-path "$docs/$docDstDir")) {
        Die "Could not find $docs/$docDstDir. Maybe you should build the docs first?"
    }

    if (test-path "$pages") {
        remove-item -recurse -force "$pages"
    }

    &git clone . "$pages"
    &git -C "$pages" remote set-url origin https://github.com/hazelcast/hazelcast-csharp-client.git
    &git -C "$pages" fetch origin gh-pages
    &git -C "$pages" checkout gh-pages

    if (test-path "$pages/$docDstDir") {
        remove-item -recurse -force "$pages/$docDstDir"
    }
    copy-item "$docs/$docDstDir" "$pages" -recurse

    cp "$docs/styles/*" "$pages/styles/"
    cp "$docs/images/*" "$pages/images/"

    cp "$docs/*.html" "$pages"
    cp "$docs/*.json" "$pages"
    cp "$docs/*.yml" "$pages"

    &git -C "$pages" add -A
    &git -C "$pages" commit -m "$docMessage"

    Write-Output "Doc release is ready, but NOT pushed."
    Write-Output "Review $pages commit and push."
}

function getJavaKerberosArgs() {
    return @(
        "-Djava.util.logging.config.file=krb5/logging.properties",
        "-Djava.security.krb5.realm=HZ.LOCAL",
        "-Djava.security.krb5.kdc=SERVER19.HZ.LOCAL",
        "-Djava.security.krb5.conf=krb5/krb5.conf"
    )
}

function StartRemoteController() {

    if (-not (test-path "$tmpDir/rc")) { mkdir "$tmpDir/rc" >$null }

    Write-Output ""
    Write-Output "Starting Remote Controller..."
    Write-Output "ClassPath: $script:classpath"

    # start the remote controller
    $args = @(
        "-Dhazelcast.enterprise.license.key=$enterpriseKey",
        "-cp", "$script:classpath",
        "com.hazelcast.remotecontroller.Main"
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
    if (!(test-path "$serverConfig")) {
        Die "Missing server configuration file $serverConfig"
    }

    Write-Output ""
    Write-Output "Starting Server..."
    Write-Output "ClassPath: $script:classpath"

    # depending on server version, different starter class
    $mainClass = "com.hazelcast.core.server.HazelcastMemberStarter" # 4.0
    if ($server.StartsWith("3.")) {
        $mainClass = "com.hazelcast.core.server.StartServer" # 3.x
    }

    # start the server
    $args = @(
        "-Dhazelcast.enterprise.license.key=$enterpriseKey",
        "-cp", "$script:classpath",
        "-Dhazelcast.config=$serverConfig",
        "-server", "-Xms2g", "-Xmx2g", "-Dhazelcast.multicast.group=224.206.1.1", "-Djava.net.preferIPv4Stack=true",
        "$mainClass"
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
        Write-Output "Stopping remote controller (pid=$($script:remoteController.Id))..."
        $script:remoteController.Kill($true) # entire tree
	}
    else {
        Write-Output "Remote controller is not running."
	}
}

function StopServer() {

    # stop the server
    Write-Output ""
    if ($script:serverProcess -and $script:serverProcess.Id -and -not $script:serverProcess.HasExited) {
        Write-Output "Stopping server (pid=$($script:serverProcess.Id))..."
        $script:serverProcess.Kill($true) # entire tree
	}
    else {
        Write-Output "Server is not running."
	}
}

function CollectTestResults($fwk, $file) {
    $script:testResults = $script:testResults + $file
}

function RunTests($f) {

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

    #
    $dotnetArgs = @(
        "$srcDir/Hazelcast.Net.Tests/Hazelcast.Net.Tests.csproj",
        "-c", "$configuration",
        "--no-restore", "--no-build",
        "-f", "$f",
        "-v", "normal",
        "--logger", "trx;LogFileName=results-$f.trx",
        "--results-directory", "$tmpDir/tests/results"
    )

    # see https://docs.nunit.org/articles/vs-test-adapter/Tips-And-Tricks.html
    # for available options and names here
    $nunitArgs = @(
        "NUnit.WorkDirectory=`"$tmpDir/tests/results`"",
        "NUnit.TestOutputXml=`".`"",
        "NUnit.Labels=Before",
        "NUnit.DefaultTestNamePattern=`"$($testName.Replace("<FRAMEWORK>", $f))`""
    )

    if ($testFilter -ne "") { $nunitArgs += "NUnit.Where=`"$($testFilter.Replace("<FRAMEWORK>", $f))`"" }

    if ($cover) {
        $coveragePath = "$tmpDir/tests/cover/cover-$f"
        if (!(test-path $coveragePath)) {
            mkdir $coveragePath > $null
        }

        $dotCoverArgs = @(
            "--dotCoverFilters=$coverageFilter",
            "--dotCoverAttributeFilters=System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute",
            "--dotCoverOutput=$coveragePath/index.html",
            "--dotCoverReportType=HTML",
            "--dotCoverLogFile=$tmpDir/tests/cover/cover-$f.log",
            "--dotCoverSourcesSearchPaths=$srcDir"
        )

        $testArgs = @( "test" )
        $testArgs += $dotnetArgs
        $testArgs += $dotCoverArgs
        $testArgs += @( "--" )
        $testArgs += $nunitArgs

        Write-Output "> dotnet dotcover $testArgs"
        pushd "$srcDir/Hazelcast.Net.Tests"
        &dotnet dotcover $testArgs
        popd
    }
    else {
        $testArgs = @()
        $testArgs += $dotnetArgs
        $testArgs += @( "--" )
        $testArgs += $nunitArgs

        Write-Output "> dotnet test $testArgs"
        &dotnet test $testArgs
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

if ($doTests -or $doRc -or $doServer) {
   # prepare server/rc
    Write-Output ""
    Write-Output "Prepare server/rc..."
    if (-not (test-path "$tmpDir/lib")) { mkdir "$tmpDir/lib" >$null }

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

    rm "$tmpDir\tests\results\results-*" >$null 2>&1

    try {
        StartRemoteController

        Write-Output ""
        Write-Output "Run tests..."
        foreach ($framework in $frameworks) {
            Write-Output ""
            Write-Output "Run tests for $framework..."
            RunTests $framework
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
    
    if (!$testsSuccess) {
        Die "Some tests have failed"
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
    if (-not (test-path "$tmpDir\docfx.out")) {
        Die "Missing documentation directory."
    }

    Write-Output ""
    Write-Output "Documentation server is running..."
    Write-Output "Press ENTER to stop"
    &$docfx serve "$tmpDir\docfx.out"
}

if ($doNupack -and -not $testsSuccess) {
    Write-Output ""
    Write-Output "Tests failed, skipping building NuGet packages..."
    $doNupack = $false
}

function packNuget($name)
{
    $packArgs = @(
        "$srcDir\$name\$name.csproj", `
        "--no-build", "--nologo", `
        "-o", "$tmpDir\output", `
        "-c", "$configuration"        
    )

    if ($reproducible) {
        $packArgs += "/p:ContinuousIntegrationBuild=true"
    }

    if ($hasVersion) {
        $packArgs += "/p:AssemblyVersion=$versionPrefix"
        $packArgs += "/p:FileVersion=$versionPrefix"
        $packArgs += "/p:VersionPrefix=$versionPrefix"
        $packArgs += "/p:VersionSuffix=$versionSuffix"
    }

    &dotnet pack $packArgs
}

if ($doNupack) {
    Write-Output ""
    Write-Output "Pack NuGet packages..."

    # creates the nupkg (which contains dll)
    # creates the snupkg (which contains pdb with source code reference)
    # https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-pack

    packNuGet("Hazelcast.Net")
    packNuGet("Hazelcast.Net.Win32")

    Get-ChildItem "$tmpDir\output" | Foreach-Object { Write-Output "  $_" }
}

if ($doNupush -and -not $testsSuccess) {
    Write-Output ""
    Write-Output "Tests failed, skipping pushing NuGet packages..."
    $doNupush = $false
}

if ($doNupush) {
    Write-Output ""
    Write-Output "Push NuGet packages..."

    &dotnet nuget push "$tmpDir\output\Hazelcast.Net.$version.nupkg" --api_key $nugetApiKey --source "https://api.nuget.org/v3/index.json"
    &dotnet nuget push "$tmpDir\output\Hazelcast.Net.Win32.$version.nupkg" --api-key $nugetApiKey --source "https://api.nuget.org/v3/index.json"
}

Write-Output ""
Write-Output "Done."

# eof
