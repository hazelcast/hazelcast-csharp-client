param(
    [switch]$enterprise = $false,
    [switch]$netcore =  $false,
    [string]$serverVersion = $(throw "-serverVersion is required."),
    [switch]$coverage = $false
)

$hazelcastTestVersion=$serverVersion
$hazelcastVersion=$serverVersion
$hazelcastEnterpriseVersion=$serverVersion
$hazelcastEnterpriseTestVersion=$serverVersion
$hazelcastRCVersion="0.5-SNAPSHOT"
$snapshotRepo="https://oss.sonatype.org/content/repositories/snapshots"
$releaseRepo="http://repo1.maven.apache.org/maven2"
$enterpriseReleaseRepo="https://repository.hazelcast.com/release/"
$enterpriseSnapshotRepo="https://repository.hazelcast.com/snapshot/"

if ($serverVersion.Contains("SNAPSHOT")) {
    $repo=$snapshotRepo
    $enterpriseRepo=$enterpriseSnapshotRepo
} else {
    $repo=$releaseRepo
    $enterpriseRepo=$enterpriseReleaseRepo
}

if(!($enterprise)) {
	$testCategory +="cat != enterprise"
}

Write-Host "PARAMETERS:"
Write-Host "Server version : $serverVersion"
Write-Host "Code coverage enabled : $coverage"
Write-Host "Enterprise server :$enterprise"
Write-Host "Exclude param: $testCategory"
Write-Host "Net core: $netcore"
Write-Host "Starting build..."

nuget restore

if ($netcore) {
    $targetFramework="netcoreapp2.0"
}
else
{
    $targetFramework="net46"
}

msbuild Hazelcast.Test\Hazelcast.Test.csproj /p:Configuration=Release /p:TargetFramework=$targetFramework /target:"Restore;Build"

if(Test-Path "hazelcast-remote-controller-${hazelcastRCVersion}.jar") {
	Write-Host "remote controller already exist, not downloading from maven."
} else {
	Write-Host "Downloading hazelcast-remote-controller-${hazelcastRCVersion}.jar ..."
    & "mvn" -q "dependency:get" "-DrepoUrl=${snapshotRepo}" "-Dartifact=com.hazelcast:hazelcast-remote-controller:${hazelcastRCVersion}" "-Ddest=hazelcast-remote-controller-${hazelcastRCVersion}.jar"
}

if(Test-Path "hazelcast-${hazelcastTestVersion}-tests.jar") {
	Write-Host "hazelcast-${hazelcastTestVersion}-tests.jar already exist, not downloading from maven."
} else {
	Write-Host "Downloading hazelcast-${hazelcastTestVersion}-tests.jar ..."
    & "mvn" -q "dependency:get" "-DrepoUrl=${repo}" "-Dartifact=com.hazelcast:hazelcast:${hazelcastTestVersion}:jar:tests" "-Ddest=hazelcast-${hazelcastTestVersion}-tests.jar"
}

[string]$classpath="hazelcast-remote-controller-${hazelcastRCVersion}.jar;hazelcast-${hazelcastTestVersion}-tests.jar;"
if($enterprise){
	if(Test-Path "hazelcast-enterprise-${hazelcastEnterpriseVersion}.jar") {
		Write-Host "hazelcast-enterprise-${hazelcastEnterpriseVersion}.jar already exist, not downloading from maven."
	} else {
		Write-Host "Downloading hazelcast-enterprise-${hazelcastEnterpriseVersion}.jar ..."
		& "mvn" -q "dependency:get" "-DrepoUrl=${enterpriseRepo}" "-Dartifact=com.hazelcast:hazelcast-enterprise:${hazelcastEnterpriseVersion}" "-Ddest=hazelcast-enterprise-${hazelcastEnterpriseVersion}.jar"
	}
	if(Test-Path "hazelcast-enterprise-${hazelcastEnterpriseTestVersion}-tests.jar") {
		Write-Host "hazelcast-enterprise-${hazelcastEnterpriseTestVersion}-tests.jar already exist, not downloading from maven."
	} else {
		Write-Host "Downloading hazelcast-enterprise-${hazelcastEnterpriseTestVersion}-tests.jar ..."
		& "mvn" -q "dependency:get" "-DrepoUrl=${enterpriseRepo}" "-Dartifact=com.hazelcast:hazelcast-enterprise:${hazelcastEnterpriseTestVersion}:jar:tests" "-Ddest=hazelcast-enterprise-${hazelcastEnterpriseTestVersion}-tests.jar"
	}
	$classpath += "hazelcast-enterprise-${hazelcastEnterpriseVersion}.jar;hazelcast-enterprise-${hazelcastEnterpriseTestVersion}-tests.jar"
} else{
	if(Test-Path "hazelcast-${hazelcastVersion}.jar") {
		Write-Host "hazelcast-${hazelcastVersion}.jar already exist, not downloading from maven."
	} else {
		Write-Host "Downloading hazelcast-${hazelcastVersion}.jar ..."
		& "mvn" -q "dependency:get" "-DrepoUrl=${repo}" "-Dartifact=com.hazelcast:hazelcast:${hazelcastVersion}" "-Ddest=hazelcast-${hazelcastVersion}.jar"
	}
	$classpath += "hazelcast-${hazelcastVersion}.jar"
}

$remoteControllerApp = Start-Process -FilePath java -ArgumentList ( "-Dhazelcast.enterprise.license.key=$env:HAZELCAST_ENTERPRISE_KEY","-cp", "$classpath", "com.hazelcast.remotecontroller.Main" ) -RedirectStandardOutput "rc_stdout.log" -RedirectStandardError "rc_stderr.log" -PassThru

$testDLL=".\Hazelcast.Test\bin\Release\${targetFramework}\Hazelcast.Test.dll"
$nunitConsolePath=".\packages\NUnit.ConsoleRunner.3.9.0\tools\nunit3-console.exe"
$nunitArgs=@("`"${testDLL}`"", "--labels=All", "--result=console-text.xml;format=nunit2")

if($testCategory.Length -gt 0) {
  $nunitArgs += @("--where", "\`"${testCategory}\`"")
}

if (!$netcore) {
	$nunitArgs += "--framework=v4.0"
	if($coverage) {
		$dotCoverCmd=".\packages\JetBrains.dotCover.CommandLineTools.2018.2.3\tools\dotCover.exe"
		$dotCoverArgs=@("cover", "/Filters=-:Hazelcast.Test", "/TargetWorkingDir=.", "/Output=Coverage.html", "/ReportType=HTML", "/TargetExecutable=${nunitConsolePath}", "/TargetArguments=${nunitArgs}")
		Write-Host "$dotCoverCmd" $dotCoverArgs
		& "$dotCoverCmd" $dotCoverArgs
	} else {
		& $nunitConsolePath $nunitArgs
	}
}
else
{
    dotnet test Hazelcast.Test\Hazelcast.Test.csproj -c Release --no-build --no-restore -f netcoreapp2.0 -v n
}

Stop-Process -Force -Id $remoteControllerApp.Id
