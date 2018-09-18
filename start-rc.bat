@echo off
setlocal EnableDelayedExpansion
pushd %~dp0

set HZ_VERSION=3.10.5
set HAZELCAST_TEST_VERSION=%HZ_VERSION%
set HAZELCAST_VERSION=%HZ_VERSION%
set HAZELCAST_ENTERPRISE_VERSION=%HZ_VERSION%
set HAZELCAST_RC_VERSION=0.5-SNAPSHOT
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

echo Downloading ...
if not exist hazelcast-remote-controller-%HAZELCAST_RC_VERSION%.jar (
	echo Downloading hazelcast-remote-controller-%HAZELCAST_RC_VERSION%.jar ...
    call mvn -q dependency:get -DrepoUrl=%SNAPSHOT_REPO% -Dartifact=com.hazelcast:hazelcast-remote-controller:%HAZELCAST_RC_VERSION% -Ddest=hazelcast-remote-controller-%HAZELCAST_RC_VERSION%.jar
) else (
    echo remote controller already exist, not downloading from maven.
)

if not exist hazelcast-%HAZELCAST_TEST_VERSION%-tests.jar (
	echo Downloading hazelcast-%HAZELCAST_TEST_VERSION%-tests.jar ...
    call mvn -q dependency:get -DrepoUrl=%SNAPSHOT_REPO% -Dartifact=com.hazelcast:hazelcast:%HAZELCAST_TEST_VERSION%:jar:tests -Ddest=hazelcast-%HAZELCAST_TEST_VERSION%-tests.jar
) else (
    echo hazelcast-%HAZELCAST_TEST_VERSION%-tests.jar already exist, not downloading from maven.
)

set CLASSPATH=hazelcast-remote-controller-%HAZELCAST_RC_VERSION%.jar;hazelcast-%HAZELCAST_TEST_VERSION%-tests.jar
if -%HAZELCAST_ENTERPRISE_KEY%-==-- (
    if exist hazelcast-%HAZELCAST_VERSION%.jar (
        echo hazelcast.jar already exists, not downloading from maven.
    ) else (
        echo Downloading Hazelcast %HAZELCAST_VERSION% ...
        call mvn -q dependency:get -DrepoUrl=%REPO% -Dartifact=com.hazelcast:hazelcast:%HAZELCAST_VERSION% -Ddest=hazelcast-%HAZELCAST_VERSION%.jar
	)
	set CLASSPATH=%CLASSPATH%;hazelcast-%HAZELCAST_VERSION%.jar
) else (
    if exist hazelcast-enterprise-%HAZELCAST_ENTERPRISE_VERSION%.jar (
        echo hazelcast-enterprise.jar already exists, not downloading from maven.
    ) else (
    	echo Downloading Hazelcast Enterprise %HAZELCAST_ENTERPRISE_VERSION%...
	    call mvn -q dependency:get -DrepoUrl=%ENTERPRISE_REPO% -Dartifact=com.hazelcast:hazelcast-enterprise:%HAZELCAST_ENTERPRISE_VERSION% -Ddest=hazelcast-enterprise-%HAZELCAST_ENTERPRISE_VERSION%.jar
    ) 
	set CLASSPATH=%CLASSPATH%;hazelcast-enterprise-%HAZELCAST_ENTERPRISE_VERSION%.jar
)

echo "Starting hazelcast-remote-controller"

call java -Dhazelcast.enterprise.license.key=%HAZELCAST_ENTERPRISE_KEY% -cp %CLASSPATH% com.hazelcast.remotecontroller.Main

pause

popd
