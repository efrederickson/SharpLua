..\GenerateDocumentation\bin\Debug\GenerateDocumentation.exe Documentation.slua 
copy Documentation.xml ..\SharpLuaAddIn
cd ..\SharpLuaAddIn
msbuild /m 
:: msbuild /m /t:rebuild