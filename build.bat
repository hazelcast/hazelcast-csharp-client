@echo off
setlocal EnableDelayedExpansion
pushd %~dp0
IF "%1"=="" (set COVERAGE="--no-coverage") ELSE (set COVERAGE=%1)
set HAZELCAST_VERSION=3.7-SNAPSHOT
set HAZELCAST_HOME=%~dp0\server
set HAZELCAST_REDIRECT_OUTPUT=true
echo Starting build...
msbuild Build.proj /p:Configuration=Release /p:Platform="Any CPU" /target:Build
IF %ERRORLEVEL% NEQ 0 exit /b %ERRORLEVEL%
echo Downlading latest HZ snapshot from Maven Central...
call mvn dependency:get -DrepoUrl=https://oss.sonatype.org/content/repositories/snapshots -Dartifact=com.hazelcast:hazelcast:%HAZELCAST_VERSION% -Ddest=%HAZELCAST_HOME%/hazelcast.jar
IF %ERRORLEVEL% NEQ 0 exit /b %ERRORLEVEL%
IF %COVERAGE%==--coverage (
  echo Running Unit Tests with coverage...
  dotcover analyse /TargetExecutable="packages\NUnit.Runners.2.6.4\tools\nunit-console.exe" /TargetArguments="/xml:"console-text.xml" Hazelcast.Test/bin/Release/Hazelcast.Test.dll" /TargetWorkingDir=. /Output=Coverage.html /ReportType=HTML
) ELSE (
  echo Running Unit Tests...
  packages\NUnit.Runners.2.6.4\tools\nunit-console /xml:"console-text.xml" "Hazelcast.Test/bin/Release/Hazelcast.Test.dll" /noshadow 
)
popd