@echo off
dotnet pack
cd .\bin\Release
rd /s /q .\lib
mkdir lib
move .\netstandard2.1 .\lib
del *.nupkg