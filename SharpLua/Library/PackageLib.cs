/*
 * Created by SharpDevelop.
 * User: elijah
 * Date: 12/22/2011
 * Time: 4:08 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
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
            module.SetNameValue("path", new LuaString(".\\?;.\\?.lua;.\\?.slua;.\\?.wlua.\\?.mlua"));
            module.SetNameValue("loaded", new LuaTable());
            module.SetNameValue("preload", new LuaTable());
            module.Register("seeall", SeeAll);
            
            // Load package loader functions
            LuaTable loaderfunctions = new LuaTable();
            loaderfunctions.Register("PreloadSearcher", new LuaFunc(delegate(LuaValue[] args) {
                                                                        LuaTable t = (Lua.GlobalEnvironment.GetValue("package") as LuaTable).GetValue("preload") as LuaTable;
                                                                        string mod = (args[0] as LuaString).Text;
                                                                        LuaValue v = t.GetValue(mod);
                                                                        if (v != null && v != LuaNil.Nil)
                                                                            return new LuaMultiValue(new LuaValue[] {LuaBoolean.True, v as LuaTable});
                                                                        else
                                                                            return new LuaMultiValue(new LuaValue[] {LuaBoolean.False});
                                                                    }));
            
            loaderfunctions.Register("PathSearcher", new LuaFunc(delegate(LuaValue[] args)
                                                                 {
                                                                     // get package.path variable
                                                                     string path = (Lua.GlobalEnvironment.GetValue("package") as LuaTable).GetValue("path").Value.ToString();
                                                                     // split into paths
                                                                     string[] paths = path.Split(';');
                                                                     // check file names
                                                                     foreach (string p in paths)
                                                                     {
                                                                         foreach (LuaValue arg in args)
                                                                         {
                                                                             string sfn = arg.Value.ToString();
                                                                             string fn = p.Replace("?", sfn);
                                                                             if (File.Exists(fn))
                                                                             {
                                                                                 LuaTable m = new LuaTable();
                                                                                 m.AddValue(LuaRuntime.RunFile(fn));
                                                                                 return new LuaMultiValue(new LuaValue[] {LuaBoolean.True, m});
                                                                             }
                                                                         }
                                                                     }
                                                                     return new LuaMultiValue(new LuaValue[] {LuaBoolean.False});
                                                                 }));
            
            loaderfunctions.Register("CSPathSearcher", new LuaFunc(delegate(LuaValue[] args)
                                                                   {
                                                                       // get package.path variable
                                                                       string path = (Lua.GlobalEnvironment.GetValue("package") as LuaTable).GetValue("cpath").Value.ToString();
                                                                       // split into paths
                                                                       string[] paths = path.Split(';');
                                                                       // check file names
                                                                       foreach (string p in paths)
                                                                       {
                                                                           foreach (LuaValue arg in args)
                                                                           {
                                                                               string sfn = arg.Value.ToString();
                                                                               string fn = p.Replace("?", sfn);
                                                                               if (File.Exists(fn))
                                                                               {
                                                                                   Console.WriteLine("Loading file '" + fn + "'...");
                                                                                   string[] modules = ExternalLibraryLoader.Load(fn);
                                                                                   LuaTable t = Lua.GlobalEnvironment.GetValue(modules[0]) as LuaTable;
                                                                                   
                                                                                   return new LuaMultiValue(new LuaValue[] {LuaBoolean.True, t});
                                                                               }
                                                                           }
                                                                       }
                                                                       return new LuaMultiValue(new LuaValue[] {LuaBoolean.False});
                                                                   }));
            module.SetNameValue("loaders", loaderfunctions);
            module.Register("loadlib", new LuaFunc((LuaValue[] args) =>
                                                   {
                                                       return args[0];
                                                   }));
        }
        
        public static LuaValue SeeAll(LuaValue[] args)
        {
            LuaTable t = args[0] as LuaTable;
            t.MetaTable.SetNameValue("__index", Lua.GlobalEnvironment);
            return t;
        }
    }
}
