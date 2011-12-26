using System;
using System.Collections.Generic;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua.AST
{
    /// <summary>
    /// An if/then statement
    /// </summary>
    [Serializable()]
    public partial class IfStmt : Statement
    {
        /// <summary>
        /// Executes the chunk
        /// </summary>
        /// <param name="enviroment">The environment to run in</param>
        /// <param name="isBreak">whether to break execution</param>
        /// <returns></returns>
        public override LuaValue Execute(LuaTable enviroment, out bool isBreak)
        {
            LuaValue condition = this.Condition.Evaluate(enviroment);

            if (condition.GetBooleanValue() == true)
            {
                return this.ThenBlock.Execute(enviroment, out isBreak);
            }
            else
            {
                foreach (ElseifBlock elseifBlock in this.ElseifBlocks)
                {
                    condition = elseifBlock.Condition.Evaluate(enviroment);

                    if (condition.GetBooleanValue() == true)
                    {
                        return elseifBlock.ThenBlock.Execute(enviroment, out isBreak);
                    }
                }

                if (this.ElseBlock != null)
                {
                    return this.ElseBlock.Execute(enviroment, out isBreak);
                }
            }

            isBreak = false;
            return null;
        }
    }
}
