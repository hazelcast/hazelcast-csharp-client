param(
	# whether to test ??
    [switch]$enterprise = $false,

	# whether to test ??
    [switch]$netcore =  $false,

	# the server version
    [string]$serverVersion = "4.0",

	# whether to run tests coverage
    [switch]$coverage = $false
)

# clear rogue environment variable
$env:FrameworkPathOverride=""

# versions and configuration
$hzVersion = $serverVersion
$hzRCVersion = "0.7-SNAPSHOT" # use appropriate version
$configuration = "Release" # or Debug, should always be Release
$hzLocalBuild = $false # $true to skip downloading dependencies
$hzToolsCache = 12 #days
$hzVsMajor = 16 # force VS major version, default to 16 (VS2019) for now
$hzVsPreview = $false # whether to look for previews of VS

# determine code repositories
$mvnOssSnapshotRepo = "https://oss.sonatype.org/content/repositories/snapshots"
$mvnEntSnapshotRepo = "https://repository.hazelcast.com/snapshot/"
$mvnOssReleaseRepo = "http://repo1.maven.apache.org/maven2"
$mvnEntReleaseRepo = "https://repository.hazelcast.com/release/"

if ($serverVersion.Contains("SNAPSHOT")) {
    $mvnOssRepo = $mvnOssSnapshotRepo
    $mvnEntRepo = $mvnEntSnapshotRepo
} else {
    $mvnOssRepo = $mvnOssReleaseRepo
    $mvnEntRepo = $mvnEntReleaseRepo
}

# determine target .NET framework
$fwkTarget = "net462"
$coreTarget = "netcoreapp3.1"
if ($netcore) {
    $targetFramework = $coreTarget
}
else {
    $targetFramework = $fwkTarget
}

# determine tests categories
$testCategory = ""
if(!($enterprise)) {
	$testCategory +="cat != enterprise"
}

# prepare directories
$scriptRoot = "$PSScriptRoot" # expected to be build/
$slnRoot = [System.IO.Path]::GetFullPath("$scriptRoot\..")

$srcDir = "$slnRoot\src"
$tmpDir = "$slnRoot\temp"
$outDir = "$slnRoot\temp\output"
$docDir = "$slnRoot\docs"
$buildDir = "$slnRoot\build"

if (-not (test-path $tmpDir)) { mkdir $tmpDir >$null }
if (-not (test-path $outDir)) { mkdir $outDir >$null }

# remove all the bins and objs recursively
Get-ChildItem .\ -include bin,obj -Recurse | foreach ($_) { remove-item $_.fullname -Force -Recurse }

# clears the output directory
remove-item $outDir -force -recurse

# functions
function findLatestVersion($path) {
	if ([System.IO.Directory]::Exists($path)) {
		$v = ls $path | `
			 foreach-object { [Version]::Parse($_.Name) } | `
			 sort -descending | `
			 select -first 1
	}
	else {
		$l = [System.IO.Path]::GetDirectoryname($path).Length
		$l = $path.Length - $l
		$v = ls "$path.*" | `
			 foreach-object { [Version]::Parse($_.Name.SubString($l)) } | `
			 sort -descending | `
			 select -first 1
	}
	return $v
}

# say hello
Write-Host "Hazelcast.NET Client Build"
Write-Host "  Server version : $serverVersion"
Write-Host "  Target         : $targetFramework"
Write-Host "  Building to    : $outDir"
Write-Host ""
Write-Host "Hazelcast.NET Client Tests"
Write-Host "  Enterprise     : $enterprise"
Write-Host "  Exclude param  : $testCategory"
Write-Host "  Code coverage  : $coverage"
Write-Host ""

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
			Write-Host "Download NuGet..."
			Invoke-WebRequest $source -OutFile $nuget
			if (-not $?) { throw "Failed to download NuGet." }
			Write-Host "  -> $nuget"
		}
		else {
			Write-Host "Detected NuGet"
			Write-Host "  at '$nuget'"
		}
	}
	elseif (-not (test-path $nuget))
	{
		throw "Failed to locate NuGet.exe."
	}
}
$nuget = "$tmpDir\nuget.exe"
ensureNuGet

# ensure we have the required NuGet packages for building and testing
Write-Host ""
Write-Host "Restore NuGet packages for building..."
&$nuget restore "$buildDir\build.proj"

# ensure we have vswhere
function ensureVsWhere() {
	Write-Host ""
	Write-Host "Detected VsWhere"
	$v = findLatestVersion "$($env:USERPROFILE)\.nuget\packages\vswhere"
	Write-Host "  v$v"
	$dir = "$($env:USERPROFILE)\.nuget\packages\vswhere\$v"
	Write-Host "  -> $dir"

	return $dir
}
$vswhere = ensureVsWhere
$vswhere = "$vswhere\tools\vswhere.exe"

# find Visual Studio
function ensureVisualStudio() {
	$vsPath = ""
	$vsVer = ""
	$msBuild = $null

	$vsMajor = $hzVsMajor

	$vsPaths = new-object System.Collections.Generic.List[System.String]
	$vsVersions = new-object System.Collections.Generic.List[System.Version]
	$vsNames = new-object System.Collections.Generic.List[System.String]

	Write-Host ""

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
		Write-Host "Detected $($vsNames[$vsIx1]) ($_)"
		Write-Host "  at '$($vsPaths[$vsIx1])'"

		if ($_.Major -le $vsMajor -and $_ -gt $vsVersion) {
			$vsVersion = $_
			$vsIx2 = $vsIx1
		}
	}
	if ($vsIx2 -ge 0) {
		$vsPath = $vsPaths[$vsIx2]
		$vsName = $vsNames[$vsIx2]

		if ($vsVersion.Major -eq 16) {
			$msBuild = "$vsPath\MSBuild\Current\Bin\MsBuild.exe"
		}
		elseif ($vsVersion.Major -eq 15) {
			$msBuild = "$vsPath\MSBuild\$($vsVersion.Major).0\Bin\MsBuild.exe"
		}
	}

	dir 'C:\Program Files (x86)\Microsoft Visual Studio\*\BuildTools\MSBuild\*\Bin\MSBuild.exe' | ForEach-Object {
		$msBuildVersion = &$_ -nologo -version
		$msBuildVersion = [System.Version]::Parse($msBuildVersion)
		$msBuildExe = $_.FullName
		$toolsPath = $msBuildExe.SubString(0, $msBuildExe.IndexOf("\MSBuild\"))
		$toolsYear = $msBuildExe.SubString('C:\Program Files (x86)\Microsoft Visual Studio\'.Length, 4)
		Write-Host "Detected BuildTools $toolsYear ($msBuildVersion)"
		Write-Host "  at '$toolsPath'"

		if ($msBuildVersion.Major -le $vsMajor -and $msBuildVersion -gt $vsVersion)  {
			$msBuild = $msBuildExe
			$vsVersion = $msBuildVersion
			$vsName = "BuildTools "
		}
	}

	if (-not (test-path $msBuild)) {
		throw "Failed to locate MsBuild.exe."
	}
	else {
		Write-Host "Selecting $vsName ($vsVersion)"
		Write-Host "  at '$msBuild'"
	}
	return $msBuild
}
$msBuild = ensureVisualStudio

# ensure docfx
function ensureDocFx() {
	Write-Host ""
	Write-Host "Detected DocFX"
	$v = findLatestVersion "$($env:USERPROFILE)\.nuget\packages\docfx.console"
	Write-Host "  v$v"
	$dir = "$($env:USERPROFILE)\.nuget\packages\docfx.console\$v"
	Write-Host "  -> $dir"

	return $dir
}
$docfxDir = ensureDocFx
$docfxDir = "$docFxDir\tools"

# build the solution
Write-Host ""
Write-Host "Build solution..."
&$msBuild "$slnRoot\Hazelcast.Net.sln" `
	/p:Configuration=$configuration `
	/target:"Restore;Build"
	#/p:TargetFramework=$targetFramework `

# if it failed, we can stop here
if ($LASTEXITCODE) {
	Write-Host "Build failed, aborting."
	exit $LASTEXITCODE
}

# build docs
Write-Host ""
Write-Host "Build documentation..."
$envPath = $env:Path
$env:Path = "$docfxDir;$env:Path"
docfx metadata "$docDir\docfx.json"
docfx build "$docDir\docfx.json"
#docfx docs/docfx.json --serve
$env:Path = $envPath

# prepare for tests
Write-Host ""
Write-Host "Prepare for tests..."
if (-not (test-path "$tmpDir\lib")) { mkdir "$tmpDir\lib" >$null }
[string]$classpath=""

# ensure we have the remote controller jar
if(Test-Path "$tmpDir\lib\hazelcast-remote-controller-${hzRCVersion}.jar") {
	Write-Host "Detected hazelcast-remote-controller-${hzRCVersion}.jar."
} else {
	Write-Host "Downloading hazelcast-remote-controller-${hzRCVersion}.jar ..."
    &"mvn" -q "dependency:get" "-DrepoUrl=${mvnOssSnapshotRepo}" `
		"-Dartifact=com.hazelcast:hazelcast-remote-controller:${hzRCVersion}" `
		"-Ddest=$tmpDir\lib\hazelcast-remote-controller-${hzRCVersion}.jar"
}

$classpath += "$tmpDir\lib\hazelcast-remote-controller-${hzRCVersion}.jar;"

# ensure we have the hazelcast test jar
if(Test-Path "$tmpDir\lib\hazelcast-${hzVersion}-tests.jar") {
	Write-Host "Detected hazelcast-${hzVersion}-tests.jar."
} else {
	Write-Host "Downloading hazelcast-${hzVersion}-tests.jar ..."
    &"mvn" -q "dependency:get" "-DrepoUrl=${mvnOssRepo}" `
		"-Dartifact=com.hazelcast:hazelcast:${hzVersion}:jar:tests" `
		"-Ddest=$tmpDir\lib\hazelcast-${hzVersion}-tests.jar"
}

$classpath += "$tmpDir\lib\hazelcast-${hzVersion}-tests.jar;"

if($enterprise){

	# ensure we have the hazelcast jar
	if (Test-Path "$tmpDir\lib\hazelcast-enterprise-${hzVersion}.jar") {
		Write-Host "Detected hazelcast-enterprise-${hzVersion}.jar."
	} else {
		Write-Host "Downloading hazelcast-enterprise-${hzVersion}.jar ..."
		&"mvn" -q "dependency:get" "-DrepoUrl=${mvnEntRepo}" `
			"-Dartifact=com.hazelcast:hazelcast-enterprise:${hzVersion}" `
			"-Ddest=$tmpDir\lib\hazelcast-enterprise-${hzVersion}.jar"
	}

	$classpath += "$tmpDir\lib\hazelcast-enterprise-${hzVersion}.jar;"

	# ensure we have the hazelcast enterprise test jar
	if (Test-Path "$tmpDir\lib\hazelcast-enterprise-${hzVersion}-tests.jar") {
		Write-Host "Detected hazelcast-enterprise-${hzVersion}-tests.jar."
	} else {
		Write-Host "Downloading hazelcast-enterprise-${hzVersion}-tests.jar ..."
		&"mvn" -q "dependency:get" "-DrepoUrl=${mvnEntRepo}" `
			"-Dartifact=com.hazelcast:hazelcast-enterprise:${hzVersion}:jar:tests" `
			"-Ddest=$tmpDir\lib\hazelcast-enterprise-${hzVersion}-tests.jar"
	}

	$classpath += "$tmpDir\lib\hazelcast-enterprise-${hzVersion}-tests.jar;"

} else {

	# ensure we have the hazelcast jar
	if(Test-Path "$tmpDir\lib\hazelcast-${hzVersion}.jar") {
		Write-Host "Detected hazelcast-${hzVersion}.jar."
	} else {
		Write-Host "Downloading hazelcast-${hzVersion}.jar ..."
		&"mvn" -q "dependency:get" "-DrepoUrl=${mvnOssRepo}" `
			"-Dartifact=com.hazelcast:hazelcast:${hzVersion}" "-Ddest=$tmpDir\lib\hazelcast-${hzVersion}.jar"
	}

	$classpath += "$tmpDir\lib\hazelcast-${hzVersion}.jar;"
}

function StartRemoteController() {

	# start the remote controller
	Write-Host ""
	Write-Host "Starting Remote Controller..."
	$p = Start-Process -FilePath java -ArgumentList ( `
			"-Dhazelcast.enterprise.license.key=$env:HAZELCAST_ENTERPRISE_KEY", `
			"-cp", "$classpath", `
			"com.hazelcast.remotecontroller.Main" `
		) -RedirectStandardOutput "$tmpDir\rc\rc_stdout.log" -RedirectStandardError "$tmpDir\rc\rc_stderr.log" -PassThru
	Write-Host "Started remote controller with pid=$($p.Id)"
	return $p
}

function StopRemoteController($remoteController) {

	# stop the remote controller
	Write-Host ""
	Write-Host "Stopping remote controller..."
	Stop-Process -Force -Id $remoteController.Id
}

function RunDotNetCoreTests() {

	# run .NET Core unit tests
	# note:
	#   on some machines (??) MSBuild does not copy the NUnit adapter to the bin directory,
	#   but the 'dotnet test' command does copy it, provided that we don't use the --no-build
	#   option - it does not do a full build anyways - just sets tests up
    dotnet test $srcDir\Hazelcast.Tests\Hazelcast.Tests.csproj -c $configuration --no-restore -f $coreTarget -v n
}

function RunDotNetFrameworkTests() {

	# run .NET Framework unit tests
	$testDLL="$srcDir\Hazelcast.Tests\bin\${configuration}\${fwkTarget}\Hazelcast.Tests.dll"

	$v = findLatestVersion "$($env:USERPROFILE)\.nuget\packages\nunit.consolerunner"
	$nunit = "$($env:USERPROFILE)\.nuget\packages\nunit.consolerunner\$v\tools\nunit3-console.exe"
	$nunitArgs=@("`"${testDLL}`"", "--labels=All", "--result=`"tmpDir\tests\console-text.xml`"", "--framework=v4.6.2")

	if($testCategory.Length -gt 0) {
		$nunitArgs += @("--where=`"${testCategory}`"")
	}

	if($coverage) {

		$coveragePath = "$tmpDir\coverage"
		if (!(test-path $coveragePath)) {
			mkdir $coveragePath > $null
		}

		$v = findLatestVersion "$($env:USERPROFILE)\.nuget\packages\jetbrains.dotcover.commandlinetools"
		$dotCover = "$($env:USERPROFILE)\.nuget\packages\jetbrains.dotcover.commandlinetools\$v\tools\dotCover.exe"
		$dotCoverArgs=@( "cover", `
			"/Filters=-:Hazelcast.Test", `
			"/TargetWorkingDir=.", `
			"/Output=$coveragePath\index.html", `
			"/ReportType=HTML", `
			"/TargetExecutable=${nunit}", `
			"/TargetArguments=${nunitArgs}")

		Write-Host "$dotCoverCmd $dotCoverArgs"
		&$dotCover $dotCoverArgs

	} else {

		Write-Host "$nunit $nunitArgs"
		&$nunit $nunitArgs
	}
}

# run tests
try {
	$remoteController = StartRemoteController

	Write-Host ""
	Write-Host "Run tests..."
	Write-Host "TESTS ARE DISABLED FOR NOW"
	#if ($netcore) {
	#	RunDotNetCoreTests
	#}
	#else {
	#	RunDotNetFrameworkTests
	#}
}
finally {
	StopRemoteController $remoteController
}

Write-Host ""
Write-Host "Done."

# eof
