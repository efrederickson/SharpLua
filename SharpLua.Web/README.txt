Create a new ASP.NET web application project.
Change the web.config to include the following within <system.web>:

<httpHandlers>
<add verb="*" path="*.slua" type="SharpLua.Web.SharpLuaHttpHandler,SharpLua.Web"/>
<add verb="*" path="*.lua" type="SharpLua.Web.SharpLuaHttpHandler,SharpLua.Web"/>
</httpHandlers>

Copy SharpLua.dll and SharpLua.Web.dll into the bin directory of the web application
Then write your LSharp Page as a text file with a *.slua or *.lua extension

Web context stuff is in the table called 'web'
