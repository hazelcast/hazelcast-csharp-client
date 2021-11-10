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

# prepare directories
$scriptRoot = "$PSScriptRoot"
$slnRoot = [System.IO.Path]::GetFullPath("$scriptRoot")
$srcDir = [System.IO.Path]::GetFullPath("$slnRoot/src")
$tmpDir = [System.IO.Path]::GetFullPath("$slnRoot/temp")
$buildDir = [System.IO.Path]::GetFullPath("$slnRoot/build")

# include utils
. "$buildDir\utils.ps1"

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
    @{ name = "server";          type = [string];  default = "5.0-SNAPSHOT"
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
       desc = "the full path to the server configuration xml file"
    },
    @{ name = "verbose-tests";   type = [switch];  default = $false;
       desc = "verbose tests results with errors"
    },
    @{ name = "yolo";            type = [switch]; default = $false;
       desc = "confirms excution of sensitive actions"
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
    @{ name = "build";
       desc = "builds the solution";
       need = @( "git", "dotnet-complete", "build-proj", "can-sign" )
    },
    @{ name = "test";
       desc = "runs the tests";
       need = @( "git", "dotnet-complete", "java", "server-files", "server-version", "build-proj", "enterprise-key" )
    },
    @{ name = "build-docs";
       desc = "builds the documentation";
       note = "Building the documentation is not supported on non-Windows platforms as DocFX requires .NET Framework.";
       need = @( "git", "build-proj", "docfx" )
    },
    @{ name = "git-docs";
       desc = "prepares the documentation release Git commit";
       note = "The commit still needs to be pushed to GitHub pages."
    },
    @{ name = "pack-nuget";
       desc = "packs the NuGet packages";
       need = @( "dotnet-minimal" )
    },
    @{ name = "serve-docs";
       desc = "serves the documentation";
       need = @( "build-proj", "docfx" )
    },
    @{ name = "run-remote-controller"; alias = "rc";
       uniq = $true;
       desc = "runs the remote controller for tests";
       note = "This command downloads the required JARs and configuration file.";
       need = @( "java", "server-files", "server-version", "enterprise-key" )
    },
    @{ name = "start-remote-controller";
       uniq = $true;
       desc = "starts the remote controller for tests";
       note = "This command downloads the required JARs and configuration file.";
       need = @( "java", "server-files", "server-version", "enterprise-key" )
    },
    @{ name = "stop-remote-controller";
       uniq = $true;
       desc = "stops the remote controller";
    },
    @{ name = "run-server";
       uniq = $true;
       desc = "runs a server for tests";
       note = "This command downloads the required JARs and configuration file.";
       need = @( "java", "server-files", "server-version", "enterprise-key" )
    },
    @{ name = "get-server";
       uniq = $true;
       desc = "gets a server for tests";
       note = "This command downloads the required JARs and configuration file.";
       need = @( "java", "server-files", "server-version", "enterprise-key" )
    },
    @{ name = "generate-codecs";
       uniq = $true;
       desc = "generates the codec source files";
       need = @( "git", "python" )
    },
    @{ name = "run-example"; alias = "ex";
       uniq = $true;
       desc = "runs an example";
       note = "The example name must be passed as first command arg e.g. ./hz.ps1 run-example Logging. Extra raw parameters can be passed to the example."
    },
    @{ name = "publish-examples";
       desc = "publishes examples";
       note = "Publishes examples into temp/examples";
       need = @( "dotnet-complete" )
    },
    @{ name = "cover-to-docs";
       desc = "copy test coverage to documentation";
       note = "Documentation and test coverage must exist."
    }
)

# include devops
$devops_actions = "$buildDir\devops\csharp-release\actions.ps1"
if (test-path $devops_actions) { . $devops_actions }

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
$err = Parse-Commands $options.commands $actions
if ($err -is [string]) {
    Die "$err - use 'help' to list valid commands"
}

if ((get-action $actions help).run) {
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
        Die "Version `"$($options.version)`" is not a valid SemVer version"
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
$serverVersion = $options.server # use specified value by default FIXME KILL THIS?
$isSnapshot = $options.server.Contains("SNAPSHOT") -or $options.server -eq "master"
$hzRCVersion = "0.8-SNAPSHOT" # use appropriate version
#$hzRCVersion = "0.5-SNAPSHOT" # for 3.12.x

# determine java code repositories for tests
$mvnOssSnapshotRepo = "https://oss.sonatype.org/content/repositories/snapshots"
$mvnEntSnapshotRepo = "https://repository.hazelcast.com/snapshot"
$mvnOssReleaseRepo = "https://repo1.maven.org/maven2"
$mvnEntReleaseRepo = "https://repository.hazelcast.com/release"

if ($isSnapshot) {

    $mvnOssRepo = $mvnOssSnapshotRepo
    $mvnEntRepo = $mvnEntSnapshotRepo

} else {

    $mvnOssRepo = $mvnOssReleaseRepo
    $mvnEntRepo = $mvnEntReleaseRepo
}

# more directories
$outDir = [System.IO.Path]::GetFullPath("$slnRoot/temp/output")
$docDir = [System.IO.Path]::GetFullPath("$slnRoot/doc")

if ($isWindows) { $userHome = $env:USERPROFILE } else { $userHome = $env:HOME }

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

function determine-target-frameworks {
    $csproj = [xml] (get-content "$srcDir/Hazelcast.Net.Tests/Hazelcast.Net.Tests.csproj")
    $csproj.Project.PropertyGroup | foreach-object {
        if ($_.Condition -ne $null -and $_.Condition.Contains("Windows_NT")) {
            if ($_.Condition.Contains("==")) {
                $targetsOnWindows = $_.TargetFrameworks
            }
            if ($_.Condition.Contains("!=")) {
                $targetsOnLinux = $_.TargetFrameworks
            }
        }
    }
    if ($isWindows) {
        $frameworks = $targetsOnWindows
    }
    else {
        $frameworks = $targetsOnLinux
    }
    $frameworks = $frameworks.Split(";", [StringSplitOptions]::RemoveEmptyEntries)
    for ($i = 0; $i -lt $framework.Length; $i++) {
        $frameworks[$i] = $frameworks[$i].Trim()
    }
    return $frameworks
}

# determine framework(s) - for building and running tests
$frameworks = determine-target-frameworks
if (-not [System.String]::IsNullOrWhiteSpace($options.framework)) {
    $framework = $options.framework.ToLower()
    if ($framework.Contains(',')) {
        $fwks = $framework.Split(',')
        foreach ($fwk in $fwks) {
            if (-not $frameworks.Contains($fwk)) {
                Die "Framework '$fwk' is not supported on platform '$platform', supported frameworks are: $([System.String]::Join(", ", $frameworks))."
            }
        }
        $frameworks = $fwks
    }
    else {
        if (-not $frameworks.Contains($framework)) {
            Die "Framework '$framework' is not supported on platform '$platform', supported frameworks are: $([System.String]::Join(", ", $frameworks))."
        }
        $frameworks = @( $framework )
    }
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

# $options.server contains the specified server version, which can be 5.0, 5.0.1,
# 5.0-SNAPSHOT, 5.0.1-SNAPSHOT, master, or anything really - and it may match an
# actual server version, but also be master, or 4.0-SNAPSHOT that would be n/a on
# Maven, because for some reason we don't keep .0-SNAPSHOT on Maven, but Maven
# would advertise the 4.0.x-SNAPSHOT instead - so here we are going to figure out
# if, from the specified $options.server, we can derive a $script:serverVersion
# that is an available, actual server version.
# or, $options.serverActual
function ensure-server-version {

    $version = $options.server

    # set the actual server version
    # this will be updated below if required
    $script:serverVersion = $version

    # set server version (to filter tests)
    # this will be updated below if required
    $env:HAZELCAST_SERVER_VERSION=$version.TrimEnd("-SNAPSHOT")

    if (-not $isSnapshot) {
        Write-Output "Server: version $version is not a -SNAPSHOT, using this version"
        return;
    }

    if ($version -eq "master") {
        Write-Output "Server: version is $version, determine actual version from GitHub"
        $url = "https://raw.githubusercontent.com/hazelcast/hazelcast/master/pom.xml"
        Write-Output "GET $url"
        $response = invoke-web-request $url
        if ($response.StatusCode -ne 200) {
            Die "Error: could not download POM file from GitHub ($($response.StatusCode))"
        }
        $pom = [xml] $response.Content
        if ($pom.project -eq $null -or $pom.project.version -eq $null) {
            Die "Error: got invalid POM file from GitHub (could not find version)"
        }
        $version = $pom.project.version
        if ([string]::IsNullOrWhiteSpace($version)) {
            Die "Error: got invalid POM file from GitHub (could not find version)"
        }
        if (-not $version.EndsWith("-SNAPSHOT")) {
            $version += "-SNAPSHOT"
        }
        Write-Output "Server: determined version $version from GitHub"
        $env:HAZELCAST_SERVER_VERSION=$version.TrimEnd("-SNAPSHOT")
        $script:serverVersion = $version
    }

    $url = "$mvnOssSnapshotRepo/com/hazelcast/hazelcast/$version/maven-metadata.xml"
    Write-Output "GET $url"
    $response = invoke-web-request $url
    if ($response.StatusCode -eq 200) {
        Write-Output "Server: found version $version on Maven, using this version"
        return;
    }

    Write-Output "Server: could not find version $version on Maven ($($response.StatusCode))"

    $url2 = "$mvnOssSnapshotRepo/com/hazelcast/hazelcast/maven-metadata.xml"
    Write-Output "GET $url2"
    $response2 = invoke-web-request $url2
    if ($response2.StatusCode -ne 200) {
        Die "Error: could not download metadata from Maven ($($response2.StatusCode))"
    }

    $metadata = [xml] $response2.Content

    $version0 = $version
    $version = $version.SubString(0, $version.Length - "-SNAPSHOT".Length)
    $nodes = $metadata.SelectNodes("//version [starts-with(., '$version')]")

    if ($nodes.Count -lt 1) {
        Die "Server: could not find a version starting with '$version' on Maven"
    }

    foreach ($node in $nodes | sort-object -descending -property innerText) {
        $nodeVersion = $node.innerText
        if ($nodeVersion -eq $version0) {  # we 404ed on that one already (why is it listed?!)
            Write-Output "Server: skip listed version $nodeVersion"
            continue
        }
        Write-Output "Server: try listed version $nodeVersion"
        $url = "$mvnOssSnapshotRepo/com/hazelcast/hazelcast/$nodeVersion/maven-metadata.xml"
        Write-Output "Maven: $url"
        $response = invoke-web-request $url
        if ($response.StatusCode -eq 200) {
            Write-Output "Server: found version $nodeVersion on Maven, using this version"
            $script:serverVersion = $nodeVersion
            $env:HAZELCAST_SERVER_VERSION=$nodeVersion.TrimEnd("-SNAPSHOT")
            return;
        }
        else {
            Write-Output "Server: could not find version $nodeVersion on Maven ($($response.StatusCode))"
        }
    }

    Die "Server: could not find a version."
}

# get a Maven artifact
function download-maven-artifact ( $repoUrl, $group, $artifact, $jversion, $classifier, $dest ) {

    if ($jversion.EndsWith("-SNAPSHOT")) {
        $url = "$repoUrl/$group/$artifact/$jversion/maven-metadata.xml"
        $response = invoke-web-request $url
        if ($response.StatusCode -ne 200) {
            Die "Failed to download $url ($($response.StatusCode))"
        }

        try {
            $metadata = [xml] $response.Content
        }
        catch {
            Die "Invalid metadata content at $url."
        }

        $xpath = "//snapshotVersion [extension='jar'"
        if (![System.String]::IsNullOrWhiteSpace($classifier)) {
            $xpath += " and classifier='$classifier'"
        }
        else {
            $xpath += " and not(classifier)"
        }
        $xpath += "]"
        $node = $metadata.SelectNodes($xpath)[0]
        if ($node -eq $null) {
            Die "Incomplete metadata at $url."
        }
        $jarVersion = "-" + $node.value
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
    if (-not $isWindows) { $s = ":" }
    $classpath = $script:options.classpath
    if (-not [System.String]::IsNullOrWhiteSpace($classpath)) { $classpath += $s }
    $classpath += "$tmpDir/lib/$jar"
    # Be sure to quote the path to escape from white space
    # where you call the $script:options.classpath
    # ex: $quotedClassPath = '"{0}"' -f $script:options.classpath
    $script:options.classpath = $classpath
}

# ensures we have all jars & config required for the remote controller and the server,
# by downloading them if needed, and add them to the $script:options.classpath
function ensure-server-files {

    Write-Output "Prepare server/rc..."
    if (-not (test-path "$tmpDir/lib")) { mkdir "$tmpDir/lib" >$null }

    # ensure we have the remote controller + hazelcast test jar
    ensure-jar "hazelcast-remote-controller-${hzRCVersion}.jar" $mvnOssSnapshotRepo "com.hazelcast:hazelcast-remote-controller:${hzRCVersion}"
    ensure-jar "hazelcast-${serverVersion}-tests.jar" $mvnOssRepo "com.hazelcast:hazelcast:${serverVersion}:jar:tests"

    if ($options.enterprise) {

        # ensure we have the hazelcast enterprise server
        if (${serverVersion} -lt "5.0") { # FIXME version comparison
            ensure-jar "hazelcast-enterprise-all-${serverVersion}.jar" $mvnEntRepo "com.hazelcast:hazelcast-enterprise-all:${serverVersion}"
        }
        else {
            ensure-jar "hazelcast-enterprise-${serverVersion}.jar" $mvnEntRepo "com.hazelcast:hazelcast-enterprise:${serverVersion}"
            ensure-jar "hazelcast-sql-${serverVersion}.jar" $mvnOssRepo "com.hazelcast:hazelcast-sql:${serverVersion}"
        }

        # ensure we have the hazelcast enterprise test jar
        ensure-jar "hazelcast-enterprise-${serverVersion}-tests.jar" $mvnEntRepo "com.hazelcast:hazelcast-enterprise:${serverVersion}:jar:tests"
    }
    else {

        # ensure we have the hazelcast server jar
        if (${serverVersion} -lt "5.0") { # FIXME version comparison
            ensure-jar "hazelcast-all-${serverVersion}.jar" $mvnOssRepo "com.hazelcast:hazelcast-all:${serverVersion}"
        }
        else {
            ensure-jar "hazelcast-${serverVersion}.jar" $mvnOssRepo "com.hazelcast:hazelcast:${serverVersion}"
            ensure-jar "hazelcast-sql-${serverVersion}.jar" $mvnOssRepo "com.hazelcast:hazelcast-sql:${serverVersion}"
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($options.serverConfig)) {
        # config was specified, it must exist
        if  (test-path $options.serverConfig) {
            Write-Output "Detected $($options.serverConfig)"
        }
        else {
            Die "Configuration file $($options.serverConfig) is missing."
        }
    }
    elseif (test-path "$buildDir\hazelcast-$($options.server).xml") {
        # config was not specified, try with exact specified server version
        Write-Output "Detected hazelcast-$($options.server).xml"
        $options.serverConfig = "$buildDir\hazelcast-$($options.server).xml"
    }
    elseif (test-path "$buildDir\hazelcast-$serverVersion.xml") {
        # config was not specified, try with detected server version
        Write-Output "Detected hazelcast-$serverVersion.xml"
        $options.serverConfig = "$buildDir\hazelcast-$serverVersion.xml"
    }
    else {
        # no config found, try to download

        Write-Output "Downloading hazelcast-default.xml -> hazelcast-$serverVersion.xml..."
        $found = $false
        $v = $serverVersion.TrimEnd("-SNAPSHOT")

        # special master case
        if ($options.server -eq "master") {
            $url = "https://raw.githubusercontent.com/hazelcast/hazelcast/master/hazelcast/src/main/resources/hazelcast-default.xml"
            $dest = "$buildDir/hazelcast-$serverVersion.xml"
            $response = invoke-web-request $url $dest
            if ($response.StatusCode -ne 200) {
                if (test-path $dest) { rm $dest }
                Die "Error: failed to download hazelcast-default.xml (${response.StatusCode}) from branch master"
            }
            Write-Output "Found hazelcast-default.xml from branch master"
            $found = $true
        }

        if (-not $found) {
            # try tag eg 'v4.2.1' or 'v4.3'
            $url = "https://raw.githubusercontent.com/hazelcast/hazelcast/v$v/hazelcast/src/main/resources/hazelcast-default.xml"
            $dest = "$buildDir/hazelcast-$serverVersion.xml"
            $response = invoke-web-request $url $dest

            if ($response.StatusCode -ne 200) {
                Write-Output "Failed to download hazelcast-default.xml (${response.StatusCode}) from tag v$v"
                if (test-path $dest) { rm $dest }
            }
            else {
                Write-Output "Found hazelcast-default.xml from tag v$v"
                $found = $true
            }
        }

        if (-not $found) {
            $p0 = $v.IndexOf('.')
            $p1 = $v.LastIndexOf('.')
            if ($p0 -ne $p1) {
                $v = $v.SubString(0, $p1) # 4.2.1 -> 4.2 but 4.3 remains 4.3
            }

            # try branch eg '4.2.z' or '4.3.z'
            $url = "https://raw.githubusercontent.com/hazelcast/hazelcast/$v.z/hazelcast/src/main/resources/hazelcast-default.xml"
            $response = invoke-web-request $url $dest

            if ($response.StatusCode -ne 200) {
                Write-Output "Failed to download hazelcast-default.xml (${response.StatusCode}) from branch $v.z"
                if (test-path $dest) { rm $dest }
            }
            else {
                Write-Output "Found hazelcast-default.xml from branch $v.z"
                $found = $true
            }
        }

        if (-not $found) {
            # try branch eg '4.3' because '5.0' exists but not '5.0.z'
            $url = "https://raw.githubusercontent.com/hazelcast/hazelcast/$v/hazelcast/src/main/resources/hazelcast-default.xml"
            $response = invoke-web-request $url $dest

            if ($response.StatusCode -ne 200) {
                Write-Output "Failed to download hazelcast-default.xml (${response.StatusCode}) from branch $v"
                if (test-path $dest) { rm $dest }
            }
            else {
                Write-Output "Found hazelcast-default.xml from branch $v"
                $found = $true
            }
        }

        if (-not $found) {
            Die "Running out of options... failed to download hazelcast-default.xml."
        }

        $options.serverConfig = "$buildDir\hazelcast-$serverVersion.xml"
    }
}

# gets a dotnet sdk for a particular version
function get-dotnet-sdk ( $sdks, $v, $preview ) {

    # trust dotnet to return the sdks ordered by version, so last is the highest version
    # exclude versions containing "-" ie anything pre-release (TODO: why?)
    $sdk = $sdks `
        | Select-String -pattern "^$v" `
        | Foreach-Object { $_.ToString().Split(' ')[0] } `
        | Where-Object { ($preview -and $_.Contains('-')) -or (-not $preview -and -not $_.Contains('-')) } `
        | Select-Object -last 1

    if ($null -eq $sdk) { return "n/a" }
    else { return $sdk.ToString() }
}

function require-dotnet ( $full ) {

    if ($script:ensuredDotnet) { return }

    ensure-command "dotnet"

    $dotnetVersion = (&dotnet --version)
    Write-Output "  Version $dotnetVersion"

    $allowPrerelease = $true
    if (test-path "$slnRoot/global.json") {
        $json = (get-content "$slnRoot/global.json" -raw) | convertFrom-json
        if ($json.sdk -ne $null -and $json.sdk.allowPrerelease -ne $null) {
            $allowPrerelease = $json.sdk.allowPrerelease
        }
    }

    if ($allowPrerelease) {
        Write-Output "  (global.json is missing or allows pre-release versions)"
    }
    else {
        Write-Output "  (global.json does not allow pre-release versions)"
    }

    $sdks = (&dotnet --list-sdks)

    $v21 = get-dotnet-sdk $sdks "2.1" $false
    if ($full -and $null -eq $v21) {
        Write-Output ""
        Write-Output "This script requires Microsoft .NET Core 2.1.x SDK, which can be downloaded at: https://dotnet.microsoft.com/download/dotnet-core"
        Die "Could not find dotnet SDK version 2.1.x"
    }
    $v31 = get-dotnet-sdk $sdks "3.1" $false
    if ($full -and $null -eq $v31) {
        Write-Output ""
        Write-Output "This script requires Microsoft .NET Core 3.1.x SDK, which can be downloaded at: https://dotnet.microsoft.com/download/dotnet-core"
        Die "Could not find dotnet SDK version 3.1.x"
    }
    $v50 = get-dotnet-sdk $sdks "5.0" $false
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
    $v60 = get-dotnet-sdk $sdks "6.0" $false # 6.0 is not required

    $v50preview = get-dotnet-sdk $sdks "5.0" $true
    $v60preview = get-dotnet-sdk $sdks "6.0" $true

    if ($allowPrerelease) { $v50 = $v50preview } elseif ($v50preview -ne 'n/a') { $v50 = "$v50 ($v50preview)" }
    if ($allowPrerelease) { $v60 = $v60preview } elseif ($v60preview -ne 'n/a') { $v60 = "$v60 ($v60preview)" }

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

function clean-dir ( $dir ) {
    if (test-path $dir) {
        Write-Output "  $dir"
        remove-item $dir -force -recurse
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

    # clears output, publish
    clean-dir $outDir
    clean-dir "$tmpdir/publish"

    # clears tests (results, cover...)
    clean-dir "$tmpDir\tests"

    # clears logs (server, rc...)
    if (test-path "$tmpDir") {
        gci $tmpDir -include *.log -Recurse | foreach ($_) {
            Write-Output "  $($_.fullname)"
            remove-item $_.fullname -Force
        }
    }

    # clears docs
    clean-dir "$tmpDir\docfx.out"
    clean-dir "$docDir\templates\hz\Plugins"
    clean-dir "$tmpDir\gh-pages"
    clean-dir "$tmpDir\gh-pages-patches"

    # clean ndepend
    clean-dir "$tmpDir\ndepend.out"

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

        if ($javaVersionString -match "\`"([0-9]+\.[0-9]+\.[0-9]+)([_-][0-9-_]+)?`"") {

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

        # "Scripting is currently unsupported for Java 15 and newer. These versions
        #  of Java do not come with a JavaScript engine, which is necessary for this
        #  feature to work."
        $pos = $javaVersion.IndexOf('.')
        $javaMajor = $javaVersion.SubString(0, $pos)
        if (-not ($javaVersion -lt "12") ) {
            Die "Java version >11 not supported (JavaScript scripting not supported)"
        }
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

    if ($frameworks.Count -eq 1) {
        $framework = $frameworks[0]
    }
    else {
        $framework = "(all)"
    }

    Write-Output "Build"
    Write-Output "  Platform       : $platform"
    Write-Output "  Configuration  : $($options.configuration)"
    Write-Output "  Define         : $($options.constants)"
    Write-Output "  Framework      : $framework"
    Write-Output "  Building to    : $outDir"
    Write-Output "  Sign code      : $($options.sign)"
    Write-Output "  Version        : $($options.version)"
    Write-Output ""

    Write-Output "Resolve projects..."
    $projs = Get-ChildItem -path $srcDir -recurse -depth 1 -include *.csproj
    $t = @{}
    $sc = [System.IO.Path]::DirectorySeparatorChar
    $projs | Foreach-Object {
        $proj = $_

        $k = $proj.FullName.SubString($srcDir.Length + 1).Replace("\", $sc).Replace("/", $sc)

        # exclude
        if ($proj.BaseName -eq "Hazelcast.Net.DocAsCode" -and (
            !$isWindows -or            # not on linux
            $frameworks.Count -eq 1 )  # not for specific framework
        ) {
            Write-Output "  $(get-project-name $k) -> (excluded) "
            return  # continue
        }

        $x = [xml] (Get-Content $proj);
        $n = $x.SelectNodes("//ProjectReference/@Include");
        if ($t[$k] -eq $null) { $t[$k] = @() }

        if ($n.Count -eq 0) {
            Write-Output "  $(get-project-name $k) -> (no dependencies)"
        }
        else {
            $n | Foreach-Object {
                $dep = $_.Value
                $d = $dep.SubString("../".Length).Replace("\", $sc).Replace("/", $sc)
                Write-Output "  $(get-project-name $k) -> $(get-project-name $d)"
                $t[$k] += $d
            }
        }
    }

    Write-Output ""
    Write-Output "Order projects..."
    $projs = Get-TopologicalSort $t
    $projs | Foreach-Object {
       Write-Output "  $(get-project-name $_) "
    }

    Write-Output ""
    Write-Output "Build projets..."
    $buildArgs = @(
        "-c", $options.configuration,
        "--packages", $nugetPackages
    )

    if ($frameworks.Count -eq 1) {
        $buildArgs += "-f"
        $buildArgs += $frameworks[0]
    }

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
        Write-Output "> dotnet build $srcDir\$_ $buildArgs"
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
        foreach-object {
            $_ -replace "__DEST__", $docDstDir `
               -replace "__VERSION__", $options.version } |
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
    }
    else {
        $devwarnMessage = ""
        $devwarnClass = ""
    }

    # set or clear the DEVWARN header in documentation files
    get-childitem -recurse -path "$tmpDir/docfx.out/$docDstDir" -filter *.html |
        foreach-object {
            $text = get-content -path $_
            $text = $text `
                -replace "<!-- DEVWARN -->", $devwarnMessage `
                -replace "DEVWARN", $devwarnClass
            set-content -path $_ -value $text
        }

    # change the TOC link - probably can be done directly in DocFX but meh
    get-childitem -recurse -path "$tmpDir/docfx.out/$docDstDir" -filter *.html |
        foreach-object {
            $text = get-content -path $_
            $text = $text `
                -replace "(?s-m)\<meta property=`"docfx:navrel`" content=`"../(.*)toc.html`"\>", "<meta property=`"docfx:navrel`" content=`"`${1}toc.html`">"
            set-content -path $_ -value $text
        }

    # add link to public development documentation, if version has a suffix
    $path = "$tmpDir/docfx.out/versions.html"
    $text = get-content -path $path
    $repl = "n/a"
    if (-not [System.String]::IsNullOrWhiteSpace($versionSuffix))
    {
        $repl = "$($options.version)`${1}"
    }
    $text = $text -replace '(?s-m)\<devdoc\>\$version(.*)\</devdoc\>', $repl # s-m enables single-line, disables multi-lines
    set-content -path $path -value $text

    $text = get-content "$tmpDir/docfx.out/404.html"
    $text = $text -replace "<head>", "<head>`n    <base href=`"/hazelcast-csharp-client/`">"
    set-content -path "$tmpDir/docfx.out/404.html" -value $text
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

# copy test coverage to doc
function hz-cover-to-docs-on-windows {

    $docs = "$tmpDir/docfx.out"
    $versiondocs = "$docs/$docDstDir"
    $coverdocs = "$versiondocs/cover"
    $coverdir = "$tmpDir/tests/cover/cover-netcoreapp3.1.html"

    Write-Output ""
    Write-Output "Copy tests coverage to documentation"
    Write-Output "  Source         : $coverdir"
    Write-Output "  Documentation  : $coverdocs"

    if (-not (test-path $docs)) { Die "Could not find $docs. Maybe you should build the docs first?" }
    if (-not (test-path $coverdir)) { Die "Could not find $coverdir. Maybe you should run tests with coverage first?" }
    if (-not (test-path $versiondocs)) { mkdir $versiondocs >$null 2>&1 }

    if (test-path $coverdocs) { remove-item -recurse -force $coverdocs }
    mkdir $coverdocs >$null 2>&1

    copy-item -recurse "$coverdir/*" "$coverdocs/"
    add-content "$coverdocs/index/css/dotcover.report.css" "`n`npre.source-code { font-family:Menlo,Monaco,Consolas,`"Courier New`",monospace; font-size:12px; }"
    $index1 = get-content "$docDir/templates/hz/cover-index.html"
    $index2 = [string]::Join(' ', (get-content "$coverdocs/index.html"))
    if (-not ($index2 -match '(?s-m)<script type="text/javascript">(.*)</script>')) {
        Die "panic: no cover data"
    }
    $data = $Matches.1
    $index1 = $index1.Replace("/*DOTCOVER_DATA*/", $data);
    set-content -path "$coverdocs/index.html" -value $index1
}

# copy test coverage to doc
# but only on Windows for now because docfx 3 (for .NET) is still prerelease and not complete
function hz-cover-to-docs {
    if ($isWindows) {
        hz-cover-to-docs-on-windows
    }
    else {
        Write-Output "Docs: cover-to-docs is not supported on non-Windows platforms"
    }
}

# copy test coverage to doc
# but only on Windows for now because docfx 3 (for .NET) is still prerelease and not complete
function hz-cover-to-docs {
    if ($isWindows) {
        hz-cover-to-docs-on-windows
    }
    else {
        Write-Output "Docs: cover-to-docs is not supported on non-Windows platforms"
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

    # create a symlink for latest docs -- NO! this is a release process task, see build-release.yml

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
    if (test-path "$tmpDir/rc/pid") {
        Die "Error: cannot start remote controller, pid file found in $tmpDir/rc"
    }

    Write-Output "Starting Remote Controller..."
    Write-Output "ClassPath: $($script:options.classpath)"

    $quotedClassPath = '"{0}"' -f $script:options.classpath

    # start the remote controller
    $args = @(
        "-Dhazelcast.enterprise.license.key=$script:enterpriseKey",
        "-cp", $quotedClassPath,
        "com.hazelcast.remotecontroller.Main"
    )

    $args = $args + $javaFix

    # uncomment to test Kerberos (but don't commit)
    #$args = $args + get-java-kerberos-args

    $script:remoteController = Start-Process -FilePath $java -ArgumentList $args `
        -RedirectStandardOutput "$tmpDir/rc/stdout-$serverVersion.log" `
        -RedirectStandardError "$tmpDir/rc/stderr-$serverVersion.log" `
        -PassThru
    Start-Sleep -Seconds 4

    if ($script:remoteController.HasExited) {
        Write-Output "stderr:"
        Write-Output $(get-content "$tmpDir/rc/stderr-$serverVersion.log")
        Write-Output ""
        Die "Remote controller has exited immediately."
	}
    else {
        set-content "$tmpDir/rc/pid" $script:remoteController.Id
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

    $quotedClassPath = '"{0}"' -f $script:options.classpath
    $quotedConfig = '"{0}"' -f $options.serverConfig

    # start the server
    $args = @(
        "-Dhazelcast.enterprise.license.key=$enterpriseKey",
        "-cp", $quotedClassPath,
        "-Dhazelcast.config=$quotedConfig",
        "-server", "-Xms2g", "-Xmx2g", "-Dhazelcast.multicast.group=224.206.1.1", "-Djava.net.preferIPv4Stack=true",
        "$mainClass"
    )

    $args = $args + $javaFix

    # uncomment to test Kerberos (but don't commit)
    #$args = $args + get-java-kerberos-args

    $script:serverProcess = Start-Process -FilePath $java -ArgumentList $args `
        -RedirectStandardOutput "$tmpDir/server/stdout-$serverVersion.log" `
        -RedirectStandardError "$tmpDir/server/stderr-$serverVersion.log" `
        -PassThru
    Start-Sleep -Seconds 4

    if ($script:serverProcess.HasExited) {
        Die "Server has exited immediately."
	}
    else {
        Write-Output "Started server with pid=$($script:serverProcess.Id)"
    }
}

# tests the remote controller
function test-remote-controller() {
    return test-path "$tmpDir/rc/pid"
}

# stops the remote controller
function stop-remote-controller() {

    # stop the remote controller
    Write-Output ""
    if ($script:remoteController -and $script:remoteController.Id -and -not $script:remoteController.HasExited) {
        Write-Output "Stopping remote controller (pid=$($script:remoteController.Id))..."
        $script:remoteController.Kill($true) # entire tree
        rm "$tmpDir/rc/pid"
	}
    else {
        Write-Output "Remote controller is not running."
	}
}

# kills a process tree
function kill-tree ([int] $ppid) {
    get-cimInstance Win32_Process | `
        where-object { $_.ParentProcessId -eq $ppid } | `
        foreach-object { kill-tree $_.ProcessId }
    stop-process -force -id $ppid
}

# kills the remote controller
function kill-remote-controller() {
    if (-not (test-path "$tmpDir/rc/pid")) {
        Write-Output "Remote controller is not running."
    }
    else {
        $rcpid = get-content "$tmpDir/rc/pid"
        kill-tree $rcpid
        rm "$tmpDir/rc/pid"
        Write-Output "Remote controller process $pid has been killed"
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
        $coveragePath = "$tmpDir/tests/cover/cover-$f.html"
        if (!(test-path $coveragePath)) {
            mkdir $coveragePath > $null
        }

        $dotCoverArgs = @(
            "--dotCoverFilters=$($options.coverageFilter)",
            "--dotCoverAttributeFilters=System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute",
            "--dotCoverLogFile=$tmpDir/tests/cover/cover-$f.log", # log
            "--dotCoverSourcesSearchPaths=$srcDir", # reference sources in HTML output

            # generate HTML (to publish on docs), JSON (to parse results for GitHub), DetailedXML (for codecov)
            "--dotCoverReportType=HTML,JSON,DetailedXML", # HTML|XML|JSON|... https://www.jetbrains.com/help/dotcover/dotCover__Console_Runner_Commands.html#cover-dotnet
            "--dotCoverOutput=$coveragePath/index.html;$tmpDir/tests/cover/cover-$f.json;$tmpDir/tests/cover/cover-$f.xml"
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

    if ($options.cover) {
        Write-Output ""
        Write-Output "Tests Coverage"
        Write-Output "  Filter         : $($options.coverageFilter)"
        Write-Output "  Reports & logs : $tmpDir/tests/cover"
    }

    # run tests
    $script:testResults = @()

    rm "$tmpDir\tests\results\results-*" >$null 2>&1

    $ownsrc = $false
    try {

        if (!(test-remote-controller)) {
            start-remote-controller
            $ownsrc = $true # we own it and need to stop it
        }

        Write-Output ""
        Write-Output "Run tests..."
        foreach ($framework in $frameworks) {
            Write-Output ""
            Write-Output "Run tests for $framework..."
            run-tests $framework
        }
    }
    finally {

        if ($ownsrc) {
            stop-remote-controller
        }
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
                    if ($options.'verbose-tests') {
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
    Write-Output "  Server version : $serverVersion"
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

# starts the remote controller
function hz-start-remote-controller {

    Write-Output "Remote Controller"
    Write-Output "  Server version : $serverVersion"
    Write-Output "  RC Version     : $hzRCVersion"
    Write-Output "  Enterprise     : $($options.enterprise)"
    Write-Output "  Logging to     : $tmpDir/rc"

    start-remote-controller

    Write-Output ""
    Write-Output "Remote controller is running..."
}

# stops the remote Controller
function hz-stop-remote-controller {
    kill-remote-controller
}

# gets the Server
function hz-get-server {

    Write-Output "Server"
    Write-Output "  Server version : $serverVersion"
    Write-Output "  Enterprise     : $($options.enterprise)"
    Write-Output "  Configuration  : $($options.serverConfig)"

    # nothing to do
}

# runs the server
function hz-run-server {

    Write-Output "Server"
    Write-Output "  Server version : $serverVersion"
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
    nuget-pack("Hazelcast.Net.DependencyInjection")

    Get-ChildItem "$tmpDir\output" | Foreach-Object { Write-Output "  $_" }
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
    $ext = ""
    if ($isWindows) { $ext = ".exe" }
    $hx = "$srcDir/Hazelcast.Net.Examples/bin/$($options.configuration)/$f/hx$ext"
    if (-not (test-path $hx)) {
        Die "Could not find the examples executable. Did you build the solution?"
    }
    if ($options.commargs.Count -eq 0) {
        Die "oops"
    }
    &$hx $options.commargs
}

# publish examples
function hz-publish-examples {

        Write-Output ""
        Write-Output "Publish examples..."

        if (test-path "$tmpDir/examples") {
            remove-item "$tmpDir/examples" -Force -Recurse
        }

        mkdir "$tmpDir/examples" >$null 2>&1

        foreach ($framework in $frameworks) {
            Write-Output ""
            Write-Output "Publish examples for $framework..."
            $publishArgs = @(
                "$srcDir/Hazelcast.Net.Examples",
                "-c", "$($options.configuration)",
                "-f", "$framework",
                "-o", "$tmpDir/examples/examples-$framework",
                "--no-restore", "--no-build", "--packages", $nugetPackages
            )
            dotnet publish $publishArgs
            compress-archive -path "$tmpDir/examples/examples-$framework/" -destinationPath "$tmpDir/examples/examples-$framework.zip"
        }
}

# ########
# ########
# ########

# globals
if ($isWindows) { $java = "javaw" } else { $java = "java" }
$javaFix = @()
$testsSuccess = $true
$ensuredDotnet = $false

# define needs - ordered!
$needs = new-object Collections.Specialized.OrderedDictionary
function register-needs { $args | foreach-object { $script:needs[$_] = $false } }
register-needs git
register-needs dotnet-complete dotnet-minimal # order is important, if we need both ensure we have complete
register-needs java server-version server-files # ensure server files *after* server version!
register-needs enterprise-key nuget-api-key
register-needs build-proj can-sign docfx

# gather needs from actions
$actions | foreach-object {

    $action = $_
    if (-not $action.run) { return }

    if ($action.need -ne $null) {
        $action.need | foreach-object {
            $needs[$_] = $true
        }
    }
}

# ensure needs are satisfied (in order)
Write-Output ""
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
$clean = get-action $actions clean
if ($clean.run) {
    hz-clean
    $clean.run = $false
}

# then always prepare directories
if (-not (test-path $tmpDir)) { mkdir $tmpDir >$null }
if (-not (test-path $outDir)) { mkdir $outDir >$null }

# do actions
$actions | foreach-object {

    $action = $_
    if (-not $action.run) { return }

    $f = "hz-$($action.name)"
    get-command $f >$null 2>&1

    if (-not $?) { Die "Panic: function '$f' not found" }

    Write-Output ""
    &$f
}

Write-Output ""
Write-Output "Done."

# eof
