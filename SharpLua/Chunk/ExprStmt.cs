using System;
using System.Collections.Generic;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua
{
    public partial class ExprStmt : Statement
    {
        public override LuaValue Execute(LuaTable enviroment, out bool isBreak)
        {
            this.Expr.Evaluate(enviroment);
            isBreak = false;
            return null;
        }
    }
}
