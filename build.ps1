param(
    [switch]$enterprise = $false,
    [switch]$netcore =  $false,
    [string]$serverVersion = "4.0",
    [switch]$coverage = $false
)

# configure
$hazelcastTestVersion=$serverVersion
$hazelcastVersion=$serverVersion
$hazelcastEnterpriseVersion=$serverVersion
$hazelcastEnterpriseTestVersion=$serverVersion
$hazelcastRCVersion="0.7-SNAPSHOT"
$snapshotRepo="https://oss.sonatype.org/content/repositories/snapshots"
$releaseRepo="http://repo1.maven.apache.org/maven2"
$enterpriseReleaseRepo="https://repository.hazelcast.com/release/"
$enterpriseSnapshotRepo="https://repository.hazelcast.com/snapshot/"

# these must match what the tests project installs!
$nunitVersion = "3.10.0"
$dotCoverVersion = "2019.3.4"

$options = @{
	Local = $false;
	Cache = 12 #days
}

# clear rogue environment variable
$env:FrameworkPathOverride=""

# determine code repositories
if ($serverVersion.Contains("SNAPSHOT")) {
    $repo=$snapshotRepo
    $enterpriseRepo=$enterpriseSnapshotRepo
} else {
    $repo=$releaseRepo
    $enterpriseRepo=$enterpriseReleaseRepo
}

# determine tests categories
if(!($enterprise)) {
	$testCategory +="cat != enterprise"
}

# say hello
Write-Host "PARAMETERS:"
Write-Host "Server version : $serverVersion"
Write-Host "Code coverage enabled : $coverage"
Write-Host "Enterprise server : $enterprise"
Write-Host "Exclude param: $testCategory"
Write-Host "Net core: $netcore"

# determine target .NET framework
if ($netcore) {
    $targetFramework="netcoreapp2.1"
}
else {
    $targetFramework="net45"
}
Write-Host "Target framework: $targetFramework"

# remove all the bins and objs recursively
Get-ChildItem .\ -include bin,obj -Recurse | foreach ($_) { remove-item $_.fullname -Force -Recurse }

# ensure we have temp folder for downloads
$scriptRoot = "$PSScriptRoot"
$scriptTemp = "$scriptRoot\build\temp"
if (-not (test-path $scriptTemp)) { mkdir $scriptTemp > $null }

# ensure we have NuGet
$nuget = "$scriptTemp\nuget.exe"
if (-not $options.Local)
{
	$source = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
	if ((test-path $nuget) -and ((ls $nuget).CreationTime -lt [DateTime]::Now.AddDays(-$options.Cache)))
	{
		Remove-Item $nuget -force -errorAction SilentlyContinue > $null
	}
	if (-not (test-path $nuget))
	{
		Write-Host "Download NuGet..."
		Invoke-WebRequest $source -OutFile $nuget
		if (-not $?) { throw "Failed to download NuGet." }
	}
	else {
		Write-Host "Detected NuGet."
	}
}
elseif (-not (test-path $nuget))
{
	throw "Failed to locate NuGet.exe."
}

# ensure we have vswhere
$vswhere = "$scriptTemp\vswhere.exe"
if (-not $options.Local)
{
    if ((test-path $vswhere) -and ((ls $vswhere).CreationTime -lt [DateTime]::Now.AddDays(-$options.Cache)))
    {
	    Remove-Item $vswhere -force -errorAction SilentlyContinue > $null
    }
    if (-not (test-path $vswhere))
    {
		Write-Host "Download VsWhere..."
		$params = "-OutputDirectory", $scriptTemp, "-Verbosity", "quiet"
		&$nuget install vswhere @params
		if (-not $?) { throw "Failed to download VsWhere." }
		$dir = ls "$scriptTemp\vswhere.*" | sort -property Name -descending | select -first 1
		write-host $dir
		$file = ls -path "$dir" -name vswhere.exe -recurse
		mv "$dir\$file" $vswhere
		#$this.RemoveDirectory($dir)
    }
	else {
		Write-Host "Detected VsWhere."
	}
}
elseif (-not (test-path $vswhere))
{
    throw "Failed to locate VsWhere.exe."
}

# find Visual Studio
$vsPath = ""
$vsVer = ""
$msBuild = $null
$toolsVersion = ""

$vsMajor = if ($options.VsMajor) { $options.VsMajor } else { "16" } # default to 16 (VS2019) for now
$vsMajor = [int]::Parse($vsMajor)

$vsPaths = new-object System.Collections.Generic.List[System.String]
$vsVersions = new-object System.Collections.Generic.List[System.Version]
$vsNames = new-object System.Collections.Generic.List[System.String]

# parse vswhere output
$params = @()
if ($options.VsPreview) { $params += "-prerelease" }
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
		#$toolsVersion = "Current"
    }
    elseif ($vsVersion.Major -eq 15) {
		$msBuild = "$vsPath\MSBuild\$($vsVersion.Major).0\Bin\MsBuild.exe"
		#$toolsVersion = "15.0"
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
	Write-Host "MsBuild: $msBuild"
	Write-Host "  from $vsName ($vsVersion)"
}

# build the solution
Write-Host "Build solution..."
&$msBuild Hazelcast.Test\Hazelcast.Test.csproj /p:Configuration=Release /p:TargetFramework=$targetFramework /target:"Restore;Build"

# if it failed, we can stop here
if ($LASTEXITCODE) {
	Write-Host "Build failed, aborting."
	exit $LASTEXITCODE
}

# prepare
Write-Host "Prepare for tests..."

# ensure we have the remote controller jar
if(Test-Path "hazelcast-remote-controller-${hazelcastRCVersion}.jar") {
	Write-Host "Detected hazelcast-remote-controller-${hazelcastRCVersion}.jar."
} else {
	Write-Host "Downloading hazelcast-remote-controller-${hazelcastRCVersion}.jar ..."
    & "mvn" -q "dependency:get" "-DrepoUrl=${snapshotRepo}" "-Dartifact=com.hazelcast:hazelcast-remote-controller:${hazelcastRCVersion}" "-Ddest=hazelcast-remote-controller-${hazelcastRCVersion}.jar"
}

# ensure we have the hazelcast test jar
if(Test-Path "hazelcast-${hazelcastTestVersion}-tests.jar") {
	Write-Host "Detected hazelcast-${hazelcastTestVersion}-tests.jar."
} else {
	Write-Host "Downloading hazelcast-${hazelcastTestVersion}-tests.jar ..."
    & "mvn" -q "dependency:get" "-DrepoUrl=${repo}" "-Dartifact=com.hazelcast:hazelcast:${hazelcastTestVersion}:jar:tests" "-Ddest=hazelcast-${hazelcastTestVersion}-tests.jar"
}

[string]$classpath="hazelcast-remote-controller-${hazelcastRCVersion}.jar;hazelcast-${hazelcastTestVersion}-tests.jar;"

if($enterprise){

	# ensure we have the hazelcast jar
	if (Test-Path "hazelcast-enterprise-${hazelcastEnterpriseVersion}.jar") {
		Write-Host "Detected hazelcast-enterprise-${hazelcastEnterpriseVersion}.jar."
	} else {
		Write-Host "Downloading hazelcast-enterprise-${hazelcastEnterpriseVersion}.jar ..."
		& "mvn" -q "dependency:get" "-DrepoUrl=${enterpriseRepo}" "-Dartifact=com.hazelcast:hazelcast-enterprise:${hazelcastEnterpriseVersion}" "-Ddest=hazelcast-enterprise-${hazelcastEnterpriseVersion}.jar"
	}

	# ensure we have the hazelcast enterprise test jar
	if (Test-Path "hazelcast-enterprise-${hazelcastEnterpriseTestVersion}-tests.jar") {
		Write-Host "Detected hazelcast-enterprise-${hazelcastEnterpriseTestVersion}-tests.jar."
	} else {
		Write-Host "Downloading hazelcast-enterprise-${hazelcastEnterpriseTestVersion}-tests.jar ..."
		& "mvn" -q "dependency:get" "-DrepoUrl=${enterpriseRepo}" "-Dartifact=com.hazelcast:hazelcast-enterprise:${hazelcastEnterpriseTestVersion}:jar:tests" "-Ddest=hazelcast-enterprise-${hazelcastEnterpriseTestVersion}-tests.jar"
	}

	$classpath += "hazelcast-enterprise-${hazelcastEnterpriseVersion}.jar;hazelcast-enterprise-${hazelcastEnterpriseTestVersion}-tests.jar"
} else {

	# ensure we have the hazelcast jar
	if(Test-Path "hazelcast-${hazelcastVersion}.jar") {
		Write-Host "Detected hazelcast-${hazelcastVersion}.jar."
	} else {
		Write-Host "Downloading hazelcast-${hazelcastVersion}.jar ..."
		& "mvn" -q "dependency:get" "-DrepoUrl=${repo}" "-Dartifact=com.hazelcast:hazelcast:${hazelcastVersion}" "-Ddest=hazelcast-${hazelcastVersion}.jar"
	}

	$classpath += "hazelcast-${hazelcastVersion}.jar"
}

function StartRemoteController() {

	# start the remote controller
	$p = Start-Process -FilePath java -ArgumentList ( "-Dhazelcast.enterprise.license.key=$env:HAZELCAST_ENTERPRISE_KEY","-cp", "$classpath", "com.hazelcast.remotecontroller.Main" ) -RedirectStandardOutput "rc_stdout.log" -RedirectStandardError "rc_stderr.log" -PassThru
	Write-Host "Started remote controller with pid=$($p.Id)"
	return $p
}

function StopRemoteController($remoteController) {

	# stop the remote controller
	Write-Host "Stopping remote controller..."
	Stop-Process -Force -Id $remoteController.Id
}

function RunDotNetCoreTests() {

	# run .NET Core unit tests
	# note:
	#   on some machines (??) MSBuild does not copy the NUnit adapter to the bin directory,
	#   but the 'dotnet test' command does copy it, provided that we don't use the --no-build
	#   option - it does not do a full build anyways - just sets tests up
    dotnet test Hazelcast.Test\Hazelcast.Test.csproj -c Release --no-restore -f netcoreapp2.1 -v n
}

function RunDotNetFrameworkTests() {

	# run .NET Framework unit tests
	$testDLL=".\Hazelcast.Test\bin\Release\${targetFramework}\Hazelcast.Test.dll"

	$nunit = "$($env:USERPROFILE)\.nuget\packages\nunit.consolerunner\$nunitVersion\tools\nunit3-console.exe"
	$nunitArgs=@("`"${testDLL}`"", "--labels=All", "--result=console-text.xml;format=nunit2")

	if($testCategory.Length -gt 0) {
		$nunitArgs += @("--where=`"${testCategory}`"")
	}

	$nunitArgs += "--framework=v4.5"

	if($coverage) {

		$coveragePath = ".\Coverage"
		if (!(test-path $coveragePath)) {
			mkdir $coveragePath > $null
		}

		$dotCover = "$($env:USERPROFILE)\.nuget\packages\jetbrains.dotcover.commandlinetools\$dotCoverVersion\tools\dotCover.exe"
		$dotCoverArgs=@("cover", "/Filters=-:Hazelcast.Test", "/TargetWorkingDir=.", "/Output=$coveragePath\index.html", "/ReportType=HTML", "/TargetExecutable=${nunit}", "/TargetArguments=${nunitArgs}")

		Write-Host "$dotCoverCmd $dotCoverArgs"
		&$dotCover $dotCoverArgs

	} else {

		Write-Host "$nunit $nunitArgs"
		&$nunit $nunitArgs
	}
}

# run tests
Write-Host "Run tests..."
try {
	$remoteController = StartRemoteController
	if ($netcore) {
		RunDotNetCoreTests
	}
	else {
		RunDotNetFrameworkTests
	}
}
finally {
	StopRemoteController $remoteController
}

Write-Host "Done."