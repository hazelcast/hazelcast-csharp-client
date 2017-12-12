nuget restore

REM nuget.exe pack Hazelcast.Net/Hazelcast.Net.csproj -properties Configuration=Release

msbuild Hazelcast.Net/Hazelcast.Net.csproj /p:Configuration=Release /target:Restore;Build;Pack
