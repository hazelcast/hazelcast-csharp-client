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
    $noRestore, # don't restore NuGet packages (assume they are there already)

    [alias("d")]
    [string]
    $defineConstants, # define additional build constants

    [alias("cp")]
    [string]
    $classpath, # additional classpath for rc/server

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
    Exit
}

# process commands
# in case it was passed by a script and not processed as an array
# PowerShell can be weird at times ;(
if ($commands.Length -eq 1 -and $commands[0].Contains(',')) {
    $commands = $commands[0].Replace(" ", "").Split(',')
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

# determine platform
$platform = "windows"
if ($isLinux) { $platform = "linux" }
if ($isWindows) { $platform = "windows" }
if ($isMacOS) { $platform = "macOS" }
if (-not $isWindows -and $platform -eq "windows") { $isWindows = $true }

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
            Write-Output "  nuget : builds the NuGet package(s)"
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
        "nuget"       { $doNuget = $true }
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
$hzLocalBuild = $false # $true to skip downloading dependencies
$hzToolsCache = 12 #days
$hzVsMajor = 16 # force VS major version, default to 16 (VS2019) for now
$hzVsPreview = $false # whether to look for previews of VS

# determine java code repositories for tests
$mvnOssSnapshotRepo = "https://oss.sonatype.org/content/repositories/snapshots"
$mvnEntSnapshotRepo = "https://repository.hazelcast.com/snapshot/"
$mvnOssReleaseRepo = "https://repo1.maven.org/maven2"
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
    Write-Output "DocFX is not supported on '$platform', cannot build documentation."
    $doDocs = $false
}
if ($doDocsRelease -and -not $isWindows) {
    Write-Output "DocFX is not supported on '$platform', cannot release documentation."
    $doDocsRelease = $false
}

# get current version
$propsXml = [xml] (gc "$srcDir/Directory.Build.props")
$currentVersionPrefix = $propsXml.project.propertygroup.versionprefix | where { -not [System.String]::IsNullOrWhiteSpace($_) }
$currentVersionSuffix = $propsXml.project.propertygroup.versionsuffix | where { -not [System.String]::IsNullOrWhiteSpace($_) }
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

function invokeWebRequest($url, $dest) {
    $args = @{ Uri = $url }
    if (![System.String]::IsNullOrWhiteSpace($dest)) {
        $args.OutFile = $dest
        $args.PassThru = $true
    }

    $pp = $progressPreference
    $progressPreference = 'SilentlyContinue'

    # PowerShell 7+ has -skipHttpErrorCheck parameter but not everyone will have it
    # so, try... catch is required
    try {
        return invoke-webRequest @args
    }
    catch [System.Net.WebException] {
        $response = $_.Exception.Response
        Die "Failed to GET $url : $($response.StatusCode) $($response.StatusDescription)"
    }
    finally {
        $progressPreference = $pp
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
            $response = invokeWebRequest $source $nuget
            if ($response.StatusCode -ne 200) { Die "Failed to download NuGet." }
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

# get a Maven artifact
function getMvn($repoUrl, $group, $artifact, $jversion, $classifier, $dest) {

    if ($jversion.EndsWith("-SNAPSHOT")) {
        $url = "$repoUrl/$group/$artifact/$jversion/maven-metadata.xml"
        $response = invokeWebRequest $url
        if ($response.StatusCode -ne 200) {
            Die "GET $url : $($response.StatusCode) $($response.StatusDescription)"
        }

        $metadata = [xml] $response.Content
        $xpath = "//snapshotVersion [extension='jar'"
        if (![System.String]::IsNullOrWhiteSpace($classifier)) {
            $xpath += " and classifier='$classifier'"
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
        Die "GET $url : $($response.StatusCode) $($response.StatusDescription)"
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

# say hello
Write-Output "Hazelcast .NET Command Line"
Write-Output ""

# ensure we have git
ensureCommand "git"
Write-Output ""

# validate git submodules
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

if ($doNuget) {
    Write-Output "Nuget Package"
    Write-Output "  Configuration  : $configuration"
    Write-Output "  Version        : $version"
    Write-Output "  To             : $tmpDir/output"
    Write-Output ""
}

if ($doRc) {
    Write-Output "Remote Controller"
    Write-Output "  Server version : $server"
    Write-Output "  RC Version     : $hzRCVersion"
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
        Write-Host ""
        Write-Host "This script requires Microsoft .NET Core 2.1.x SDK, which can be downloaded at: https://dotnet.microsoft.com/download/dotnet-core"
        Die "Could not find dotnet SDK version 2.1.x"
  }
  $v31 = ($sdks | select-string -pattern "^3\.1" | foreach-object { $_.ToString().Split(' ')[0] } | select -last 1)
  if ($v31 -eq $null) {
        Write-Host ""
        Write-Host "This script requires Microsoft .NET Core 3.1.x SDK, which can be downloaded at: https://dotnet.microsoft.com/download/dotnet-core"
        Die "Could not find dotnet SDK version 3.1.x"
  }
}

# use NuGet to ensure we have the required packages for building and testing
if ($noRestore) {
    Write-Output ""
    Write-Output "Skip NuGet packages restore (assume we have them already)"
}
else {
    Write-Output ""
    Write-Output "Restore NuGet packages..."
    if ($isWindows) {
        &$nuget restore "$buildDir/build.proj" -Verbosity Quiet
    }
    else {
        dotnet restore "$buildDir/build.proj"
    }
}

# get the required packages version (as specified in build.proj)
$buildAssets = (get-content "$buildDir/obj/project.assets.json" -raw) | ConvertFrom-Json
$buildLibs = $buildAssets.Libraries
$buildLibs.PSObject.Properties.Name | Foreach-Object {
    $p = $_.Split('/')
    $name = $p[0].ToLower()
    $pversion = $p[1]
    if ($name -eq "vswhere") { $vswhereVersion = $pversion }
    if ($name -eq "nunit.consolerunner") { $nunitVersion = $pversion }
    if ($name -eq "jetbrains.dotcover.commandlinetools") { $dotcoverVersion = $pversion }
    if ($name -eq "docfx.console") { $docfxVersion = $pversion }
    if ($name -eq "memberpage") { $memberpageVersion = $pversion }
}

if ($doBuild -and $isWindows) {
    $vswhere = ""
    ensureVsWhere
    $msBuild = ""
    ensureMsBuild
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
if ($isNewVersion -and $doSetVersion)
{
    Write-Output "Version: new version, update props and commit"

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
    rm -force -recurse $slnRoot/protocol/output/cs

    Write-Output "Generate codecs"
    python $slnRoot/protocol/generator.py -l cs --no-binary

    # copy generated codecs to the C# repository
    Write-Output "Copy codecs"
    cp $slnRoot/protocol/output/cs/src/Hazelcast.Net/Protocol/Codecs/*.cs $srcDir/Hazelcast.Net/Protocol/Codecs/
    cp $slnRoot/protocol/output/cs/src/Hazelcast.Net/Protocol/CustomCodecs/*.cs $srcDir/Hazelcast.Net/Protocol/CustomCodecs/

    # normalize codecs FIXME platform?
    Write-Output "Normalize codecs"
    foreach ($file in $(ls $srcDir/Hazelcast.Net/Protocol/Codecs/*.cs)) {
        clrSrc $file
    }
    foreach ($file in $(ls $srcDir/Hazelcast.Net/Protocol/CustomCodecs/*.cs)) {
        clrSrc $file
    }

    Write-Output ""
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
    Write-Output "Build solution..."
    if ($isWindows) {
        $buildArgs = @(
            "$slnRoot/Hazelcast.Net.sln", `
            "/p:Configuration=$configuration", `
            "/target:`"Restore;Build`""
            #/p:TargetFramework=$framework
        )
    }
    else {
        $buildArgs = @(
            "$slnRoot/Hazelcast.Net.sln", `
            "-c", "$configuration"
            # "-f", "$framework"
        )
    }

    if (![string]::IsNullOrWhiteSpace($defineConstants)) {
        $buildArgs += "/p:DefineUserConstants=`"$defineConstants`""
    }

    if ($signAssembly) {
        $buildArgs += "/p:SignAssembly=true"
        $buildArgs += "/p:PublicSign=false"
        $buildArgs += "/p:AssemblyOriginatorKeyFile=`"$buildDir\hazelcast.snk`""
    }

    if ($hasVersion) {
        $buildArgs += "/p:AssemblyVersion=$versionPrefix"
        $buildArgs += "/p:FileVersion=$versionPrefix"
        $buildArgs += "/p:VersionPrefix=$versionPrefix"
        $buildArgs += "/p:VersionSuffix=$versionSuffix"
    }

    if ($isWindows) {
        &$msBuild $buildArgs
    }
    else {
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
    if (test-path "$tmpDir/docfx.out") {
        remove-item -recurse -force "$tmpDir/docfx.out"
    }

    # clear temp
    if (test-path "$docDir/obj") {
        remove-item -recurse -force "$docDir/obj"
    }

    # prepare templates
    $template = "default,$userHome/.nuget/packages/memberpage/$memberpageVersion/content,$docDir/templates/hz"

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
    }
    else {
        $devwarnMessage = ""
        $devwarnClass = ""
    }

    get-childitem -recurse -path "$tmpDir/docfx.out/$docDstDir" -filter *.html |
        foreach-object {
            $text = get-content -path $_
            $text = $text `
                -replace "<!-- DEVWARN -->", $devwarnMessage `
                -replace "DEVWARN", $devwarnClass
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

    Write-Host "Doc release is ready, but NOT pushed."
    Write-Host "Review $pages commit and push."
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
    Write-Output "ClassPath: $classpath"

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
    if (!(test-path "$buildDir/hazelcast-$hzVersion.xml")) {
        Die "Missing server configuration file $buildDir/hazelcast-$hzVersion.xml"
    }

    Write-Output ""
    Write-Output "Starting Server..."
    Write-Output "ClassPath: $classpath"

    # depending on server version, different starter class
    $mainClass = "com.hazelcast.core.server.HazelcastMemberStarter" # 4.0
    if ($server.StartsWith("3.")) {
        $mainClass = "com.hazelcast.core.server.StartServer" # 3.x
    }

    # start the server
    $args = @(
        "-Dhazelcast.enterprise.license.key=$enterpriseKey",
        "-cp", "$script:classpath",
        "-Dhazelcast.config=$buildDir/hazelcast-$hzVersion.xml",
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
            "test",
            "$srcDir/Hazelcast.Net.Tests/Hazelcast.Net.Tests.csproj",
            "-c", "$configuration",
            "--no-restore",
            "-f", "$f",
            "-v", "normal",

            "--dotCoverFilters=$coverageFilter",
            "--dotCoverAttributeFilters=System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute",
            "--dotCoverOutput=$coveragePath/index.html",
            "--dotCoverReportType=HTML",
            "--dotCoverLogFile=$tmpDir/tests/cover/cover-$f.log",
            "--dotCoverSourcesSearchPaths=$srcDir",
            "--"
        ) + $nunitArgs

        Write-Output "exec: dotnet dotcover $dotCoverArgs"
        pushd "$srcDir/Hazelcast.Net.Tests"
        &dotnet dotcover $dotCoverArgs
        popd
    }
    else {
        $dotnetArgs = @(
            "$srcDir/Hazelcast.Net.Tests/Hazelcast.Net.Tests.csproj",
            "-c", "$configuration",
            "--no-restore",
            "-f", "$f",
            "-v", "normal",
            "--"
        ) + $nunitArgs

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
    $nunitArgs = @(
        "`"${testDLL}`"",
        "--labels=Before",
        "--result=`"$tmpDir/tests/results/results-$f.xml`"",
        "--framework=$nuf",
        "--test-name-format=`"$($testName.Replace("<FRAMEWORK>", $f))`""
    )

    if ($testFilter -ne "") { $nunitArgs += @("--where=`"$($testFilter.Replace("<FRAMEWORK>", $f))`"") }

    if ($cover) {

        $coveragePath = "$tmpDir/tests/cover/cover-$f"
        if (!(test-path $coveragePath)) {
            mkdir $coveragePath > $null
        }

        $v = $dotcoverVersion
        $dotCover = "$userHome/.nuget/packages/jetbrains.dotcover.commandlinetools/$v/tools/dotCover.exe"

        # note: separate attributes filters with ';'
        $dotCoverArgs = @(
            "cover",
            "--Filters=$coverageFilter",
            "--AttributeFilters=System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute",
            "--TargetWorkingDir=.",
            "--Output=$coveragePath/index.html",
            "--ReportType=HTML",
            "--TargetExecutable=${nunit}",
            "--LogFile=$tmpDir/tests/cover/cover-$f.log",
            "--SourcesSearchPaths=$srcDir",
            "--"
        ) + $nunitArgs

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
    if (-not (test-path "$tmpDir\docfx.out")) {
        Die "Missing documentation directory."
    }

    Write-Output ""
    Write-Output "Documentation server is running..."
    Write-Output "Press ENTER to stop"
    &$docfx serve "$tmpDir\docfx.out"
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

    &$nuget push "$tmpDir\output\Hazelcast.Net.$version.nupkg" -ApiKey $nugetApiKey -Source "https://api.nuget.org/v3/index.json"
}

Write-Output ""
Write-Output "Done."

# eof
