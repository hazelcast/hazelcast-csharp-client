@echo off
pushd %~dp0
set HAZELCAST_VERSION=3.6-SNAPSHOT
set HAZELCAST_HOME=%~dp0\server
echo Starting build...
msbuild Build.proj /p:Configuration=Release /p:Platform="Any CPU" /target:Build

echo Downlading latest HZ snapshot from Maven Central...
call mvn -U org.apache.maven.plugins:maven-dependency-plugin:2.8:copy -Dartifact=com.hazelcast:hazelcast:%HAZELCAST_VERSION% -DoutputDirectory=%HAZELCAST_HOME% -Dmdep.stripVersion=true
echo Running Unit Tests...
packages\NUnit.Runners.2.6.4\tools\nunit-console /xml:"console-text.xml" "Hazelcast.Test/Hazelcast.Test.nunit" /noshadow /config:Release
popd