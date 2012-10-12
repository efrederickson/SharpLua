using System;

namespace experimental_newparser
{
    /// <summary>
    /// Lua script parse error
    /// </summary>
    public class LuaSourceException : Exception
    {
        public new string Message;
        public int Line, Column;

        public LuaSourceException(int line, int col, string msg)
        {
            Line = line;
            Column = col;
            Message = msg;
        }
    }
}
