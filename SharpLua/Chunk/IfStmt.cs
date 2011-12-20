using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua
{
    public partial class IfStmt : Statement
    {
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
