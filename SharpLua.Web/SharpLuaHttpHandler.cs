using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.SessionState;

namespace SharpLua.Web
{
    /// <summary>
    /// An HttpHandler that lets you use #Lua in Web apps
    /// </summary>
    public class SharpHttpHandler : IHttpHandler, IRequiresSessionState
    {
        /// <summary>
        /// This handler is reusable
        /// </summary>
        public bool IsReusable
        {
            get { return true; }
        }

        /// <summary>
        /// Processes an HTTP Web request for a #Lua doc
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(HttpContext context)
        {
            // Create a new environment and add a table 'web' that has access to the HttpObjects
            LuaTypes.LuaTable env = LuaRuntime.CreateGlobalEnviroment();
            
            LuaTypes.LuaTable Web = new SharpLua.LuaTypes.LuaTable();
            Web.SetNameValue("context", SharpLua.ObjectToLua.ToLuaValue(context));
            Web.SetNameValue("response", SharpLua.ObjectToLua.ToLuaValue(context.Response));
            Web.SetNameValue("request", SharpLua.ObjectToLua.ToLuaValue(context.Request));
            Web.SetNameValue("application", SharpLua.ObjectToLua.ToLuaValue(context.Application));
            Web.SetNameValue("session", SharpLua.ObjectToLua.ToLuaValue(context.Session));
            Web.SetNameValue("filepath", SharpLua.ObjectToLua.ToLuaValue(context.Request.PhysicalPath));
            
            env.SetNameValue("web", Web);

            try
            {
                LuaRuntime.RunFile(context.Request.PhysicalPath, env);
            }
            catch (Exception e)
            {
                context.Response.Write("<h1>Error Loading #Lua File " + context.Request.PhysicalPath + "</h1><br />");
                context.Response.Write("<br /> <font color='red'>");
                context.Response.Write(e.ToString().Replace("\n", "<br />"));
                context.Response.Write("</font>");
            }
        }  
    }
}
