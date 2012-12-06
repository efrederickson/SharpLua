using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace SharpLua
{
    /// <summary>
    /// A Lua Runtime for running files and strings
    /// </summary>
    public class LuaRuntime
    {
        static LuaInterface _interface = new LuaInterface();

        /// <summary>
        /// Runs a file
        /// </summary>
        /// <param name="luaFile"></param>
        /// <returns></returns>
        public static object[] RunFile(string luaFile)
        {
            return _interface.DoFile(luaFile);
        }

        /// <summary>
        /// Runs the code
        /// </summary>
        /// <param name="luaCode"></param>
        /// <param name="enviroment"></param>
        /// <returns></returns>
        public static object[] Run(string luaCode)
        {
            return _interface.DoString(luaCode);
        }

        /// <summary>
        /// Attempts to find a file from a shortened path (no extension)
        /// </summary>
        /// <param name="spath"></param>
        /// <returns></returns>
        public static string FindFullPath(string spath)
        {
            if (File.Exists(spath))
                return spath;
            if (File.Exists(spath + ".lua")) // lua
                return spath + ".lua";
            if (File.Exists(spath + ".slua")) // sLua (SharpLua)
                return spath + ".slua";
            if (File.Exists(spath + ".sluac")) // sLuac (SharpLua compiled)
                return spath + ".sluac";
            if (File.Exists(spath + ".luac")) // Luac (Lua compiled)
                return spath + ".luac";
            /*if (File.Exists(spath + ".dll"))
                return spath + ".dll";
            if (File.Exists(spath + ".exe"))
                return spath + ".exe";
            */

            return spath; // let the caller handle the invalid filename
        }

        /// <summary>
        /// Prints the SharpLua Banner
        /// </summary>
        public static void PrintBanner()
        {
            string asmVer = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Console.WriteLine("SharpLua " + asmVer + ", Copyright (C) 2011-2012 LoDC");
        }

        /// <summary>
        /// Returns the variable.
        /// You should capture returns in a 'dynamic' object
        /// </summary>
        /// <example>
        /// dynamic name = LuaRuntime.GetVariable("name");
        /// </example>
        /// <param name="varName"></param>
        /// <returns></returns>
        public static object GetVariable(string varName)
        {
            return _interface[varName];
        }

        public static void SetVariable(string varName, object val)
        {
            _interface[varName] = val;
        }

        public static void RegisterModule(Type t)
        {
            _interface.RegisterModule(t);
        }

        public static LuaInterface GetLua()
        {
            return _interface;
        }

        public static void SetLua(LuaInterface i)
        {
            _interface = i;
        }

        public static void SetLua(Lua.LuaState lua)
        {
            _interface = lua.Interface;
        }
        
        public static void Require(string lib)
        {
            Run("require('" + lib + "')");
            
            //Lua.lua_getglobal(_interface.LuaState, "require");
            //Lua.lua_pushstring(_interface.LuaState, lib);
            //return report(L, docall(L, 1, 1));
        }
    }
}
