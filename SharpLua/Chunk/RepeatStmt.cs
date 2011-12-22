using System;
using System.Collections.Generic;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua
{
    public partial class RepeatStmt : Statement
    {
        public override LuaValue Execute(LuaTable enviroment, out bool isBreak)
        {
            while (true)
            {
                var returnValue = this.Body.Execute(enviroment, out isBreak);
                if (returnValue != null || isBreak == true)
                {
                    isBreak = false;
                    return returnValue;
                }

                LuaValue condition = this.Condition.Evaluate(enviroment);

                if (condition.GetBooleanValue() == true)
                {
                    break;
                }
            }

            return null;
        }
    }
}
