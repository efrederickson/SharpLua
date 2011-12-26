using System;
using System.Collections.Generic;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua.AST
{
    /// <summary>
    /// A local function
    /// </summary>
    public partial class LocalFunc : Statement
    {
        /// <summary>
        /// Executes the chunk
        /// </summary>
        /// <param name="enviroment">The environment to run in</param>
        /// <param name="isBreak">whether to break execution</param>
        /// <returns></returns>
        public override LuaValue Execute(LuaTable enviroment, out bool isBreak)
        {
            enviroment.SetNameValue(this.Name, this.Body.Evaluate(enviroment));
            isBreak = false;
            return null;
        }
    }
}
