@echo off
cd Package

:: create folder
mkdir lib
cd lib
mkdir net40
cd net40

:: copy binaries
copy /Y ..\..\..\..\bin\SharpLua.dll .

:: create NuGet package
cd ..\..
nuget pack SharpLua.nuspec

move *.nupkg ..
cd ..
del Package\lib\net40\SharpLua.dll

for %%f in (*.nupkg) do nuget push "%%f"
del *.nupkg