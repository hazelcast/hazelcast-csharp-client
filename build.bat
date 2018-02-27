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

IF -%3-==-- (
	set FRAMEWORK=net40
) ELSE (
	IF -%3-==-net40- (
		set FRAMEWORK=net40
	) ELSE (
		IF -%3-==-netcore- (
			set FRAMEWORK=netstandard2.0
		) ELSE (
			echo "Invalide .Net Framework type, use net40 or netcore"
			exit /b
		)
	)
)

set HZ_VERSION=3.9.3
set HAZELCAST_TEST_VERSION=%HZ_VERSION%
set HAZELCAST_VERSION=%HZ_VERSION%
set HAZELCAST_ENTERPRISE_VERSION=%HZ_VERSION%
set HAZELCAST_RC_VERSION=0.3-SNAPSHOT
set SNAPSHOT_REPO=https://oss.sonatype.org/content/repositories/snapshots
set RELEASE_REPO=http://repo1.maven.apache.org/maven2
set ENTERPRISE_RELEASE_REPO=https://repository-hazelcast-l337.forge.cloudbees.com/release/
set ENTERPRISE_SNAPSHOT_REPO=https://repository-hazelcast-l337.forge.cloudbees.com/snapshot/

if not "x%HZ_VERSION:SNAPSHOT=%"=="x%HZ_VERSION%" (
    set REPO=%SNAPSHOT_REPO%
	set ENTERPRISE_REPO=%ENTERPRISE_SNAPSHOT_REPO%
) else (
	set REPO=%RELEASE_REPO%
	set ENTERPRISE_REPO=%ENTERPRISE_RELEASE_REPO%
)

echo "PARAMETERS:" 
echo %COVERAGE%
echo %SERVER_TYPE% 
echo %CP_PARAM%
echo Exclude param: %EXCLUDE_PARAM%
echo Framework : %FRAMEWORK%
echo Starting build...

REM nuget locals all -clear
nuget restore

if -%FRAMEWORK%-==-net40- (
	msbuild Hazelcast.Net.sln /p:Configuration=Release /p:Platform="Any CPU" /p:TargetFramework=net40 /target:Restore;Build
) ELSE (
	msbuild Hazelcast.Test\Hazelcast.Test.csproj /p:Configuration=Release /p:TargetFramework=netcoreapp2.0 /target:Restore;Build
)

IF %ERRORLEVEL% NEQ 0 exit /b %ERRORLEVEL%

echo Downlading ...
call mvn -q dependency:get -DrepoUrl=%SNAPSHOT_REPO% -Dartifact=com.hazelcast:hazelcast-remote-controller:%HAZELCAST_RC_VERSION% -Ddest=hazelcast-remote-controller-%HAZELCAST_RC_VERSION%.jar
call mvn -q dependency:get -DrepoUrl=%SNAPSHOT_REPO% -Dartifact=com.hazelcast:hazelcast:%HAZELCAST_TEST_VERSION%:jar:tests -Ddest=hazelcast-%HAZELCAST_TEST_VERSION%-tests.jar

set CLASSPATH=hazelcast-remote-controller-%HAZELCAST_RC_VERSION%.jar;hazelcast-%HAZELCAST_TEST_VERSION%-tests.jar
if %SERVER_TYPE%==--enterprise (
	echo Downlading Hazelcast Enterprise %HAZELCAST_ENTERPRISE_VERSION%...
	call mvn -q dependency:get -DrepoUrl=%ENTERPRISE_REPO% -Dartifact=com.hazelcast:hazelcast-enterprise:%HAZELCAST_ENTERPRISE_VERSION% -Ddest=hazelcast-enterprise-%HAZELCAST_ENTERPRISE_VERSION%.jar
	set CLASSPATH=%CLASSPATH%;hazelcast-enterprise-%HAZELCAST_ENTERPRISE_VERSION%.jar
) else (
    echo Downlading Hazelcast %HAZELCAST_VERSION% ...
    call mvn -q dependency:get -DrepoUrl=%REPO% -Dartifact=com.hazelcast:hazelcast:%HAZELCAST_VERSION% -Ddest=hazelcast-%HAZELCAST_VERSION%.jar
	set CLASSPATH=%CLASSPATH%;hazelcast-%HAZELCAST_VERSION%.jar
)

taskkill /T /F /FI "WINDOWTITLE eq hazelcast-remote-controller"
if exist errorlevel del errorlevel

echo "Starting hazelcast-remote-controller"
start /min "hazelcast-remote-controller" cmd /c "java -Dhazelcast.enterprise.license.key=%HAZELCAST_ENTERPRISE_KEY% -cp %CLASSPATH% com.hazelcast.remotecontroller.Main> rc_stdout.txt 2>rc_stderr.txt || call echo %^errorlevel% > errorlevel"

REM Wait for Hazelcast RC to start
ping -n 4 127.0.0.1 > nul
if exist errorlevel (
    set /p exitcode=<errorlevel
    echo ERROR: Unable to start hazelcast-remote-controller
    exit /b %exitcode%
)

IF %ERRORLEVEL% NEQ 0 exit /b %ERRORLEVEL%

set TEST_PARAMETERS="Hazelcast.Test/bin/Release/net40/Hazelcast.Test.dll" --labels=All --result=console-text.xml;format=nunit2 --framework=v4.0
if -%FRAMEWORK%-==-net40- (
	
	IF %COVERAGE%==--coverage (
	  echo Running Unit Tests with coverage...
	  dotcover analyse /TargetExecutable="packages\NUnit.ConsoleRunner.3.7.0\tools\nunit3-console.exe" /TargetArguments="%TEST_PARAMETERS%" /Filter=-:Hazelcast.Test /TargetWorkingDir=. /Output=Coverage.html /ReportType=HTML %EXCLUDE_PARAM%
	) ELSE (
	  echo Running Unit Tests...
	  packages\NUnit.ConsoleRunner.3.7.0\tools\nunit3-console.exe %TEST_PARAMETERS%
	)

) ELSE (
	REM dotnet core test
	dotnet test Hazelcast.Test\Hazelcast.Test.csproj -c Release --no-build --no-restore -f netcoreapp2.0 -v n
)
taskkill /T /F /FI "WINDOWTITLE eq hazelcast-remote-controller"
popd
