using System;
using System.Collections.Generic;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua.AST
{
    [Serializable()]
    public partial class GroupExpr : BaseExpr
    {
        public override LuaValue Evaluate(LuaTable enviroment)
        {
            return this.Expr.Evaluate(enviroment);
        }

        public override Term Simplify()
        {
            return this.Expr.Simplify();
        }
    }
}
