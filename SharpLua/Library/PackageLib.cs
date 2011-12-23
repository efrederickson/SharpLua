/*
 * Created by SharpDevelop.
 * User: elijah
 * Date: 12/22/2011
 * Time: 4:08 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using SharpLua.LuaTypes;

namespace SharpLua.Library
{
    /// <summary>
    /// Packages library
    /// </summary>
    public class PackageLib
    {
        public static void RegisterModule(LuaTable env)
        {
            LuaTable module = new LuaTable();
            RegisterFunctions(module);
            env.SetNameValue("package", module);
        }
        
        public static void RegisterFunctions(LuaTable module)
        {
            // cpath -> cspath?
            module.SetNameValue("cpath", new LuaString(".\\?;.\\?.dll;.\\?.exe"));
            module.SetNameValue("path", new LuaString(".\\?;.\\?.lua;.\\?.slua;.\\?.wlua"));
            module.SetNameValue("loaded", new LuaTable());
            module.SetNameValue("preload", new LuaTable());
            module.Register("seeall", SeeAll);
            
            LuaTable loaders = new LuaTable();
            loaders.Register("a", new LuaFunc(delegate(LuaValue[] args)
        }
        
        public static LuaValue SeeAll(LuaValue[] args)
        {
            LuaTable t = args[0] as LuaTable;
            t.MetaTable.SetNameValue("__index", Lua.GlobalEnvironment);
            return t;
        }
    }
}
