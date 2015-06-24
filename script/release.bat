@echo off
echo starting build.....
MSBuild.exe ..\Hazelcast.Net\Hazelcast.Net.csproj /t:Version
MSBuild.exe ..\Hazelcast.Net\Hazelcast.Net.csproj /p:Configuration=Release /p:Platform=AnyCPU /t:Release

MSBuild.exe ..\Hazelcast.Test\Hazelcast.Test.csproj /p:Configuration=Debug /p:Platform=AnyCPU /t:Build
REM START HAZELCAST SERVER
REM MSBuild.exe ..\Hazelcast.Test\Hazelcast.Test.csproj /p:Configuration=Release /p:Platform=AnyCPU /t:RunTests
REM STOP HAZELCAST SERVER

REM if required define more Platform builds below
REM MSBuild.exe ..\Hazelcast.Net\Hazelcast.Net.csproj /p:Configuration=Release /p:Platform=x64 /t:Release
