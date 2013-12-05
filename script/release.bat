@echo off
echo starting build.....
MSBuild.exe ..\Hazelcast.Net\Hazelcast.Net.csproj /t:Version
MSBuild.exe ..\Hazelcast.Net\Hazelcast.Net.csproj /p:Configuration=Release /p:Platform=x86 /t:Release
MSBuild.exe ..\Hazelcast.Net\Hazelcast.Net.csproj /p:Configuration=Release /p:Platform=x64 /t:Release