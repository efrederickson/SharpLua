@echo off
cd Package

:: create folder
mkdir lib
cd lib
mkdir net40
cd net40

:: copy binaries
copy /Y ..\..\..\..\bin\debug\SharpLua.dll .

:: create NuGet package
cd ..\..
nuget pack SharpLua.nuspec

move *.nupkg ..
cd ..
del Package\lib\net40\SharpLua.dll

cd Package\lib\net40

:: copy binaries
copy /Y ..\..\..\..\bin\debug\SharpLua.Web.dll .

:: create NuGet package
cd ..\..
nuget pack SharpLua.Web.nuspec

move *.nupkg ..
cd ..
del Package\lib\net40\SharpLua.Web.dll