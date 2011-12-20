using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua
{
    public abstract partial class Expr
    {
        public abstract LuaValue Evaluate(LuaTable enviroment);

        public abstract Term Simplify();
    }
}
