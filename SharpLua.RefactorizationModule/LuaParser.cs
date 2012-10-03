using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpLua;
using System.Windows.Forms;

namespace SharpLua.RefactorizationModule
{
    /// <summary>
    /// Provides access to parsing, lexing, and reconstructing code through LParser.
    /// Eventually, LParser will be brought to C#
    /// </summary>
    public class LuaParser
    {
        LuaInterface _interface;

        public LuaParser(LuaInterface li)
        {
            li.DoFile(Application.StartupPath + "\\lparser\\lparser\\util.lua");
            li.DoFile(Application.StartupPath + "\\lparser\\lparser\\parser.lua");
            li.DoFile(Application.StartupPath + "\\lparser\\lparser\\minify.lua");
            li.DoFile(Application.StartupPath + "\\lparser\\lparser\\beautify.lua");
            _interface = li;
        }

        public LuaTable Parse(string code)
        {
            string sep = "=";
            while (code.Contains("[" + sep + "["))
                sep += "=";
            object[] vals = _interface.DoString("return ParseLua([" + sep + "[" + code + "]" + sep + "])");
            bool success = (bool)vals[0];
            if (success)
                return (LuaTable)vals[1];
            else
                throw new Exception(vals[1].ToString());
        }

        public LuaTable Lex(string code)
        {
            string sep = "=";
            while (code.Contains("[" + sep + "["))
                sep += "=";
            object[] vals = _interface.DoString("return LexLua([" + sep + "[" + code + "]" + sep + "])");
            bool success = (bool)vals[0];
            if (success)
                return (LuaTable)vals[1];
            else
                throw new Exception(vals[1].ToString());
        }

        public string Minify(string code)
        {
            string sep = "=";
            while (code.Contains("[" + sep + "["))
                sep += "=";
            object[] vals = _interface.DoString("local x, y = ParseLua([" + sep + "[" + code + "]" + sep + "]) return Minify(y)");
            return (string)vals[0];
        }

        public string Beautify(string code)
        {
            string sep = "=";
            while (code.Contains("[" + sep + "["))
                sep += "=";
            object[] vals = _interface.DoString("local x, y = ParseLua([" + sep + "[" + code + "]" + sep + "]) return Beautify(y)");
            return (string)vals[0];
        }
    }
}
