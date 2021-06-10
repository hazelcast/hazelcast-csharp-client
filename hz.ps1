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

# PowerShell errors can *also* be a pain
# see https://stackoverflow.com/questions/10666035
# see https://stackoverflow.com/questions/10666101
# don't die as soon as a command reports an error, we will take care of it!
# (and, GitHub actions tend to end up running with 'Stop' by default)
$ErrorActionPreference='Continue'

# include utils
. ./build/utils.ps1

# ensure we have the right platform
Validate-Platform

# say hello
Write-Output "Hazelcast .NET Command Line"
Write-Output "PowerShell $powershellVersion on $platform"

# PowerShell args can *also* be a pain - because 'pwsh' loves to pre-handle
# args when run directly but not when run from a scrip, etc - so we have our
# own way of dealing with args

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

$params = @(
    @{ name = "enterprise";      type = [switch];  default = $false;
       desc = "whether to run enterprise tests";
       info = "Running enterprise tests require an enterprise key, which can be supplied either via the HAZELCAST_ENTERPRISE_KEY environment variable, or the build/enterprise.key file."
    },
    @{ name = "server";          type = [string];  default = "4.0-SNAPSHOT"  # -SNAPSHOT to avoid obsolete certs in JARs
       parm = "<version>";
       desc = "the server version when running tests, the remote controller, or a server";
       note = "The server <version> must match a released Hazelcast IMDG server version, e.g. 4.0 or 4.1-SNAPSHOT. Server JARs are automatically downloaded."
    },
    @{ name = "framework";       type = [string];  default = $null;       alias = "f"
       parm = "<version>";
       desc = "the framework to build (default is all)";
       note = "The framework <version> must match a valid .NET target framework moniker, e.g. net462 or netcoreapp3.1. Check the project files (.csproj) for supported versions."
    },
    @{ name = "configuration";   type = [string];  default = "Release";   alias = "c"      
       parm = "<config>";
       desc = "the build configuration";
       note = "Configuration is 'Release' by default but can be forced to be 'Debug'."
    },
    @{ name = "testFilter";      type = [string];  default = $null;       alias = "tf";
       parm = "<filter>";
       desc = "a test filter (default is all tests)";
       note = "The test <filter> can be used to filter the tests to run, it must respect the NUnit test selection language, which is detailed at: https://docs.nunit.org/articles/nunit/running-tests/Test-Selection-Language.html. Example: -tf `"test == /Hazelcast.Tests.NearCache.NearCacheRecoversFromDistortionsTest/`""
    },
    @{ name = "test";            type = [string];  default = $null;       alias = "t";
       parm = "<pattern>";
       desc = "a simplified test filter";
       note = "The simplified test <pattern> filter is equivalent to the full `"name =~ /<pattern>/`" filter."
    },
    @{ name = "coverageFilter";  type = [string];  default = $null;       alias = "cf";
       parm = "<filter>";
       desc = "a test coverage filter (default is all)";
       note = "The coverage <filter> can be used to filter the tests to cover, it must respect the dotCover language, which is detailed at: https://www.jetbrains.com/help/dotcover/Running_Coverage_Analysis_from_the_Command_LIne.html#filters."
    },
    @{ name = "sign";            type = [switch];  default = $false;
       desc = "whether to sign assemblies";
       note = "Signing assemblies requires the private signing key in build/hazelcast.snk file."
    },
    @{ name = "cover";           type = [switch];  default = $false;
       desc = "whether to run test coverage during tests"
    },
    @{ name = "version";         type = [string];  default = $null;  
       parm = "<version>";
       desc = "the version to build, set, tag, etc.";
       note = "The <version> must be a valid SemVer version such as 3.2.1 or 6.7.8-preview.2. If no value is specified then the version is obtained from src/Directory.Build.props."
    },
    @{ name = "noRestore";       type = [switch];  default = $false;      alias = "nr";
       desc = "do not restore global NuGet packages"
    },
    @{ name = "localRestore";    type = [switch];  default = $false;      alias = "lr";
       desc = "restore all NuGet packages locally"
    },
    @{ name = "constants";       type = [string];  default = $null;
       parm = "<constants>";
       desc = "additional MSBuild constants"
    },
    @{ name = "classpath";       type = [string];  default = $null;       alias = "cp";
       parm = "<classpath>";
       desc = "define an additional classpath"; 
       info = "The classpath is appended to the default remote controller or server classpath." },
    @{ name = "reproducible";    type = [switch];  default = $false;      alias = "repro";
       desc = "build reproducible assemblies" },
    @{ name = "serverConfig";    type = [string];  default = $null;
       parm = "<path>";
       desc = "the server configuration xml file"
    },
    @{ name = "accept-risks";    type = [switch];  default = $false; 
       desc = "accepts risks associated with certain commands";
       note = "Commands such as trigger-release or publish-docs will not run unless this param is set. This is a safety to prevent triggering them by accident."
    }
)

# first one is the default one
# order is important as they will run in the specified order
$actions = @(
    @{ name = "help"; 
       desc = "display this help"
    },
    @{ name = "clean";
       desc = "cleans the solution"
    },
    @{ name = "set-version"; 
       desc = "sets the version";
       note = "Updates the version in src/Directory.Build.props with the specified version."
    },
    @{ name = "verify-version"; 
       desc = "verifies the version";
       note = "Ensures that the version in src/Directory.Build.prop matches the -version option."
    },
    @{ name = "tag-release";
       desc = "tags a release";
       note = "Create a vX.Y.Z tag corresponding to the version in src/Directory.Build.Props, or the version specified via the -version option."
    },
    @{ name = "trigger-release";
       desc = "triggers a release (BEWARE!)";
       note = "Creates the vX.Y.Z tag corresponding to the version in src/Directory.Build.Props if required, and pushes the tag."
    },
    @{ name = "build"; 
       desc = "builds the solution"
    },
    @{ name = "test"; 
       desc = "runs the tests"
    },
    @{ name = "build-docs";
       desc = "builds the documentation";
       note = "Building the documentation is not supported on non-Windows platforms as DocFX requires .NET Framework."
    },
    @{ name = "git-docs"; 
       desc = "prepares the documentation release Git commit";
       note = "The commit still needs to be pushed to GitHub pages."
    },
    @{ name = "publish-docs";
       desc = "pushes the documentation releast Git commit to GitHub";
       note = "The commits needs to be prepared with git-doc."
    },
    @{ name = "pack-nuget"; 
       desc = "packs the NuGet packages"
    },
    @{ name = "push-nuget"; 
       desc = "pushes the NuGet packages to NuGet";
       note = "Pushing the NuGet packages requires a NuGet API key, which must be supplied via the NUGET_API_KEY environment variable."
    },
    @{ name = "serve-docs";
       uniq = $true;
       desc = "serves the documentation"
    },
    @{ name = "run-remote-controller"; alias = "rc";
       uniq = $true;
       desc = "runs the remote controller for tests"
    },
    @{ name = "run-server"; 
       uniq = $true;
       desc = "runs a server for tests"
    },
    @{ name = "generate-codecs";
       uniq = $true;
       desc = "generates the codec source files"
    },
    @{ name = "run-example"; alias = "ex";
       uniq = $true;
       desc = "runs an example";
       note = "The example name must be passed as first command arg e.g. ./hz.ps1 run-example Logging. Extra raw parameters can be passed to the example."
    }

    # failed-tests?!
)

# pwsh handling of args is... interesting
$clargs = [Environment]::GetCommandLineArgs()
$script = [IO.Path]::GetFileName($PSCommandPath)  # this is always going to be 'script.ps1'
$clarg0 = [IO.Path]::GetFileName($clargs[0])      # this is always going to be the pwsh exe/dll
$clarg1 = $null
if ($clargs.Count -gt 1) {
    $clarg1 = [IO.Path]::GetFileName($clargs[1])  # this is going to be either 'script.ps1' or the first arg
}
if ($script -eq $clarg1) {
    # the pwsh exe/dll running the script (e.g. launched from bash, cmd...)
    $ignore, $ignore, $argx = [Environment]::GetCommandLineArgs()
}
else {
    # the script running within pwsh (e.g. script launched from the pwsh prompt)
    $argx = $args
    # still, unquoted -- is stripped by pwsh no matter what
    # and, --foo:bar is OK but -foo:bar is still processed by pwsh
    # need to use pwsh --% escape
}

# process args
$options = Parse-Args $argx $params
if ($options -is [string]) {
    Die "$options - use 'help' to list valid parameters"
}
$do = Parse-Commands $options.commands $actions
if ($do -is [string]) {
    Die "$do - use 'help' to list valid commands"
}

if ($do.'help') {
    Write-Usage $params $actions
    exit 0
}

# process defined constants
# see https://github.com/dotnet/sdk/issues/9562
if (![string]::IsNullOrWhiteSpace($options.constants)) {
    $options.constants = $options.constants.Replace(",", ";")
}

# clear rogue environment variable
$env:FrameworkPathOverride=""

# this will be SystemDefault by default, and on some oldish environment (Windows 8...) it
# may not enable Tls12 by default, and use Tls10, and that will prevent us from connecting
# to some SSL/TLS servers (for instance, NuGet) => explicitly add Tls12
[Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol `
    -bor [Net.SecurityProtocolType]::Tls12

# validate the version to build
$hasVersion = $false
$versionPrefix = ""
$versionSuffix = ""
if (-not [System.String]::IsNullOrWhiteSpace($options.version)) {

    if (-not ($options.version -match '^(\d+\.\d+\.\d+)(?:\-([a-z0-9\.\-]*))?$')) {
        Die "Version `"$options.version`" is not a valid SemVer version"
    }

    $versionPrefix = $Matches.1
    $versionSuffix = $Matches.2

    $options.version = $versionPrefix.Trim()
    if (-not [System.String]::IsNullOrWhiteSpace($versionSuffix)) {
        $options.version += "-$($versionSuffix.Trim())"
    }
    $hasVersion = $true
}

# set versions and configure
# FIXME RENAME THESE
$hzVersion = $options.server
$hzRCVersion = "0.7-SNAPSHOT" # use appropriate version
#$hzRCVersion = "0.5-SNAPSHOT" # for 3.12.x

# determine java code repositories for tests
$mvnOssSnapshotRepo = "https://oss.sonatype.org/content/repositories/snapshots"
$mvnEntSnapshotRepo = "https://repository.hazelcast.com/snapshot"
$mvnOssReleaseRepo = "https://repo1.maven.org/maven2"
$mvnEntReleaseRepo = "https://repository.hazelcast.com/release"

if ($options.server.Contains("SNAPSHOT")) {

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

if ([string]::IsNullOrWhiteSpace($options.serverConfig)) {

    $options.serverConfig = "$buildDir/hazelcast-$hzVersion.xml"
}

# nuget packages
$nugetPackages = "$userHome/.nuget"
if ($options.localRestore) {
    $nugetPackages = "$slnRoot/.nuget"
    if (-not (Test-Path $nugetPackages)) { mkdir $nugetPackages }
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
    # a version was passed in arguments
    $isNewVersion = ($options.version -ne $currentVersion)
}
else
{
    $versionPrefix = $currentVersionPrefix
    $versionSuffix = $currentVersionSuffix
    $options.version = $currentVersion
    $isNewVersion = $false
}

$isPreRelease = -not [System.String]::IsNullOrWhiteSpace($versionSuffix)

# get doc destination according to version
if ($isPreRelease) {
    $docDstDir = "dev"
    $docMessage = "Update dev documentation ($($options.version))"
}
else {
    $docDstDir = $versionPrefix
    $docMessage = "Version $($options.version) documentation"
}

# determine framework(s)
$frameworks = @( "net462", "netcoreapp2.1", "netcoreapp3.1" )
if (-not $isWindows2) {
    $frameworks = @( "netcoreapp2.1", "netcoreapp3.1" )
}
if (-not [System.String]::IsNullOrWhiteSpace($options.framework)) {
    $framework = $options.framework.ToLower()
    if (-not $frameworks.Contains($framework)) {
        Die "Framework '$framework' is not supported on platform '$platform', supported frameworks are: $([System.String]::Join(", ", $frameworks))."
    }
    $frameworks = @( $framework )
}

# ensure we have the enterprise key for testing
function ensure-enterprise-key {

    $enterpriseKey = $env:HAZELCAST_ENTERPRISE_KEY
    if ($options.enterprise) {
        if ([System.String]::IsNullOrWhiteSpace($enterpriseKey)) {

            if (test-path "$buildDir/enterprise.key") {
                Write-Output ""
                Write-Output "Found enterprise key in build/enterprise.key file."
                $enterpriseKey = @(get-content "$buildDir/enterprise.key")[0].Trim()
                $env:HAZELCAST_ENTERPRISE_KEY = $enterpriseKey
            }
            else {
                Die "Enterprise features require an enterprise key, either in`n- HAZELCAST_ENTERPRISE_KEY environment variable, or`n- $buildDir/enterprise.key file."
            }
        }
        else {
            Write-Output ""
            Write-Output "Found enterprise key in HAZELCAST_ENTERPRISE_KEY environment variable."
        }
    }
    $script:enterpriseKey = $enterpriseKey
}

# ensure we have the nuget api key for pushing
function ensure-nuget-api-key {

    $nugetApiKey = $env:NUGET_API_KEY
    if ($do.'push-nuget' -and [System.String]::IsNullOrWhiteSpace($nugetApiKey)) {
        Die "Pushing to NuGet requires a NuGet API key in NUGET_API_KEY environment variable."
    }
}

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
function ensure-command($command) {
    $r = get-command $command 2>&1
    if ($nul -eq $r.Name) {
        Die "Command '$command' is missing."
    }
    else {
        Write-Output "Detected $command at '$($r.Source)'"
    }
}

# ensure that $script:serverVersion does not contain a -SNAPSHOT version,
# or contains a valid -SNAPSHOT version, by updating the version if necessary,
# e.g. '4.0-SNAPSHOT' may become '4.0.4-SNAPSHOT'
function ensure-server-version {
    
    $version = $script:hzVersion

    if (-not ($version.EndsWith("-SNAPSHOT"))) {
        Write-Output "Server: version $version is not a -SNAPSHOT, using this version"
        return;
    }
        
    $url = "$mvnOssSnapshotRepo/com/hazelcast/hazelcast/$version/maven-metadata.xml"
    $response = invoke-web-request $url
    if ($response.StatusCode -eq 200) {
        Write-Output "Server: found version $version on Maven, using this version"
        return;
    }
    
    Write-Output "Server: could not find $version on Maven"
    
    $url2 = "$mvnOssSnapshotRepo/com/hazelcast/hazelcast/maven-metadata.xml"
    $response2 = invoke-web-request $url2
    if ($response2.StatusCode -ne 200) {
        Die "Error: could not download metadata from Maven"
    }
    
    $metadata = [xml] $response2.Content
    $version = $version.SubString(0, $version.Length - "-SNAPSHOT".Length)
    $nodes = $metadata.SelectNodes("//version [starts-with(., '$version')]")
    
    if ($nodes.Count -lt 1) {
        Die "Server: could not find a version starting with '$version' on Maven"
    }
    
    $version2 = $nodes[0].innerText
    
    Write-Output "Server: found version $version2 on Maven, using this version"
    $script:hzVersion = $version2

    # set server version (to filter tests)
    $env:HAZELCAST_SERVER_VERSION=$version2.TrimEnd("-SNAPSHOT")
}

# get a Maven artifact
function download-maven-artifact ( $repoUrl, $group, $artifact, $jversion, $classifier, $dest ) {

    if ($jversion.EndsWith("-SNAPSHOT")) {
        $url = "$repoUrl/$group/$artifact/$jversion/maven-metadata.xml"
        $response = invoke-web-request $url
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
    $response = invoke-web-request $url $dest
    if ($response.StatusCode -ne 200) {
        Die "Failed to download $url ($($response.StatusCode))"
    }
}

# ensure we have a specified jar, by downloading it needed
# add the jar to the $script:options.classpath
function ensure-jar ( $jar, $repo, $artifact ) {

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

        download-maven-artifact $repo $group $art $ver $cls "$tmpDir/lib/$jar"
    }
    $s = ";"
    if (-not $isWindows2) { $s = ":" }
    $classpath = $script:options.classpath
    if (-not [System.String]::IsNullOrWhiteSpace($classpath)) { $classpath += $s }
    $classpath += "$tmpDir/lib/$jar"
    $script:options.classpath = $classpath
}

# ensures we have all jars required for the remote controller and the server,
# by downloading them if needed, and add them to the $script:options.classpath
function ensure-jars {

    Write-Output "Prepare server/rc..."
    if (-not (test-path "$tmpDir/lib")) { mkdir "$tmpDir/lib" >$null }

    # ensure we have the remote controller + hazelcast test jar
    ensure-jar "hazelcast-remote-controller-${hzRCVersion}.jar" $mvnOssSnapshotRepo "com.hazelcast:hazelcast-remote-controller:${hzRCVersion}"
    ensure-jar "hazelcast-${hzVersion}-tests.jar" $mvnOssRepo "com.hazelcast:hazelcast:${hzVersion}:jar:tests"

    if ($options.enterprise) {

        # ensure we have the hazelcast enterprise server + test jar
        ensure-jar "hazelcast-enterprise-${hzVersion}.jar" $mvnEntRepo "com.hazelcast:hazelcast-enterprise:${hzVersion}"
        ensure-jar "hazelcast-enterprise-${hzVersion}-tests.jar" $mvnEntRepo "com.hazelcast:hazelcast-enterprise:${hzVersion}:jar:tests"
    } else {

        # ensure we have the hazelcast server jar
        ensure-jar "hazelcast-${hzVersion}.jar" $mvnOssRepo "com.hazelcast:hazelcast:${hzVersion}"
    }
}

# gets a dotnet sdk for a particular version
function get-dotnet-sdk ( $sdks, $v ) {

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

function require-dotnet ( $full ) {

    ensure-command "dotnet"

    $dotnetVersion = (&dotnet --version)
    Write-Output "  Version $dotnetVersion"

    $sdks = (&dotnet --list-sdks)
    
    $v21 = get-dotnet-sdk $sdks "2.1"
    if ($full -and $null -eq $v21) {
        Write-Output ""
        Write-Output "This script requires Microsoft .NET Core 2.1.x SDK, which can be downloaded at: https://dotnet.microsoft.com/download/dotnet-core"
        Die "Could not find dotnet SDK version 2.1.x"
    }
    $v31 = get-dotnet-sdk $sdks "3.1"
    if ($full -and $null -eq $v31) {
        Write-Output ""
        Write-Output "This script requires Microsoft .NET Core 3.1.x SDK, which can be downloaded at: https://dotnet.microsoft.com/download/dotnet-core"
        Die "Could not find dotnet SDK version 3.1.x"
    }
    $v50 = get-dotnet-sdk $sdks "5.0"
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
    $v60 = get-dotnet-sdk $sdks "6.0" # 6.0 is not required
    
    Write-Output "  SDKs 2.1:$v21, 3.1:$v31, 5.0:$v50, 6.0:$v60"

    $script:ensuredDotnet = $true
}

# ensures we have minimal dotnet
function ensure-dotnet-minimal {
    require-dotnet $false
}

# ensures we have complete dotnet
function ensure-dotnet-complete {
    require-dotnet $true
}

# ensures that we have docfx and updates $script:docfx
# note: ensure-packages-version must run before this function
function ensure-docfx {

    $dir = "$nugetPackages/docfx.console/$docfxVersion"
    $docfx = "$dir/tools/docfx.exe"
    Write-Output "Detected DocFX $docfxVersion at '$docfx'"
    $script:docfx = $docfx

    $dir = "$nugetPackages/memberpage/$memberpageVersion"
    Write-Output "Detected DocFX MemberPage $memberpageVersion at $dir"
}

# ensures we have the git command, and validate git submodules
function ensure-git {

    ensure-command "git"
    foreach ($x in (git submodule status))
    {
        if ($x.StartsWith("-"))
        {
            Write-Output "Some Git submodules are missing, please ensure that submodules are"
            Write-Output "initialized and updated. You can initialize and update all submodules"
            Write-Output "at once with `"git submodule update --init`"."
            Die "Some Git submodules are missing."
        }
    }
    Write-Output "Detected Git submodules"
}

# ensures we have python
function ensure-python {
    ensure-command "python"
}

# ensures we can sign if required
function ensure-can-sign {

    if ($options.sign -and -not (test-path "$buildDir\hazelcast.snk")) {

        Die "Cannot sign code, missing key file $buildDir\hazelcast.snk"
    }
}

# restores the build.proj packages if needed
function ensure-build-proj {

    if ($options.noRestore) {

        Write-Output "Skip build.proj packages restore"
    }
    else {

        Write-Output "Restore build.proj packages..."
        dotnet restore "$buildDir/build.proj" --packages $nugetPackages
    }

    # get versions
    $buildAssets = (get-content "$buildDir/obj/project.assets.json" -raw) | ConvertFrom-Json
    $buildLibs = $buildAssets.Libraries
    $buildLibs.PSObject.Properties.Name | Foreach-Object {
        $p = $_.Split('/')
        $name = $p[0].ToLower()
        $pversion = $p[1]
        if ($name -eq "nunit.consolerunner") { $script:nunitVersion = $pversion }
        if ($name -eq "jetbrains.dotcover.commandlinetools") { $script:dotcoverVersion = $pversion }
        if ($name -eq "docfx.console") { $script:docfxVersion = $pversion }
        if ($name -eq "memberpage") { $script:memberpageVersion = $pversion }
    }
}

# cleans the solution
function hz-clean {

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
    if (test-path "$tmpDir\gh-pages") {
        Write-Output "  $tmpDir\gh-pages"
        remove-item "$tmpDir\gh-pages" -force -recurse
    }

    Write-Output ""
}

# ensure we have Java and it is configured
function ensure-java {

    ensure-command $java

    # sad java
    try {

        $javaVersionString = &java -version 2>&1
        $javaVersionString = $javaVersionString[0].ToString()
    }
    catch {

        Write-Output "ERROR: $_"
        Write-Output "Exception: $($_.Exception)"
        Die "Failed to get Java version"
    }

    if ($javaVersionString.StartsWith("openjdk ")) {

        if ($javaVersionString -match "\`"([0-9]+\.[0-9]+\.[0-9]+)`"") {

            $javaVersion = $matches[1]
        }
        else {

            Die "Fail to parse Java version."
        }
    }
    else {

        $p0 = $javaVersionString.IndexOf('"')
        $p1 = $javaVersionString.LastIndexOf('"')
        $javaVersion = $javaVersionString.SubString($p0+1,$p1-$p0-1)
    }

    Write-Output "  Version: $javaVersion ('$javaVersionString')"

    if (-not $javaVersion.StartsWith("1.8")) {

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

# sets the version
function hz-set-version {

    if ($isNewVersion) {

        Write-Output "Version: commit change to version $($options.version)"

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
        git commit -m "Version $($options.version)" >$null 2>&1
    }
    else {

        Write-Output "Version: already at version $($options.version)"
    }
}

# tags the release
function hz-tag-release {

    git rev-parse "refs/tags/v$($options.version)" >$null 2>&1
    if ($LASTEXITCODE -eq 0) {

        Die "Version: tag v$($options.version) already exists."
    }
    else {

        Write-Output "Version: create tag v$($options.version)"
        git diff --cached --exit-code > $null 2>&1
        if (-not $?) {
            Die "Git cache is not empty, cannot create an empty commit"
        }
        # create an empty commit to isolate the tag (helps with GitHub Actions)
        git commit --allow-empty --message "Tag v$($options.version)" >$null 2>&1
        git tag "v$($options.version)" >$null 2>&1
    }
}

# triggers the release
function hz-trigger-release {

    if (-not $options.'accept-risks') {
        Die "Cannot trigger a release if you do not accept the risks (see -accept-risks option)."
    }

    Write-Output "Version: trigger v$($options.version) release"

    $remote = Get-HazelcastRemote
    if ($remote -eq $null) {
        Die "Failed to get Hazelcast remote"
    }

    git rev-parse "refs/tags/v$($options.version)" >$null 2>&1
    if ($LASTEXITCODE -ne 0) {
        hz-tag-release
    }

    Write-Output "Version: push tag v$($options.version)"
    git push --tags $remote refs/tags/v$($options.version) >$null 2>&1
}

# generates the codecs
function hz-generate-codecs {

    Write-Output "Generated codecs"
    Write-Output "  Source         : protocol/cs"
    Write-Output "  Filters        : protocol/cs/__init__.py"

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
}

# builds the solution
function hz-build {

    # process define constants - it's a mess
    # see https://github.com/dotnet/sdk/issues/9562
    if (![string]::IsNullOrWhiteSpace($options.constants)) {
        $options.constants = $options.constants.Replace(";", "%3B") # escape ';'
    }

    Write-Output "Build"
    Write-Output "  Platform       : $platform"
    Write-Output "  Configuration  : $($options.configuration)"
    Write-Output "  Define         : $($options.constants)"
    Write-Output "  Framework      : $([System.String]::Join(", ", $frameworks))"
    Write-Output "  Building to    : $outDir"
    Write-Output "  Sign code      : $($options.sign)"
    Write-Output "  Version        : $($options.version)"
    
    Write-Output "Resolve projects dependencies..."
    $projs = Get-ChildItem -path $srcDir -recurse -depth 1 -include *.csproj
    $t = @{}
    $sc = [System.IO.Path]::DirectorySeparatorChar
    $projs | Foreach-Object {
        $proj = $_
        
        # exclude
        if (!$isWindows2 -and $proj.BaseName -eq "Hazelcast.Net.DocAsCode") { return } # continue
        
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
    Write-Output "Order projects..."
    $projs = Get-TopologicalSort $t
    $projs | Foreach-Object {
       Write-Output "  $_ "
    }

    Write-Output ""
    Write-Output "Build projets..."
    $buildArgs = @(
        "-c", $options.configuration,
        "--packages", $nugetPackages
        # "-f", "$framework"
    )

    if ($options.reproducible) {
        $buildArgs += "-p:ContinuousIntegrationBuild=true"
    }

    if ($options.sign) {
        $buildArgs += "-p:ASSEMBLY_SIGNING=true"
        $buildArgs += "-p:AssemblyOriginatorKeyFile=`"$buildDir\hazelcast.snk`""
    }

    if (![string]::IsNullOrWhiteSpace($options.constants)) {
        $buildArgs += "-p:DefineUserConstants=`"$($options.constants)`""
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
}

# builds the documentation (on Windows)
function hz-build-docs-on-windows {

    $r = "release"
    if ($isPreRelease) { $r = "pre-$r" }
    Write-Output "Build Documentation"
    Write-Output "  Version        : $($options.version)"
    Write-Output "  Version Path   : $docDstDir ($r)"
    Write-Output "  Path           : $tmpdir/docfx.out"

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
    $pluginDll = "$srcDir/Hazelcast.Net.DocAsCode/bin/$($options.configuration)/$target/Hazelcast.Net.DocAsCode.dll"
    if (-not (test-path $pluginDll)) {
        Die "Could not find Hazelcast.Net.DocAsCode.dll, make sure to build the solution first.`nIn: $srcDir/Hazelcast.Net.DocAsCode/bin/$($options.configuration)/$target"
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
    if (-not (test-path "$docDir/obj/dev/api/toc.yml")) { Die "Error: failed to generate metadata" }
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

# builds the documentation
# but only on Windows for now because docfx 3 (for .NET) is still prerelease and not complete
function hz-build-docs {
    if ($isWindows) {
        hz-build-docs-on-windows
    }
    else {
        Write-Output "Docs: building is not supported on non-Windows platforms"
    }
}

# gits the documentation (on Windows)
function hz-git-docs-on-windows {

    Write-Output "Release Documentation"
    Write-Output "  Source         : $tmpdir/docfx.out"
    Write-Output "  Pages repo     : $tmpdir/gh-pages"
    Write-Output "  Message        : $docMessage"

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

    $gitEmail = get-commarg "git-docs.user.email"
    $gitName = get-commarg "git-docs.user.name"

    if (-not [string]::IsNullOrWhiteSpace($gitEmail) -and -not [string]::IsNullOrWhiteSpace($gitName)) {
        Write-Output "Update git user to `"$gitName <$gitEmail>`""
        &git -C "$pages" config user.email $gitEmail
        &git -C "$pages" config user.name $gitName
    }

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

# gits the documentation
# but only on Windows for now because docfx 3 (for .NET) is still prerelease and not complete
function hz-git-docs {
    if ($isWindows) {
        hz-git-docs-on-windows
    }
    else {
        Write-Output "Docs: gitting is not supported on non-Windows platforms"
    }
}

# publishes the documentation (on Windows)
function hz-publish-docs-on-windows {

    Write-Output "Release Documentation"
    Write-Output "  Source         : $tmpdir/docfx.out"
    Write-Output "  Pages repo     : $tmpdir/gh-pages"

    git -C $tmpdir/gh-pages push origin gh-pages
}

# pushes the documentation
# but only on Windows
function hz-publish-docs {
    if ($isWindows) {
        if (-not $options.'accept-risks') {
            Die "Cannot publish docs if you do not accept the risks (see -accept-risks option)."
        }
        hz-publish-docs-on-windows
    }
    else {
        Write-Output "Docs: publishing is not supported on non-Windows platforms"
    }
}

# gets extra arguments for Java for Kerberos
function get-java-kerberos-args() {
    return @(
        "-Djava.util.logging.config.file=krb5/logging.properties",
        "-Djava.security.krb5.realm=HZ.LOCAL",
        "-Djava.security.krb5.kdc=SERVER19.HZ.LOCAL",
        "-Djava.security.krb5.conf=krb5/krb5.conf"
    )
}

# starts the remote controller
function start-remote-controller() {

    if (-not (test-path "$tmpDir/rc")) { mkdir "$tmpDir/rc" >$null }

    Write-Output "Starting Remote Controller..."
    Write-Output "ClassPath: $($script:options.classpath)"

    # start the remote controller
    $args = @(
        "-Dhazelcast.enterprise.license.key=$script:enterpriseKey",
        "-cp", "$($script:options.classpath)",
        "com.hazelcast.remotecontroller.Main"
    )

    $args = $args + $javaFix

    # uncomment to test Kerberos (but don't commit)
    #$args = $args + get-java-kerberos-args

    $script:remoteController = Start-Process -FilePath $java -ArgumentList $args `
        -RedirectStandardOutput "$tmpDir/rc/stdout-$hzVersion.log" `
        -RedirectStandardError "$tmpDir/rc/stderr-$hzVersion.log" `
        -PassThru
    Start-Sleep -Seconds 4

    if ($script:remoteController.HasExited) {
        Write-Output "stderr:"
        Write-Output $(get-content "$tmpDir/rc/stderr-$hzVersion.log")
        Write-Output ""
        Die "Remote controller has exited immediately."
	}
    else {
        Write-Output "Started remote controller with pid=$($script:remoteController.Id)"
    }
}

# starts the server
function start-server() {

    if (-not (test-path "$tmpDir/server")) { mkdir "$tmpDir/server" >$null }

    # ensure we have a configuration file
    if (!(test-path "$($options.serverConfig)")) {
        Die "Missing server configuration file $($options.serverConfig)"
    }

    Write-Output ""
    Write-Output "Starting Server..."
    Write-Output "ClassPath: $($script:options.classpath)"

    # depending on server version, different starter class
    $mainClass = "com.hazelcast.core.server.HazelcastMemberStarter" # 4.0
    if ($options.server.StartsWith("3.")) {
        $mainClass = "com.hazelcast.core.server.start-server" # 3.x
    }

    # start the server
    $args = @(
        "-Dhazelcast.enterprise.license.key=$enterpriseKey",
        "-cp", $script:options.classpath,
        "-Dhazelcast.config=$($options.serverConfig)",
        "-server", "-Xms2g", "-Xmx2g", "-Dhazelcast.multicast.group=224.206.1.1", "-Djava.net.preferIPv4Stack=true",
        "$mainClass"
    )

    $args = $args + $javaFix

    # uncomment to test Kerberos (but don't commit)
    #$args = $args + get-java-kerberos-args

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

# stops the remote controller
function stop-remote-controller() {

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

# stops the server
function stop-server() {

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

# runs tests for a specified framework
function run-tests ( $f ) {

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
        "-c", $options.configuration,
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

    if (-not [string]::IsNullOrEmpty($options.testFilter)) { $nunitArgs += "NUnit.Where=`"$($options.testFilter.Replace("<FRAMEWORK>", $f))`"" }

    if ($options.cover) {
        $coveragePath = "$tmpDir/tests/cover/cover-$f"
        if (!(test-path $coveragePath)) {
            mkdir $coveragePath > $null
        }

        $dotCoverArgs = @(
            "--dotCoverFilters=$($options.coverageFilter)",
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

    $script:testResults += "$tmpDir/tests/results/results-$f.xml"
}

# runs tests
function hz-test {

    # support filtering tests via a build/test.filter File
    # this is not documented / supported and is just so that we can push test-PRs that do not run all tests,
    # exclusively when testing our PR and CI system
    if ([string]::IsNullOrWhiteSpace($options.testFilter) -and [string]::IsNullOrWhiteSpace($options.test) -and (test-path "$buildDir/test.filter")) {
        $options.testFilter = (get-content "$buildDir/test.filter" -first 1)
    }

    # determine tests categories
    if(!($options.enterprise)) {
        if (-not [System.String]::IsNullOrWhiteSpace($options.testFilter)) { $options.testFilter += " && " } else { $options.testFilter = "" }
        $options.testFilter += "cat != enterprise"
    }
    if (-not [System.String]::IsNullOrWhiteSpace($options.test)) {
        if (-not [System.String]::IsNullOrWhiteSpace($options.testFilter)) { $options.testFilter += " && " } else { $options.testFilter = "" }
        $options.testFilter += "name =~ /$($options.test)/"
    }

    # determine tests name
    $testName = "<FRAMEWORK>.{C}.{m}{a}"

     # do not cover tests themselves, nor the testing plumbing
    if (-not [System.String]::IsNullOrWhiteSpace($options.coverageFilter)) { $options.coverageFilter += ";" }
    $options.coverageFilter += "-:Hazelcast.Net.Tests;-:Hazelcast.Net.Testing;-:ExpectedObjects"

    Write-Output "Tests"
    Write-Output "  Server version : $($options.server)"
    Write-Output "  Enterprise     : $($options.enterprise)"
    Write-Output "  Filter         : $($options.testFilter)"
    Write-Output "  Test Name      : $testName"
    Write-Output "  Results        : $tmpDir/tests/results"

    if ($do.'test' -and $options.cover) {
        Write-Output ""
        Write-Output "Tests Coverage"
        Write-Output "  Filter         : $($options.coverageFilter)"
        Write-Output "  Reports & logs : $tmpDir/tests/cover"
    }   


    # run tests
    $script:testResults = @()

    rm "$tmpDir\tests\results\results-*" >$null 2>&1

    try {

        start-remote-controller

        Write-Output ""
        Write-Output "Run tests..."
        foreach ($framework in $frameworks) {
            Write-Output ""
            Write-Output "Run tests for $framework..."
            run-tests $framework
        }
    }
    finally {

        stop-remote-controller
    }

    Write-Output ""
    Write-Output "Summary:"

    foreach ($testResult in $script:testResults) {

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

# runs the remote controller
function hz-run-remote-controller {

    Write-Output "Remote Controller"
    Write-Output "  Server version : $hzVersion"
    Write-Output "  RC Version     : $hzRCVersion"
    Write-Output "  Enterprise     : $($options.enterprise)"
    Write-Output "  Logging to     : $tmpDir/rc"

    try {

        start-remote-controller

        Write-Output ""
        Write-Output "Remote controller is running..."
        Read-Host "Press ENTER to stop"
    }
    finally {

        stop-remote-controller
    }
}

# runs the server
function hz-run-server {

    Write-Output "Server"
    Write-Output "  Server version : $hzVersion"
    Write-Output "  Enterprise     : $($options.enterprise)"
    Write-Output "  Configuration  : $($options.serverConfig)"
    Write-Output "  Logging to     : $tmpDir/server"

    try {

        start-server

        Write-Output ""
        Write-Output "Server is running..."
        Read-Host "Press ENTER to stop"
    }
    finally {

        stop-server
    }
}

# serves the documentation
function hz-serve-docs {

    Write-Output "Documentation Server"
    Write-Output "  Path           : $tmpdir/docfx.out"

    if (-not (test-path "$tmpDir\docfx.out")) {

        Die "Missing documentation directory."
    }

    Write-Output "Documentation server is running..."
    Write-Output "Press ENTER to stop"
    &$docfx serve "$tmpDir\docfx.out"
}

# packs a NuGet package
function nuget-pack ( $name ) {

    $packArgs = @(
        "$srcDir\$name\$name.csproj", `
        "--no-build", "--nologo", `
        "-o", "$tmpDir\output", `
        "-c", $options.configuration
    )

    if ($options.reproducible) {

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

# packs NuGet packages
function hz-pack-nuget {

    Write-Output "Nuget Package"
    Write-Output "  Configuration  : $($options.configuration)"
    Write-Output "  Version        : $($options.version)"
    Write-Output "  To             : $tmpDir/output"

    if (-not $testsSuccess) {

        Die "Cannot pack NuGet packages if tests were not successful."
    }

    Write-Output ""
    Write-Output "Pack NuGet packages..."

    # creates the nupkg (which contains dll)
    # creates the snupkg (which contains pdb with source code reference)
    # https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-pack

    nuget-pack("Hazelcast.Net")
    nuget-pack("Hazelcast.Net.Win32")

    Get-ChildItem "$tmpDir\output" | Foreach-Object { Write-Output "  $_" }
}

# pushes packages to NuGet
function hz-push-nuget {

    if (-not $testsSuccess) {

        Die "Cannot push NuGet packages if tests are not successful."
    }

    Write-Output "Push NuGet packages..."

    Write-Output "FROM: $tmpDir/output/"
    ls "$tmpDir/output/"

    &dotnet nuget push "$tmpDir/output/Hazelcast.Net.$($options.version).nupkg" --api-key $nugetApiKey --source "https://api.nuget.org/v3/index.json"
    &dotnet nuget push "$tmpDir/output/Hazelcast.Net.Win32.$($options.version).nupkg" --api-key $nugetApiKey --source "https://api.nuget.org/v3/index.json"
}

# verifies that the version in Directory.Build.props is the specified version
function hz-verify-version {

    if ($isNewVersion) {

        Die "Failed version $($options.version) verification, current is $currentVersion"
    }
}

# runs an example
function hz-run-example {

    if ($options.framework -ne $null) { $f = $options.framework } else { $f = "netcoreapp3.1" }
    $hx = "$srcDir/Hazelcast.Net.Examples/bin/$($options.configuration)/$f/hx.exe"
    if (-not (test-path $hx)) {
        Die "Could not find the examples executable. Did you build the solution?"
    }
    if ($options.commargs.Count -eq 0) {
        Die "oops"
    }
    &$hx $options.commargs
}

# ########
# ########
# ########

# globals
if ($isWindows) { $java = "javaw" } else { $java = "java" }
$javaFix = @()
$testsSuccess = $true
$ensuredDotnet = $false

# define needs
$needs = new-object Collections.Specialized.OrderedDictionary

function register-needs { 

    $args | foreach-object { $script:needs[$_] = $false } 
}

function need {

    $args | foreach-object {      
        if ($script:needs[$_] -eq $null) {
            Die "Panic: unknown need $_"
        }
        $script:needs[$_] = $true
    }
}

# register ordered needs
register-needs git 
register-needs dotnet-complete dotnet-minimal # order is important, if we need both ensure we have complete
register-needs build-proj can-sign docfx
register-needs java server-version jars # ensure jars *after* server version!
register-needs enterprise-key nuget-api-key

# define with actions + pretty sure some are missing
if ($do.'build') { need git dotnet-complete build-proj can-sign }
if ($do.'test') { need git dotnet-complete java jars server-version build-proj enterprise-key }
if ($do.'pack-nuget') { need dotnet-minimal }
if ($do.'push-nuget') { need dotnet-minimal nuget-api-key }
if ($do.'run-remote-controller') { need java jars server-version enterprise-key }
if ($do.'run-server') { need java jars server-version enterprise-key }
if ($do.'build-docs') { need git build-proj docfx }
if ($do.'publish-docs') { need git }
if ($do.'serve-docs') { need build-proj docfx }
if ($do.'generate-codecs') { need git python }

# ensure needs are satisfied
$needs.Keys | foreach-object {

    $f = $_
    if (-not $needs[$f]) { return }

    get-command "ensure-$f" >$null 2>&1
    if (-not $?) {

        Die "Panic: function 'ensure-$f' not found"
    }

    &"ensure-$f"
}

Write-Output ""
$s = ""
if ($isNewVersion) { $s += " (new, was $currentVersion)" }
Write-Output "Client version $($options.version)$s"

# this goes first
if ($do.'clean') {
    hz-clean 
    $do.'clean' = $false
}

# then always prepare directories
if (-not (test-path $tmpDir)) { mkdir $tmpDir >$null }
if (-not (test-path $outDir)) { mkdir $outDir >$null }

# do actions
$do.Keys | foreach-object {

    $f = $_
    if (-not $do[$f]) { return }

    get-command "hz-$f" >$null 2>&1
    if (-not $?) {

        Die "Panic: function 'hz-$f' not found"
    }

    Write-Output ""
    &"hz-$f"
}

Write-Output ""
Write-Output "Done."

# eof
