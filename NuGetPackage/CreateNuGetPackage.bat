@echo off
cd Package

:: create folder
mkdir lib
cd lib
mkdir net40
cd net40

:: copy binaries
copy /Y ..\..\..\..\bin\debug\SharpLua.dll .
::copy /Y ..\..\..\..\bin\debug\*.exe .

:: remove example projects
::del csharpexampleproject.exe
::del vbnetexampleproject.exe
::del iextendframework.dll
::del cryptolib.dll
::del sharplua.scripts.dll

:: create NuGet package
cd ..\..
nuget pack

move *.nupkg ..