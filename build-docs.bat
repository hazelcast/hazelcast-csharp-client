@echo off
pushd %~dp0
msbuild docs\hazelcastdoc.shfbproj /p:Configuration=Release /p:Platform="Any CPU"
popd