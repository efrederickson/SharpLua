using System;
using System.Collections.Generic;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua.AST
{
    /// <summary>
    /// A statement that breaks the current loop
    /// </summary>
    public partial class BreakStmt : Statement
    {
        /// <summary>
        /// Does nothing
        /// </summary>
        /// <param name="enviroment"></param>
        /// <param name="isBreak"></param>
        /// <returns></returns>
        public override LuaValue Execute(LuaTable enviroment, out bool isBreak)
        {
            throw new NotImplementedException();
        }
    }
}
