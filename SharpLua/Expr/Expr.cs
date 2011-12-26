using System;
using System.Collections.Generic;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua.AST
{
    public abstract partial class Expr
    {
        public abstract LuaValue Evaluate(LuaTable enviroment);

        public abstract Term Simplify();
    }
}
