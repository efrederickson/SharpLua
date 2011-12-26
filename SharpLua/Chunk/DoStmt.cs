using System;
using System.Collections.Generic;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua.AST
{
    /// <summary>
    /// A do statement
    /// </summary>
    public partial class DoStmt : Statement
    {
        /// <summary>
        /// Executes the chunk
        /// </summary>
        /// <param name="enviroment">Runs in the given environment</param>
        /// <param name="isBreak">whether to break execution</param>
        /// <returns></returns>
        public override LuaValue Execute(LuaTable enviroment, out bool isBreak)
        {
            return this.Body.Execute(enviroment, out isBreak);
        }
    }
}
