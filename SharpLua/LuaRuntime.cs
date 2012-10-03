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
        /// Returns the parsed code
        /// </summary>
        /// <param name="luaCode"></param>
        /// <returns></returns>
        public static Lua.Proto Parse(string luaCode)
        {
            Lua.LuaState L = new Lua.LuaState();
            Lua.Zio zio = new Lua.Zio();
            zio.L = L;
            zio.p = luaCode;
            return Lua.luaY_parser(L, zio, new Lua.Mbuffer(), "@chunk");
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
            /*if (File.Exists(spath + ".slua")) // sLua (SharpLua)
                return spath + ".slua";
            if (File.Exists(spath + ".dll"))
                return spath + ".dll";
            if (File.Exists(spath + ".exe"))
                return spath + ".exe";
            */
            // TODO: .cs, .luac; possibly C

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
    }
}
