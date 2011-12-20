using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SharpLua.Library;

namespace SharpLua
{
    public class LuaInterpreter
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

        static Parser parser = new Parser();

        public static Chunk Parse(string luaCode)
        {
            bool success;
            Chunk chunk = parser.ParseChunk(new TextInput(luaCode), out success);
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
            
            global.SetNameValue("_G", global);
            global.SetNameValue("LUA_PATH", new LuaString( ".\\;" + 
                               "require\\"
                              ));// format: <dir>;<dir2>
                                            // it automatically takes care of extensions
            
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
            if (File.Exists(spath + ".mlua")) // meta Lua
                return spath + ".mlua";
            // TODO: .so, .c, .cs, .dll, .exe
            
            return spath; // let the caller handle the invalid filename
        }
    }
}
