using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using SharpLua.Library;
using SharpLua.LuaTypes;

namespace SharpLua
{
    public class LuaRuntime
    {
        public static LuaValue RunFile(string luaFile)
        {
            luaFile = FindFullPath(luaFile);
            return Run(File.ReadAllText(luaFile));
        }

        public static LuaValue RunFile(string luaFile, LuaTable enviroment)
        {
            luaFile = FindFullPath(luaFile);
            return Run(File.ReadAllText(luaFile), enviroment);
        }

        public static LuaValue Run(string luaCode)
        {
            return Run(luaCode, CreateGlobalEnviroment());
        }

        public static LuaValue Run(string luaCode, LuaTable enviroment)
        {
            Chunk chunk = Parse(luaCode);
            chunk.Enviroment = enviroment;
            return chunk.Execute();
        }

        static Parser.Parser parser = new Parser.Parser();

        public static Chunk Parse(string luaCode)
        {
            // remove previous errors
            parser.Errors.Clear();
            
            bool success;
            Chunk chunk = parser.ParseChunk(new Parser.TextInput(luaCode), out success);
            if (success)
            {
                return chunk;
            }
            else
            {
                throw new ArgumentException("Code has syntax errors:\r\n" + parser.GetErrorMessages());
            }
        }

        public static LuaTable CreateGlobalEnviroment()
        {
            LuaTable global = new LuaTable();

            BaseLib.RegisterFunctions(global);
            StringLib.RegisterModule(global);
            TableLib.RegisterModule(global);
            IOLib.RegisterModule(global);
            FileLib.RegisterModule(global);
            MathLib.RegisterModule(global);
            OSLib.RegisterModule(global);
            ScriptLib.RegisterModule(global);
            WinFormLib.RegisterModule(global);
            ConsoleLib.RegisterModule(global);
            GarbageCollectorLib.RegisterModule(global);
            CoroutineLib.RegisterModule(global);
            PackageLib.RegisterModule(global);
            
            global.SetNameValue("_WORKDIR", new LuaString(Application.StartupPath + "\\"));
            global.SetNameValue("_VERSION", new LuaString("Sharp Lua 1.0"));
            global.SetNameValue("_G", global);
            
            return global;
        }
        
        public static string FindFullPath(string spath)
        {
            if (File.Exists(spath))
                return spath;
            if (File.Exists(spath + ".lua")) // lua
                return spath + ".lua";
            if (File.Exists(spath + ".wlua"))
                return spath + ".wlua"; // wLua
            if (File.Exists(spath + ".slua")) // sLua (SharpLua)
                return spath + ".slua";
            if (File.Exists(spath + ".mlua")) // MetaLua
                return spath + ".mlua";
            if (File.Exists(spath + ".dll"))
                return spath + ".dll";
            if (File.Exists(spath + ".exe"))
                return spath + ".exe";
            
            // TODO: .so, .c, .cs, .luac
            
            return spath; // let the caller handle the invalid filename
        }
    }
}
