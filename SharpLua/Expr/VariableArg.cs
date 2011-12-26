using System;
using System.Collections.Generic;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua.AST
{
    public partial class VariableArg : Term
    {
        public override LuaValue Evaluate(LuaTable enviroment)
        {
            return enviroment.GetValue(this.Name);
        }
    }
}
