using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua
{
    public partial class VariableArg : Term
    {
        public override LuaValue Evaluate(LuaTable enviroment)
        {
            return enviroment.GetValue(this.Name);
        }
    }
}
