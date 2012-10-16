using System;

namespace SharpLua
{
    /// <summary>
    /// Lua script parsing error
    /// </summary>
    public class LuaSourceException : Exception
    {
        public int Line, Column;

        public LuaSourceException(int line, int col, string msg)
            : base(msg)
        {
            Line = line;
            Column = col;
            //Message = msg;
        }

        public string GenerateMessage(string filename)
        {
            return filename + ":" + Line + ":" + Column + ":" + Message;
        }
    }
}
