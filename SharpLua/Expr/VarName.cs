using System;
using System.Collections.Generic;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua
{
    public partial class VarName : BaseExpr
    {
        public override LuaValue Evaluate(LuaTable enviroment)
        {
            return enviroment.GetValue(this.Name);
        }

        public override Term Simplify()
        {
            return this;
        }
    }
}
