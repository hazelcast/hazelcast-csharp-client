@echo off
setlocal EnableDelayedExpansion
pushd %~dp0

REM TODO UPDATE PARAMETER PARSING
IF -%1-==-- (set COVERAGE="--no-coverage") ELSE (set COVERAGE=%1)
IF -%2-==-- (
	set SERVER_TYPE=--oss
	set EXLUDE_PARAM=/exclude:enterprise
) ELSE (
	set SERVER_TYPE=%2
	set EXLUDE_PARAM=
)

set HAZELCAST_VERSION=3.6.3
set HAZELCAST_RC_VERSION=0.1-SNAPSHOT
set SNAPSHOT_REPO=https://oss.sonatype.org/content/repositories/snapshots
set RELEASE_REPO=http://repo1.maven.apache.org/maven2
set ENTERPRISE_REPO=https://repository-hazelcast-l337.forge.cloudbees.com/release/

IF %SERVER_TYPE%==--enterprise (set CP_PARAM=-cp hazelcast-enterprise-%HAZELCAST_VERSION%.jar;hazelcast-%HAZELCAST_VERSION%.jar;hazelcast-tests-%HAZELCAST_VERSION%.jar) ELSE (set CP_PARAM=-cp hazelcast-%HAZELCAST_VERSION%.jar)
set CP_PARAM=%CP_PARAM%;hazelcast-remote-controller-%HAZELCAST_RC_VERSION%.jar

echo "PARAMETERS:" 
echo %COVERAGE%
echo %SERVER_TYPE% 
echo %CP_PARAM%
echo Exclude param: %EXCLUDE_PARAM%

echo Starting build...
msbuild Build.proj /p:Configuration=Release /p:Platform="Any CPU" /target:Build

IF %ERRORLEVEL% NEQ 0 exit /b %ERRORLEVEL%

echo Downlading Hazelcast %HAZELCAST_VERSION% ...
call mvn dependency:get -DrepoUrl=%SNAPSHOT_REPO% -Dartifact=com.hazelcast:hazelcast-remote-controller:%HAZELCAST_RC_VERSION% -Ddest=hazelcast-remote-controller-%HAZELCAST_RC_VERSION%.jar
call mvn dependency:get -DrepoUrl=%RELEASE_REPO% -Dartifact=com.hazelcast:hazelcast:%HAZELCAST_VERSION% -Ddest=hazelcast-%HAZELCAST_VERSION%.jar
call mvn dependency:get -DrepoUrl=%RELEASE_REPO% -Dartifact=com.hazelcast:hazelcast:%HAZELCAST_VERSION%:jar:tests -Ddest=hazelcast-tests-%HAZELCAST_VERSION%.jar

if %SERVER_TYPE%==--enterprise (
	echo Downlading Hazelcast Enterprise %HAZELCAST_VERSION%...
	call mvn dependency:get -DrepoUrl=%ENTERPRISE_REPO% -Dartifact=com.hazelcast:hazelcast-enterprise:%HAZELCAST_VERSION% -Ddest=hazelcast-enterprise-%HAZELCAST_VERSION%.jar
)

taskkill /T /F /FI "WINDOWTITLE eq hazelcast-remote-controller"
if exist errorlevel del errorlevel

echo "Starting hazelcast-remote-controller"
start /min "hazelcast-remote-controller" cmd /c "java -Dhazelcast.enterprise.license.key=%HAZELCAST_ENTERPRISE_KEY% %CP_PARAM% com.hazelcast.remotecontroller.Main> rc_stdout.txt 2>rc_stderr.txt || call echo %^errorlevel% > errorlevel"

REM Wait for Hazelcast RC to start
ping -n 4 127.0.0.1 > nul
if exist errorlevel (
    set /p exitcode=<errorlevel
    echo ERROR: Unable to start hazelcast-remote-controller
    exit /b %exitcode%
)

IF %ERRORLEVEL% NEQ 0 exit /b %ERRORLEVEL%
IF %COVERAGE%==--coverage (
  echo Running Unit Tests with coverage...
  dotcover analyse /TargetExecutable="packages\NUnit.Runners.2.6.4\tools\nunit-console.exe" /TargetArguments="/labels /xml:"console-text.xml" Hazelcast.Test/bin/Release/Hazelcast.Test.dll" /TargetWorkingDir=. /Output=Coverage.html /ReportType=HTML %EXCLUDE_PARAM%
) ELSE (
  echo Running Unit Tests...
  packages\NUnit.Runners.2.6.4\tools\nunit-console /labels /xml:"console-text.xml" "Hazelcast.Test/bin/Release/Hazelcast.Test.dll" /noshadow %EXCLUDE_PARAM% 
)
taskkill /T /F /FI "WINDOWTITLE eq hazelcast-remote-controller"
popd