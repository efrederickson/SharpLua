using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua
{
    public partial class WhileStmt : Statement
    {
        public override LuaValue Execute(LuaTable enviroment, out bool isBreak)
        {
            while (true)
            {
                LuaValue condition = this.Condition.Evaluate(enviroment);

                if (condition.GetBooleanValue() == false)
                {
                    break;
                }

                var returnValue = this.Body.Execute(enviroment, out isBreak);
                if (returnValue != null || isBreak == true)
                {
                    isBreak = false;
                    return returnValue;
                }
            }

            isBreak = false;
            return null;
        }
    }
}
