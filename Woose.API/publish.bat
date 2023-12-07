@echo off
dotnet pack
cd .\bin\Release
rd /s /q .\lib
mkdir lib
move .\net7.0 .\lib
del *.nupkg