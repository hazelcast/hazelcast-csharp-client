@echo off
echo starting build.....
MSBuild.exe ..\Hazelcast.Net\Hazelcast.Net.csproj /t:Version
MSBuild.exe ..\Hazelcast.Net\Hazelcast.Net.csproj /p:Configuration=Release /p:Platform=AnyCPU /t:Release
REM if required define more Platform builds below
REM MSBuild.exe ..\Hazelcast.Net\Hazelcast.Net.csproj /p:Configuration=Release /p:Platform=x64 /t:Release