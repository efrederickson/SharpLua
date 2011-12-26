using System;
using System.Collections.Generic;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua.AST
{
    [Serializable()]
    public partial class Term : Expr
    {
        public override LuaValue Evaluate(LuaTable enviroment)
        {
            throw new NotImplementedException();
        }

        public override Term Simplify()
        {
            return this;
        }
    }
}
